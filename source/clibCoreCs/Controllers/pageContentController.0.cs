using System;

//========================================================================
// This conversion was produced by the Free Edition of
// Instant C# courtesy of Tangible Software Solutions.
// Order the Premium Edition at https://www.tangiblesoftwaresolutions.com
//========================================================================

//
using Contensive.BaseClasses;
using Contensive.Core.Models.Entity;
//
namespace Contensive.Core.Controllers
{
	//
	//====================================================================================================
	/// <summary>
	/// build page content system. Persistence is the docController.
	/// </summary>
	public class pageContentController : IDisposable
	{
		//
		//========================================================================
		//   Returns the HTML body
		//
		//   This code is based on the GoMethod site script
		//========================================================================
		//
		public static string getHtmlBodyTemplate(coreClass cpCore)
		{
			string returnBody = "";
			try
			{
				string AddonReturn = null;
				string layoutError = string.Empty;
				stringBuilderLegacyController Result = new stringBuilderLegacyController();
				string PageContent = null;
				stringBuilderLegacyController Stream = new stringBuilderLegacyController();
				int LocalTemplateID = 0;
				string LocalTemplateName = null;
				string LocalTemplateBody = null;
				bool blockSiteWithLogin = false;
				//
				// -- OnBodyStart add-ons
				CPUtilsBaseClass.addonExecuteContext bodyStartContext = new CPUtilsBaseClass.addonExecuteContext() {addonType = CPUtilsBaseClass.addonContext.ContextOnBodyStart};
				foreach (addonModel addon in cpCore.addonCache.getOnBodyStartAddonList)
				{
					returnBody += cpCore.addon.execute(addon, bodyStartContext);
					//returnBody &= cpCore.addon.execute_legacy2(addon.id, "", "", CPUtilsBaseClass.addonContext.ContextOnBodyStart, "", 0, "", "", False, 0, "", False, Nothing)
				}
				//
				// ----- main_Get Content (Already Encoded)
				//
				blockSiteWithLogin = false;
				PageContent = getHtmlBodyTemplateContent(cpCore, true, true, false, ref blockSiteWithLogin);
				if (blockSiteWithLogin)
				{
					//
					// section blocked, just return the login box in the page content
					//
					returnBody = ""
						+ "\r" + "<div class=\"ccLoginPageCon\">"
						+ genericController.htmlIndent(PageContent) + "\r" + "</div>"
						+ "";
				}
				else if (!cpCore.doc.continueProcessing)
				{
					//
					// exit if stream closed during main_GetSectionpage
					//
					returnBody = "";
				}
				else
				{
					//
					// -- no section block, continue
					LocalTemplateID = cpCore.doc.template.ID;
					LocalTemplateBody = cpCore.doc.template.BodyHTML;
					if (string.IsNullOrEmpty(LocalTemplateBody))
					{
						LocalTemplateBody = TemplateDefaultBody;
					}
					LocalTemplateName = cpCore.doc.template.Name;
					if (string.IsNullOrEmpty(LocalTemplateName))
					{
						LocalTemplateName = "Template " + LocalTemplateID;
					}
					//
					// ----- Encode Template
					//
					LocalTemplateBody = cpCore.html.executeContentCommands(null, LocalTemplateBody, CPUtilsBaseClass.addonContext.ContextTemplate, cpCore.doc.authContext.user.id, cpCore.doc.authContext.isAuthenticated, layoutError);
					returnBody = returnBody + cpCore.html.convertActiveContentToHtmlForWebRender(LocalTemplateBody, "Page Templates", LocalTemplateID, 0, cpCore.webServer.requestProtocol + cpCore.webServer.requestDomain, cpCore.siteProperties.defaultWrapperID, CPUtilsBaseClass.addonContext.ContextTemplate);
					//
					// If Content was not found, add it to the end
					//
					if (returnBody.IndexOf(fpoContentBox) + 1 != 0)
					{
						returnBody = genericController.vbReplace(returnBody, fpoContentBox, PageContent);
					}
					else
					{
						returnBody = returnBody + PageContent;
					}
					//
					// ----- Add tools Panel
					//
					if (!cpCore.doc.authContext.isAuthenticated())
					{
						//
						// not logged in
						//
					}
					else
					{
						//
						// Add template editing
						//
						if (cpCore.visitProperty.getBoolean("AllowAdvancedEditor") & cpCore.doc.authContext.isEditing("Page Templates"))
						{
							returnBody = cpCore.html.getEditWrapper("Page Template [" + LocalTemplateName + "]", cpCore.html.main_GetRecordEditLink2("Page Templates", LocalTemplateID, false, LocalTemplateName, cpCore.doc.authContext.isEditing("Page Templates")) + returnBody);
						}
					}
					//
					// ----- OnBodyEnd add-ons
					//
					CPUtilsBaseClass.addonExecuteContext bodyEndContext = new CPUtilsBaseClass.addonExecuteContext() {addonType = CPUtilsBaseClass.addonContext.ContextFilter};
					foreach (var addon in cpCore.addonCache.getOnBodyEndAddonList())
					{
						cpCore.doc.docBodyFilter = returnBody;
						AddonReturn = cpCore.addon.execute(addon, bodyEndContext);
						//AddonReturn = cpCore.addon.execute_legacy2(addon.id, "", "", CPUtilsBaseClass.addonContext.ContextFilter, "", 0, "", "", False, 0, "", False, Nothing)
						returnBody = cpCore.doc.docBodyFilter + AddonReturn;
					}
					//
					// -- Make it pretty for those who care
					returnBody = htmlReflowController.reflow(cpCore, returnBody);
				}
				//
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
				throw;
			}
			return returnBody;
		}
		//
		//=============================================================================
		//   main_Get Section
		//       Two modes
		//           pre 3.3.613 - SectionName = RootPageName
		//           else - (IsSectionRootPageIDMode) SectionRecord has a RootPageID field
		//=============================================================================
		//
		public static string getHtmlBodyTemplateContent(coreClass cpCore, bool AllowChildPageList, bool AllowReturnLink, bool AllowEditWrapper, ref bool return_blockSiteWithLogin)
		{
			string returnHtml = "";
			try
			{
				bool allowPageWithoutSectionDislay = false;
				int FieldRows = 0;
				//Dim templateId As Integer
				string RootPageContentName = null;
				int PageID = 0;
				bool UseContentWatchLink = cpCore.siteProperties.useContentWatchLink;
				//
				RootPageContentName = "Page Content";
				//
				// -- validate domain
				//domain = Models.Entity.domainModel.createByName(cpCore, cpCore.webServer.requestDomain, New List(Of String))
				if (cpCore.doc.domain == null)
				{
					//
					// -- domain not listed, this is now an error
					logController.log_appendLogPageNotFound(cpCore, cpCore.webServer.requestUrlSource);
					return "<div style=\"width:300px; margin: 100px auto auto auto;text-align:center;\">The domain name is not configured for this site.</div>";
				}
				//
				// -- validate page
				if (cpCore.doc.page.id == 0)
				{
					//
					// -- landing page is not valid -- display error
					logController.log_appendLogPageNotFound(cpCore, cpCore.webServer.requestUrlSource);
					cpCore.doc.redirectBecausePageNotFound = true;
					cpCore.doc.redirectReason = "Redirecting because the page selected could not be found.";
					cpCore.doc.redirectLink = pageContentController.main_ProcessPageNotFound_GetLink(cpCore, cpCore.doc.redirectReason, "", "", PageID, 0);
					cpCore.handleException(new ApplicationException("Page could not be determined. Error message displayed."));
					return "<div style=\"width:300px; margin: 100px auto auto auto;text-align:center;\">This page is not valid.</div>";
				}
				//PageID = cpCore.docProperties.getInteger(rnPageId)
				//If (PageID = 0) Then
				//    '
				//    ' -- Nothing specified, use the Landing Page
				//    PageID = main_GetLandingPageID()
				//    If PageID = 0 Then
				//        '
				//        ' -- landing page is not valid -- display error
				//        Call logController.log_appendLogPageNotFound(cpCore, cpCore.webServer.requestUrlSource)
				//        RedirectBecausePageNotFound = True
				//        RedirectReason = "Redirecting because the page selected could not be found."
				//        redirectLink = main_ProcessPageNotFound_GetLink(RedirectReason, , , PageID, 0)

				//        cpCore.handleExceptionAndContinue(New ApplicationException("Page could not be determined. Error message displayed."))
				//        Return "<div style=""width:300px; margin: 100px auto auto auto;text-align:center;"">This page is not valid.</div>"
				//    End If
				//End If
				//Call cpCore.htmlDoc.webServerIO_addRefreshQueryString(rnPageId, CStr(PageID))
				//templateReason = "The reason this template was selected could not be determined."
				//'
				//' -- build parentpageList (first = current page, last = root)
				//' -- add a 0, then repeat until another 0 is found, or there is a repeat
				//pageToRootList = New List(Of Models.Entity.pageContentModel)()
				//Dim usedPageIdList As New List(Of Integer)()
				//Dim targetPageId = PageID
				//usedPageIdList.Add(0)
				//Do While (Not usedPageIdList.Contains(targetPageId))
				//    usedPageIdList.Add(targetPageId)
				//    Dim targetpage As Models.Entity.pageContentModel = Models.Entity.pageContentModel.create(cpCore, targetPageId, New List(Of String))
				//    If (targetpage Is Nothing) Then
				//        Exit Do
				//    Else
				//        pageToRootList.Add(targetpage)
				//        targetPageId = targetpage.id
				//    End If
				//Loop
				//If (pageToRootList.Count = 0) Then
				//    '
				//    ' -- page is not valid -- display error
				//    cpCore.handleExceptionAndContinue(New ApplicationException("Page could not be determined. Error message displayed."))
				//    Return "<div style=""width:300px; margin: 100px auto auto auto;text-align:center;"">This page is not valid.</div>"
				//End If
				//page = pageToRootList.First
				//'
				//' -- get template from pages
				//Dim template As Models.Entity.pageTemplateModel = Nothing
				//For Each page As Models.Entity.pageContentModel In pageToRootList
				//    If page.TemplateID > 0 Then
				//        template = Models.Entity.pageTemplateModel.create(cpCore, page.TemplateID, New List(Of String))
				//        If (template IsNot Nothing) Then
				//            If (page Is page) Then
				//                templateReason = "This template was used because it is selected by the current page."
				//            Else
				//                templateReason = "This template was used because it is selected one of this page's parents [" & page.name & "]."
				//            End If
				//            Exit For
				//        End If
				//    End If
				//Next
				//'
				//If (template Is Nothing) Then
				//    '
				//    ' -- get template from domain
				//    If (domain IsNot Nothing) Then
				//        template = Models.Entity.pageTemplateModel.create(cpCore, domain.DefaultTemplateId, New List(Of String))
				//    End If
				//    If (template Is Nothing) Then
				//        '
				//        ' -- get template named Default
				//        template = Models.Entity.pageTemplateModel.createByName(cpCore, "default", New List(Of String))
				//    End If
				//End If
				//
				// -- get contentbox
				returnHtml = getContentBox(cpCore, "", AllowChildPageList, AllowReturnLink, false, 0, UseContentWatchLink, allowPageWithoutSectionDislay);
				//
				// -- if fpo_QuickEdit it there, replace it out
				string Editor = null;
				string styleOptionList = string.Empty;
				string addonListJSON = null;
				if (cpCore.doc.redirectLink == "" && (returnHtml.IndexOf(html_quickEdit_fpo) + 1 != 0))
				{
					FieldRows = genericController.EncodeInteger(cpCore.userProperty.getText("Page Content.copyFilename.PixelHeight", "500"));
					if (FieldRows < 50)
					{
						FieldRows = 50;
						cpCore.userProperty.setProperty("Page Content.copyFilename.PixelHeight", 50);
					}
					string stylesheetCommaList = ""; //cpCore.html.main_GetStyleSheet2(csv_contentTypeEnum.contentTypeWeb, templateId, 0)
					addonListJSON = cpCore.html.main_GetEditorAddonListJSON(csv_contentTypeEnum.contentTypeWeb);
					Editor = cpCore.html.getFormInputHTML("copyFilename", cpCore.doc.quickEditCopy, FieldRows.ToString(), "100%", false, true, addonListJSON, stylesheetCommaList, styleOptionList);
					returnHtml = genericController.vbReplace(returnHtml, html_quickEdit_fpo, Editor);
				}
				//
				// -- Add admin warning to the top of the content
				if (cpCore.doc.authContext.isAuthenticatedAdmin(cpCore) & cpCore.doc.adminWarning != "")
				{
					//
					// Display Admin Warnings with Edits for record errors
					//
					if (cpCore.doc.adminWarningPageID != 0)
					{
						cpCore.doc.adminWarning = cpCore.doc.adminWarning + "</p>" + cpCore.html.main_GetRecordEditLink2("Page Content", cpCore.doc.adminWarningPageID, true, "Page " + cpCore.doc.adminWarningPageID, cpCore.doc.authContext.isAuthenticatedAdmin(cpCore)) + "&nbsp;Edit the page<p>";
						cpCore.doc.adminWarningPageID = 0;
					}
					//
					returnHtml = ""
					+ cpCore.html.html_GetAdminHintWrapper(cpCore.doc.adminWarning) + returnHtml + "";
					cpCore.doc.adminWarning = "";
				}
				//
				// -- handle redirect and edit wrapper
				//------------------------------------------------------------------------------------
				//
				if (cpCore.doc.redirectLink != "")
				{
					return cpCore.webServer.redirect(cpCore.doc.redirectLink, cpCore.doc.redirectReason, cpCore.doc.redirectBecausePageNotFound);
				}
				else if (AllowEditWrapper)
				{
					returnHtml = cpCore.html.getEditWrapper("Page Content", returnHtml);
				}
				//
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
			}
			return returnHtml;
		}
		//
		//=============================================================================
		//   Add content padding around content
		//       is called from main_GetPageRaw, as well as from higher up when blocking is turned on
		//=============================================================================
		//
		internal static string getContentBoxWrapper(coreClass cpcore, string Content, int ContentPadding)
		{
			string tempgetContentBoxWrapper = null;
			//dim buildversion As String
			//
			// BuildVersion = app.dataBuildVersion
			tempgetContentBoxWrapper = Content;
			if (cpcore.siteProperties.getBoolean("Compatibility ContentBox Pad With Table"))
			{
				//
				if (ContentPadding > 0)
				{
					//
					tempgetContentBoxWrapper = ""
						+ "\r" + "<table border=0 width=\"100%\" cellspacing=0 cellpadding=0>"
						+ cr2 + "<tr>"
						+ cr3 + "<td style=\"padding:" + ContentPadding + "px\">"
						+ genericController.htmlIndent(genericController.htmlIndent(genericController.htmlIndent(tempgetContentBoxWrapper))) + cr3 + "</td>"
						+ cr2 + "</tr>"
						+ "\r" + "</table>";
					//            main_GetContentBoxWrapper = "" _
					//                & cr & "<table border=0 width=""100%"" cellspacing=0 cellpadding=" & ContentPadding & ">" _
					//                & cr2 & "<tr>" _
					//                & cr3 & "<td>" _
					//                & genericController.kmaIndent(KmaIndent(KmaIndent(main_GetContentBoxWrapper))) _
					//                & cr3 & "</td>" _
					//                & cr2 & "</tr>" _
					//                & cr & "</table>"
				}
				tempgetContentBoxWrapper = ""
					+ "\r" + "<div class=\"contentBox\">"
					+ genericController.htmlIndent(tempgetContentBoxWrapper) + "\r" + "</div>";
			}
			else
			{
				//
				tempgetContentBoxWrapper = ""
					+ "\r" + "<div class=\"contentBox\" style=\"padding:" + ContentPadding + "px\">"
					+ genericController.htmlIndent(tempgetContentBoxWrapper) + "\r" + "</div>";
			}
			return tempgetContentBoxWrapper;
		}
		//
		//
		//
		internal static string getDefaultBlockMessage(coreClass cpcore, bool UseContentWatchLink)
		{
				string tempgetDefaultBlockMessage = null;
			try
			{
				tempgetDefaultBlockMessage = "";
				//
				int CS = 0;
				int PageID = 0;
				//
				CS = cpcore.db.csOpen("Copy Content", "name=" + cpcore.db.encodeSQLText(ContentBlockCopyName), "ID",,,,, "Copy,ID");
				if (cpcore.db.csOk(CS))
				{
					tempgetDefaultBlockMessage = cpcore.db.csGet(CS, "Copy");
				}
				cpcore.db.csClose(CS);
				//
				// ----- Do not allow blank message - if still nothing, create default
				//
				if (string.IsNullOrEmpty(tempgetDefaultBlockMessage))
				{
					tempgetDefaultBlockMessage = "<p>The content on this page has restricted access. If you have a username and password for this system, <a href=\"?method=login\" rel=\"nofollow\">Click Here</a>. For more information, please contact the administrator.</p>";
				}
				//
				// ----- Create Copy Content Record for future
				//
				CS = cpcore.db.csInsertRecord("Copy Content");
				if (cpcore.db.csOk(CS))
				{
					cpcore.db.csSet(CS, "Name", ContentBlockCopyName);
					cpcore.db.csSet(CS, "Copy", tempgetDefaultBlockMessage);
					cpcore.db.csSave2(CS);
					//Call cpcore.workflow.publishEdit("Copy Content", genericController.EncodeInteger(cpcore.db.cs_get(CS, "ID")))
				}
				//
				return tempgetDefaultBlockMessage;
				//
				// ----- Error Trap
				//
			}
			catch
			{
				goto ErrorTrap;
			}
ErrorTrap:
			throw new ApplicationException("Unexpected exception"); // Call cpcore.handleLegacyError13("main_GetDefaultBlockMessage")
			return tempgetDefaultBlockMessage;
		}
		//
		//
		//
		//
		public static string getMoreInfoHtml(coreClass cpCore, int PeopleID)
		{
			string result = "";
			try
			{
				int CS = 0;
				string ContactName = null;
				string ContactPhone = null;
				string ContactEmail = null;
				string Copy;
				//
				Copy = "";
				CS = cpCore.db.cs_openContentRecord("People", PeopleID,,,, "Name,Phone,Email");
				if (cpCore.db.csOk(CS))
				{
					ContactName = (cpCore.db.csGetText(CS, "Name"));
					ContactPhone = (cpCore.db.csGetText(CS, "Phone"));
					ContactEmail = (cpCore.db.csGetText(CS, "Email"));
					if (!string.IsNullOrEmpty(ContactName))
					{
						Copy = Copy + "For more information, please contact " + ContactName;
						if (string.IsNullOrEmpty(ContactEmail))
						{
							if (!string.IsNullOrEmpty(ContactPhone))
							{
								Copy = Copy + " by phone at " + ContactPhone;
							}
						}
						else
						{
							Copy = Copy + " by <A href=\"mailto:" + ContactEmail + "\">email</A>";
							if (!string.IsNullOrEmpty(ContactPhone))
							{
								Copy = Copy + " or by phone at " + ContactPhone;
							}
						}
						Copy = Copy;
					}
					else
					{
						if (string.IsNullOrEmpty(ContactEmail))
						{
							if (!string.IsNullOrEmpty(ContactPhone))
							{
								Copy = Copy + "For more information, please call " + ContactPhone;
							}
						}
						else
						{
							Copy = Copy + "For more information, please <A href=\"mailto:" + ContactEmail + "\">email</A>";
							if (!string.IsNullOrEmpty(ContactPhone))
							{
								Copy = Copy + ", or call " + ContactPhone;
							}
							Copy = Copy;
						}
					}
				}
				cpCore.db.csClose(CS);
				//
				result = Copy;
			}
			catch (Exception ex)
			{
				cpCore.handleException(ex);
			}
			return result;
		}
		//
		//
		//
		internal static string getTableRow(string Caption, string Result, bool EvenRow)
		{
			//
			string CopyCaption = null;
			string CopyResult = null;
			//
			CopyCaption = Caption;
			if (string.IsNullOrEmpty(CopyCaption))
			{
				CopyCaption = "&nbsp;";
			}
			//
			CopyResult = Result;
			if (string.IsNullOrEmpty(CopyResult))
			{
				CopyResult = "&nbsp;";
			}
			//
			return genericController.GetTableCell("<nobr>" + CopyCaption + "</nobr>", "150",, EvenRow, "right") + genericController.GetTableCell(CopyResult, "100%",, EvenRow, "left") + kmaEndTableRow;
		}
		//
		public static void loadPage(coreClass cpcore, int requestedPageId, domainModel domain)
		{
			try
			{
				if (domain == null)
				{
					//
					// -- domain is not valid
					cpcore.handleException(new ApplicationException("Page could not be determined because the domain was not recognized."));
				}
				else
				{
					//
					// -- attempt requested page
					pageContentModel requestedPage = null;
					if (!requestedPageId.Equals(0))
					{
						requestedPage = pageContentModel.create(cpcore, requestedPageId);
						if (requestedPage == null)
						{
							//
							// -- requested page not found
							requestedPage = pageContentModel.create(cpcore, getPageNotFoundPageId(cpcore));
						}
					}
					if (requestedPage == null)
					{
						//
						// -- use the Landing Page
						requestedPage = getLandingPage(cpcore, domain);
					}
					cpcore.doc.addRefreshQueryString(rnPageId, Convert.ToString(requestedPage.id));
					//
					// -- build parentpageList (first = current page, last = root)
					// -- add a 0, then repeat until another 0 is found, or there is a repeat
					cpcore.doc.pageToRootList = new List<Models.Entity.pageContentModel>();
					List<int> usedPageIdList = new List<int>();
					object targetPageId = requestedPage.id;
					usedPageIdList.Add(0);
					while (!usedPageIdList.Contains(targetPageId))
					{
						usedPageIdList.Add(targetPageId);
						Models.Entity.pageContentModel targetpage = Models.Entity.pageContentModel.create(cpcore, targetPageId, new List<string>());
						if (targetpage == null)
						{
							break;
						}
						else
						{
							cpcore.doc.pageToRootList.Add(targetpage);
							targetPageId = targetpage.ParentID;
						}
					}
					if (cpcore.doc.pageToRootList.Count == 0)
					{
						//
						// -- attempt failed, create default page
						cpcore.doc.page = pageContentModel.add(cpcore);
						cpcore.doc.page.name = DefaultNewLandingPageName + ", " + domain.name;
						cpcore.doc.page.Copyfilename.content = landingPageDefaultHtml;
						cpcore.doc.page.save(cpcore);
						cpcore.doc.pageToRootList.Add(cpcore.doc.page);
					}
					else
					{
						cpcore.doc.page = cpcore.doc.pageToRootList.First;
					}
					//
					// -- get template from pages
					cpcore.doc.template = null;
					foreach (Models.Entity.pageContentModel page in cpcore.doc.pageToRootList)
					{
						if (page.TemplateID > 0)
						{
							cpcore.doc.template = Models.Entity.pageTemplateModel.create(cpcore, page.TemplateID, new List<string>());
							if (cpcore.doc.template == null)
							{
								//
								// -- templateId is not valid
								page.TemplateID = 0;
								page.save(cpcore);
							}
							else
							{
								if (page == cpcore.doc.pageToRootList.First)
								{
									cpcore.doc.templateReason = "This template was used because it is selected by the current page.";
								}
								else
								{
									cpcore.doc.templateReason = "This template was used because it is selected one of this page's parents [" + page.name + "].";
								}
								break;
							}
						}
					}
					//
					if (cpcore.doc.template == null)
					{
						//
						// -- get template from domain
						if (domain != null)
						{
							if (domain.DefaultTemplateId > 0)
							{
								cpcore.doc.template = Models.Entity.pageTemplateModel.create(cpcore, domain.DefaultTemplateId, new List<string>());
								if (cpcore.doc.template == null)
								{
									//
									// -- domain templateId is not valid
									domain.DefaultTemplateId = 0;
									domain.save(cpcore);
								}
							}
						}
						if (cpcore.doc.template == null)
						{
							//
							// -- get template named Default
							cpcore.doc.template = Models.Entity.pageTemplateModel.createByName(cpcore, defaultTemplateName, new List<string>());
							if (cpcore.doc.template == null)
							{
								//
								// -- ceate new template named Default
								cpcore.doc.template = Models.Entity.pageTemplateModel.add(cpcore, new List<string>());
								cpcore.doc.template.Name = defaultTemplateName;
								cpcore.doc.template.BodyHTML = cpcore.appRootFiles.readFile(defaultTemplateHomeFilename);
								cpcore.doc.template.save(cpcore);
							}
							//
							// -- set this new template to all domains without a template
							foreach (domainModel d in domainModel.createList(cpcore, "((DefaultTemplateId=0)or(DefaultTemplateId is null))"))
							{
								d.DefaultTemplateId = cpcore.doc.template.ID;
								d.save(cpcore);
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				cpcore.handleException(ex);
			}
		}
		public static int getPageNotFoundPageId(coreClass cpcore)
		{
			int pageId = 0;
			try
			{
				pageId = cpcore.domainLegacyCache.domainDetails.pageNotFoundPageId;
				if (pageId == 0)
				{
					//
					// no domain page not found, use site default
					//
					pageId = cpcore.siteProperties.getinteger("PageNotFoundPageID", 0);
				}
			}
			catch (Exception ex)
			{
				cpcore.handleException(ex);
				throw;
			}
			return pageId;
		}
		//
		//---------------------------------------------------------------------------
		//
		//---------------------------------------------------------------------------
		//
		public static pageContentModel getLandingPage(coreClass cpcore, domainModel domain)
		{
			Models.Entity.pageContentModel landingPage = null;
			try
			{
				if (domain == null)
				{
					//
					// -- domain not available
					cpcore.handleException(new ApplicationException("Landing page could not be determined because the domain was not recognized."));
				}
				else
				{
					//
					// -- attempt domain landing page
					if (!domain.RootPageID.Equals(0))
					{
						landingPage = pageContentModel.create(cpcore, domain.RootPageID);
						if (landingPage == null)
						{
							domain.RootPageID = 0;
							domain.save(cpcore);
						}
					}
					if (landingPage == null)
					{
						//
						// -- attempt site landing page
						int siteLandingPageID = cpcore.siteProperties.getinteger("LandingPageID", 0);
						if (!siteLandingPageID.Equals(0))
						{
							landingPage = pageContentModel.create(cpcore, siteLandingPageID);
							if (landingPage == null)
							{
								cpcore.siteProperties.setProperty("LandingPageID", 0);
								domain.RootPageID = 0;
								domain.save(cpcore);
							}
						}
						if (landingPage == null)
						{
							//
							// -- create detault landing page
							landingPage = pageContentModel.add(cpcore);
							landingPage.name = DefaultNewLandingPageName + ", " + domain.name;
							landingPage.Copyfilename.content = landingPageDefaultHtml;
							landingPage.save(cpcore);
							cpcore.doc.landingPageID = landingPage.id;
						}
						//
						// -- save new page to the domain
						domain.RootPageID = landingPage.id;
						domain.save(cpcore);
					}
				}
			}
			catch (Exception ex)
			{
				cpcore.handleException(ex);
				throw;
			}
			return landingPage;
		}
		//
		// Verify a link from the template link field to be used as a Template Link
		//
		internal static string verifyTemplateLink(coreClass cpcore, string linkSrc)
		{
			string tempverifyTemplateLink = null;
			//
			//
			// ----- Check Link Format
			//
			tempverifyTemplateLink = linkSrc;
			if (!string.IsNullOrEmpty(tempverifyTemplateLink))
			{
				if (genericController.vbInstr(1, tempverifyTemplateLink, "://") != 0)
				{
					//
					// protocol provided, do not fixup
					//
					tempverifyTemplateLink = genericController.EncodeAppRootPath(tempverifyTemplateLink, cpcore.webServer.requestVirtualFilePath, requestAppRootPath, cpcore.webServer.requestDomain);
				}
				else
				{
					//
					// no protocol, convert to short link
					//
					if (tempverifyTemplateLink.Substring(0, 1) != "/")
					{
						//
						// page entered without path, assume it is in root path
						//
						tempverifyTemplateLink = "/" + tempverifyTemplateLink;
					}
					tempverifyTemplateLink = genericController.ConvertLinkToShortLink(tempverifyTemplateLink, cpcore.webServer.requestDomain, cpcore.webServer.requestVirtualFilePath);
					tempverifyTemplateLink = genericController.EncodeAppRootPath(tempverifyTemplateLink, cpcore.webServer.requestVirtualFilePath, requestAppRootPath, cpcore.webServer.requestDomain);
				}
			}
			return tempverifyTemplateLink;
		}
		//
		//
		//
		internal static string main_ProcessPageNotFound_GetLink(coreClass cpcore, string adminMessage, string BackupPageNotFoundLink = "", string PageNotFoundLink = "", int EditPageID = 0, int EditSectionID = 0)
		{
			string result = string.Empty;
			try
			{
				int Pos = 0;
				string DefaultLink = null;
				int PageNotFoundPageID = 0;
				string Link = null;
				//
				PageNotFoundPageID = getPageNotFoundPageId(cpcore);
				if (PageNotFoundPageID == 0)
				{
					//
					// No PageNotFound was set -- use the backup link
					//
					if (string.IsNullOrEmpty(BackupPageNotFoundLink))
					{
						adminMessage = adminMessage + " The Site Property 'PageNotFoundPageID' is not set so the Landing Page was used.";
						Link = cpcore.doc.landingLink;
					}
					else
					{
						adminMessage = adminMessage + " The Site Property 'PageNotFoundPageID' is not set.";
						Link = BackupPageNotFoundLink;
					}
				}
				else
				{
					//
					// Set link
					//
					Link = getPageLink(cpcore, PageNotFoundPageID, "", true, false);
					DefaultLink = getPageLink(cpcore, 0, "", true, false);
					if (Link != DefaultLink)
					{
					}
					else
					{
						adminMessage = adminMessage + "</p><p>The current 'Page Not Found' could not be used. It is not valid, or it is not associated with a valid site section. To configure a valid 'Page Not Found' page, first create the page as a child page on your site and check the 'Page Not Found' checkbox on it's control tab. The Landing Page was used.";
					}
				}
				//
				// Add the Admin Message to the link
				//
				if (cpcore.doc.authContext.isAuthenticatedAdmin(cpcore))
				{
					if (string.IsNullOrEmpty(PageNotFoundLink))
					{
						PageNotFoundLink = cpcore.webServer.requestUrl;
					}
					//
					// Add the Link to the Admin Msg
					//
					adminMessage = adminMessage + "<p>The URL was " + PageNotFoundLink + ".";
					//
					// Add the Referrer to the Admin Msg
					//
					if (cpcore.webServer.requestReferer != "")
					{
						Pos = genericController.vbInstr(1, cpcore.webServer.requestReferrer, "main_AdminWarningPageID=", Microsoft.VisualBasic.Constants.vbTextCompare);
						if (Pos != 0)
						{
							cpcore.webServer.requestReferrer = cpcore.webServer.requestReferrer.Substring(0, Pos - 2);
						}
						Pos = genericController.vbInstr(1, cpcore.webServer.requestReferrer, "main_AdminWarningMsg=", Microsoft.VisualBasic.Constants.vbTextCompare);
						if (Pos != 0)
						{
							cpcore.webServer.requestReferrer = cpcore.webServer.requestReferrer.Substring(0, Pos - 2);
						}
						Pos = genericController.vbInstr(1, cpcore.webServer.requestReferrer, "blockcontenttracking=", Microsoft.VisualBasic.Constants.vbTextCompare);
						if (Pos != 0)
						{
							cpcore.webServer.requestReferrer = cpcore.webServer.requestReferrer.Substring(0, Pos - 2);
						}
						adminMessage = adminMessage + " The referring page was " + cpcore.webServer.requestReferrer + ".";
					}
					//
					adminMessage = adminMessage + "</p>";
					//
					if (EditPageID != 0)
					{
						Link = genericController.modifyLinkQuery(Link, "main_AdminWarningPageID", EditPageID.ToString(), true);
					}
					//
					if (EditSectionID != 0)
					{
						Link = genericController.modifyLinkQuery(Link, "main_AdminWarningSectionID", EditSectionID.ToString(), true);
					}
					//
					Link = genericController.modifyLinkQuery(Link, RequestNameBlockContentTracking, "1", true);
					Link = genericController.modifyLinkQuery(Link, "main_AdminWarningMsg", "<p>" + adminMessage + "</p>", true);
				}
				//
				result = Link;
			}
			catch (Exception ex)
			{
				cpcore.handleException(ex);
			}
			return result;
		}
		//
		//====================================================================================================
		//
		public static string getPageLink(coreClass cpcore, int PageID, string QueryStringSuffix, bool AllowLinkAliasIfEnabled = true, bool UseContentWatchNotDefaultPage = false)
		{
			string result = "";
			try
			{
				string linkPathPage = null;
				//
				// -- set linkPathPath to linkAlias
				if (AllowLinkAliasIfEnabled && cpcore.siteProperties.allowLinkAlias)
				{
					linkPathPage = docController.getLinkAlias(cpcore, PageID, QueryStringSuffix, "");
				}
				if (string.IsNullOrEmpty(linkPathPage))
				{
					//
					// -- if not linkAlis, set default page and qs
					linkPathPage = cpcore.siteProperties.serverPageDefault;
					if (string.IsNullOrEmpty(linkPathPage))
					{
						linkPathPage = "/" + getDefaultScript();
					}
					else
					{
						int Pos = genericController.vbInstr(1, linkPathPage, "?");
						if (Pos != 0)
						{
							linkPathPage = linkPathPage.Substring(0, Pos - 1);
						}
						if (linkPathPage.Substring(0, 1) != "/")
						{
							linkPathPage = "/" + linkPathPage;
						}
					}
					//
					// -- calc linkQS (cleared in come cases later)
					linkPathPage += "?" + rnPageId + "=" + PageID;
					if (!string.IsNullOrEmpty(QueryStringSuffix))
					{
						linkPathPage += "&" + QueryStringSuffix;
					}
				}
				//
				// -- domain -- determine if the domain has any template requirements, and if so, is this template allowed
				string SqlCriteria = "(domainId=" + cpcore.doc.domain.id + ")";
				List<Models.Entity.TemplateDomainRuleModel> allowTemplateRuleList = Models.Entity.TemplateDomainRuleModel.createList(cpcore, SqlCriteria);
				bool templateAllowed = false;
				foreach (TemplateDomainRuleModel rule in allowTemplateRuleList)
				{
					if (rule.templateId == cpcore.doc.template.ID)
					{
						templateAllowed = true;
						break;
					}
				}
				string linkDomain = "";
				if (allowTemplateRuleList.Count == 0)
				{
					//
					// this template has no domain preference, use current domain
					//
					linkDomain = cpcore.webServer.requestDomain;
				}
				else if (cpcore.domainLegacyCache.domainDetails.id == 0)
				{
					//
					// the current domain is not recognized, or is default - use it
					//
					linkDomain = cpcore.webServer.requestDomain;
				}
				else if (templateAllowed)
				{
					//
					// current domain is in the allowed domain list
					//
					linkDomain = cpcore.webServer.requestDomain;
				}
				else
				{
					//
					// there is an allowed domain list and current domain is not on it, or use first
					//
					int setdomainId = allowTemplateRuleList.First.domainId;
					linkDomain = cpcore.db.getRecordName("domains", setdomainId);
					if (string.IsNullOrEmpty(linkDomain))
					{
						linkDomain = cpcore.webServer.requestDomain;
					}
				}
				//
				// -- protocol
				string linkprotocol = "";
				if (cpcore.doc.page.IsSecure | cpcore.doc.template.IsSecure)
				{
					linkprotocol = "https://";
				}
				else
				{
					linkprotocol = "http://";
				}
				//
				// -- assemble
				result = linkprotocol + linkDomain + linkPathPage;
			}
			catch (Exception ex)
			{
				cpcore.handleException(ex);
			}
			return result;
		}
		//
		//====================================================================================================
		// main_Get a page link if you know nothing about the page
		//   If you already have all the info, lik the parents templateid, etc, call the ...WithArgs call
		//====================================================================================================
		//
		public static string main_GetPageLink3(coreClass cpcore, int PageID, string QueryStringSuffix, bool AllowLinkAlias)
		{
			return getPageLink(cpcore, PageID, QueryStringSuffix, AllowLinkAlias, false);
		}
		//
		//
		internal static string getDefaultScript()
		{
			return "default.aspx";
		}
		//
		//========================================================================
		//   main_IsChildRecord
		//
		//   Tests if this record is in the ParentID->ID chain for this content
		//========================================================================
		//
		public static bool isChildRecord(coreClass cpcore, string ContentName, int ChildRecordID, int ParentRecordID)
		{
				bool tempisChildRecord = false;
			try
			{
				//
				Models.Complex.cdefModel CDef = null;
				//
				tempisChildRecord = (ChildRecordID == ParentRecordID);
				if (!tempisChildRecord)
				{
					CDef = Models.Complex.cdefModel.getCdef(cpcore, ContentName);
					if (genericController.IsInDelimitedString(CDef.SelectCommaList.ToUpper(), "PARENTID", ","))
					{
						tempisChildRecord = main_IsChildRecord_Recurse(cpcore, CDef.ContentDataSourceName, CDef.ContentTableName, ChildRecordID, ParentRecordID, "");
					}
				}
				return tempisChildRecord;
				//
				// ----- Error Trap
				//
			}
			catch
			{
				goto ErrorTrap;
			}
ErrorTrap:
			throw new ApplicationException("Unexpected exception"); // Call cpcore.handleLegacyError18("cpCoreClass.IsChildRecord")
			//
			return tempisChildRecord;
		}
		//
		//========================================================================
		//   main_IsChildRecord
		//
		//   Tests if this record is in the ParentID->ID chain for this content
		//========================================================================
		//
		internal static bool main_IsChildRecord_Recurse(coreClass cpcore, string DataSourceName, string TableName, int ChildRecordID, int ParentRecordID, string History)
		{
			bool result = false;
			try
			{
				string SQL = null;
				int CS = 0;
				int ChildRecordParentID = 0;
				//
				SQL = "select ParentID from " + TableName + " where id=" + ChildRecordID;
				CS = cpcore.db.csOpenSql(SQL);
				if (cpcore.db.csOk(CS))
				{
					ChildRecordParentID = cpcore.db.csGetInteger(CS, "ParentID");
				}
				cpcore.db.csClose(CS);
				if ((ChildRecordParentID != 0) & (!genericController.IsInDelimitedString(History, ChildRecordID.ToString(), ",")))
				{
					result = (ParentRecordID == ChildRecordParentID);
					if (!result)
					{
						result = main_IsChildRecord_Recurse(cpcore, DataSourceName, TableName, ChildRecordParentID, ParentRecordID, History + "," + ChildRecordID.ToString());
					}
				}
			}
			catch (Exception ex)
			{
				cpcore.handleException(ex);
			}
			return result;
		}

		//
		//====================================================================================================
#region  IDisposable Support 
		//
		// this class must implement System.IDisposable
		// never throw an exception in dispose
		// Do not change or add Overridable to these methods.
		// Put cleanup code in Dispose(ByVal disposing As Boolean).
		//====================================================================================================
		//
		protected bool disposed = false;
		//
		public void Dispose()
		{
			// do not add code here. Use the Dispose(disposing) overload
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		//
		~pageContentController()
		{
			// do not add code here. Use the Dispose(disposing) overload
			Dispose(false);
//INSTANT C# NOTE: The base class Finalize method is automatically called from the destructor:
			//base.Finalize();
		}
		//
		//====================================================================================================
		/// <summary>
		/// dispose.
		/// </summary>
		/// <param name="disposing"></param>
		protected virtual void Dispose(bool disposing)
		{
			if (!this.disposed)
			{
				this.disposed = true;
				if (disposing)
				{
					//If (cacheClient IsNot Nothing) Then
					//    cacheClient.Dispose()
					//End If
				}
				//
				// cleanup non-managed objects
				//
			}
		}
#endregion
	}
	//
}