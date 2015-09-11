Option Explicit On
Option Strict On

Imports System
Imports System.Runtime.InteropServices
Imports Xwt



Partial Public Class App
    Private Shared myToolkitWindows As ToolkitType = ToolkitType.Wpf
    Private Shared myToolkitLinux As ToolkitType = ToolkitType.Gtk
    Private Shared myToolkitMacOS As ToolkitType = ToolkitType.XamMac

    Public Shared Property ToolkitWindows() As ToolkitType
        Get
            Return myToolkitWindows
        End Get
        Private Set(ByVal value As ToolkitType)
            myToolkitWindows = value
        End Set
    End Property

    Public Shared Property ToolkitLinux() As ToolkitType
        Get
            Return myToolkitLinux
        End Get
        Private Set(ByVal value As ToolkitType)
            myToolkitLinux = value
        End Set
    End Property

    Public Shared Property ToolkitMacOS() As ToolkitType
        Get
            Return myToolkitMacOS
        End Get
        Set(ByVal value As ToolkitType)
            myToolkitMacOS = value
        End Set
    End Property

    Public Shared Sub Run(ByVal XwtToolkit As ToolkitType,
                          ByVal args As String())

        Application.Initialize(XwtToolkit)

        OnRun(args)

        Application.Run()

        OnExit()
        Application.Dispose()

    End Sub

    Public Shared Sub Run(ByVal args As String())

        Dim XwtToolkit As Nullable(Of ToolkitType)

        For Each arg As String In args
            If (arg.ToLower().StartsWith("toolkittype", StringComparison.Ordinal) And arg.Contains("=")) Then
                Dim tkname As String = arg.Split(CChar("="))(1).Trim()
                Try
                    XwtToolkit = CType([Enum].Parse(GetType(ToolkitType), tkname, True), ToolkitType)
                Catch ex As ArgumentException
                    Throw New ArgumentException("Unknown Xwt Toolkit type: " + tkname, ex)
                End Try
            End If
        Next arg

        If Not XwtToolkit.HasValue Then
            XwtToolkit = DetectPlatformToolkit()
        End If

        Run(CType(XwtToolkit, Xwt.ToolkitType), args)

    End Sub

    Shared Function DetectPlatformToolkit() As ToolkitType

        Select Case Environment.OSVersion.Platform
            Case PlatformID.MacOSX
                Return ToolkitMacOS
            Case PlatformID.Unix
                Return If(IsRunningOnMac(), ToolkitMacOS, ToolkitLinux)
            Case Else
                Return ToolkitWindows
        End Select

    End Function

    ' from Xwt.GtkBackend.Platform
    Private Shared Function IsRunningOnMac() As Boolean
        Dim buf As IntPtr = IntPtr.Zero
        Try
            buf = Marshal.AllocHGlobal(8192)
            ' This is a hacktastic way of getting sysname from uname ()
            If uname(buf) = 0 Then
                Dim Os As String = Marshal.PtrToStringAnsi(buf)
                If Os.ToUpper() = "DARWIN" Then
                    Return True
                End If
            End If
        Catch ex As Exception
        Finally
            If buf <> IntPtr.Zero Then
                Marshal.FreeHGlobal(buf)
            End If
        End Try
        Return False
    End Function

    <DllImport("libc")> _
    Private Shared Function uname(buf As IntPtr) As Integer
    End Function

End Class

