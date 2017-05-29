
Option Explicit On
Option Strict On

Imports Contensive.Core.Models
Imports Contensive.Core.Models.Context
Imports Contensive.Core.Models.Entity
Imports Contensive.Core.Controllers

Namespace Contensive.Core.Models.Entity

    '
    '
    ' ----- Table Schema caching to speed up update
    '
    Public Class tableSchemaModel
        Public TableName As String
        Public Dirty As Boolean
        Public columns As List(Of String)
        ' list of all indexes, with the field it covers
        Public indexes As List(Of String)
    End Class

End Namespace
