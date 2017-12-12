
Imports System
Imports System.IO
Imports System.Runtime.InteropServices
Imports System.Net
'
#Const DebugBuild = False

Namespace Contensive.Core

    <ComVisible(True)> _
    <ProgId("guidClass")> _
    <ComClass(guidClass.ClassId, guidClass.InterfaceId, guidClass.EventsId)> _
    Public Class guidClass
        '
#Region "COM GUIDs"
        ' These  GUIDs provide the COM identity for this class 
        ' and its COM interfaces. If you change them, existing 
        ' clients will no longer be able to access the class.
        Public Const ClassId As String = "A93C5B74-9BDD-48C3-8AD4-05D60A23288F"
        Public Const InterfaceId As String = "34A3C84F-C644-4BA8-9CA5-635247779FE6"
        Public Const EventsId As String = "35007A82-5552-45AF-B59F-B8680BE5EBC5"
#End Region
        '
        '======================================================================================
        '   Requests the doc and saves the body in the file specified
        '
        '   check the HTTPResponse and SocketResponse when done
        '   If the HTTPResponse is "", Check the SocketResponse
        '======================================================================================
        '
        Public Function getGuid() As String
            Return Guid.NewGuid().ToString
        End Function
    End Class

End Namespace

