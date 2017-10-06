
Option Strict Off
Option Explicit On

Imports System.Runtime.InteropServices

Namespace Contensive.Core
    '
    ' comVisible to be activeScript compatible
    '
    <ComVisible(True)>
    <ComClass(CPBlockClass.ClassId, CPBlockClass.InterfaceId, CPBlockClass.EventsId)>
    Public Class CPBlockClass
        Inherits BaseClasses.CPBlockBaseClass
        Implements IDisposable
        '
#Region "COM GUIDs"
        Public Const ClassId As String = "9E4DF603-A94B-4E3A-BD06-E19BB9CB1B5F"
        Public Const InterfaceId As String = "E4D5D9F0-DF96-492E-9CAC-1107F0187A40"
        Public Const EventsId As String = "5911548D-7637-4021-BD08-C7676F3E12C6"
#End Region
        '
        Private Property cpCore As Contensive.Core.coreClass
        Private Property cp As CPClass
        Private Property accum As String
        Private Property htmlDoc As Controllers.htmlController
        Protected Property disposed As Boolean = False
        '
        '====================================================================================================
        ''' <summary>
        ''' Constructor - Initialize the Main and Csv objects
        ''' </summary>
        ''' <param name="cpParent"></param>
        Public Sub New(ByRef cpParent As CPClass)
            MyBase.New()
            Try
                accum = ""
                cp = cpParent
                cpCore = cp.core
                Try
                    htmlDoc = New Controllers.htmlController(cpCore)
                Catch ex As Exception
                    cpCore.handleException(ex, "Error creating object Controllers.htmlToolsController during cp.block constructor.") : Throw
                End Try
            Catch ex As Exception
                Call cpCore.handleException(ex) : Throw
            End Try
        End Sub
        '
        '====================================================================================================
        '
        Public Overrides Sub Load(ByVal htmlString As String)
            Try
                accum = htmlString
            Catch ex As Exception
                Call cpCore.handleException(ex) : Throw
            End Try
        End Sub
        '
        '====================================================================================================
        '
        Public Overrides Sub Append(ByVal htmlString As String)
            Try
                accum &= htmlString
            Catch ex As Exception
                Call cpCore.handleException(ex) : Throw
            End Try
        End Sub
        '
        '====================================================================================================
        '
        Public Overrides Sub Clear()
            Try
                accum = ""
            Catch ex As Exception
                Call cpCore.handleException(ex) : Throw
            End Try
        End Sub
        '
        '====================================================================================================
        '
        Public Overrides Function GetHtml() As String
            Return accum
        End Function
        '
        '====================================================================================================
        '
        Public Overrides Function GetInner(ByVal findSelector As String) As String
            Dim s As String = ""
            Try
                Dim a As String = accum
                If findSelector <> "" Then
                    s = htmlDoc.getInnerHTML(cpCore, a, findSelector)
                End If
            Catch ex As Exception
                Call cpCore.handleException(ex) : Throw
            End Try
            Return s
        End Function
        '
        '====================================================================================================
        '
        Public Overrides Function GetOuter(ByVal findSelector As String) As String
            Dim s As String = ""
            Try
                Dim a As String = accum
                If findSelector <> "" Then
                    s = htmlDoc.getOuterHTML(cpCore, a, findSelector)
                End If
            Catch ex As Exception
                Call cpCore.handleException(ex) : Throw
            End Try
            Return s
        End Function
        '
        '====================================================================================================
        '
        Public Overrides Sub ImportFile(ByVal wwwFileName As String)
            Dim headTags As String = ""
            Try
                If wwwFileName <> "" Then
                    accum = cp.wwwFiles.read(wwwFileName)
                    If accum <> "" Then
                        headTags = Controllers.htmlController.getTagInnerHTML(accum, "head", False)
                        If headTags <> "" Then
                            Call cpCore.doc.addHeadTags(headTags)
                        End If
                        accum = Controllers.htmlController.getTagInnerHTML(accum, "body", False)
                    End If
                End If
            Catch ex As Exception
                Call cpCore.handleException(ex) : Throw
            End Try
        End Sub
        '
        '====================================================================================================
        '
        Public Overrides Sub OpenCopy(ByVal copyRecordNameOrGuid As String)
            Dim cs As CPCSClass = cp.CSNew
            Try
                accum = ""
                If copyRecordNameOrGuid <> "" Then
                    Call cs.Open("copy content", "(name=" & cp.Db.EncodeSQLText(copyRecordNameOrGuid) & ")or(ccGuid=" & cp.Db.EncodeSQLText(copyRecordNameOrGuid) & ")", "id", , "copy")
                    If cs.Ok Then
                        accum = cs.GetText("copy")
                    End If
                    Call cs.Close()
                End If
            Catch ex As Exception
                Call cpCore.handleException(ex) : Throw
            End Try
        End Sub
        '
        '====================================================================================================
        '
        Public Overrides Sub OpenFile(ByVal wwwFileName As String)
            Try
                accum = ""
                If (Not String.IsNullOrEmpty(wwwFileName)) Then
                    accum = cp.wwwFiles.read(wwwFileName)
                End If
            Catch ex As Exception
                Call cpCore.handleException(ex) : Throw
            End Try
        End Sub
        '
        '====================================================================================================
        '
        Public Overrides Sub OpenLayout(ByVal layoutRecordNameOrGuid As String)
            Try
                Dim cs As CPCSClass = cp.CSNew
                accum = ""
                If layoutRecordNameOrGuid <> "" Then
                    Call cs.Open("layouts", "(name=" & cp.Db.EncodeSQLText(layoutRecordNameOrGuid) & ")or(ccGuid=" & cp.Db.EncodeSQLText(layoutRecordNameOrGuid) & ")", "id", , "layout")
                    If cs.Ok Then
                        accum = cs.GetText("layout")
                    End If
                    Call cs.Close()
                End If
            Catch ex As Exception
                Call cpCore.handleException(ex) : Throw
            End Try
        End Sub
        '
        '====================================================================================================
        '
        Public Overrides Sub Prepend(ByVal htmlString As String)
            Try
                accum = htmlString & accum
            Catch ex As Exception
                Call cpCore.handleException(ex) : Throw
            End Try
        End Sub
        '
        '====================================================================================================
        '
        Public Overrides Sub SetInner(ByVal findSelector As String, ByVal htmlString As String)
            Try
                accum = htmlDoc.insertInnerHTML(cpCore, accum, findSelector, htmlString)
            Catch ex As Exception
                Call cpCore.handleException(ex) : Throw
            End Try
        End Sub
        '
        '====================================================================================================
        '
        Public Overrides Sub SetOuter(ByVal findSelector As String, ByVal htmlString As String)
            Try
                accum = htmlDoc.insertOuterHTML(cpCore, accum, findSelector, htmlString)
            Catch ex As Exception
                Call cpCore.handleException(ex) : Throw
            End Try
        End Sub
        '
        '====================================================================================================
        Protected Overridable Overloads Sub Dispose(ByVal disposing As Boolean)
            If Not Me.disposed Then
                If disposing Then
                    '
                    ' dispose managed objects, dereference local object pointers 
                    '
                    htmlDoc = Nothing
                    cp = Nothing
                    cpCore = Nothing
                End If
                '
                ' Add code here to release the unmanaged resource.
                '
            End If
            Me.disposed = True
        End Sub
        '
        ' Dispose Support
        '
#Region " IDisposable Support "
        ' Do not change or add Overridable to these methods.
        ' Put cleanup code in Dispose(ByVal disposing As Boolean).
        Public Overloads Sub Dispose() Implements IDisposable.Dispose
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub
        Protected Overrides Sub Finalize()
            Dispose(False)
            MyBase.Finalize()
        End Sub
#End Region
        '
    End Class
End Namespace