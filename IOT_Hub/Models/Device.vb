Public Class Device
    Inherits IotComponent
    Public ReadOnly Property sensors As New Sensors
    Public Property lastReport As DateTime


    Public Sub New()
        sensors = New Sensors
        sensors.deviceId = componentId
    End Sub

    Public Overrides Property id As String
        Get
            Return componentId
        End Get
        Set(value As String)
            componentId = value
            sensors.deviceId = componentId
        End Set
    End Property

    Public Function getDeviceStatus() As DeviceStatus
        Dim ds As New DeviceStatus
        ds.deviceId = Me.id
        ds.deviceName = Me.name
        ds.lastUpdate = Me.lastReport

        For Each ss As Sensor In Me.sensors.Values
            ds.sensors.Add(ss.getSensorStatus())
        Next
        Return ds
    End Function

End Class

Public Class Devices
    Inherits Dictionary(Of String, Device)

End Class
