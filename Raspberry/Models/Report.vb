Imports Windows.Storage.Streams

Namespace server
    Public Class DeviceReport
        Public Property device As String
        Public Property name As String
        Public Property type As String
        Public Property value As Object
    End Class

    Public Class SensorValue
        Public Property sensor As String
        Public Property name As String
        Public Property type As String
        Public Property value As Object
    End Class

    Public Class MemoryFile
        Public Property filename As String
        Public Property buffer As Byte()

        Public Async Function asInMemoryRandomAccessStream() As Task(Of InMemoryRandomAccessStream)
            Dim RandomAccessStream As InMemoryRandomAccessStream = New InMemoryRandomAccessStream()
            Await RandomAccessStream.WriteAsync(buffer.AsBuffer())
            RandomAccessStream.Seek(0)
            Return RandomAccessStream
        End Function

        Public Function asStream() As Stream
            Return New IO.MemoryStream(buffer)
        End Function

        Public Function asStreamContent() As StreamContent
            Return New StreamContent(asStream())
        End Function

        Public Async Function asBitmapImage() As Task(Of BitmapImage)
            Try
                Dim bitmap As BitmapImage = New BitmapImage()
                Dim ras As InMemoryRandomAccessStream = Await asInMemoryRandomAccessStream()
                bitmap.SetSource(ras)
                Return bitmap

            Catch ex As Exception
                Return Nothing
            End Try

        End Function

    End Class

End Namespace
