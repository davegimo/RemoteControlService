Public Class SystemStatus
    Public Property devices As New List(Of DeviceStatus)
End Class

Public Class DeviceStatus
    Public Property deviceId As String
    Public Property deviceName As String
    Public Property lastUpdate As DateTime
    Public Property sensors As New List(Of SensorStatus)
End Class

Public Class SensorStatus
    Public Property sensorId As String
    Public Property sensorName As String
    Public Property currentValue As String
    Public Property lastUpdate As DateTime
End Class
