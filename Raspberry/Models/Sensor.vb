Imports System.Net.Sockets

Imports Newtonsoft.Json.Linq

Namespace client

    Public Class Sensor
        Inherits gimus.iot.server.IotComponent
        Protected _state As gimus.iot.server.SensorStateEnum
        Public Property stateReason As String
        Protected _currentValue As String
        Public Property sensors As Sensors
        Public Property type As String = ""
        Public Property content As Object = Nothing

        Protected checkHardwareDeviceInterval As Integer = 300
        Protected WithEvents checkHardwareDeviceTimer As DispatcherTimer


        Protected HWProps As New Dictionary(Of String, Object)

        Public Sub New()
            _state = server.SensorStateEnum.disabled
            stateReason = ""
            checkHardwareDeviceTimer = New DispatcherTimer
        End Sub

        Private Sub checkHardwareDeviceTimer_Tick(sender As Object, e As Object) Handles checkHardwareDeviceTimer.Tick
            If _state = server.SensorStateEnum.enabled Then
                checkHardwareDeviceTimer.Interval = TimeSpan.FromMilliseconds(checkHardwareDeviceInterval)
                Try
                    OnCheckHardwareDeviceStatus()
                Catch ex As Exception
                End Try
            End If
        End Sub

        Protected Overridable Sub OnCheckHardwareDeviceStatus()
        End Sub

        Protected Sub updateHDProps(p As JObject)
            Try
                Dim k As String
                For Each c As Object In p.Values(Of JToken)
                    k = c.Name

                    If Not HWProps.ContainsKey(k) Then
                        HWProps.Add(k, Nothing)
                    End If
                    HWProps(k) = c.value.value
                Next

            Catch ex As Exception

            End Try
        End Sub

        Public Function GetHDProp(propName As String) As String
            If HWProps.ContainsKey(propName) Then
                Return HWProps(propName).ToString
            Else
                Return ""
            End If
        End Function

        Public Overridable Function buildSensorValueMessage() As server.SensorValue
            Dim sv As New server.SensorValue
            sv.sensor = id
            sv.value = currentValue
            sv.name = name
            sv.type = type
            Return sv
        End Function

        Public ReadOnly Property currentValue As String
            Get
                Return _currentValue
            End Get
        End Property

        Protected Sub setCurrentValue(val As String)
            Dim oldval As String = _currentValue
            _currentValue = val
            If oldval <> val Then
                Logger.push(String.Format("Sensor {0} new value: {1}", name, currentValue))
                OnValueChanged(oldval, val)
            End If
        End Sub

        Public Overridable Sub OnValueChanged(oldValue As String, newValue As String)
            If sensors IsNot Nothing And isEnabled Then
                sensors.OnSensorValueChanged(Me, oldValue, newValue)
            End If
        End Sub

        Public Overridable Sub onCommandReceived(command As server.DeviceCommand)
            Logger.push(String.Format("SENSOR {0} Command received from server: {1} ", name, command.content))
        End Sub

        Public Property isEnabled() As Boolean
            Get
                Return _state = server.SensorStateEnum.enabled
            End Get
            Set(value As Boolean)
                If value Then
                    _state = server.SensorStateEnum.enabled
                    Logger.push(String.Format("Sensor {0} is ENABLED", name))
                    checkHardwareDeviceTimer.Interval = TimeSpan.FromMilliseconds(checkHardwareDeviceInterval)
                    checkHardwareDeviceTimer.Start()
                Else
                    checkHardwareDeviceTimer.Stop()
                    _state = server.SensorStateEnum.disabled
                    Logger.push(String.Format("Sensor {0} is DISABLED", name))
                End If

            End Set
        End Property

        Protected Async Function sendBytes(ip As String, port As String, data() As Byte) As Task(Of Byte())
            Try
                Dim client As New TcpClient()
                Await client.ConnectAsync(ip, port)
                Dim stream As NetworkStream = client.GetStream()
                stream.Write(data, 0, data.Length)
                Dim rdata = New [Byte](20000) {}
                Dim bytes As Int32 = stream.Read(rdata, 0, rdata.Length)
                ReDim Preserve rdata(bytes - 1)
                Return rdata
            Catch ex As Exception
                Return Nothing
            End Try
        End Function

    End Class

    Public Class Sensors
        Inherits Dictionary(Of String, Sensor)
        Public Property device As Device

        Public Overloads Function Add(s As Sensor) As Sensor
            s.sensors = Me
            Me.Add(s.id, s)
            Return s
        End Function

        Public Function GetSensorByName(name As String) As Sensor
            Return Me.Values.Where(Function(x) x.name = name).FirstOrDefault
        End Function

        Public Function GetSensorById(id As String) As Sensor
            If Me.ContainsKey(id) Then
                Return Me(id)
            Else
                Return Nothing
            End If
        End Function

        Public Overridable Sub OnSensorValueChanged(sensor As Sensor, oldValue As String, newValue As String)
            If device IsNot Nothing Then
                device.OnSensorValueChanged(sensor, oldValue, newValue)
            End If
        End Sub

    End Class
End Namespace