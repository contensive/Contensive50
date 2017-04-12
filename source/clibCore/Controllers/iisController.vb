
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
                cpCore.handleExceptionAndRethrow(ex, "verifySite")
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
                cpCore.handleExceptionAndRethrow(ex, "verifyAppPool")
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
                cpCore.handleExceptionAndRethrow(ex, "verifyWebsite")
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
                cpCore.handleExceptionAndRethrow(ex, "verifyWebsite_Binding")
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
                cpCore.handleExceptionAndRethrow(ex, "verifyWebsite_VirtualDirectory")
            End Try
        End Sub
    End Class
End Namespace
