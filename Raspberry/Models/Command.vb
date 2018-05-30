Namespace server

    Public Class DeviceCommand
        Public Property commandId As String = Guid.NewGuid().ToString
        Public Property deviceId As String = ""
        Public Property sensorId As String = ""
        Public Property content As String = ""
        Public Property sent As DateTime = DateTime.MinValue
    End Class

End Namespace

