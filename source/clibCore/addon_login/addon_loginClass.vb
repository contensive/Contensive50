
Option Explicit On
Option Strict On
'
Imports Contensive.Core.Controllers
Imports Contensive.Core.Controllers.genericController

Imports System.Xml
Imports Contensive.Core
Imports Contensive.Core.Models.Entity
'
Namespace Contensive.Addons
    '
    Public Class addon_loginClass
        Inherits Contensive.BaseClasses.AddonBaseClass
        '
        '====================================================================================================
        ' objects passed in - do not dispose
        '   sets cp from argument For use In calls To other objects, Then cpCore because cp cannot be used since that would be a circular depenancy
        '====================================================================================================
        '
        Private cp As CPClass                   ' local cp set in constructor
        Private cpCore As coreClass           ' cpCore -- short term, this is the migration solution from a built-in tool, to an addon
        '
        ' -- tmp solution
        Public Sub New(cpCore As coreClass)
            Me.cpCore = cpCore
        End Sub
        '
        '====================================================================================================
        ''' <summary>
        ''' addon method, deliver complete Html admin site
        ''' </summary>
        ''' <param name="cp"></param>
        ''' <returns></returns>
        Public Overrides Function execute(cp As Contensive.BaseClasses.CPBaseClass) As Object
            Dim returnHtml As String = ""
            Try
                '
            Catch ex As Exception
                cp.Site.ErrorReport(ex)
            End Try
            Return returnHtml
        End Function
        '
        '========================================================================
        '   Print the login form in an intercept page
        '========================================================================
        '
        Public Function getLoginPage(forceDefaultLogin As Boolean) As String
            Dim returnREsult As String = ""
            Try
                Dim Body As String
                Dim head As String
                Dim bodyTag As String
                '
                ' ----- Default Login
                '
                If forceDefaultLogin Then
                    Body = getLoginForm_Default()
                Else
                    Body = getLoginForm()
                End If
                Body = "" _
                    & cr & "<p class=""ccAdminNormal"">You are attempting to enter an access controlled area. Continue only if you have authority to enter this area. Information about your visit will be recorded for security purposes.</p>" _
                    & Body _
                    & ""
                '
                Body = "" _
                    & cpCore.html.main_GetPanel(Body, "ccPanel", "ccPanelHilite", "ccPanelShadow", "400", 15) _
                    & cr & "<p>&nbsp;</p>" _
                    & cr & "<p>&nbsp;</p>" _
                    & cr & "<p style=""text-align:center""><a href=""http://www.Contensive.com"" target=""_blank""><img src=""/ccLib/images/ccLibLogin.GIF"" width=""80"" height=""33"" border=""0"" alt=""Contensive Content Control"" ></A></p>" _
                    & cr & "<p style=""text-align:center"" class=""ccAdminSmall"">The content on this web site is managed and delivered by the Contensive Site Management Server. If you do not have member access, please use your back button to return to the public area.</p>" _
                    & ""
                '
                ' --- create an outer table to hold the form
                '
                Body = "" _
                    & cr & "<div class=""ccCon"" style=""width:400px;margin:100px auto 0 auto;"">" _
                    & htmlIndent(cpCore.html.main_GetPanelHeader("Login")) _
                    & htmlIndent(Body) _
                    & "</div>"
                '
                Call cpCore.html.main_SetMetaContent(0, 0)
                Call cpCore.html.main_AddPagetitle2("Login", "loginPage")
                head = cpCore.html.getHtmlDocHead(False)
                If cpCore.doc.template.BodyTag <> "" Then
                    bodyTag = cpCore.doc.template.BodyTag
                Else
                    bodyTag = TemplateDefaultBodyTag
                End If
                returnREsult = cpCore.html.assembleHtmlDoc(cpCore.siteProperties.docTypeDeclaration(), head, bodyTag, Body & cpCore.html.getHtmlDoc_beforeEndOfBodyHtml(False, False, False, False))
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnREsult
        End Function
        '
        '========================================================================
        '   default login form
        '========================================================================
        '
        Public Function getLoginForm_Default() As String
            Dim returnHtml As String = ""
            Try
                Dim Panel As String
                Dim usernameMsg As String
                Dim QueryString As String
                Dim loginForm As String
                Dim Caption As String
                Dim formType As String
                Dim needLoginForm As Boolean
                '
                ' ----- process the previous form, if login OK, return blank (signal for page refresh)
                '
                needLoginForm = True
                formType = cpCore.docProperties.getText("type")
                If formType = FormTypeLogin Then
                    If processFormLoginDefault() Then
                        returnHtml = ""
                        needLoginForm = False
                    End If
                End If
                If needLoginForm Then
                    '
                    ' ----- When page loads, set focus on login username
                    '
                    Call cpCore.doc.addRefreshQueryString("method", "")
                    loginForm = ""
                    Call cpCore.html.addOnLoadJavascript("document.getElementById('LoginUsernameInput').focus()", "login")
                    '
                    ' ----- Error Messages
                    '
                    If genericController.EncodeBoolean(cpCore.siteProperties.getBoolean("allowEmailLogin", False)) Then
                        usernameMsg = "<b>To login, enter your username or email address with your password.</b></p>"
                    Else
                        usernameMsg = "<b>To login, enter your username and password.</b></p>"
                    End If
                    '
                    QueryString = cpCore.webServer.requestQueryString
                    QueryString = genericController.ModifyQueryString(QueryString, RequestNameHardCodedPage, "", False)
                    QueryString = genericController.ModifyQueryString(QueryString, "requestbinary", "", False)
                    '
                    ' ----- Username
                    '
                    If genericController.EncodeBoolean(cpCore.siteProperties.getBoolean("allowEmailLogin", False)) Then
                        Caption = "Username&nbsp;or&nbsp;Email"
                    Else
                        Caption = "Username"
                    End If
                    '
                    loginForm = loginForm _
                    & cr & "<tr>" _
                    & cr2 & "<td style=""text-align:right;vertical-align:middle;width:30%;padding:4px"" align=""right"" width=""30%"">" & SpanClassAdminNormal & Caption & "&nbsp;</span></td>" _
                    & cr2 & "<td style=""text-align:left;vertical-align:middle;width:70%;padding:4px"" align=""left""  width=""70%""><input ID=""LoginUsernameInput"" NAME=""" & "username"" VALUE="""" SIZE=""20"" MAXLENGTH=""50"" ></td>" _
                    & cr & "</tr>"
                    '
                    ' ----- Password
                    '
                    If genericController.EncodeBoolean(cpCore.siteProperties.getBoolean("allowNoPasswordLogin", False)) Then
                        Caption = "Password&nbsp;(optional)"
                    Else
                        Caption = "Password"
                    End If
                    loginForm = loginForm _
                    & cr & "<tr>" _
                    & cr2 & "<td style=""text-align:right;vertical-align:middle;width:30%;padding:4px"" align=""right"">" & SpanClassAdminNormal & Caption & "&nbsp;</span></td>" _
                    & cr2 & "<td style=""text-align:left;vertical-align:middle;width:70%;padding:4px"" align=""left"" ><input NAME=""" & "password"" VALUE="""" SIZE=""20"" MAXLENGTH=""50"" type=""password""></td>" _
                    & cr & "</tr>" _
                    & ""
                    '
                    ' ----- autologin support
                    '
                    If genericController.EncodeBoolean(cpCore.siteProperties.getBoolean("AllowAutoLogin", False)) Then
                        loginForm = loginForm _
                        & cr & "<tr>" _
                        & cr2 & "<td align=""right"">&nbsp;</td>" _
                        & cr2 & "<td align=""left"" >" _
                        & cr3 & "<table border=""0"" cellpadding=""5"" cellspacing=""0"" width=""100%"">" _
                        & cr4 & "<tr>" _
                        & cr5 & "<td valign=""top"" width=""20""><input type=""checkbox"" name=""" & "autologin"" value=""ON"" checked></td>" _
                        & cr5 & "<td valign=""top"" width=""100%"">" & SpanClassAdminNormal & "Login automatically from this computer</span></td>" _
                        & cr4 & "</tr>" _
                        & cr3 & "</table>" _
                        & cr2 & "</td>" _
                        & cr & "</tr>"
                    End If
                    loginForm = loginForm _
                        & cr & "<tr>" _
                        & cr2 & "<td colspan=""2"">&nbsp;</td>" _
                        & cr & "</tr>" _
                        & ""
                    loginForm = "" _
                        & cr & "<table border=""0"" cellpadding=""5"" cellspacing=""0"" width=""100%"">" _
                        & htmlIndent(loginForm) _
                        & cr & "</table>" _
                        & ""
                    loginForm = loginForm _
                        & cpCore.html.html_GetFormInputHidden("Type", FormTypeLogin) _
                        & cpCore.html.html_GetFormInputHidden("email", cpCore.authContext.user.Email) _
                        & cpCore.html.main_GetPanelButtons(ButtonLogin, "Button") _
                        & ""
                    loginForm = "" _
                        & cpCore.html.html_GetFormStart(QueryString) _
                        & htmlIndent(loginForm) _
                        & cr & "</form>" _
                        & ""

                    '-------

                    Panel = "" _
                        & errorController.error_GetUserError(cpCore) _
                        & cr & "<p class=""ccAdminNormal"">" & usernameMsg _
                        & loginForm _
                        & ""
                    '
                    ' ----- Password Form
                    '
                    If genericController.EncodeBoolean(cpCore.siteProperties.getBoolean("allowPasswordEmail", True)) Then
                        Panel = "" _
                            & Panel _
                            & cr & "<p class=""ccAdminNormal""><b>Forget your password?</b></p>" _
                            & cr & "<p class=""ccAdminNormal"">If you are a member of the system and can not remember your password, enter your email address below and we will email your matching username and password.</p>" _
                            & getSendPasswordForm() _
                            & ""
                    End If
                    '
                    returnHtml = "" _
                        & cr & "<div class=""ccLoginFormCon"">" _
                        & htmlIndent(Panel) _
                        & cr & "</div>" _
                        & ""
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnHtml
        End Function
        '
        '========================================================================
        '   same as main_GetLoginForm
        '========================================================================
        '
        Public Function getLoginPanel() As String
            Return getLoginForm()
        End Function
        '
        '=============================================================================
        ''' <summary>
        ''' 
        ''' </summary>
        ''' <returns></returns>
        Public Function getLoginForm() As String
            Dim returnHtml As String = ""
            Try
                '
                Dim loginAddonID As Integer
                Dim isAddonOk As Boolean
                Dim QS As String
                '
                loginAddonID = cpCore.siteProperties.getinteger("Login Page AddonID")
                If loginAddonID <> 0 Then
                    '
                    ' Custom Login
                    '
                    returnHtml = cpCore.addon.execute_legacy2(loginAddonID, "", "", Contensive.BaseClasses.CPUtilsBaseClass.addonContext.ContextPage, "", 0, "", "", False, 0, "", isAddonOk, Nothing)
                    If Not isAddonOk Then
                        loginAddonID = 0
                    ElseIf (returnHtml = "") And (isAddonOk) Then
                        '
                        ' login successful, redirect back to this page (without a method)
                        '
                        QS = cpCore.doc.refreshQueryString
                        QS = genericController.ModifyQueryString(QS, "method", "")
                        QS = genericController.ModifyQueryString(QS, "RequestBinary", "")
                        '
                        Call cpCore.webServer.redirect("?" & QS, "Login form success", False)
                    End If
                End If
                If loginAddonID = 0 Then
                    '
                    ' ----- When page loads, set focus on login username
                    '
                    returnHtml = getLoginForm_Default()
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnHtml
        End Function
        '
        '=============================================================================
        ''' <summary>
        ''' a simple email password form
        ''' </summary>
        ''' <returns></returns>
        Public Function getSendPasswordForm() As String
            Dim returnResult As String = ""
            Try
                Dim QueryString As String
                '
                If cpCore.siteProperties.getBoolean("allowPasswordEmail", True) Then
                    returnResult = "" _
                    & cr & "<table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"">" _
                    & cr2 & "<tr>" _
                    & cr3 & "<td style=""text-align:right;vertical-align:middle;width:30%;padding:4px"" align=""right"" width=""30%"">" & SpanClassAdminNormal & "Email</span></td>" _
                    & cr3 & "<td style=""text-align:left;vertical-align:middle;width:70%;padding:4px"" align=""left""  width=""70%""><input NAME=""" & "email"" VALUE=""" & genericController.encodeHTML(cpCore.authContext.user.Email) & """ SIZE=""20"" MAXLENGTH=""50""></td>" _
                    & cr2 & "</tr>" _
                    & cr2 & "<tr>" _
                    & cr3 & "<td colspan=""2"">&nbsp;</td>" _
                    & cr2 & "</tr>" _
                    & cr2 & "<tr>" _
                    & cr3 & "<td colspan=""2"">" _
                    & htmlIndent(htmlIndent(cpCore.html.main_GetPanelButtons(ButtonSendPassword, "Button"))) _
                    & cr3 & "</td>" _
                    & cr2 & "</tr>" _
                    & cr & "</table>" _
                    & ""
                    '
                    ' write out all of the form input (except state) to hidden fields so they can be read after login
                    '
                    '
                    returnResult = "" _
                    & returnResult _
                    & cpCore.html.html_GetFormInputHidden("Type", FormTypeSendPassword) _
                    & ""
                    For Each key As String In cpCore.docProperties.getKeyList
                        With cpCore.docProperties.getProperty(key)
                            If .IsForm Then
                                Select Case genericController.vbUCase(.Name)
                                    Case "S", "MA", "MB", "USERNAME", "PASSWORD", "EMAIL"
                                    Case Else
                                        returnResult = returnResult & cpCore.html.html_GetFormInputHidden(.Name, .Value)
                                End Select
                            End If
                        End With
                    Next
                    '
                    QueryString = cpCore.doc.refreshQueryString
                    QueryString = genericController.ModifyQueryString(QueryString, "S", "")
                    QueryString = genericController.ModifyQueryString(QueryString, "ccIPage", "")
                    returnResult = "" _
                    & cpCore.html.html_GetFormStart(QueryString) _
                    & htmlIndent(returnResult) _
                    & cr & "</form>" _
                    & ""
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnResult
        End Function
        '
        '========================================================================
        ' ----- Process the login form
        '========================================================================
        '
        Public Function processFormLoginDefault() As Boolean
            Dim returnREsult As Boolean = False
            Try
                Dim LocalMemberID As Integer
                Dim loginForm_Username As String = ""       ' Values entered with the login form
                Dim loginForm_Password As String = ""       '   =
                Dim loginForm_Email As String = ""          '   =
                Dim loginForm_AutoLogin As Boolean = False    '   =
                returnREsult = False
                '
                If True Then
                    '
                    ' Processing can happen
                    '   1) early in init() -- legacy
                    '   2) as well as at the front of main_GetLoginForm - to support addon Login forms
                    ' This flag prevents the default form from processing twice
                    '
                    loginForm_Username = cpCore.docProperties.getText("username")
                    loginForm_Password = cpCore.docProperties.getText("password")
                    loginForm_AutoLogin = cpCore.docProperties.getBoolean("autologin")
                    '
                    If (cpCore.authContext.visit.LoginAttempts < cpCore.siteProperties.maxVisitLoginAttempts) And cpCore.authContext.visit.CookieSupport Then
                        LocalMemberID = cpCore.authContext.authenticateGetId(cpCore, loginForm_Username, loginForm_Password)
                        If LocalMemberID = 0 Then
                            cpCore.authContext.visit.LoginAttempts = cpCore.authContext.visit.LoginAttempts + 1
                            cpCore.authContext.visit.saveObject(cpCore)
                        Else
                            returnREsult = cpCore.authContext.authenticateById(cpCore, LocalMemberID, cpCore.authContext)
                            If returnREsult Then
                                Call logController.logActivity2(cpCore, "successful username/password login", cpCore.authContext.user.id, cpCore.authContext.user.OrganizationID)
                            Else
                                Call logController.logActivity2(cpCore, "bad username/password login", cpCore.authContext.user.id, cpCore.authContext.user.OrganizationID)
                            End If
                        End If
                    End If
                End If
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
            Return returnREsult
        End Function
        '
        '========================================================================
        ' ----- Process the send password form
        '========================================================================
        '
        Public Sub processFormSendPassword()
            Try
                Call cpCore.email.sendPassword(cpCore.docProperties.getText("email"))
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
        End Sub
        '
        '========================================================================
        ' ----- Process the send password form
        '========================================================================
        '
        Public Sub processFormJoin()
            Try
                Dim ErrorMessage As String = ""
                Dim CS As Integer
                Dim FirstName As String
                Dim LastName As String
                Dim FullName As String
                Dim Email As String
                Dim errorCode As Integer = 0
                '
                Dim loginForm_Username As String = ""       ' Values entered with the login form
                Dim loginForm_Password As String = ""       '   =
                Dim loginForm_Email As String = ""          '   =
                Dim loginForm_AutoLogin As Boolean = False    '   =
                '
                loginForm_Username = cpCore.docProperties.getText("username")
                loginForm_Password = cpCore.docProperties.getText("password")
                '
                If Not genericController.EncodeBoolean(cpCore.siteProperties.getBoolean("AllowMemberJoin", False)) Then
                    errorController.error_AddUserError(cpCore, "This site does not accept public main_MemberShip.")
                Else
                    If Not cpCore.authContext.isNewLoginOK(cpCore, loginForm_Username, loginForm_Password, ErrorMessage, errorCode) Then
                        Call errorController.error_AddUserError(cpCore, ErrorMessage)
                    Else
                        If Not (cpCore.debug_iUserError <> "") Then
                            CS = cpCore.db.cs_open("people", "ID=" & cpCore.db.encodeSQLNumber(cpCore.authContext.user.id))
                            If Not cpCore.db.cs_ok(CS) Then
                                cpCore.handleException(New Exception("Could not open the current members account to set the username and password."))
                            Else
                                If (cpCore.db.cs_getText(CS, "username") <> "") Or (cpCore.db.cs_getText(CS, "password") <> "") Or (cpCore.db.cs_getBoolean(CS, "admin")) Or (cpCore.db.cs_getBoolean(CS, "developer")) Then
                                    '
                                    ' if the current account can be logged into, you can not join 'into' it
                                    '
                                    Call cpCore.authContext.logout(cpCore)
                                End If
                                FirstName = cpCore.docProperties.getText("firstname")
                                LastName = cpCore.docProperties.getText("firstname")
                                FullName = FirstName & " " & LastName
                                Email = cpCore.docProperties.getText("email")
                                Call cpCore.db.cs_set(CS, "FirstName", FirstName)
                                Call cpCore.db.cs_set(CS, "LastName", LastName)
                                Call cpCore.db.cs_set(CS, "Name", FullName)
                                Call cpCore.db.cs_set(CS, "username", loginForm_Username)
                                Call cpCore.db.cs_set(CS, "password", loginForm_Password)
                                Call cpCore.authContext.authenticateById(cpCore, cpCore.authContext.user.id, cpCore.authContext)
                            End If
                            Call cpCore.db.cs_Close(CS)
                        End If
                    End If
                End If
                Call cpCore.cache.invalidateContent("People")
            Catch ex As Exception
                cpCore.handleException(ex) : Throw
            End Try
        End Sub
    End Class
End Namespace
