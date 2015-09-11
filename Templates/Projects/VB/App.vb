
Option Explicit On
Option Strict On

Imports System
Imports Xwt



Partial Public Class App

    Shared mainWindow As MainWindow

    Shared Sub New()
        ToolkitWindows = ToolkitType.Wpf
        ToolkitLinux = ToolkitType.Gtk
        ToolkitMacOS = ToolkitType.XamMac
    End Sub

    Private Shared Sub OnRun(ByVal args As String())
        mainWindow = New MainWindow()
        mainWindow.Show()
    End Sub

    Private Shared Sub OnExit()
        If (mainWindow IsNot Nothing) Then
            mainWindow.Dispose()
        End If
    End Sub

End Class
