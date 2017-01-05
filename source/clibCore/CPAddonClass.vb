
Option Explicit On
Option Strict On

Imports System.Runtime.InteropServices

Namespace Contensive.Core
    '
    ' comVisible to be activeScript compatible
    '
    <ComVisible(True)> _
    <ComClass(CPAddonClass.ClassId, CPAddonClass.InterfaceId, CPAddonClass.EventsId)> _
    Public Class CPAddonClass
        Inherits BaseClasses.CPAddonBaseClass
        Implements IDisposable
        '
#Region "COM GUIDs"
        Public Const ClassId As String = "6F43E5CA-6367-475C-AE65-FC988234922A"
        Public Const InterfaceId As String = "440D19E3-47A9-4CA2-B20C-077221015525"
        Public Const EventsId As String = "70B800AA-148A-4338-9EDB-70C85E1ADBDD"
#End Region
        '
        Private cp As CPClass
        Private Property UpgradeOK As Boolean
        '
        Friend Sub New(ByVal cp As CPClass)
            MyBase.New()
            Me.cp = cp
        End Sub
        '
        ' dispose
        '
        Protected Overridable Overloads Sub Dispose(ByVal disposing As Boolean)
            If Not Me.disposed Then
                Call appendDebugLog(".dispose, dereference main, csv")
                If disposing Then
                    '
                    ' call .dispose for managed objects
                    '
                    cp = Nothing
                End If
                '
                ' Add code here to release the unmanaged resource.
                '
            End If
            Me.disposed = True
        End Sub
        Protected disposed As Boolean = False
        '
        '
        '
        Public Overrides ReadOnly Property Admin() As Boolean
            Get
                Return False
            End Get
        End Property

        Public Overrides ReadOnly Property ArgumentList() As String
            Get
                Return ""
            End Get
        End Property

        Public Overrides ReadOnly Property AsAjax() As Boolean
            Get
                Return False
            End Get
        End Property

        Public Overrides ReadOnly Property BlockDefaultStyles() As String
            Get
                Return ""

            End Get
        End Property

        Public Overrides ReadOnly Property ccGuid() As String
            Get
                Return ""

            End Get
        End Property

        Public Overrides ReadOnly Property CollectionID() As Integer
            Get
                Return 0
            End Get
        End Property

        Public Overrides ReadOnly Property Content() As Boolean
            Get
                Return False
            End Get
        End Property

        Public Overrides ReadOnly Property Copy() As String
            Get
                Return ""

            End Get
        End Property

        Public Overrides ReadOnly Property CopyText() As String
            Get
                Return ""

            End Get
        End Property

        Public Overrides ReadOnly Property CustomStyles() As String
            Get
                Return ""

            End Get
        End Property

        Public Overrides ReadOnly Property DefaultStyles() As String
            Get
                Return ""

            End Get
        End Property

        Public Overrides ReadOnly Property Description() As String
            Get
                Return ""

            End Get
        End Property

        Public Overrides ReadOnly Property DotNetClass() As String
            Get
                Return ""

            End Get
        End Property

        Public Overrides ReadOnly Property FormXML() As String
            Get
                Return ""

            End Get
        End Property

        Public Overrides ReadOnly Property Help() As String
            Get
                Return ""

            End Get
        End Property

        Public Overrides ReadOnly Property HelpLink() As String
            Get
                Return ""

            End Get
        End Property

        Public Overrides ReadOnly Property IconFilename() As String
            Get
                Return ""

            End Get
        End Property

        Public Overrides ReadOnly Property IconHeight() As Integer
            Get
                Return 0
            End Get
        End Property

        Public Overrides ReadOnly Property IconSprites() As Integer
            Get
                Return 0

            End Get
        End Property

        Public Overrides ReadOnly Property IconWidth() As Integer
            Get
                Return 0

            End Get
        End Property

        Public Overrides ReadOnly Property ID() As Integer
            Get
                Return 0

            End Get
        End Property

        Public Overrides ReadOnly Property InFrame() As Boolean
            Get

                Return False
            End Get
        End Property

        Public Overrides ReadOnly Property IsInline() As Boolean
            Get
                Return False

            End Get
        End Property

        Public Overrides ReadOnly Property JavaScriptBodyEnd() As String
            Get

                Return ""
            End Get
        End Property

        Public Overrides ReadOnly Property JavascriptInHead() As String
            Get
                Return ""

            End Get
        End Property

        Public Overrides ReadOnly Property JavaScriptOnLoad() As String
            Get
                Return ""

            End Get
        End Property

        Public Overrides ReadOnly Property Link() As String
            Get
                Return ""

            End Get
        End Property

        Public Overrides ReadOnly Property MetaDescription() As String
            Get
                Return ""

            End Get
        End Property

        Public Overrides ReadOnly Property MetaKeywordList() As String
            Get

                Return ""
            End Get
        End Property

        Public Overrides ReadOnly Property Name() As String
            Get
                Return ""

            End Get
        End Property

        Public Overrides ReadOnly Property NavIconType() As String
            Get
                Return ""
            End Get
        End Property

        Public Overrides ReadOnly Property ObjectProgramID() As String
            Get
                Return ""

            End Get
        End Property

        Public Overrides ReadOnly Property OnBodyEnd() As Boolean
            Get
                Return False

            End Get
        End Property

        Public Overrides ReadOnly Property OnBodyStart() As Boolean
            Get
                Return False

            End Get
        End Property

        Public Overrides ReadOnly Property OnContentEnd() As Boolean
            Get
                Return False

            End Get
        End Property

        Public Overrides ReadOnly Property OnContentStart() As Boolean
            Get
                Return False

            End Get
        End Property

        Public Overrides Function Open(ByVal AddonId As Integer) As Boolean
            Return False

        End Function

        Public Overrides Function Open(ByVal AddonNameOrGuid As String) As Boolean

            Return False
        End Function

        Public Overrides ReadOnly Property OtherHeadTags() As String
            Get
                Return ""

            End Get
        End Property

        Public Overrides ReadOnly Property PageTitle() As String
            Get
                Return ""

            End Get
        End Property

        Public Overrides ReadOnly Property ProcessInterval() As String
            Get
                Return ""

            End Get
        End Property

        Public Overrides ReadOnly Property ProcessNextRun() As Date
            Get
                Return #12:00:00 AM#
            End Get
        End Property

        Public Overrides ReadOnly Property ProcessRunOnce() As Boolean
            Get
                Return False
            End Get
        End Property

        Public Overrides ReadOnly Property RemoteAssetLink() As String
            Get
                Return ""

            End Get
        End Property

        Public Overrides ReadOnly Property RemoteMethod() As Boolean
            Get
                Return False
            End Get
        End Property

        Public Overrides ReadOnly Property RobotsTxt() As String
            Get
                Return ""

            End Get
        End Property

        Public Overrides ReadOnly Property ScriptCode() As String
            Get

                Return ""
            End Get
        End Property

        Public Overrides ReadOnly Property ScriptEntryPoint() As String 'Inherits BaseClasses.CPAddonBaseClass.ScriptEntryPoint
            Get

                Return ""
            End Get
        End Property

        Public Overrides ReadOnly Property ScriptLanguage() As String 'Inherits BaseClasses.CPAddonBaseClass.ScriptLanguage
            Get
                Return ""

            End Get
        End Property

        Public Overrides ReadOnly Property SharedStyles() As String 'Inherits BaseClasses.CPAddonBaseClass.SharedStyles
            Get
                Return ""

            End Get
        End Property

        Public Overrides ReadOnly Property Template() As Boolean 'Inherits BaseClasses.CPAddonBaseClass.Template
            Get
                Return False
            End Get
        End Property
        '==========================================================================================
        ''' <summary>
        ''' Install an uploaded collection file from a private folder. Return true if successful, else the issue is in the returnUserError
        ''' </summary>
        ''' <param name="privateFolder"></param>
        ''' <param name="returnUserError"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function installCollectionFile(privateFolder As String, ByRef returnUserError As String) As Boolean
            Dim returnOk As Boolean = False
            Try
                returnOk = cp.core.addonInstall_installCollectionFile(privateFolder, returnUserError)
            Catch ex As Exception
                cp.core.handleExceptionAndRethrow(ex)
                If Not cp.core.siteProperties.trapErrors Then
                    Throw New ApplicationException("rethrow", ex)
                End If
            End Try
            Return returnOk
        End Function
        '
        Public Overrides Function installCollectionFromLibrary(collectionGuid As String, ByRef returnUserError As String) As Boolean
            Return False
        End Function
        '
        ' append to logfile
        '
        Private Sub appendDebugLog(ByVal copy As String)
            'My.Computer.FileSystem.WriteAllText("c:\clibCpDebug.log", Now & " - cp.addon, " & copy & vbCrLf, True)
            ' 'My.Computer.FileSystem.WriteAllText(System.AppDocmc.main_CurrentDocmc.main_BaseDirectory() & "cpLog.txt", Now & " - " & copy & vbCrLf, True)
        End Sub
        '
        ' testpoint
        '
        Private Sub tp(ByVal msg As String)
            'Call appendDebugLog(msg)
        End Sub
#Region " IDisposable Support "
        ' Do not change or add Overridable to these methods.
        ' Put cleanup code in Dispose(ByVal disposing As Boolean).
        Public Overloads Sub Dispose() Implements IDisposable.Dispose
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub
        Protected Overrides Sub Finalize()
            Dispose(False)
            MyBase.Finalize()
        End Sub
#End Region
    End Class
End Namespace