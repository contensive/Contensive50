
Option Explicit On
Option Strict On

Namespace Contensive.Core.Models.Entity
    '
    '====================================================================================================
    ''' <summary>
    ''' classSummary
    ''' </summary>
    Public Class domainLegacyModel
        '
        Private cpCore As coreClass
        '
        Private domainList_local As New List(Of String)
        Private serverDomainList_localLoaded As Boolean = False
        Public ServerMultiDomainMode As Boolean = False    ' When true, the site can run from any domain name.
        '
        '   values read from the domain record during init
        '
        Public Class domainDetailsClass
            Public name As String
            Public rootPageId As Integer
            Public noFollow As Boolean
            Public typeId As Integer
            Public visited As Boolean
            Public id As Integer
            Public forwardUrl As String
            Public defaultTemplateId As Integer
            Public pageNotFoundPageId As Integer
            Public allowCrossLogin As Boolean
            Public forwardDomainId As Integer
        End Class
        '
        ' information about all the domains for this app
        '
        Public domainDetailsList As Dictionary(Of String, domainDetailsClass)
        Public domainDetails As New domainDetailsClass
        '
        Public Sub New(cpCore As coreClass)
            MyBase.New()
            Me.cpCore = cpCore
        End Sub
        '
        '===========================================================================================
        '
        Public ReadOnly Property getDomainDbList As List(Of String)
            Get
                Dim returnDomainDbList As List(Of String) = Nothing
                Dim SQL As String
                Dim dt As DataTable
                '
                Const cacheName = "Domain Content List Cache"
                '
                Try
                    If Not serverDomainList_localLoaded Then
                        domainList_local = DirectCast(cpCore.cache.getObject(Of List(Of String))(cacheName), List(Of String))
                        If (domainList_local Is Nothing) Then
                            '
                            ' recreate (non-default) domain table list
                            '
                            domainList_local = New List(Of String)
                            domainList_local.Add(cpCore.serverConfig.appConfig.domainList(0))
                            '
                            ' select all Normal domains (non-Forward)
                            '
                            SQL = "select name from ccDomains where typeId=1"
                            dt = cpCore.db.executeSql(SQL)
                            For Each dr As DataRow In dt.Rows
                                domainList_local.Add(dr(0).ToString)
                            Next
                            Call cpCore.cache.setObject(cacheName, domainList_local, "domains")
                            dt.Dispose()
                        End If
                        serverDomainList_localLoaded = True
                    End If
                    returnDomainDbList = domainList_local
                Catch ex As Exception
                    cpCore.handleExceptionAndRethrow(ex)
                End Try
                Return returnDomainDbList
            End Get
        End Property
        '
        '===========================================================================================
        '
    End Class
End Namespace