
Option Explicit On
Option Strict On
'

Imports System.Xml
Imports Contensive.Core
Imports Contensive.BaseClasses
'
Namespace Contensive.Addons
    '
    Public Class addon_toolConfigureEditClass
        Inherits Contensive.BaseClasses.AddonBaseClass
        '
        '====================================================================================================
        ''' <summary>
        ''' addon method, deliver complete Html admin site
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <returns></returns>
        Public Overrides Function execute(cp As CPBaseClass) As Object
            Dim result As String = ""
            Try
                'result = GetForm_ConfigureEdit(cp)
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try
            Return result
        End Function
    End Class
End Namespace

