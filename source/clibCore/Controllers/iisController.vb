
Option Explicit On
Option Strict On

Imports System.Xml
Imports Microsoft.Web.Administration
'
Namespace Contensive.Core.Controllers
    '
    '====================================================================================================
    ''' <summary>
    ''' code to built and upgrade apps
    ''' not IDisposable - not contained classes that need to be disposed
    ''' </summary>
    Public Class iisController
        '
        '====================================================================================================
        '
        Private Structure fieldTypePrivate
            Dim Name As String
            Dim fieldTypePrivate As Integer
        End Structure
        '
        '====================================================================================================
        ''' <summary>
        ''' Verify a site exists, it not add it, it is does, verify all its settings
        ''' </summary>
        ''' <param name="cpCore"></param>
        ''' <param name="appName"></param>
        ''' <param name="DomainName"></param>
        ''' <param name="rootPublicFilesPath"></param>
        ''' <param name="defaultDocOrBlank"></param>
        ''' '
        Public Shared Sub verifySite(cpCore As coreClass, ByVal appName As String, ByVal DomainName As String, ByVal rootPublicFilesPath As String, ByVal defaultDocOrBlank As String)
            Try
                verifyAppPool(cpCore, appName)
                verifyWebsite(cpCore, appName, DomainName, rootPublicFilesPath, appName)
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex, "verifySite")
            End Try
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' verify the application pool. If it exists, update it. If not, create it
        ''' </summary>
        ''' <param name="cpCore"></param>
        ''' <param name="poolName"></param>
        Private Shared Sub verifyAppPool(cpCore As coreClass, poolName As String)
            Try
                Using serverManager As ServerManager = New ServerManager()
                    Dim poolFound As Boolean = False
                    Dim appPool As ApplicationPool
                    For Each appPool In serverManager.ApplicationPools
                        If (appPool.Name = poolName) Then
                            poolFound = True
                            Exit For
                        End If
                    Next
                    If Not poolFound Then
                        appPool = serverManager.ApplicationPools.Add(poolName)
                    Else
                        appPool = serverManager.ApplicationPools(poolName)
                    End If
                    appPool.ManagedRuntimeVersion = "v4.0"
                    appPool.Enable32BitAppOnWin64 = True
                    appPool.ManagedPipelineMode = ManagedPipelineMode.Integrated
                    serverManager.CommitChanges()
                End Using
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex, "verifyAppPool")
            End Try
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' verify the website. If it exists, update it. If not, create it
        ''' </summary>
        ''' <param name="cpCore"></param>
        ''' <param name="appName"></param>
        ''' <param name="domainName"></param>
        ''' <param name="phyPath"></param>
        ''' <param name="appPool"></param>
        Private Shared Sub verifyWebsite(cpCore As coreClass, appName As String, domainName As String, phyPath As String, appPool As String)
            Try

                Using iisManager As ServerManager = New ServerManager()
                    Dim site As Site
                    Dim found As Boolean = False
                    '
                    ' -- verify the site exists
                    For Each site In iisManager.Sites
                        If site.Name.ToLower() = appName.ToLower() Then
                            found = True
                            Exit For
                        End If
                    Next
                    If Not found Then
                        iisManager.Sites.Add(appName, "http", "*:80:" & appName, phyPath)
                    End If
                    site = iisManager.Sites(appName)
                    '
                    ' -- verify the bindings
                    verifyWebsite_Binding(cpCore, site, "*:80:" & appName, "http")
                    verifyWebsite_Binding(cpCore, site, "*:80:" & domainName, "http")
                    '
                    ' -- verify the application pool
                    site.ApplicationDefaults.ApplicationPoolName = appPool
                    For Each iisApp As Application In site.Applications
                        iisApp.ApplicationPoolName = appPool
                    Next
                    '
                    ' -- verify the cdn virtual directory (if configured)
                    Dim cdnFilesPrefix As String = cpCore.serverConfig.appConfig.cdnFilesNetprefix
                    If (cdnFilesPrefix.IndexOf("://") < 0) Then
                        verifyWebsite_VirtualDirectory(cpCore, site, appName, cdnFilesPrefix, cpCore.serverConfig.appConfig.cdnFilesPath)
                    End If
                    '
                    ' -- commit any changes
                    iisManager.CommitChanges()
                End Using
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex, "verifyWebsite")
            End Try
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' Verify the binding
        ''' </summary>
        ''' <param name="cpCore"></param>
        ''' <param name="site"></param>
        ''' <param name="bindingInformation"></param>
        ''' <param name="bindingProtocol"></param>
        Private Shared Sub verifyWebsite_Binding(cpCore As coreClass, site As Site, bindingInformation As String, bindingProtocol As String)
            Try
                Using iisManager As ServerManager = New ServerManager()
                    Dim binding As Binding
                    Dim found As Boolean = False
                    found = False
                    For Each binding In site.Bindings
                        If (binding.BindingInformation = bindingInformation) And (binding.Protocol = bindingProtocol) Then
                            found = True
                            Exit For
                        End If
                    Next
                    If Not found Then
                        binding = site.Bindings.CreateElement()
                        binding.BindingInformation = bindingInformation
                        binding.Protocol = bindingProtocol
                        site.Bindings.Add(binding)
                        iisManager.CommitChanges()
                    End If
                End Using
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex, "verifyWebsite_Binding")
            End Try
        End Sub
        '
        '====================================================================================================
        '
        Private Shared Sub verifyWebsite_VirtualDirectory(cpCore As coreClass, site As Site, appName As String, virtualFolder As String, physicalPath As String)
            Try
                Dim found As Boolean = False
                For Each iisApp As Application In site.Applications
                    If iisApp.ApplicationPoolName.ToLower() = appName.ToLower() Then
                        For Each virtualDirectory As VirtualDirectory In iisApp.VirtualDirectories
                            If virtualDirectory.Path = virtualFolder Then
                                found = True
                                Exit For
                            End If
                        Next
                        If Not found Then
                            Dim vpList As List(Of String) = virtualFolder.Split("\"c).ToList
                            Dim newDirectoryPath As String = ""

                            For Each newDirectoryFolderName As String In vpList
                                If (Not String.IsNullOrEmpty(newDirectoryFolderName)) Then
                                    newDirectoryPath &= "/" & newDirectoryFolderName
                                    Dim directoryFound As Boolean = False
                                    For Each currentDirectory As VirtualDirectory In iisApp.VirtualDirectories
                                        If (currentDirectory.Path.ToLower() = newDirectoryPath.ToLower()) Then
                                            directoryFound = True
                                            Exit For
                                        End If
                                    Next
                                    If (Not directoryFound) Then
                                        iisApp.VirtualDirectories.Add(newDirectoryPath, physicalPath)
                                    End If
                                End If
                            Next
                        End If
                    End If
                    If found Then Exit For
                Next
            Catch ex As Exception
                cpCore.handleExceptionAndContinue(ex, "verifyWebsite_VirtualDirectory")
            End Try
        End Sub
        '========================================================================
        ' main_RedirectByRecord( iContentName, iRecordID )
        '   looks up the record
        '   increments the 'clicks' field and redirects to the 'link' field
        '   returns true if the redirect happened OK
        '========================================================================
        '
        Public Shared Function main_RedirectByRecord_ReturnStatus(cpcore As coreClass, ByVal ContentName As String, ByVal RecordID As Integer, Optional ByVal FieldName As String = "") As Boolean
            Dim Link As String
            Dim CSPointer As Integer
            Dim MethodName As String
            Dim ContentID As Integer
            Dim CSHost As Integer
            Dim HostContentName As String
            Dim HostRecordID As Integer
            Dim BlockRedirect As Boolean
            Dim iContentName As String
            Dim iRecordID As Integer
            Dim iFieldName As String
            Dim LinkPrefix As String
            Dim EncodedLink As String
            Dim NonEncodedLink As String = ""
            Dim RecordActive As Boolean
            '
            iContentName = genericController.encodeText(ContentName)
            iRecordID = genericController.EncodeInteger(RecordID)
            iFieldName = genericController.encodeEmptyText(FieldName, "link")
            '
            MethodName = "main_RedirectByRecord_ReturnStatus( " & iContentName & ", " & iRecordID & ", " & genericController.encodeEmptyText(FieldName, "(fieldname empty)") & ")"
            '
            main_RedirectByRecord_ReturnStatus = False
            BlockRedirect = False
            CSPointer = cpcore.db.cs_open(iContentName, "ID=" & iRecordID)
            If cpcore.db.cs_ok(CSPointer) Then
                ' 2/18/2008 - EncodeLink change
                '
                ' Assume all Link fields are already encoded -- as this is how they would appear if the admin cut and pasted
                '
                EncodedLink = Trim(cpcore.db.cs_getText(CSPointer, iFieldName))
                If EncodedLink = "" Then
                    BlockRedirect = True
                Else
                    '
                    ' ----- handle content special cases (prevent redirect to deleted records)
                    '
                    NonEncodedLink = cpcore.htmlDoc.main_DecodeUrl(EncodedLink)
                    Select Case genericController.vbUCase(iContentName)
                        Case "CONTENT WATCH"
                            '
                            ' ----- special case
                            '       if this is a content watch record, check the underlying content for
                            '       inactive or expired before redirecting
                            '
                            LinkPrefix = cpcore.webServer.webServerIO_requestContentWatchPrefix
                            ContentID = (cpcore.db.cs_getInteger(CSPointer, "ContentID"))
                            HostContentName = cpcore.metaData.getContentNameByID(ContentID)
                            If (HostContentName = "") Then
                                '
                                ' ----- Content Watch with a bad ContentID, mark inactive
                                '
                                BlockRedirect = True
                                Call cpcore.db.cs_set(CSPointer, "active", 0)
                            Else
                                HostRecordID = (cpcore.db.cs_getInteger(CSPointer, "RecordID"))
                                If HostRecordID = 0 Then
                                    '
                                    ' ----- Content Watch with a bad iRecordID, mark inactive
                                    '
                                    BlockRedirect = True
                                    Call cpcore.db.cs_set(CSPointer, "active", 0)
                                Else
                                    CSHost = cpcore.db.cs_open(HostContentName, "ID=" & HostRecordID)
                                    If Not cpcore.db.cs_ok(CSHost) Then
                                        '
                                        ' ----- Content Watch host record not found, mark inactive
                                        '
                                        BlockRedirect = True
                                        Call cpcore.db.cs_set(CSPointer, "active", 0)
                                    End If
                                End If
                                Call cpcore.db.cs_Close(CSHost)
                            End If
                            If BlockRedirect Then
                                '
                                ' ----- if a content watch record is blocked, delete the content tracking
                                '
                                Call cpcore.db.deleteContentRules(cpcore.metaData.getContentId(HostContentName), HostRecordID)
                            End If
                    End Select
                End If
                If Not BlockRedirect Then
                    '
                    ' If link incorrectly includes the LinkPrefix, take it off first, then add it back
                    '
                    NonEncodedLink = genericController.ConvertShortLinkToLink(NonEncodedLink, LinkPrefix)
                    If cpcore.db.cs_isFieldSupported(CSPointer, "Clicks") Then
                        Call cpcore.db.cs_set(CSPointer, "Clicks", (cpcore.db.cs_getNumber(CSPointer, "Clicks")) + 1)
                    End If
                    Call cpcore.webServer.redirect(LinkPrefix & NonEncodedLink, "Call to " & MethodName & ", no reason given.", False)
                    main_RedirectByRecord_ReturnStatus = True
                End If
            End If
            Call cpcore.db.cs_Close(CSPointer)
        End Function
    End Class
End Namespace
