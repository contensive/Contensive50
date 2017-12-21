
Option Explicit On
Option Strict On

Imports Contensive.Core.Models
Imports Contensive.Core.Models.Context
Imports Contensive.Core.Models.Entity
Imports Contensive.Core.Controllers

Namespace Contensive.Core.Models.Complex

    '
    '
    ' ----- Table Schema caching to speed up update
    '
    Public Class tableSchemaModel
        Public Property TableName As String
        Public Property Dirty As Boolean
        Public Property columns As List(Of String)
        ' list of all indexes, with the field it covers
        Public Property indexes As List(Of String)
        '
        '=================================================================================
        ' Returns a pointer into the cdefCache.tableSchema() array for the table that matches
        '   returns -1 if the table is not found
        '=================================================================================
        '
        Public Shared Function getTableSchema(cpcore As coreClass, ByVal TableName As String, ByVal DataSourceName As String) As Models.Complex.tableSchemaModel
            Dim tableSchema As Models.Complex.tableSchemaModel = Nothing
            Try
                Dim dt As DataTable
                Dim isInCache As Boolean = False
                Dim isInDb As Boolean = False
                'Dim readFromDb As Boolean
                Dim lowerTablename As String
                Dim buildCache As Boolean
                '
                If (DataSourceName <> "") And (DataSourceName <> "-1") And (DataSourceName.ToLower <> "default") Then
                    Throw New NotImplementedException("alternate datasources not supported yet")
                Else
                    If TableName <> "" Then
                        lowerTablename = TableName.ToLower
                        If (cpcore.doc.tableSchemaDictionary) Is Nothing Then
                            cpcore.doc.tableSchemaDictionary = New Dictionary(Of String, Models.Complex.tableSchemaModel)
                        Else
                            isInCache = cpcore.doc.tableSchemaDictionary.TryGetValue(lowerTablename, tableSchema)
                        End If
                        buildCache = Not isInCache
                        If isInCache Then
                            buildCache = tableSchema.Dirty
                        End If
                        If buildCache Then
                            '
                            ' cache needs to be built
                            '
                            dt = cpcore.db.getTableSchemaData(TableName)
                            If dt.Rows.Count <= 0 Then
                                tableSchema = Nothing
                            Else
                                isInDb = True
                                tableSchema = New Models.Complex.tableSchemaModel
                                tableSchema.columns = New List(Of String)
                                tableSchema.indexes = New List(Of String)
                                tableSchema.TableName = lowerTablename
                                '
                                ' load columns
                                '
                                dt = cpcore.db.getColumnSchemaData(TableName)
                                If dt.Rows.Count > 0 Then
                                    For Each row As DataRow In dt.Rows
                                        tableSchema.columns.Add(genericController.encodeText(row("COLUMN_NAME")).ToLower)
                                    Next
                                End If
                                '
                                ' Load the index schema
                                '
                                dt = cpcore.db.getIndexSchemaData(TableName)
                                If dt.Rows.Count > 0 Then
                                    For Each row As DataRow In dt.Rows
                                        tableSchema.indexes.Add(genericController.encodeText(row("INDEX_NAME")).ToLower)
                                    Next
                                End If
                            End If
                            If Not isInDb And isInCache Then
                                cpcore.doc.tableSchemaDictionary.Remove(lowerTablename)
                            ElseIf isInDb And (Not isInCache) Then
                                cpcore.doc.tableSchemaDictionary.Add(lowerTablename, tableSchema)
                            ElseIf isInDb And isInCache Then
                                cpcore.doc.tableSchemaDictionary(lowerTablename) = tableSchema
                            End If
                        End If
                    End If
                End If
            Catch ex As Exception
                cpcore.handleException(ex) : Throw
            End Try
            Return tableSchema
        End Function
        '
        '====================================================================================================
        Public Shared Sub tableSchemaListClear(cpcore As coreClass)
            cpcore.doc.tableSchemaDictionary.Clear()
        End Sub
    End Class

End Namespace
