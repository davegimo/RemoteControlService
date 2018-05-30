Imports System.IO
Imports System.Net.Http
Imports System.Web.Http
Imports System.Web.Http.Cors
Imports gimus.iot.server
Imports Newtonsoft.Json

Namespace Controllers
    <EnableCors("*", "*", "*")>
    Public Class IotController
        Inherits ApiController
        Shared ReadOnly ServerUploadFolder As String = Path.GetTempPath() & "iis_upload_temp_files\"
        Dim splitchar As String = "«"
        Dim sk As String

        <EnableCors("*", "*", "*")>
        <Route("api/status")>
        Public Function GetStatus(<FromUri> Optional deviceId As String = "") As SystemStatus
            Return IoTHub.H.getSystemStatus(deviceId)
        End Function

        <EnableCors("*", "*", "*")>
        <Route("api/get_report")>
        Public Async Function GetReport(<FromUri> deviceId As String, <FromUri> report As String) As Threading.Tasks.Task(Of List(Of DeviceCommand))
            Dim res = Await PostReport(deviceId, report)
            Return res
        End Function


        <EnableCors("*", "*", "*")>
        <Route("api/report")>
        Public Async Function PostReport(<FromUri> deviceId As String, <FromUri> report As String) As Threading.Tasks.Task(Of List(Of DeviceCommand))
            Dim dtext As String
            Dim dreport As String

            Try
                If report.Substring(0, 1) = "{" Then
                    ' il report inviato è in chiaro ... passiamolo così come è al metodo di gestione
                    dreport = report
                Else
                    ' nel caso il report del device sia criptato con l'algoritmo AES 256, padding pkcs7, modo CBC utilizzando una chiave condivisa
                    ' una volta decriptato il messaggio, lo dividiamo in due e prendiamo la seconda parte 
                    ' perché la prima è semplicemente una sequenza numerica necessaria per rendere continuamente variabile il testo criptato
                    ' e rendere ancora più difficile l'eventuale decodifica del testo.
                    ' La cifratura serve più che altro per garantire la sicurezza mediante l'autenticazione certa tra due endpoint, la privacy la fornisce il canale ssl se viene usato.

                    sk = IoTHub.H.getDeviceSharedKey(deviceId)
                    dtext = Crittografia.AES256.DecryptText(Convert.FromBase64String(report.Replace("*", "+")), sk)
                    dreport = dtext.Split(splitchar)(1)
                End If

                Dim files As UploadedFiles = Await GetUploadedFiles()
                Dim l As List(Of DeviceCommand) = DigestReport(deviceId, dreport, files)
                'Dim rl As New List(Of DeviceCommand)

                '' cifriamo anche il contenuto dei comandi che mandiamo al device
                'Dim ndc As DeviceCommand
                'For Each dc As DeviceCommand In l
                '    ndc = New DeviceCommand
                '    ndc.deviceId = dc.deviceId
                '    ndc.sensorId = dc.sensorId
                '    ndc.content = Convert.ToBase64String(Crittografia.AES256.EncryptText(System.Environment.TickCount.ToString & splitchar & dc.content, sk))
                '    rl.Add(ndc)
                'Next

                Return l
            Catch ex As Exception
                Return New List(Of DeviceCommand)
            End Try

        End Function

        Protected Function decryptAndExtract(deviceId As String, text As String) As String
            Dim dtext As String = Crittografia.AES256.DecryptText(Convert.FromBase64String(text.Replace("*", "+")), sk)
            Return dtext.Split(splitchar)(1)
        End Function

        Private Function DigestReport(deviceId As String, report As String, files As UploadedFiles) As List(Of DeviceCommand)
            Try
                If report <> "" Then
                    IoTHub.H.ProcessDeviceReport(JsonConvert.DeserializeObject(Of DeviceReport)(report), files)
                End If

            Catch ex As Exception
                Call IoTHub.H.bot.sendMessageAsync(IoTHub.debugChatId, Now().ToString("dd/MM/yy HH:mm:sss") & vbCrLf & "ERRORE: " & ex.Message)
            End Try

            Return IoTHub.H.commands.getDeviceCommands(deviceId)

        End Function

        Private Async Function GetUploadedFiles() As Threading.Tasks.Task(Of UploadedFiles)
            Dim files As New UploadedFiles

            Try
                If Request.Content IsNot Nothing Then
                    Dim streamProvider = New MultipartFormDataStreamProvider(ServerUploadFolder)
                    CleanTempFiles(ServerUploadFolder, 1)
                    Await Request.Content.ReadAsMultipartAsync(streamProvider)
                    Dim uf As UploadedFile = Nothing

                    For Each mpfd As MultipartFileData In streamProvider.FileData
                        uf = New UploadedFile
                        uf.filename = mpfd.Headers.ContentDisposition.FileName.Replace("""", "")
                        uf.content = IO.File.ReadAllBytes(mpfd.LocalFileName)
                        files.Add(uf)
                    Next
                End If

            Catch ex As Exception

            End Try
            Return files
        End Function

        Private Sub CleanTempFiles(ByVal dir As String, ByVal ageInMinutes As Integer)
            Try
                If Not Directory.Exists(dir) Then
                    Directory.CreateDirectory(dir)
                Else
                    Dim files As String() = Directory.GetFiles(dir)
                    Dim fi As FileInfo
                    For Each file As String In files
                        fi = New FileInfo(file)
                        Dim time = fi.CreationTime
                        If fi.CreationTime.AddMinutes(ageInMinutes) < DateTime.Now Then
                            fi.Delete()
                        End If
                    Next
                End If

            Catch ex As Exception

            End Try
        End Sub

    End Class
End Namespace