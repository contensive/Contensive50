
Imports Contensive.Core.Controllers
Imports Contensive.Core.Controllers.genericController
Imports System.Windows.Forms
Imports Contensive.BaseClasses
Imports System.Runtime.InteropServices

Namespace Contensive.Core
    '
    ' comVisible to be activeScript compatible
    '
    <ComVisible(True)>
    <ComClass(CPSiteClass.ClassId, CPSiteClass.InterfaceId, CPSiteClass.EventsId)>
    Public Class CPSiteClass
        Inherits BaseClasses.CPSiteBaseClass
        Implements IDisposable
        '
#Region "COM GUIDs"
        Public Const ClassId As String = "7C159DA2-6677-426B-8631-3F235F24BCF0"
        Public Const InterfaceId As String = "50DD3209-AE54-46EF-8344-C6CD8960DD65"
        Public Const EventsId As String = "5E88DB23-E8D7-4CE8-9793-9C7A20F4CF3A"
#End Region
        '
        Private cpCore As Contensive.Core.coreClass
        Private CP As CPClass
        Protected disposed As Boolean = False
        '
        Public Sub New(ByVal cpCoreObj As Contensive.Core.coreClass, ByRef CPParent As CPClass)
            MyBase.New()
            cpCore = cpCoreObj
            CP = CPParent
        End Sub
        '
        ' dispose
        '
        Protected Overridable Overloads Sub Dispose(ByVal disposing As Boolean)
            If Not Me.disposed Then
                Call appendDebugLog(".dispose, dereference cp, main, csv")
                If disposing Then
                    '
                    ' call .dispose for managed objects
                    '
                    CP = Nothing
                    cpCore = Nothing
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
        Public Overrides ReadOnly Property Name() As String 'Inherits BaseClasses.CPSiteBaseClass.Name
            Get
                Return cpCore.serverConfig.appConfig.name
            End Get
        End Property
        '
        '
        '
        Public Overrides Sub SetProperty(ByVal FieldName As String, ByVal FieldValue As String) 'Inherits BaseClasses.CPSiteBaseClass.SetProperty
            Call cpCore.siteProperties.setProperty(FieldName, FieldValue)
        End Sub
        '
        '
        '
        Public Overrides Function GetProperty(ByVal FieldName As String, Optional ByVal DefaultValue As String = "") As String 'Inherits BaseClasses.CPSiteBaseClass.GetProperty
            Return cpCore.siteProperties.getText(FieldName, DefaultValue)
        End Function
        '
        '=======================================================================================================
        '
        '=======================================================================================================
        '
        Public Overrides Function GetBoolean(ByVal PropertyName As String, Optional ByVal DefaultValue As String = "") As Boolean
            Return genericController.EncodeBoolean(GetProperty(PropertyName, DefaultValue))
        End Function
        '
        '=======================================================================================================
        '
        '=======================================================================================================
        '
        Public Overrides Function GetDate(ByVal PropertyName As String, Optional ByVal DefaultValue As String = "") As Date
            Return genericController.EncodeDate(GetProperty(PropertyName, DefaultValue))
        End Function
        '
        '=======================================================================================================
        '
        '=======================================================================================================
        '
        Public Overrides Function GetInteger(ByVal PropertyName As String, Optional ByVal DefaultValue As String = "") As Integer
            Return CP.Utils.EncodeInteger(GetProperty(PropertyName, DefaultValue))
        End Function
        '
        '=======================================================================================================
        '
        '=======================================================================================================
        '
        Public Overrides Function GetNumber(ByVal PropertyName As String, Optional ByVal DefaultValue As String = "") As Double
            Return CP.Utils.EncodeNumber(GetProperty(PropertyName, DefaultValue))
        End Function
        '
        '=======================================================================================================
        '
        '=======================================================================================================
        '
        Public Overrides Function GetText(ByVal FieldName As String, Optional ByVal DefaultValue As String = "") As String
            Return GetProperty(FieldName, DefaultValue)
        End Function
        '
        '
        '
        Public Overrides ReadOnly Property MultiDomainMode() As Boolean 'Inherits BaseClasses.CPSiteBaseClass.MultiDomainMode
            Get
                MultiDomainMode = False
                If genericController.vbInstr(1, "," & cpCore.app_domainList & ",", ",*,", vbTextCompare) <> 0 Then
                    MultiDomainMode = True
                End If
            End Get
        End Property
        '
        '
        '
        <Obsolete("Deprecated, please use cp.File.cdnFiles, cp.File.privateFiles, cp.File.appRootFiles, or cp.File.serverFiles instead.", True)>
        Public Overrides ReadOnly Property PhysicalFilePath() As String 'Inherits BaseClasses.CPSiteBaseClass.PhysicalFilePath
            Get
                'cpCore.handleException(New ApplicationException("PhysicalFilePath is no longer supported. Use cp.file.cdnFiles instead to support scale modes"))
                Return cpCore.cdnFiles.rootLocalPath
            End Get
        End Property
        '
        '
        '
        <Obsolete("Deprecated, please use cp.File.cdnFiles, cp.File.privateFiles, cp.File.appRootFiles, or cp.File.serverFiles instead.", True)>
        Public Overrides ReadOnly Property PhysicalInstallPath() As String 'Inherits BaseClasses.CPSiteBaseClass.PhysicalInstallPath
            Get
                'cpCore.handleException(New ApplicationException("physicalInstallPath is no longer supported"))
                Return cpCore.privateFiles.rootLocalPath
            End Get
        End Property
        '
        '
        '
        <Obsolete("Deprecated, please use cp.File.cdnFiles, cp.File.privateFiles, cp.File.appRootFiles, or cp.File.serverFiles instead.", True)>
        Public Overrides ReadOnly Property PhysicalWWWPath() As String 'Inherits BaseClasses.CPSiteBaseClass.PhysicalWWWPath
            Get
                'cpCore.handleException(New ApplicationException("PhysicalFilePath is no longer supported. Use cp.file.appRootFiles instead to support scale modes"))
                Return cpCore.appRootFiles.rootLocalPath
            End Get
        End Property
        '
        '
        '
        Public Overrides ReadOnly Property TrapErrors() As Boolean 'Inherits BaseClasses.CPSiteBaseClass.TrapErrors
            Get
                Return genericController.EncodeBoolean(GetProperty("TrapErrors", "1"))
            End Get
        End Property

        Public Overrides ReadOnly Property AppPath() As String 'Inherits BaseClasses.CPSiteBaseClass.AppPath
            Get
                Return AppRootPath
                'Return cpCore.web_requestAppPath
            End Get
        End Property
        '
        '
        '
        Public Overrides ReadOnly Property AppRootPath() As String 'Inherits BaseClasses.CPSiteBaseClass.AppRootPath
            Get
                If False Then
                    Return "/"
                Else
                    Return requestAppRootPath
                End If
            End Get
        End Property
        '
        '
        '
        Public Overrides ReadOnly Property DomainPrimary() As String 'Inherits BaseClasses.CPSiteBaseClass.DomainPrimary
            Get
                DomainPrimary = ""
                If cpCore.serverConfig.appConfig.domainList.Count > 0 Then
                    DomainPrimary = cpCore.serverConfig.appConfig.domainList(0)
                End If
                Return DomainPrimary
            End Get
        End Property
        '
        '
        '
        Public Overrides ReadOnly Property Domain() As String 'Inherits BaseClasses.CPSiteBaseClass.Domain
            Get
                If False Then
                    Return DomainPrimary
                Else
                    Return cpCore.webServer.webServerIO_requestDomain
                End If
            End Get
        End Property
        '
        '
        '
        Public Overrides ReadOnly Property DomainList() As String 'Inherits BaseClasses.CPSiteBaseClass.DomainList
            Get
                Return cpCore.app_domainList
            End Get
        End Property
        '
        '
        '
        Public Overrides ReadOnly Property FilePath() As String 'Inherits BaseClasses.CPSiteBaseClass.FilePath
            Get
                Return cpCore.serverConfig.appConfig.cdnFilesNetprefix ' "/" & cpCore.app.config.name & "/files/"
            End Get
        End Property
        '
        '
        '
        Public Overrides ReadOnly Property PageDefault() As String 'Inherits BaseClasses.CPSiteBaseClass.PageDefault
            Get
                If False Then
                    Return GetProperty(siteproperty_serverPageDefault_name, "")
                Else
                    Return cpCore.siteProperties.serverPageDefault
                End If
            End Get
        End Property
        '
        '
        '
        Public Overrides ReadOnly Property VirtualPath() As String 'Inherits BaseClasses.CPSiteBaseClass.VirtualPath
            Get
                Return "/" & cpCore.serverConfig.appConfig.name
            End Get
        End Property
        '
        '
        '
        Public Overrides Function EncodeAppRootPath(ByVal Link As String) As String 'Inherits BaseClasses.CPSiteBaseClass.EncodeAppRootPath
            If False Then
                Return Link
            Else
                Return genericController.EncodeAppRootPath(genericController.encodeText(Link), cpCore.webServer.webServerIO_requestVirtualFilePath, requestAppRootPath, cpCore.webServer.requestDomain)
            End If
        End Function

        Public Overrides Function IsTesting() As Boolean 'Inherits BaseClasses.CPSiteBaseClass.IsTesting
            Return False
        End Function

        Public Overrides Sub LogActivity(ByVal Message As String, ByVal UserID As Integer, ByVal OrganizationID As Integer) 'Inherits BaseClasses.CPSiteBaseClass.LogActivity
            Call cpCore.log_logActivity(Message, 0, UserID, OrganizationID)
        End Sub
        '
        ' Report an alarm
        '   This will set off the server monitor
        '
        Public Overrides Sub LogWarning(ByVal name As String, ByVal description As String, ByVal typeOfWarningKey As String, ByVal instanceKey As String)
            Call cpCore.csv_reportWarning(name, description, typeOfWarningKey, instanceKey)
        End Sub
        '
        ' Report an alarm
        '   This will set off the server monitor
        '
        Public Overrides Sub LogAlarm(ByVal cause As String)
            Call cpCore.csv_reportAlarm(cause)
        End Sub
        '
        ' Report an error message
        '   Eventually, move the cmc.main_ReportError code here so process errors get emailed, etc.
        '
        Public Overrides Sub ErrorReport(ByVal Cause As String) 'Inherits BaseClasses.CPSiteBaseClass.ErrorReport
            '
            throw (New ApplicationException("Unexpected exception")) 'cpCore.handleLegacyError8(Cause, "", True)
        End Sub
        '
        ' Report a structured exception object event, with a message
        '
        Public Overrides Sub ErrorReport(ByVal Ex As System.Exception, Optional ByVal Message As String = "") 'Inherits BaseClasses.CPSiteBaseClass.ErrorReport
            If Message = "" Then
                Call cpCore.handleException(Ex, "n/a", 2)
            Else
                Call cpCore.handleException(Ex, Message, 2)
            End If
            'Dim s As String = ""
            's = Ex.Source & ", " & Ex.ToString
            'If Message <> "" Then
            '    s = Message & " [" & s & "]"
            'End If
            'cpCore.handleLegacyError8(s, "", True)
        End Sub

        Public Overrides Sub RequestTask(ByVal Command As String, ByVal SQL As String, ByVal ExportName As String, ByVal Filename As String) 'Inherits BaseClasses.CPSiteBaseClass.RequestTask
            If False Then
                Call cpCore.tasks_RequestTask(Command, SQL, ExportName, Filename, 0)
            Else
                Call cpCore.main_RequestTask(Command, SQL, ExportName, Filename)
            End If
        End Sub

        Public Overrides Sub TestPoint(ByVal Message As String) 'Inherits BaseClasses.CPSiteBaseClass.TestPoint
            If False Then
                '
                '
                '
            Else
                Call cpCore.debug_testPoint(Message)
            End If
        End Sub

        Public Overrides Function LandingPageId(Optional ByVal DomainName As String = "") As Integer 'Inherits BaseClasses.CPSiteBaseClass.LandingPageId
            Return GetProperty("LandingPageID", "")
        End Function
        '
        '
        '
        Public Overrides Sub addLinkAlias(ByVal linkAlias As String, ByVal pageId As Integer, Optional ByVal queryStringSuffix As String = "")
            Call cpCore.app_addLinkAlias(linkAlias, pageId, queryStringSuffix)
        End Sub
        '
        '
        '
        Public Overrides Function throwEvent(ByVal eventNameIdOrGuid As String) As String
            Dim returnString As String = ""
            Try
                Dim sql As String
                Dim cs As CPCSClass = CP.CSNew()
                Dim addonid As Integer = 0
                '
                sql = "select e.id,c.addonId" _
                    & " from (ccAddonEvents e" _
                    & " left join ccAddonEventCatchers c on c.eventId=e.id)" _
                    & " where "
                If genericController.vbIsNumeric(eventNameIdOrGuid) Then
                    sql &= "e.id=" & CP.Db.EncodeSQLNumber(CDbl(eventNameIdOrGuid))
                ElseIf CP.Utils.isGuid(eventNameIdOrGuid) Then
                    sql &= "e.ccGuid=" & CP.Db.EncodeSQLText(eventNameIdOrGuid)
                Else
                    sql &= "e.name=" & CP.Db.EncodeSQLText(eventNameIdOrGuid)
                End If
                If Not cs.OpenSQL(sql) Then
                    '
                    ' event not found
                    '
                    If genericController.vbIsNumeric(eventNameIdOrGuid) Then
                        '
                        ' can not create an id
                        '
                    ElseIf CP.Utils.isGuid(eventNameIdOrGuid) Then
                        '
                        ' create event with Guid and id for name
                        '
                        Call cs.Close()
                        Call cs.Insert("add-on Events")
                        Call cs.SetField("ccguid", eventNameIdOrGuid)
                        Call cs.SetField("name", "Event " & cs.GetInteger("id").ToString())
                    ElseIf (eventNameIdOrGuid <> "") Then
                        '
                        ' create event with name
                        '
                        Call cs.Close()
                        Call cs.Insert("add-on Events")
                        Call cs.SetField("name", eventNameIdOrGuid)
                    End If
                Else
                    Do While cs.OK
                        addonid = cs.GetInteger("addonid")
                        If addonid <> 0 Then
                            returnString &= CP.Utils.ExecuteAddon(addonid)
                        End If
                        Call cs.GoNext()
                    Loop
                End If
                Call cs.Close()
                '
            Catch ex As Exception
                Call ErrorReport(ex, "Expected error in throwEvent()")
            End Try
            Return returnString
        End Function
        ''
        '' 20151121 removed to resolve issue with com compatibility
        '' Report an unstructured Err object event, with a message
        ''
        'Public Overrides Sub ErrorReport(ByVal Err As Microsoft.VisualBasic.ErrObject, Optional ByVal Message As String = "") 'Inherits BaseClasses.CPSiteBaseClass.ErrorReport
        '    Dim Ex As System.Exception
        '    '
        '    Ex = Err.GetException
        '    CP.Utils.AppendLog(Message & vbCrLf & "source " & Err.Source & vbCrLf & "#" & Err.Number & vbCrLf & "line " & Err.Erl & vbCrLf & "description: " & Err.Description & "]" & vbCrLf & Ex.ToString)
        '    If True Then
        '        '
        '        ' if main is available, report it out that way too
        '        '
        '        throw new applicationException("Unexpected exception") ' Call cpcore.handleLegacyError23(Message)
        '    End If
        'End Sub
        '
        '
        '
        Private Sub appendDebugLog(ByVal copy As String)
            'My.Computer.FileSystem.WriteAllText("c:\clibCpDebug.log", Now & " - cp.site, " & copy & vbCrLf, True)
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