Public Class DeviceCommand
    Public Property commandId As String = Guid.NewGuid().ToString
    Public Property deviceId As String = ""
    Public Property sensorId As String = ""
    Public Property content As String = ""
    Public Property sent As DateTime = DateTime.MinValue
End Class

Public Class DeviceCommands
    Inherits Dictionary(Of String, DeviceCommand)

    Public Overloads Function Add(c As DeviceCommand) As DeviceCommand
        Me.Add(c.commandId, c)
        Return c
    End Function

    Public Overloads Function Add(deviceID As String, sensorId As String, content As String) As DeviceCommand
        Dim dc As New DeviceCommand
        dc.deviceId = deviceID
        dc.sensorId = sensorId
        dc.content = content
        dc.sent = DateTime.Now
        Me.Add(dc.commandId, dc)
        Return dc
    End Function

    Public Function getDeviceCommands(deviceId As String, Optional addAck As Boolean = True) As List(Of DeviceCommand)
        Dim l As New List(Of DeviceCommand)
        l.AddRange(Me.Values.Where(Function(x) x.deviceId = deviceId))
        For Each dc As DeviceCommand In l
            Me.Remove(dc.commandId)
        Next
        If addAck Then
            Dim dc As New DeviceCommand
            dc.content = "ack"
            dc.deviceId = deviceId
            dc.sent = DateTime.Now
            l.Add(dc)
        End If
        Return l
    End Function

End Class
