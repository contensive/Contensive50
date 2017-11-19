
Option Explicit On
Option Strict On

Imports Contensive.BaseClasses
Imports Contensive.Core
Imports Contensive.Core.Models.Entity
Imports Contensive.Core.Controllers
Imports Contensive.Core.Controllers.genericController
'
Namespace Contensive.Addons.Core
    Public Class processPayPalConformMethodClass
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
                ' -- Should be a remote method in commerce
                Dim ConfirmOrderID As Integer = cpCore.docProperties.getInteger("item_name")
                If ConfirmOrderID <> 0 Then
                    '
                    ' Confirm the order
                    '
                    Dim CS As Integer = cpCore.db.csOpen("Orders", "(ID=" & ConfirmOrderID & ") and ((OrderCompleted=0)or(OrderCompleted is Null))")
                    If cpCore.db.csOk(CS) Then
                        Call cpCore.db.csSet(CS, "OrderCompleted", True)
                        Call cpCore.db.csSet(CS, "DateCompleted", cpCore.doc.profileStartTime)
                        Call cpCore.db.csSet(CS, "ccAuthCode", cpCore.docProperties.getText("txn_id"))
                        Call cpCore.db.csSet(CS, "ccActionCode", cpCore.docProperties.getText("payment_status"))
                        Call cpCore.db.csSet(CS, "ccRefCode", cpCore.docProperties.getText("pending_reason"))
                        Call cpCore.db.csSet(CS, "PayMethod", "PayPal " & cpCore.docProperties.getText("payment_type"))
                        Call cpCore.db.csSet(CS, "ShipName", cpCore.docProperties.getText("first_name") & " " & cpCore.docProperties.getText("last_name"))
                        Call cpCore.db.csSet(CS, "ShipAddress", cpCore.docProperties.getText("address_street"))
                        Call cpCore.db.csSet(CS, "ShipCity", cpCore.docProperties.getText("address_city"))
                        Call cpCore.db.csSet(CS, "ShipState", cpCore.docProperties.getText("address_state"))
                        Call cpCore.db.csSet(CS, "ShipZip", cpCore.docProperties.getText("address_zip"))
                        Call cpCore.db.csSet(CS, "BilleMail", cpCore.docProperties.getText("payer_email"))
                        Call cpCore.db.csSet(CS, "ContentControlID", cpCore.metaData.getContentId("Orders Completed"))
                        Call cpCore.db.csSave2(CS)
                    End If
                    Call cpCore.db.csClose(CS)
                    '
                    ' Empty the cart
                    '
                    CS = cpCore.db.csOpen("Visitors", "OrderID=" & ConfirmOrderID)
                    If cpCore.db.csOk(CS) Then
                        Call cpCore.db.csSet(CS, "OrderID", 0)
                        Call cpCore.db.csSave2(CS)
                    End If
                    Call cpCore.db.csClose(CS)
                    '
                    ' TEmp fix until HardCodedPage is complete
                    '
                    Dim Recipient As String = cpCore.siteProperties.getText("EmailOrderNotifyAddress", cpCore.siteProperties.emailAdmin)
                    If genericController.vbInstr(genericController.encodeText(Recipient), "@") = 0 Then
                        Throw New ApplicationException("Unexpected exception") ' todo - remove this - handleLegacyError12("Init", "PayPal confirmation Order Process Notification email was not sent because EmailOrderNotifyAddress SiteProperty is not valid")
                    Else
                        Dim Sender As String = cpCore.siteProperties.getText("EmailOrderFromAddress")
                        Dim subject As String = cpCore.webServer.requestDomain & " Online Order Pending, #" & ConfirmOrderID
                        Dim Message As String = "<p>An order confirmation has been recieved from PayPal for " & cpCore.webServer.requestDomain & "</p>"
                        Call cpCore.email.send_Legacy(Recipient, Sender, subject, Message, , False, True)
                    End If
                End If
                cpCore.doc.continueProcessing = False
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try
            Return result
        End Function
    End Class
End Namespace
