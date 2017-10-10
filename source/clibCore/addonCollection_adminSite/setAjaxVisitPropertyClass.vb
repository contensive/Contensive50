
Option Explicit On
Option Strict On

Imports Contensive.BaseClasses
Imports Contensive.Core
Imports Contensive.Core.Models.Entity
Imports Contensive.Core.Controllers
Imports Contensive.Core.Controllers.genericController
'
Namespace Contensive.Addons.AdminSite
    '
    Public Class setAjaxVisitPropertyClass
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

                Dim ArgList As String = cpCore.docProperties.getText("args")
                Dim Args As String() = Split(ArgList, "&")
                Dim gd As GoogleDataType = New GoogleDataType
                gd.IsEmpty = True
                For Ptr = 0 To UBound(Args)
                    Dim ArgNameValue As String() = Split(Args(Ptr), "=")
                    Dim PropertyName As String = ArgNameValue(0)
                    Dim PropertyValue As String = ""
                    If UBound(ArgNameValue) > 0 Then
                        PropertyValue = ArgNameValue(1)
                    End If
                    Call cpCore.visitProperty.setProperty(PropertyName, PropertyValue)
                Next
                result = remoteQueryController.main_FormatRemoteQueryOutput(cpCore, gd, RemoteFormatEnum.RemoteFormatJsonNameValue)
                result = cpCore.html.main_encodeHTML(result)
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try
            Return result
        End Function
    End Class
End Namespace
