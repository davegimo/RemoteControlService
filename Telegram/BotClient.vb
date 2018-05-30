Imports Telegram.Bot
Imports Telegram.Bot.Args
Public Class TelegramBotClient
    Public name As String = "BOT"
    Public chatId As Integer = 0
    Protected bot As Telegram.Bot.TelegramBotClient
    Public Event TextMessageReceived(sender As TelegramBotClient, messageText As String)

    Public Sub New(name As String, botKey As String, chatId As Integer)
        Me.name = name
        Me.chatId = chatId
        If botKey = "" Then
            botKey = "585465276:AAEhUzjRvvi-cr0frws7Vs9h-8JCRy7f8vk"
        End If
        bot = New Telegram.Bot.TelegramBotClient(botKey)

        AddHandler bot.OnMessage, AddressOf bot_OnMessage

    End Sub

    Public Sub StartReceiving()
        bot.StartReceiving()
    End Sub

    Public Sub StopReceiving()
        bot.StopReceiving()
    End Sub

    Public Async Sub sendMessageAsync(chatId As Integer, messageText As String)
        Try
            Dim t = Await bot.SendTextMessageAsync(chatId, messageText)
        Catch ex As Exception
            Throw New Exception("Errore inviando un messaggio con telegram: " & ex.Message, ex)
        End Try
    End Sub

    Public Async Sub sendVideoAsync(chatId As Integer, filename As String, content() As Byte)
        Dim ms As New IO.MemoryStream(content)
        Await sendVideoAsync(chatId, filename, ms)
    End Sub

    Public Async Function sendVideoAsync(chatId As Integer, filename As String, str As IO.Stream) As Task
        Dim fts As Types.InputFiles.InputOnlineFile = New Types.InputFiles.InputOnlineFile(str, filename)
        Dim t = Await bot.SendVideoAsync(chatId, fts)
    End Function


    Public Async Sub sendPhotoAsync(chatId As Integer, filename As String, content() As Byte, Optional message As String = "")
        Dim ms As New IO.MemoryStream(content)
        Try
            Await sendPhotoAsync(chatId, filename, ms, message)
        Catch ex As Exception

        End Try
    End Sub

    Public Async Function sendPhotoAsync(chatId As Integer, filename As String, str As IO.Stream, Optional message As String = "") As Task
        Dim fts As Types.InputFiles.InputOnlineFile = New Types.InputFiles.InputOnlineFile(str, filename)
        Try
            Dim t = Await bot.SendPhotoAsync(chatId, fts, message)
        Catch ex As Exception

        End Try
    End Function

    Public Async Sub sendPhotoAsync(chatId As Integer, filename As String, filePath As String, Optional message As String = "")
        Dim str As New IO.FileStream(filePath, IO.FileMode.Open)
        Try
            Await sendPhotoAsync(chatId, filename, str, message)
        Catch ex As Exception

        End Try
    End Sub


    Public Async Function sendFileAsync(chatId As Integer, filePath As String) As Task
        Try
            Dim str As New IO.FileStream(filePath, IO.FileMode.Open)
            Dim fi As New IO.FileInfo(filePath)
            Dim fts As Types.InputFiles.InputOnlineFile = New Types.InputFiles.InputOnlineFile(str, fi.Name)
            Dim t = Await bot.SendDocumentAsync(chatId, fts)

        Catch ex As Exception

        End Try
    End Function

    Private Sub bot_OnMessage(sender As Object, e As MessageEventArgs)
        Dim m As Types.Message = e.Message
        Select Case m.Type
            Case Types.Enums.MessageType.Text
                RaiseEvent TextMessageReceived(Me, m.Text)
            Case Else
        End Select
    End Sub

End Class

