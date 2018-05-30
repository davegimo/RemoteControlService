Imports System.Net.Http
Imports Newtonsoft.Json
Imports Windows.Storage

Namespace client
    Public Class ApiClient
        Inherits ClientBase

        Public Sub New(Optional __baseAddress As String = "https://localhost:44330/")
            MyBase.New(__baseAddress)
        End Sub

        Protected Overrides Function httpResponseMessageDataConverter(functionName As String, response As HttpResponseMessage) As Object
            Dim s As String
            Select Case functionName
                Case "DeviceCommands"
                    s = response.Content.ReadAsStringAsync().Result
                    Return JsonConvert.DeserializeObject(Of List(Of server.DeviceCommand))(s)
                Case Else
                    s = response.Content.ReadAsStringAsync().Result
                    Return s
            End Select

        End Function

#Region "Report"

        'Public Function SendReport(deviceId As String, report As String, Optional receiver As iApiAsyncReceiver = Nothing) As List(Of gimus.iot.server.DeviceCommand)
        '    Dim requestURL As String = String.Format("api/report?deviceId={0}&report={1}", deviceId, report)
        '    Return ApiGetCallHandler("DeviceCommands", requestURL, receiver)
        'End Function

        Public Async Function SendReport(deviceId As String, report As String, files As List(Of StorageFile), mfiles As List(Of server.MemoryFile), Optional receiver As iApiAsyncReceiver = Nothing) As Task(Of List(Of gimus.iot.server.DeviceCommand))
            Dim requestURL As String = String.Format("api/report?deviceId={0}&report={1}", deviceId, report)
            Dim cont As New MultipartFormDataContent
            Dim i As Integer = 1

            If files IsNot Nothing Then
                For Each sf As StorageFile In files
                    Dim stream As IO.Stream = Await sf.OpenStreamForReadAsync()
                    If stream IsNot Nothing Then
                        stream.Seek(0, SeekOrigin.Begin)
                        cont.Add(New StreamContent(stream), "file_" & i.ToString, sf.Name)
                        i += 1
                    End If
                Next
            End If

            If mfiles IsNot Nothing Then
                For Each mf As server.MemoryFile In mfiles
                    cont.Add(mf.asStreamContent, "file_" & i.ToString, mf.filename)
                    i += 1
                Next
            End If
            Return ApiPostCallHandler("DeviceCommands", requestURL, cont, receiver)
        End Function

#End Region


    End Class


End Namespace

