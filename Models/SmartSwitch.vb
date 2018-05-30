Public Class SmartSwitch
    Inherits Sensor

    Public Overrides Function ProcessSensorValue(sv As SensorValue, files As UploadedFiles) As Boolean
        ProcessSensorValue = MyBase.ProcessSensorValue(sv, files)
    End Function

    Public Function command_SetOn() As DeviceCommand
        Dim c As String = String.Format("set_on")
        Return Me.Command(c)
    End Function
    Public Function command_SetOff() As DeviceCommand
        Dim c As String = String.Format("set_off")
        Return Me.Command(c)
    End Function


End Class
