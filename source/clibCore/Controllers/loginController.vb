
Option Explicit On
Option Strict On
'
Imports Contensive.Core.Controllers
Imports Contensive.Core.Controllers.genericController

Imports System.Xml
Imports Contensive.Core
Imports Contensive.Core.Models.Entity
Imports Contensive.BaseClasses
'
Namespace Contensive.Core.Controllers
    '
    Public Class loginController
        '
        '========================================================================
        ''' <summary>
        ''' A complete html page with the login form in the middle
        ''' </summary>
        ''' <param name="forceDefaultLogin"></param>
        ''' <returns></returns>
        Public Shared Function getLoginPage(cpcore As coreClass, forceDefaultLogin As Boolean) As String
            Dim returnREsult As String = ""
            Try
                Dim Body As String
                'Dim head As String
                Dim bodyTag As String
                '
                ' ----- Default Login
                '
                If forceDefaultLogin Then
                    Body = getLoginForm_Default(cpcore)
                Else
                    Body = getLoginForm(cpcore)
                End If
                Body = "" _
                    & cr & "<p class=""ccAdminNormal"">You are attempting to enter an access controlled area. Continue only if you have authority to enter this area. Information about your visit will be recorded for security purposes.</p>" _
                    & Body _
                    & ""
                '
                Body = "" _
                    & cpcore.html.main_GetPanel(Body, "ccPanel", "ccPanelHilite", "ccPanelShadow", "400", 15) _
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
                    & htmlIndent(cpcore.html.main_GetPanelHeader("Login")) _
                    & htmlIndent(Body) _
                    & "</div>"
                '
                Call cpcore.doc.setMetaContent(0, 0)
                Call cpcore.html.doc_AddPagetitle2("Login", "loginPage")
                bodyTag = TemplateDefaultBodyTag
                returnREsult = cpcore.html.getHtmlDoc(Body, bodyTag, True, True, False, True)
            Catch ex As Exception
                cpcore.handleException(ex) : Throw
            End Try
            Return returnREsult
        End Function
        '
        '========================================================================
        '   default login form
        '========================================================================
        '
        Public Shared Function getLoginForm_Default(cpcore As coreClass) As String
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
                formType = cpcore.docProperties.getText("type")
                If formType = FormTypeLogin Then
                    If processFormLoginDefault(cpcore) Then
                        returnHtml = ""
                        needLoginForm = False
                    End If
                End If
                If needLoginForm Then
                    '
                    ' ----- When page loads, set focus on login username
                    '
                    Call cpcore.doc.addRefreshQueryString("method", "")
                    loginForm = ""
                    Call cpcore.html.addOnLoadJavascript("document.getElementById('LoginUsernameInput').focus()", "login")
                    '
                    ' ----- Error Messages
                    '
                    If genericController.EncodeBoolean(cpcore.siteProperties.getBoolean("allowEmailLogin", False)) Then
                        usernameMsg = "<b>To login, enter your username or email address with your password.</b></p>"
                    Else
                        usernameMsg = "<b>To login, enter your username and password.</b></p>"
                    End If
                    '
                    QueryString = cpcore.webServer.requestQueryString
                    QueryString = genericController.ModifyQueryString(QueryString, RequestNameHardCodedPage, "", False)
                    QueryString = genericController.ModifyQueryString(QueryString, "requestbinary", "", False)
                    '
                    ' ----- Username
                    '
                    If genericController.EncodeBoolean(cpcore.siteProperties.getBoolean("allowEmailLogin", False)) Then
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
                    If genericController.EncodeBoolean(cpcore.siteProperties.getBoolean("allowNoPasswordLogin", False)) Then
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
                    If genericController.EncodeBoolean(cpcore.siteProperties.getBoolean("AllowAutoLogin", False)) Then
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
                        & cpcore.html.html_GetFormInputHidden("Type", FormTypeLogin) _
                        & cpcore.html.html_GetFormInputHidden("email", cpcore.authContext.user.Email) _
                        & cpcore.html.main_GetPanelButtons(ButtonLogin, "Button") _
                        & ""
                    loginForm = "" _
                        & cpcore.html.html_GetFormStart(QueryString) _
                        & htmlIndent(loginForm) _
                        & cr & "</form>" _
                        & ""

                    '-------

                    Panel = "" _
                        & errorController.error_GetUserError(cpcore) _
                        & cr & "<p class=""ccAdminNormal"">" & usernameMsg _
                        & loginForm _
                        & ""
                    '
                    ' ----- Password Form
                    '
                    If genericController.EncodeBoolean(cpcore.siteProperties.getBoolean("allowPasswordEmail", True)) Then
                        Panel = "" _
                            & Panel _
                            & cr & "<p class=""ccAdminNormal""><b>Forget your password?</b></p>" _
                            & cr & "<p class=""ccAdminNormal"">If you are a member of the system and can not remember your password, enter your email address below and we will email your matching username and password.</p>" _
                            & getSendPasswordForm(cpcore) _
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
                cpcore.handleException(ex) : Throw
            End Try
            Return returnHtml
        End Function
        '
        '=============================================================================
        ''' <summary>
        ''' A login form that can be added to any page. This is just form with no surrounding border, etc. 
        ''' </summary>
        ''' <returns></returns>
        Public Shared Function getLoginForm(cpcore As coreClass, Optional forceDefaultLoginForm As Boolean = False) As String
            Dim returnHtml As String = ""
            Try
                Dim loginAddonID As Integer = 0
                If (Not forceDefaultLoginForm) Then
                    loginAddonID = cpcore.siteProperties.getinteger("Login Page AddonID")
                    If loginAddonID <> 0 Then
                        '
                        ' -- Custom Login
                        Dim addon As Models.Entity.addonModel = Models.Entity.addonModel.create(cpcore, loginAddonID)
                        Dim executeContext As New CPUtilsBaseClass.addonExecuteContext() With {
                            .addonType = CPUtilsBaseClass.addonContext.ContextPage
                        }
                        returnHtml = cpcore.addon.execute(addon, executeContext)
                        'returnHtml = cpcore.addon.execute_legacy2(loginAddonID, "", "", Contensive.BaseClasses.CPUtilsBaseClass.addonContext.ContextPage, "", 0, "", "", False, 0, "", False, Nothing)
                        If (String.IsNullOrEmpty(returnHtml)) Then
                            '
                            ' -- login successful, redirect back to this page (without a method)
                            Dim QS As String = cpcore.doc.refreshQueryString
                            QS = genericController.ModifyQueryString(QS, "method", "")
                            QS = genericController.ModifyQueryString(QS, "RequestBinary", "")
                            '
                            Call cpcore.webServer.redirect("?" & QS, "Login form success", False)
                        End If
                    End If
                End If
                If loginAddonID = 0 Then
                    '
                    ' ----- When page loads, set focus on login username
                    '
                    returnHtml = getLoginForm_Default(cpcore)
                End If
            Catch ex As Exception
                cpcore.handleException(ex) : Throw
            End Try
            Return returnHtml
        End Function
        '
        '=============================================================================
        ''' <summary>
        ''' a simple email password form
        ''' </summary>
        ''' <returns></returns>
        Public Shared Function getSendPasswordForm(cpcore As coreClass) As String
            Dim returnResult As String = ""
            Try
                Dim QueryString As String
                '
                If cpcore.siteProperties.getBoolean("allowPasswordEmail", True) Then
                    returnResult = "" _
                    & cr & "<table border=""0"" cellpadding=""0"" cellspacing=""0"" width=""100%"">" _
                    & cr2 & "<tr>" _
                    & cr3 & "<td style=""text-align:right;vertical-align:middle;width:30%;padding:4px"" align=""right"" width=""30%"">" & SpanClassAdminNormal & "Email</span></td>" _
                    & cr3 & "<td style=""text-align:left;vertical-align:middle;width:70%;padding:4px"" align=""left""  width=""70%""><input NAME=""" & "email"" VALUE=""" & genericController.encodeHTML(cpcore.authContext.user.Email) & """ SIZE=""20"" MAXLENGTH=""50""></td>" _
                    & cr2 & "</tr>" _
                    & cr2 & "<tr>" _
                    & cr3 & "<td colspan=""2"">&nbsp;</td>" _
                    & cr2 & "</tr>" _
                    & cr2 & "<tr>" _
                    & cr3 & "<td colspan=""2"">" _
                    & htmlIndent(htmlIndent(cpcore.html.main_GetPanelButtons(ButtonSendPassword, "Button"))) _
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
                    & cpcore.html.html_GetFormInputHidden("Type", FormTypeSendPassword) _
                    & ""
                    For Each key As String In cpcore.docProperties.getKeyList
                        With cpcore.docProperties.getProperty(key)
                            If .IsForm Then
                                Select Case genericController.vbUCase(.Name)
                                    Case "S", "MA", "MB", "USERNAME", "PASSWORD", "EMAIL"
                                    Case Else
                                        returnResult = returnResult & cpcore.html.html_GetFormInputHidden(.Name, .Value)
                                End Select
                            End If
                        End With
                    Next
                    '
                    QueryString = cpcore.doc.refreshQueryString
                    QueryString = genericController.ModifyQueryString(QueryString, "S", "")
                    QueryString = genericController.ModifyQueryString(QueryString, "ccIPage", "")
                    returnResult = "" _
                    & cpcore.html.html_GetFormStart(QueryString) _
                    & htmlIndent(returnResult) _
                    & cr & "</form>" _
                    & ""
                End If
            Catch ex As Exception
                cpcore.handleException(ex) : Throw
            End Try
            Return returnResult
        End Function
        '
        '========================================================================
        ' ----- Process the login form
        '========================================================================
        '
        Public Shared Function processFormLoginDefault(cpcore As coreClass) As Boolean
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
                    loginForm_Username = cpcore.docProperties.getText("username")
                    loginForm_Password = cpcore.docProperties.getText("password")
                    loginForm_AutoLogin = cpcore.docProperties.getBoolean("autologin")
                    '
                    If (cpcore.authContext.visit.LoginAttempts < cpcore.siteProperties.maxVisitLoginAttempts) And cpcore.authContext.visit.CookieSupport Then
                        LocalMemberID = cpcore.authContext.authenticateGetId(cpcore, loginForm_Username, loginForm_Password)
                        If LocalMemberID = 0 Then
                            cpcore.authContext.visit.LoginAttempts = cpcore.authContext.visit.LoginAttempts + 1
                            cpcore.authContext.visit.saveObject(cpcore)
                        Else
                            returnREsult = cpcore.authContext.authenticateById(cpcore, LocalMemberID, cpcore.authContext)
                            If returnREsult Then
                                Call logController.logActivity2(cpcore, "successful username/password login", cpcore.authContext.user.id, cpcore.authContext.user.OrganizationID)
                            Else
                                Call logController.logActivity2(cpcore, "bad username/password login", cpcore.authContext.user.id, cpcore.authContext.user.OrganizationID)
                            End If
                        End If
                    End If
                End If
            Catch ex As Exception
                cpcore.handleException(ex) : Throw
            End Try
            Return returnREsult
        End Function
        '
        '========================================================================
        ' ----- Process the send password form
        '========================================================================
        '
        Public Shared Sub processFormSendPassword(cpcore As coreClass)
            Try
                Call cpcore.email.sendPassword(cpcore.docProperties.getText("email"))
            Catch ex As Exception
                cpcore.handleException(ex) : Throw
            End Try
        End Sub
        '
        '========================================================================
        ' ----- Process the send password form
        '========================================================================
        '
        Public Shared Sub processFormJoin(cpcore As coreClass)
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
                loginForm_Username = cpcore.docProperties.getText("username")
                loginForm_Password = cpcore.docProperties.getText("password")
                '
                If Not genericController.EncodeBoolean(cpcore.siteProperties.getBoolean("AllowMemberJoin", False)) Then
                    errorController.error_AddUserError(cpcore, "This site does not accept public main_MemberShip.")
                Else
                    If Not cpcore.authContext.isNewLoginOK(cpcore, loginForm_Username, loginForm_Password, ErrorMessage, errorCode) Then
                        Call errorController.error_AddUserError(cpcore, ErrorMessage)
                    Else
                        If Not (cpcore.debug_iUserError <> "") Then
                            CS = cpcore.db.cs_open("people", "ID=" & cpcore.db.encodeSQLNumber(cpcore.authContext.user.id))
                            If Not cpcore.db.cs_ok(CS) Then
                                cpcore.handleException(New Exception("Could not open the current members account to set the username and password."))
                            Else
                                If (cpcore.db.cs_getText(CS, "username") <> "") Or (cpcore.db.cs_getText(CS, "password") <> "") Or (cpcore.db.cs_getBoolean(CS, "admin")) Or (cpcore.db.cs_getBoolean(CS, "developer")) Then
                                    '
                                    ' if the current account can be logged into, you can not join 'into' it
                                    '
                                    Call cpcore.authContext.logout(cpcore)
                                End If
                                FirstName = cpcore.docProperties.getText("firstname")
                                LastName = cpcore.docProperties.getText("firstname")
                                FullName = FirstName & " " & LastName
                                Email = cpcore.docProperties.getText("email")
                                Call cpcore.db.cs_set(CS, "FirstName", FirstName)
                                Call cpcore.db.cs_set(CS, "LastName", LastName)
                                Call cpcore.db.cs_set(CS, "Name", FullName)
                                Call cpcore.db.cs_set(CS, "username", loginForm_Username)
                                Call cpcore.db.cs_set(CS, "password", loginForm_Password)
                                Call cpcore.authContext.authenticateById(cpcore, cpcore.authContext.user.id, cpcore.authContext)
                            End If
                            Call cpcore.db.cs_Close(CS)
                        End If
                    End If
                End If
                Call cpcore.cache.invalidateObject_Content("People")
            Catch ex As Exception
                cpcore.handleException(ex) : Throw
            End Try
        End Sub
    End Class
End Namespace
