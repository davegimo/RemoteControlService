Imports gimus.iot
Imports Windows.Devices.Gpio
Namespace client

    Public Class RaspberryP3
        Inherits gimus.iot.client.Device

        Public Sub New()
            MyBase.New
            InitGPIO()
        End Sub

        Protected Sub InitGPIO()
            Dim s As client.RbyGpioPin
            For i As Integer = 1 To 27
                Try
                    s = New gimus.iot.client.RbyGpioPin(i)
                    Me.sensors.Add(s)
                Catch ex As Exception
                End Try
            Next
        End Sub

        Public Function getGPIOPin(pin_index As Integer) As RbyGpioPin
            Return sensors.GetSensorByName("GPIO_" & pin_index)
        End Function

        Public Sub enablePin(pin_index As Integer, Optional enabled As Boolean = True)
            Dim p As RbyGpioPin = getGPIOPin(pin_index)
            p.isEnabled = enabled
        End Sub

    End Class

End Namespace


