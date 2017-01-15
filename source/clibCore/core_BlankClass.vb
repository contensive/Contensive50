
Option Explicit On
Option Strict On

Imports Xunit

Namespace Contensive.Core
    '
    '====================================================================================================
    ''' <summary>
    ''' classSummary
    ''' - first routine should be constructor
    ''' - disposable region at end
    ''' - if disposable is not needed add: not IDisposable - not contained classes that need to be disposed
    ''' </summary>
    Public Class core_BlankClass
        Implements IDisposable
        '
        ' ----- objects passed in constructor, do not dispose
        '
        Private cpCore As coreClass
        '
        ' ----- objects constructed that must be disposed
        '
        Private localObject As Object
        '
        ' ----- constants
        '
        Private Const localConstant As Integer = 100
        '
        ' ----- shared globals
        '
        '
        ' ----- private globals
        '
        '
        Public Sub New(cpCore As coreClass)
            MyBase.New()
            Me.cpCore = cpCore
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' sample function
        ''' </summary>
        ''' <param name="sampleArg"></param>
        ''' <returns></returns>
        Public Function sampleFunction(sampleArg As String) As String
            Dim returnValue As String = ""
            Try
                If String.IsNullOrEmpty(sampleArg) Then
                    Throw New ArgumentException("sampleArg cannot be blank")
                Else
                    '
                    ' code
                    '
                End If
            Catch ex As Exception
                ' if an exception in this routine might need to interrupt program flow
                cpCore.handleExceptionAndRethrow(ex)
                ' if an exception in this routine will never
                cpCore.handleExceptionAndContinue(ex)
            End Try
            Return returnValue
        End Function
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
                    'If Not (AddonObj Is Nothing) Then AddonObj.Dispose()
                End If
                '
                ' cleanup non-managed objects
                '
            End If
        End Sub
#End Region
    End Class
    '
    '====================================================================================================
    ''' <summary>
    ''' unit tests
    ''' </summary>
    Public Class core_blankClass_UnitTests
        '
        <Fact> Public Sub sampleMethod_unit()
            ' arrange
            ' act
            ' assert
            Assert.Equal(True, True)
        End Sub
    End Class
End Namespace