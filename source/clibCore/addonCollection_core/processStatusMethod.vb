
Option Explicit On
Option Strict On

Imports Contensive.BaseClasses
Imports Contensive.Core
Imports Contensive.Core.Models.Entity
Imports Contensive.Core.Controllers
Imports Contensive.Core.Controllers.genericController
'
Namespace Contensive.Addons.Core
    Public Class processStatusMethodClass
        Inherits Contensive.BaseClasses.AddonBaseClass
        '
        '====================================================================================================
        ''' <summary>
        ''' getFieldEditorPreference remote method
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <returns></returns>
        Public Overrides Function execute(cp As Contensive.BaseClasses.CPBaseClass) As Object
            Dim result As String = ""
            Try
                Dim processor As CPClass = DirectCast(cp, CPClass)
                Dim cpCore As coreClass = processor.core
                '
                ' test default data connection
                '
                Dim InsertTestOK As Boolean = False
                Dim TrapID As Integer = 0
                Dim CS As Integer = cpCore.db.csInsertRecord("Trap Log")
                If Not cpCore.db.csOk(CS) Then
                    Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError10(ignoreInteger, "dll", "Error during Status. Called InsertCSRecord to insert 'Trap Log' test, record set was not OK.", "Init", False, True)
                Else
                    InsertTestOK = True
                    TrapID = cpCore.db.csGetInteger(CS, "ID")
                End If
                Call cpCore.db.csClose(CS)
                If InsertTestOK Then
                    If TrapID = 0 Then
                        Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError10(ignoreInteger, "dll", "Error during Status. Called InsertCSRecord to insert 'Trap Log' test, record set was OK, but ID=0.", "Init", False, True)
                    Else
                        Call cpCore.db.deleteContentRecord("Trap Log", TrapID)
                    End If
                End If
                If Err.Number <> 0 Then
                    Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError10(ignoreInteger, "dll", "Error during Status. After traplog insert, " & genericController.GetErrString(Err), "Init", False, True)
                    Err.Clear()
                End If
                '
                ' Close page
                '
                Call cpCore.html.main_ClearStream()
                If cpCore.errorCount = 0 Then
                    result = "Contensive OK"
                Else
                    result = "Contensive Error Count = " & cpCore.errorCount
                End If
                result = cpCore.html.getHtmlDoc_beforeEndOfBodyHtml(False, False, False, False)
                cpCore.continueProcessing = False
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try
            Return result
        End Function
    End Class
End Namespace
