
Option Explicit On
Option Strict On

Imports System.Data.SqlClient

Namespace Contensive.Core
    '
    '====================================================================================================
    ''' <summary>
    ''' cluster srervices - properties and methods to maintain the cluster. Applications do not have access to this. 
    ''' </summary>
    ''' <remarks></remarks>
    Public Class coreClusterClass
        Implements IDisposable
        '
        ' ----- objects passed in constructor, do not dispose
        '
        Private cpCore As coreClass
        '
        '========================================================================
        ''' <summary>
        ''' Constructor builds data. read from cache and deserialize, if not in cache, build it from scratch, eventually, setup public properties as indivisual lazyCache
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <remarks></remarks>
        Public Sub New(cpCore As coreClass)
            '
            ' called during core constructor - so cp.core is not valid
            '
            MyBase.New()
            Me.cpCore = cpCore
            Try
                _ok = True
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
        End Sub
        '
        '===================================================================================================
        ''' <summary>
        ''' file object pointed to the cluster folder in the serverconfig file. Used initially to boot the cluster.
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property localClusterFiles As coreFileSystemClass
            Get
                If (_localClusterFiles Is Nothing) Then
                    _localClusterFiles = New coreFileSystemClass(cpCore, cpCore.clusterConfig.isLocal, coreFileSystemClass.fileSyncModeEnum.activeSync, cpCore.serverConfig.clusterPath)
                End If
                Return _localClusterFiles
            End Get
        End Property
        Private _localClusterFiles As coreFileSystemClass
        '
        '====================================================================================================
        ''' <summary>
        ''' ok - means the class has initialized and methods can be used to maintain the cluser
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property ok As Boolean
            Get
                Return _ok
            End Get
        End Property
        Private _ok As Boolean = False

        '
        '====================================================================================================
        ''' <summary>
        ''' save config changes to the clusterConfig.json file
        ''' </summary>
        Public Sub saveConfig()
            Dim jsonTemp As String = cpCore.json.Serialize(cpCore.clusterConfig)
            localClusterFiles.saveFile("clusterConfig.json", jsonTemp)
        End Sub
        '
        '====================================================================================================
#Region " IDisposable Support "
        '
        ' this class must implement System.IDisposable
        ' never throw an exception in dispose
        ' Do not change or add Overridable to these methods.
        ' Put cleanup code in Dispose(ByVal disposing As Boolean).
        '====================================================================================================
        '
        Protected disposed As Boolean = False
        '
        Public Overloads Sub Dispose() Implements IDisposable.Dispose
            ' do not add code here. Use the Dispose(disposing) overload
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub
        '
        Protected Overrides Sub Finalize()
            ' do not add code here. Use the Dispose(disposing) overload
            Dispose(False)
            MyBase.Finalize()
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' dispose.
        ''' </summary>
        ''' <param name="disposing"></param>
        Protected Overridable Overloads Sub Dispose(ByVal disposing As Boolean)
            If Not Me.disposed Then
                Me.disposed = True
                If disposing Then
                    '
                    ' call .dispose for managed objects
                    '
                    If Not (_localClusterFiles Is Nothing) Then _localClusterFiles.Dispose()
                End If
                '
                ' cleanup non-managed objects
                '
            End If
        End Sub
#End Region
    End Class
End Namespace
