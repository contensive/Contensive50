
Option Explicit On
Option Strict On

Imports System.Runtime.InteropServices
'
Namespace Contensive.Core
    ''' <summary>
    ''' This class provides a compatibility api for legacy active-script addons that use the "cclib" object for main class nad "csv" for the csv object
    ''' </summary>
    <ComVisible(True)>
    <ComClass(mainCsvCompatibilityClass.ClassId, mainCsvCompatibilityClass.InterfaceId, mainCsvCompatibilityClass.EventsId)>
    Public Class mainCsvCompatibilityClass
        '
#Region "COM GUIDs"
        Public Const ClassId As String = "D9099AAE-3FCB-4398-B94C-19EE7FA97B2B"
        Public Const InterfaceId As String = "CE342EA5-339F-4C31-9F90-F878F527E17A"
        Public Const EventsId As String = "21D9D0FB-9B5B-43C2-A7A5-3C84ABFAF90A"
#End Region
        '
        Private cpCore As coreClass

        Public Sub New(cpCore As coreClass)
            Me.cpCore = cpCore
        End Sub

        '
        '====================================================================================================
        '
        Public Function EncodeContent9(Source As Object, i0 As Integer, s2 As String, i3 As Integer, i4 As Integer, b5 As Boolean, b6 As Boolean, b7 As Boolean, b8 As Boolean, b9 As Boolean, b10 As Boolean, s11 As String, s12 As String, b13 As Boolean, i14 As Integer, s15 As String, i16 As Integer) As String
            Return cpCore.html.encodeContent10(Controllers.genericController.encodeText(Source), 0, "", 0, 0, False, False, False, True, True, False, "", "", False, 0, "", Contensive.BaseClasses.CPUtilsBaseClass.addonContext.ContextSimple, False, Nothing, False)
        End Function
    End Class
End Namespace
