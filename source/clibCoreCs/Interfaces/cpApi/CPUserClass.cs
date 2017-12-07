using System;

//========================================================================
// This conversion was produced by the Free Edition of
// Instant C# courtesy of Tangible Software Solutions.
// Order the Premium Edition at https://www.tangiblesoftwaresolutions.com
//========================================================================

using Contensive.Core.Controllers;

using Contensive.BaseClasses;
using System.Runtime.InteropServices;

namespace Contensive.Core
{
	//
	// comVisible to be activeScript compatible
	//
	[ComVisible(true), Microsoft.VisualBasic.ComClass(CPUserClass.ClassId, CPUserClass.InterfaceId, CPUserClass.EventsId)]
	public class CPUserClass : BaseClasses.CPUserBaseClass, IDisposable
	{
		//
#region COM GUIDs
		public const string ClassId = "08EF64C6-9C51-4B32-84D9-0D3BDAF42A28";
		public const string InterfaceId = "B1A95B1F-A00D-4AC6-B3A0-B1619568C2EA";
		public const string EventsId = "DBE2B6CB-6339-4FFB-92D7-BE37AEA841CC";
#endregion
		//
		private Contensive.Core.coreClass cpCore;
		private CPClass CP;
		protected bool disposed = false;
		//
		//====================================================================================================
		//
		public CPUserClass(Contensive.Core.coreClass cpCoreObj, CPClass CPParent) : base()
		{
			cpCore = cpCoreObj;
			CP = CPParent;
		}
		//
		//====================================================================================================
		//
		protected virtual void Dispose(bool disposing)
		{
			if (!this.disposed)
			{
				appendDebugLog(".dispose, dereference cp, main, csv");
				if (disposing)
				{
					//
					// call .dispose for managed objects
					//
					CP = null;
					cpCore = null;
				}
				//
				// Add code here to release the unmanaged resource.
				//
			}
			this.disposed = true;
		}
		//
		//====================================================================================================
		//
		public override string Email //Inherits BaseClasses.CPUserBaseClass.Email
		{
			get
			{
				if (true)
				{
					return CP.core.doc.authContext.user.Email;
				}
				else
				{
					return "";
				}
			}
		}
		//
		//====================================================================================================
		//
		public override int GetIdByLogin(string Username, string Password) //Inherits BaseClasses.CPUserBaseClass.GetIdByLogin
		{
			if (true)
			{
				return CP.core.doc.authContext.authenticateGetId(cpCore, Username, Password);
			}
			else
			{
				return 0;
			}
		}
		//
		//====================================================================================================
		//
		public override int Id //Inherits BaseClasses.CPUserBaseClass.Id
		{
			get
			{
				int localId = 0;
				//
				if (true)
				{
					localId = CP.core.doc.authContext.user.id;
					if (localId == 0)
					{
						localId = CP.core.db.insertContentRecordGetID("people", 0);
						CP.core.doc.authContext.recognizeById(cpCore, localId, CP.core.doc.authContext);
					}
				}
				return localId;
			}
		}
		//
		//====================================================================================================
		//
		public override bool IsAdmin //Inherits BaseClasses.CPUserBaseClass.admin
		{
			get
			{
				if (true)
				{
					return CP.core.doc.authContext.isAuthenticatedAdmin(cpCore);
				}
				else
				{
					return false;
				}
			}
		}
		//
		//====================================================================================================
		//
		public override bool IsAdvancedEditing(string ContentName) //Inherits BaseClasses.CPUserBaseClass.IsAdvancedEditing
		{
			if (true)
			{
				return CP.core.doc.authContext.isAdvancedEditing(cpCore, ContentName);
			}
			else
			{
				return false;
			}
		}
		//
		//====================================================================================================
		//
		public override bool IsAuthenticated //Inherits BaseClasses.CPUserBaseClass.IsAuthenticated
		{
			get
			{
				if (true)
				{
					return (CP.core.doc.authContext.isAuthenticated);
				}
				else
				{
					return false;
				}
			}
		}
		//
		//====================================================================================================
		//
		public override bool IsAuthoring(string ContentName) //Inherits BaseClasses.CPUserBaseClass.IsAuthoring
		{
			if (true)
			{
				return CP.core.doc.authContext.isEditing(ContentName);
			}
			else
			{
				return false;
			}
		}
		//
		//====================================================================================================
		//
		public override bool IsContentManager(string ContentName = "Page Content") //Inherits BaseClasses.CPUserBaseClass.IsContentManager
		{
			if (true)
			{
				return CP.core.doc.authContext.isAuthenticatedContentManager(cpCore, ContentName);
			}
			else
			{
				return false;
			}
		}
		//
		//====================================================================================================
		//
		public override bool IsDeveloper //Inherits BaseClasses.CPUserBaseClass.IsDeveloper
		{
			get
			{
				if (true)
				{
					return CP.core.doc.authContext.isAuthenticatedDeveloper(cpCore);
				}
				else
				{
					return false;
				}
			}
		}
		//
		//====================================================================================================
		//
		public override bool IsEditing(string ContentName) //Inherits BaseClasses.CPUserBaseClass.IsEditing
		{
			if (true)
			{
				return CP.core.doc.authContext.isEditing(ContentName);
			}
			else
			{
				return false;
			}
		}
		//
		//====================================================================================================
		//
		public override bool IsEditingAnything //Inherits BaseClasses.CPUserBaseClass.IsEditingAnything
		{
			get
			{
				if (true)
				{
					return CP.core.doc.authContext.isEditingAnything();
				}
				else
				{
					return false;
				}
			}
		}
		//
		//====================================================================================================
		//
		public override bool IsGuest //Inherits BaseClasses.CPUserBaseClass.IsGuest
		{
			get
			{
				if (true)
				{
					return CP.core.doc.authContext.isGuest(cpCore);
				}
				else
				{
					return false;
				}
			}
		}
		//
		//====================================================================================================
		//
		public override bool IsInGroup(string groupName, int userId = 0) //Inherits BaseClasses.CPUserBaseClass.IsInGroup
		{
			int groupId = 0;
			bool result = false;
			//
			try
			{
				groupId = CP.Group.GetId(groupName);
				if (userId == 0)
				{
					userId = Id;
				}
				if (groupId == 0)
				{
					result = false;
				}
				else
				{
					result = IsInGroupList(groupId.ToString(), userId);
				}
			}
			catch (Exception ex)
			{
				CP.core.handleException(ex, "Unexpected error in cs.user.IsInGroup");
				result = false;
			}
			return result;
		}
		//
		//====================================================================================================
		//
		public override bool IsInGroupList(string GroupIDList, int userId = 0) //Inherits BaseClasses.CPUserBaseClass.IsInGroup
		{
			bool result = false;
			//
			try
			{
				if (userId == 0)
				{
					userId = Id;
				}
				result = CP.core.doc.authContext.isMemberOfGroupIdList(cpCore, userId, IsAuthenticated, GroupIDList, false);
			}
			catch (Exception ex)
			{
				CP.core.handleException(ex, "Unexpected error in cs.user.IsInGroupList");
				result = false;
			}
			return result;
			//
		}
		//
		//====================================================================================================
		//
		public override bool IsMember //Inherits BaseClasses.CPUserBaseClass.IsMember
		{
			get
			{
				if (true)
				{
					return CP.core.doc.authContext.isAuthenticatedMember(cpCore);
				}
				else
				{
					return false;
				}
			}
		}
		//
		//====================================================================================================
		//
		public override bool IsQuickEditing(string ContentName) //Inherits BaseClasses.CPUserBaseClass.IsQuickEditing
		{
			return CP.core.doc.authContext.isQuickEditing(cpCore, ContentName);
		}
		//
		//====================================================================================================
		//
		public override bool IsRecognized //Inherits BaseClasses.CPUserBaseClass.IsRecognized
		{
			get
			{
				return CP.core.doc.authContext.isRecognized(cpCore);
			}
		}
		//
		//====================================================================================================
		//
		public override bool IsWorkflowRendering //Inherits BaseClasses.CPUserBaseClass.IsWorkflowRendering
		{
			get
			{
				return CP.core.doc.authContext.isWorkflowRendering;
			}
		}
		//
		//====================================================================================================
		//
		public override string Language //Inherits BaseClasses.CPUserBaseClass.Language
		{
			get
			{
				if (CP.core.doc.authContext.userLanguage != null)
				{
					return CP.core.doc.authContext.userLanguage.name;
				}
				return string.Empty;
			}
		}
		//
		//====================================================================================================
		//
		public override int LanguageID //Inherits BaseClasses.CPUserBaseClass.LanguageId
		{
			get
			{
				return CP.core.doc.authContext.user.LanguageID;
			}
		}
		//
		//====================================================================================================
		//
		public override bool Login(string UsernameOrEmail, string Password, bool SetAutoLogin = false) //Inherits BaseClasses.CPUserBaseClass.Login
		{
			return CP.core.doc.authContext.authenticate(cpCore, UsernameOrEmail, Password, SetAutoLogin);
		}
		//
		//====================================================================================================
		//
		[Obsolete("Use LoginById(integer) instead", false)]
		public override bool LoginByID(string RecordID, bool SetAutoLogin = false)
		{
			return CP.core.doc.authContext.authenticateById(cpCore, EncodeInteger(RecordID), CP.core.doc.authContext);
		}
		//
		//====================================================================================================
		//
		public override bool LoginByID(int RecordID)
		{
			return CP.core.doc.authContext.authenticateById(cpCore, RecordID, CP.core.doc.authContext);
		}
		//
		//====================================================================================================
		//
		public override bool LoginByID(int RecordID, bool SetAutoLogin)
		{
			return CP.core.doc.authContext.authenticateById(cpCore, RecordID, CP.core.doc.authContext);
			if (!CP.core.doc.authContext.user.AutoLogin)
			{
				CP.core.doc.authContext.user.AutoLogin = true;
				CP.core.doc.authContext.user.save(cpCore);
			}
//INSTANT C# NOTE: Inserted the following 'return' since all code paths must return a value in C#:
			return false;
		}
		//
		//====================================================================================================
		//
		public override bool LoginIsOK(string UsernameOrEmail, string Password)
		{
			return CP.core.doc.authContext.isLoginOK(cpCore, UsernameOrEmail, Password);
		}
		//
		//====================================================================================================
		//
		public override void Logout()
		{
			CP.core.doc.authContext.logout(cpCore);
		}
		//
		//====================================================================================================
		//
		public override string Name
		{
			get
			{
				return CP.core.doc.authContext.user.name;
			}
		}
		//
		//====================================================================================================
		//
		public override bool IsNew
		{
			get
			{
				return CP.core.doc.authContext.visit.MemberNew;
			}
		}
		//
		//====================================================================================================
		//
		public override bool IsNewLoginOK(string Username, string Password)
		{
			string errorMessage = "";
			int errorCode = 0;
			return CP.core.doc.authContext.isNewLoginOK(cpCore, Username, Password, errorMessage, errorCode);
		}
		//
		//====================================================================================================
		//
		public override int OrganizationID
		{
			get
			{
				return CP.core.doc.authContext.user.OrganizationID;
			}
		}
		//
		//====================================================================================================
		//
		public override string Password //Inherits BaseClasses.CPUserBaseClass.Password
		{
			get
			{
				return CP.core.doc.authContext.user.Password;
			}
		}
		//
		//====================================================================================================
		//
		public override bool Recognize(int UserID) //Inherits BaseClasses.CPUserBaseClass.Recognize
		{
			return CP.core.doc.authContext.recognizeById(cpCore, UserID, CP.core.doc.authContext);
		}
		//
		//====================================================================================================
		//
		public override string Username //Inherits BaseClasses.CPUserBaseClass.Username
		{
			get
			{
				return CP.core.doc.authContext.user.Username;
			}
		}
		//
		//=======================================================================================================
		//
		public override string GetProperty(string PropertyName, string DefaultValue = "", int TargetMemberId = 0)
		{
			if (TargetMemberId == 0)
			{
				return CP.core.userProperty.getText(PropertyName, DefaultValue);
			}
			else
			{
				return CP.core.userProperty.getText(PropertyName, DefaultValue, TargetMemberId);
			}
		}
		//
		//=======================================================================================================
		//
		public override void SetProperty(string PropertyName, string Value, int TargetMemberId = 0)
		{
			if (TargetMemberId == 0)
			{
				CP.core.userProperty.setProperty(PropertyName, Value);
			}
			else
			{
				CP.core.userProperty.setProperty(PropertyName, Value, TargetMemberId);
			}
		}
		//
		//=======================================================================================================
		// REFACTOR -- obsolete this and setup the defaultValue type correctly, and add the targetUserId
		//=======================================================================================================
		//
		public override bool GetBoolean(string PropertyName, string DefaultValue = "")
		{
			return CP.core.userProperty.getBoolean(PropertyName, genericController.EncodeBoolean(DefaultValue));
		}
		//
		//=======================================================================================================
		// REFACTOR -- obsolete this and setup the defaultValue type correctly, and add the targetUserId
		//=======================================================================================================
		//
		public override DateTime GetDate(string PropertyName, string DefaultValue = "")
		{
			return CP.core.userProperty.getDate(PropertyName, genericController.EncodeDate(DefaultValue));
		}
		//
		//=======================================================================================================
		// REFACTOR -- obsolete this and setup the defaultValue type correctly, and add the targetUserId
		//=======================================================================================================
		//
		public override int GetInteger(string PropertyName, string DefaultValue = "")
		{
			return CP.core.userProperty.getInteger(PropertyName, genericController.EncodeInteger(DefaultValue));
		}
		//
		//=======================================================================================================
		// REFACTOR -- obsolete this and setup the defaultValue type correctly, and add the targetUserId
		//=======================================================================================================
		//
		public override double GetNumber(string PropertyName, string DefaultValue = "")
		{
			return CP.core.userProperty.getNumber(PropertyName, EncodeNumber(DefaultValue));
		}
		//
		//=======================================================================================================
		//
		public override string GetText(string PropertyName, string DefaultValue = "")
		{
			return CP.core.userProperty.getText(PropertyName, DefaultValue);
		}
		//
		//====================================================================================================
		// REFACTOR -- figure out what this does and rewrite
		//
		public override void track()
		{
			int localId = 0;
			if (true)
			{
				//
				// get the id property, which triggers a track() if it returns o
				//
				localId = Id;
				//If Id = 0 Then
				//    localId = cmc.csv_InsertContentRecordGetID("people", 0)
				//    Call cmc.main_RecognizeMemberByID(localId)
				//End If
			}
		}
		//
		//====================================================================================================
		//
		private void appendDebugLog(string copy)
		{
			//
		}
		//
		//====================================================================================================
		//
		private void tp(string msg)
		{
			//Call appendDebugLog(msg)
		}
#region  IDisposable Support 
		// Do not change or add Overridable to these methods.
		// Put cleanup code in Dispose(ByVal disposing As Boolean).
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		~CPUserClass()
		{
			Dispose(false);
//INSTANT C# NOTE: The base class Finalize method is automatically called from the destructor:
			//base.Finalize();
		}
#endregion
	}

}