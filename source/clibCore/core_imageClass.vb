
Imports System.IO
Imports System.Drawing
Imports System.Runtime.InteropServices
'
Namespace Contensive.Core

    '<ComVisible(True)> _
    '<Guid("F6A726A6-08EE-465B-A5D9-0923656618A1")> _
    '<InterfaceType(ComInterfaceType.InterfaceIsDual)> _
    'Public Interface IimageClass
    '    Function load(ByVal physicalFilePath As String) As Boolean
    'End Interface
    '
    '<ComVisible(True)> _
    '<ProgId("Contensive.Core.imageClass")> _
    '<Guid("A96E0F31-70E2-4F48-82C9-8C4D38CD5BF9")> _
    '<ClassInterface(ClassInterfaceType.None)> _
    Public Class imageClass
        Implements IDisposable
        'Implements IimageClass
        '
        '#Region "COM GUIDs"
        '        ' These  GUIDs provide the COM identity for this class 
        '        ' and its COM interfaces. If you change them, existing 
        '        ' clients will no longer be able to access the class.
        '        Public Const ClassId As String = "A96E0F31-70E2-4F48-82C9-8C4D38CD5BF9"
        '        Public Const InterfaceId As String = "F6A726A6-08EE-465B-A5D9-0923656618A1"
        '        Public Const EventsId As String = "FBAFBFC6-C37E-4EF3-A9D3-FF2C9361A1CC"
        '#End Region
        '
        Private loaded As Boolean = False
        Private src As String = ""
        Private srcImage As System.Drawing.Image
        Private setWidth As Integer = 0
        Private setHeight As Integer = 0
        '
        ' dispose
        '
        Protected disposed As Boolean = False
        Protected Overridable Overloads Sub Dispose(ByVal disposing As Boolean)
            If Not Me.disposed Then
                If disposing Then
                    '
                    ' call .dispose for managed objects
                    '
                    If loaded Then
                        srcImage.Dispose()
                        srcImage = Nothing
                    End If
                End If
                '
                ' Add code here to release the unmanaged resource.
                '
            End If
            Me.disposed = True
        End Sub
        '
        '
        '
        Public Function load(ByVal physicalFilePath As String) As Boolean 'Implements IimageClass.load
            Dim returnOk As Boolean = False
            Try
                If System.IO.File.Exists(physicalFilePath) Then
                    src = physicalFilePath
                    srcImage = System.Drawing.Image.FromFile(src)
                    setWidth = srcImage.Width
                    setHeight = srcImage.Height
                    loaded = True
                End If
            Catch ex As Exception

            End Try
            Return returnOk
        End Function
        '
        '
        '
        Public Function save(ByVal physicalFilePath As String) As Boolean
            Dim returnOk As Boolean = False
            Try
                If loaded Then
                    If src = physicalFilePath Then
                        If System.IO.File.Exists(src) Then
                            System.IO.File.Delete(src)
                        End If
                    End If
                    Dim imgOutput As New Bitmap(srcImage, setWidth, setHeight)
                    Dim imgFormat = srcImage.RawFormat

                    Call imgOutput.Save(physicalFilePath, imgFormat)
                    imgOutput.Dispose()
                    returnOk = True
                End If
            Catch ex As Exception

            End Try
            Return returnOk
        End Function
        '
        '
        '
        Public Property width As Integer
            Get
                Return setWidth
            End Get
            Set(ByVal value As Integer)
                setWidth = value
            End Set
        End Property
        '
        '
        '
        Public Property height As Integer
            Get
                Return setWidth
            End Get
            Set(ByVal value As Integer)
                setHeight = value
            End Set
        End Property
        '
        '
        '=======================================================================================================
        '
        ' IDisposable support
        '
        '=======================================================================================================
        '
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