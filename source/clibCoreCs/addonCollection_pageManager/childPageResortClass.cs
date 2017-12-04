using System;

//========================================================================
// This conversion was produced by the Free Edition of
// Instant C# courtesy of Tangible Software Solutions.
// Order the Premium Edition at https://www.tangiblesoftwaresolutions.com
//========================================================================

using Contensive.BaseClasses;
using Contensive.Core;
using Models.Entity;
using Contensive.Core.Controllers;

//
namespace Contensive.Addons.PageManager
{
	public class savePageManagerChildListSortClass : AddonBaseClass
	{
		//
		//====================================================================================================
		/// <summary>
		/// pageManager addon interface
		/// </summary>
		/// <param name="cp"></param>
		/// <returns></returns>
		public override object Execute(Contensive.BaseClasses.CPBaseClass cp)
		{
			object tempexecute = null;
			string returnHtml = "";
			try
			{
				coreClass cpCore = ((CPClass)cp).core;
				//
				// decode: "sortlist=childPageList_{parentId}_{listName},page{idOfChild},page{idOfChild},etc"
				//
				string pageCommaList = cp.Doc.GetText("sortlist");
				List<string> pageList = new List<string>(pageCommaList.Split(','));
				string[] ParentPageValues = null;
				if (pageList.Count > 1)
				{
					ParentPageValues = pageList(0).Split('_');
					if (ParentPageValues.Count() < 3)
					{
						//
						// -- parent page is not valid
						cp.Site.ErrorReport(new ArgumentException("pageResort requires first value to identify the parent page"));
					}
					else
					{
						int parentPageId = EncodeInteger(ParentPageValues[1]);
						if (parentPageId == 0)
						{
							//
							// -- parent page is not valid
							cp.Site.ErrorReport(new ArgumentException("pageResort requires a parent page id"));
						}
						else
						{
							//
							// -- create childPageIdList
							//Dim childListName As String = ParentPageValues(2)
							List<int> childPageIdList = new List<int>();
							foreach (string PageIDText in pageList)
							{
								int pageId = EncodeInteger(PageIDText.Replace("page", ""));
								if (pageId > 0)
								{
									childPageIdList.Add(pageId);
								}
							}
							//
							Models.Entity.pageContentModel parentPage = pageContentModel.create(cpCore, parentPageId, new List<string>());
							if (parentPage == null)
							{
								//
								// -- parent page is not valid
								cp.Site.ErrorReport(new ArgumentException("pageResort requires a parent page id"));
							}
							else
							{
								//
								// -- verify page set to required sort method Id
								Models.Entity.sortMethodModel sortMethod = sortMethodModel.createByName(cpCore, "By Alpha Sort Order Field");
								if (sortMethod == null)
								{
									sortMethod = sortMethodModel.createByName(cpCore, "Alpha Sort Order Field");
								}
								if (sortMethod == null)
								{
									//
									// -- create the required sortMethod
									sortMethod = sortMethodModel.add(cpCore);
									sortMethod.name = "By Alpha Sort Order Field";
									sortMethod.OrderByClause = "sortOrder";
									sortMethod.save(cpCore);
								}
								if (parentPage.ChildListSortMethodID != sortMethod.id)
								{
									//
									// -- update page if not set correctly
									parentPage.ChildListSortMethodID = sortMethod.id;
									parentPage.save(cpCore);
								}
								int pagePtr = 0;
								foreach (var childPageId in childPageIdList)
								{
									if (childPageId == 0)
									{
										//
										// -- invalid child page
										cp.Site.ErrorReport(new ApplicationException("child page id is invalid from remote request [" + pageCommaList + "]"));
									}
									else
									{
										string SortOrder = (100000 + (pagePtr * 10)).ToString();
										Models.Entity.pageContentModel childPage = pageContentModel.create(cpCore, childPageId, new List<string>());
										if (childPage.SortOrder != SortOrder)
										{
											childPage.SortOrder = SortOrder;
											childPage.save(cpCore);
										}
									}
									pagePtr += 1;
								}
							}
						}
					}
				}
				tempexecute = "";
			}
			catch (Exception ex)
			{
				cp.Site.ErrorReport(ex);
			}
			return returnHtml;
		}
	}
}
