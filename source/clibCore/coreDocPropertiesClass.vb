
Option Explicit On
Option Strict On

Namespace Contensive.Core
    '
    '====================================================================================================
    ''' <summary>
    ''' doc properties are properties limited in scope to this single hit, or viewing
    ''' </summary>
    Public Class coreDocPropertiesClass
        '
        Private cpCore As cpCoreClass
        '
        Public docPropertiesDict As New Dictionary(Of String, docPropertiesClass)
        '
        Public Sub New(cpCore As cpCoreClass)
            MyBase.New()
            Me.cpCore = cpCore
        End Sub
        '
        '====================================================================================================
        '
        Public Sub setProperty(key As String, value As Integer)
            Call setProperty(key, CStr(value))
        End Sub
        '
        '====================================================================================================
        '
        Public Sub setProperty(key As String, value As Date)
            Call setProperty(key, value.ToString)
        End Sub
        '
        '====================================================================================================
        '
        Public Sub setProperty(key As String, value As Boolean)
            Call setProperty(key, value.ToString())
        End Sub
        '
        '====================================================================================================
        '
        Public Sub setProperty(key As String, value As String)
            Try
                Dim prop As New docPropertiesClass
                prop.NameValue = key
                prop.FileContent = Nothing
                prop.FileSize = 0
                prop.fileType = ""
                prop.IsFile = False
                prop.IsForm = False
                prop.Name = key
                prop.NameValue = key & "=" & value
                prop.Value = value
                setProperty(key, prop)

            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
        End Sub
        '
        '====================================================================================================
        '
        Public Sub setProperty(key As String, value As docPropertiesClass)
            Dim propKey As String = encodeDocPropertyKey(key)
            If Not String.IsNullOrEmpty(propKey) Then
                If docPropertiesDict.ContainsKey(propKey) Then
                    docPropertiesDict.Remove(propKey)
                End If
                docPropertiesDict.Add(propKey, value)
            End If
        End Sub
        '
        '====================================================================================================
        '
        Public Function containsKey(ByVal RequestName As String) As Boolean
            Return docPropertiesDict.ContainsKey(encodeDocPropertyKey(RequestName))
        End Function
        '
        '=============================================================================================
        '
        Public Function getInteger(ByVal RequestName As String) As Integer
            Try
                getInteger = EncodeInteger(getText(RequestName))
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
        End Function
        '
        '====================================================================================================
        '
        Public Function getText(ByVal RequestName As String) As String
            Dim returnText As String = ""
            Try
                Dim Key As String
                '
                Key = encodeDocPropertyKey(RequestName)
                If Not String.IsNullOrEmpty(Key) Then
                    If docPropertiesDict.ContainsKey(Key) Then
                        returnText = docPropertiesDict(Key).Value
                    End If
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return returnText
        End Function
        '
        '====================================================================================================
        '
        Private Function encodeDocPropertyKey(sourceKey As String) As String
            Dim returnResult As String = ""
            Try
                returnResult = sourceKey.ToLower()
                If cpCore.webServer.requestSpaceAsUnderscore Then
                    returnResult = Replace(returnResult, " ", "_")
                End If
                If cpCore.webServer.requestDotAsUnderscore Then
                    returnResult = Replace(returnResult, ".", "_")
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return returnResult
        End Function
    End Class


End Namespace