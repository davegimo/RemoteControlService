Namespace client

    Public Interface iApiAsyncReceiver
        Sub dataReady(requestURI As String, data As Object)
        Sub dataError(requestURI As String, ex As Exception)
    End Interface

End Namespace
