
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
        Private cpCore As Contensive.Core.coreClass
        Private cp As CPClass
        Private accum As String
        Private htmlTools As coreHtmlClass
        Protected disposed As Boolean = False
        '
        ' Constructor - Initialize the Main and Csv objects
        '
        Public Sub New(ByRef cpParent As CPClass)
            MyBase.New()
            Try
                accum = ""
                cp = cpParent
                cpCore = cp.core
                Try
                    htmlTools = New coreHtmlClass(cpCore)
                Catch ex As Exception
                    cp.core.handleExceptionAndRethrow(ex, "Error creating object aoPrimitives.HtmlToolsClass during cp.block constructor.")
                End Try
            Catch ex As Exception
                cp.core.handleExceptionAndRethrow(ex, "Unexpected Error creating cp.block Object")
            End Try
        End Sub
        '
        ' dispose
        '
        Protected Overridable Overloads Sub Dispose(ByVal disposing As Boolean)
            If Not Me.disposed Then
                If disposing Then
                    '
                    ' dispose managed objects, dereference local object pointers 
                    '
                    htmlTools = Nothing
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
        ' testpoint
        '
        Private Sub tp(ByVal msg As String)
            'My.Computer.FileSystem.WriteAllText("c:\clibCpDebug.log", Now & " - cp.block, " & msg & vbCrLf, True)
        End Sub
        '
        '
        '
        Public Overrides Sub Load(ByVal htmlString As String)
            Try
                accum = htmlString
                'tp("Load,accum=" & accum)
            Catch ex As Exception
                cp.core.handleExceptionAndRethrow(ex, "Unexpected Error in block.Load")
            End Try
        End Sub
        '
        '
        '
        Public Overrides Sub Append(ByVal htmlString As String)
            Try
                accum &= htmlString
            Catch ex As Exception
                cp.core.handleExceptionAndRethrow(ex, "Unexpected Error in block.Append")
            End Try
        End Sub
        '
        '
        '
        Public Overrides Sub Clear()
            Try
                accum = ""
            Catch ex As Exception
                cp.core.handleExceptionAndRethrow(ex, "Unexpected Error in block.Clear")
            End Try
        End Sub
        '
        '
        '
        Public Overrides Function GetHtml() As String
            Dim s As String = ""
            Try
                'tp("getHtml,accum=" & accum)
                s = accum
            Catch ex As Exception
                cp.core.handleExceptionAndRethrow(ex, "Unexpected Error in block.GetHtml")
            End Try
            Return s
        End Function
        '
        '
        '
        Public Overrides Function GetInner(ByVal findSelector As String) As String
            Dim s As String = ""
            Try
                Dim a As String = accum
                If findSelector <> "" Then
                    s = htmlTools.getInnerHTML(cpCore, a, findSelector)
                End If
            Catch ex As Exception
                cp.core.handleExceptionAndRethrow(ex, "Unexpected Error in block.GetInner")
            End Try
            Return s
        End Function
        '
        '
        '
        Public Overrides Function GetOuter(ByVal findSelector As String) As String
            Dim s As String = ""
            Try
                Dim a As String = accum
                If findSelector <> "" Then
                    s = htmlTools.getOuterHTML(cpCore, a, findSelector)
                End If
            Catch ex As Exception
                cp.core.handleExceptionAndRethrow(ex, "Unexpected Error in block.GetOuter")
            End Try
            Return s
        End Function
        '
        '
        '
        Public Overrides Sub ImportFile(ByVal wwwFileName As String)
            Dim headTags As String = ""
            Try
                If wwwFileName <> "" Then
                    accum = cp.wwwFiles.read(wwwFileName)
                    If accum <> "" Then
                        headTags = coreCommonModule.GetTagInnerHTML(accum, "head", False)
                        If headTags <> "" Then
                            Call cpCore.html_addHeadTags(headTags)
                        End If
                        accum = coreCommonModule.GetTagInnerHTML(accum, "body", False)
                    End If
                End If
            Catch ex As Exception
                cp.core.handleExceptionAndRethrow(ex, "Unexpected Error in block.ImportFile")
            End Try
        End Sub
        '
        '
        '
        Public Overrides Sub OpenCopy(ByVal copyRecordNameOrGuid As String)
            Dim cs As CPCSClass = cp.CSNew
            Try
                accum = ""
                If copyRecordNameOrGuid <> "" Then
                    Call cs.Open("copy content", "(name=" & cp.Db.EncodeSQLText(copyRecordNameOrGuid) & ")or(ccGuid=" & cp.Db.EncodeSQLText(copyRecordNameOrGuid) & ")", "id", , "copy")
                    If cs.OK Then
                        accum = cs.GetText("copy")
                    End If
                    Call cs.Close()
                End If
            Catch ex As Exception
                cp.core.handleExceptionAndRethrow(ex, "Unexpected Error in block.OpenCopy")
            End Try
        End Sub
        '
        '
        '
        Public Overrides Sub OpenFile(ByVal wwwFileName As String)
            Try
                accum = ""
                If (Not String.IsNullOrEmpty(wwwFileName)) Then
                    accum = cp.wwwFiles.read(wwwFileName)
                End If
            Catch ex As Exception
                cp.core.handleExceptionAndRethrow(ex, "Unexpected Error in block.OpenFile")
            End Try
        End Sub
        '
        '
        '
        Public Overrides Sub OpenLayout(ByVal layoutRecordNameOrGuid As String)
            Try
                Dim cs As CPCSClass = cp.CSNew
                accum = ""
                If layoutRecordNameOrGuid <> "" Then
                    Call cs.Open("layouts", "(name=" & cp.Db.EncodeSQLText(layoutRecordNameOrGuid) & ")or(ccGuid=" & cp.Db.EncodeSQLText(layoutRecordNameOrGuid) & ")", "id", , "layout")
                    If cs.OK Then
                        accum = cs.GetText("layout")
                    End If
                    Call cs.Close()
                End If
            Catch ex As Exception
                cp.core.handleExceptionAndRethrow(ex, "Unexpected Error in block.OpenLayout")
            End Try
        End Sub
        '
        '
        '
        Public Overrides Sub Prepend(ByVal htmlString As String)
            Try
                accum = htmlString & accum
            Catch ex As Exception
                cp.core.handleExceptionAndRethrow(ex, "Unexpected Error in block.Prepend")
            End Try
        End Sub
        '
        '
        '
        Public Overrides Sub SetInner(ByVal findSelector As String, ByVal htmlString As String)
            Try
                accum = htmlTools.insertInnerHTML(cpCore, accum, findSelector, htmlString)
            Catch ex As Exception
                cp.core.handleExceptionAndRethrow(ex, "Unexpected Error in block.SetInner")
            End Try
        End Sub
        '
        '
        '
        Public Overrides Sub SetOuter(ByVal findSelector As String, ByVal htmlString As String)
            Try
                accum = htmlTools.insertOuterHTML(cpCore, accum, findSelector, htmlString)
            Catch ex As Exception
                cp.core.handleExceptionAndRethrow(ex, "Unexpected Error in block.SetOuter")
            End Try
        End Sub
    End Class
End Namespace