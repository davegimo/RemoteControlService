Imports Windows.Storage
Imports System.Threading
Imports gimus.iot.server
Imports Windows.Media.Capture
Imports Windows.Media.MediaProperties
Imports Windows.Storage.Streams

Namespace client
    Public Class CamSensor
        Inherits Sensor

        Protected snapshotCounter As Integer = 0
        Protected videoclipCounter As Integer = 1000
        Protected currentVideoClip As StorageFile = Nothing
        Protected stopRecordingTimer As Timer

        ' ----------------------------------------------------------

        Protected _isRecording As Boolean = False
        Protected _isPreviewing As Boolean = False

        Protected mediaRecording As LowLagMediaRecording
        Protected mediaCapture As MediaCapture
        Protected previewElement As CaptureElement

        Protected _currentAudioFile As StorageFile
        Protected _currentVideoFile As StorageFile

        Public LastException As Exception

        ' ----------------------------------------------------------

        Public Sub New()
            MyBase.New()
            Me.type = "webcam"
            Me.name = "HD3000"
            Me.id = "webcam1"
            checkHardwareDeviceInterval = 1000
            stopRecordingTimer = New Timer(AddressOf onRecordingTimerTick, Nothing, Timeout.Infinite, Timeout.Infinite)
        End Sub

        Protected Async Sub onRecordingTimerTick(state As Object)
            Await StopVideoRecording()
        End Sub

        Public Overrides Sub onCommandReceived(command As DeviceCommand)
            MyBase.onCommandReceived(command)
            Dim c() As String = command.content.Split("|")
            Select Case c(0)
                Case "start_clip"
                    Call StartVideoRecording(c(1))
                Case "stop_clip"
                    Call StopVideoRecording()
                Case Else
            End Select
        End Sub

        Protected Overrides Sub OnCheckHardwareDeviceStatus()
            MyBase.OnCheckHardwareDeviceStatus()
            Try
                Call NewSnapshot()
            Catch ex As Exception
            End Try
        End Sub


        Public ReadOnly Property isRecording As Boolean
            Get
                Return CAM_isRecording
            End Get
        End Property


        Public Overloads Async Function NewSnapshot() As Task(Of Boolean)
            If Not CAM_isRecording Then
                content = Nothing
                snapshotCounter += 1

                Dim mf As New server.MemoryFile
                mf.filename = "snapshot " & snapshotCounter.ToString & ".jpg"
                mf.buffer = Await CAM_TakeSnapshot()

                If mf.buffer IsNot Nothing Then
                    content = mf
                    If content IsNot Nothing Then
                        setCurrentValue("snapshot " & snapshotCounter.ToString)
                        Return True
                    End If
                Else
                    Logger.push(String.Format("ERROR Taking Webcam Snapshot {0}:", LastException.Message))
                End If

            End If
            Return False
        End Function

        Public Overloads Async Sub StartVideoRecording(Optional durataSecondi As Integer = 300, Optional fileName As String = "")
            If fileName = "" Then
                fileName = "videoclip_" & DateTime.Now().ToString("dd_MM_yyyy_HH_mm_ss") & ".mp4"
            End If


            Await StopVideoRecording()


            videoclipCounter += 1

            currentVideoClip = Await CAM_StartVideoRecording(fileName)


            stopRecordingTimer.Change(durataSecondi * 1000 + 100, Timeout.Infinite)

            Logger.push(String.Format("CAM {0} start recording videoclip: {1}, max duration: {2} secs", name, fileName, durataSecondi))


        End Sub

        Public Overloads Async Function StopVideoRecording() As Task(Of Boolean)
            If CAM_isRecording Then
                Await CAM_StopRecording()
                stopRecordingTimer.Change(Timeout.Infinite, Timeout.Infinite)

                Try
                    Dim mf As New server.MemoryFile
                    mf.filename = "videoclip " & currentVideoClip.Name

                    Dim s As Stream = Await currentVideoClip.OpenStreamForReadAsync()
                    Dim b(s.Length) As Byte
                    Await s.ReadAsync(b, 0, s.Length)
                    s.Dispose()

                    mf.buffer = b
                    Me.content = mf
                    setCurrentValue("videoclip " & videoclipCounter.ToString)

                    Logger.push(String.Format("CAM {0} stop recording videoclip", name))

                    Return True
                Catch ex As Exception
                    Logger.push(String.Format("ERROR stopping videoclip {0}:", ex.Message))
                End Try
            End If
            Return False
        End Function

        ' --------------------------------------------------------------------------------------------------------------

        Protected ReadOnly Property CAM_isRecording As Boolean
            Get
                Return _isRecording
            End Get
        End Property

        Protected ReadOnly Property CAM_isPreviewing As Boolean
            Get
                Return _isPreviewing
            End Get
        End Property

        'Public ReadOnly Property currentAudioFile As StorageFile
        '    Get
        '        Return _currentAudioFile
        '    End Get
        'End Property

        Public Async Function InitWebCam() As Task(Of Boolean)
            Try

                If Not mediaCapture Is Nothing Then
                    If _isPreviewing Then
                        Await mediaCapture.StopPreviewAsync()
                        _isPreviewing = False
                    End If
                    If _isRecording Then
                        Await mediaCapture.StopRecordAsync()
                        _isRecording = False
                    End If
                Else

                    mediaCapture = New MediaCapture()
                    Await mediaCapture.InitializeAsync()

                    AddHandler mediaCapture.Failed, New MediaCaptureFailedEventHandler(AddressOf mediaCapture_Failed)
                    AddHandler mediaCapture.RecordLimitationExceeded, New Windows.Media.Capture.RecordLimitationExceededEventHandler(AddressOf mediaCapture_RecordLimitExceeded)

                End If



                Return True
            Catch ex As Exception
                LastException = ex
                Return False
            End Try

        End Function


        Protected Async Function CAM_StartPreview(previewElement As CaptureElement) As Task(Of Boolean)
            LastException = Nothing

            Try
                If (Await InitWebCam()) Then
                    previewElement.Source = mediaCapture
                    Await mediaCapture.StartPreviewAsync()
                    _isPreviewing = True
                    Me.previewElement = previewElement
                    Return True

                End If
            Catch ex As Exception
            End Try
            Return False
        End Function

        Protected Async Function CAM_TakeSnapshotStream() As Task(Of Stream)
            Try
                If (Await InitWebCam()) Then
                    Dim RandomAccessStream As New InMemoryRandomAccessStream
                    Await mediaCapture.CapturePhotoToStreamAsync(ImageEncodingProperties.CreateJpeg, RandomAccessStream)
                    RandomAccessStream.Seek(0)

                    Return RandomAccessStream.AsStream

                End If

            Catch ex As Exception
                LastException = ex
            End Try
            Return Nothing
        End Function


        Protected Async Function CAM_TakeSnapshotBitmap() As Task(Of BitmapImage)
            Dim RandomAccessStream As New InMemoryRandomAccessStream
            Await mediaCapture.CapturePhotoToStreamAsync(ImageEncodingProperties.CreateJpeg, RandomAccessStream)
            RandomAccessStream.Seek(0)
            Dim bitmap As BitmapImage = New BitmapImage()
            bitmap.SetSource(RandomAccessStream)
            RandomAccessStream.Dispose()
            Return bitmap
        End Function


        Protected Async Function CAM_TakeSnapshotFile() As Task(Of StorageFile)
            Dim photoFile As StorageFile = Nothing
            Try
                If (Await InitWebCam()) Then
                    photoFile = Await KnownFolders.PicturesLibrary.CreateFileAsync("last_snapshot.jpg", CreationCollisionOption.ReplaceExisting)
                    Await Me.mediaCapture.CapturePhotoToStorageFileAsync(ImageEncodingProperties.CreateJpeg, photoFile)
                    Await CAM_Cleanup()
                    Return photoFile
                End If
            Catch ex As Exception
                Try
                    Call photoFile.DeleteAsync()
                Catch ex2 As Exception
                End Try
            Finally
            End Try
            Await CAM_Cleanup()
            Return Nothing
        End Function

        Protected Async Function CAM_TakeSnapshot() As Task(Of Byte())
            Try
                If (Await InitWebCam()) Then
                    Dim s As Stream = Await CAM_TakeSnapshotStream()
                    Dim buf(s.Length) As Byte
                    Await s.ReadAsync(buf, 0, s.Length)
                    Return buf
                End If
            Catch ex As Exception
            Finally
            End Try
            Return Nothing
        End Function

        Protected Async Function CAM_StartVideoRecording(filename As String) As Task(Of StorageFile)
            Try
                _isRecording = True
                Dim myVideos As StorageLibrary = Await Windows.Storage.StorageLibrary.GetLibraryAsync(Windows.Storage.KnownLibraryId.Videos)
                Dim File As StorageFile = Await myVideos.SaveFolder.CreateFileAsync(filename, CreationCollisionOption.ReplaceExisting)
                mediaRecording = Await mediaCapture.PrepareLowLagRecordToStorageFileAsync(MediaEncodingProfile.CreateMp4(VideoEncodingQuality.Auto), File)
                Await mediaRecording.StartAsync()

                Return File

            Catch ex As Exception
                _isRecording = False
                Logger.push(String.Format("ERROR CAM {0} - {1}", name, ex.Message))

                Return Nothing
            End Try
        End Function

        Protected Async Function CAM_StopRecording() As Task(Of Boolean)
            Try
                Await mediaRecording.StopAsync()
                _isRecording = False
                Return True
            Catch ex As Exception
                Logger.push(String.Format("ERROR CAM {0} - {1}", name, ex.Message))
                _isRecording = False
                Return False
            End Try
        End Function


        Protected Async Sub mediaCapture_Failed(ByVal currentCaptureObject As MediaCapture, ByVal currentFailure As MediaCaptureFailedEventArgs)
            Try
                If _isRecording Then
                    Await mediaCapture.StopRecordAsync()
                    _isRecording = False
                End If
            Catch __unusedException1__ As Exception
            Finally
            End Try
        End Sub

        Protected Async Sub mediaCapture_RecordLimitExceeded(ByVal currentCaptureObject As Windows.Media.Capture.MediaCapture)
            Try
                If _isRecording Then
                    Await mediaCapture.StopRecordAsync()
                    _isRecording = False
                End If
            Catch __unusedException1__ As Exception
            Finally
            End Try
        End Sub

        Private Async Function CAM_Cleanup() As Task
            If mediaCapture IsNot Nothing Then
                If _isPreviewing Then
                    Await mediaCapture.StopPreviewAsync()
                    _isPreviewing = False
                End If

                If _isRecording Then
                    Await mediaCapture.StopRecordAsync()
                    _isRecording = False
                End If

                mediaCapture.Dispose()
                mediaCapture = Nothing
            End If
        End Function

    End Class

End Namespace
