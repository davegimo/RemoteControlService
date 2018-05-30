
Public Class UploadedFile
        Public Property filename As String
        Public Property mimetype As String
    Public Property content As Byte()
End Class

Public Class UploadedFiles
    Inherits List(Of UploadedFile)
End Class



