Imports gimus.iot.server

Public Class TestApp
    Inherits IotApplication
    Dim cam As String = "CAM1"
    Dim dev As String = "RASPB3"
    Dim cs As CamSensor
    Dim ss As SmartSwitch

    Public Overrides Sub OnSensorReportReceived(sensor As Sensor)
        MyBase.OnSensorReportReceived(sensor)
        If sensor.name = cam Then
            cs = hub.GetSensor(dev, cam)
        End If
        If sensor.name = "PLUG1" Then
            ss = hub.GetSensor(dev, "PLUG1")
        End If

        Try
            If sensor.id = "SW_MOV" And sensor.value = "ON" Then
                SendLastImage(cam, "movement detected!")
            End If

            If sensor.id = "SW_LUX" And sensor.value = "ON" Then
                IoTHub.H.commands.Add(ss.command_SetOn)
            End If

            If sensor.id = "SW_LUX" And sensor.value = "OFF" Then
                IoTHub.H.commands.Add(ss.command_SetOff)
            End If

        Catch ex As Exception
            IoTHub.H.bot.sendMessageAsync(IoTHub.debugChatId, ex.Message)
        End Try

    End Sub

    Public Overrides Sub OnSensorValueChanged(sensor As Sensor, oldValue As String, newValue As String)
        If sensor.id = "RSPB_PIN_37" And sensor.value = "ON" Then
            SendLastImage(cam, "Porta aperta! Chi è?")
            IoTHub.H.commands.Add(ss.command_SetOn)
        End If
        If sensor.id = "RSPB_PIN_37" And sensor.value = "OFF" Then
            IoTHub.H.commands.Add(ss.command_SetOff)
        End If
        If sensor.id = cam And sensor.value.StartsWith("videoclip") Then
            SendLastVideoClip(cam)
        End If

    End Sub

    Public Overrides Sub OnBotMessageReceived(sender As TelegramBotClient, msg As String)
        Dim lca As New Text.StringBuilder(1000)
        lca.AppendLine("on:  accende la luce")
        lca.AppendLine("off: spegne la luce")
        lca.AppendLine("send photo: invia uno snapshot della webcam")
        lca.AppendLine("start: attiva la registrazione di un video di max 30 secondi")
        lca.AppendLine("stop: interrompe la registrazione del video e lo invia")
        lca.AppendLine("status: fornisce lo stato di tutti i sensori")

        Dim cmdOk As Boolean = True
        Select Case msg.ToLower
            Case "send photo"
                cs = hub.GetSensor(dev, cam)
                SendLastImage(cam, "Eccoti servito!")

            Case "start"
                cs = hub.GetSensor(dev, cam)
                IoTHub.H.commands.Add(cs.command_StartClip(30))
            Case "stop"
                cs = hub.GetSensor(dev, cam)
                IoTHub.H.commands.Add(cs.command_StopClip)
            Case "on"
                ss = hub.GetSensor(dev, "PLUG1")
                IoTHub.H.commands.Add(ss.command_SetOn)
            Case "off"
                ss = hub.GetSensor(dev, "PLUG1")
                IoTHub.H.commands.Add(ss.command_SetOff)
            Case "status"
                sendStatus()
            Case Else
                cmdOk = False
        End Select
        If cmdOk Then
            sender.sendMessageAsync(sender.chatId, "ok: " & msg)
        Else
            sender.sendMessageAsync(sender.chatId, "cmd: " & msg & " non riconosciuto!")
            sender.sendMessageAsync(sender.chatId, "lista comandi abilitati: " & vbCrLf & lca.ToString)
        End If


        If sender.name = "DBOT" Then
            IoTHub.H.dbot.sendMessageAsync(31352897, "lo so... hai scritto " & msg)
            IoTHub.H.dbot.sendMessageAsync(-311755409, "nel bot ... hai scritto " & msg)
        End If

    End Sub

    Protected Sub sendStatus()
        Dim t As New Text.StringBuilder(1000)
        Dim ss As SystemStatus = IoTHub.H.getSystemStatus()
        t.AppendLine("Current system status:")
        For Each d As DeviceStatus In ss.devices
            t.AppendFormat("device: {0}{1}", d.deviceId, vbCrLf)
            t.AppendFormat("last update: {0}{1}{1}", d.lastUpdate.ToString("dd/MM/yy HH:mm:sss"), vbCrLf)
            t.AppendLine("device sensors:")
            t.AppendLine()

            For Each s As SensorStatus In d.sensors
                t.AppendFormat("   sensor: {0}{1}", s.sensorId, vbCrLf)
                t.AppendFormat("   name: {0}{1}", s.sensorName, vbCrLf)
                t.AppendFormat("   last value: {0}{1}", s.currentValue, vbCrLf)
                t.AppendFormat("   last update: {0}{1}{1}", s.lastUpdate.ToString("dd/MM/yy HH:mm:sss"), vbCrLf)
            Next
        Next

        IoTHub.H.bot.sendMessageAsync(IoTHub.botChatId, t.ToString)
        IoTHub.H.bot.sendMessageAsync(IoTHub.debugChatId, t.ToString)

    End Sub

    Protected Sub SendLastImage(camName As String, msg As String)
        If cs IsNot Nothing Then
            Call hub.bot.sendPhotoAsync(IoTHub.debugChatId, cs.lastSnapshot.filename, cs.lastSnapshot.content, msg)
            Call hub.dbot.sendPhotoAsync(IoTHub.debugChatId, cs.lastSnapshot.filename, cs.lastSnapshot.content, msg)
        End If
    End Sub

    Protected Sub SendLastVideoClip(camName As String)
        If cs IsNot Nothing Then
            Call hub.bot.sendVideoAsync(IoTHub.debugChatId, cs.lastVideoClip.filename, cs.lastVideoClip.content)
        End If
    End Sub

End Class
