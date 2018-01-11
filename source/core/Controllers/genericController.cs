
using System;
using System.Reflection;
using System.Xml;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Contensive.Core;
using Contensive.Core.Models.Entity;
using Contensive.Core.Controllers;
using static Contensive.Core.Controllers.genericController;
using static Contensive.Core.constants;
//
using System.Net;
using System.Text;
//
namespace Contensive.Core.Controllers {
    //
    //====================================================================================================
    /// <summary>
    /// controller for shared non-specific tasks
    /// </summary>
    public class genericController {
        //
        //
        //====================================================================================================
        /// <summary>
        /// return a normalized guid in registry format
        /// </summary>
        /// <param name="CP"></param>
        /// <param name="registryFormat"></param>
        /// <returns></returns>
        public static string getGUID() => getGUID(false);
        //
        public static string getGUID(bool registryFormat) {
            string result = "";
            Guid g = Guid.NewGuid();
            if (g != Guid.Empty) {
                result = g.ToString();
                //
                if (!string.IsNullOrEmpty(result)) {
                    result = registryFormat ? result : "{" + result + "}";
                }
            }
            return result;
        }
        //
        //
        //
        public static string encodeEmptyText(string sourceText, string DefaultText) {
            string returnText = sourceText;
            if (string.IsNullOrEmpty(returnText)) {
                returnText = DefaultText;
            }
            return returnText;
        }
        //
        public static int encodeEmptyInteger(string sourceText, int DefaultInteger) => encodeInteger(encodeEmptyText(sourceText, DefaultInteger.ToString()));
        //
        public static DateTime encodeEmptyDate(string sourceText, DateTime DefaultDate) => encodeDate(encodeEmptyText(sourceText, DefaultDate.ToString()));
        //
        public static double encodeEmptyNumber(string sourceText, double DefaultNumber) => encodeNumber(encodeEmptyText(sourceText, DefaultNumber.ToString()));
        //
        public static bool encodeEmptyBoolean(string sourceText, bool DefaultState) => encodeBoolean(encodeEmptyText(sourceText, DefaultState.ToString()));
        //
        //=============================================================================
        // Create the part of the sql where clause that is modified by the user
        //   WorkingQuery is the original querystring to change
        //   QueryName is the name part of the name pair to change
        //   If the QueryName is not found in the string, ignore call
        //=============================================================================
        //
        public static string ModifyQueryString(string WorkingQuery, string QueryName, string QueryValue, bool AddIfMissing = true) {
            string result = null;
            //
            if (WorkingQuery.IndexOf("?") >= 0) {
                result = modifyLinkQuery(WorkingQuery, QueryName, QueryValue, AddIfMissing);
            } else {
                result = modifyLinkQuery("?" + WorkingQuery, QueryName, QueryValue, AddIfMissing);
                if ( result.Length>0) {
                    result = result.Substring(1);
                }
            }
            return result;
        }
        //
        public static string ModifyQueryString(string WorkingQuery, string QueryName, int QueryValue, bool AddIfMissing = true) => ModifyQueryString(WorkingQuery, QueryName, QueryValue.ToString(), AddIfMissing);
        //
        public static string ModifyQueryString(string WorkingQuery, string QueryName, bool QueryValue, bool AddIfMissing = true) => ModifyQueryString(WorkingQuery, QueryName, QueryValue.ToString(), AddIfMissing);
        //
        //=============================================================================
        /// <summary>
        /// Modify the querystring at the end of a link. If there is no, question mark, the link argument is assumed to be a link, not the querysting
        /// </summary>
        /// <param name="Link"></param>
        /// <param name="QueryName"></param>
        /// <param name="QueryValue"></param>
        /// <param name="AddIfMissing"></param>
        /// <returns></returns>
        public static string modifyLinkQuery(string Link, string QueryName, string QueryValue, bool AddIfMissing = true) {
            string tempmodifyLinkQuery = null;
            try {
                string[] Element = { };
                int ElementCount = 0;
                int ElementPointer = 0;
                string[] NameValue = null;
                string UcaseQueryName = null;
                bool ElementFound = false;
                bool iAddIfMissing = false;
                string QueryString = null;
                //
                iAddIfMissing = AddIfMissing;
                if (vbInstr(1, Link, "?") != 0) {
                    tempmodifyLinkQuery = Link.Left( vbInstr(1, Link, "?") - 1);
                    QueryString = Link.Substring(tempmodifyLinkQuery.Length + 1);
                } else {
                    tempmodifyLinkQuery = Link;
                    QueryString = "";
                }
                UcaseQueryName = vbUCase(EncodeRequestVariable(QueryName));
                if (!string.IsNullOrEmpty(QueryString)) {
                    Element = QueryString.Split('&');
                    ElementCount = Element.GetUpperBound(0) + 1;
                    for (ElementPointer = 0; ElementPointer < ElementCount; ElementPointer++) {
                        NameValue = Element[ElementPointer].Split('=');
                        if (NameValue.GetUpperBound(0) == 1) {
                            if (vbUCase(NameValue[0]) == UcaseQueryName) {
                                if (string.IsNullOrEmpty(QueryValue)) {
                                    Element[ElementPointer] = "";
                                } else {
                                    Element[ElementPointer] = QueryName + "=" + QueryValue;
                                }
                                ElementFound = true;
                                break;
                            }
                        }
                    }
                }
                if (!ElementFound && (!string.IsNullOrEmpty(QueryValue))) {
                    //
                    // element not found, it needs to be added
                    //
                    if (iAddIfMissing) {
                        if (string.IsNullOrEmpty(QueryString)) {
                            QueryString = EncodeRequestVariable(QueryName) + "=" + EncodeRequestVariable(QueryValue);
                        } else {
                            QueryString = QueryString + "&" + EncodeRequestVariable(QueryName) + "=" + EncodeRequestVariable(QueryValue);
                        }
                    }
                } else {
                    //
                    // element found
                    //
                    QueryString = string.Join("&", Element);
                    if ((!string.IsNullOrEmpty(QueryString)) && (string.IsNullOrEmpty(QueryValue))) {
                        //
                        // element found and needs to be removed
                        //
                        QueryString = vbReplace(QueryString, "&&", "&");
                        if (QueryString.Left( 1) == "&") {
                            QueryString = QueryString.Substring(1);
                        }
                        if (QueryString.Substring(QueryString.Length - 1) == "&") {
                            QueryString = QueryString.Left( QueryString.Length - 1);
                        }
                    }
                }
                if (!string.IsNullOrEmpty(QueryString)) {
                    tempmodifyLinkQuery = tempmodifyLinkQuery + "?" + QueryString;
                }
            } catch (Exception ex) {
                throw new ApplicationException("Exception in modifyLinkQuery", ex);
            }
            //
            return tempmodifyLinkQuery;
        }
        //    '
        //    '=================================================================================
        //    '
        //    '=================================================================================
        //    '
        //    Public shared Function GetIntegerString(Value As Integer, DigitCount As Integer) As String
        //        If Len(Value) <= DigitCount Then
        //        GetIntegerString = String(DigitCount - Len(CStr(Value)), "0") & CStr(Value)
        //        Else
        //            GetIntegerString = CStr(Value)
        //        End If
        //    End Function
        //    '
        //    '==========================================================================================
        //    '   the current process to a high priority
        //    '       Should be called once from the objects parent when it is first created.
        //    '
        //    '   taken from an example labeled
        //    '       KPD-Team 2000
        //    '       URL: http://www.allapi.net/
        //    '       Email: KPDTeam@Allapi.net
        //    '==========================================================================================
        //    '
        //    Public shared sub SetProcessHighPriority()
        //        Dim hProcess As Integer
        //        '
        //        'set the new priority class
        //        '
        //        hProcess = GetCurrentProcess
        //        Call SetPriorityClass(hProcess, HIGH_PRIORITY_CLASS)
        //        '
        //    End Sub
        //
        //==========================================================================================
        //   Format the current error object into a standard string
        //==========================================================================================
        //
        //Public shared Function GetErrString(Optional ErrorObject As Object) As String
        //    Dim Copy As String
        //    If ErrorObject Is Nothing Then
        //        If Err.Number = 0 Then
        //            GetErrString = "[no error]"
        //        Else
        //            Copy = Err.Description
        //            Copy = vbReplace(Copy, vbCrLf, "-")
        //            Copy = vbReplace(Copy, vbLf, "-")
        //            Copy = vbReplace(Copy, vbCrLf, "")
        //            GetErrString = "[" & Err.Source & " #" & Err.Number & ", " & Copy & "]"
        //        End If
        //    Else
        //        If ErrorObject.Number = 0 Then
        //            GetErrString = "[no error]"
        //        Else
        //            Copy = ErrorObject.Description
        //            Copy = vbReplace(Copy, vbCrLf, "-")
        //            Copy = vbReplace(Copy, vbLf, "-")
        //            Copy = vbReplace(Copy, vbCrLf, "")
        //            GetErrString = "[" & ErrorObject.Source & " #" & ErrorObject.Number & ", " & Copy & "]"
        //        End If
        //    End If
        //    '
        //End Function
        //    '
        //    '==========================================================================================
        //    '   Format the current error object into a standard string
        //    '==========================================================================================
        //    '
        //    Public shared Function GetProcessID() As Integer
        //        GetProcessID = GetCurrentProcessId
        //    End Function
        //    '
        //    '==========================================================================================
        //    '   Test if a test string is in a delimited string
        //    '==========================================================================================
        //    '
        //    Public shared Function genericController.IsInDelimitedString(DelimitedString As String, TestString As String, Delimiter As String) As Boolean
        //        IsInDelimitedString = (0 <> vbInstr(1, Delimiter & DelimitedString & Delimiter, Delimiter & TestString & Delimiter, vbTextCompare))
        //    End Function
        //    '
        //    '========================================================================
        //    ' encodeURL
        //    '
        //    '   Encodes only what is to the left of the first ?
        //    '   All URL path characters are assumed to be correct (/:#)
        //    '========================================================================
        //    '
        //    Function encodeURL(Source As String) As String
        //        ' ##### removed to catch err<>0 problem //On Error //Resume Next
        //        '
        //        Dim URLSplit() As String
        //        Dim LeftSide As String
        //        Dim RightSide As String
        //        '
        //        encodeURL = Source
        //        If Source <> "" Then
        //            URLSplit = Split(Source, "?")
        //            encodeURL = URLSplit(0)
        //            encodeURL = vbReplace(encodeURL, "%", "%25")
        //            '
        //            encodeURL = vbReplace(encodeURL, """", "%22")
        //            encodeURL = vbReplace(encodeURL, " ", "%20")
        //            encodeURL = vbReplace(encodeURL, "$", "%24")
        //            encodeURL = vbReplace(encodeURL, "+", "%2B")
        //            encodeURL = vbReplace(encodeURL, ",", "%2C")
        //            encodeURL = vbReplace(encodeURL, ";", "%3B")
        //            encodeURL = vbReplace(encodeURL, "<", "%3C")
        //            encodeURL = vbReplace(encodeURL, "=", "%3D")
        //            encodeURL = vbReplace(encodeURL, ">", "%3E")
        //            encodeURL = vbReplace(encodeURL, "@", "%40")
        //            If UBound(URLSplit) > 0 Then
        //                encodeURL = encodeURL & "?" & encodeQueryString(URLSplit(1))
        //            End If
        //        End If
        //        '
        //    End Function
        //    '
        //    '========================================================================
        //    ' encodeQueryString
        //    '
        //    '   This routine encodes the URL QueryString to conform to rules
        //    '========================================================================
        //    '
        //    Function encodeQueryString(Source As String) As String
        //        ' ##### removed to catch err<>0 problem //On Error //Resume Next
        //        '
        //        Dim QSSplit() As String
        //        Dim QSPointer As Integer
        //        Dim NVSplit() As String
        //        Dim NV As String
        //        '
        //        encodeQueryString = ""
        //        If Source <> "" Then
        //            QSSplit = Split(Source, "&")
        //            For QSPointer = 0 To UBound(QSSplit)
        //                NV = QSSplit(QSPointer)
        //                If NV <> "" Then
        //                    NVSplit = Split(NV, "=")
        //                    If UBound(NVSplit) = 0 Then
        //                        NVSplit(0) = encodeRequestVariable(NVSplit(0))
        //                        encodeQueryString = encodeQueryString & "&" & NVSplit(0)
        //                    Else
        //                        NVSplit(0) = encodeRequestVariable(NVSplit(0))
        //                        NVSplit(1) = encodeRequestVariable(NVSplit(1))
        //                        encodeQueryString = encodeQueryString & "&" & NVSplit(0) & "=" & NVSplit(1)
        //                    End If
        //                End If
        //            Next
        //            If encodeQueryString <> "" Then
        //                encodeQueryString = Mid(encodeQueryString, 2)
        //            End If
        //        End If
        //        '
        //    End Function
        //    '
        //    '========================================================================
        //    ' encodeRequestVariable
        //    '
        //    '   This routine encodes a request variable for a URL Query String
        //    '       ...can be the requestname or the requestvalue
        //    '========================================================================
        //    '
        //    Function encodeRequestVariable(Source As String) As String
        //        ' ##### removed to catch err<>0 problem //On Error //Resume Next
        //        '
        //        Dim SourcePointer As Integer
        //        Dim Character As String
        //        Dim LocalSource As String
        //        '
        //        If Source <> "" Then
        //            LocalSource = Source
        //            ' "+" is an allowed character for filenames. If you add it, the wrong file will be looked up
        //            'LocalSource = vbReplace(LocalSource, " ", "+")
        //            For SourcePointer = 1 To Len(LocalSource)
        //                Character = Mid(LocalSource, SourcePointer, 1)
        //                ' "%" added so if this is called twice, it will not destroy "%20" values
        //                'If Character = " " Then
        //                '    encodeRequestVariable = encodeRequestVariable & "+"
        //                If vbInstr(1, "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789.-_!*()", Character, vbTextCompare) <> 0 Then
        //                    'ElseIf vbInstr(1, "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789./:-_!*()", Character, vbTextCompare) <> 0 Then
        //                    'ElseIf vbInstr(1, "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789./:?#-_!~*'()%", Character, vbTextCompare) <> 0 Then
        //                    encodeRequestVariable = encodeRequestVariable & Character
        //                Else
        //                    encodeRequestVariable = encodeRequestVariable & "%" & Hex(Asc(Character))
        //                End If
        //            Next
        //        End If
        //        '
        //    End Function
        //    '
        //    '========================================================================
        //    ' encodeHTML
        //    '
        //    '   Convert all characters that are not allowed in HTML to their Text equivalent
        //    '   in preperation for use on an HTML page
        //    '========================================================================
        //    '
        //    Function encodeHTML(Source As String) As String
        //        ' ##### removed to catch err<>0 problem //On Error //Resume Next
        //        '
        //        encodeHTML = Source
        //        encodeHTML = vbReplace(encodeHTML, "&", "&amp;")
        //        encodeHTML = vbReplace(encodeHTML, "<", "&lt;")
        //        encodeHTML = vbReplace(encodeHTML, ">", "&gt;")
        //        encodeHTML = vbReplace(encodeHTML, """", "&quot;")
        //        encodeHTML = vbReplace(encodeHTML, "'", "&#39;")
        //        'encodeHTML = vbReplace(encodeHTML, "'", "&apos;")
        //        '
        //    End Function
        //
        //========================================================================
        // decodeHtml
        //
        //   Convert HTML equivalent characters to their equivalents
        //========================================================================
        //
        public static string decodeHtml(string Source) {
            string tempdecodeHtml = null;
            // ##### removed to catch err<>0 problem //On Error //Resume Next
            //
            int Pos = 0;
            string s = null;
            string CharCodeString = null;
            int CharCode = 0;
            int posEnd = 0;
            //
            // 11/26/2009 - basically re-wrote it, I commented the old one out below
            //
            tempdecodeHtml = "";
            if (!string.IsNullOrEmpty(Source)) {
                s = Source;
                //
                Pos = s.Length;
                Pos = s.LastIndexOf("&#", Pos - 1) + 1;
                while (Pos != 0) {
                    CharCodeString = "";
                    if (s.Substring(Pos + 2, 1) == ";") {
                        CharCodeString = s.Substring(Pos + 1, 1);
                        posEnd = Pos + 4;
                    } else if (s.Substring(Pos + 3, 1) == ";") {
                        CharCodeString = s.Substring(Pos + 1, 2);
                        posEnd = Pos + 5;
                    } else if (s.Substring(Pos + 4, 1) == ";") {
                        CharCodeString = s.Substring(Pos + 1, 3);
                        posEnd = Pos + 6;
                    }
                    if (!string.IsNullOrEmpty(CharCodeString)) {
                        if (CharCodeString.IsNumeric()) {
                            CharCode = encodeInteger(CharCodeString);
                            s = s.Left( Pos - 1) + Convert.ToChar(CharCode) + s.Substring(posEnd - 1);
                        }
                    }
                    //
                    Pos = s.LastIndexOf("&#", Pos - 1) + 1;
                }
                //
                // replace out all common names (at least the most common for now)
                //
                s = vbReplace(s, "&lt;", "<");
                s = vbReplace(s, "&gt;", ">");
                s = vbReplace(s, "&quot;", "\"");
                s = vbReplace(s, "&apos;", "'");
                //
                // Always replace the amp last
                //
                s = vbReplace(s, "&amp;", "&");
                //
                tempdecodeHtml = s;
            }
            // pre-11/26/2009
            //decodeHtml = Source
            //decodeHtml = vbReplace(decodeHtml, "&amp;", "&")
            //decodeHtml = vbReplace(decodeHtml, "&lt;", "<")
            //decodeHtml = vbReplace(decodeHtml, "&gt;", ">")
            //decodeHtml = vbReplace(decodeHtml, "&quot;", """")
            //decodeHtml = vbReplace(decodeHtml, "&nbsp;", " ")
            //
            return tempdecodeHtml;
        }
        //    '
        //    '   Indent every line by 1 tab
        //    '
        public static string htmlIndent(string Source, int depth = 1) {
            string temphtmlIndent = null;
            int posStart = 0;
            int posEnd = 0;
            string pre = null;
            string post = null;
            string target = null;
            //
            posStart = vbInstr(1, Source, "<![CDATA[", 1);
            if (posStart == 0) {
                //
                // no cdata
                //
                posStart = vbInstr(1, Source, "<textarea", 1);
                if (posStart == 0) {
                    //
                    // no textarea
                    //
                    string replaceText = "\r\n" + new string(Convert.ToChar("\t"), (depth + 1));
                    temphtmlIndent = vbReplace(Source, "\r\n\t", replaceText);
                } else {
                    //
                    // text area found, isolate it and indent before and after
                    //
                    posEnd = vbInstr(posStart, Source, "</textarea>", 1);
                    pre = Source.Left( posStart - 1);
                    if (posEnd == 0) {
                        target = Source.Substring(posStart - 1);
                        post = "";
                    } else {
                        target = Source.Substring(posStart - 1, posEnd - posStart + ((string)("</textarea>")).Length);
                        post = Source.Substring((posEnd + ((string)("</textarea>")).Length) - 1);
                    }
                    temphtmlIndent = htmlIndent(pre) + target + htmlIndent(post);
                }
            } else {
                //
                // cdata found, isolate it and indent before and after
                //
                posEnd = vbInstr(posStart, Source, "]]>", 1);
                pre = Source.Left( posStart - 1);
                if (posEnd == 0) {
                    target = Source.Substring(posStart - 1);
                    post = "";
                } else {
                    target = Source.Substring(posStart - 1, posEnd - posStart + ((string)("]]>")).Length);
                    post = Source.Substring(posEnd + 2);
                }
                temphtmlIndent = htmlIndent(pre) + target + htmlIndent(post);
            }
            //    kmaIndent = Source
            //    If vbInstr(1, kmaIndent, "<textarea", vbTextCompare) = 0 Then
            //        kmaIndent = vbReplace(Source, vbCrLf & vbTab, vbCrLf & vbTab & vbTab)
            //    End If
            return temphtmlIndent;
        }
        //
        //========================================================================================================
        //Place code in a form module
        //Add a Command button.
        //========================================================================================================
        //
        public static string kmaByteArrayToString(byte[] Bytes) {
            return System.Text.UTF8Encoding.ASCII.GetString(Bytes);

            //Dim iUnicode As Integer, i As Integer, j As Integer

            ////On Error //Resume Next
            //i = UBound(Bytes)

            //If (i < 1) Then
            //    'ANSI, just convert to unicode and return
            //    kmaByteArrayToString = StrConv(Bytes, VbStrConv.vbUnicode)
            //    Exit Function
            //End If
            //i = i + 1

            //Examine the first two bytes
            //CopyMemory(iUnicode, Bytes(0), 2)

            //If iUnicode = Bytes(0) Then 'Unicode
            //    'Account for terminating null
            //    If (i Mod 2) Then i = i - 1
            //    'Set up a buffer to recieve the string
            //    kmaByteArrayToString = String$(i / 2, 0)
            //    'Copy to string
            //    CopyMemory ByVal StrPtr(kmaByteArrayToString), Bytes(0), i
            //Else 'ANSI
            //    kmaByteArrayToString = StrConv(Bytes, vbUnicode)
            //End If

        }
        //    '
        //    '========================================================================================================
        //    '
        //    '========================================================================================================
        //    '
        //    Public shared Function kmaStringToByteArray(strInput As String, _
        //                                    Optional bReturnAsUnicode As Boolean = True, _
        //                                    Optional bAddNullTerminator As Boolean = False) As Byte()

        //        Dim lRet As Integer
        //        Dim bytBuffer() As Byte
        //        Dim lLenB As Integer

        //        If bReturnAsUnicode Then
        //            'Number of bytes
        //            lLenB = LenB(strInput)
        //            'Resize buffer, do we want terminating null?
        //            If bAddNullTerminator Then
        //                ReDim bytBuffer(lLenB)
        //            Else
        //                ReDim bytBuffer(lLenB - 1)
        //            End If
        //            'Copy characters from string to byte array
        //        CopyMemory bytBuffer(0), ByVal StrPtr(strInput), lLenB
        //        Else
        //            'METHOD ONE
        //            '        'Get rid of embedded nulls
        //            '        strRet = StrConv(strInput, vbFromUnicode)
        //            '        lLenB = LenB(strRet)
        //            '        If bAddNullTerminator Then
        //            '            ReDim bytBuffer(lLenB)
        //            '        Else
        //            '            ReDim bytBuffer(lLenB - 1)
        //            '        End If
        //            '        CopyMemory bytBuffer(0), ByVal StrPtr(strInput), lLenB

        //            'METHOD TWO
        //            'Num of characters
        //            lLenB = Len(strInput)
        //            If bAddNullTerminator Then
        //                ReDim bytBuffer(lLenB)
        //            Else
        //                ReDim bytBuffer(lLenB - 1)
        //            End If
        //        lRet = WideCharToMultiByte(CP_ACP, 0&, ByVal StrPtr(strInput), -1, ByVal VarPtr(bytBuffer(0)), lLenB, 0&, 0&)
        //        End If

        //        kmaStringToByteArray = bytBuffer

        //    End Function
        //    '
        //    '========================================================================================================
        //    '   Sample kmaStringToByteArray
        //    '========================================================================================================
        //    '
        //    Private Sub SampleStringToByteArray()
        //        Dim bAnsi() As Byte
        //        Dim bUni() As Byte
        //        Dim str As String
        //        Dim i As Integer
        //        '
        //        str = "Convert"
        //        bAnsi = kmaStringToByteArray(str, False)
        //        bUni = kmaStringToByteArray(str)
        //        '
        //        For i = 0 To UBound(bAnsi)
        //            Debug.Print("=" & bAnsi(i))
        //        Next
        //        '
        //        Debug.Print("========")
        //        '
        //        For i = 0 To UBound(bUni)
        //            Debug.Print("=" & bUni(i))
        //        Next
        //        '
        //        Debug.Print("ANSI= " & kmaByteArrayToString(bAnsi))
        //        Debug.Print("UNICODE= " & kmaByteArrayToString(bUni))
        //        'Using StrConv to convert a Unicode character array directly
        //        'will cause the resultant string to have extra embedded nulls
        //        'reason, StrConv does not know the difference between Unicode and ANSI
        //        Debug.Print("Resull= " & StrConv(bUni, vbUnicode))
        //    End Sub

        //    '======================================================================================
        //    '
        //    '======================================================================================
        //    '
        //    Public shared sub StartDebugTimer(Enabled As Boolean, Label As String)
        //        ' ##### removed to catch err<>0 problem //On Error //Resume Next
        //        If Enabled Then
        //            If TimerStackCount < TimerStackMax Then
        //                TimerStack(TimerStackCount).Label = Label
        //                TimerStack(TimerStackCount).StartTicks = GetTickCount
        //            Else
        //                Call AppendLogFile("dll" & ".?.StartDebugTimer, " & "Timer Stack overflow, attempting push # [" & TimerStackCount & "], but max = [" & TimerStackMax & "]")
        //            End If
        //            TimerStackCount = TimerStackCount + 1
        //        End If
        //    End Sub
        //    '
        //    Public shared sub StopDebugTimer(Enabled As Boolean, Label As String)
        //        ' ##### removed to catch err<>0 problem //On Error //Resume Next
        //        If Enabled Then
        //            If TimerStackCount <= 0 Then
        //                Call AppendLogFile("dll" & ".?.StopDebugTimer, " & "Timer Error, attempting to Pop, but the stack is empty")
        //            Else
        //                If TimerStackCount <= TimerStackMax Then
        //                    If TimerStack(TimerStackCount - 1).Label = Label Then
        //                    Call AppendLogFile("dll" & ".?.StopDebugTimer, " & "Timer [" & String(2 * TimerStackCount, ".") & Label & "] took " & (GetTickCount - TimerStack(TimerStackCount - 1).StartTicks) & " msec")
        //                    Else
        //                        Call AppendLogFile("dll" & ".?.StopDebugTimer, " & "Timer Error, [" & Label & "] was popped, but [" & TimerStack(TimerStackCount).Label & "] was on the top of the stack")
        //                    End If
        //                End If
        //                TimerStackCount = TimerStackCount - 1
        //            End If
        //        End If
        //    End Sub
        //    '
        //    '
        //    '
        //    Public shared Function PayString(Index) As String
        //        ' ##### removed to catch err<>0 problem //On Error //Resume Next
        //        Select Case Index
        //            Case PayTypeCreditCardOnline
        //                PayString = "Credit Card"
        //            Case PayTypeCreditCardByPhone
        //                PayString = "Credit Card by phone"
        //            Case PayTypeCreditCardByFax
        //                PayString = "Credit Card by fax"
        //            Case PayTypeCHECK
        //                PayString = "Personal Check"
        //            Case PayTypeCHECKCOMPANY
        //                PayString = "Company Check"
        //            Case PayTypeCREDIT
        //                PayString = "You will be billed"
        //            Case PayTypeNetTerms
        //                PayString = "Net Terms (Approved customers only)"
        //            Case PayTypeCODCompanyCheck
        //                PayString = "COD- Pre-Approved Only"
        //            Case PayTypeCODCertifiedFunds
        //                PayString = "COD- Certified Funds"
        //            Case PayTypePAYPAL
        //                PayString = "PayPal"
        //            Case Else
        //                ' Case PayTypeNONE
        //                PayString = "No payment required"
        //        End Select
        //    End Function
        //    '
        //    '
        //    '
        //    Public shared Function CCString(Index) As String
        //        ' ##### removed to catch err<>0 problem //On Error //Resume Next
        //        Select Case Index
        //            Case CCTYPEVISA
        //                CCString = "Visa"
        //            Case CCTYPEMC
        //                CCString = "MasterCard"
        //            Case CCTYPEAMEX
        //                CCString = "American Express"
        //            Case CCTYPEDISCOVER
        //                CCString = "Discover"
        //            Case Else
        //                ' Case CCTYPENOVUS
        //                CCString = "Novus Card"
        //        End Select
        //    End Function
        //    '
        //    '========================================================================
        //    ' Get a Long from a CommandPacket
        //    '   position+0, 4 byte value
        //    '========================================================================
        //    '
        //    Public shared Function GetLongFromByteArray(ByteArray() As Byte, Position As Integer) As Integer
        //        ' ##### removed to catch err<>0 problem //On Error //Resume Next
        //        '
        //        GetLongFromByteArray = ByteArray(Position + 3)
        //        GetLongFromByteArray = ByteArray(Position + 2) + (256 * GetLongFromByteArray)
        //        GetLongFromByteArray = ByteArray(Position + 1) + (256 * GetLongFromByteArray)
        //        GetLongFromByteArray = ByteArray(Position + 0) + (256 * GetLongFromByteArray)
        //        Position = Position + 4
        //        '
        //    End Function
        //    '
        //    '========================================================================
        //    ' Get a Long from a byte array
        //    '   position+0, 4 byte size of the number
        //    '   position+3, start of the number
        //    '========================================================================
        //    '
        //    Public shared Function GetNumberFromByteArray(ByteArray() As Byte, Position As Integer) As Integer
        //        ' ##### removed to catch err<>0 problem //On Error //Resume Next
        //        '
        //        Dim ArgumentCount As Integer
        //        Dim ArgumentLength As Integer
        //        '
        //        ArgumentLength = GetLongFromByteArray(ByteArray(), Position)
        //        '
        //        If ArgumentLength > 0 Then
        //            GetNumberFromByteArray = 0
        //            For ArgumentCount = ArgumentLength - 1 To 0 Step -1
        //                GetNumberFromByteArray = ByteArray(Position + ArgumentCount) + (256 * GetNumberFromByteArray)
        //            Next
        //        End If
        //        Position = Position + ArgumentLength
        //        '
        //    End Function
        //    '
        //    '========================================================================
        //    ' Get a String a byte array
        //    '   position+0, 4 byte length of the string
        //    '   position+3, start of the string
        //    '========================================================================
        //    '
        //    Public shared Function GetStringFromByteArray(ByteArray() As Byte, Position As Integer) As String
        //        ' ##### removed to catch err<>0 problem //On Error //Resume Next
        //        '
        //        Dim Pointer As Integer
        //        Dim ArgumentLength As Integer
        //        '
        //        ArgumentLength = GetLongFromByteArray(ByteArray(), Position)
        //        '
        //        GetStringFromByteArray = ""
        //        If ArgumentLength > 0 Then
        //            For Pointer = 0 To ArgumentLength - 1
        //                GetStringFromByteArray = GetStringFromByteArray & chr(ByteArray(Position + Pointer))
        //            Next
        //        End If
        //        Position = Position + ArgumentLength
        //        '
        //    End Function
        //    '
        //    '========================================================================
        //    ' Get a Long from a byte array
        //    '========================================================================
        //    '
        //    Public shared sub SetLongByteArray(ByRef ByteArray() As Byte, Position As Integer, LongValue As Integer)
        //        ' ##### removed to catch err<>0 problem //On Error //Resume Next
        //        '
        //        ByteArray(Position + 0) = LongValue And (&HFF)
        //        ByteArray(Position + 1) = Int(LongValue / 256) And (&HFF)
        //        ByteArray(Position + 2) = Int(LongValue / (256 ^ 2)) And (&HFF)
        //        ByteArray(Position + 3) = Int(LongValue / (256 ^ 3)) And (&HFF)
        //        Position = Position + 4
        //        '
        //    End Sub
        //    '
        //    '========================================================================
        //    ' Set a string in a byte array
        //    '========================================================================
        //    '
        //    Public shared sub SetStringByteArray(ByRef ByteArray() As Byte, Position As Integer, StringValue As String)
        //        ' ##### removed to catch err<>0 problem //On Error //Resume Next
        //        '
        //        Dim Pointer As Integer
        //        Dim LenStringValue As Integer
        //        '
        //        LenStringValue = Len(StringValue)
        //        If LenStringValue > 0 Then
        //            For Pointer = 0 To LenStringValue - 1
        //                ByteArray(Position + Pointer) = Asc(Mid(StringValue, Pointer + 1, 1)) And (&HFF)
        //            Next
        //            Position = Position + LenStringValue
        //        End If
        //        '
        //    End Sub

        //    '
        //    '========================================================================
        //    '   a Long long on the end of a RMB (Remote Method Block)
        //    '       You determine the position, or it will add it to the end
        //    '========================================================================
        //    '
        //Public shared sub SetRMBLong(ByRef ByteArray() As Byte, LongValue As Integer, Optional Position)
        //        ' ##### removed to catch err<>0 problem //On Error //Resume Next
        //        '
        //        Dim Temp As Integer
        //        Dim MyPosition As Integer
        //        Dim ByteArraySize As Integer
        //        '
        //        ' ----- determine the position
        //        '
        //        If Not IsMissing(Position) Then
        //            MyPosition = Position
        //        Else
        //            '
        //            ' ----- Add it to the end, determine length
        //            '
        //            MyPosition = ByteArray(RMBPositionLength + 3)
        //            MyPosition = ByteArray(RMBPositionLength + 2) + (256 * MyPosition)
        //            MyPosition = ByteArray(RMBPositionLength + 1) + (256 * MyPosition)
        //            MyPosition = ByteArray(RMBPositionLength + 0) + (256 * MyPosition)
        //            '
        //            ' ----- adjust size of array if necessary
        //            '
        //            ByteArraySize = UBound(ByteArray)
        //            If ByteArraySize < (MyPosition + 8) Then
        //                ReDim Preserve ByteArray(ByteArraySize + 8)
        //            End If
        //        End If
        //        '
        //        ' ----- set the length
        //        '
        //        'ByteArray(MyPosition + 0) = 4
        //        'ByteArray(MyPosition + 1) = 0
        //        'ByteArray(MyPosition + 2) = 0
        //        'ByteArray(MyPosition + 3) = 0
        //        'MyPosition = MyPosition + 4
        //        '
        //        ' ----- set the value
        //        '
        //        ByteArray(MyPosition + 0) = LongValue And (&HFF)
        //        ByteArray(MyPosition + 1) = Int(LongValue / 256) And (&HFF)
        //        ByteArray(MyPosition + 2) = Int(LongValue / (256 ^ 2)) And (&HFF)
        //        ByteArray(MyPosition + 3) = Int(LongValue / (256 ^ 3)) And (&HFF)
        //        MyPosition = MyPosition + 4
        //        '
        //        If IsMissing(Position) Then
        //            '
        //            ' ----- Adjust the RMB length if length not given
        //            '
        //            ByteArray(RMBPositionLength + 0) = MyPosition And (&HFF)
        //            ByteArray(RMBPositionLength + 1) = Int(MyPosition / 256) And (&HFF)
        //            ByteArray(RMBPositionLength + 2) = Int(MyPosition / (256 ^ 2)) And (&HFF)
        //            ByteArray(RMBPositionLength + 3) = Int(MyPosition / (256 ^ 3)) And (&HFF)
        //        End If
        //        '
        //    End Sub
        //    '
        //    '========================================================================
        //    '   a Long long on the end of a RMB (Remote Method Block)
        //    '       You determine the position, or it will add it to the end
        //    '========================================================================
        //    '
        //Public shared sub SetRMBString(ByRef ByteArray() As Byte, StringValue As String, Optional Position)
        //        ' ##### removed to catch err<>0 problem //On Error //Resume Next
        //        '
        //        Dim Temp As Integer
        //        Dim MyPosition As Integer
        //        Dim ByteArraySize As Integer
        //        '
        //        ' ----- determine the position
        //        '
        //        If Not IsMissing(Position) Then
        //            MyPosition = Position
        //        Else
        //            '
        //            ' ----- Add it to the end, determine length
        //            '
        //            MyPosition = ByteArray(RMBPositionLength + 3)
        //            MyPosition = ByteArray(RMBPositionLength + 2) + (256 * MyPosition)
        //            MyPosition = ByteArray(RMBPositionLength + 1) + (256 * MyPosition)
        //            MyPosition = ByteArray(RMBPositionLength + 0) + (256 * MyPosition)
        //            '
        //            ' ----- adjust size of array if necessary
        //            '
        //            ByteArraySize = UBound(ByteArray)
        //            If ByteArraySize < (MyPosition + 8) Then
        //                ReDim Preserve ByteArray(ByteArraySize + 4 + Len(StringValue))
        //            End If
        //        End If
        //        '
        //        ' ----- set the value
        //        '

        //        '
        //        Dim Pointer As Integer
        //        Dim LenStringValue As Integer
        //        '
        //        LenStringValue = Len(StringValue)
        //        If LenStringValue > 0 Then
        //            For Pointer = 0 To LenStringValue - 1
        //                ByteArray(MyPosition + Pointer) = Asc(Mid(StringValue, Pointer + 1, 1)) And (&HFF)
        //            Next
        //            MyPosition = MyPosition + LenStringValue
        //        End If
        //        '
        //        If IsMissing(Position) Then
        //            '
        //            ' ----- Adjust the RMB length if length not given
        //            '
        //            ByteArray(RMBPositionLength + 0) = MyPosition And (&HFF)
        //            ByteArray(RMBPositionLength + 1) = Int(MyPosition / 256) And (&HFF)
        //            ByteArray(RMBPositionLength + 2) = Int(MyPosition / (256 ^ 2)) And (&HFF)
        //            ByteArray(RMBPositionLength + 3) = Int(MyPosition / (256 ^ 3)) And (&HFF)
        //        End If
        //        '
        //    End Sub
        //    '
        //    '========================================================================
        //    '   IsTrue
        //    '       returns true or false depending on the state of the variant input
        //    '========================================================================
        //    '
        //    Function IsTrue(ValueVariant) As Boolean
        //        IsTrue = EncodeBoolean(ValueVariant)
        //    End Function
        //    '
        //    '========================================================================
        //    ' EncodeXML
        //    '
        //    '========================================================================
        //    '
        //    Function EncodeXML(ValueVariant As Object, fieldType As Integer) As String
        //        ' ##### removed to catch err<>0 problem //On Error //Resume Next
        //        '
        //        Dim TimeValuething As Single
        //        Dim TimeHours As Integer
        //        Dim TimeMinutes As Integer
        //        Dim TimeSeconds As Integer
        //        '
        //        Select Case fieldType
        //            Case FieldTypeInteger, FieldTypeLookup, FieldTypeRedirect, FieldTypeManyToMany, FieldTypeMemberSelect
        //                If IsNull(ValueVariant) Then
        //                    EncodeXML = "null"
        //                ElseIf ValueVariant = "" Then
        //                    EncodeXML = "null"
        //                ElseIf vbIsNumeric(ValueVariant) Then
        //                    EncodeXML = Int(ValueVariant)
        //                Else
        //                    EncodeXML = "null"
        //                End If
        //            Case FieldTypeBoolean
        //                If IsNull(ValueVariant) Then
        //                    EncodeXML = "0"
        //                ElseIf ValueVariant <> False Then
        //                    EncodeXML = "1"
        //                Else
        //                    EncodeXML = "0"
        //                End If
        //            Case FieldTypeCurrency
        //                If IsNull(ValueVariant) Then
        //                    EncodeXML = "null"
        //                ElseIf ValueVariant = "" Then
        //                    EncodeXML = "null"
        //                ElseIf vbIsNumeric(ValueVariant) Then
        //                    EncodeXML = ValueVariant
        //                Else
        //                    EncodeXML = "null"
        //                End If
        //            Case FieldTypeFloat
        //                If IsNull(ValueVariant) Then
        //                    EncodeXML = "null"
        //                ElseIf ValueVariant = "" Then
        //                    EncodeXML = "null"
        //                ElseIf vbIsNumeric(ValueVariant) Then
        //                    EncodeXML = ValueVariant
        //                Else
        //                    EncodeXML = "null"
        //                End If
        //            Case FieldTypeDate
        //                If IsNull(ValueVariant) Then
        //                    EncodeXML = "null"
        //                ElseIf ValueVariant = "" Then
        //                    EncodeXML = "null"
        //                ElseIf IsDate(ValueVariant) Then
        //                    'TimeVar = CDate(ValueVariant)
        //                    'TimeValuething = 86400! * (TimeVar - Int(TimeVar))
        //                    'TimeHours = Int(TimeValuething / 3600!)
        //                    'TimeMinutes = Int(TimeValuething / 60!) - (TimeHours * 60)
        //                    'TimeSeconds = TimeValuething - (TimeHours * 3600!) - (TimeMinutes * 60!)
        //                    'EncodeXML = Year(ValueVariant) & "-" & Right("0" & Month(ValueVariant), 2) & "-" & Right("0" & Day(ValueVariant), 2) & " " & Right("0" & TimeHours, 2) & ":" & Right("0" & TimeMinutes, 2) & ":" & Right("0" & TimeSeconds, 2)
        //                    EncodeXML = encodeText(ValueVariant)
        //                End If
        //            Case Else
        //                '
        //                ' ----- FieldTypeText
        //                ' ----- FieldTypeLongText
        //                ' ----- FieldTypeFile
        //                ' ----- FieldTypeImage
        //                ' ----- FieldTypeTextFile
        //                ' ----- FieldTypeCSSFile
        //                ' ----- FieldTypeXMLFile
        //                ' ----- FieldTypeJavascriptFile
        //                ' ----- FieldTypeLink
        //                ' ----- FieldTypeResourceLink
        //                ' ----- FieldTypeHTML
        //                ' ----- FieldTypeHTMLFile
        //                '
        //                If IsNull(ValueVariant) Then
        //                    EncodeXML = "null"
        //                ElseIf ValueVariant = "" Then
        //                    EncodeXML = ""
        //                Else
        //                    'EncodeXML = ASPServer.HTMLEncode(ValueVariant)
        //                    'EncodeXML = vbReplace(ValueVariant, "&", "&lt;")
        //                    'EncodeXML = vbReplace(ValueVariant, "<", "&lt;")
        //                    'EncodeXML = vbReplace(EncodeXML, ">", "&gt;")
        //                End If
        //        End Select
        //        '
        //    End Function
        //
        //========================================================================
        // EncodeFilename
        //
        //========================================================================
        //
        public static string encodeFilename(string Source) {
            string allowed = null;
            string chr = null;
            int Ptr = 0;
            int Cnt = 0;
            string returnString;
            //
            returnString = "";
            Cnt = Source.Length;
            if (Cnt > 254) {
                Cnt = 254;
            }
            allowed = "0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ^&'@{}[],$-#()%.+~_";
            for (Ptr = 1; Ptr <= Cnt; Ptr++) {
                chr = Source.Substring(Ptr - 1, 1);
                if (allowed.IndexOf(chr) + 1 >= 0) {
                    returnString = returnString + chr;
                } else {
                    returnString += "_";
                }
            }
            return returnString;
        }
        //    '
        //    'Function encodeFilename(Filename As String) As String
        //    '    ' ##### removed to catch err<>0 problem //On Error //Resume Next
        //    '    '
        //    '    Dim Source() as object
        //    '    Dim Replacement() as object
        //    '    '
        //    '    Source = Array("""", "*", "/", ":", "<", ">", "?", "\", "|", "=")
        //    '    Replacement = Array("_", "_", "_", "_", "_", "_", "_", "_", "_", "_")
        //    '    '
        //    '    encodeFilename = ReplaceMany(Filename, Source, Replacement)
        //    '    If Len(encodeFilename) > 254 Then
        //    '        encodeFilename = Left(encodeFilename, 254)
        //    '    End If
        //    '    encodeFilename = vbReplace(encodeFilename, vbCr, "_")
        //    '    encodeFilename = vbReplace(encodeFilename, vbLf, "_")
        //    '    '
        //    '    End Function
        //    '
        //    '
        //    '

        //    '
        //    '========================================================================
        //    ' DecodeHTML
        //    '
        //    '========================================================================
        //    '
        //    Function DecodeHTML(Source As String) As String
        //        ' ##### removed to catch err<>0 problem //On Error //Resume Next
        //        '
        //        DecodeHTML = decodeHtml(Source)
        //        'Dim SourceChr() as object
        //        'Dim ReplacementChr() as object
        //        ''
        //        'SourceChr = Array("&gt;", "&lt;", "&nbsp;", "&amp;")
        //        'ReplacementChr = Array(">", "<", " ", "&")
        //        ''
        //        'DecodeHTML = ReplaceMany(Source, SourceChr, ReplacementChr)
        //        '
        //    End Function
        //    '
        //    '========================================================================
        //    ' EncodeFilename
        //    '
        //    '========================================================================
        //    '
        //    Function ReplaceMany(Source As String, ArrayOfSource() As Object, ArrayOfReplacement() As Object) As String
        //        ' ##### removed to catch err<>0 problem //On Error //Resume Next
        //        '
        //        Dim Count As Integer
        //        Dim Pointer As Integer
        //        '
        //        Count = UBound(ArrayOfSource) + 1
        //        ReplaceMany = Source
        //        For Pointer = 0 To Count - 1
        //            ReplaceMany = vbReplace(ReplaceMany, ArrayOfSource(Pointer), ArrayOfReplacement(Pointer))
        //        Next
        //        '
        //    End Function
        //    '
        //    '
        //    '
        //    Public shared Function GetURIHost(URI) As String
        //        ' ##### removed to catch err<>0 problem //On Error //Resume Next
        //        '
        //        '   Divide the URI into URIHost, URIPath, and URIPage
        //        '
        //        Dim URIWorking As String
        //        Dim Slash As Integer
        //        Dim LastSlash As Integer
        //        Dim URIHost As String
        //        Dim URIPath As String
        //        Dim URIPage As String
        //        URIWorking = URI
        //        If Mid(vbUCase(URIWorking), 1, 4) = "HTTP" Then
        //            URIWorking = Mid(URIWorking, vbInstr(1, URIWorking, "//") + 2)
        //        End If
        //        URIHost = URIWorking
        //        Slash = vbInstr(1, URIHost, "/")
        //        If Slash = 0 Then
        //            URIPath = "/"
        //            URIPage = ""
        //        Else
        //            URIPath = Mid(URIHost, Slash)
        //            URIHost = Mid(URIHost, 1, Slash - 1)
        //            Slash = vbInstr(1, URIPath, "/")
        //            Do While Slash <> 0
        //                LastSlash = Slash
        //                Slash = vbInstr(LastSlash + 1, URIPath, "/")
        //                '''DoEvents()
        //            Loop
        //            URIPage = Mid(URIPath, LastSlash + 1)
        //            URIPath = Mid(URIPath, 1, LastSlash)
        //        End If
        //        GetURIHost = URIHost
        //        '
        //    End Function
        //    '
        //    '
        //    '
        //    Public shared Function GetURIPage(URI) As String
        //        ' ##### removed to catch err<>0 problem //On Error //Resume Next
        //        '
        //        '   Divide the URI into URIHost, URIPath, and URIPage
        //        '
        //        Dim Slash As Integer
        //        Dim LastSlash As Integer
        //        Dim URIHost As String
        //        Dim URIPath As String
        //        Dim URIPage As String
        //        Dim URIWorking As String
        //        URIWorking = URI
        //        If Mid(vbUCase(URIWorking), 1, 4) = "HTTP" Then
        //            URIWorking = Mid(URIWorking, vbInstr(1, URIWorking, "//") + 2)
        //        End If
        //        URIHost = URIWorking
        //        Slash = vbInstr(1, URIHost, "/")
        //        If Slash = 0 Then
        //            URIPath = "/"
        //            URIPage = ""
        //        Else
        //            URIPath = Mid(URIHost, Slash)
        //            URIHost = Mid(URIHost, 1, Slash - 1)
        //            Slash = vbInstr(1, URIPath, "/")
        //            Do While Slash <> 0
        //                LastSlash = Slash
        //                Slash = vbInstr(LastSlash + 1, URIPath, "/")
        //                '''DoEvents()
        //            Loop
        //            URIPage = Mid(URIPath, LastSlash + 1)
        //            URIPath = Mid(URIPath, 1, LastSlash)
        //        End If
        //        GetURIPage = URIPage
        //        '
        //    End Function
        //    '
        //    '
        //    '
        //    Function GetDateFromGMT(GMTDate As String) As Date
        //        ' ##### removed to catch err<>0 problem //On Error //Resume Next
        //        '
        //        Dim WorkString As String
        //        GetDateFromGMT = 0
        //        If GMTDate <> "" Then
        //            WorkString = Mid(GMTDate, 6, 11)
        //            If IsDate(WorkString) Then
        //                GetDateFromGMT = CDate(WorkString)
        //                WorkString = Mid(GMTDate, 18, 8)
        //                If IsDate(WorkString) Then
        //                    GetDateFromGMT = GetDateFromGMT + CDate(WorkString) + 4 / 24
        //                End If
        //            End If
        //        End If
        //        '
        //    End Function
        //
        // Wdy, DD-Mon-YYYY HH:MM:SS GMT
        //
        public static string GetGMTFromDate(DateTime DateValue) {
            string tempGetGMTFromDate = null;
            int WorkLong = 0;
            //
            tempGetGMTFromDate = "";
            if (dateController.IsDate(DateValue)) {
                switch ((int)DateValue.DayOfWeek) {
                    case 0:
                        tempGetGMTFromDate = "Sun, ";
                        break;
                    case 1:
                        tempGetGMTFromDate = "Mon, ";
                        break;
                    case 2:
                        tempGetGMTFromDate = "Tue, ";
                        break;
                    case 3:
                        tempGetGMTFromDate = "Wed, ";
                        break;
                    case 4:
                        tempGetGMTFromDate = "Thu, ";
                        break;
                    case 5:
                        tempGetGMTFromDate = "Fri, ";
                        break;
                    case 6:
                        tempGetGMTFromDate = "Sat, ";
                        break;
                }
                //
                WorkLong = DateValue.Day;
                if (WorkLong < 10) {
                    tempGetGMTFromDate = tempGetGMTFromDate + "0" + WorkLong.ToString() + " ";
                } else {
                    tempGetGMTFromDate = tempGetGMTFromDate + WorkLong.ToString() + " ";
                }
                //
                switch (DateValue.Month) {
                    case 1:
                        tempGetGMTFromDate = tempGetGMTFromDate + "Jan ";
                        break;
                    case 2:
                        tempGetGMTFromDate = tempGetGMTFromDate + "Feb ";
                        break;
                    case 3:
                        tempGetGMTFromDate = tempGetGMTFromDate + "Mar ";
                        break;
                    case 4:
                        tempGetGMTFromDate = tempGetGMTFromDate + "Apr ";
                        break;
                    case 5:
                        tempGetGMTFromDate = tempGetGMTFromDate + "May ";
                        break;
                    case 6:
                        tempGetGMTFromDate = tempGetGMTFromDate + "Jun ";
                        break;
                    case 7:
                        tempGetGMTFromDate = tempGetGMTFromDate + "Jul ";
                        break;
                    case 8:
                        tempGetGMTFromDate = tempGetGMTFromDate + "Aug ";
                        break;
                    case 9:
                        tempGetGMTFromDate = tempGetGMTFromDate + "Sep ";
                        break;
                    case 10:
                        tempGetGMTFromDate = tempGetGMTFromDate + "Oct ";
                        break;
                    case 11:
                        tempGetGMTFromDate = tempGetGMTFromDate + "Nov ";
                        break;
                    case 12:
                        tempGetGMTFromDate = tempGetGMTFromDate + "Dec ";
                        break;
                }
                //
                tempGetGMTFromDate = tempGetGMTFromDate + encodeText(DateValue.Year) + " ";
                //
                WorkLong = DateValue.Hour;
                if (WorkLong < 10) {
                    tempGetGMTFromDate = tempGetGMTFromDate + "0" + WorkLong.ToString() + ":";
                } else {
                    tempGetGMTFromDate = tempGetGMTFromDate + WorkLong.ToString() + ":";
                }
                //
                WorkLong = DateValue.Minute;
                if (WorkLong < 10) {
                    tempGetGMTFromDate = tempGetGMTFromDate + "0" + WorkLong.ToString() + ":";
                } else {
                    tempGetGMTFromDate = tempGetGMTFromDate + WorkLong.ToString() + ":";
                }
                //
                WorkLong = DateValue.Second;
                if (WorkLong < 10) {
                    tempGetGMTFromDate = tempGetGMTFromDate + "0" + WorkLong.ToString();
                } else {
                    tempGetGMTFromDate = tempGetGMTFromDate + WorkLong.ToString();
                }
                //
                tempGetGMTFromDate = tempGetGMTFromDate + " GMT";
            }
            //
            return tempGetGMTFromDate;
        }
        //    '
        //    '========================================================================
        //    '   EncodeSQL
        //    '       encode a variable to go in an sql expression
        //    '       NOT supported
        //    '========================================================================
        //    '
        //Public shared Function EncodeSQL(ByVal expression As Object, Optional ByVal fieldType As Integer = FieldTypeIdText) As String
        //    ' ##### removed to catch err<>0 problem //On Error //Resume Next
        //    '
        //    Dim iFieldType As Integer
        //    Dim MethodName As String
        //    '
        //    MethodName = "EncodeSQL"
        //    '
        //    iFieldType = fieldType
        //    Select Case iFieldType
        //        Case FieldTypeIdBoolean
        //            EncodeSQL = app.EncodeSQLBoolean(expression)
        //        Case FieldTypeIdCurrency, FieldTypeIdAutoIdIncrement, FieldTypeIdFloat, FieldTypeIdInteger, FieldTypeIdLookup, FieldTypeIdMemberSelect
        //            EncodeSQL = app.EncodeSQLNumber(expression)
        //        Case FieldTypeIdDate
        //            EncodeSQL = app.EncodeSQLDate(expression)
        //        Case FieldTypeIdLongText, FieldTypeIdHTML
        //            EncodeSQL = app.EncodeSQLText(expression)
        //        Case FieldTypeIdFile, FieldTypeIdFileImage, FieldTypeIdLink, FieldTypeIdResourceLink, FieldTypeIdRedirect, FieldTypeIdManyToMany, FieldTypeIdText, FieldTypeIdFileTextPrivate, FieldTypeIdFileJavascript, FieldTypeIdFileXML, FieldTypeIdFileCSS, FieldTypeIdFileHTMLPrivate
        //            EncodeSQL = app.EncodeSQLText(expression)
        //        Case Else
        //            EncodeSQL = app.EncodeSQLText(expression)
        //            On Error GoTo 0
        //            fixme-- cpCore.handleException(New ApplicationException("")) ' -----ignoreInteger, "dll", "Unknown Field Type [" & fieldType & "] used FieldTypeText.")
        //    End Select
        //    '
        //End Function
        //
        //=====================================================================================================
        //   a value in a name/value pair
        //=====================================================================================================
        //
        //Public shared sub SetNameValueArrays(ByVal InputName As String, ByVal InputValue As String, ByRef SQLName() As String, ByRef SQLValue() As String, ByRef Index As Integer)
        //    ' ##### removed to catch err<>0 problem //On Error //Resume Next
        //    '
        //    SQLName(Index) = InputName
        //    SQLValue(Index) = InputValue
        //    Index = Index + 1
        //    '
        //End Sub
        //    '
        //    '
        //    '
        public static string GetApplicationStatusMessage(Models.Context.serverConfigModel.appStatusEnum ApplicationStatus) {
            string tempGetApplicationStatusMessage = null;

            switch (ApplicationStatus) {
                //Case Models.Context.serverConfigModel.appStatusEnum.notFound
                //    GetApplicationStatusMessage = "Application not found"
                //Case Models.Context.serverConfigModel.appStatusEnum.notEnabled
                //    GetApplicationStatusMessage = "Application not enabled"
                //Case Models.Context.serverConfigModel.appStatusEnum.errorDbBad
                //    GetApplicationStatusMessage = "Error verifying core database records"
                //Case Models.Context.serverConfigModel.appStatusEnum.errorDbNotFound
                //    GetApplicationStatusMessage = "Error opening application database"
                //Case Models.Context.serverConfigModel.appStatusEnum.errorDbFoundButContentMetaMissing
                //    GetApplicationStatusMessage = "The database for this application was found, but content meta table could not be read."
                //Case Models.Context.serverConfigModel.appStatusEnum.errorAppConfigNotValid
                //    GetApplicationStatusMessage = "The application configuration file on this front-end server is not valid."
                //Case Models.Context.serverConfigModel.appStatusEnum.errorAppConfigNotFound
                //    GetApplicationStatusMessage = "The application configuration file was not be found on this front-end server."
                //Case Models.Context.serverConfigModel.appStatusEnum.errorNoHostService
                //    GetApplicationStatusMessage = "Contensive server not running"
                //Case Models.Context.serverConfigModel.appStatusEnum.errorKernelFailure
                //    GetApplicationStatusMessage = "Error contacting Contensive kernel services"
                //Case Models.Context.serverConfigModel.appStatusEnum.errorLicenseFailure
                //    GetApplicationStatusMessage = "Error verifying Contensive site license, see Http://www.Contensive.com/License"
                //Case Models.Context.serverConfigModel.appStatusEnum.errorConnectionObjectFailure
                //    GetApplicationStatusMessage = "Error creating ODBC Connection object"
                //Case Models.Context.serverConfigModel.appStatusEnum.errorConnectionStringFailure
                //    GetApplicationStatusMessage = "ODBC Data Source connection failed"
                //Case Models.Context.serverConfigModel.appStatusEnum.errorDataSourceFailure
                //    GetApplicationStatusMessage = "Error opening default data source"
                //Case Models.Context.serverConfigModel.appStatusEnum.errorDuplicateDomains
                //    GetApplicationStatusMessage = "Can not determine application because there are multiple applications with domain names that match this site's domain (See Application Manager)"
                //Case Models.Context.serverConfigModel.appStatusEnum.errorFailedToInitialize
                //    GetApplicationStatusMessage = "Application failed to initialize, see trace log for details"
                //    'Case Models.Context.serverConfigModel.applicationStatusEnum.ApplicationStatusPaused
                //    '    GetApplicationStatusMessage = "Contensive application paused"
                case Models.Context.serverConfigModel.appStatusEnum.OK:
                    tempGetApplicationStatusMessage = "Application OK";
                    break;
                case Models.Context.serverConfigModel.appStatusEnum.building:
                    tempGetApplicationStatusMessage = "Application building";
                    break;
                default:
                    tempGetApplicationStatusMessage = "Unknown status code [" + ApplicationStatus + "], see trace log for details";
                    break;
            }
            return tempGetApplicationStatusMessage;
        }
        //    '
        //    '
        //    '
        //    Public shared Function GetFormInputSelectNameValue(SelectName As String, NameValueArray() As NameValuePairType) As String
        //        Dim Pointer As Integer
        //        Dim Source() As NameValuePairType
        //        '
        //        Source = NameValueArray
        //        GetFormInputSelectNameValue = "<SELECT name=""" & SelectName & """ Size=""1"">"
        //        For Pointer = 0 To UBound(NameValueArray)
        //            GetFormInputSelectNameValue = GetFormInputSelectNameValue & "<OPTION value=""" & Source(Pointer).Value & """>" & Source(Pointer).Name & "</OPTION>"
        //        Next
        //        GetFormInputSelectNameValue = GetFormInputSelectNameValue & "</SELECT>"
        //    End Function
        //
        //
        //
        public static string getSpacer(int Width, int Height) {
            return "<img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"" + Width + "\" height=\"" + Height + "\" border=\"0\">";
        }
        //
        //
        //
        public static string processReplacement(object NameValueLines, object Source) {
            string tempprocessReplacement = null;
            //
            string iNameValueLines = null;
            string[] Lines = null;
            int LinePtr = 0;
            string[] Names = { };
            string[] Values = { };
            int PairPtr = 0;
            int PairCnt = 0;
            string[] Splits = null;
            //
            // ----- read pairs in from NameValueLines
            //
            iNameValueLines = encodeText(NameValueLines);
            if (vbInstr(1, iNameValueLines, "=") != 0) {
                PairCnt = 0;
                Lines = SplitCRLF(iNameValueLines);
                for (LinePtr = 0; LinePtr <= Lines.GetUpperBound(0); LinePtr++) {
                    if (vbInstr(1, Lines[LinePtr], "=") != 0) {
                        Splits = Lines[LinePtr].Split('=');
                        Array.Resize(ref Names, PairCnt + 1);
                        Array.Resize(ref Names, PairCnt + 1);
                        Array.Resize(ref Values, PairCnt + 1);
                        Names[PairCnt] = Splits[0].Trim(' ');
                        Names[PairCnt] = vbReplace(Names[PairCnt], "\t", "");
                        Splits[0] = "";
                        Values[PairCnt] = Splits[1].Trim(' ');
                        PairCnt = PairCnt + 1;
                    }
                }
            }
            //
            // ----- Process replacements on Source
            //
            tempprocessReplacement = encodeText(Source);
            if (PairCnt > 0) {
                for (PairPtr = 0; PairPtr < PairCnt; PairPtr++) {
                    tempprocessReplacement = vbReplace(tempprocessReplacement, Names[PairPtr], Values[PairPtr], 1, 99, 1);
                }
            }
            //
            return tempprocessReplacement;
        }
        public static string ConvertLinksToAbsolute(string Source, string RootLink) {
            string result = Source;
            try {
                result = result.Replace(" href=\"", " href=\"/");
                result = result.Replace(" href=\"/http", " href=\"http");
                result = result.Replace(" href=\"/mailto", " href=\"mailto");
                result = result.Replace(" href=\"//", " href=\"" + RootLink);
                result = result.Replace(" href=\"/?", " href=\"" + RootLink + "?");
                result = result.Replace(" href=\"/", " href=\"" + RootLink);
                //
                result = result.Replace(" href=", " href=/");
                result = result.Replace(" href=/\"", " href=\"");
                result = result.Replace(" href=/http", " href=http");
                result = result.Replace(" href=//", " href=" + RootLink);
                result = result.Replace(" href=/?", " href=" + RootLink + "?");
                result = result.Replace(" href=/", " href=" + RootLink);
                //
                result = result.Replace(" src=\"", " src=\"/");
                result = result.Replace(" src=\"/http", " src=\"http");
                result = result.Replace(" src=\"//", " src=\"" + RootLink);
                result = result.Replace(" src=\"/?", " src=\"" + RootLink + "?");
                result = result.Replace(" src=\"/", " src=\"" + RootLink);
                //
                result = result.Replace(" src=", " src=/");
                result = result.Replace(" src=/\"", " src=\"");
                result = result.Replace(" src=/http", " src=http");
                result = result.Replace(" src=//", " src=" + RootLink);
                result = result.Replace(" src=/?", " src=" + RootLink + "?");
                result = result.Replace(" src=/", " src=" + RootLink);
            } catch (Exception) {
                throw new ApplicationException("Error in ConvertLinksToAbsolute");
            }
            return result;
        }
        //
        //
        //
        //Public shared Function GetAddonRootPath() As String
        //    Dim testPath As String
        //    '
        //    GetAddonRootPath = getAppPath & "\addons"
        //    If vbInstr(1, GetAddonRootPath, "\github\", vbTextCompare) <> 0 Then
        //        '
        //        ' debugging - change program path to dummy path so addon builds all copy to
        //        '
        //        testPath = Environ$("programfiles(x86)")
        //        If testPath = "" Then
        //            testPath = Environ$("programfiles")
        //        End If
        //        GetAddonRootPath = testPath & "\kma\contensive\addons"
        //    End If
        //End Function
        //
        //
        //
        public static string GetHTMLComment(string Comment) {
            return "<!-- " + Comment + " -->";
        }
        //
        //
        //
        public static string[] SplitCRLF(string Expression) {
            string[] tempSplitCRLF = null;
            //
            if (vbInstr(1, Expression, "\r\n") != 0) {
                tempSplitCRLF = Expression.Split(new[] { "\r\n" }, StringSplitOptions.None);
            } else if (vbInstr(1, Expression, "\r") != 0) {
                tempSplitCRLF = Expression.Split('\r');
            } else if (vbInstr(1, Expression, "\n") != 0) {
                tempSplitCRLF = Expression.Split('\n');
            } else {
                tempSplitCRLF = new string[1];
                tempSplitCRLF = Expression.Split(new[] { "\r\n" }, StringSplitOptions.None);
            }
            return tempSplitCRLF;
        }
        //    '
        //    '
        //    '
        //Public shared sub runProcess(cp.core,Cmd As String, Optional ByVal eWindowStyle As VBA.VbAppWinStyle = vbHide, Optional WaitForReturn As Boolean)
        //        On Error GoTo ErrorTrap
        //        '
        //        Dim ShellObj As Object
        //        '
        //        ShellObj = CreateObject("WScript.Shell")
        //        If Not (ShellObj Is Nothing) Then
        //            Call ShellObj.Run(Cmd, 0, WaitForReturn)
        //        End If
        //        ShellObj = Nothing
        //        Exit Sub
        //        '
        ////ErrorTrap:
        //        Call AppendLogFile("ErrorTrap, runProcess running command [" & Cmd & "], WaitForReturn=" & WaitForReturn & ", err=" & GetErrString(Err))
        //    End Sub
        //
        //------------------------------------------------------------------------------------------------------------
        //   Encodes an argument in an Addon OptionString (QueryString) for all non-allowed characters
        //       call this before parsing them together
        //       call decodeAddonConstructorArgument after parsing them apart
        //
        //       Arg0,Arg1,Arg2,Arg3,Name=Value&Name=VAlue[Option1|Option2]
        //
        //       This routine is needed for all Arg, Name, Value, Option values
        //
        //------------------------------------------------------------------------------------------------------------
        //
        public static string EncodeAddonConstructorArgument(string Arg) {
            string a = Arg;
            a = vbReplace(a, "\\", "\\\\");
            a = vbReplace(a, "\r\n", "\\n");
            a = vbReplace(a, "\t", "\\t");
            a = vbReplace(a, "&", "\\&");
            a = vbReplace(a, "=", "\\=");
            a = vbReplace(a, ",", "\\,");
            a = vbReplace(a, "\"", "\\\"");
            a = vbReplace(a, "'", "\\'");
            a = vbReplace(a, "|", "\\|");
            a = vbReplace(a, "[", "\\[");
            a = vbReplace(a, "]", "\\]");
            a = vbReplace(a, ":", "\\:");
            return a;
        }
        //
        //------------------------------------------------------------------------------------------------------------
        //   Decodes an argument parsed from an AddonConstructorString for all non-allowed characters
        //       AddonConstructorString is a & delimited string of name=value[selector]descriptor
        //
        //       to get a value from an AddonConstructorString, first use getargument() to get the correct value[selector]descriptor
        //       then remove everything to the right of any '['
        //
        //       call encodeAddonConstructorargument before parsing them together
        //       call decodeAddonConstructorArgument after parsing them apart
        //
        //       Arg0,Arg1,Arg2,Arg3,Name=Value&Name=VAlue[Option1|Option2]
        //
        //       This routine is needed for all Arg, Name, Value, Option values
        //
        //------------------------------------------------------------------------------------------------------------
        //
        public static string DecodeAddonConstructorArgument(string EncodedArg) {
            string a;
            //
            a = EncodedArg;
            a = vbReplace(a, "\\:", ":");
            a = vbReplace(a, "\\]", "]");
            a = vbReplace(a, "\\[", "[");
            a = vbReplace(a, "\\|", "|");
            a = vbReplace(a, "\\'", "'");
            a = vbReplace(a, "\\\"", "\"");
            a = vbReplace(a, "\\,", ",");
            a = vbReplace(a, "\\=", "=");
            a = vbReplace(a, "\\&", "&");
            a = vbReplace(a, "\\t", "\t");
            a = vbReplace(a, "\\n", "\r\n");
            a = vbReplace(a, "\\\\", "\\");
            return a;
        }
        //    '
        //    ' returns true of the link is a valid link on the source host
        //    '
        public static bool IsLinkToThisHost(string Host, string Link) {
            bool tempIsLinkToThisHost = false;
            bool result = false;
            try {
                string LinkHost = null;
                int Pos = 0;
                //
                if (string.IsNullOrEmpty(Link.Trim(' '))) {
                    //
                    // Blank is not a link
                    //
                    tempIsLinkToThisHost = false;
                } else if (vbInstr(1, Link, "://") != 0) {
                    //
                    // includes protocol, may be link to another site
                    //
                    LinkHost = vbLCase(Link);
                    Pos = 1;
                    Pos = vbInstr(Pos, LinkHost, "://");
                    if (Pos > 0) {
                        Pos = vbInstr(Pos + 3, LinkHost, "/");
                        if (Pos > 0) {
                            LinkHost = LinkHost.Left( Pos - 1);
                        }
                        tempIsLinkToThisHost = (vbLCase(Host) == LinkHost);
                        if (!tempIsLinkToThisHost) {
                            //
                            // try combinations including/excluding www.
                            //
                            if (vbInstr(1, LinkHost, "www.", 1) != 0) {
                                //
                                // remove it
                                //
                                LinkHost = vbReplace(LinkHost, "www.", "", 1, 99, 1);
                                tempIsLinkToThisHost = (vbLCase(Host) == LinkHost);
                            } else {
                                //
                                // add it
                                //
                                LinkHost = vbReplace(LinkHost, "://", "://www.", 1, 99, 1);
                                tempIsLinkToThisHost = (vbLCase(Host) == LinkHost);
                            }
                        }
                    }
                } else if (vbInstr(1, Link, "#") == 1) {
                    //
                    // Is a bookmark, not a link
                    //
                    tempIsLinkToThisHost = false;
                } else {
                    //
                    // all others are links on the source
                    //
                    tempIsLinkToThisHost = true;
                }
                if (!tempIsLinkToThisHost) {
                    //Link = Link;
                }
            } catch (Exception) {
                throw;
            }
            return result;
        }
        //
        //========================================================================================================
        //   ConvertLinkToRootRelative
        //
        //   /images/logo-cmc.main_jpg with any Basepath to /images/logo-cmc.main_jpg
        //   http://gcm.brandeveolve.com/images/logo-cmc.main_jpg with any BasePath  to /images/logo-cmc.main_jpg
        //   images/logo-cmc.main_jpg with Basepath '/' to /images/logo-cmc.main_jpg
        //   logo-cmc.main_jpg with Basepath '/images/' to /images/logo-cmc.main_jpg
        //
        //========================================================================================================
        //
        public static string ConvertLinkToRootRelative(string Link, string BasePath) {
            string tempConvertLinkToRootRelative = null;
            //
            int Pos = 0;
            //
            tempConvertLinkToRootRelative = Link;
            if (vbInstr(1, Link, "/") == 1) {
                //
                //   case /images/logo-cmc.main_jpg with any Basepath to /images/logo-cmc.main_jpg
                //
            } else if (vbInstr(1, Link, "://") != 0) {
                //
                //   case http://gcm.brandeveolve.com/images/logo-cmc.main_jpg with any BasePath  to /images/logo-cmc.main_jpg
                //
                Pos = vbInstr(1, Link, "://");
                if (Pos > 0) {
                    Pos = vbInstr(Pos + 3, Link, "/");
                    if (Pos > 0) {
                        tempConvertLinkToRootRelative = Link.Substring(Pos - 1);
                    } else {
                        //
                        // This is just the domain name, RootRelative is the root
                        //
                        tempConvertLinkToRootRelative = "/";
                    }
                }
            } else {
                //
                //   case images/logo-cmc.main_jpg with Basepath '/' to /images/logo-cmc.main_jpg
                //   case logo-cmc.main_jpg with Basepath '/images/' to /images/logo-cmc.main_jpg
                //
                tempConvertLinkToRootRelative = BasePath + Link;
            }
            //
            return tempConvertLinkToRootRelative;
        }
        //
        //
        //
        public static string GetAddonIconImg(string AdminURL, int IconWidth, int IconHeight, int IconSprites, bool IconIsInline, string IconImgID, string IconFilename, string serverFilePath, string IconAlt, string IconTitle, string ACInstanceID, int IconSpriteColumn) {
            string tempGetAddonIconImg = null;
            //
            if (string.IsNullOrEmpty(IconAlt)) {
                IconAlt = "Add-on";
            }
            if (string.IsNullOrEmpty(IconTitle)) {
                IconTitle = "Rendered as Add-on";
            }
            if (string.IsNullOrEmpty(IconFilename)) {
                //
                // No icon given, use the default
                //
                if (IconIsInline) {
                    IconFilename = "/ccLib/images/IconAddonInlineDefault.png";
                    IconWidth = 62;
                    IconHeight = 17;
                    IconSprites = 0;
                } else {
                    IconFilename = "/ccLib/images/IconAddonBlockDefault.png";
                    IconWidth = 57;
                    IconHeight = 59;
                    IconSprites = 4;
                }
            } else if (vbInstr(1, IconFilename, "://") != 0) {
                //
                // icon is an Absolute URL - leave it
                //
            } else if (IconFilename.Left( 1) == "/") {
                //
                // icon is Root Relative, leave it
                //
            } else {
                //
                // icon is a virtual file, add the serverfilepath
                //
                IconFilename = serverFilePath + IconFilename;
            }
            //IconFilename = encodeJavascript(IconFilename)
            if ((IconWidth == 0) || (IconHeight == 0)) {
                IconSprites = 0;
            }

            if (IconSprites == 0) {
                //
                // just the icon
                //
                tempGetAddonIconImg = "<img"
                    + " border=0"
                    + " id=\"" + IconImgID + "\""
                    + " onDblClick=\"window.parent.OpenAddonPropertyWindow(this,'" + AdminURL + "');\""
                    + " alt=\"" + IconAlt + "\""
                    + " title=\"" + IconTitle + "\""
                    + " src=\"" + IconFilename + "\"";
                //GetAddonIconImg = "<img" _
                //    & " id=""AC,AGGREGATEFUNCTION,0," & FieldName & "," & ArgumentList & """" _
                //    & " onDblClick=""window.parent.OpenAddonPropertyWindow(this);""" _
                //    & " alt=""" & IconAlt & """" _
                //    & " title=""" & IconTitle & """" _
                //    & " src=""" & IconFilename & """"
                if (IconWidth != 0) {
                    tempGetAddonIconImg = tempGetAddonIconImg + " width=\"" + IconWidth + "px\"";
                }
                if (IconHeight != 0) {
                    tempGetAddonIconImg = tempGetAddonIconImg + " height=\"" + IconHeight + "px\"";
                }
                if (IconIsInline) {
                    tempGetAddonIconImg = tempGetAddonIconImg + " style=\"vertical-align:middle;display:inline;\" ";
                } else {
                    tempGetAddonIconImg = tempGetAddonIconImg + " style=\"display:block\" ";
                }
                if (!string.IsNullOrEmpty(ACInstanceID)) {
                    tempGetAddonIconImg = tempGetAddonIconImg + " ACInstanceID=\"" + ACInstanceID + "\"";
                }
                tempGetAddonIconImg = tempGetAddonIconImg + ">";
            } else {
                //
                // Sprite Icon
                //
                tempGetAddonIconImg = GetIconSprite(IconImgID, IconSpriteColumn, IconFilename, IconWidth, IconHeight, IconAlt, IconTitle, "window.parent.OpenAddonPropertyWindow(this,'" + AdminURL + "');", IconIsInline, ACInstanceID);
            }
            return tempGetAddonIconImg;
        }
        //
        //
        //
        public static string ConvertRSTypeToGoogleType(int RecordFieldType) {
            string tempConvertRSTypeToGoogleType = null;
            switch (RecordFieldType) {
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                case 14:
                case 16:
                case 17:
                case 18:
                case 19:
                case 20:
                case 21:
                case 131:
                    tempConvertRSTypeToGoogleType = "number";
                    break;
                default:
                    tempConvertRSTypeToGoogleType = "string";
                    break;
            }
            return tempConvertRSTypeToGoogleType;
        }
        //    '
        //    '
        //    '
        public static string GetIconSprite(string TagID, int SpriteColumn, string IconSrc, int IconWidth, int IconHeight, string IconAlt, string IconTitle, string onDblClick, bool IconIsInline, string ACInstanceID) {
            string tempGetIconSprite = null;
            //
            string ImgStyle = null;
            //
            tempGetIconSprite = "<img"
                + " border=0"
                + " id=\"" + TagID + "\""
                + " onMouseOver=\"this.style.backgroundPosition='" + (-1 * SpriteColumn * IconWidth) + "px -" + (2 * IconHeight) + "px';\""
                + " onMouseOut=\"this.style.backgroundPosition='" + (-1 * SpriteColumn * IconWidth) + "px 0px'\""
                + " onDblClick=\"" + onDblClick + "\""
                + " alt=\"" + IconAlt + "\""
                + " title=\"" + IconTitle + "\""
                + " src=\"/ccLib/images/spacer.gif\"";
            ImgStyle = "background:url(" + IconSrc + ") " + (-1 * SpriteColumn * IconWidth) + "px 0px no-repeat;";
            ImgStyle = ImgStyle + "width:" + IconWidth + "px;";
            ImgStyle = ImgStyle + "height:" + IconHeight + "px;";
            if (IconIsInline) {
                ImgStyle = ImgStyle + "vertical-align:middle;display:inline;";
            } else {
                ImgStyle = ImgStyle + "display:block;";
            }
            if (!string.IsNullOrEmpty(ACInstanceID)) {
                tempGetIconSprite = tempGetIconSprite + " ACInstanceID=\"" + ACInstanceID + "\"";
            }
            return tempGetIconSprite + " style=\"" + ImgStyle + "\">";
        }
        //
        //================================================================================================================
        //   Separate a URL into its host, path, page parts
        //================================================================================================================
        //
        public static void SeparateURL(string SourceURL, ref string Protocol, ref string Host, ref string Path, ref string Page, ref string QueryString) {
            //
            //   Divide the URL into URLHost, URLPath, and URLPage
            //
            string WorkingURL = null;
            int Position = 0;
            //
            // Get Protocol (before the first :)
            //
            WorkingURL = SourceURL;
            Position = vbInstr(1, WorkingURL, ":");
            //Position = vbInstr(1, WorkingURL, "://")
            if (Position != 0) {
                Protocol = WorkingURL.Left( Position + 2);
                WorkingURL = WorkingURL.Substring(Position + 2);
            }
            //
            // compatibility fix
            //
            if (vbInstr(1, WorkingURL, "//") == 1) {
                if (string.IsNullOrEmpty(Protocol)) {
                    Protocol = "http:";
                }
                Protocol = Protocol + "//";
                WorkingURL = WorkingURL.Substring(2);
            }
            //
            // Get QueryString
            //
            Position = vbInstr(1, WorkingURL, "?");
            if (Position > 0) {
                QueryString = WorkingURL.Substring(Position - 1);
                WorkingURL = WorkingURL.Left( Position - 1);
            }
            //
            // separate host from pathpage
            //
            //iURLHost = WorkingURL
            Position = vbInstr(WorkingURL, "/");
            if ((Position == 0) && (string.IsNullOrEmpty(Protocol))) {
                //
                // Page without path or host
                //
                Page = WorkingURL;
                Path = "";
                Host = "";
            } else if (Position == 0) {
                //
                // host, without path or page
                //
                Page = "";
                Path = "/";
                Host = WorkingURL;
            } else {
                //
                // host with a path (at least)
                //
                Path = WorkingURL.Substring(Position - 1);
                Host = WorkingURL.Left( Position - 1);
                //
                // separate page from path
                //
                Position = Path.LastIndexOf("/") + 1;
                if (Position == 0) {
                    //
                    // no path, just a page
                    //
                    Page = Path;
                    Path = "/";
                } else {
                    Page = Path.Substring(Position);
                    Path = Path.Left( Position);
                }
            }
        }
        //
        //================================================================================================================
        //   Separate a URL into its host, path, page parts
        //================================================================================================================
        //
        public static void ParseURL(string SourceURL, ref string Protocol, ref string Host, ref string Port, ref string Path, ref string Page, ref string QueryString) {
            //
            //   Divide the URL into URLHost, URLPath, and URLPage
            //
            string iURLWorking = "";
            string iURLProtocol = "";
            string iURLHost = "";
            string iURLPort = "";
            string iURLPath = "";
            string iURLPage = "";
            string iURLQueryString = "";
            int Position = 0;
            //
            iURLWorking = SourceURL;
            Position = vbInstr(1, iURLWorking, "://");
            if (Position != 0) {
                iURLProtocol = iURLWorking.Left( Position + 2);
                iURLWorking = iURLWorking.Substring(Position + 2);
            }
            //
            // separate Host:Port from pathpage
            //
            iURLHost = iURLWorking;
            Position = vbInstr(iURLHost, "/");
            if (Position == 0) {
                //
                // just host, no path or page
                //
                iURLPath = "/";
                iURLPage = "";
            } else {
                iURLPath = iURLHost.Substring(Position - 1);
                iURLHost = iURLHost.Left( Position - 1);
                //
                // separate page from path
                //
                Position = iURLPath.LastIndexOf("/") + 1;
                if (Position == 0) {
                    //
                    // no path, just a page
                    //
                    iURLPage = iURLPath;
                    iURLPath = "/";
                } else {
                    iURLPage = iURLPath.Substring(Position);
                    iURLPath = iURLPath.Left( Position);
                }
            }
            //
            // Divide Host from Port
            //
            Position = vbInstr(iURLHost, ":");
            if (Position == 0) {
                //
                // host not given, take a guess
                //
                switch (vbUCase(iURLProtocol)) {
                    case "FTP://":
                        iURLPort = "21";
                        break;
                    case "HTTP://":
                    case "HTTPS://":
                        iURLPort = "80";
                        break;
                    default:
                        iURLPort = "80";
                        break;
                }
            } else {
                iURLPort = iURLHost.Substring(Position);
                iURLHost = iURLHost.Left( Position - 1);
            }
            Position = vbInstr(1, iURLPage, "?");
            if (Position > 0) {
                iURLQueryString = iURLPage.Substring(Position - 1);
                iURLPage = iURLPage.Left( Position - 1);
            }
            Protocol = iURLProtocol;
            Host = iURLHost;
            Port = iURLPort;
            Path = iURLPath;
            Page = iURLPage;
            QueryString = iURLQueryString;
        }
        //
        //
        //
        public static DateTime DecodeGMTDate(string GMTDate) {
            DateTime tempDecodeGMTDate = default(DateTime);
            //
            double YearPart = 0;
            double HourPart = 0;
            //
            tempDecodeGMTDate = Convert.ToDateTime("12:00:00 AM");
            if (!string.IsNullOrEmpty(GMTDate)) {
                HourPart = encodeNumber(GMTDate.Substring(5, 11));
                if (dateController.IsDate(HourPart)) {
                    YearPart = encodeNumber(GMTDate.Substring(17, 8));
                    if (dateController.IsDate(YearPart)) {
                        tempDecodeGMTDate = DateTime.FromOADate(YearPart + (HourPart + 4) / 24);
                    }
                }
            }
            return tempDecodeGMTDate;
        }
        //
        //
        //
        //Public shared Function  EncodeGMTDate(ByVal MSDate As Date) As String
        //    EncodeGMTDate = ""
        //End Function
        //
        //=================================================================================
        // Get the value of a name in a string of name value pairs parsed with vrlf and =
        //   the legacy line delimiter was a '&' -> name1=value1&name2=value2"
        //   new format is "name1=value1 crlf name2=value2 crlf ..."
        //   There can be no extra spaces between the delimiter, the name and the "="
        //=================================================================================
        //
        public static string GetArgument(string Name, string ArgumentString, string DefaultValue = "", string Delimiter = "") {
            string tempGetArgument = null;
            //
            string WorkingString = null;
            string iDefaultValue = null;
            int NameLength = 0;
            int ValueStart = 0;
            int ValueEnd = 0;
            bool IsQuoted = false;
            //
            // determine delimiter
            //
            if (string.IsNullOrEmpty(Delimiter)) {
                //
                // If not explicit
                //
                if (vbInstr(1, ArgumentString, "\r\n") != 0) {
                    //
                    // crlf can only be here if it is the delimiter
                    //
                    Delimiter = "\r\n";
                } else {
                    //
                    // either only one option, or it is the legacy '&' delimit
                    //
                    Delimiter = "&";
                }
            }
            iDefaultValue = DefaultValue;
            WorkingString = ArgumentString;
            tempGetArgument = iDefaultValue;
            if (!string.IsNullOrEmpty(WorkingString)) {
                WorkingString = Delimiter + WorkingString + Delimiter;
                ValueStart = vbInstr(1, WorkingString, Delimiter + Name + "=", 1);
                if (ValueStart != 0) {
                    NameLength = Name.Length;
                    ValueStart = ValueStart + Delimiter.Length + NameLength + 1;
                    if (WorkingString.Substring(ValueStart - 1, 1) == "\"") {
                        IsQuoted = true;
                        ValueStart = ValueStart + 1;
                    }
                    if (IsQuoted) {
                        ValueEnd = vbInstr(ValueStart, WorkingString, "\"" + Delimiter);
                    } else {
                        ValueEnd = vbInstr(ValueStart, WorkingString, Delimiter);
                    }
                    if (ValueEnd == 0) {
                        tempGetArgument = WorkingString.Substring(ValueStart - 1);
                    } else {
                        tempGetArgument = WorkingString.Substring(ValueStart - 1, ValueEnd - ValueStart);
                    }
                }
            }
            return tempGetArgument;
        }
        //
        //=================================================================================
        //   Get a Random Long Value
        //=================================================================================
        //
        public static int GetRandomInteger(coreClass cpCore) {
            int RandomBase = 1; ;
            int RandomLimit = encodeInteger( (Math.Pow(2, 31)) - RandomBase - 1);
            return cpCore.random.Next(RandomBase, RandomLimit);
        }
        //
        //=================================================================================
        // fix for isDataTableOk
        //=================================================================================
        //
        public static bool isDataTableOk(DataTable dt) {
            return (dt.Rows.Count > 0);
        }
        //
        //=================================================================================
        // fix for closeRS
        //=================================================================================
        //
        public static void closeDataTable(DataTable dt) {
            // nothing needed
            //dt.Clear()
            dt.Dispose();
        }
        //
        //=============================================================================
        // Create the part of the sql where clause that is modified by the user
        //   WorkingQuery is the original querystring to change
        //   QueryName is the name part of the name pair to change
        //   If the QueryName is not found in the string, ignore call
        //=============================================================================
        //
        //Public shared Function ModifyQueryString(ByVal WorkingQuery As String, ByVal QueryName As String, ByVal QueryValue As String, Optional ByVal AddIfMissing As Boolean = True) As String
        //    '
        //    If vbInstr(1, WorkingQuery, "?") Then
        //        ModifyQueryString = ModifyLinkQueryString(WorkingQuery, QueryName, QueryValue, AddIfMissing)
        //    Else
        //        ModifyQueryString = Mid(ModifyLinkQueryString("?" & WorkingQuery, QueryName, QueryValue, AddIfMissing), 2)
        //    End If
        //End Function
        //
        //=============================================================================
        //   Modify a querystring name/value pair in a Link
        //=============================================================================
        //
        public static string ModifyLinkQueryString(string Link, string QueryName, string QueryValue, bool AddIfMissing = true) {
            string tempModifyLinkQueryString = null;
            //
            string[] Element = ("").Split(',');
            int ElementCount = 0;
            int ElementPointer = 0;
            string[] NameValue = null;
            string UcaseQueryName = null;
            bool ElementFound = false;
            string QueryString = null;
            //
            if (vbInstr(1, Link, "?") != 0) {
                tempModifyLinkQueryString = Link.Left( vbInstr(1, Link, "?") - 1);
                QueryString = Link.Substring(tempModifyLinkQueryString.Length + 1);
            } else {
                tempModifyLinkQueryString = Link;
                QueryString = "";
            }
            UcaseQueryName = vbUCase(EncodeRequestVariable(QueryName));
            if (!string.IsNullOrEmpty(QueryString)) {
                Element = QueryString.Split('&');
                ElementCount = Element.GetUpperBound(0) + 1;
                for (ElementPointer = 0; ElementPointer < ElementCount; ElementPointer++) {
                    NameValue = Element[ElementPointer].Split('=');
                    if (NameValue.GetUpperBound(0) == 1) {
                        if (vbUCase(NameValue[0]) == UcaseQueryName) {
                            if (string.IsNullOrEmpty(QueryValue)) {
                                Element[ElementPointer] = "";
                            } else {
                                Element[ElementPointer] = QueryName + "=" + QueryValue;
                            }
                            ElementFound = true;
                            break;
                        }
                    }
                }
            }
            if (!ElementFound && (!string.IsNullOrEmpty(QueryValue))) {
                //
                // element not found, it needs to be added
                //
                if (AddIfMissing) {
                    if (string.IsNullOrEmpty(QueryString)) {
                        QueryString = EncodeRequestVariable(QueryName) + "=" + EncodeRequestVariable(QueryValue);
                    } else {
                        QueryString = QueryString + "&" + EncodeRequestVariable(QueryName) + "=" + EncodeRequestVariable(QueryValue);
                    }
                }
            } else {
                //
                // element found
                //
                QueryString = string.Join("&", Element);
                if ((!string.IsNullOrEmpty(QueryString)) && (string.IsNullOrEmpty(QueryValue))) {
                    //
                    // element found and needs to be removed
                    //
                    QueryString = vbReplace(QueryString, "&&", "&");
                    if (QueryString.Left( 1) == "&") {
                        QueryString = QueryString.Substring(1);
                    }
                    if (QueryString.Substring(QueryString.Length - 1) == "&") {
                        QueryString = QueryString.Left( QueryString.Length - 1);
                    }
                }
            }
            if (!string.IsNullOrEmpty(QueryString)) {
                tempModifyLinkQueryString = tempModifyLinkQueryString + "?" + QueryString;
            }
            return tempModifyLinkQueryString;
        }
        //
        //=================================================================================
        //
        //=================================================================================
        //
        public static string GetIntegerString(int Value, int DigitCount) {
            string tempGetIntegerString = null;
            if (sizeof(int) <= DigitCount) {
                tempGetIntegerString = new string('0', DigitCount - Value.ToString().Length) + Value.ToString();
            } else {
                tempGetIntegerString = Value.ToString();
            }
            return tempGetIntegerString;
        }
        //
        //==========================================================================================
        //   Format the current error object into a standard string
        //==========================================================================================
        //
        public static int GetProcessID() {
            Process Instance = Process.GetCurrentProcess();
            //
            return Instance.Id;
        }
        //
        //==========================================================================================
        //   Test if a test string is in a delimited string
        //==========================================================================================
        //
        public static bool IsInDelimitedString(string DelimitedString, string TestString, string Delimiter) {
            return (0 != vbInstr(1, Delimiter + DelimitedString + Delimiter, Delimiter + TestString + Delimiter, 1));
        }
        //
        //========================================================================
        // EncodeURL
        //
        //   Encodes only what is to the left of the first ?
        //   All URL path characters are assumed to be correct (/:#)
        //========================================================================
        //
        public static string EncodeURL(string Source) {
            return WebUtility.UrlEncode(Source);
            // ##### removed to catch err<>0 problem //On Error //Resume Next
            //
            //Dim URLSplit() As String
            //Dim LeftSide As String
            //Dim RightSide As String
            //
            //EncodeURL = Source
            //If Source <> "" Then
            //    URLSplit = Split(Source, "?")
            //    EncodeURL = URLSplit(0)
            //    EncodeURL = vbReplace(EncodeURL, "%", "%25")
            //    '
            //    EncodeURL = vbReplace(EncodeURL, """", "%22")
            //    EncodeURL = vbReplace(EncodeURL, " ", "%20")
            //    EncodeURL = vbReplace(EncodeURL, "$", "%24")
            //    EncodeURL = vbReplace(EncodeURL, "+", "%2B")
            //    EncodeURL = vbReplace(EncodeURL, ",", "%2C")
            //    EncodeURL = vbReplace(EncodeURL, ";", "%3B")
            //    EncodeURL = vbReplace(EncodeURL, "<", "%3C")
            //    EncodeURL = vbReplace(EncodeURL, "=", "%3D")
            //    EncodeURL = vbReplace(EncodeURL, ">", "%3E")
            //    EncodeURL = vbReplace(EncodeURL, "@", "%40")
            //    If UBound(URLSplit) > 0 Then
            //        EncodeURL = EncodeURL & "?" & EncodeQueryString(URLSplit(1))
            //    End If
            //End If
            //
        }
        //
        //========================================================================
        // EncodeQueryString
        //
        //   This routine encodes the URL QueryString to conform to rules
        //========================================================================
        //
        public static string EncodeQueryString(string Source) {
            string tempEncodeQueryString = null;
            // ##### removed to catch err<>0 problem //On Error //Resume Next
            //
            string[] QSSplit = null;
            int QSPointer = 0;
            string[] NVSplit = null;
            string NV = null;
            //
            tempEncodeQueryString = "";
            if (!string.IsNullOrEmpty(Source)) {
                QSSplit = Source.Split('&');
                for (QSPointer = 0; QSPointer <= QSSplit.GetUpperBound(0); QSPointer++) {
                    NV = QSSplit[QSPointer];
                    if (!string.IsNullOrEmpty(NV)) {
                        NVSplit = NV.Split('=');
                        if (NVSplit.GetUpperBound(0) == 0) {
                            NVSplit[0] = EncodeRequestVariable(NVSplit[0]);
                            tempEncodeQueryString = tempEncodeQueryString + "&" + NVSplit[0];
                        } else {
                            NVSplit[0] = EncodeRequestVariable(NVSplit[0]);
                            NVSplit[1] = EncodeRequestVariable(NVSplit[1]);
                            tempEncodeQueryString = tempEncodeQueryString + "&" + NVSplit[0] + "=" + NVSplit[1];
                        }
                    }
                }
                if (!string.IsNullOrEmpty(tempEncodeQueryString)) {
                    tempEncodeQueryString = tempEncodeQueryString.Substring(1);
                }
            }
            //
            return tempEncodeQueryString;
        }
        //
        //========================================================================
        // EncodeRequestVariable
        //
        //   This routine encodes a request variable for a URL Query String
        //       ...can be the requestname or the requestvalue
        //========================================================================
        //
        public static string EncodeRequestVariable(string Source) {
            if (Source == null) {
                return "";
            } else {
                return System.Uri.EscapeDataString(Source);
            }
            // ##### removed to catch err<>0 problem //On Error //Resume Next
            //
            //Dim SourcePointer As Integer
            //Dim Character As String
            //Dim LocalSource As String
            //
            //EncodeRequestVariable = ""
            //If Source <> "" Then
            //    LocalSource = Source
            //    ' "+" is an allowed character for filenames. If you add it, the wrong file will be looked up
            //    'LocalSource = vbReplace(LocalSource, " ", "+")
            //    For SourcePointer = 1 To Len(LocalSource)
            //        Character = Mid(LocalSource, SourcePointer, 1)
            //        ' "%" added so if this is called twice, it will not destroy "%20" values
            //        If False Then
            //            'End If
            //            'If Character = " " Then
            //            EncodeRequestVariable += "+"
            //        ElseIf vbInstr(1, "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789./:-_!*()", Character, vbTextCompare) <> 0 Then
            //            'ElseIf vbInstr(1, "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789./:?#-_!~*'()%", Character, vbTextCompare) <> 0 Then
            //            EncodeRequestVariable += Character
            //        Else
            //            EncodeRequestVariable += "%" & Hex(Asc(Character))
            //        End If
            //    Next
            //End If
            //
        }
        //
        //========================================================================
        // DecodeHTML
        //
        //   Convert HTML equivalent characters to their equivalents
        //========================================================================
        //
        //Public shared Function DecodeHTML(ByVal Source As String) As String
        //    ' ##### removed to catch err<>0 problem //On Error //Resume Next
        //    '
        //    Dim Pos As Integer
        //    Dim s As String
        //    Dim CharCodeString As String
        //    Dim CharCode As Integer
        //    Dim PosEnd As Integer
        //    '
        //    ' 11/26/2009 - basically re-wrote it, I commented the old one out below
        //    '
        //    s = Source
        //    '
        //    ' numeric entities
        //    '
        //    Pos = Len(s)
        //    Pos = InStrRev(s, "&#", Pos)
        //    Do While Pos <> 0
        //        CharCodeString = ""
        //        If Mid(s, Pos + 3, 1) = ";" Then
        //            CharCodeString = Mid(s, Pos + 2, 1)
        //            PosEnd = Pos + 4
        //        ElseIf Mid(s, Pos + 4, 1) = ";" Then
        //            CharCodeString = Mid(s, Pos + 2, 2)
        //            PosEnd = Pos + 5
        //        ElseIf Mid(s, Pos + 5, 1) = ";" Then
        //            CharCodeString = Mid(s, Pos + 2, 3)
        //            PosEnd = Pos + 6
        //        End If
        //        If CharCodeString <> "" Then
        //            If vbIsNumeric(CharCodeString) Then
        //                CharCode = CLng(CharCodeString)
        //                s = Mid(s, 1, Pos - 1) & Chr(CharCode) & Mid(s, PosEnd)
        //            End If
        //        End If
        //        '
        //        Pos = InStrRev(s, "&#", Pos)
        //    Loop
        //    '
        //    ' character entities (at least the most common )
        //    '
        //    s = vbReplace(s, "&lt;", "<")
        //    s = vbReplace(s, "&gt;", ">")
        //    s = vbReplace(s, "&quot;", """")
        //    s = vbReplace(s, "&apos;", "'")
        //    '
        //    ' always last
        //    '
        //    s = vbReplace(s, "&amp;", "&")
        //    '
        //    DecodeHTML = s
        //    '
        //End Function
        //
        //========================================================================
        // AddSpanClass
        //
        //   Adds a span around the copy with the class name provided
        //========================================================================
        //
        public static string AddSpan(string Copy, string ClassName) => "<span class=\"" + ClassName + "\">" + Copy + "</span>";
        public static string AddSpan(string Copy) => "<span>" + Copy + "</span>";
        public static string AddSpan() => "<span/>";
        //
        //========================================================================
        // DecodeResponseVariable
        //
        //   Converts a querystring name or value back into the characters it represents
        //   This is the same code as the decodeurl
        //========================================================================
        //
        public static string DecodeResponseVariable(string Source) {
            string tempDecodeResponseVariable = null;
            //
            int Position = 0;
            string ESCString = null;
            string Digit0 = null;
            string Digit1 = null;
            //Dim iURL As String
            //
            //iURL = Source
            // plus to space only applies for query component of a URL, but %99 encoding works for both
            //DecodeResponseVariable = vbReplace(iURL, "+", " ")
            tempDecodeResponseVariable = Source;
            Position = vbInstr(1, tempDecodeResponseVariable, "%");
            while (Position != 0) {
                ESCString = tempDecodeResponseVariable.Substring(Position - 1, 3);
                Digit0 = vbUCase(ESCString.Substring(1, 1));
                Digit1 = vbUCase(ESCString.Substring(2, 1));
                if (((string.CompareOrdinal(Digit0, "0") >= 0) && (string.CompareOrdinal(Digit0, "9") <= 0)) || ((string.CompareOrdinal(Digit0, "A") >= 0) && (string.CompareOrdinal(Digit0, "F") <= 0))) {
                    if (((string.CompareOrdinal(Digit1, "0") >= 0) && (string.CompareOrdinal(Digit1, "9") <= 0)) || ((string.CompareOrdinal(Digit1, "A") >= 0) && (string.CompareOrdinal(Digit1, "F") <= 0))) {
                        int ESCValue = 0;
                        try {
                            ESCValue = Convert.ToInt32(ESCString.Substring(1), 16);
                        } catch  {
                            // do nothing -- just put a 0 in as the escape code was not valid, a data problem not a code problem
                        }
                        tempDecodeResponseVariable = tempDecodeResponseVariable.Left(Position - 1) + Convert.ToChar(ESCValue) + tempDecodeResponseVariable.Substring(Position + 2);
                    }
                }
                Position = vbInstr(Position + 1, tempDecodeResponseVariable, "%");
            }
            //
            return tempDecodeResponseVariable;
        }
        //
        //========================================================================
        // DecodeURL
        //   Converts a querystring from an Encoded URL (with %20 and +), to non incoded (with spaced)
        //========================================================================
        //
        public static string DecodeURL(string Source) {
            string tempDecodeURL = null;
            // ##### removed to catch err<>0 problem //On Error //Resume Next
            //
            int Position = 0;
            string ESCString = null;
            int ESCValue = 0;
            string Digit0 = null;
            string Digit1 = null;
            //Dim iURL As String
            //
            //iURL = Source
            // plus to space only applies for query component of a URL, but %99 encoding works for both
            //DecodeURL = vbReplace(iURL, "+", " ")
            tempDecodeURL = Source;
            Position = vbInstr(1, tempDecodeURL, "%");
            while (Position != 0) {
                ESCString = tempDecodeURL.Substring(Position - 1, 3);
                Digit0 = vbUCase(ESCString.Substring(1, 1));
                Digit1 = vbUCase(ESCString.Substring(2, 1));
                if (((string.CompareOrdinal(Digit0, "0") >= 0) && (string.CompareOrdinal(Digit0, "9") <= 0)) || ((string.CompareOrdinal(Digit0, "A") >= 0) && (string.CompareOrdinal(Digit0, "F") <= 0))) {
                    if (((string.CompareOrdinal(Digit1, "0") >= 0) && (string.CompareOrdinal(Digit1, "9") <= 0)) || ((string.CompareOrdinal(Digit1, "A") >= 0) && (string.CompareOrdinal(Digit1, "F") <= 0))) {
                        ESCValue = int.Parse("&H" + ESCString.Substring(1));
                        tempDecodeURL = vbReplace(tempDecodeURL, ESCString, Convert.ToChar(ESCValue));
                    }
                }
                Position = vbInstr(Position + 1, tempDecodeURL, "%");
            }
            //
            return tempDecodeURL;
        }
        //
        //========================================================================
        // GetFirstNonZeroDate
        //
        //   Converts a querystring name or value back into the characters it represents
        //========================================================================
        //
        public static DateTime GetFirstNonZeroDate(DateTime Date0, DateTime Date1) {
            DateTime tempGetFirstNonZeroDate = default(DateTime);
            // ##### removed to catch err<>0 problem //On Error //Resume Next
            //
            DateTime NullDate;
            //
            NullDate = DateTime.MinValue;
            if (Date0 == NullDate) {
                if (Date1 == NullDate) {
                    //
                    // Both 0, return 0
                    //
                    tempGetFirstNonZeroDate = NullDate;
                } else {
                    //
                    // Date0 is NullDate, return Date1
                    //
                    tempGetFirstNonZeroDate = Date1;
                }
            } else {
                if (Date1 == NullDate) {
                    //
                    // Date1 is nulldate, return Date0
                    //
                    tempGetFirstNonZeroDate = Date0;
                } else if (Date0 < Date1) {
                    //
                    // Date0 is first
                    //
                    tempGetFirstNonZeroDate = Date0;
                } else {
                    //
                    // Date1 is first
                    //
                    tempGetFirstNonZeroDate = Date1;
                }
            }
            //
            return tempGetFirstNonZeroDate;
        }
        //
        //========================================================================
        // getFirstposition
        //
        //   returns 0 if both are zero
        //   returns 1 if the first integer is non-zero and less then the second
        //   returns 2 if the second integer is non-zero and less then the first
        //========================================================================
        //
        public static int GetFirstNonZeroInteger(int Integer1, int Integer2) {
            int tempGetFirstNonZeroInteger = 0;
            // ##### removed to catch err<>0 problem //On Error //Resume Next
            //
            if (Integer1 == 0) {
                if (Integer2 == 0) {
                    //
                    // Both 0, return 0
                    //
                    tempGetFirstNonZeroInteger = 0;
                } else {
                    //
                    // Integer1 is 0, return Integer2
                    //
                    tempGetFirstNonZeroInteger = 2;
                }
            } else {
                if (Integer2 == 0) {
                    //
                    // Integer2 is 0, return Integer1
                    //
                    tempGetFirstNonZeroInteger = 1;
                } else if (Integer1 < Integer2) {
                    //
                    // Integer1 is first
                    //
                    tempGetFirstNonZeroInteger = 1;
                } else {
                    //
                    // Integer2 is first
                    //
                    tempGetFirstNonZeroInteger = 2;
                }
            }
            //
            return tempGetFirstNonZeroInteger;
        }
        //
        //========================================================================
        // splitDelimited
        //   returns the result of a Split, except it honors quoted text
        //   if a quote is found, it is assumed to also be a delimiter ( 'this"that"theother' = 'this "that" theother' )
        //========================================================================
        //
        public static string[] SplitDelimited(string WordList, string Delimiter) {
            // ##### removed to catch err<>0 problem //On Error //Resume Next
            //
            string[] QuoteSplit = null;
            int QuoteSplitCount = 0;
            int QuoteSplitPointer = 0;
            bool InQuote = false;
            string[] Out = null;
            int OutPointer = 0;
            int OutSize = 0;
            string[] SpaceSplit = null;
            int SpaceSplitCount = 0;
            int SpaceSplitPointer = 0;
            string Fragment = null;
            //
            OutPointer = 0;
            Out = new string[1];
            OutSize = 1;
            if (!string.IsNullOrEmpty(WordList)) {
                QuoteSplit = stringSplit( WordList, @"\" );
                QuoteSplitCount = QuoteSplit.GetUpperBound(0) + 1;
                InQuote = (string.IsNullOrEmpty(WordList.Left( 1)));
                for (QuoteSplitPointer = 0; QuoteSplitPointer < QuoteSplitCount; QuoteSplitPointer++) {
                    Fragment = QuoteSplit[QuoteSplitPointer];
                    if (string.IsNullOrEmpty(Fragment)) {
                        //
                        // empty fragment
                        // this is a quote at the end, or two quotes together
                        // do not skip to the next out pointer
                        //
                        if (OutPointer >= OutSize) {
                            OutSize = OutSize + 10;
                            Array.Resize(ref Out, OutSize + 1);
                        }
                        //OutPointer = OutPointer + 1
                    } else {
                        if (!InQuote) {
                            SpaceSplit = Fragment.Split(Delimiter.ToCharArray());
                            SpaceSplitCount = SpaceSplit.GetUpperBound(0) + 1;
                            for (SpaceSplitPointer = 0; SpaceSplitPointer < SpaceSplitCount; SpaceSplitPointer++) {
                                if (OutPointer >= OutSize) {
                                    OutSize = OutSize + 10;
                                    Array.Resize(ref Out, OutSize + 1);
                                }
                                Out[OutPointer] = Out[OutPointer] + SpaceSplit[SpaceSplitPointer];
                                if (SpaceSplitPointer != (SpaceSplitCount - 1)) {
                                    //
                                    // divide output between splits
                                    //
                                    OutPointer = OutPointer + 1;
                                    if (OutPointer >= OutSize) {
                                        OutSize = OutSize + 10;
                                        Array.Resize(ref Out, OutSize + 1);
                                    }
                                }
                            }
                        } else {
                            Out[OutPointer] = Out[OutPointer] + "\"" + Fragment + "\"";
                        }
                    }
                    InQuote = !InQuote;
                }
            }
            Array.Resize(ref Out, OutPointer + 1);
            //
            //
            return Out;
            //
        }
        //
        //
        //
        public static string GetYesNo(bool Key) {
            string tempGetYesNo = null;
            if (Key) {
                tempGetYesNo = "Yes";
            } else {
                tempGetYesNo = "No";
            }
            return tempGetYesNo;
        }
        //
        //
        //
        //Public shared Function GetFilename(ByVal PathFilename As String) As String
        //    Dim Position As Integer
        //    '
        //    GetFilename = PathFilename
        //    Position = InStrRev(GetFilename, "/")
        //    If Position <> 0 Then
        //        GetFilename = Mid(GetFilename, Position + 1)
        //    End If
        //End Function
        //        '
        //        '
        //        '
        public static string StartTable(int Padding, int Spacing, int Border, string ClassStyle = "") {
            return "<table border=\"" + Border + "\" cellpadding=\"" + Padding + "\" cellspacing=\"" + Spacing + "\" class=\"" + ClassStyle + "\" width=\"100%\">";
        }
        //        '
        //        '
        //        '
        public static string StartTableRow() {
            return "<tr>";
        }
        //        '
        //        '
        //        '
        public static string StartTableCell(string Width = "", int ColSpan = 0, bool EvenRow = false, string Align = "", string BGColor = "") {
            string tempStartTableCell = null;
            tempStartTableCell = "";
            if (!string.IsNullOrEmpty(Width)) {
                tempStartTableCell = " width=\"" + Width + "\"";
            }
            if (!string.IsNullOrEmpty(BGColor)) {
                tempStartTableCell = tempStartTableCell + " bgcolor=\"" + BGColor + "\"";
            } else if (EvenRow) {
                tempStartTableCell = tempStartTableCell + " class=\"ccPanelRowEven\"";
            } else {
                tempStartTableCell = tempStartTableCell + " class=\"ccPanelRowOdd\"";
            }
            if (ColSpan != 0) {
                tempStartTableCell = tempStartTableCell + " colspan=\"" + ColSpan + "\"";
            }
            if (!string.IsNullOrEmpty(Align)) {
                tempStartTableCell = tempStartTableCell + " align=\"" + Align + "\"";
            }
            return "<TD" + tempStartTableCell + ">";
        }
        //        '
        //        '
        //        '
        public static string GetTableCell(string Copy, string Width = "", int ColSpan = 0, bool EvenRow = false, string Align = "", string BGColor = "") {
            return StartTableCell(Width, ColSpan, EvenRow, Align, BGColor) + Copy + kmaEndTableCell;
        }
        //        '
        //        '
        //        '
        public static string GetTableRow(string Cell, int ColSpan = 0, bool EvenRow = false) {
            return StartTableRow() + GetTableCell(Cell, "100%", ColSpan, EvenRow) + kmaEndTableRow;
        }
        //
        // remove the host and approotpath, leaving the "active" path and all else
        //
        public static string ConvertShortLinkToLink(string URL, string PathPagePrefix) {
            string tempConvertShortLinkToLink = null;
            tempConvertShortLinkToLink = URL;
            if (!string.IsNullOrEmpty(URL) & !string.IsNullOrEmpty(PathPagePrefix)) {
                if (vbInstr(1, tempConvertShortLinkToLink, PathPagePrefix, 1) == 1) {
                    tempConvertShortLinkToLink = tempConvertShortLinkToLink.Substring(PathPagePrefix.Length);
                }
            }
            return tempConvertShortLinkToLink;
        }
        //
        // ------------------------------------------------------------------------------------------------------
        //   Preserve URLs that do not start HTTP or HTTPS
        //   Preserve URLs from other sites (offsite)
        //   Preserve HTTP://ServerHost/ServerVirtualPath/Files/ in all cases
        //   Convert HTTP://ServerHost/ServerVirtualPath/folder/page -> /folder/page
        //   Convert HTTP://ServerHost/folder/page -> /folder/page
        // ------------------------------------------------------------------------------------------------------
        //
        public static string ConvertLinkToShortLink(string URL, string ServerHost, string ServerVirtualPath) {
            //
            string BadString = "";
            string GoodString = "";
            string Protocol = "";
            string WorkingLink = "";
            //
            WorkingLink = URL;
            //
            // ----- Determine Protocol
            //
            if (vbInstr(1, WorkingLink, "HTTP://", 1) == 1) {
                //
                // HTTP
                //
                Protocol = WorkingLink.Left( 7);
            } else if (vbInstr(1, WorkingLink, "HTTPS://", 1) == 1) {
                //
                // HTTPS
                //
                // an ssl link can not be shortened
                return WorkingLink;
            }
            if (!string.IsNullOrEmpty(Protocol)) {
                //
                // ----- Protcol found, determine if is local
                //
                GoodString = Protocol + ServerHost;
                if (WorkingLink.IndexOf(GoodString, System.StringComparison.OrdinalIgnoreCase)  != -1) {
                    //
                    // URL starts with Protocol ServerHost
                    //
                    GoodString = Protocol + ServerHost + ServerVirtualPath + "/files/";
                    if (WorkingLink.IndexOf(GoodString, System.StringComparison.OrdinalIgnoreCase)  != -1) {
                        //
                        // URL is in the virtual files directory
                        //
                        BadString = GoodString;
                        GoodString = ServerVirtualPath + "/files/";
                        WorkingLink = vbReplace(WorkingLink, BadString, GoodString, 1, 99, 1);
                    } else {
                        //
                        // URL is not in files virtual directory
                        //
                        BadString = Protocol + ServerHost + ServerVirtualPath + "/";
                        GoodString = "/";
                        WorkingLink = vbReplace(WorkingLink, BadString, GoodString, 1, 99, 1);
                        //
                        BadString = Protocol + ServerHost + "/";
                        GoodString = "/";
                        WorkingLink = vbReplace(WorkingLink, BadString, GoodString, 1, 99, 1);
                    }
                }
            }
            return WorkingLink;
        }
        //
        // Correct the link for the virtual path, either add it or remove it
        //
        public static string EncodeAppRootPath(string Link, string VirtualPath, string AppRootPath, string ServerHost) {
            string tempEncodeAppRootPath = null;
            //
            string Protocol = "";
            string Host = "";
            string Path = "";
            string Page = "";
            string QueryString = "";
            bool VirtualHosted = false;
            //
            tempEncodeAppRootPath = Link;
            if ((tempEncodeAppRootPath.IndexOf(ServerHost, System.StringComparison.OrdinalIgnoreCase)  != -1) || (Link.IndexOf("/") + 1 == 1)) {
                //If (InStr(1, EncodeAppRootPath, ServerHost, vbTextCompare) <> 0) And (InStr(1, Link, "/") <> 0) Then
                //
                // This link is onsite and has a path
                //
                VirtualHosted = (AppRootPath.IndexOf(VirtualPath, System.StringComparison.OrdinalIgnoreCase)  != -1);
                if (VirtualHosted && (Link.IndexOf(AppRootPath, System.StringComparison.OrdinalIgnoreCase) + 1 == 1)) {
                    //
                    // quick - virtual hosted and link starts at AppRootPath
                    //
                } else if ((!VirtualHosted) && (Link.Left( 1) == "/") && (Link.IndexOf(AppRootPath, System.StringComparison.OrdinalIgnoreCase) + 1 == 1)) {
                    //
                    // quick - not virtual hosted and link starts at Root
                    //
                } else {
                    SeparateURL(Link, ref Protocol, ref Host, ref Path, ref Page, ref QueryString);
                    if (VirtualHosted) {
                        //
                        // Virtual hosted site, add VirualPath if it is not there
                        //
                        if (vbInstr(1, Path, AppRootPath, 1) == 0) {
                            if (Path == "/") {
                                Path = AppRootPath;
                            } else {
                                Path = AppRootPath + Path.Substring(1);
                            }
                        }
                    } else {
                        //
                        // Root hosted site, remove virtual path if it is there
                        //
                        if (vbInstr(1, Path, AppRootPath, 1) != 0) {
                            Path = vbReplace(Path, AppRootPath, "/");
                        }
                    }
                    tempEncodeAppRootPath = Protocol + Host + Path + Page + QueryString;
                }
            }
            return tempEncodeAppRootPath;
        }
        //
        // Return just the tablename from a tablename reference (database.object.tablename->tablename)
        //
        public static string GetDbObjectTableName(string DbObject) {
            string tempGetDbObjectTableName = null;
            int Position = 0;
            //
            tempGetDbObjectTableName = DbObject;
            Position = tempGetDbObjectTableName.LastIndexOf(".") + 1;
            if (Position > 0) {
                tempGetDbObjectTableName = tempGetDbObjectTableName.Substring(Position);
            }
            return tempGetDbObjectTableName;
        }
        //
        //
        //
        public static string GetLinkedText(string AnchorTag, string AnchorText) {
            string tempGetLinkedText = null;
            //
            string UcaseAnchorText = null;
            int LinkPosition = 0;
            string iAnchorTag = null;
            string iAnchorText = null;
            //
            tempGetLinkedText = "";
            iAnchorTag = AnchorTag;
            iAnchorText = AnchorText;
            UcaseAnchorText = vbUCase(iAnchorText);
            if ((!string.IsNullOrEmpty(iAnchorTag)) & (!string.IsNullOrEmpty(iAnchorText))) {
                LinkPosition = UcaseAnchorText.LastIndexOf("<LINK>") + 1;
                if (LinkPosition == 0) {
                    tempGetLinkedText = iAnchorTag + iAnchorText + "</A>";
                } else {
                    tempGetLinkedText = iAnchorText;
                    LinkPosition = UcaseAnchorText.LastIndexOf("</LINK>") + 1;
                    while (LinkPosition > 1) {
                        tempGetLinkedText = tempGetLinkedText.Left( LinkPosition - 1) + "</A>" + tempGetLinkedText.Substring(LinkPosition + 6);
                        LinkPosition = UcaseAnchorText.LastIndexOf("<LINK>", LinkPosition - 2) + 1;
                        if (LinkPosition != 0) {
                            tempGetLinkedText = tempGetLinkedText.Left( LinkPosition - 1) + iAnchorTag + tempGetLinkedText.Substring(LinkPosition + 5);
                        }
                        LinkPosition = UcaseAnchorText.LastIndexOf("</LINK>", LinkPosition - 1) + 1;
                    }
                }
            }
            //
            return tempGetLinkedText;
        }
        //
        public static string EncodeInitialCaps(string Source) {
            string tempEncodeInitialCaps = null;
            string[] SegSplit = null;
            int SegPtr = 0;
            int SegMax = 0;
            //
            tempEncodeInitialCaps = "";
            if (!string.IsNullOrEmpty(Source)) {
                SegSplit = Source.Split(' ');
                SegMax = SegSplit.GetUpperBound(0);
                if (SegMax >= 0) {
                    for (SegPtr = 0; SegPtr <= SegMax; SegPtr++) {
                        SegSplit[SegPtr] = vbUCase(SegSplit[SegPtr].Left( 1)) + vbLCase(SegSplit[SegPtr].Substring(1));
                    }
                }
                tempEncodeInitialCaps = string.Join(" ", SegSplit);
            }
            return tempEncodeInitialCaps;
        }
        //
        //
        //
        public static string RemoveTag(string Source, string TagName) {
            string tempRemoveTag = null;
            int Pos = 0;
            int PosEnd = 0;
            tempRemoveTag = Source;
            Pos = vbInstr(1, Source, "<" + TagName, 1);
            if (Pos != 0) {
                PosEnd = vbInstr(Pos, Source, ">");
                if (PosEnd > 0) {
                    tempRemoveTag = Source.Left( Pos - 1) + Source.Substring(PosEnd);
                }
            }
            return tempRemoveTag;
        }
        //
        //
        //
        public static string RemoveStyleTags(string Source) {
            string tempRemoveStyleTags = null;
            tempRemoveStyleTags = Source;
            while (vbInstr(1, tempRemoveStyleTags, "<style", 1) != 0) {
                tempRemoveStyleTags = RemoveTag(tempRemoveStyleTags, "style");
            }
            while (vbInstr(1, tempRemoveStyleTags, "</style", 1) != 0) {
                tempRemoveStyleTags = RemoveTag(tempRemoveStyleTags, "/style");
            }
            return tempRemoveStyleTags;
        }
        //
        //
        //
        public static string GetSingular(string PluralSource) {
            string tempGetSingular = null;
            //
            bool UpperCase = false;
            string LastCharacter = null;
            //
            tempGetSingular = PluralSource;
            if (tempGetSingular.Length > 1) {
                LastCharacter = tempGetSingular.Substring(tempGetSingular.Length - 1);
                if (LastCharacter != vbUCase(LastCharacter)) {
                    UpperCase = true;
                }
                if (vbUCase(tempGetSingular.Substring(tempGetSingular.Length - 3)) == "IES") {
                    if (UpperCase) {
                        tempGetSingular = tempGetSingular.Left( tempGetSingular.Length - 3) + "Y";
                    } else {
                        tempGetSingular = tempGetSingular.Left( tempGetSingular.Length - 3) + "y";
                    }
                } else if (vbUCase(tempGetSingular.Substring(tempGetSingular.Length - 2)) == "SS") {
                    // nothing
                } else if (vbUCase(tempGetSingular.Substring(tempGetSingular.Length - 1)) == "S") {
                    tempGetSingular = tempGetSingular.Left( tempGetSingular.Length - 1);
                } else {
                    // nothing
                }
            }
            return tempGetSingular;
        }
        //
        //
        //
        public static string EncodeJavascript(string Source) {
            string tempEncodeJavascript = null;
            //
            tempEncodeJavascript = Source;
            tempEncodeJavascript = vbReplace(tempEncodeJavascript, "\\", "\\\\");
            tempEncodeJavascript = vbReplace(tempEncodeJavascript, "'", "\\'");
            //EncodeJavascript = vbReplace(EncodeJavascript, "'", "'+""'""+'")
            tempEncodeJavascript = vbReplace(tempEncodeJavascript, "\r\n", "\\n");
            tempEncodeJavascript = vbReplace(tempEncodeJavascript, "\r", "\\n");
            return vbReplace(tempEncodeJavascript, "\n", "\\n");
            //
        }
        /// <summary>
        /// returns a 1-based index into the comma seperated ListOfItems where Item is found
        /// </summary>
        /// <param name="Item"></param>
        /// <param name="ListOfItems"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        public static int GetListIndex(string Item, string ListOfItems) {
            int tempGetListIndex = 0;
            //
            string[] Items = null;
            string LcaseItem = null;
            string LcaseList = null;
            int Ptr = 0;
            //
            tempGetListIndex = 0;
            if (!string.IsNullOrEmpty(ListOfItems)) {
                LcaseItem = vbLCase(Item);
                LcaseList = vbLCase(ListOfItems);
                Items = SplitDelimited(LcaseList, ",");
                for (Ptr = 0; Ptr <= Items.GetUpperBound(0); Ptr++) {
                    if (Items[Ptr] == LcaseItem) {
                        tempGetListIndex = Ptr + 1;
                        break;
                    }
                }
            }
            //
            return tempGetListIndex;
        }
        //
        //
        //
        public static int encodeInteger(object expression) {
            int result = 0;
            if (expression == null) {
                return 0;
            } else {
                double number = 0;
                string tmpString = expression.ToString();
                if ( String.IsNullOrWhiteSpace( tmpString)) {
                    return 0;
                } else {
                    if (Int32.TryParse(tmpString, out result)) {
                        return result;
                    } else if (Double.TryParse(tmpString, out number)) {
                        return Convert.ToInt32(number);
                    } else {
                        return 0;
                    }
                }
            }

            //if (expression == null) {
            //    return 0;
            //}
            //string result = expression.ToString();



            //else if ((expression is int) || (expression is Int16) || (expression is Int32)) {
            //    return Convert.ToInt32(expression);
            //} else if ((expression is decimal) || (expression is float) || (expression is double) || (expression is Int64)) {
            //    double? result = expression as double?;
            //    if ( result == null ) {
            //        return 0;
            //    } else {
            //        return Conversion.toIN;
            //    }

            //    Int32 output = 0;
            //    if (Int32.TryParse((string)expression, out output)) {
            //        return output;
            //    } else {
            //        return 0;
            //    }
            //} else if (expression is string) {
            //    double output = 0;
            //    if (double.TryParse((string)expression, out output)) {
            //        return    ( output );
            //    } else {
            //        return 0;
            //    }

            //    return double.TryParse((string)expression, out output);
            //} else {
            //    return false;
            //}


            //tempEncodeInteger = 0;
            //if (Expression.IsNumeric()) {
            //    tempEncodeInteger = Convert.ToInt32(Expression);
            //} else if (Expression is bool) {
            //    if ((bool)Expression) {
            //        tempEncodeInteger = 1;
            //    }
            //}
            //return tempEncodeInteger;
        }
        //
        //
        //
        public static double encodeNumber(object Expression) {
            double tempEncodeNumber = 0;
            tempEncodeNumber = 0;
            if (Expression.IsNumeric()) {
                tempEncodeNumber = Convert.ToDouble(Expression);
            } else if (Expression is bool) {
                if ((bool)Expression) {
                    tempEncodeNumber = 1;
                }
            }
            return tempEncodeNumber;
        }
        //
        //====================================================================================================
        //
        public static string encodeText(object Expression) {
            if (!(Expression is DBNull)) {
                if (Expression != null) {
                    return Expression.ToString();
                }
            }
            return string.Empty;
        }
        //
        //====================================================================================================
        //
        public static bool encodeBoolean(object Expression) {
            bool tempEncodeBoolean = false;
            tempEncodeBoolean = false;
            if (Expression == null) {
                tempEncodeBoolean = false;
            } else if (Expression is bool) {
                tempEncodeBoolean = (bool)Expression;
            } else if (Expression.IsNumeric()) {
                tempEncodeBoolean = (encodeText(Expression) != "0");
            } else if (Expression is string) {
                switch (Expression.ToString().ToLower().Trim()) {
                    case "on":
                    case "yes":
                    case "true":
                        tempEncodeBoolean = true;
                        break;
                }
            }
            return tempEncodeBoolean;
        }
        //
        //====================================================================================================
        //
        public static DateTime encodeDate(object Expression) {
            DateTime tempEncodeDate = default(DateTime);
            tempEncodeDate = DateTime.MinValue;
            if (dateController.IsDate(Expression)) {
                tempEncodeDate = Convert.ToDateTime(Expression);
                //If EncodeDate < #1/1/1990# Then
                //    EncodeDate = Date.MinValue
                //End If
            }
            return tempEncodeDate;
        }
        //
        //========================================================================
        //   Gets the next line from a string, and removes the line
        //========================================================================
        //
        public static string getLine(ref string Body) {
            string returnFirstLine = Body;
            try {
                int EOL = 0;
                int NextCR = 0;
                int NextLF = 0;
                int BOL = 0;
                //
                NextCR = vbInstr(1, Body, "\r");
                NextLF = vbInstr(1, Body, "\n");

                if (NextCR != 0 | NextLF != 0) {
                    if (NextCR != 0) {
                        if (NextLF != 0) {
                            if (NextCR < NextLF) {
                                EOL = NextCR - 1;
                                if (NextLF == NextCR + 1) {
                                    BOL = NextLF + 1;
                                } else {
                                    BOL = NextCR + 1;
                                }

                            } else {
                                EOL = NextLF - 1;
                                BOL = NextLF + 1;
                            }
                        } else {
                            EOL = NextCR - 1;
                            BOL = NextCR + 1;
                        }
                    } else {
                        EOL = NextLF - 1;
                        BOL = NextLF + 1;
                    }
                    returnFirstLine = Body.Left( EOL);
                    Body = Body.Substring(BOL - 1);
                } else {
                    returnFirstLine = Body;
                    Body = "";
                }
            } catch (Exception) {}
            return returnFirstLine;
        }
        //
        //
        //
        public static string runProcess(coreClass cpCore, string Cmd, string Arguments = "", bool WaitForReturn = false) {
            string returnResult = "";
            Process p = new Process();
            //
            logController.appendLog(cpCore, "ccCommonModule.runProcess, cmd=[" + Cmd + "], Arguments=[" + Arguments + "], WaitForReturn=[" + WaitForReturn + "]");
            //
            p.StartInfo.FileName = Cmd;
            p.StartInfo.Arguments = Arguments;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.CreateNoWindow = true;
            p.StartInfo.ErrorDialog = false;
            p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            if (WaitForReturn) {
                p.StartInfo.RedirectStandardOutput = true;
            }
            p.Start();
            if (WaitForReturn) {
                p.WaitForExit(1000 * 60 * 5);
                returnResult = p.StandardOutput.ReadToEnd();
            }

            p.Dispose();
            //
            return returnResult;
        }
        //
        //Public shared sub runProcess(cp.core,cmd As String, Optional ignore As Object = "", Optional waitforreturn As Boolean = False)
        //    Call runProcess(cp.core,cmd, waitforreturn)
        //    'Dim ShellObj As Object
        //    'ShellObj = CreateObject("WScript.Shell")
        //    'If Not (ShellObj Is Nothing) Then
        //    '    Call ShellObj.Run(Cmd, 0, WaitForReturn)
        //    'End If
        //    'ShellObj = Nothing
        //End Sub
        //
        //------------------------------------------------------------------------------------------------------------
        //   use only internally
        //
        //   encode an argument to be used in a name=value& (N-V-A) string
        //
        //   an argument can be any one of these is this format:
        //       Arg0,Arg1,Arg2,Arg3,Name=Value&Name=Value[Option1|Option2]descriptor
        //
        //   to create an nva string
        //       string = encodeNvaArgument( name ) & "=" & encodeNvaArgument( value ) & "&"
        //
        //   to decode an nva string
        //       split on ampersand then on equal, and genericController.decodeNvaArgument() each part
        //
        //------------------------------------------------------------------------------------------------------------
        //
        public static string encodeNvaArgument(string Arg) {
            string a = Arg;
            if (!string.IsNullOrEmpty(a)) {
                a = vbReplace(a, "\r\n", "#0013#");
                a = vbReplace(a, "\n", "#0013#");
                a = vbReplace(a, "\r", "#0013#");
                a = vbReplace(a, "&", "#0038#");
                a = vbReplace(a, "=", "#0061#");
                a = vbReplace(a, ",", "#0044#");
                a = vbReplace(a, "\"", "#0034#");
                a = vbReplace(a, "'", "#0039#");
                a = vbReplace(a, "|", "#0124#");
                a = vbReplace(a, "[", "#0091#");
                a = vbReplace(a, "]", "#0093#");
                a = vbReplace(a, ":", "#0058#");
            }
            return a;
        }
        //
        //------------------------------------------------------------------------------------------------------------
        //   use only internally
        //       decode an argument removed from a name=value& string
        //       see encodeNvaArgument for details on how to use this
        //------------------------------------------------------------------------------------------------------------
        //
        public static string decodeNvaArgument(string EncodedArg) {
            string a;
            //
            a = EncodedArg;
            a = vbReplace(a, "#0058#", ":");
            a = vbReplace(a, "#0093#", "]");
            a = vbReplace(a, "#0091#", "[");
            a = vbReplace(a, "#0124#", "|");
            a = vbReplace(a, "#0039#", "'");
            a = vbReplace(a, "#0034#", "\"");
            a = vbReplace(a, "#0044#", ",");
            a = vbReplace(a, "#0061#", "=");
            a = vbReplace(a, "#0038#", "&");
            a = vbReplace(a, "#0013#", "\r\n");
            return a;
        }
        //        '
        //        '========================================================================
        //        '   encodeSQLText
        //        '========================================================================
        //        '
        //        Public shared Function app.EncodeSQLText(ByVal expression As Object) As String
        //            Dim returnString As String = ""
        //            If expression Is Nothing Then
        //                returnString = "null"
        //            Else
        //                returnString = encodeText(expression)
        //                If returnString = "" Then
        //                    returnString = "null"
        //                Else
        //                    returnString = "'" & vbReplace(returnString, "'", "''") & "'"
        //                End If
        //            End If
        //            Return returnString
        //        End Function
        //        '
        //        '========================================================================
        //        '   encodeSQLLongText
        //        '========================================================================
        //        '
        //        Public shared Function app.EncodeSQLText(ByVal expression As Object) As String
        //            Dim returnString As String = ""
        //            If expression Is Nothing Then
        //                returnString = "null"
        //            Else
        //                returnString = encodeText(expression)
        //                If returnString = "" Then
        //                    returnString = "null"
        //                Else
        //                    returnString = "'" & vbReplace(returnString, "'", "''") & "'"
        //                End If
        //            End If
        //            Return returnString
        //        End Function
        //        '
        //        '========================================================================
        //        '   encodeSQLDate
        //        '       encode a date variable to go in an sql expression
        //        '========================================================================
        //        '
        //        Public shared Function app.EncodeSQLDate(ByVal expression As Object) As String
        //            Dim returnString As String = ""
        //            Dim expressionDate As Date = Date.MinValue
        //            If expression Is Nothing Then
        //                returnString = "null"
        //            ElseIf Not IsDate(expression) Then
        //                returnString = "null"
        //            Else
        //                If IsDBNull(expression) Then
        //                    returnString = "null"
        //                Else
        //                    expressionDate =  EncodeDate(expression)
        //                    If (expressionDate = Date.MinValue) Then
        //                        returnString = "null"
        //                    Else
        //                        returnString = "'" & Year(expressionDate) & Right("0" & Month(expressionDate), 2) & Right("0" & Day(expressionDate), 2) & " " & Right("0" & expressionDate.Hour, 2) & ":" & Right("0" & expressionDate.Minute, 2) & ":" & Right("0" & expressionDate.Second, 2) & ":" & Right("00" & expressionDate.Millisecond, 3) & "'"
        //                    End If
        //                End If
        //            End If
        //            Return returnString
        //        End Function
        //        '
        //        '========================================================================
        //        '   encodeSQLNumber
        //        '       encode a number variable to go in an sql expression
        //        '========================================================================
        //        '
        //        Public shared Function app.EncodeSQLNumber(ByVal expression As Object) As String
        //            Dim returnString As String = ""
        //            Dim expressionNumber As Double = 0
        //            If expression Is Nothing Then
        //                returnString = "null"
        //            ElseIf VarType(expression) = vbBoolean Then
        //                If expression Then
        //                    returnString = SQLTrue
        //                Else
        //                    returnString = SQLFalse
        //                End If
        //            ElseIf Not vbIsNumeric(expression) Then
        //                returnString = "null"
        //            Else
        //                returnString = expression.ToString
        //            End If
        //            Return returnString
        //            Exit Function
        //            '
        //            ' ----- Error Trap
        //            '
        ////ErrorTrap:
        //        End Function
        //        '
        //        '========================================================================
        //        '   encodeSQLBoolean
        //        '       encode a boolean variable to go in an sql expression
        //        '========================================================================
        //        '
        //        Public shared Function app.EncodeSQLBoolean(ByVal ExpressionVariant As Object) As String
        //            '
        //            'Dim src As String
        //            '
        //            app.EncodeSQLBoolean = SQLFalse
        //            If EncodeBoolean(ExpressionVariant) Then
        //                app.EncodeSQLBoolean = SQLTrue
        //            End If
        //        End Function
        //
        //=================================================================================
        //   Renamed to catch all the cases that used it in addons
        //
        //   Do not use this routine in Addons to get the addon option string value
        //   to get the value in an option string, use cmc.csv_getAddonOption("name")
        //
        // Get the value of a name in a string of name value pairs parsed with vrlf and =
        //   the legacy line delimiter was a '&' -> name1=value1&name2=value2"
        //   new format is "name1=value1 crlf name2=value2 crlf ..."
        //   There can be no extra spaces between the delimiter, the name and the "="
        //=================================================================================
        //
        public static string getSimpleNameValue(string Name, string ArgumentString, string DefaultValue, string Delimiter) {
            string tempgetSimpleNameValue = null;
            //
            string WorkingString = null;
            string iDefaultValue = null;
            int NameLength = 0;
            int ValueStart = 0;
            int ValueEnd = 0;
            bool IsQuoted = false;
            //
            // determine delimiter
            //
            if (string.IsNullOrEmpty(Delimiter)) {
                //
                // If not explicit
                //
                if (vbInstr(1, ArgumentString, "\r\n") != 0) {
                    //
                    // crlf can only be here if it is the delimiter
                    //
                    Delimiter = "\r\n";
                } else {
                    //
                    // either only one option, or it is the legacy '&' delimit
                    //
                    Delimiter = "&";
                }
            }
            iDefaultValue = DefaultValue;
            WorkingString = ArgumentString;
            tempgetSimpleNameValue = iDefaultValue;
            if (!string.IsNullOrEmpty(WorkingString)) {
                WorkingString = Delimiter + WorkingString + Delimiter;
                ValueStart = vbInstr(1, WorkingString, Delimiter + Name + "=", 1);
                if (ValueStart != 0) {
                    NameLength = Name.Length;
                    ValueStart = ValueStart + Delimiter.Length + NameLength + 1;
                    if (WorkingString.Substring(ValueStart - 1, 1) == "\"") {
                        IsQuoted = true;
                        ValueStart = ValueStart + 1;
                    }
                    if (IsQuoted) {
                        ValueEnd = vbInstr(ValueStart, WorkingString, "\"" + Delimiter);
                    } else {
                        ValueEnd = vbInstr(ValueStart, WorkingString, Delimiter);
                    }
                    if (ValueEnd == 0) {
                        tempgetSimpleNameValue = WorkingString.Substring(ValueStart - 1);
                    } else {
                        tempgetSimpleNameValue = WorkingString.Substring(ValueStart - 1, ValueEnd - ValueStart);
                    }
                }
            }
            return tempgetSimpleNameValue;
        }
        ////==========================================================================================================================
        ////   To convert from site license to server licenses, we still need the URLEncoder in the site license
        ////   This routine generates a site license that is just the URL encoder.
        ////==========================================================================================================================
        ////
        //public static string GetURLEncoder()
        //{
        //	Microsoft.VisualBasic.VBMath.Randomize();
        //	return encodeText(EncodeInteger(Math.Floor(EncodeNumber(1 + (Microsoft.VisualBasic.VBMath.Rnd() * 8))))) + encodeText(EncodeInteger(Math.Floor(EncodeNumber(1 + (Microsoft.VisualBasic.VBMath.Rnd() * 8))))) + encodeText(EncodeInteger(Math.Floor(EncodeNumber(1000000000 + (Microsoft.VisualBasic.VBMath.Rnd() * 899999999)))));
        //}
        //
        //
        //
        public static string getIpAddressList() {
            string ipAddressList = "";
            IPHostEntry host;
            //
            host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (IPAddress ip in host.AddressList) {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork) {
                    ipAddressList += "," + ip.ToString();
                }
            }
            if (!string.IsNullOrEmpty(ipAddressList)) {
                ipAddressList = ipAddressList.Substring(1);
            }
            return ipAddressList;
        }
        //
        //
        //
        //Public shared Function GetAddonRootPath() As String
        //    Dim testPath As String
        //    '
        //    GetAddonRootPath = "addons"
        //    If vbInstr(1, GetAddonRootPath, "\github\", vbTextCompare) <> 0 Then
        //        '
        //        ' debugging - change program path to dummy path so addon builds all copy to
        //        '
        //        testPath = Environ$("programfiles(x86)")
        //        If testPath = "" Then
        //            testPath = Environ$("programfiles")
        //        End If
        //        GetAddonRootPath = testPath & "\kma\contensive\addons"
        //    End If
        //End Function
        //
        //
        //
        public static bool IsNull(object source) {
            return (source == null);
        }
        //
        //
        //
        //Public shared Function isNothing(ByVal source As Object) As Boolean
        //    Return IsNothing(source)
        //    'Dim returnIsEmpty As Boolean = True
        //    'Try
        //    '    If Not IsNothing(source) Then

        //    '    End If
        //    'Catch ex As Exception
        //    '    '
        //    'End Try
        //    'Return returnIsEmpty
        //End Function
        //
        //
        //
        public static bool isMissing(object source) {
            return false;
        }
        //
        // convert date to number of seconds since 1/1/1990
        //
        public static int dateToSeconds(DateTime sourceDate) {
            int returnSeconds = 0;
            DateTime oldDate = new DateTime(1900, 1, 1);
            if (sourceDate.CompareTo(oldDate) > 0) {
                returnSeconds = encodeInteger(sourceDate.Subtract(oldDate).TotalSeconds);
            }
            return returnSeconds;
        }
        ////
        //// ==============================================================================
        //// true if there is a previous instance of this application running
        //// ==============================================================================
        ////
        //public static bool PrevInstance() {
        //    if (System.Diagnostics.Process.GetProcessesByName(Diagnostics.Process.GetCurrentProcess.ProcessName).GetUpperBound(0) > 0) {
        //        return true;
        //    } else {
        //        return false;
        //    }
        //}
        //
        //====================================================================================================
        /// <summary>
        /// Encode a date to minvalue, if date is < minVAlue,m set it to minvalue, if date < 1/1/1990 (the beginning of time), it returns date.minvalue
        /// </summary>
        /// <param name="sourceDate"></param>
        /// <returns></returns>
        public static DateTime encodeDateMinValue(DateTime sourceDate) {
            if (sourceDate <= new DateTime(1000, 1, 1)) {
                return DateTime.MinValue;
            }
            return sourceDate;
        }
        //
        //====================================================================================================
        /// <summary>
        /// Return true if a date is the mindate, else return false 
        /// </summary>
        /// <param name="sourceDate"></param>
        /// <returns></returns>
        public static bool isMinDate(DateTime sourceDate) {
            return encodeDateMinValue(sourceDate) == DateTime.MinValue;
        }
        //
        //Public Shared Function getVirtualTableFieldPath(ByVal TableName As String, ByVal FieldName As String) As String
        //    Dim result As String = TableName & "/" & FieldName & "/"
        //    Return result.Replace(" ", "_").Replace(".", "_")
        //End Function
        //Public Shared Function getVirtualTableFieldIdPath(ByVal TableName As String, ByVal FieldName As String, ByVal RecordID As Integer) As String
        //    Return getVirtualTableFieldPath(TableName, FieldName) & RecordID.ToString().PadLeft(12, "0"c) & "/"
        //End Function
        //
        //========================================================================
        // ----- Create a filename for the Virtual Directory
        //   Do not allow spaces.
        //   If the content supports authoring, the filename returned will be for the
        //   current authoring record.
        //========================================================================
        //
        //Public Shared Function getVirtualRecordPathFilename(ByVal TableName As String, ByVal FieldName As String, ByVal RecordID As Integer, ByVal OriginalFilename As String, ByVal fieldType As Integer) As String
        //    Dim result As String = ""
        //    '
        //    Dim iOriginalFilename As String = OriginalFilename.Replace(" ", "_").Replace(".", "_")
        //    If OriginalFilename <> "" Then
        //        result = getVirtualTableFieldIdPath(TableName, FieldName, RecordID) & OriginalFilename
        //    Else
        //        Dim IdFilename As String = CStr(RecordID)
        //        If RecordID = 0 Then
        //            IdFilename = getGUID().Replace("{", "").Replace("}", "").Replace("-", "")
        //        Else
        //            IdFilename = RecordID.ToString().PadLeft(12, "0"c)
        //        End If
        //        Select Case fieldType
        //            Case FieldTypeIdFileCSS
        //                result = getVirtualTableFieldPath(TableName, FieldName) & IdFilename & ".css"
        //            Case FieldTypeIdFileXML
        //                result = getVirtualTableFieldPath(TableName, FieldName) & IdFilename & ".xml"
        //            Case FieldTypeIdFileJavascript
        //                result = getVirtualTableFieldPath(TableName, FieldName) & IdFilename & ".js"
        //            Case FieldTypeIdFileHTML
        //                result = getVirtualTableFieldPath(TableName, FieldName) & IdFilename & ".html"
        //            Case Else
        //                result = getVirtualTableFieldPath(TableName, FieldName) & IdFilename & ".txt"
        //        End Select
        //    End If
        //    Return result
        //End Function
        //
        //====================================================================================================
        // the the name of the current executable
        //====================================================================================================
        //
        public static string getAppExeName() {
            return System.IO.Path.GetFileName(System.Windows.Forms.Application.ExecutablePath);
        }
        //
        //====================================================================================================
        /// <summary>
        /// convert a dtaTable to list of string 
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static List<string> convertDataTableColumntoItemList(DataTable dt) {
            List<string> returnString = new List<string>();
            foreach (DataRow dr in dt.Rows) {
                returnString.Add(dr[0].ToString().ToLower());
            }
            return returnString;
        }
        //
        //====================================================================================================
        /// <summary>
        /// convert a dtaTable to a comma delimited list of column 0
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static string convertDataTableColumntoItemCommaList(DataTable dt) {
            string returnString = "";
            foreach (DataRow dr in dt.Rows) {
                returnString += "," + dr[0].ToString();
            }
            if (returnString.Length > 0) {
                returnString = returnString.Substring(1);
            }
            return returnString;
        }
        //
        //====================================================================================================
        /// <summary>
        /// returns true or false if a string is located within another string. Similar to indexof but returns true/false 
        /// </summary>
        /// <param name="start"></param>
        /// <param name="haystack"></param>
        /// <param name="needle"></param>
        /// <param name="ignore"></param>
        /// <returns></returns>
        public static bool isInStr(int start, string haystack, string needle, Microsoft.VisualBasic.CompareMethod ignore = Microsoft.VisualBasic.CompareMethod.Text) {
            return (haystack.IndexOf(needle, start - 1, System.StringComparison.OrdinalIgnoreCase) + 1 >= 0);
        }
        public static bool isInStr(int start, string haystack, string needle ) {
            return (haystack.IndexOf(needle, start - 1, System.StringComparison.OrdinalIgnoreCase) + 1 >= 0);
        }
        //
        //====================================================================================================
        /// <summary>
        /// Convert a route to the anticipated format (lowercase,no leading /, no trailing /)
        /// </summary>
        /// <param name="route"></param>
        /// <returns></returns>
        public static string normalizeRoute(string route) {
            string normalizedRoute = route.ToLower().Trim();
            try {
                if (string.IsNullOrEmpty(normalizedRoute)) {
                    normalizedRoute = "";
                } else {
                    normalizedRoute = genericController.convertToUnixSlash(normalizedRoute);
                    while (normalizedRoute.IndexOf("//") >= 0) {
                        normalizedRoute = normalizedRoute.Replace("//", "/");
                    }
                    if (normalizedRoute.Left( 1).Equals("/")) {
                        normalizedRoute = normalizedRoute.Substring(1);
                    }
                    if (normalizedRoute.Substring(normalizedRoute.Length - 1, 1) == "/") {
                        normalizedRoute = normalizedRoute.Left( normalizedRoute.Length - 1);
                    }
                }
            } catch (Exception ex) {
                throw new ApplicationException("Unexpected exception in normalizeRoute(route=[" + route + "])", ex);
            }
            return normalizedRoute;
        }
        //
        //========================================================================
        //   converts a virtual file into a filename
        //       - in local mode, the cdnFiles can be mapped to a virtual folder in appRoot
        //           -- see appConfig.cdnFilesVirtualFolder
        //       convert all / to \
        //       if it includes "://", it is a root file
        //       if it starts with "/", it is already root relative
        //       else (if it start with a file or a path), add the publicFileContentPathPrefix
        //========================================================================
        //
        public static string convertCdnUrlToCdnPathFilename(string cdnUrl) {
            //
            // this routine was originally written to handle modes that were not adopted (content file absolute and relative URLs)
            // leave it here as a simple slash converter in case other conversions are needed later
            //
            return vbReplace(cdnUrl, "/", "\\");
        }
        //
        //==============================================================================
        public static bool isGuid(string Source) {
            bool returnValue = false;
            try {
                if ((Source.Length == 38) && (Source.Left( 1) == "{") && (Source.Substring(Source.Length - 1) == "}")) {
                    //
                    // Good to go
                    //
                    returnValue = true;
                } else if ((Source.Length == 36) && (Source.IndexOf(" ")  == -1)) {
                    //
                    // might be valid with the brackets, add them
                    //
                    returnValue = true;
                    //source = "{" & source & "}"
                } else if (Source.Length == 32) {
                    //
                    // might be valid with the brackets and the dashes, add them
                    //
                    returnValue = true;
                    //source = "{" & Mid(source, 1, 8) & "-" & Mid(source, 9, 4) & "-" & Mid(source, 13, 4) & "-" & Mid(source, 17, 4) & "-" & Mid(source, 21) & "}"
                } else {
                    //
                    // not valid
                    //
                    returnValue = false;
                    //        source = ""
                }
            } catch (Exception ex) {
                throw new ApplicationException("Exception in isGuid", ex);
            }
            return returnValue;
        }
        // todo refactor out vb fpo
        //====================================================================================================
        /// <summary>
        /// temp methods to convert from vb, refactor out
        /// </summary>
        /// <param name="string1"></param>
        /// <param name="string2"></param>
        /// <param name="text1Binary2"></param>
        /// <returns></returns>
        public static int vbInstr(int startBase1, string string1, string string2, int text1Binary2) {
            if (string.IsNullOrEmpty(string1)) {
                return 0;
            } else {
                if (startBase1 < 1) {
                    throw new ArgumentException("Instr() start must be > 0.");
                } else {
                    if (text1Binary2 == 1) {
                        return string1.IndexOf(string2, startBase1 - 1, StringComparison.CurrentCultureIgnoreCase) + 1;
                    } else {
                        return string1.IndexOf(string2, startBase1 - 1, StringComparison.CurrentCulture) + 1;
                    }
                }
            }
        }
        public static int vbInstr(string string1, string string2, int text1Binary2) {
            return vbInstr(1, string1, string2, text1Binary2);
        }
        //
        public static int vbInstr(string string1, string string2) {
            return vbInstr(1, string1, string2, 2);
        }
        //
        public static int vbInstr(int startBase1, string string1, string string2) {
            return vbInstr(startBase1, string1, string2, 2);
        }

        //
        //====================================================================================================
        //
        public static string vbReplace(string expression, string oldValue, string replacement, int startIgnore, int countIgnore, int compare) {
            if (string.IsNullOrEmpty(expression)) {
                return expression;
            } else if (string.IsNullOrEmpty(oldValue)) {
                return expression;
            } else {
                if (compare == 2) {
                    return expression.Replace(oldValue, replacement);
                } else {

                    StringBuilder sb = new StringBuilder();
                    int previousIndex = 0;
                    int Index = expression.IndexOf(oldValue, StringComparison.CurrentCultureIgnoreCase);
                    while (Index != -1) {
                        sb.Append(expression.Substring(previousIndex, Index - previousIndex));
                        sb.Append(replacement);
                        Index += oldValue.Length;
                        previousIndex = Index;
                        Index = expression.IndexOf(oldValue, Index, StringComparison.CurrentCultureIgnoreCase);
                    }
                    sb.Append(expression.Substring(previousIndex));
                    return sb.ToString();
                }
            }
        }
        //
        public static string vbReplace(string expression, string find, int replacement) { return vbReplace(expression, find, replacement.ToString()); }
        //
        public static string vbReplace(string expression, string find, string replacement) { return vbReplace(expression, find, replacement, 1, 9999, 1); }
        //
        //====================================================================================================
        /// <summary>
        /// Visual Basic UCase
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string vbUCase(string source) {
            if (string.IsNullOrEmpty(source)) {
                return "";
            } else {
                return source.ToUpper();
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// Visual Basic LCase
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string vbLCase(string source) {
            if (string.IsNullOrEmpty(source)) {
                return "";
            } else {
                return source.ToLower();
            }
        }


        //
        //====================================================================================================
        /// <summary>
        /// Visual Basic Len()
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static int vbLen(string source) {
            if (string.IsNullOrEmpty(source)) {
                return 0;
            } else {
                return source.Length;
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// Visual Basic Mid()
        /// </summary>
        /// <param name="source"></param>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        public static string vbMid(string source, int startIndex) {
            if (string.IsNullOrEmpty(source)) {
                return "";
            } else {
                return source.Substring(startIndex);
            }
        }
        //
        //====================================================================================================
        /// <summary>
        /// Visual Basic Mid()
        /// </summary>
        /// <param name="source"></param>
        /// <param name="startIndex"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string vbMid(string source, int startIndex, int length) {
            if (string.IsNullOrEmpty(source)) {
                return "";
            } else {
                return source.Substring(startIndex, length);
            }
        }

        //
        //====================================================================================================
        /// <summary>
        /// convert a date to the number of days since date.min
        /// </summary>
        /// <param name="srcDate"></param>
        /// <returns></returns>
        public static int convertDateToDayPtr(DateTime srcDate) {
            return encodeInteger(dateController.DateDiff(dateController.DateInterval.Day, srcDate, DateTime.MinValue));
        }
        //
        //====================================================================================================
        /// <summary>
        /// Encodes an argument in an Addon OptionString (QueryString) for all non-allowed characters
        /// Arg0,Arg1,Arg2,Arg3,Name=Value&Name=VAlue[Option1|Option2]
        /// call this before parsing them together
        /// call decodeAddonOptionArgument after parsing them apart
        /// </summary>
        /// <param name="Arg"></param>
        /// <returns></returns>
        //------------------------------------------------------------------------------------------------------------
        //
        public static string encodeLegacyOptionStringArgument(string Arg) {
            string a = "";
            if (!string.IsNullOrEmpty(Arg)) {
                a = Arg;
                a = genericController.vbReplace(a, "\r\n", "#0013#");
                a = genericController.vbReplace(a, "\n", "#0013#");
                a = genericController.vbReplace(a, "\r", "#0013#");
                a = genericController.vbReplace(a, "&", "#0038#");
                a = genericController.vbReplace(a, "=", "#0061#");
                a = genericController.vbReplace(a, ",", "#0044#");
                a = genericController.vbReplace(a, "\"", "#0034#");
                a = genericController.vbReplace(a, "'", "#0039#");
                a = genericController.vbReplace(a, "|", "#0124#");
                a = genericController.vbReplace(a, "[", "#0091#");
                a = genericController.vbReplace(a, "]", "#0093#");
                a = genericController.vbReplace(a, ":", "#0058#");
            }
            return a;
        }
        //
        //====================================================================================================
        //
        public static string createGuid() {
            return "{" + Guid.NewGuid().ToString() + "}";
        }
        //
        //====================================================================================================
        /// <summary>
        /// Returns true if the argument is a string in guid compatible format
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public static bool common_isGuid(string guid) {
            bool returnIsGuid = false;
            try {
                returnIsGuid = (guid.Length == 38) && (guid.Left( 1) == "{") && (guid.Substring(guid.Length - 1) == "}");
            } catch (Exception ex) {
                throw (ex);
            }
            return returnIsGuid;
        }
        //
        //========================================================================
        // main_encodeCookieName
        //   replace invalid cookie characters with %hex
        //========================================================================
        //
        public static string main_encodeCookieName(string Source) {
            return EncodeURL(Source);
            //			string result = "";

            //			int SourcePointer = 0;
            //			string Character = null;
            //			string localSource = null;
            //			//
            //			if (!string.IsNullOrEmpty(Source))
            //			{
            //				localSource = Source;
            ////todo  NOTE: The ending condition of VB 'For' loops is tested only on entry to the loop. Instant C# has created a temporary variable in order to use the initial value of Len(localSource) for every iteration:
            //				int tempVar = localSource.Length;
            //				for (SourcePointer = 1; SourcePointer <= tempVar; SourcePointer++)
            //				{
            //					Character = localSource.Substring(SourcePointer - 1, 1);
            //					if (genericController.vbInstr(1, "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789.-_!*()", Character, 1) != 0)
            //					{
            //						result = result + Character;
            //					}
            //					else
            //					{
            //						result = result + "%" + encodeText(Microsoft.VisualBasic.Strings.Asc(Character), 16).ToUpper();
            //					}
            //				}
            //			}
            //			//
            //			return result;
        }
        public static string main_GetYesNo(bool InputValue) {
            if (InputValue) {
                return "Yes";
            } else {
                return "No";
            }
        }
        //
        //
        //=============================================================================
        // ----- Return the value associated with the name given
        //   NameValueString is a string of Name=Value pairs, separated by spaces or "&"
        //   If Name is not given, returns ""
        //   If Name present but no value, returns true (as if Name=true)
        //   If Name = Value, it returns value
        //=============================================================================
        //
        public static string main_GetNameValue_Internal(coreClass cpcore, string NameValueString, string Name) {
            string result = "";
            //
            string NameValueStringWorking = NameValueString;
            string UcaseNameValueStringWorking = NameValueString.ToUpper();
            string[] pairs = null;
            int PairCount = 0;
            int PairPointer = 0;
            string[] PairSplit = null;
            //
            if ((!string.IsNullOrEmpty(NameValueString)) & (!string.IsNullOrEmpty(Name))) {
                while (genericController.vbInstr(1, NameValueStringWorking, " =") != 0) {
                    NameValueStringWorking = genericController.vbReplace(NameValueStringWorking, " =", "=");
                }
                while (genericController.vbInstr(1, NameValueStringWorking, "= ") != 0) {
                    NameValueStringWorking = genericController.vbReplace(NameValueStringWorking, "= ", "=");
                }
                while (genericController.vbInstr(1, NameValueStringWorking, "& ") != 0) {
                    NameValueStringWorking = genericController.vbReplace(NameValueStringWorking, "& ", "&");
                }
                while (genericController.vbInstr(1, NameValueStringWorking, " &") != 0) {
                    NameValueStringWorking = genericController.vbReplace(NameValueStringWorking, " &", "&");
                }
                NameValueStringWorking = NameValueString + "&";
                UcaseNameValueStringWorking = genericController.vbUCase(NameValueStringWorking);
                //
                result = "";
                if (!string.IsNullOrEmpty(NameValueStringWorking)) {
                    pairs = NameValueStringWorking.Split('&');
                    PairCount = pairs.GetUpperBound(0) + 1;
                    for (PairPointer = 0; PairPointer < PairCount; PairPointer++) {
                        PairSplit = pairs[PairPointer].Split('=');
                        if (genericController.vbUCase(PairSplit[0]) == genericController.vbUCase(Name)) {
                            if (PairSplit.GetUpperBound(0) > 0) {
                                result = PairSplit[1];
                            }
                            break;
                        }
                    }
                }
            }
            return result;
        }
        //		//
        //		//=============================================================================
        //		// Cleans a text file of control characters, allowing only vblf
        //		//=============================================================================
        //		//
        //		public static string main_RemoveControlCharacters(string DirtyText)
        //		{
        //			string result = DirtyText;
        //			int Pointer = 0;
        //			int ChrTest = 0;
        //			string iDirtyText;
        //			//
        //			iDirtyText = encodeText(DirtyText);
        //			result = "";
        //			if (!string.IsNullOrEmpty(iDirtyText))
        //			{
        //				result = "";
        ////todo  NOTE: The ending condition of VB 'For' loops is tested only on entry to the loop. Instant C# has created a temporary variable in order to use the initial value of Len(iDirtyText) for every iteration:
        //				int tempVar = iDirtyText.Length;
        //				for (Pointer = 1; Pointer <= tempVar; Pointer++)
        //				{
        //                    ChrTest = iDirtyText.Substring(Pointer - 1, 1).ToCharArray()[0];
        //					if (ChrTest >= 32 && ChrTest < 128)
        //					{
        //						result = result + (int)str[i] Microsoft.VisualBasic.Strings.Chr(ChrTest);
        //					}
        //					else
        //					{
        //						switch (ChrTest)
        //						{
        //							case 9:
        //								result = result + " ";
        //								break;
        //							case 10:
        //								result = result + "\n";
        //								break;
        //						}
        //					}
        //				}
        //				//
        //				// limit CRLF to 2
        //				//
        //				while (vbInstr(result, "\n\n\n") != 0)
        //				{
        //					result = vbReplace(result, "\n\n\n", "\n\n");
        //				}
        //				//
        //				// limit spaces to 1
        //				//
        //				while (vbInstr(result, "  ") != 0)
        //				{
        //					result = vbReplace(result, "  ", " ");
        //				}
        //			}
        //			return result;
        //		}
        //
        //========================================================================
        //   convert a virtual file into a Link usable on the website:
        //       convert all \ to /
        //       if it includes "://", leave it along
        //       if it starts with "/", it is already root relative, leave it alone
        //       else (if it start with a file or a path), add the serverFilePath
        //========================================================================
        //
        public static string getCdnFileLink(coreClass cpcore, string virtualFile) {
            string returnLink = virtualFile;
            returnLink = genericController.vbReplace(returnLink, "\\", "/");
            if (genericController.vbInstr(1, returnLink, "://") != 0) {
                //
                // icon is an Absolute URL - leave it
                //
                return returnLink;
            } else if (returnLink.Left( 1) == "/") {
                //
                // icon is Root Relative, leave it
                //
                return returnLink;
            } else {
                //
                // icon is a virtual file, add the serverfilepath
                //
                return cpcore.serverConfig.appConfig.cdnFilesNetprefix + returnLink;
            }
        }
        //
        //
        //
        public static string csv_GetLinkedText(string AnchorTag, string AnchorText) {
            string result = "";
            string UcaseAnchorText = null;
            int LinkPosition = 0;
            string iAnchorTag = null;
            string iAnchorText = null;
            //
            iAnchorTag = genericController.encodeText(AnchorTag);
            iAnchorText = genericController.encodeText(AnchorText);
            UcaseAnchorText = genericController.vbUCase(iAnchorText);
            if ((!string.IsNullOrEmpty(iAnchorTag)) & (!string.IsNullOrEmpty(iAnchorText))) {
                LinkPosition = UcaseAnchorText.LastIndexOf("<LINK>") + 1;
                if (LinkPosition == 0) {
                    result = iAnchorTag + iAnchorText + "</a>";
                } else {
                    result = iAnchorText;
                    LinkPosition = UcaseAnchorText.LastIndexOf("</LINK>") + 1;
                    while (LinkPosition > 1) {
                        result = result.Left( LinkPosition - 1) + "</a>" + result.Substring(LinkPosition + 6);
                        LinkPosition = UcaseAnchorText.LastIndexOf("<LINK>", LinkPosition - 2) + 1;
                        if (LinkPosition != 0) {
                            result = result.Left( LinkPosition - 1) + iAnchorTag + result.Substring(LinkPosition + 5);
                        }
                        LinkPosition = UcaseAnchorText.LastIndexOf("</LINK>", LinkPosition - 1) + 1;
                    }
                }
            }
            return result;
        }
        //
        public static string convertNameValueDictToREquestString(Dictionary<string, string> nameValueDict) {
            string requestFormSerialized = "";
            if (nameValueDict.Count > 0) {
                foreach (KeyValuePair<string, string> kvp in nameValueDict) {
                    requestFormSerialized += "&" + EncodeURL(kvp.Key.Left( 255)) + "=" + EncodeURL(kvp.Value.Left( 255));
                    if (requestFormSerialized.Length > 255) {
                        break;
                    }
                }
            }
            return requestFormSerialized;
        }
        //
        //====================================================================================================
        /// <summary>
        /// if valid date, return the short date, else return blank string 
        /// </summary>
        /// <param name="srcDate"></param>
        /// <returns></returns>
        public static string getShortDateString(DateTime srcDate) {
            string returnString = "";
            DateTime workingDate = srcDate.MinValueIfOld();
            if (!srcDate.isOld()) {
                returnString = workingDate.ToShortDateString();
            }
            return returnString;
        }
        //
        //====================================================================================================
        //
        public static string convertToDosSlash(string path) {
            return path.Replace("/", "\\");
        }
        //
        //====================================================================================================
        //
        public static string convertToUnixSlash(string path) {
            return path.Replace("\\", "/");
        }
        //
        //====================================================================================================
        //
        public static string getPath(string PathFilename) {
            string result = PathFilename;
            if (!string.IsNullOrEmpty(result)) {
                int slashpos = PathFilename.Replace("/", "\\").LastIndexOf("\\");
                if ((slashpos >= 0) && (slashpos < PathFilename.Length)) {
                    result = PathFilename.Left( slashpos + 1);
                }
            }
            return result;
        }
        //
        //========================================================================
        // EncodeHTML
        //
        //   Convert all characters that are not allowed in HTML to their Text equivalent
        //   in preperation for use on an HTML page
        //========================================================================
        //
        public static string encodeHTML(string Source) {
            string tempencodeHTML = null;
            // ##### removed to catch err<>0 problem //On Error //Resume Next
            //
            tempencodeHTML = Source;
            tempencodeHTML = genericController.vbReplace(tempencodeHTML, "&", "&amp;");
            tempencodeHTML = genericController.vbReplace(tempencodeHTML, "<", "&lt;");
            tempencodeHTML = genericController.vbReplace(tempencodeHTML, ">", "&gt;");
            tempencodeHTML = genericController.vbReplace(tempencodeHTML, "\"", "&quot;");
            return genericController.vbReplace(tempencodeHTML, "'", "&apos;");
            //
        }
        //
        //======================================================================================
        /// <summary>
        /// Convert addon argument list to a doc property compatible dictionary of strings
        /// </summary>
        /// <param name="cpCore"></param>
        /// <param name="SrcOptionList"></param>
        /// <returns></returns>
        public static Dictionary<string, string> convertAddonArgumentstoDocPropertiesList(coreClass cpCore, string SrcOptionList) {
            Dictionary<string, string> returnList = new Dictionary<string, string>();
            try {
                string[] SrcOptions = null;
                string key = null;
                string value = null;
                int Pos = 0;
                //
                if (!string.IsNullOrEmpty(SrcOptionList)) {
                    SrcOptions = genericController.stringSplit(SrcOptionList.Replace("\r\n", "\r").Replace("\n", "\r"), "\r");
                    for (var Ptr = 0; Ptr <= SrcOptions.GetUpperBound(0); Ptr++) {
                        key = SrcOptions[Ptr].Replace("\t", "");
                        if (!string.IsNullOrEmpty(key)) {
                            value = "";
                            Pos = genericController.vbInstr(1, key, "=");
                            if (Pos > 0) {
                                value = key.Substring(Pos);
                                key = key.Left( Pos - 1);
                            }
                            returnList.Add(key, value);
                        }
                    }
                }
            } catch (Exception ex) {
                cpCore.handleException(ex);
                throw;
            }
            return returnList;
        }
        //
        //=============================================================================
        //   Return just the copy from a content page
        //=============================================================================
        //
        public static string TextDeScramble(coreClass cpcore, string Copy) {
            string returnCopy = "";
            try {
                int CPtr = 0;
                string C = null;
                int CValue = 0;
                int crc = 0;
                string Source = null;
                int Base = 0;
                const int CMin = 32;
                const int CMax = 126;
                //
                // assume this one is not converted
                //
                Source = Copy;
                Base = 50;
                //
                // First characger must be _
                // Second character is the scramble version 'a' is the starting system
                //
                if (Source.Left( 2) != "_a") {
                    returnCopy = Copy;
                } else {
                    Source = Source.Substring(2);
                    //
                    // cycle through all characters
                    //
                    for (CPtr = Source.Length - 1; CPtr >= 1; CPtr--) {
                        C = Source.Substring(CPtr - 1, 1);
                        CValue = Microsoft.VisualBasic.Strings.Asc(C);
                        crc = crc + CValue;
                        if ((CValue < CMin) || (CValue > CMax)) {
                            //
                            // if out of ascii bounds, just leave it in place
                            //
                        } else {
                            CValue = CValue - Base;
                            if (CValue < CMin) {
                                CValue = CValue + CMax - CMin + 1;
                            }
                        }
                        returnCopy = returnCopy + Microsoft.VisualBasic.Strings.Chr(CValue);
                    }
                    //
                    // Test mod
                    //
                    if ((crc % 9).ToString() != Source.Substring(Source.Length - 1, 1)) {
                        //
                        // Nope - set it back to the input
                        //
                        returnCopy = Copy;
                    }
                }
            } catch (Exception ex) {
                cpcore.handleException(ex);
                throw;
            }
            return returnCopy;
        }
        //
        //=============================================================================
        //   Return just the copy from a content page
        //=============================================================================
        //
        public static string TextScramble(coreClass cpcore, string Copy) {
            string returnCopy = "";
            try {
                int CPtr = 0;
                string C = null;
                int CValue = 0;
                int crc = 0;
                int Base = 0;
                const int CMin = 32;
                const int CMax = 126;
                //
                // scrambled starts with _
                //
                Base = 50;
                //todo  NOTE: The ending condition of VB 'For' loops is tested only on entry to the loop. Instant C# has created a temporary variable in order to use the initial value of Len(Copy) for every iteration:
                int tempVar = Copy.Length;
                for (CPtr = 1; CPtr <= tempVar; CPtr++) {
                    C = Copy.Substring(CPtr - 1, 1);
                    CValue = Microsoft.VisualBasic.Strings.Asc(C);
                    if ((CValue < CMin) || (CValue > CMax)) {
                        //
                        // if out of ascii bounds, just leave it in place
                        //
                    } else {
                        CValue = CValue + Base;
                        if (CValue > CMax) {
                            CValue = CValue - CMax + CMin - 1;
                        }
                    }
                    //
                    // CRC is addition of all scrambled characters
                    //
                    crc = crc + CValue;
                    //
                    // put together backwards
                    //
                    returnCopy = Microsoft.VisualBasic.Strings.Chr(CValue) + returnCopy;
                }
                //
                // Ends with the mod of the CRC and 13
                //
                returnCopy = "_a" + returnCopy + (crc % 9).ToString();
            } catch (Exception ex) {
                cpcore.handleException(ex);
                throw;
            }
            return returnCopy;
        }
        //
        //====================================================================================================
        //
        public static string removeScriptTag(string source) {
            string result = source;
            int StartPos = genericController.vbInstr(1, result, "<script", 1);
            if (StartPos != 0) {
                int EndPos = genericController.vbInstr(StartPos, result, "</script", 1);
                if (EndPos != 0) {
                    EndPos = genericController.vbInstr(EndPos, result, ">", 1);
                    if (EndPos != 0) {
                        result = result.Left( StartPos - 1) + result.Substring(EndPos);
                    }
                }
            }
            return result;
        }
        //
        //
        public static string[] stringSplit( string src, string delimiter ) {
            return src.Split( new[] { delimiter }, StringSplitOptions.None );
        }
    }
}
