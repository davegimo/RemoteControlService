Imports System.Net.Http
Imports System.Net.Security
Namespace client
    Public Class ClientBase
        Protected Client As HttpClient
        Protected _LastResponseOk As Boolean
        Protected _LastResponseMessage As String
        Protected _LastException As Exception

        Public Sub New(Optional __baseAddress As String = "")
            prepareClient()
            If __baseAddress <> "" Then
                baseAddress = New Uri(__baseAddress)
            End If
        End Sub

        Public ReadOnly Property serverName As String
            Get
                If baseAddress IsNot Nothing Then
                    Return baseAddress.Authority
                Else
                    Return "non connesso"
                End If
            End Get
        End Property

        Public ReadOnly Property LastResponseOK As Boolean
            Get
                Return _LastResponseOk
            End Get
        End Property

        Public ReadOnly Property LastResponseMessage As String
            Get
                Return _LastResponseMessage
            End Get
        End Property

        Public ReadOnly Property LastException As Exception
            Get
                Return _LastException
            End Get
        End Property

        Public Property baseAddress() As Uri
            Get
                Return Client.BaseAddress
            End Get
            Set(value As Uri)
                Client.BaseAddress = value
            End Set
        End Property

        Protected Sub prepareClient()

            Client = New HttpClient()
            Client.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1")
            Client.DefaultRequestHeaders.Add("User-Agent", "GimusIotApiClient")
            Client.DefaultRequestHeaders.Add("Accept", "Application/json, Text/javascript, */*; q=0.01")
            '            Client.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, sdch, br")
            Client.DefaultRequestHeaders.Add("Accept-Language", "it-IT,it;q=0.8,en-US;q=0.6,en;q=0.4")
            Client.DefaultRequestHeaders.Add("Accept-Charset", "UTF-8")
            Client.DefaultRequestHeaders.Add("X-Requested-With", "XMLHttpRequest")

        End Sub

        Protected Function GetPostResponse(requestURI As String, requestContent As MultipartFormDataContent) As HttpResponseMessage
            Try
                _LastResponseMessage = "OK"
                _LastResponseOk = True

                Dim response As HttpResponseMessage = Client.PostAsync(requestURI, requestContent).Result

                If Not response.IsSuccessStatusCode Then
                    _LastResponseMessage = "il metodo PostAsync è stato eseguito senza successo: " & response.ReasonPhrase
                    _LastResponseOk = False
                End If

                Return response
            Catch ex As Exception
                _LastResponseMessage = "Si è verificato un errore chiamando il metodo PostAsync: " & vbCrLf & ex.Message
                _LastResponseOk = False
                Return Nothing
            End Try

        End Function

        Protected Function GetResponse(requestURI) As HttpResponseMessage
            Try
                _LastResponseMessage = "OK"
                _LastResponseOk = True

                Dim response As HttpResponseMessage = Client.GetAsync(requestURI).Result

                If Not response.IsSuccessStatusCode Then
                    _LastResponseMessage = "il metodo GetAsync è stato eseguito senza successo: " & response.ReasonPhrase
                    _LastResponseOk = False
                End If

                Return response
            Catch ex As Exception
                _LastResponseMessage = "Si è verificato un errore chiamando il metodo GetAsync: " & vbCrLf & ex.Message
                _LastResponseOk = False
                Return Nothing
            End Try

        End Function

        Protected Overridable Function httpResponseMessageDataConverter(functionName As String, response As HttpResponseMessage) As Object
            Return Nothing
        End Function

        Protected Function ApiPostCallHandler(functionName As String, RequestURL As String, requestContent As MultipartFormDataContent, receiver As iApiAsyncReceiver) As Object
            If receiver Is Nothing Then
                Return httpResponseMessageDataConverter(functionName, GetPostResponse(RequestURL, requestContent))
            Else
                postAsyncCallHandler(functionName, RequestURL, requestContent, receiver)
                Return Nothing
            End If
        End Function


        Protected Function ApiGetCallHandler(functionName As String, RequestURL As String, receiver As iApiAsyncReceiver) As Object
            If receiver Is Nothing Then
                Return httpResponseMessageDataConverter(functionName, GetResponse(RequestURL))
            Else
                getAsyncCallHandler(functionName, RequestURL, receiver)
                Return Nothing
            End If
        End Function

        Public Async Sub postAsyncCallHandler(functionName As String, requestURL As String, requestContent As MultipartFormDataContent, receiver As iApiAsyncReceiver)
            _LastResponseMessage = ""
            _LastResponseOk = True
            _LastException = Nothing

            Try
                Dim response As HttpResponseMessage = Await Client.PostAsync(requestURL, requestContent)

                If response.IsSuccessStatusCode Then
                    receiver.dataReady(requestURL, httpResponseMessageDataConverter(functionName, response))
                Else
                    _LastResponseMessage = functionName & "_async: Il metodo PostAsync è stato eseguito senza successo: " & response.ReasonPhrase
                End If

            Catch ex As Exception
                _LastException = ex
                _LastResponseMessage = functionName & "_async: Si è verificato un errore chiamando il metodo PostAsync: " & vbCrLf & ex.Message
            End Try

            If _LastResponseMessage <> "" Then
                _LastResponseOk = False
                receiver.dataError(requestURL, New Exception(_LastResponseMessage, _LastException))
            End If
        End Sub

        Public Async Sub getAsyncCallHandler(functionName As String, requestURL As String, receiver As iApiAsyncReceiver)
            _LastResponseMessage = ""
            _LastResponseOk = True
            _LastException = Nothing

            Try
                Dim response As HttpResponseMessage = Await Client.GetAsync(requestURL)

                If response.IsSuccessStatusCode Then
                    receiver.dataReady(requestURL, httpResponseMessageDataConverter(functionName, response))
                Else
                    _LastResponseMessage = functionName & "_async: Il metodo GetAsync è stato eseguito senza successo: " & response.ReasonPhrase
                End If

            Catch ex As Exception
                _LastException = ex
                _LastResponseMessage = functionName & "_async: Si è verificato un errore chiamando il metodo GetAsync: " & vbCrLf & ex.Message
            End Try

            If _LastResponseMessage <> "" Then
                _LastResponseOk = False
                receiver.dataError(requestURL, New Exception(_LastResponseMessage, _LastException))
            End If
        End Sub

    End Class

End Namespace
