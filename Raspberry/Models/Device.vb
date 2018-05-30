Imports System.Threading
Imports gimus.iot.client
Imports Newtonsoft.Json
Imports Windows.Storage

Namespace client
    Public Class Device
        Inherits gimus.iot.server.IotComponent
        Implements iApiAsyncReceiver
        Dim splitchar As String = "«"
        Protected Property sharedKey As String = "PippoCiccioPeraPesca"
        Public client As ApiClient
        Public ReadOnly Property sensors As Sensors


        Protected WithEvents keepAliveTimer As DispatcherTimer
        Protected _keepAliveTimerPeriod As Integer = 1000
        Public Property isActive As Boolean = True
        Public Property UpdateServer As Boolean = False
        Public Event SensorValueChanged(Sender As Device, sensor As Sensor, value As String)


        Public Sub New()
            sensors = New Sensors()
            sensors.device = Me
            keepAliveTimer = New DispatcherTimer()
            keepAliveTimer.Interval = TimeSpan.FromMilliseconds(_keepAliveTimerPeriod)
            keepAliveTimer.Start()
        End Sub




        Public Sub dataReady(requestURI As String, data As Object) Implements iApiAsyncReceiver.dataReady
            onCommandsReceived(data)
        End Sub

        Public Sub dataError(requestURI As String, ex As Exception) Implements iApiAsyncReceiver.dataError
            Logger.push(String.Format("Connection Error: {0} ", ex.Message))

        End Sub

        Public Property keepAliveTimerPeriod() As Integer
            Get
                Return _keepAliveTimerPeriod
            End Get
            Set(value As Integer)
                _keepAliveTimerPeriod = value
                keepAliveTimer.Interval = TimeSpan.FromMilliseconds(_keepAliveTimerPeriod)
            End Set
        End Property

        Public Sub onCommandsReceived(commands As List(Of server.DeviceCommand))
            For Each cmd As server.DeviceCommand In commands
                '     cmd.content = Crittografia.AES256.DecryptText(Convert.FromBase64String(cmd.content), Me.sharedKey)
                Dim s As Sensor = Nothing
                If cmd.sensorId = "" Then
                    onCommandReceived(cmd)
                Else
                    s = sensors.Item(cmd.sensorId)
                    If s IsNot Nothing Then
                        s.onCommandReceived(cmd)
                    End If
                End If
            Next
        End Sub

        Public Overridable Sub onCommandReceived(command As server.DeviceCommand)
            Logger.push(String.Format("Device {0} Command received from server: {1} ", name, command.content))

            Select Case command.content
                Case "send_report"
                    onReportCommandReceived()
                Case Else

            End Select
        End Sub

        Protected Sub onReportCommandReceived()
            Dim lsv As New List(Of server.SensorValue)

            For Each s As Sensor In Me.sensors.Values
                lsv.Add(s.buildSensorValueMessage)
            Next

            SendReport("device_report", lsv)

        End Sub

        Public Overridable Sub OnSensorValueChanged(sensor As Sensor, ldValue As String, newValue As String)
            RaiseEvent SensorValueChanged(Me, sensor, newValue)
            sendSensorUpdate(sensor)
        End Sub

        Public Sub sendSensorUpdate(sensor As Sensor)
            Dim lsf As List(Of StorageFile) = Nothing
            Dim lmf As List(Of server.MemoryFile) = Nothing

            If sensor.content IsNot Nothing Then

                If TypeOf sensor.content Is server.MemoryFile Then
                    lmf = New List(Of server.MemoryFile)
                    lmf.Add(sensor.content)
                End If

                If TypeOf sensor.content Is StorageFile Then
                    lsf = New List(Of StorageFile)
                    lsf.Add(sensor.content)
                End If

                If TypeOf sensor.content Is List(Of StorageFile) Then
                    lsf = sensor.content
                End If

            End If
            SendReport("sensor_update", sensor.buildSensorValueMessage, lsf, lmf)
        End Sub


        Protected Sub SendPing()
            SendReport("ping", "")
        End Sub

        Protected Async Sub SendReport(type As String, data As Object, Optional files As List(Of StorageFile) = Nothing, Optional mfiles As List(Of server.MemoryFile) = Nothing)
            If client IsNot Nothing And UpdateServer Then
                Dim dr As New server.DeviceReport
                dr.device = Me.id
                dr.type = type
                dr.value = data
                dr.name = name

                '   Dim cmd As String = System.Environment.TickCount.ToString & splitchar & JsonConvert.SerializeObject(dr, Formatting.None)
                '    Dim ecmd As String = Convert.ToBase64String(Crittografia.AES256.EncryptText(cmd, Me.sharedKey)).Replace("+", "*")
                Dim ecmd As String = JsonConvert.SerializeObject(dr, Formatting.None)

                Await client.SendReport(Me.id, ecmd, files, mfiles, Me)

                Dim sensordetails As String = ""
                If type = "sensor_update" Then
                    Dim sv As server.SensorValue = data
                    sensordetails = " for sensor " & sv.name
                End If

                Logger.push(String.Format("Device {0} report type {1}{2} Sent to server:", name, type, sensordetails))

            End If

        End Sub

        Private Sub timer_Tick(sender As Object, e As Object) Handles keepAliveTimer.Tick
            onKeepAliveTick()
        End Sub

        Protected Overridable Sub onKeepAliveTick()
            SendPing()
        End Sub

    End Class

    Public Class Devices
        Inherits Dictionary(Of String, Device)

    End Class


End Namespace

