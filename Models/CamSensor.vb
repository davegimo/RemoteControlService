Imports gimus.iot.server

Public Class CamSensor
    Inherits Sensor
    Public lastVideoClip As UploadedFile
    Public lastSnapshot As UploadedFile

    Public Overrides Function ProcessSensorValue(sv As SensorValue, files As UploadedFiles) As Boolean

        ProcessSensorValue = MyBase.ProcessSensorValue(sv, files)

        Try
            If files IsNot Nothing Then
                Dim uf As UploadedFile = files.First
                If uf IsNot Nothing Then

                    If uf.filename.EndsWith(".mp4") Then
                        lastVideoClip = uf
                    End If

                    If uf.filename.EndsWith(".jpg") Then
                        lastSnapshot = uf
                    End If

                End If
            End If

        Catch ex As Exception

        End Try

    End Function

    Public Function command_StartClip(Optional durataInSecondi As Integer = 0) As DeviceCommand
        Dim c As String = String.Format("start_clip|{0}", durataInSecondi.ToString)
        Return Me.Command(c)
    End Function

    Public Function command_StopClip() As DeviceCommand
        Dim c As String = String.Format("stop_clip")
        Return Me.Command(c)
    End Function

End Class
