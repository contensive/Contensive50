//========================================================================
// This conversion was produced by the Free Edition of
// Instant C# courtesy of Tangible Software Solutions.
// Order the Premium Edition at https://www.tangiblesoftwaresolutions.com
//========================================================================

//
// documentation should be in a new project that inherits these classes. The class names should be the object names in the actual cp project
//

//INSTANT C# NOTE: Formerly VB project-level imports:
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace Contensive.BaseClasses
{
	public abstract class CPEmailBaseClass
	{
		/// <summary>
		/// Returns the site's default email from address
		/// </summary>
		/// <value></value>
		/// <returns></returns>
		/// <remarks></remarks>
		public abstract string fromAddressDefault {get;}
		/// <summary>
		/// Sends an email to an email address.
		/// </summary>
		/// <param name="ToAddress"></param>
		/// <param name="FromAddress"></param>
		/// <param name="Subject"></param>
		/// <param name="Body"></param>
		/// <param name="SendImmediately"></param>
		/// <param name="BodyIsHTML"></param>
		/// <remarks></remarks>
		public abstract void send(string ToAddress, string FromAddress, string Subject, string Body, bool SendImmediately = true, bool BodyIsHTML = true);
		/// <summary>
		/// Sends an email that includes all the form elements in the current webpage response.
		/// </summary>
		/// <param name="ToAddress"></param>
		/// <param name="FromAddress"></param>
		/// <param name="Subject"></param>
		/// <remarks></remarks>
		public abstract void sendForm(string ToAddress, string FromAddress, string Subject);
		/// <summary>
		/// Sends an email to everyone in a group list. The list can be of Group Ids or names. Group names in the list can not contain commas.
		/// </summary>
		/// <param name="GroupNameOrIdList"></param>
		/// <param name="FromAddress"></param>
		/// <param name="Subject"></param>
		/// <param name="Body"></param>
		/// <param name="SendImmediately"></param>
		/// <param name="BodyIsHTML"></param>
		/// <remarks></remarks>
		public abstract void sendGroup(string GroupNameOrIdList, string FromAddress, string Subject, string Body, bool SendImmediately = true, bool BodyIsHTML = true);
		//Public MustOverride Sub sendGroup(ByVal GroupList As String, ByVal FromAddress As String, ByVal Subject As String, ByVal Body As String, ByVal SendImmediately As Boolean, ByVal BodyIsHTML As Boolean)
		/// <summary>
		/// Send a list of usernames and passwords to the account(s) that include the given email address.
		/// </summary>
		/// <param name="UserEmailAddress"></param>
		/// <remarks></remarks>
		public abstract void sendPassword(string UserEmailAddress);
		/// <summary>
		/// Send a system email record. If the EmailIdOrName field contains a number, it is assumed first to be an Id.
		/// </summary>
		/// <param name="EmailIdOrName"></param>
		/// <param name="AdditionalCopy"></param>
		/// <param name="AdditionalUserID"></param>
		/// <remarks></remarks>
		public abstract void sendSystem(string EmailIdOrName, string AdditionalCopy = "", int AdditionalUserID = 0);
		/// <summary>
		/// Send an email using the values in a user record.
		/// </summary>
		/// <param name="ToUserID"></param>
		/// <param name="FromAddress"></param>
		/// <param name="Subject"></param>
		/// <param name="Body"></param>
		/// <param name="SendImmediately"></param>
		/// <param name="BodyIsHTML"></param>
		/// <remarks></remarks>
		public abstract void sendUser(string ToUserID, string FromAddress, string Subject, string Body, bool SendImmediately = true, bool BodyIsHTML = true);
		//Public MustOverride Sub sendUser(ByVal ToUserID As String, ByVal FromAddress As String, ByVal Subject As String, ByVal Body As String, ByVal SendImmediately As String, ByVal BodyIsHTML As String)
	}


}

