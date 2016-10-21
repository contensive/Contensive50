
<%
'
'==============================================================================
'
'	This header file contains site wide definitions and is required
'	by all asp documents in the site.
'
'	This page and its contents are copyright by Kidwell McGowan Associates.
'
'=============================================================================
'
Dim ccLib
'
'========================================================================
' ----- Initialize the API
'========================================================================
'
Sub InitLibAPI

on error goto 0
    '
    Dim NewKey
    Dim TotalBytes
    '
    ' setup IIS Response
    '
    Response.CacheControl = "no-cache"
    ' removed so a site can turn cachecontrol back on with 'private' for ie bug save-target-as with an SSL connection
    ' Response.AddHeader "Pragma", "no-cache"
    Response.Expires = -1
    Response.Buffer = True
    '
    ' Create Contensive object
    '
    Set ccLib = Server.CreateObject( "ccWeb3.MainClass" )
    '
    ' Setup Contensive from IIS Request
    '
    ccLib.ServerPathPage = CStr( Request.ServerVariables("SCRIPT_NAME"))
    ccLib.ServerReferrer = Request.ServerVariables("HTTP_REFERER")
    ccLib.ServerHost = Request.ServerVariables("SERVER_NAME")
    'ccLib.ServerCookies = Request.Cookies
    ccLib.BrowserLanguage = CStr( Request.ServerVariables("HTTP_ACCEPT_LANGUAGE"))
    ccLib.ServerPageSecure = cBool( Request.ServerVariables("SERVER_PORT_SECURE"))
    ccLib.PhysicalWWWPath = Server.MapPath("/")
    ccLib.PhysicalccLibPath = Server.MapPath( "/ccLib" )
    ccLib.VisitRemoteIP = CStr( Request.ServerVariables("REMOTE_ADDR"))
    ccLib.VisitBrowser = CStr( Request.ServerVariables("HTTP_USER_AGENT"))
    'ccLib.VisitHTTPVia = CStr( Request.ServerVariables("HTTP_VIA"))
    'ccLib.VisitHTTPFrom = CStr( Request.ServerVariables("HTTP_From"))
    ccLib.HTTP_Accept = CStr( Request.ServerVariables("HTTP_ACCEPT"))
    ccLib.HTTP_Accept_charset = CStr( Request.ServerVariables("HTTP_ACCEPT_CHARSET"))
    ccLib.HTTP_Profile = CStr( Request.ServerVariables("HTTP_PROFILE"))
    ccLib.HTTP_X_Wap_Profile = CStr( Request.ServerVariables("HTTP_X_WAP_PROFILE"))
    '
    ' Create ServerQueryString
    '
    For Each NewKey In Request.QueryString
        ccLib.ServerQueryString = ccLib.ServerQueryString & "&" & CStr(NewKey) & "=" & Server.URLEncode( Request.QueryString(NewKey))
        ccLib.ReadStreamBinaryRead = ccLib.ReadStreamBinaryRead Or (ucase(CStr(NewKey)) = "REQUESTBINARY")
    Next
    If Len( ccLib.ServerQueryString ) > 0 then
        ccLib.ServerQueryString = Mid( ccLib.ServerQueryString, 2 )
    End If
    '
    ' Create ServerForm
    '
    If ccLib.ReadStreamBinaryRead Then
        '
        ' binary multipart form 
        '
        TotalBytes = Request.TotalBytes
        If TotalBytes > 0 Then
            Server.ScriptTimeout = 1800
            ccLIb.ServerBinaryHeader = Request.BinaryRead(TotalBytes - 1)
        End If
    Else
        '
        ' non-binary form, create Form String
        '
        For Each NewKey In Request.Form
            ccLib.ServerForm = ccLib.ServerForm & "&" & CStr(NewKey) & "=" & Server.URLEncode( Request.Form(NewKey))
        Next
        If Len( ccLib.ServerForm ) > 0 then
            ccLib.ServerForm = Mid( ccLib.ServerForm, 2 )
        End If
    End If
    '
    ' Create ServerCookie string
    '
    For Each NewKey In Request.Cookies
        ccLib.ServerCookies = ccLib.ServerCookies & "&" & CStr(NewKey) & "=" & Request.Cookies(NewKey)
    Next
    If Len( ccLib.ServerCookies ) > 0 then
        ccLib.ServerCookies = Mid( ccLib.ServerCookies, 2 )
    End If
    End Sub
'
'========================================================================
' ----- Close the stream
'========================================================================
'
Sub CloseStream()
    Call ccLib.CloseStream()
    End Sub
'
'========================================================================
'   ccLib.WriteStream a line (end in a <br>)
'========================================================================
'
Sub PrintPopupPage( URL)
    Call ccLib.PrintPopupPage( URL)
    End Sub
'
'========================================================================
'   ccLib.WriteStream a line (end in a <br>)
'========================================================================
'
Sub WriteStreamLine(Thing)
    Call ccLib.WriteStreamLine(Thing)
    End Sub
'
Sub rwl(Thing)
    Call ccLib.WriteStreamLine(Thing)
    End Sub
'
'========================================================================
'   shortcut to ccLib.rw
'========================================================================
'
Sub WriteStream(Thing)
    Call ccLib.WriteStream(Thing)
    End Sub
'
Sub rw(Thing)
    Call ccLib.WriteStream(Thing)
    End Sub
'
'========================================================================
'   shortcut to write a comment
'========================================================================
'
Sub WriteStreamComment(Thing)
    Call ccLib.WriteStreamComment(Thing)
    End Sub
'
Sub rc(Thing)
    Call ccLib.WriteStreamComment(Thing)
    End Sub
'
'========================================================================
' Modify a string to be printed through the HTML stream
'
'   changes:
'   convert all characters to HTML equivelents, except:
'       convert 0x10 to <br>
'       remove 0x13
'
'
'========================================================================
'
Function EncodeCRLF(Source)
    EncodeCRLF = ccLib.EncodeCRLF(Source)
    End Function
'
Function EncodeHTML(Source)
    EncodeHTML = ccLib.EncodeHTML(Source)
    End Function
'
Function CommonHTMLEncode(Source)
    CommonHTMLEncode = ccLib.EncodeHTML(Source)
    End Function
'
'========================================================================
'   SQLTextEncode
'       encode a text variable to go in an sql expression
'========================================================================
'
Function GetVirtualFilename( Contentname, FieldName, RecordID )
    GetVirtualFilename = ccLib.GetVirtualFilename( Contentname, FieldName, RecordID )
    End Function
'
'========================================================================
'   SQLTextEncode
'       encode a text variable to go in an sql expression
'========================================================================
'
Function EncodeSQLText(var)
    EncodeSQLText = ccLib.EncodeSQLText(var)
    End Function
'
Function sqlEncodeText(var)
    sqlEncodeText = ccLib.EncodeSQLText(var)
    End Function
'
'========================================================================
'   SQLEncodeDate
'       encode a date variable to go in an sql expression
'========================================================================
'
Function EncodeSQLDate(var)
    EncodeSQLDate = ccLib.EncodeSQLDate(var)
    End Function
'
Function sqlEncodeDate(var)
    sqlEncodeDate = ccLib.EncodeSQLDate(var)
    End Function
'
'========================================================================
'   SQLEncodeNumber
'       encode a number variable to go in an sql expression
'========================================================================
'
Function EncodeSQLNumber(var)
    EncodeSQLNumber = ccLib.EncodeSQLNumber(var)
    End Function
'
Function sqlEncodeNumber(var)
    sqlEncodeNumber = ccLib.EncodeSQLNumber(var)
    End Function
'
'========================================================================
'   SQLEncodeBoolean
'       encode a boolean variable to go in an sql expression
'========================================================================
'
Function EncodeSQLBoolean(var)
    EncodeSQLBoolean = ccLib.EncodeSQLBoolean(var)
    End Function
'
Function sqlEncodeBoolean(var)
    sqlEncodeBoolean = ccLib.EncodeSQLBoolean(var)
    End Function
'
'========================================================================
'   SQLEncode
'       encode a variable to go in an sql expression
'       NOT supported
'========================================================================
'
Function EncodeSQL(var)
    EncodeSQL = ccLib.EncodeSQL(var)
    End Function
'
'========================================================================
'   Modify a string so it can be saved in a database
'       plus, add a quote mark to the front and back
'       this is needed in SQL based connections
'========================================================================
'
Function DbEncode(Source)
    DbEncode = ccLib.EncodeDbText(Source)
    End Function
'
'========================================================================
' Remove this when it is no longer used
' old routine - this was actually an HTML encode
'========================================================================
'
Function EncodeURL(Source)
    EncodeURL = ccLib.EncodeURL(Source)
    End Function
'
Function URLEncode(Source)
    URLEncode = ccLib.EncodeURL(Source)
    End Function
'
'========================================================================
'   Log a status message
'========================================================================
'
Function LogStatus(StatusMessage)
    LogStatus = ccLib.LogStatus(StatusMessage)
    End Function
'
'========================================================================
'   Test Point
'       If testing print a string, value paior
'========================================================================
'
Public Sub TestPoint(Thing)
    Call ccLib.TestPoint(Thing)
    End Sub
'
Public Sub TestPointEnter(Thing)
    Call ccLib.TestPointEnter(Thing)
    End Sub
'
Public Sub TestPointExit(Thing)
    Call ccLib.TestPointExit(Thing)
    End Sub
'
Public Sub TP2(Thing)
    Call ccLib.TestPoint(Thing)
    End Sub
'
'=============================================================================
'   CRCOK
'       check that raw is a number, and has good crc
'       returns true if all ok, false otherwise
'=============================================================================
'
Function CRCOK(Raw, EncodeMod)
    CRCOK = ccLib.CRCOK(Raw, EncodeMod)
    End Function
'
'========================================================================
'   Encode a number to present on a URL
'       returns a string
'       goal is to make it difficult, without using the session
'
'   result is vvvvvvvvvvxxxxxxxxxxc
'       v =((raw*EncodeFactor)+EncodeNumberBase), 10 digit
'       x =int(now()*100000), 10 digit, saved as LastVisitTime in visit record
'       c = crc check value, 1 digit set so (EncodeNumber mod EncodeStringMod) = 0
'
'   LicenseKey is abcccccccccd
'       a = EncodeFactor (single digit value)
'       b = EncodeMod (single digit value)
'       c = EncodeBase (9 digit value)
'       d = trash
'
'   These values must be const, defined in the same page (values are suggestions)
'       EncodeMod=6
'       EncodeBase = 132987309
'       EncodeFactor = 7
'========================================================================
'
Function EncodeNumber(Raw, EncodeTime)
    EncodeNumber = ccLib.EncodeNumber(Raw, EncodeTime)
    End Function
'
'========================================================================
'   Decode a value from an encodestring value
'       result is 0 if there was a decode error
'========================================================================
'
Function DecodeNumber(Raw)
    DecodeNumber = ccLib.DecodeNumber(Raw)
    End Function
'
'========================================================================
'   Decode a value from an encodestring value
'       result is 0 if there was a decode error
'========================================================================
'
Function DecodeTime(Raw)
    DecodeTime = ccLib.DecodeTime(Raw)
    End Function
'
'========================================================================
' Decode a value from an encodestring value
'   result is 0 if there was a decode error
'   Error info is collected and object cleared.
'   Potential error during reporting is cleared with end sub
'========================================================================
'
Sub ReportError(Message)
    Call ccLib.ReportError(Message)
    End Sub
'
Sub ErrorReport(Message)
    Call ccLib.ReportError(Message)
    End Sub
'
'========================================================================
' Page Redirect
'========================================================================
'
Sub RedirectByLink( Link )
    Call ccLib.RedirectByLink( Link )
    End Sub
'
Sub RedirectApplicationByRecord(Contentname, RecordID)
    Call ccLib.RedirectApplicationByRecord(Contentname, RecordID)
    End Sub
'
Sub RedirectByLink( Link )
    Call ccLib.RedirectByLink( Link )
    End Sub
'
Sub RedirectByRecord(Contentname, RecordID)
    Call ccLib.RedirectByRecord(ContentName, RecordID)
    End Sub 
'
Sub TableLinkRedirect(TableName, RecordID)
    Call ccLib.RedirectByRecord(TableName, RecordID)
    End Sub
'
Sub Relay(TableName, RecordID)
    Call ccLib.RedirectByRecord(TableName, RecordID)
    End Sub
'
'========================================================================
' Read/Write/Delete files
'========================================================================
'
' ----- Read files
'
Function ReadVirtualFile(Filename)
    ReadVirtualFile = ccLib.ReadVirtualFile(Filename)
    End Function
'
Function FileRead(Filename)
    FileRead = ccLib.FileRead(Filename)
    End Function
'
Function ReadFile(Filename)
    ReadFile = ccLib.FileRead(Filename)
    End Function
'
' ----- Save files
'
Sub SaveVirtualFile(Filename, filecontent)
    Call ccLib.SaveVirtualFile(Filename, filecontent)
    End Sub
'
Sub FileSave(Filename, filecontent)
    Call ccLib.FileSave(Filename, filecontent)
    End Sub
'
Sub SaveFile(Filename, filecontent)
    Call ccLib.FileSave(Filename, filecontent)
    End Sub
'
' ----- Delete files
'
Sub DeleteFile( Filename )
    Call ccLib.DeleteFile( Filename )
    End Sub
'
Sub DeleteVirtualFile( Filename )
    Call ccLib.DeleteVirtualFile( Filename )
    End Sub
'
'========================================================================
' Generate a form drop down menu form element from a database table
'   returns a string
'
'   MenuName is the name of the input select box
'   CurrentValue is the ID of the matching entry
'   Tablename is the name of hte table to collect the input from
'   Whereclause is how to select the input
'========================================================================
'
Function GetFormInputCheckBox( MenuName, CurrentValue )
	GetFormInputCheckBox = ccLib.GetFormInputCheckBox( MenuName, CurrentValue )
	end Function
'
Function GetFormInputText( MenuName, CurrentValue, Height, Width )
	GetFormInputText = ccLib.GetFormInputText( MenuName, CurrentValue, Height, Width )
	end Function
'
Sub PrintFormInputSelect(MenuName, CurrentValue, TableName, WhereClause)
	Call ccLib.PrintFormInputSelect(MenuName, CurrentValue, TableName, WhereClause)
	end Sub
'
Sub PrintFormSelect(MenuName, CurrentValue, TableName, WhereClause)
	Call ccLib.PrintFormInputSelect(MenuName, CurrentValue, TableName, WhereClause)
	end Sub
'
Function GetFormInputSelect(MenuName, CurrentValue, TableName, WhereClause)
	GetFormInputSelect = ccLib.GetFormInputSelect(MenuName, CurrentValue, TableName, WhereClause)
	end Function
'
Function GetFormSelect(MenuName, CurrentValue, TableName, WhereClause)
	GetFormSelect = ccLib.GetFormInputSelect(MenuName, CurrentValue, TableName, WhereClause)
	end Function
'
Function GetFormSelectString(MenuName, CurrentValue, TableName, WhereClause)
    GetFormSelectString = ccLib.GetFormInputSelect(MenuName, CurrentValue, TableName, WhereClause)
    End Function
'
Function CommonFormDropDown(MenuName, CurrentValue, TableName, WhereClause)
    CommonFormDropDown = ccLib.GetFormInputSelect(MenuName, CurrentValue, TableName, WhereClause)
    End Function
'
'========================================================================
' Get a number from request
'   if empty, returns null
'   if not a value, returns null
'   if RequestBlock true, tries only querystring
'========================================================================
'
Function ReadStreamNumber(key)
    ReadStreamNumber = ccLib.ReadStreamNumber(key)
    End Function
'
Function CommonRequestNumber(key)
    CommonRequestNumber = ccLib.ReadStreamNumber(key)
    End Function
'
'========================================================================
' Get a Date from request
'   if empty, returns null
'   if not a date, returns null
'   if RequestBlock true, tries only querystring
'========================================================================
'
Function ReadStreamDate(key)
    ReadStreamDate = ccLib.ReadStreamDate(key)
    End Function
'
Function CommonRequestDate(key)
    CommonRequestDate = ccLib.ReadStreamDate(key)
    End Function
'
'========================================================================
' Get a Text string from request
'   if empty, returns null
'   if RequestBlock true, tries only querystring
'========================================================================
'
Function ReadStreamText(key)
    ReadStreamText = ccLib.ReadStreamText(key)
    End Function
'
Function CommonRequestText(key)
    CommonRequestText = ccLib.ReadStreamText(key)
    End Function
'
'========================================================================
' Get a string from request
'   if empty, returns null
'   if RequestBlock true, tries only querystring
'========================================================================
'
Function ReadStreamString(key)
    ReadStreamString = ccLib.ReadStreamText(key)
    End Function
'
Function CommonRequestString(key)
    CommonRequestString = ccLib.ReadStreamText(key)
    End Function
'
'========================================================================
' Get a boolean from request
'   if empty, returns null
'   if RequestBlock true, tries only querystring
'   if "ON", "YES", "TRUE" or non-0 number returns true, otherwise false
'========================================================================
'
Function ReadStreamBoolean(key)
    ReadStreamBoolean = ccLib.ReadStreamBoolean(key)
    End Function
'
Function CommonRequestBoolean(key)
    CommonRequestBoolean = ccLib.ReadStreamBoolean(key)
    End Function
'
'========================================================================
' Output the common font tag as a string
'   note that it must be manually ended with a </font>
'========================================================================
'
Function FontTagString()
    FontTagString = CommonFont
    End Function
'
'========================================================================
' Print a font tag
'   note that it must be manually ended with a </font>
'========================================================================
'
Sub PrintFontTag()
    ccLib.rw (CommonFont)
    End Sub
'
'========================================================================
' End the process early
'========================================================================
'
Sub HaltPage(Message)
    Call ccLib.HaltPage(Message)
    End Sub
'
Sub CommonHalt(Message)
    Call ccLib.HaltPage(Message)
    End Sub
'
'========================================================================
'   Display an icon with a link to the login form/cclib.net/admin area
'========================================================================
'
Sub PrintLoginLink()
    Call ccLib.PrintLoginLink
    End Sub
'
Sub CommonLoginLink()
    Call ccLib.PrintLoginLink
    End Sub
'
'========================================================================
'   Close all open objects in preparation for the page to terminate
'   and print the login ling
'========================================================================
'
Sub ClosePage()
    Call ccLib.ClosePage( true, true )
    End Sub
'
Sub PageClose()
    Call ccLib.ClosePage( true, true )
    End Sub
'
Sub CommonClosePage()
    Call ccLib.ClosePage( true, true )
    End Sub
'
'========================================================================
'   Close all open objects in preparation for the page to terminate
'========================================================================
'
Sub CommonClose()
    Call ccLib.ClosePage( false, false )
    End Sub
'
'========================================================================
' Database Interface
'========================================================================
'
Sub QueueSQL( DataSource, sql )
    Call ccLib.QueueSQL( DataSource, sql )
    End Sub
'
Sub CacheSQL( DataSource, sql )
    Call ccLib.QueueSQL( DataSource, sql )
    End Sub
'
Function RunSQL( DataSource, sql )
    Set RunSQL = ccLib.ExecuteSQL( DataSource, sql )
    End Function
'
Function CommonConnExecute(sql)
    Set CommonConnExecute = ccLib.ExecuteSQL("Default",sql)
    End Function
'
Function ccRunSQL(sql)
    Set ccRunSQL = ccLib.ExecuteSQL("Default",sql)
    End Function
'
' ----- Open Table
'
Function OpenTable(TableName, WhereClause)
    Set OpenTable = ccLib.OpenTable(TableName, WhereClause)
    End Function
'
Function TableOpen(TableName, WhereClause)
    Set TableOpen = ccLib.OpenTable(TableName, WhereClause)
    End Function
'
' ----- Alternate name
'
Function OpenContentTable(TableName, WhereClause)
    Set OpenContentTable = ccLib.OpenTable(TableName, WhereClause)
    End Function
'
'========================================================================
' Depricated - Get a Contents ID from the content pointer
'========================================================================
'
Function GetContentIDByPointer(ContentPointer)
    GetContentIDByPointer = ccLib.GetContentIDByPointer(ContentPointer)
    End Function
'
'========================================================================
' Get ContentID from the content name
'========================================================================
'
Function GetContentID(ContentName)
    GetContentID = ccLib.GetContentID(ContentName)
    End Function
'
'========================================================================
' Get ContentName from the contentID
'========================================================================
'
Function GetContentNameByID(ContentID)
    GetContentNameByID = ccLib.GetContentNameByID(ContentID)
    End Function
'
'========================================================================
' Get DataSourceName from the ContentName
'========================================================================
'
Function GetContentDataSource(ContentName)
    GetContentDataSource = ccLib.GetContentDataSource(ContentName)
    End Function
'
'========================================================================
' Get a Contents Tablename
'========================================================================
'
Function GetContentTablename(ContentName)
    GetContentTablename = ccLib.GetContentTablename(ContentName)
    End Function
'
Function GetTablenameByPointer(ContentPointer)
    GetTablenameByPointer = ccLib.GetTablenameByPointer(ContentPointer)
    End Function
'
Function ContentTable(ContentPointer)
    GetContentTable = ccLib.GetTablenameByPointer(ContentPointer)
    End Function
'
'========================================================================
' Get a tables first ContentID from Tablename
'========================================================================
'
Function GetContentIDByTablename(TableName)
    GetContentIDByTablename = ccLib.GetContentIDByTablename(TableName)
    End Function
'
Function GetCID(TableName)
    GetCID = ccLib.GetContentIDByTablename(TableName)
    End Function
'
'========================================================================
' Get a Records ContentControlID
'========================================================================
'
' Function GetContentControlID(TableName, RecordID)
'     GetContentControlID = ccLib.GetContentControlID(TableName, RecordID)
'     End Function
'
'========================================================================
' PrintAdminContentLink( ContentID, RecordID, WhereClause )
'
'   ContentID   The content for this link
'   RecordID    The ID of the record in the Table, null to add a record
'   WhereClause The sql equivalent used to select the record.
'           translates to (name0=value0)&(name1=value1).. pairs separated by &
'========================================================================
'
Sub PrintRecordEditLink(ContentName, RecordID )
    Call ccLib.PrintRecordEditLink(ContentName, RecordID )
    End Sub
'
Sub PrintRecordAddLink(ContentName, Criteria )
    Call ccLib.PrintRecordAddLink(ContentName, Criteria )
    End Sub
'
Sub PrintRecordEditLinkByContent(ContentID, RecordID, WhereClause)
    Call ccLib.PrintRecordEditLinkByContent(ContentID, RecordID, WhereClause)
    End Sub
'
Sub PrintAdminContentLink(ContentID, RecordID, WhereClause)
    Call ccLib.PrintRecordEditLinkByContent(ContentID, RecordID, WhereClause)
    End Sub
'
'========================================================================
' PrintAdminTableLink( Tablename, RecordID, WhereClause )
'
'   Identical to PrintAdminContentLink except it links to the entire table.
'   The major difference is if the add button is displayed, an add button
'   is included for each content area that the visitor has access
'========================================================================
'
Sub PrintRecordEditLinkByTable(TableName, RecordID, WhereClause)
    Call ccLib.PrintRecordEditLinkByTable(TableName, RecordID, WhereClause)
    End Sub
'
Sub PrintAdminTableLink(TableName, RecordID, WhereClause)
    Call ccLib.PrintRecordEditLinkByTable(TableName, RecordID, WhereClause)
    End Sub
'
Sub CommonAdminLink2(TableName, RecordID, WhereClause)
    Call ccLib.PrintRecordEditLinkByTable(TableName, RecordID, WhereClause)
    End Sub
'
' -------------------------------
'
Sub CommonAdminLink(TableName, RecordID)
    Call ccLib.PrintRecordEditLinkByTable(TableName, RecordID, "(PathPage='" & Path & PageName & "')")
    End Sub
'
' -------------------------------
'
' Sub PrivatePrintContentLinkButton(ContentID, RecordID, WhereClause)
'    Call ccLib.PrivatePrintContentLinkButton(ContentID, RecordID, WhereClause)
'    End Sub
'
'========================================================================
' Check for a table in the database
'   if missing, create the table
'========================================================================
'
Sub MakeTable(TableName)
    End Sub
'
Sub TableMake(TableName)
    End Sub
'
Sub CommonTableCheck(TableName)
    End Sub
'
Sub TableCheck(TableName)
    End Sub
'
Sub MakeTableField(TableName, FieldName, FieldType)
    End Sub
'
Sub CommonTableFieldCheck(TableName, FieldName, FieldType)
    End Sub
'
'========================================================================
' CommonEmailForm
'   sends an email with the contents of a form
'========================================================================
'
Sub SendFormEmail(SendTo, SendFrom, SendSubject)
    Call ccLib.SendFormEmail(SendTo, SendFrom, SendSubject)
    End Sub
'
Sub CommonEmailForm(SendTo, SendFrom, SendSubject)
    Call ccLib.SendFormEmail(SendTo, SendFrom, SendSubject)
    End Sub
'
'========================================================================
' CommonCreateKey
'   Returns a random long value to use as a createkey
'========================================================================
'
Function CommonCreateKey()
    CommonCreateKey = ccLib.GetCreateKey()
    End Function
'
'========================================================================
' Delete a Record from a content area
'   Deletes records from a content area.
'========================================================================
'
Sub DeleteContentRecord(ContentName, Criteria)
    Call ccLib.DeleteContentRecord(ContentName, Criteria)
    End Sub
'
'========================================================================
' Delete a Record from a table
'   Deletes records from a content area.
'========================================================================
'
Sub DeleteTable(TableName, WhereClause)
    Call ccLib.DeleteTable(TableName, WhereClause)
    End Sub
'
'========================================================================
' Insert a record in a content area
'   Inserts a record into a content area.
'   Returns the ID of the record, null if error
'========================================================================
'
Function InsertContentRecord(ContentName)
    InsertContentRecord = ccLib.InsertContentRecord(ContentName)
    End Function
'
Function InsertContentRecordByID(ContentID)
    InsertContentRecordByID = ccLib.InsertContentRecordByID(ContentID)
    End Function
'
Function ContentInsert(ContentID)
    ContentInsert = ccLib.InsertContentRecordByID(ContentID)
    End Function
'
Function CommonContentInsert(ContentID)
    CommonContentInsert = ccLib.InsertContentRecordByID(ContentID)
    End Function
'
'========================================================================
' UpdateContent
'   Updates a record in a content table.
'   Returns the ID of the record, null if error
'
'   ContentID is the content
'   WhereClause is the condition (should be ID=#)
'   Fieldcount is the number of fields to update
'   FieldArray( fieldCount, 3 )
'       FieldArray( #, 0 ) is the FieldName
'       FieldArray( #, 1 ) is the Fieldtype (see defines)
'       FieldArray( #, 2 ) is the FieldValue
'========================================================================
'
Sub UpdateContentRecordByID(ContentID, WhereClause, FieldCount, FieldArray)
    Call ccLib.UpdateContentByID(ContentID, WhereClause, FieldCount, FieldArray)
    End Sub
'
Sub ContentUpdate(ContentID, WhereClause, FieldCount, FieldArray)
    Call ccLib.UpdateContentRecordByID(ContentID, WhereClause, FieldCount, FieldArray)
    End Sub
'
Sub CommonContentUpdate(ContentID, WhereClause, FieldCount, FieldArray)
    Call ccLib.UpdateContentRecordByID(ContentID, WhereClause, FieldCount, FieldArray)
    End Sub
'
'========================================================================
' TableInsert
'   Inserts a record into a table.
'   Returns the ID of the record, null if error
'========================================================================
'
Function InsertTable(TableName)
    InsertTable = ccLib.InsertTable(TableName)
    End Function

Function CommonTableInsert(TableName)
    CommonTableInsert = ccLib.InsertTable(TableName)
    End Function
'
'========================================================================
' ContentIncrementField( ContentID, RecordID, Fieldname )
'========================================================================
'
Sub IncrementContentField(ContentID, RecordID, FieldName)
    Call ccLib.IncrementContentField(ContentID, RecordID, FieldName)
    End Sub
'
Sub ContentFieldIncrement(ContentID, RecordID, FieldName)
    Call ccLib.IncrementContentField(ContentID, RecordID, FieldName)
    End Sub
'
Sub ContentIncrementField(ContentID, RecordID, FieldName)
    Call ccLib.IncrementContentField(ContentID, RecordID, FieldName)
    End Sub
'
'========================================================================
' TableFieldIncrement( TableName, RecordID, Fieldname )
'========================================================================
'
Sub IncrementTableField(TableName, RecordID, FieldName)
    Call ccLib.IncrementTableField(TableName, RecordID, FieldName)
    End Sub
'
Sub CommonIncrementField(TableName, RecordID, FieldName)
    Call ccLib.IncrementTableField(TableName, RecordID, FieldName)
    End Sub
'
'========================================================================
' If first Run of the application, return true
'========================================================================
'
Function CheckTable(TableName)
    CheckTable = ccLib.CheckTable(TableName)
    End Function
'
Function TableUpdateCheck(TableName)
    TableUpdateCheck = ccLib.CheckTable(TableName)
    End Function
'
Function CheckDb(TableName)
    CheckDb = ccLib.CheckTable(TableName)
    End Function
'
'========================================================================
'   SendMemberEmail()
'   Returns "" if send is OK, otherwise it returns an error message
'========================================================================
'
Function EmailMemberSend(EMemberID, EFrom, ESubject, EBody)
    EmailMemberSend = ccLib.SendMemberEmail(EMemberID, EFrom, ESubject, EBody)
    End Function
'
Function SendMemberEmail(EMemberID, EFrom, ESubject, EBody)
    SendMemberEmail = ccLib.SendMemberEmail(EMemberID, EFrom, ESubject, EBody)
    End Function
'
'========================================================================
'   EmailSend()
'
'   Sends an email without message merging
'   Returns "" if send is OK, otherwise it returns an error message
'========================================================================
'
Function SendEmail(ToAddress, FromAddress, SubjectMessage, BodyMessage)
    SendEmail = ccLib.SendEmail(ToAddress, FromAddress, SubjectMessage, BodyMessage)
    End Function
'
Function EmailSend(ToAddress, FromAddress, SubjectMessage, BodyMessage)
    EmailSend = ccLib.SendEmail(ToAddress, FromAddress, SubjectMessage, BodyMessage)
    End Function
'
'========================================================================
'   CacheSend()
'
'   Sends an email without message merging after the page is complete
'========================================================================
'
Sub CacheEmail(ToAddress, FromAddress, SubjectMessage, BodyMessage)
    Call ccLib.CacheEmail(ToAddress, FromAddress, SubjectMessage, BodyMessage)
    End Sub
'
'========================================================================
' MemberMerge( MemberID, Message )
'========================================================================
'
Function EncodeMessagingByMemberID(SourceMemberID, Source)
    EncodeMessagingByMemberID = ccLib.EncodeContent(Source, SourceMemberID,,false,,true,true,false,true)
    End Function
'
Function MemberMerge(SourceMemberID, Source)
    MemberMerge = ccLib.EncodeContent(Source, SourceMemberID,,false,,true,true,false,true)
    End Function
'
Function EncodeMessaging(Source)
    EncodeMessaging = ccLib.EncodeContent(Source, ccLib.MemberID,,false,,true,true,false,true)
    End Function
'
Function MessageMerge(Source)
    MessageMerge = ccLib.EncodeContent(Source, ccLib.MemberID,,false,,true,true,false,true)
    End Function
'
'=============================================================================
' ----- Return the value associated with the name given
'   If Name is not given, returns ""
'   If Name present but no value, returns true (as if Name=true)
'   If Name = Value, it returns value
'=============================================================================
'
Function GetNameValue(Tag, Name)
    GetNameValue = ccLib.GetNameValue(Tag, Name)
    End Function
'
'=============================================================================
' Automatic Updates
'   These are system changes that need to be implimented automatically
'=============================================================================
'
Sub CommonAutoUpdates()
    Call ccLib.CommonAutoUpdates
    End Sub
'
'========================================================================
' Opens the content specified and returns the data in a recordset
'
'   Returns the recordset if all is OK
'   Not IsObject if there is a problem
'========================================================================
'
Function OpenContentByID(ContentID, WhereClause)
    Set OpenContentByID = ccLib.OpenContentByID(ContentID, WhereClause)
    End Function
'
Function ContentOpen(ContentName, WhereClause)
	If IsNumeric( ContentName ) then
		Set ContentOpen = ccLib.ContentRecordOpenByID(ContentName, WhereClause)
		Call ErrorReport( "ContentOpen called with ContentID, needs to be converted" )
	else
		Set ContentOpen = ccLib.ContentOpen(ContentName, WhereClause)
		end if
    End Function
'
'========================================================================
' Return a panel with the input as center
'========================================================================
'
Function MakePanel(Panel, ColorBase, ColorHilite, ColorShadow, Width, Padding)
    MakePanel = ccLib.MakePanel(Panel, ColorBase, ColorHilite, ColorShadow, Width, Padding)
    End Function
'
'========================================================================
' Print the text Field Edit Panel
'========================================================================
'
Sub PrintFieldEditDialog()
    Call ccLib.PrintFieldEditDialog
    End Sub
'
'========================================================================
' Print the profile panel at the bottom of the page
'========================================================================
'
Sub PrintToolsPanel()
    Call ccLib.PrintToolsPanel
    End Sub
'
'========================================================================
' Create a record in the ccFields table
'
'   Based only on the FieldName and FieldType
'========================================================================
'
Sub CreateContentField(ContentTableID, ContentFieldName, ContentFieldType, DefaultSortOrder)
    Call ccLib.CreateContentField(ContentTableID, ContentFieldName, ContentFieldType, DefaultSortOrder)
    End Sub
'
'========================================================================
' ----- Create a filename for the Virtual Directory
'   Do not allow spaces.
'========================================================================
'
Function VirtualFilename(TableName, FieldName, RecordID)
    VirtualFilename = ccLib.VirtualFilename(TableName, FieldName, RecordID)
    End Function
'
Function GetTextFilename(TableName, FieldName, RecordID)
    GetTextFilename = ccLib.VirtualFilename(TableName, FieldName, RecordID)
    End Function
'
'========================================================================
' Track the current content
'	tracking includes both setting current PublicURI and topicHabits
'========================================================================
'
Sub TrackContent( ContentName, RecordID )
    Call ccLib.TrackContent( ContentName, RecordID )
    End Sub
'
Sub TrackContentSet( CSPointer )
    Call ccLib.TrackContentSet( CSPointer )
    End Sub
'
Sub ContentWatch( ContentName, RecordID )
    Call ccLib.TrackContent( ContentName, RecordID )
    End Sub
'
'=============================================================================
' Imports the named table into the content system
'=============================================================================
'
Sub TableImport(TableName)
    Call ccLib.TableImport(TableName)
    End Sub
'
'=============================================================================
' Add an entry to the ccFields table
'=============================================================================
'
Sub CommonAddContentField(FieldName, ContentControlID, FieldType, Caption, ReadOnly, LookupContentID, RedirectContentID, RedirectPath, RedirectID, textBuffered, EditSortPriority, AdminOnly, DeveloperOnly)
    Call ccLib.CommonAddContentField(FieldName, ContentControlID, FieldType, Caption, ReadOnly, LookupContentID, RedirectContentID, RedirectPath, RedirectID, textBuffered, EditSortPriority, AdminOnly, DeveloperOnly)
    End Sub

'
'========================================================================
' Print a content blocks headline
'========================================================================
'
Sub PrintTitle(Title)
    Call ccLib.PrintTitle(Title)
    End Sub
'
'========================================================================
' Error handling Interface
'========================================================================
'
Sub AddErrorMessage( Message )
    Call ccLib.AddErrorMessage( Message )
    End Sub
'
Function GetErrorMessage()
    GetErrorMessage = ccLib.GetErrorMessage()
    End Function
'
Function GetErrorMessage()
    GetErrorMessage = ccLib.GetErrorMessage()
    End Function
'
Function GetCommonErrorForm()
    GetCommonErrorForm = ccLib.GetErrorMessage()
    End Function
'
Sub PrintErrorMessage()
    ccLib.PrintErrorMessage()
    End Sub
'
Sub PrintErrorMessage()
    ccLib.PrintErrorMessage()
    End Sub
'
Sub PrintCommonErrors()
    ccLib.PrintErrorMessage()
    End Sub
'
Sub CommonPrintErrors()
    ccLib.PrintErrorMessage()
    End Sub
'
'=============================================================================
' Print the See Also listing
'   Tablename is the name of the parent table
'   RecordID is the parent RecordID
'=============================================================================
'
Sub PrintSeeAlso(TableName, RecordID)
    Call ccLib.PrintSeeAlso(TableName, RecordID)
    End Sub
'
'=============================================================================
' Print a list of items with short description, linked back to
'   the current page with i=ID#
'
' Table Needs:
'   ID, Name, active, SortOrder, Brief
'=============================================================================
'
Sub PrintRecordList(ListTitle, TableName, WhereClause)
    Call ccLib.PrintRecordList(ListTitle, TableName, WhereClause)
    End Sub
'
'=============================================================================
' Print a list of records, and redirects to them when hit
'
'   Passes ID back through i
'
' Table Needs:
'   ID, Name, active, SortOrder
'   Brief, Link, Clicks
'=============================================================================
'
Sub PrintLinkList( ListTitle, ContentName )
    Call ccLib.PrintLinkList(ListTitle, ContentName )
    End Sub
'
'=============================================================================
' Print a list of products (Items format table), and redirects to them when hit
'
'   Passes ID back through i
'
' Table Needs:
'   ID, Name, active, SortOrder
'   Brief, Link, Clicks
'   ImageFilename, CopyFilename, MemberID (for contact name / email / phone )
'=============================================================================
'
Sub PrintProductList(ListTitle, TableName, WhereClause)
    Call ccLib.PrintProductList(ListTitle, TableName, WhereClause)
    End Sub
'
'=============================================================================
' Print a list of links with a short description
'   TableName conforms to the spec below
'   QueryStringList is added to all QueryStrings
'
' Table Needs:
'   ID, Name, active, SortOrder
'   DocFilename, Viewings
'=============================================================================
'
Sub PrintDownloadList(ListTitle, TableName, WhereClause)
    Call ccLib.PrintDownloadList(ListTitle, TableName, WhereClause)
    End Sub
'
Sub PrintRecordDownloadList(TableName)
    Call ccLib.PrintDownloadList("", TableName, "")
    End Sub
'
Sub PrintRecordDownloadList(TableName)
    Call ccLib.PrintDownloadList("", TableName, "")
    End Sub
'
'=============================================================================
' Print all the members of a group
'
'   Does not print a page title
'=============================================================================
'
Sub PrintGroupMemberList(ListTitle, GroupName)
    Call ccLib.PrintGroupMemberList(ListTitle, GroupName)
    End Sub
'
Sub GroupMemberListPrint(ListTitle, GroupName)
    Call ccLib.PrintGroupMemberList(ListTitle, GroupName)
    End Sub
'
'=============================================================================
' Print a list of organizations
'
'
'=============================================================================
'
Sub PrintOrganization( OrganizationID )
    Call ccLib.PrintOrganization( OrganizationID )
    End Sub
'
Sub PrintOrganizationList( ListTitle )
    Call ccLib.PrintOrganizationList( ListTitle )
    End Sub
'
Sub PrintOrganizations(TableName, WhereClause)
    Call PrintOrganizationList( "" )
    End Sub

'
'=============================================================================
' Print a list of books
'   WhereClause is included in the where clause of all selects.
'
' Books have:
'   a cover page with an index
'   Chapter Pages, with a list of Pages
'   Pages, that can be either text or a download
'
' Tables:
'   Book Table (Tablename)
'       PubDate, BriefFilename, DocFilename, Viewings
'   ChapterTable (Tablename & "Chapters")
'       BookID, DocFilename, BriefFilename, Viewings
'   PageTable (Tablename & "Pages")
'       BookChapterID, DocFilename, CopyFilename, Viewings
'=============================================================================
'
Sub PrintBookList(ListTitle, BookTableName, WhereClause)
    Call ccLib.PrintBookList(ListTitle, BookTableName, WhereClause)
    End Sub
'
'========================================================================
' Old Interface, to be reomved
'========================================================================
'
Sub PrintPage(TableName, WhereClause)
    Call ccLib.PrintPage(TableName, WhereClause)
    End Sub
'
'========================================================================
' Print the page content that matches the given page name
'
'   The pagename is used as a label in the API, so it should
'   not be changed. This is useful for more then one block per page
'========================================================================
'
Sub PrintPageByName(PageName)
    Call ccLib.PrintContentPage(PageName, "Page Content" )
    End Sub
'
'========================================================================
' Print the page content that matches the current Page
'========================================================================
'
Sub PrintPageByPath()
    Dim PathPage
    '
    tp2 ("PrintPageByPath()")
    '
    PathPage = ccLib.ServerPath & ccLib.ServerPage
    If InStr(UCase(PathPage), UCase(ccLib.ServerAppRootPath)) = 1 Then
        PathPage = Mid(PathPage, Len(ccLib.ServerAppRootPath))
        End If
    Call ccLib.PrintContentPage(PageName, "(PathPage=" & EncodeSQLText(PathPage) & ")" )
    End Sub
'
' ----- Old name
'
Sub PrintPathPage(TableName)
    Call ccLib.PrintPathPage(TableName)
    End Sub
'
' ----- Old name
'
Sub PrintPageContent(ContentID)
    Call ccLib.PrintPathPage(GetContentTable(ContentID))
    End Sub
'
' ----- Old name
'
Sub CommonPrintContent()
    Call ccLib.PrintPageByPath
    End Sub
'
'=============================================================================
' Old Interface, to be removed
'=============================================================================
'
Sub PageListPrint(TableName, WhereClause, RootPageID)
    Call ccLib.PageListPrint(TableName, WhereClause, RootPageID)
    End Sub
'
Sub PrintPageList(TableName, WhereClause, RootPageID)
    Call ccLib.PageListPrint(TableName, WhereClause, RootPageID)
    End Sub
'
Sub PrintPageListByName( RootPageName )
    Call ccLib.PrintContentPage(RootPageName)
    End Sub
'
Function MakePageParentList(ContentID, PageID, RootPageID)
    cclib ReportError( "Call to unsupported method, MakePageParentList" )
    End Function
'
'=============================================================================
' Print a Content Page
'=============================================================================
'
Sub PrintContentPage( RootPageName )
    Call ccLib.PrintContentPage( RootPageName )
    End Sub
'
'========================================================================
' Print the "for more information, please contact" line
'
'========================================================================
'
Sub PrintMoreInfo(ContactMemberID)
    Call ccLib.PrintMoreInfo(ContactMemberID)
    End Sub
'
'========================================================================
' ----- prints a link to the feedback popup form
'
'   Creates a sub-form that when submitted, is logged by the notes
'   system (in MembersLib right now). When submitted, it prints a thank you
'   message.
'
'========================================================================
'
Sub PrintFeedbackForm(ContentID, RecordID, ToMemberID, Headline)
    Call ccLib.PrintFeedbackForm(ContentID, RecordID, ToMemberID, Headline)
    End Sub
'
Sub PrintFeedback(ContentID, RecordID, ToMemberID, Headline)
    Call ccLib.PrintFeedbackForm(ContentID, RecordID, ToMemberID, Headline)
    End Sub
'
'========================================================================
' Print a Content Block
'   If block does not exist, it is created.
'   Blocks are groups of content elements.
'   Each content element has:
'       A Name (must be unique within a ContentBlock)
'       A sortorder
'       Formatting Attributes
'           Bold (toggles state)
'           Italic (toggles state)
'           paragraph (start)
'           a function (like merge, contentopen, contentnext, JumpToLabel, etc)
'       a ImageID into the ccImages table
'       a block of copy
'========================================================================
'
Function PrintContentBlock( BlockName )
    PrintContentBlock = ccLib.PrintContentBlock( BlockName )
    End Function
'
'========================================================================
'
'========================================================================
'
Sub CheckPageContentTable(TableName)
    Call ccLib.CheckPageContentTable(TableName)
    End Sub
'
'========================================================================
' Print Whats New
'   Prints a linked list of new content
'
'========================================================================
'
Sub PrintWhatsNew()
    Call ccLib.PrintWhatsNew
    End Sub
'
'========================================================================
' Admin Entrance
'========================================================================
'
Sub PrintAdminPage()
	response.write( ccLib.getAdminPage())
    'Call ccLib.PrintAdminPage
End Sub
'
Sub AdminPage()
	response.write( ccLib.getAdminPage())
'    Call ccLib.PrintAdminPage
End Sub
'
'========================================================================
' Development Tools Entrance
'========================================================================
'
Sub DevToolsPage()
    Call ccLib.PrintToolsPage()
    End Sub
'
'========================================================================
' News and Newsletter calls
'========================================================================
'
Sub PrintNewsletterList( ListName )
	cclib.PrintNewsletterList( ListName )
	end sub
'
Sub PrintNewsletterIssueList( ListName )
	cclib.PrintNewsletterList( ListName )
	end sub
'
'========================================================================
' Member Personalization
'========================================================================
'
Sub PrintMemberProfileForm()
	Call ccLib.PrintMemberProfileForm()
	end sub
'
Sub PrintMemberProfile()
	Call ccLib.PrintMemberProfileForm()
	end sub
'
'========================================================================
' Surveys
'========================================================================
'
Sub PrintSurveyList( ListTitle )
	Call ccLib.PrintSurveyList( ListTitle )
	end sub
'
Sub PrintSurveyForm( SurveyName )
	Call ccLib.PrintSurveyForm( SurveyName )
	end sub
'
' ----- support old call
'
Sub PrintSurvey( )
	Call ccLib.PrintSurveyList( "" )
	end sub
'
'========================================================================
' ----- Commerce Interface
'========================================================================
'
Sub OrderID()
	OrderID = ccLib.OrderID()
	end sub
'
Sub PrintOrderForms()
	Call ccLib.PrintOrderForms()
	end sub
'
Sub ProcessOrderForms()
	Call ccLib.ProcessOrderForms()
	end sub
'
Sub PrintAccount()
	Call ccLib.PrintAccount()
	end sub
'
Sub PrintOrderProcess()
	Call ccLib.PrintOrderProcess()
	end sub
'
Sub OrderPrintForm()
	Call ccLib.PrintOrderProcess()
	end sub
'
Sub PrintCatalog()
	Call ccLib.PrintCatalog()
	end sub
'
Sub PrintCatalog()
	Call ccLib.PrintCatalog()
	end sub
'
Sub CatalogFeaturedListingPrint
	Call ccLib.PrintCatalogFeaturedListing
	end sub
'
Sub PrintCatalogFeaturedListing
	Call ccLib.PrintCatalogFeaturedListing
	end sub

Sub PrintCatalogNewListing
	Call ccLib.PrintCatalogNewListing
	end sub

Sub PrintCatalogCategoryListing( CategoryName )
	Call ccLib.PrintCatalogCategoryListing( CategoryName )
	end sub

Sub CatalogColumnsSet( Columns )
	ccLib.CatalogListingColumns = Columns
	end sub

Sub CatalogSetColumns( Columns )
	ccLib.CatalogListingColumns = Columns
	end sub

Sub CatalogAllowPricingSet( AllowPricing )
	ccLib.CatalogAllowPricing = AllowPricing
	end sub

Sub CatalogSetAllowPricing( AllowPricing )
	ccLib.CatalogAllowPricing = AllowPricing
	end sub

Sub CatalogAllowOrderingSet( AllowOrdering )
	ccLib.CatalogAllowOrdering = AllowOrdering
	end sub

Sub CatalogSetAllowOrdering( AllowOrdering )
	ccLib.CatalogAllowOrdering = AllowOrdering
	end sub
'
' ----- compatibility calls
'
Sub PrintCatalogIndex()
	Call ccLib.PrintCatalog()
	end sub

Sub PrintCatalogForm()
	Call ccLib.PrintCatalog()
	end sub
'
'========================================================================
' Forum Interface
'========================================================================
'
Sub PrintForum( ForumName )
	Call ccLib.PrintForum( ForumName )
	end sub
'
Sub PrintForumList( PageTitle )
	Call ccLib.PrintForumList( PageTitle )
	end sub
'
Sub PrintAllForums( PageTitle )
	Call ccLib.PrintForumList( PageTitle )
	end sub
'
'========================================================================
' Meetings and Events Interface
'========================================================================
'
'Sub PrintMeetingIndex( MeetingTitle, MeetingTableName )
'	Call ccLib.PrintMeetingIndex( MeetingTitle, MeetingTableName )
'	end sub
'
'Sub PrintMeetingDetails( MeetingTitle, MeetingTableName, MeetingID )
'	Call ccLib.PrintMeetingDetails( MeetingTitle, MeetingTableName, MeetingID )
'	end sub
'
'Sub PrintMeetingAttendees( MeetingTitle, MeetingTableName, MeetingID )
'	Call ccLib.PrintMeetingAttendees( MeetingTitle, MeetingTableName, MeetingID )
'	end sub
'
'Sub PrintMeetingLinks( MeetingTitle, MeetingTableName, MeetingID )
'	Call ccLib.PrintMeetingLinks( MeetingTitle, MeetingTableName, MeetingID )
'	end sub
'
Sub PrintMeetingPage( PageTitle, ContentName )
	Call ccLib.PrintMeetingPage( PageTitle, ContentName )
	end sub
'
Sub PrintMeetingList( PageTitle, MeetingTableName )
	Dim ContentID
	ContentID = GetContentIDByTablename( MeetingTableName )
	Call ccLib.PrintMeetingPage( PageTitle, GetContentNameByID( ContentID ))
	end sub
'
Sub PrintMeetingIndex( PageTitle, MeetingTableName )
	Dim ContentID
	ContentID = GetContentIDByTablename( MeetingTableName )
	Call ccLib.PrintMeetingPage( PageTitle, GetContentNameByID( ContentID ))
	end sub
'
'========================================================================
' Visitor Identification
'========================================================================
'
Function IsGroupMember( GroupName )
	IsGroupMember = ccLib.IsGroupMember( GroupName )
	end function
'
Function IsGuest()
	IsGuest = ccLib.IsGuest()
	end function
'
Function IsMember()
	IsMember = ccLib.IsMember()
	end function
'
Function IsAuthenticated()
	IsAuthenticated = ccLib.IsAuthenticated()
	end function
'
Function IsContentManager()
	IsContentManager= ccLib.IsContentManager()
	end function
'
Function IsAuthoring( ContentName )
	IsAuthoring= ccLib.IsAuthoring( ContentName )
	end function
'
Function IsAdmin()
	IsAdmin = ccLib.IsAdmin()
	end function
'
Function IsDeveloper()
	IsDeveloper = ccLib.IsDeveloper()
	end function
'
' ----- Log out the current member
'
Sub LogOutMember()
	Call ccLib.LogoutMember()
	end sub
'
Sub MemberLogout()
	Call ccLib.LogoutMember()
	end sub
'
' ----- Login the current member
'
Function LoginMember( Username, Password )
	LoginMember = ccLib.LoginMember( Username, Password )
	end function
'
Function MemberLogin( Username, Password )
	MemberLogin = ccLib.LoginMember( Username, Password )
	end function
'
' ----- Send a password to a member
'
Function SendMemberpassword( Email )
	SendMemberpassword = ccLib.SendMemberpassword( Email )
	end function
'
' ----- Login the current member by their ID
'
Function LoginMemberByID( MemberID )
	LoginMemberByID = ccLib.LoginMemberByID( MemberID )
	end function
'
Function MemberLoginPreForm()
	Dim Username
	Dim Password
	'
	Username = ReadStreamText( "username" )
	Password = ReadStreamText( "password" )
	MemberLoginPreForm = ccLib.MemberLogin( username, password )
	end function
'
Sub OpenMember( MemberID ) 
	Call ccLib.OpenMember( MemberID )
	end sub
'
Sub MemberOpen( MemberID ) 
	Call ccLib.OpenMember( MemberID )
	end sub
'
' ----- Check member, if not member, put up login page
'
Sub CheckMember()
	Call ccLib.CheckMember()
	end sub
'
Sub MemberCheck()
	Call ccLib.CheckMember()
	end sub
'
' ----- Create a new member
'
Function CreateMember( Username, Password, Email )
	CreateMember = ccLib.CreateMember( Username, Password, Email )
	end function
'
Function NewMember( Username, Password )
	NewMember = ccLib.CreateMember( Username, Password )
	end function
'
'Function MemberNew( Username, Password )
'	MemberNew = ccLib.CreateMember( Username, Password )
'	end function
'
' ----- Create a new member, and log them in
'
Function NewMemberLogin( Username, Password )
	NewMember = ccLib.NewMember( Username, Password )
	end function
'
' ----- Save the current member record
'
Sub SaveMember()
	Call ccLib.SaveMember()
	end sub
'
Sub MemberSave()
	Call ccLib.SaveMember()
	end sub
'
' ----- Generic Login form
'
Function GetMemberLoginForm()
	GetMemberLoginForm = ccLib.GetMemberLoginForm()
	end function
'
Function GetLoginForm()
	GetLoginForm = ccLib.GetMemberLoginForm()
	end function
'
Function MemberLoginForm()
	MemberLoginForm = ccLib.GetMemberLoginForm()
	end function
'
' ----- Generic Join form
'
Function GetJoinForm()
	GetJoinForm = ccLib.GetJoinForm()
	end function
'
Function MemberJoinForm()
	MemberJoinForm = ccLib.GetJoinForm()
	end function
'
Function GetSendPasswordForm()
	GetSendPasswordForm = ccLib.GetSendPasswordForm()
	end function
'
Function MemberEmailPasswordForm()
	MemberEmailPasswordForm = ccLib.GetSendPasswordForm()
	end function
'
Sub PrintJoinForm()
	Call ccLib.PrintJoinForm()
	end sub
'
Sub MemberJoinPrint()
	Call ccLib.PrintJoinForm()
	end sub
'
Sub PrintMemberJoin()
	Call ccLib.PrintJoinForm()
	end sub
'
Sub PrintMemberProfile()
	Call ccLib.PrintMemberProfile()
	end sub
'
Sub MemberTopicsPrint()
	Call ccLib.MemberTopicsPrint()
	end sub
'
' ----- Print out a short member description
'
Sub PrintMember( MemberID )
	Call ccLib.PrintMember( MemberID )
	end sub
'
Sub PrintMemberTopics()
	Call ccLib.MemberTopicsPrint()
	end sub
'
'========================================================================
' ----- Rule Processing Internface
'========================================================================
'
' ----- Get a Group ID
'
Function GetGroupID( GroupName )
	GetGroupID = ccLib.GetGroupID( GroupName )
	end function
'
' ----- Get a Group Name
'
Function GetGroupByID( GroupName )
	GetGroupByID = ccLib.GetGroupByID( GroupName )
	end function
'
' ----- Add a member to a group
'
Sub AddGroupMember( GroupName )
	if IsNumeric( GroupName ) then
		'
		' ----- Old style call, notify administrator to fix
		'
		Call ReportError( "AddGroupMember called with numeric GroupName, might be GroupID." )
		end if
	Call ccLib.AddGroupMember( GroupName )
	end sub
'
' ----- Delete a member from a group
'
Sub DeleteGroupMember( GroupName )
	Call ccLib.DeleteGroupMember( GroupName )
	end sub
'
' ----- Get a ContentSet with all the members of a group
'
Function GetCSGroupMembers( GroupName )
	GetCSGroupMembers = cclib.GetCSGroupMembers( GroupName )
	end function
'
'========================================================================
' Notes Interface
'========================================================================
'
Sub PrintNoteFrame( NoteFrameWidth, NoteFrameHeight )
	Call ccLib.PrintNoteFrame( NoteFrameWidth, NoteFrameHeight )
	end sub
'
'========================================================================
' Reports Interface
'========================================================================
'
Sub PrintReports()
	Call ccLib.PrintReportsPage()
	end sub
'
Sub ReportPage()
	Call ccLib.PrintReportsPage()
	end sub
'
Sub PrintReportsPage()
	Call ccLib.PrintReportsPage()
	end sub
'
'========================================================================
' ContentSet Interface
'========================================================================
'
Sub PrintCSRecordEditLink( CSPointer )
	Call ccLib.PrintCSRecordEditLink( CSPointer )
	end sub
'
Sub PrintCSRecordAddLink( CSPointer, Criteria )
	Call ccLib.PrintCSRecordAddLink( CSPointer, Criteria )
	end sub
'
Sub PrintCSField( CSPointer, FieldName )
	Call ccLib.PrintCSField( CSPointer, FieldName )
	end sub
'
Sub SetCSField( CSPointer, FieldName, FieldValue )
	Call ccLib.SetCSField( CSPointer, FieldName, FieldValue )
	end sub
'
' ----- GetCSFirstFieldName
'
Function GetCSFirstFieldName( CSPointer )
	GetCSFirstFieldName = ccLib.GetCSFirstFieldName( CSPointer )
	end function
'
' ----- GetCSNextFieldName
'
Function GetCSNextFieldName( CSPointer )
	GetCSNextFieldName = ccLib.GetCSNextFieldName( CSPointer )
	end function
'
' ----- GetCSFieldCaption
'
Function GetCSFieldCaption( CSPointer, FieldName )
	GetCSFieldCaption = ccLib.GetCSFieldCaption( CSPointer, FieldName )
	end function
'
' ----- GetCSField
'
Function GetCSField( CSPointer, FieldName )
	GetCSField = ccLib.GetCSField( CSPointer, FieldName )
	end function
'
' ----- GetCSEncodedField
'
Function GetCSEncodedField( CSPointer, FieldName )
	GetCSEncodedField = ccLib.GetCSEncodedField( CSPointer, FieldName )
	end function
'
' ----- IsCSOK
'
Function IsCSOK( CSPointer )
	IsCSOK = ccLib.IsCSOK( CSPointer )
	end function
'
Function CSOK( CSPointer )
	CSOK = ccLib.IsCSOK( CSPointer )
	end function
'
'
'
Sub SaveCSRecord( CSPointer )
	Call ccLib.SaveCSRecord( CSPointer )
	end sub
'
Sub FirstCSRecord( CSPointer )
	Call ccLib.FirstCSRecord( CSPointer )
	end sub
'
Sub NextCSRecord( CSPointer )
	Call ccLib.NextCSRecord( CSPointer )
	end sub
'
Sub CloseCS( CSPointer )
	Call ccLib.CloseCS( CSPointer )
	end sub
'
' ----- InsertCSRecord
'
Function InsertCSRecord( ContentName )
	InsertCSRecord = ccLib.InsertCSRecord( ContentName )
	end function
'
Function InsertCSContent( ContentName )
	InsertCSContent = ccLib.InsertCSRecord( ContentName )
	end function
'
Function InsertContent( ContentName )
	InsertContent = ccLib.InsertCSRecord( ContentName )
	end function
'
' ----- DeleteCSRecord
'
Sub DeleteCSRecord( ContentName )
	Call ccLib.DeleteCSRecord( ContentName )
	end sub
'
' ----- OpenCSContentRecord
'
Function OpenCSContentRecord( ContentName, RecordID )
	OpenCSContentRecord = ccLib.OpenCSContentRecord( ContentName, RecordID )
	end function
'
' ----- OpenCSContent
'
Function OpenCSContent( ContentName, Criteria )
	OpenCSContent = ccLib.OpenCSContent( ContentName, Criteria )
	end function
'
Function OpenContent( ContentName, Criteria )
	OpenContent = ccLib.OpenCSContent( ContentName, Criteria )
	end function
'
' ----- OpenCSSQL
'
Function OpenCSSQL( DataSourceID, SQL )
	OpenCSSQL = ccLib.OpenCSSQL( DataSourceID, SQL )
	end function
'
Function OpenSQL( DataSourceID, SQL )
	OpenSQL = ccLib.OpenCSSQL( DataSourceID, SQL )
	end function
'
' ----- OpenCSGroupMembers
'
Function OpenCSGroupMembers( GroupName, Criteria )
	OpenCSGroupMembers = ccLib.OpenCSGroupMembers( GroupName, Criteria )
	end function
'
' ----- OpenCSWhatsNew
'
Function OpenCSWhatsNew( )
	OpenCSWhatsNew = ccLib.OpenCSWhatsNew( )
	end function
'
' ----- OpenCSWhatsRelated
'
Function OpenCSWhatsRelated( ContentName, RecordID )
	OpenCSWhatsRelated = ccLib.OpenCSWhatsRelated( ContentName, RecordID )
	end function
'
' ----- PublishCS
'
Sub PublishCS( CSPointer )
	CAll ccLib.PublishCS( CSPointer )
	end Sub
'
' ----- PublishCS
'
Sub AbortEditCS( CSPointer )
	CAll ccLib.AbortEditCS( CSPointer )
	end Sub
'
'========================================================================
' HTML Form Interface
'
'========================================================================
'
Sub PrintFormStart()
	Call ccLib.PrintFormStart()
	End sub
'
Sub PrintFormEnd()
	Call ccLib.PrintFormEnd()
	End Sub
'
Sub PrintFormCSInput( CSPointer, FieldName )
	Call ccLib.PrintFormCSInput( CSPointer, FieldName )
	end sub
'
Sub PrintFormCSHidden( CSPointer, FieldName )
	Call cclib.PrintFormCSHidden( CSPointer, FieldName )
	end sub
'
Sub PrintFormButton( ButtonLabel )
	Call ccLib.PrintFormButton( ButtonLabel )
	end sub
'
Sub PrintFormInputText(TagName, DefaultValue, Height, Width)
	Call cclib.PrintFormInputText(TagName, DefaultValue, Height, Width)
	end sub
'
Sub PrintFormInputCheckBox(TagName, DefaultValue )
	Call cclib.PrintFormInputCheckBox(TagName, DefaultValue)
	end sub
'
Sub PrintFormHidden(TagName, DefaultValue )
	Call cclib.PrintFormHidden(TagName, DefaultValue)
	end sub
'
'========================================================================
' Public Properties
'
'========================================================================
'
'
' ----- Content Server Properties
'
Function StateString()
	StateString = ccLib.StateString
	end function
'
Function PageName()
	PageName = ccLib.ServerPage
	end function
'
Function Path()
	Path = ccLib.ServerPath
	end function
'
Function ErrorTrapping()
	ErrorTrapping = ccLib.TrapErrors
	end function
'
Function TrapErrors()
	TrapErrors = ccLib.TrapErrors
	end function
'
Function ServerSecureURLRoot()
	ServerSecureURLRoot = ccLib.ServerSecureURLRoot
	end function
'
Function ServerVirtualPath
	ServerVirtualPath = cclib.ServerVirtualPath
	end function
'
Function VirtualPath
	VirtualPath = cclib.ServerVirtualPath
	end function
'
Function ServerDomain
	ServerDomain = cclib.ServerDomain
	end function
'
Function ServerHost
	ServerHost = cclib.ServerHost
	end function
'
Function ServerPath
	ServerPath = cclib.ServerPath
	end function
'
Function ServerPage
	ServerPage = cclib.ServerPage
	end function
'
Function ServerQueryString
	ServerQueryString = cclib.ServerQueryString
	end function
'
Function ServerAppRootPath
	ServerAppRootPath = cclib.ServerAppRootPath
	end function
'
Function ServerAppPath
	ServerAppPath = cclib.ServerAppPath
	end function
'
Function ServerFilePath
	ServerFilePath = cclib.ServerFilePath
	end function
'
Function ServerSecure
	ServerSecure = cclib.ServerPageSecure
	end function
'
Function ServerPageSecure
	ServerPageSecure = cclib.ServerPageSecure
	end function
'
Function HostFilePath
	HostFilePath = cclib.ServerFilePath
	end function
'
Function AllowPasswordEmail
	AllowPasswordEmail = cclib.AllowPasswordEmail
	end function
'
' ----- Visit/Visitor Properties
'
Function VisitID()
	VisitID = ccLib.VisitID
	end function
'
Function VisitTime()
	VisitTime = ccLib.PageStartTime
	end function
'
Function VisitAuthenticated()
	VisitAuthenticated = ccLib.VisitAuthenticated
	end function
'
Function VisitorID()
	VisitorID = ccLib.VisitorID
	end function
'
Function VisitorNew()
	VisitorNew = ccLib.VisitorNew
	end function
'
' ----- Member Properties
'
Function MemberID()
	MemberID = ccLib.MemberID
	end function
'
Function MemberNew()
	MemberNew = ccLib.MemberNew
	end function
'
Function MemberUsername()
	MemberUsername = ccLib.MemberUsername
	end function
'
Function MemberLoginUsername()
	MemberLoginUsername = ccLib.MemberLoginUsername
	end function
'
Function MemberLoginPassword()
	MemberLoginPassword = ccLib.MemberLoginPassword
	end function
'
Function MemberLoginEmail()
	MemberLoginEmail = ccLib.MemberLoginEmail
	end function
'
Function MemberName()
	MemberName = ccLib.MemberName
	end function
'
Function MemberAdmin()
	MemberAdmin = ccLib.MemberAdmin
	end function
'
Function MemberDeveloper()
	MemberDeveloper = ccLib.MemberDeveloper
	end function
'
'Function MemberCompany()
'	MemberCompany = ccLib.MemberOrganizationName
'	end function
'
'Function MemberOrganization()
'	MemberOrganization = ccLib.MemberOrganizationName
'	end function
'
Function MemberOrganizationID()
	MemberOrganizationID = ccLib.MemberOrganizationID
	end function
'
' ----- Commerse Properties
'
Function OrderItemCount()
	OrderItemCount = ccLib.OrderItemCount
	end function
'
Function IsOrderOK()
	IsOrderOK = ccLib.IsOrderOK
	end function
'
Function OrderOK()
	OrderOK = ccLib.IsOrderOK
	end function
'
'==============================================================================
' ----- Promotion Methods
'==============================================================================
'
Sub PrintPromotionRegistration()
	Call ccLib.PrintPromotionRegistration()
	end sub
'
Function IsPromotionUsed( PromotionKey )
	IsPromotionUsed = ccLib.IsPromotionUsed( PromotionKey )
	end function
'
Function IsPromotionExpired( PromotionKey )
	IsPromotionExpired = ccLib.IsPromotionExpired( PromotionKey )
	end function
'
Function OpenPromotion( PromotionID )
	OpenPromotion = ccLib.OpenPromotion( PromotionID )
	end function
'
Function OpenPromo( PromotionID )
	OpenPromo = ccLib.OpenPromotion( PromotionID )
	end function
'
Function SetPromotion( PromotionKey )
	SetPromotion = ccLib.SetPromotion( PromotionKey )
	end function
'
Function SetPromo( PromotionKey )
	SetPromo = ccLib.SetPromotion( PromotionKey )
	end function
'
'==============================================================================
' ----- IO Interface Methods
'==============================================================================
'
Function ReadFile( Filename )
	ReadFile = ccLib.ReadFile( Filename )
	end function
'
Sub SaveFile( Filename, FileContent )
	Call ccLib.SaveFile( Filename, FileContent )
	end Sub
'
Function EncodeActiveContent(MemberID, ContentName, RecordID, FieldName)
	EncodeActiveContent = ccLib.EncodeActiveContent(MemberID, ContentName, RecordID, FieldName)
	end Function
'
'==============================================================================
' Send Note
'==============================================================================
'
Sub SendNote( ToMemberID, FromName, FromEmail, Subject, Body, DateExpires )
	Call ccLib.SendNote( ToMemberID, FromName, FromEmail, Subject, Body, DateExpires )
	end sub
'
Sub SetMember( MemberID )
	Call ccLib.SetMember( MemberID )
	end sub
'
Function DecodeHTML( Source )
	DecodeHTML = ccLib.DecodeHTML( Source )
	end Function
'
'==============================================================================
'	Initialize
'==============================================================================
'
if Response.IsClientConnected then
      Call InitLibAPI()
else
      Response.End()
      end if

%>
