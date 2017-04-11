
Option Explicit On
Option Strict On

Imports Contensive.BaseClasses

Namespace Contensive.Core
    Public Module constantsModule
        '
        ' code version for this build. This is saved in a site property and checked in the housekeeping event - checkDataVersion
        '
        Public Const codeVersion As Integer = 0
        '
        ' -- content names
        Public Const cnBlank As String = ""
        Public Const cnPeople As String = "people"
        '
        Public Const requestAppRootPath As String = "/"
    End Module
    '
End Namespace
