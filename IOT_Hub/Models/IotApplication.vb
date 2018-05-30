Imports gimus.iot.server

Public MustInherit Class IotApplication
    Protected WithEvents hub As IoTHub = IoTHub.H

    Private Sub hub_SensorReportReceived(sensor As Sensor) Handles hub.SensorReportReceived
        OnSensorReportReceived(sensor)
    End Sub

    Public Overridable Sub OnSensorReportReceived(sensor As Sensor)
    End Sub

    Private Sub hub_SensorValueChanged(sensor As Sensor, oldValue As String, newValue As String) Handles hub.SensorValueChanged
        OnSensorValueChanged(sensor, oldValue, newValue)
    End Sub

    Public Overridable Sub OnSensorValueChanged(sensor As Sensor, oldValue As String, newValue As String)
    End Sub

    Private Sub hub_BotMessageReceived(sender As TelegramBotClient, msg As String) Handles hub.BotMessageReceived
        OnBotMessageReceived(sender, msg)
    End Sub

    Public Overridable Sub OnBotMessageReceived(sender As TelegramBotClient, msg As String)
    End Sub

End Class
