
Option Explicit On
Option Strict On

Imports Contensive.Core.Controllers
Imports Contensive.Core.Controllers.genericController
Imports System.Runtime.InteropServices

Namespace Contensive.Core
    '
    ' comVisible to be activeScript compatible
    '
    <ComVisible(True)>
    <ComClass(CPDocClass.ClassId, CPDocClass.InterfaceId, CPDocClass.EventsId)>
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
        Private cpCore As Contensive.Core.coreClass
        Protected disposed As Boolean = False
        '
        '====================================================================================================
        ''' <summary>
        ''' constructor
        ''' </summary>
        ''' <param name="cpParent"></param>
        Public Sub New(ByVal cpParent As CPClass)
            MyBase.New()
            cp = cpParent
            cpCore = cp.core
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' destructor
        ''' </summary>
        ''' <param name="disposing"></param>
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
        '====================================================================================================
        ''' <summary>
        ''' Returns the pageManager content
        ''' </summary>
        ''' <returns></returns>
        <Obsolete("Page Content comes from the pageManager cpcore.addon.", True)> Public Overrides Property content() As String
            Get
                If True Then
                    Return cpCore.pageManager.pageManager_PageContent
                Else
                    Return ""
                End If
            End Get
            Set(ByVal value As String)
                If True Then
                    cpCore.pageManager.pageManager_PageContent = value
                End If
            End Set
        End Property
        '
        '====================================================================================================
        ''' <summary>
        ''' deprecated.
        ''' </summary>
        ''' <returns></returns>
        <Obsolete("Use addon navigation.", True)> Public Overrides ReadOnly Property navigationStructure() As String
            Get
                If True Then
                    Return cpCore.pageManager.main_RenderedNavigationStructure
                Else
                    Return ""
                End If
            End Get
        End Property
        '
        '====================================================================================================
        ''' <summary>
        ''' Returns to the current value of NoFollow, set by addon execution
        ''' </summary>
        ''' <returns></returns>
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
        '
        '====================================================================================================
        ''' <summary>
        ''' returns the pageId set by the pageManager Addon
        ''' </summary>
        ''' <returns></returns>
        Public Overrides ReadOnly Property pageId() As Integer
            Get
                If True Then
                    Return cpCore.pageManager.main_RenderedPageID
                Else
                    Return 0
                End If
            End Get
        End Property
        '
        '====================================================================================================
        ''' <summary>
        ''' returns the page name, set by the pagemenager addon
        ''' </summary>
        ''' <returns></returns>
        Public Overrides ReadOnly Property pageName() As String
            Get
                Dim s As String
                If True Then
                    s = cpCore.pageManager.main_RenderedPageName
                    If (s Is Nothing) Then
                        s = ""
                    End If
                    Return s
                Else
                    Return ""
                End If
            End Get
        End Property
        '
        '====================================================================================================
        ''' <summary>
        ''' returns the current value of refreshquerystring 
        ''' </summary>
        ''' <returns></returns>
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
        '
        '====================================================================================================
        ''' <summary>
        ''' Returns the value of sectionId as set by pageManager
        ''' </summary>
        ''' <returns></returns>
        Public Overrides ReadOnly Property sectionId() As Integer
            Get
                If True Then
                    Return cpCore.pageManager.main_RenderedSectionID
                Else
                    Return 0
                End If
            End Get
        End Property
        '
        '====================================================================================================
        ''' <summary>
        ''' the time and date when this document was started 
        ''' </summary>
        ''' <returns></returns>
        Public Overrides ReadOnly Property startTime() As Date
            Get
                If True Then
                    Return cpCore.app_startTime
                Else
                    Return New Date()
                End If
            End Get
        End Property
        '
        '====================================================================================================
        ''' <summary>
        ''' returns the id of the template, as set by the page manager
        ''' </summary>
        ''' <returns></returns>
        Public Overrides ReadOnly Property templateId() As Integer
            Get
                If True Then
                    Return cpCore.pageManager.main_RenderedTemplateID
                Else
                    Return 0
                End If
            End Get
        End Property
        '
        '====================================================================================================
        ''' <summary>
        ''' returns the docType, set by the page manager settings 
        ''' </summary>
        ''' <returns></returns>
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
        '====================================================================================================
        ''' <summary>
        ''' adds javascript code to the head of the document
        ''' </summary>
        ''' <param name="NewCode"></param>
        Public Overrides Sub addHeadJavascript(ByVal NewCode As String)
            If True Then
                Call cpCore.htmlDoc.main_AddHeadJavascript(NewCode)
            End If
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' adds a javascript tag to the head of the document
        ''' </summary>
        ''' <param name="HeadTag"></param>
        Public Overrides Sub addHeadTag(ByVal HeadTag As String)
            If True Then
                Call cpCore.htmlDoc.main_AddHeadTag(HeadTag)
            End If
        End Sub
        '
        '====================================================================================================
        '
        Public Overrides Sub addMetaDescription(ByVal MetaDescription As String)
            If True Then
                Call cpCore.htmlDoc.main_addMetaDescription(MetaDescription)
            End If
        End Sub
        '
        '====================================================================================================
        '
        Public Overrides Sub addMetaKeywordList(ByVal MetaKeywordList As String)
            If True Then
                Call cpCore.htmlDoc.main_addMetaKeywordList(MetaKeywordList)
            End If
        End Sub
        '
        '====================================================================================================
        '
        Public Overrides Sub addOnLoadJavascript(ByVal NewCode As String)
            If True Then
                Call cpCore.htmlDoc.main_AddOnLoadJavascript(NewCode)
            End If
        End Sub
        '
        '====================================================================================================
        '
        Public Overrides Sub addTitle(ByVal PageTitle As String)
            If True Then
                Call cpCore.htmlDoc.main_AddPagetitle(PageTitle)
            End If
        End Sub
        '
        '====================================================================================================
        '
        Public Overrides Sub addRefreshQueryString(ByVal Name As String, ByVal Value As String)
            If True Then
                Call cpCore.htmlDoc.webServerIO_addRefreshQueryString(Name, Value)
            End If
        End Sub
        '
        '====================================================================================================
        '
        Public Overrides Sub addHeadStyle(ByVal StyleSheet As String)
            If True Then
                Call addHeadTag(vbCrLf & vbTab & "<style type=""text/css"">" & vbCrLf & vbTab & vbTab & StyleSheet & vbCrLf & vbTab & "</style>")
            End If
        End Sub
        '
        '====================================================================================================
        '
        Public Overrides Sub addHeadStyleLink(ByVal StyleSheetLink As String)
            If True Then
                Call cpCore.htmlDoc.main_AddStylesheetLink(StyleSheetLink)
            End If
        End Sub
        '
        '====================================================================================================
        '
        Public Overrides Sub addBodyEnd(ByVal NewCode As String)
            If True Then
                cpCore.htmlDoc.htmlForEndOfBody = cpCore.htmlDoc.htmlForEndOfBody & NewCode
            End If
        End Sub
        '
        '====================================================================================================
        '
        Public Overrides Property body() As String
            Get
                If True Then
                    body = cpCore.htmlDoc.html_DocBodyFilter
                Else
                    Return ""
                End If
            End Get
            Set(ByVal value As String)
                If True Then
                    cpCore.htmlDoc.html_DocBodyFilter = value
                End If
            End Set
        End Property

        '
        '====================================================================================================
        '
        Public Overrides ReadOnly Property siteStylesheet() As String
            Get
                Return cpCore.pageManager.pageManager_GetStyleSheet2()
            End Get
        End Property
        '
        '====================================================================================================
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
        Public Function decodeLegacyAddonOptionArgument(ByVal EncodedArg As String) As String
            Dim a As String
            '
            decodeLegacyAddonOptionArgument = ""
            If EncodedArg <> "" Then
                a = EncodedArg
                a = genericController.vbReplace(a, "#0058#", ":")
                a = genericController.vbReplace(a, "#0093#", "]")
                a = genericController.vbReplace(a, "#0091#", "[")
                a = genericController.vbReplace(a, "#0124#", "|")
                a = genericController.vbReplace(a, "#0039#", "'")
                a = genericController.vbReplace(a, "#0034#", """")
                a = genericController.vbReplace(a, "#0044#", ",")
                a = genericController.vbReplace(a, "#0061#", "=")
                a = genericController.vbReplace(a, "#0038#", "&")
                a = genericController.vbReplace(a, "#0013#", vbCrLf)
                decodeLegacyAddonOptionArgument = a
            End If
        End Function
        '
        '=======================================================================================================
        '
        Public Overrides Function GetProperty(PropertyName As String, Optional DefaultValue As String = "") As String
            If cpCore.docProperties.containsKey(PropertyName) Then
                Return cpCore.docProperties.getText(PropertyName)
            Else
                Return DefaultValue
            End If
        End Function
        '
        '=======================================================================================================
        '
        Public Overrides Function GetBoolean(PropertyName As String, Optional DefaultValue As String = "") As Boolean
            Return genericController.EncodeBoolean(GetProperty(PropertyName, DefaultValue))
        End Function
        '
        '=======================================================================================================
        '
        Public Overrides Function GetDate(PropertyName As String, Optional DefaultValue As String = "") As Date
            Return genericController.EncodeDate(GetProperty(PropertyName, DefaultValue))
        End Function
        '
        '=======================================================================================================
        '
        Public Overrides Function GetInteger(PropertyName As String, Optional DefaultValue As String = "") As Integer
            Return cp.Utils.EncodeInteger(GetProperty(PropertyName, DefaultValue))
        End Function
        '
        '=======================================================================================================
        '
        Public Overrides Function GetNumber(PropertyName As String, Optional DefaultValue As String = "") As Double
            Return cp.Utils.EncodeNumber(GetProperty(PropertyName, DefaultValue))
        End Function
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
            Return cpCore.docProperties.containsKey(FieldName)
        End Function
        '
        '=======================================================================================================
        '
        Public Overrides Sub SetProperty(FieldName As String, FieldValue As String)
            cpCore.docProperties.setProperty(FieldName, FieldValue)
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
                Return cpCore.docProperties.getText(Name)
                'Dim lcName As String
                'If Name = "" Then
                '    var = ""
                'Else
                '    lcName = Name.ToLower
                '    If LocalVars.Contains(lcName) Then
                '        var = LocalVars(lcName)
                '    Else
                '        var = globalVar(lcName)
                '    End If
                'End If
                'Call tp("Property var get exit, " & Name & "=" & var)
            End Get
            Set(ByVal value As String)
                cpCore.docProperties.setProperty(Name, value)
                'Dim lcName As String
                ''Dim valueObj As String = ""
                ''If Not value Is Nothing Then
                ''    valueObj = value
                ''End If
                'value = genericController.encodeText(value)
                'Call appendDebugLog("var set, " & Name & "=" & value.ToString)
                'If Name <> "" Then
                '    lcName = Name.ToLower
                '    If LocalVars.Contains(lcName) Then
                '        Call LocalVars.Remove(lcName)
                '        If value = "" Then
                '            LocalVarNameList = genericController.vbReplace(LocalVarNameList, vbCrLf & lcName, "", , , CompareMethod.Text)
                '        Else
                '            Call LocalVars.Add(value, lcName)
                '            Call tp("Property var set, name found in localVars, removed and re-added, LocalVarNameList=" & LocalVarNameList)
                '        End If
                '    ElseIf value <> "" Then
                '        Call LocalVars.Add(value, lcName)
                '        LocalVarNameList = LocalVarNameList & vbCrLf & lcName
                '        Call tp("Property var set, name not found in localVars so it was added, LocalVarNameList=" & LocalVarNameList)
                '    End If
                'End If
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
                Return cpCore.docProperties.getText(Name)
                'Dim lcName As String
                'If True Then
                '    If Name = "" Then
                '        globalVar = ""
                '    Else
                '        lcName = Name.ToLower
                '        If cpCore.main_IsViewingProperty(lcName) Then
                '            globalVar = cpCore.docProperties.getText(lcName)
                '        Else
                '            globalVar = cpCore.docProperties.getText(lcName)
                '        End If
                '    End If
                'Else
                '    globalVar = ""
                'End If
            End Get
            Set(ByVal value As String)
                cpCore.docProperties.setProperty(Name, value)
                'If True Then
                '    Call cpCore.docProperties.setProperty(Name.ToLower, value)
                'End If
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
                Return False
                'Dim lcName As String
                'If True Then
                '    If Name = "" Then
                '        isGlobalVar = ""
                '    Else
                '        lcName = Name.ToLower
                '        isGlobalVar = cpCore.main_IsViewingProperty(lcName)
                '        If Not isGlobalVar Then
                '            isGlobalVar = cpCore.docProperties.containsKey(lcName)
                '        End If
                '    End If
                'Else
                '    isGlobalVar = False
                'End If
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
                Return cpCore.docProperties.containsKey(Name)
                'Dim lcName As String
                'If Name = "" Then
                '    isVar = False
                'Else
                '    lcName = Name.ToLower
                '    isVar = LocalVars.Contains(lcName)
                '    If Not isVar Then
                '        isVar = isGlobalVar(lcName)
                '    End If
                'End If
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
                    Call cpCore.handleExceptionAndContinue(ex, "unexpected error in IsAdminSite")
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