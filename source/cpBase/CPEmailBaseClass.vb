'
' documentation should be in a new project that inherits these classes. The class names should be the object names in the actual cp project
'
Namespace Contensive.BaseClasses
    Public MustInherit Class CPEmailBaseClass
        ''' <summary>
        ''' Returns the site's default email from address
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public MustOverride ReadOnly Property fromAddressDefault() As String
        ''' <summary>
        ''' Sends an email to an email address.
        ''' </summary>
        ''' <param name="ToAddress"></param>
        ''' <param name="FromAddress"></param>
        ''' <param name="Subject"></param>
        ''' <param name="Body"></param>
        ''' <param name="SendImmediately"></param>
        ''' <param name="BodyIsHTML"></param>
        ''' <remarks></remarks>
        Public MustOverride Sub send(ByVal ToAddress As String, ByVal FromAddress As String, ByVal Subject As String, ByVal Body As String, Optional ByVal SendImmediately As Boolean = True, Optional ByVal BodyIsHTML As Boolean = True)
        ''' <summary>
        ''' Sends an email that includes all the form elements in the current webpage response.
        ''' </summary>
        ''' <param name="ToAddress"></param>
        ''' <param name="FromAddress"></param>
        ''' <param name="Subject"></param>
        ''' <remarks></remarks>
        Public MustOverride Sub sendForm(ByVal ToAddress As String, ByVal FromAddress As String, ByVal Subject As String)
        ''' <summary>
        ''' Sends an email to everyone in a group list. The list can be of Group Ids or names. Group names in the list can not contain commas.
        ''' </summary>
        ''' <param name="GroupNameOrIdList"></param>
        ''' <param name="FromAddress"></param>
        ''' <param name="Subject"></param>
        ''' <param name="Body"></param>
        ''' <param name="SendImmediately"></param>
        ''' <param name="BodyIsHTML"></param>
        ''' <remarks></remarks>
        Public MustOverride Sub sendGroup(ByVal GroupNameOrIdList As String, ByVal FromAddress As String, ByVal Subject As String, ByVal Body As String, Optional ByVal SendImmediately As Boolean = True, Optional ByVal BodyIsHTML As Boolean = True)
        'Public MustOverride Sub sendGroup(ByVal GroupList As String, ByVal FromAddress As String, ByVal Subject As String, ByVal Body As String, ByVal SendImmediately As Boolean, ByVal BodyIsHTML As Boolean)
        ''' <summary>
        ''' Send a list of usernames and passwords to the account(s) that include the given email address.
        ''' </summary>
        ''' <param name="UserEmailAddress"></param>
        ''' <remarks></remarks>
        Public MustOverride Sub sendPassword(ByVal UserEmailAddress As String)
        ''' <summary>
        ''' Send a system email record. If the EmailIdOrName field contains a number, it is assumed first to be an Id.
        ''' </summary>
        ''' <param name="EmailIdOrName"></param>
        ''' <param name="AdditionalCopy"></param>
        ''' <param name="AdditionalUserID"></param>
        ''' <remarks></remarks>
        Public MustOverride Sub sendSystem(ByVal EmailIdOrName As String, Optional ByVal AdditionalCopy As String = "", Optional ByVal AdditionalUserID As Integer = 0)
        ''' <summary>
        ''' Send an email using the values in a user record.
        ''' </summary>
        ''' <param name="ToUserID"></param>
        ''' <param name="FromAddress"></param>
        ''' <param name="Subject"></param>
        ''' <param name="Body"></param>
        ''' <param name="SendImmediately"></param>
        ''' <param name="BodyIsHTML"></param>
        ''' <remarks></remarks>
        Public MustOverride Sub sendUser(ByVal ToUserID As String, ByVal FromAddress As String, ByVal Subject As String, ByVal Body As String, Optional ByVal SendImmediately As Boolean = True, Optional ByVal BodyIsHTML As Boolean = True)
        'Public MustOverride Sub sendUser(ByVal ToUserID As String, ByVal FromAddress As String, ByVal Subject As String, ByVal Body As String, ByVal SendImmediately As String, ByVal BodyIsHTML As String)
    End Class


End Namespace

