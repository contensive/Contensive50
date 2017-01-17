
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
        Private cpCore As coreClass
        '
        Public docPropertiesDict As New Dictionary(Of String, docPropertiesClass)
        '
        Public Sub New(cpCore As coreClass)
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
            setProperty(key, value, False)
        End Sub
        '
        '====================================================================================================
        '
        Public Sub setProperty(key As String, value As String, isForm As Boolean)
            Try
                Dim prop As New docPropertiesClass
                prop.NameValue = key
                prop.FileSize = 0
                prop.fileType = ""
                prop.IsFile = False
                prop.IsForm = isForm
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
                If Not String.IsNullOrEmpty(sourceKey) Then
                    returnResult = sourceKey.ToLower()
                    If cpCore.webServerIO.requestSpaceAsUnderscore Then
                        returnResult = vbReplace(returnResult, " ", "_")
                    End If
                    If cpCore.webServerIO.requestDotAsUnderscore Then
                        returnResult = vbReplace(returnResult, ".", "_")
                    End If
                End If
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
            Return returnResult
        End Function
        '
        '
        '
        '==========================================================================================
        ''' <summary>
        ''' add querystring to the doc properties
        ''' </summary>
        ''' <param name="QS"></param>
        Public Sub addQueryString(QS As String)
            Try
                '
                Dim ampSplit() As String
                Dim ampSplitCount As Integer
                Dim ValuePair() As String
                Dim key As String
                Dim Ptr As Integer
                '
                ampSplit = Split(QS, "&")
                ampSplitCount = UBound(ampSplit) + 1
                For Ptr = 0 To ampSplitCount - 1
                    Dim nameValuePair As String = ampSplit(Ptr)
                    Dim docProperty As New docPropertiesClass
                    With docProperty
                        If Not String.IsNullOrEmpty(nameValuePair) Then
                            If vbInstr(1, nameValuePair, "=") <> 0 Then
                                ValuePair = Split(nameValuePair, "=")
                                key = DecodeResponseVariable(CStr(ValuePair(0)))
                                If key <> "" Then
                                    .Name = key
                                    If UBound(ValuePair) > 0 Then
                                        .Value = DecodeResponseVariable(CStr(ValuePair(1)))
                                    End If
                                    .IsForm = False
                                    .IsFile = False
                                    cpCore.webServerIO_ReadStreamJSForm = cpCore.webServerIO_ReadStreamJSForm Or (UCase(.Name) = vbUCase(RequestNameJSForm))
                                    cpCore.main_ReadStreamJSProcess = cpCore.main_ReadStreamJSProcess Or (UCase(.Name) = vbUCase(RequestNameJSProcess))
                                    cpCore.docProperties.setProperty(key, docProperty)
                                End If
                            End If
                        End If
                    End With
                Next
            Catch ex As Exception
                cpCore.handleExceptionAndRethrow(ex)
            End Try
        End Sub

    End Class


End Namespace