
Option Explicit On
Option Strict On

Imports Contensive.BaseClasses
Imports Contensive.Core
Imports Contensive.Core.Models.Entity
Imports Contensive.Core.Controllers
Imports Contensive.Core.Controllers.genericController
'
Namespace Contensive.Addons.Core
    Public Class robotsTxtClass
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
                ' -- Robots.txt
                Dim Filename As String = "config/RobotsTxtBase.txt"
                ' 
                ' set this way because the preferences page needs a filename in a site property (enhance later)
                Call cpCore.siteProperties.setProperty("RobotsTxtFilename", Filename)
                result = cpCore.cdnFiles.readFile(Filename)
                If result = "" Then
                    '
                    ' save default robots.txt
                    '
                    result = "User-agent: *" & vbCrLf & "Disallow: /admin/" & vbCrLf & "Disallow: /images/"
                    Call cpCore.appRootFiles.saveFile(Filename, result)
                End If
                result = result & cpCore.addonCache.robotsTxt
                Call cpCore.webServer.setResponseContentType("text/plain")
                cpCore.continueProcessing = False
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try
            Return result
        End Function
    End Class
End Namespace
