Public MustInherit Class IotComponent
    Protected componentId As String = Guid.NewGuid().ToString
    Protected _name As String = ""

    Public Overridable Property id As String
        Get
            Return componentId
        End Get
        Set(value As String)
            componentId = value
        End Set
    End Property


    Public Property name As String
        Get
            Return _name
        End Get
        Set(value As String)
            _name = value
        End Set
    End Property
End Class


Public Class IotComponents
    Inherits Dictionary(Of String, IotComponent)

    Public Overloads Function Add(c As IotComponent)
        If c IsNot Nothing Then
            Me.Add(c.id, c)
        End If
        Return c
    End Function

End Class