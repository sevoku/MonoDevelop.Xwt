
Option Explicit On
Option Strict On

Imports System
Imports Xwt

Public Class MainWindow
	Inherits Window
	
	Public Sub New()
		Title = "${ProjectName}"
		Width = 400
		Height = 400

        MainMenu = New Menu()

        Dim FileMenu As New MenuItem("_File")
        FileMenu.SubMenu = New Menu()
        FileMenu.SubMenu.Items.Add(New MenuItem("_Open"))
        FileMenu.SubMenu.Items.Add(New MenuItem("_New"))
        Dim CloseItem As New MenuItem("_Close")
        AddHandler CloseItem.Clicked, AddressOf CloseClicked
        FileMenu.SubMenu.Items.Add(CloseItem)
        MainMenu.Items.Add(FileMenu)

        Content = New Label("${ProjectName}")
	End Sub

	Sub CloseClicked(ByVal sender As Object, ByVal e As EventArgs)
		Close ()
	End Sub

    Protected Overrides Function OnCloseRequested() As Boolean
        Dim allow_close As Boolean
        allow_close = MessageDialog.Confirm("${ProjectName} will be closed", Command.Ok)
        If allow_close Then
            Application.Exit()
        End If
        Return allow_close
    End Function
End Class
