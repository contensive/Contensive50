
Imports Contensive.BaseClasses
Imports System.Runtime.InteropServices

Namespace Contensive.Core
    '
    ' comVisible to be activeScript compatible
    '
    <ComVisible(True)> _
    <ComClass(CPDocClass.ClassId, CPDocClass.InterfaceId, CPDocClass.EventsId)> _
    Public Class CPDocClass
        Inherits BaseClasses.CPDocBaseClass
        Implements IDisposable
#Region "COM GUIDs"
        Public Const ClassId As String = "414BD6A9-195F-4E0F-AE24-B7BF56749CDD"
        Public Const InterfaceId As String = "347D06BC-4D68-4DBE-82FE-B72115E24A56"
        Public Const EventsId As String = "95E8786B-E778-4617-96BA-B45C53E4AFD1"
#End Region
        '
        Private cp As CPClass
        Private cpCore As Contensive.Core.cpCoreClass
        Private LocalVars As New Collection
        Private LocalVarNameList As String
        Protected disposed As Boolean = False
        '
        Friend Sub New(ByVal cpParent As CPClass)
            MyBase.New()
            cp = cpParent
            cpCore = cp.core
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
                    cpCore = Nothing
                    cp = Nothing
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
        Public Overrides Property content() As String
            Get
                If True Then
                    Return cpCore.main_PageContent
                Else
                    Return ""
                End If
            End Get
            Set(ByVal value As String)
                If True Then
                    cpCore.main_PageContent = value
                End If
            End Set
        End Property

        Public Overrides ReadOnly Property navigationStructure() As String
            Get
                If True Then
                    Return cpCore.main_RenderedNavigationStructure
                Else
                    Return ""
                End If
            End Get
        End Property

        Public Overrides Property noFollow() As Boolean
            Get
                If True Then
                    Return cpCore.main_MetaContentNoFollow
                Else
                    Return False
                End If
            End Get
            Set(ByVal value As Boolean)
                If True Then
                    cpCore.main_MetaContentNoFollow = value
                End If
            End Set
        End Property

        Public Overrides ReadOnly Property pageId() As Integer
            Get
                If True Then
                    Return cpCore.main_RenderedPageID
                Else
                    Return 0
                End If
            End Get
        End Property

        Public Overrides ReadOnly Property pageName() As String
            Get
                Dim s As String
                If True Then
                    s = cpCore.main_RenderedPageName
                    If (s Is Nothing) Then
                        s = ""
                    End If
                    Return s
                Else
                    Return ""
                End If
            End Get
        End Property

        Public Overrides ReadOnly Property refreshQueryString() As String
            Get
                Dim s As String
                If True Then
                    s = cpCore.web_RefreshQueryString
                    If (s Is Nothing) Then
                        s = ""
                    End If
                    Return s
                Else
                    Return ""
                End If
            End Get
        End Property

        Public Overrides ReadOnly Property sectionId() As Integer
            Get
                If True Then
                    Return cpCore.main_RenderedSectionID
                Else
                    Return 0
                End If
            End Get
        End Property

        Public Overrides ReadOnly Property startTime() As Date
            Get
                If True Then
                    Return cpCore.main_PageStartTime
                Else
                    Return New Date()
                End If
            End Get
        End Property

        Public Overrides ReadOnly Property templateId() As Integer
            Get
                If True Then
                    Return cpCore.main_RenderedTemplateID
                Else
                    Return 0
                End If
            End Get
        End Property

        Public Overrides ReadOnly Property type() As String
            Get
                If True Then
                    Return cpCore.main_docType
                Else
                    Return ""
                End If
            End Get
        End Property
        '
        '
        '
        Public Overrides Sub addHeadJavascript(ByVal NewCode As String)
            If True Then
                Call cpCore.main_AddHeadJavascript(NewCode)
            End If
        End Sub
        '
        '
        '
        Public Overrides Sub addHeadTag(ByVal HeadTag As String)
            If True Then
                Call cpCore.main_AddHeadTag(HeadTag)
            End If
        End Sub
        '
        '
        '
        Public Overrides Sub addMetaDescription(ByVal MetaDescription As String)
            If True Then
                Call cpCore.main_addMetaDescription(MetaDescription)
            End If
        End Sub
        '
        '
        '
        Public Overrides Sub addMetaKeywordList(ByVal MetaKeywordList As String)
            If True Then
                Call cpCore.main_addMetaKeywordList(MetaKeywordList)
            End If
        End Sub
        '
        '
        '
        Public Overrides Sub addOnLoadJavascript(ByVal NewCode As String)
            If True Then
                Call cpCore.main_AddOnLoadJavascript(NewCode)
            End If
        End Sub
        '
        '
        '
        Public Overrides Sub addTitle(ByVal PageTitle As String)
            If True Then
                Call cpCore.main_AddPagetitle(PageTitle)
            End If
        End Sub
        '
        '
        '
        Public Overrides Sub addRefreshQueryString(ByVal Name As String, ByVal Value As String)
            If True Then
                Call cpCore.web_addRefreshQueryString(Name, Value)
            End If
        End Sub
        '
        '
        '
        Public Overrides Sub addHeadStyle(ByVal StyleSheet As String)
            If True Then
                Call addHeadTag(vbCrLf & vbTab & "<style type=""text/css"">" & vbCrLf & vbTab & vbTab & StyleSheet & vbCrLf & vbTab & "</style>")
            End If
        End Sub
        '
        '
        '
        Public Overrides Sub addHeadStyleLink(ByVal StyleSheetLink As String)
            If True Then
                Call cpCore.main_AddStylesheetLink(StyleSheetLink)
            End If
        End Sub

        Public Overrides Sub addBodyEnd(ByVal NewCode As String)
            If True Then
                cpCore.main_ClosePageHTML = cpCore.main_ClosePageHTML & NewCode
            End If
        End Sub

        Public Overrides Property body() As String
            Get
                If True Then
                    body = cpCore.main_FilterInput
                Else
                    Return ""
                End If
            End Get
            Set(ByVal value As String)
                If True Then
                    cpCore.main_FilterInput = value
                End If
            End Set
        End Property
        '
        '
        '
        Public Function getLegacyOptionStringFromVar() As String
            Dim Value As String
            Dim lcName As String
            Dim lcNames() As String
            Dim Ptr As Integer
            '
            getLegacyOptionStringFromVar = ""
            Call tp("getLegacyOptionStringFromVar enter, LocalVarNameList=" & LocalVarNameList)
            If LocalVarNameList <> "" Then
                lcNames = Split(LocalVarNameList, vbCrLf)
                If lcNames.Length > 0 Then
                    For Ptr = 0 To lcNames.Length - 1
                        lcName = lcNames(Ptr)
                        If lcName <> "" Then
                            Value = var(lcName)
                            getLegacyOptionStringFromVar = getLegacyOptionStringFromVar & "&" & encodeLegacyAddonOptionArgument(lcName) & "=" & encodeLegacyAddonOptionArgument(Value)
                        End If
                    Next
                End If
            End If
            Call tp("getLegacyOptionStringFromVar exit, getLegacyOptionStringFromVar=" & getLegacyOptionStringFromVar)
        End Function
        '
        '
        '
        Public Overrides ReadOnly Property siteStylesheet() As String
            Get
                Return cpCore.csv_getStyleSheet()
            End Get
        End Property
        '
        '------------------------------------------------------------------------------------------------------------
        '   Encodes an argument in an Addon OptionString (QueryString) for all non-allowed characters
        '       call this before parsing them together
        '       call decodeAddonOptionArgument after parsing them apart
        '
        '       Arg0,Arg1,Arg2,Arg3,Name=Value&Name=VAlue[Option1|Option2]
        '
        '       This routine is needed for all Arg, Name, Value, Option values
        '
        '------------------------------------------------------------------------------------------------------------
        '
        Friend Function encodeLegacyAddonOptionArgument(ByVal Arg As String) As String
            Dim a As String
            encodeLegacyAddonOptionArgument = ""
            If Arg <> "" Then
                a = Arg
                a = Replace(a, vbCrLf, "#0013#")
                a = Replace(a, vbLf, "#0013#")
                a = Replace(a, vbCr, "#0013#")
                a = Replace(a, "&", "#0038#")
                a = Replace(a, "=", "#0061#")
                a = Replace(a, ",", "#0044#")
                a = Replace(a, """", "#0034#")
                a = Replace(a, "'", "#0039#")
                a = Replace(a, "|", "#0124#")
                a = Replace(a, "[", "#0091#")
                a = Replace(a, "]", "#0093#")
                a = Replace(a, ":", "#0058#")
                encodeLegacyAddonOptionArgument = a
            End If
        End Function
        '
        '------------------------------------------------------------------------------------------------------------
        '   Decodes an argument parsed from an AddonOptionString for all non-allowed characters
        '       AddonOptionString is a & delimited string of name=value[selector]descriptor
        '
        '       to get a value from an AddonOptionString, first use getargument() to get the correct value[selector]descriptor
        '       then remove everything to the right of any '['
        '
        '       call encodeaddonoptionargument before parsing them together
        '       call decodeAddonOptionArgument after parsing them apart
        '
        '       Arg0,Arg1,Arg2,Arg3,Name=Value&Name=VAlue[Option1|Option2]
        '
        '       This routine is needed for all Arg, Name, Value, Option values
        '
        '------------------------------------------------------------------------------------------------------------
        '
        Friend Function decodeLegacyAddonOptionArgument(ByVal EncodedArg As String) As String
            Dim a As String
            '
            decodeLegacyAddonOptionArgument = ""
            If EncodedArg <> "" Then
                a = EncodedArg
                a = Replace(a, "#0058#", ":")
                a = Replace(a, "#0093#", "]")
                a = Replace(a, "#0091#", "[")
                a = Replace(a, "#0124#", "|")
                a = Replace(a, "#0039#", "'")
                a = Replace(a, "#0034#", """")
                a = Replace(a, "#0044#", ",")
                a = Replace(a, "#0061#", "=")
                a = Replace(a, "#0038#", "&")
                a = Replace(a, "#0013#", vbCrLf)
                decodeLegacyAddonOptionArgument = a
            End If
        End Function
        '
        '=======================================================================================================
        '
        '=======================================================================================================
        '
        Public Overrides Function GetProperty(PropertyName As String, Optional DefaultValue As String = "") As String
            Dim s As String = ""
            Try
                '
                ' refactor to do the work directly
                '
                If isVar(PropertyName) Then
                    s = var(PropertyName)
                Else
                    s = DefaultValue
                End If
            Catch ex As Exception

            End Try
            Return s
        End Function
        '
        '=======================================================================================================
        '
        '=======================================================================================================
        '
        Public Overrides Function GetBoolean(PropertyName As String, Optional DefaultValue As String = "") As Boolean
            Return cp.Utils.EncodeBoolean(GetProperty(PropertyName, DefaultValue))
        End Function
        '
        '=======================================================================================================
        '
        '=======================================================================================================
        '
        Public Overrides Function GetDate(PropertyName As String, Optional DefaultValue As String = "") As Date
            Return cp.Utils.EncodeDate(GetProperty(PropertyName, DefaultValue))
        End Function
        '
        '=======================================================================================================
        '
        '=======================================================================================================
        '
        Public Overrides Function GetInteger(PropertyName As String, Optional DefaultValue As String = "") As Integer
            Return cp.Utils.EncodeInteger(GetProperty(PropertyName, DefaultValue))
        End Function
        '
        '=======================================================================================================
        '
        '=======================================================================================================
        '
        Public Overrides Function GetNumber(PropertyName As String, Optional DefaultValue As String = "") As Double
            Return cp.Utils.EncodeNumber(GetProperty(PropertyName, DefaultValue))
        End Function
        '
        '=======================================================================================================
        '
        '=======================================================================================================
        '
        Public Overrides Function GetText(FieldName As String, Optional DefaultValue As String = "") As String
            Return GetProperty(FieldName, DefaultValue)
        End Function
        '
        '=======================================================================================================
        '
        '=======================================================================================================
        '
        Public Overrides Function IsProperty(FieldName As String) As Boolean
            Dim s As Boolean
            '
            '
            ' refactor to do the work directly
            '
            s = False
            Try
                s = isVar(FieldName)
            Catch ex As Exception

            End Try
            Return s
        End Function
        '
        '=======================================================================================================
        '
        '=======================================================================================================
        '
        Public Overrides Sub SetProperty(FieldName As String, FieldValue As String)
            '
            ' refactor to do the work directly
            '
            Try
                var(FieldName) = FieldValue
            Catch ex As Exception

            End Try
        End Sub
        '
        '=======================================================================================================
        ' Deprecated ------------ us GetProperty and Setproperty
        ' IsVar
        '   like GlobalVar, but it includes the OptionString
        '   a set Var adds an etry to LocalVars
        '=======================================================================================================
        '
        Public Overrides Property var(ByVal Name As String) As String
            Get
                Dim lcName As String
                If Name = "" Then
                    var = ""
                Else
                    lcName = Name.ToLower
                    If LocalVars.Contains(lcName) Then
                        var = LocalVars(lcName)
                    Else
                        var = globalVar(lcName)
                    End If
                End If
                Call tp("Property var get exit, " & Name & "=" & var)
            End Get
            Set(ByVal value As String)
                Dim lcName As String
                'Dim valueObj As String = ""
                'If Not value Is Nothing Then
                '    valueObj = value
                'End If
                value = EncodeText(value)
                Call appendDebugLog("var set, " & Name & "=" & value.ToString)
                If Name <> "" Then
                    lcName = Name.ToLower
                    If LocalVars.Contains(lcName) Then
                        Call LocalVars.Remove(lcName)
                        If value = "" Then
                            LocalVarNameList = Replace(LocalVarNameList, vbCrLf & lcName, "", , , CompareMethod.Text)
                        Else
                            Call LocalVars.Add(value, lcName)
                            Call tp("Property var set, name found in localVars, removed and re-added, LocalVarNameList=" & LocalVarNameList)
                        End If
                    ElseIf value <> "" Then
                        Call LocalVars.Add(value, lcName)
                        LocalVarNameList = LocalVarNameList & vbCrLf & lcName
                        Call tp("Property var set, name not found in localVars so it was added, LocalVarNameList=" & LocalVarNameList)
                    End If
                End If
            End Set
        End Property
        '
        '=======================================================================================================
        ' Deprecated ------------ us GetProperty and Setproperty
        ' GlobalVar
        '   Like ViewingProperties but includes stream
        '   returns
        '       matches to ViewingProperties
        '       then matches to Stream
        '=======================================================================================================
        '
        Public Overrides Property globalVar(ByVal Name As String) As String
            Get
                Dim lcName As String
                If True Then
                    If Name = "" Then
                        globalVar = ""
                    Else
                        lcName = Name.ToLower
                        If cpCore.main_IsViewingProperty(lcName) Then
                            globalVar = cpCore.main_GetViewingProperty(lcName)
                        Else
                            globalVar = cpCore.doc_getText(lcName)
                        End If
                    End If
                Else
                    globalVar = ""
                End If
            End Get
            Set(ByVal value As String)
                If True Then
                    Call cpCore.main_SetViewingProperty(Name.ToLower, value)
                End If
            End Set
        End Property
        '
        '=======================================================================================================
        ' Deprecated ------------ us GetProperty and Setproperty
        ' IsGlobalVar 
        '   returns true if
        '       IsViewingProperties is true or InStream is true
        '=======================================================================================================
        '
        Public Overrides ReadOnly Property isGlobalVar(ByVal Name As String) As Boolean
            Get
                Dim lcName As String
                If True Then
                    If Name = "" Then
                        isGlobalVar = ""
                    Else
                        lcName = Name.ToLower
                        isGlobalVar = cpCore.main_IsViewingProperty(lcName)
                        If Not isGlobalVar Then
                            isGlobalVar = cpCore.main_InStream(lcName)
                        End If
                    End If
                Else
                    isGlobalVar = False
                End If
            End Get
        End Property
        '
        '=======================================================================================================
        ' Deprecated ------------ us GetProperty and Setproperty
        ' IsVar
        '   returns true if
        '       matches to GlobalVars or LocalVars collections
        '=======================================================================================================
        '
        Public Overrides ReadOnly Property isVar(ByVal Name As String) As Boolean
            Get
                Dim lcName As String
                If Name = "" Then
                    isVar = False
                Else
                    lcName = Name.ToLower
                    isVar = LocalVars.Contains(lcName)
                    If Not isVar Then
                        isVar = isGlobalVar(lcName)
                    End If
                End If
            End Get
        End Property
        '
        '
        '
        Public Overrides ReadOnly Property IsAdminSite As Boolean
            Get
                Dim returnIsAdmin As Boolean = False
                Try
                    returnIsAdmin = (InStr(1, cp.Request.PathPage, cp.Site.GetText("adminUrl"), vbTextCompare) <> 0)
                Catch ex As Exception
                    Call cp.core.handleException(ex, "unexpected error in IsAdminSite")
                End Try
                Return returnIsAdmin
            End Get
        End Property
        '
        '=======================================================================================================
        '
        ' debugging -- append to logfile
        '
        '=======================================================================================================
        '
        Private Sub appendDebugLog(ByVal copy As String)
            'My.Computer.FileSystem.WriteAllText("c:\clibCpDocDebug.log", Now & " - cp.doc, " & copy & vbCrLf, True)
            ' 'My.Computer.FileSystem.WriteAllText(System.AppDocmc.main_CurrentDocmc.main_BaseDirectory() & "cpLog.txt", Now & " - " & copy & vbCrLf, True)
        End Sub
        '
        '=======================================================================================================
        '
        ' debugging -- testpoint
        '
        '=======================================================================================================
        '
        Private Sub tp(ByVal msg As String)
            'Call appendDebugLog(msg)
        End Sub
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