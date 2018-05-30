Public Enum SensorStateEnum
    inactive = 0
    active = 1
End Enum

Public Class Sensor
    Inherits IotComponent
    Public Property sensors As Sensors
    Public Property state As SensorStateEnum
    Public Property stateReason As String
    Public Property oldValue As String
    Public Property value As String
    Public Property lastReport As DateTime
    Public Property files As UploadedFiles

    Public Overridable Function ProcessSensorValue(sv As SensorValue, files As UploadedFiles) As Boolean
        Me.oldValue = Me.value
        Me.value = sv.value
        Me.files = files
        lastReport = DateTime.Now
        Return oldValue <> value
    End Function

    Public Function Command(content As String) As DeviceCommand
        Dim dc As New DeviceCommand

        If sensors IsNot Nothing Then
            dc.deviceId = Me.sensors.deviceId
        End If

        dc.sensorId = id
        dc.content = content
        Return dc
    End Function


    Public Function getSensorStatus() As SensorStatus
        Dim ss As New SensorStatus
        ss.sensorId = Me.id
        ss.sensorName = Me.name
        ss.lastUpdate = Me.lastReport
        ss.currentValue = Me.value
        Return ss
    End Function
End Class


Public Class Sensors
    Inherits Dictionary(Of String, Sensor)
    Public Property deviceId As String

    Public Shared Function createSensor(id As String, name As String, type As String) As Sensor
        Dim s As Sensor
        Select Case type.ToUpper
            Case "WEBCAM"
                s = New CamSensor
            Case "SMART_SWITCH"
                s = New SmartSwitch
            Case Else
                s = New Sensor
        End Select
        s.id = id
        s.name = name
        Return s
    End Function

    Public Overloads Function Add(s As Sensor)
        Me.Add(s.id, s)
        s.sensors = Me
        Return s
    End Function

End Class
