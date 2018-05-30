Imports gimus.iot.server
Imports Newtonsoft.Json

Public Class IoTHub
    Public Shared H As IoTHub

    Public devices As New Devices
    Public commands As New DeviceCommands
    Public Shared debugChatId As Integer = "-297007348"
    Public Shared botChatId As Integer = "237561278"
    Public Shared dbotChatId As Integer = "31352897"

    Public WithEvents bot As New gimus.iot.server.TelegramBotClient("BOT", My.Settings.iot_bot_key, botChatId)
    Public WithEvents dbot As New gimus.iot.server.TelegramBotClient("DBOT", My.Settings.dave_bot_key, dbotChatId)

    Public Event SensorReportReceived(sensor As Sensor)
    Public Event SensorValueChanged(sensor As Sensor, oldValue As String, newValue As String)
    Public Event BotMessageReceived(sender As TelegramBotClient, messageText As String)
    Public config As XDocument

    Public Sub New()
        H = Me
        bot.StartReceiving()
        dbot.StartReceiving()
        Dim s As String = IO.File.ReadAllText(System.Web.Hosting.HostingEnvironment.MapPath("~/iothub.config"))
        config = XDocument.Parse(s)
    End Sub

    Public Sub dispose()
        bot.StopReceiving()
        dbot.StopReceiving()

    End Sub

    Public Sub ProcessDeviceReport(dr As DeviceReport, files As UploadedFiles)
        Dim sv As SensorValue
        Dim s As Sensor = Nothing

        Try
            Dim dev As Device = GetDevice(dr.device, dr.name)
            If dev IsNot Nothing Then
                dev.lastReport = DateTime.Now
                Select Case dr.type
                    Case "sensor_update"
                        sv = JsonConvert.DeserializeObject(Of SensorValue)(dr.value.ToString)
                        s = GetSensor(dev.sensors, sv.sensor, sv.name, sv.type)
                        s.ProcessSensorValue(sv, files)

                        Try
                            RaiseEvent SensorReportReceived(s)
                        Catch ex As Exception
                        End Try

                        If s.oldValue <> s.value Then
                            Try
                                RaiseEvent SensorValueChanged(s, s.oldValue, s.value)
                            Catch ex As Exception
                            End Try

                        End If

                    Case "device_report"
                    '   Call bot.sendMessageAsync(telegramChatId, Now().ToString("dd/MM/yy HH:mm:sss") & vbCrLf & rep.value.ToString)
                    Case "ping"
                        dev.lastReport = DateTime.Now
                    Case Else

                End Select

            End If
        Catch ex As Exception
            Call IoTHub.H.bot.sendMessageAsync(IoTHub.debugChatId, Now().ToString("dd/MM/yy HH:mm:sss") & vbCrLf & "ERRORz Processing DeviceReport: " & ex.Message)
        End Try
    End Sub


    Protected Function GetDevice(id As String) As Device
        Dim dev As Device = Nothing
        If devices.Keys.Contains(id) Then
            dev = devices(id)
        End If
        Return dev
    End Function

    Public Function getDeviceSharedKey(deviceId As String)
        Dim xe As XElement = Me.config.Descendants("device").Where(Function(x) x.Attribute("id") = deviceId).FirstOrDefault

        If xe IsNot Nothing Then
            Return xe.Attribute("key").Value
        Else
            Return ""
        End If
    End Function

    Protected Function GetDevice(id As String, name As String) As Device
        Dim dev As Device = Nothing
        If Not devices.Keys.Contains(id) Then
            dev = New Device
            dev.id = id
            dev.name = name
            devices.Add(dev.id, dev)
        End If

        dev = devices(id)

        Return dev
    End Function

    Protected Function GetSensor(sensors As Sensors, id As String) As Sensor
        Dim s As Sensor = Nothing
        If sensors.Keys.Contains(id) Then
            s = sensors(id)
        End If
        Return s
    End Function

    Protected Function GetSensor(sensors As Sensors, id As String, name As String, type As String) As Sensor
        Dim s As Sensor = Nothing
        If Not sensors.Keys.Contains(id) Then
            sensors.Add(Sensors.createSensor(id, name, type))
        End If
        s = sensors(id)

        Return s
    End Function

    Public Function GetSensor(deviceId As String, sensorId As String) As Sensor
        Dim dev As Device = GetDevice(deviceId)
        If dev IsNot Nothing Then
            Return GetSensor(dev.sensors, sensorId)
        Else
            Return Nothing
        End If
    End Function

    Private Sub bot_TextMessageReceived(sender As TelegramBotClient, messageText As String) Handles bot.TextMessageReceived, dbot.TextMessageReceived
        RaiseEvent BotMessageReceived(sender, messageText)
    End Sub

    Public Function getSystemStatus(Optional deviceId As String = "") As SystemStatus
        Dim ss As New SystemStatus
        For Each d As Device In Me.devices.Values.Where(Function(x) x.id = deviceId Or deviceId = "").ToList
            ss.devices.Add(d.getDeviceStatus())
        Next
        Return ss
    End Function

End Class
