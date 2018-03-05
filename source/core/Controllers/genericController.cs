
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
using Contensive.Core.Models.DbModels;
using Contensive.Core.Controllers;
using static Contensive.Core.Controllers.genericController;
using static Contensive.Core.constants;
using System.Net;
using System.Text;
using Contensive.Core.Models.Context;
using System.Web;

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
        public static string getGUID() => getGUID(true);
        //
        //====================================================================================================
        /// <summary>
        /// rturn a normalized guid
        /// </summary>
        /// <param name="registryFormat"></param>
        /// <returns></returns>
        public static string getGUID(bool registryFormat) {
            string result = "";
            Guid g = Guid.NewGuid();
            if (g != Guid.Empty) {
                result = g.ToString();
                //
                if (!string.IsNullOrEmpty(result)) {
                    result = registryFormat ? "{" + result + "}" : result;
                }
            }
            return result;
        }
        //
        //====================================================================================================
        /// <summary>
        /// If string is empty, return default value
        /// </summary>
        /// <param name="sourceText"></param>
        /// <param name="defaultText"></param>
        /// <returns></returns>
        public static string encodeEmpty(string sourceText, string defaultText) {
            string returnText = sourceText;
            if (string.IsNullOrEmpty(returnText)) {
                returnText = defaultText;
            }
            return returnText;
        }
        //
        //====================================================================================================
        /// <summary>
        /// convert a string to an integer. If the string is not empty, but a non-valid integer, return 0. If the string is empty, return a default value
        /// </summary>
        /// <param name="sourceText"></param>
        /// <param name="DefaultInteger"></param>
        /// <returns></returns>
        public static int encodeEmptyInteger(string sourceText, int DefaultInteger) => encodeInteger(encodeEmpty(sourceText, DefaultInteger.ToString()));
        //
        //====================================================================================================
        /// <summary>
        /// convert a string to a date. If the string is empty, return the defaultDate. If the string is not a valid date, return date.minvalue
        /// </summary>
        /// <param name="sourceText"></param>
        /// <param name="DefaultDate"></param>
        /// <returns></returns>
        public static DateTime encodeEmptyDate(string sourceText, DateTime DefaultDate) => encodeDate(encodeEmpty(sourceText, DefaultDate.ToString()));
        //
        //====================================================================================================
        /// <summary>
        /// convert a string to a double. If the string is empty, return the default number. If the string is not a valid number, return 0.0
        /// </summary>
        /// <param name="sourceText"></param>
        /// <param name="DefaultNumber"></param>
        /// <returns></returns>
        public static double encodeEmptyNumber(string sourceText, double DefaultNumber) => encodeNumber(encodeEmpty(sourceText, DefaultNumber.ToString()));
        //
        //====================================================================================================
        /// <summary>
        /// convert a string to a boolean. If the string is empty, return the default state. if the string is empty, return false
        /// </summary>
        /// <param name="sourceText"></param>
        /// <param name="DefaultState"></param>
        /// <returns></returns>
        public static bool encodeEmptyBoolean(string sourceText, bool DefaultState) => encodeBoolean(encodeEmpty(sourceText, DefaultState.ToString()));
        //
        //=============================================================================
        /// <summary>
        /// Modify the querystring at the end of a link. If there is no, question mark, the link argument is assumed to be a link, not the querysting
        /// </summary>
        /// <param name="link"></param>
        /// <param name="queryName"></param>
        /// <param name="queryValue"></param>
        /// <param name="addIfMissing"></param>
        /// <returns></returns>
        public static string modifyLinkQuery(string link, string queryName, string queryValue, bool addIfMissing = true) {
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
                iAddIfMissing = addIfMissing;
                if (vbInstr(1, link, "?") != 0) {
                    tempmodifyLinkQuery = link.Left(vbInstr(1, link, "?") - 1);
                    QueryString = link.Substring(tempmodifyLinkQuery.Length + 1);
                } else {
                    tempmodifyLinkQuery = link;
                    QueryString = "";
                }
                UcaseQueryName = vbUCase(encodeRequestVariable(queryName));
                if (!string.IsNullOrEmpty(QueryString)) {
                    Element = QueryString.Split('&');
                    ElementCount = Element.GetUpperBound(0) + 1;
                    for (ElementPointer = 0; ElementPointer < ElementCount; ElementPointer++) {
                        NameValue = Element[ElementPointer].Split('=');
                        if (NameValue.GetUpperBound(0) == 1) {
                            if (vbUCase(NameValue[0]) == UcaseQueryName) {
                                if (string.IsNullOrEmpty(queryValue)) {
                                    Element[ElementPointer] = "";
                                } else {
                                    Element[ElementPointer] = queryName + "=" + queryValue;
                                }
                                ElementFound = true;
                                break;
                            }
                        }
                    }
                }
                if (!ElementFound && (!string.IsNullOrEmpty(queryValue))) {
                    //
                    // element not found, it needs to be added
                    //
                    if (iAddIfMissing) {
                        if (string.IsNullOrEmpty(QueryString)) {
                            QueryString = encodeRequestVariable(queryName) + "=" + encodeRequestVariable(queryValue);
                        } else {
                            QueryString = QueryString + "&" + encodeRequestVariable(queryName) + "=" + encodeRequestVariable(queryValue);
                        }
                    }
                } else {
                    //
                    // element found
                    //
                    QueryString = string.Join("&", Element);
                    if ((!string.IsNullOrEmpty(QueryString)) && (string.IsNullOrEmpty(queryValue))) {
                        //
                        // element found and needs to be removed
                        //
                        QueryString = vbReplace(QueryString, "&&", "&");
                        if (QueryString.Left(1) == "&") {
                            QueryString = QueryString.Substring(1);
                        }
                        if (QueryString.Substring(QueryString.Length - 1) == "&") {
                            QueryString = QueryString.Left(QueryString.Length - 1);
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
        //
        //=============================================================================
        /// <summary>
        /// Create the part of the sql where clause that is modified by the user, WorkingQuery is the original querystring to change, QueryName is the name part of the name pair to change, If the QueryName is not found in the string
        /// </summary>
        /// <param name="workingQuery"></param>
        /// <param name="queryName"></param>
        /// <param name="queryValue"></param>
        /// <param name="addIfMissing"></param>
        /// <returns></returns>
        //
        //=============================================================================
        //
        public static string modifyQueryString(string workingQuery, string queryName, string queryValue, bool addIfMissing = true) {
            string result = "";
            //
            if (workingQuery.IndexOf("?") >= 0) {
                result = modifyLinkQuery(workingQuery, queryName, queryValue, addIfMissing);
            } else {
                result = modifyLinkQuery("?" + workingQuery, queryName, queryValue, addIfMissing);
                if (result.Length > 0) {
                    result = result.Substring(1);
                }
            }
            return result;
        }
        //
        //=============================================================================
        //
        public static string modifyQueryString(string WorkingQuery, string QueryName, int QueryValue, bool AddIfMissing = true) => modifyQueryString(WorkingQuery, QueryName, QueryValue.ToString(), AddIfMissing);
        //
        //=============================================================================
        //
        public static string modifyQueryString(string WorkingQuery, string QueryName, bool QueryValue, bool AddIfMissing = true) => modifyQueryString(WorkingQuery, QueryName, QueryValue.ToString(), AddIfMissing);
        //
        //========================================================================
        /// <summary>
        /// decodeHtml, Convert HTML equivalent characters to their equivalents
        /// </summary>
        /// <param name="Source"></param>
        /// <returns></returns>
        public static string decodeHtml(string Source) {
            return WebUtility.HtmlDecode(Source);
            // string result = Source.Trim();
            // if (!string.IsNullOrWhiteSpace(result)) {
            //     //
            //     int Pos = result.Length;
            //     Pos = result.LastIndexOf("&#", Pos - 1) + 1;
            //     while (Pos != 0) {
            //         string CharCodeString = "";
            //         int posEnd = 0;
            //         if (result.Substring(Pos + 2, 1) == ";") {
            //             CharCodeString = result.Substring(Pos + 1, 1);
            //             posEnd = Pos + 4;
            //         } else if (result.Substring(Pos + 3, 1) == ";") {
            //             CharCodeString = result.Substring(Pos + 1, 2);
            //             posEnd = Pos + 5;
            //         } else if (result.Substring(Pos + 4, 1) == ";") {
            //             CharCodeString = result.Substring(Pos + 1, 3);
            //             posEnd = Pos + 6;
            //         }
            //         if (!string.IsNullOrEmpty(CharCodeString)) {
            //             if (CharCodeString.IsNumeric()) {
            //                 int CharCode = encodeInteger(CharCodeString);
            //                 result = result.Left( Pos - 1) + Convert.ToChar(CharCode) + result.Substring(posEnd - 1);
            //             }
            //         }
            //         //
            //         Pos = result.LastIndexOf("&#", Pos - 1) + 1;
            //     }
            //     //
            //     // replace out all common names (at least the most common for now)
            //     result = vbReplace(result, "&lt;", "<");
            //     result = vbReplace(result, "&gt;", ">");
            //     result = vbReplace(result, "&quot;", "\"");
            //     result = vbReplace(result, "&apos;", "'");
            //     //
            //     // Always replace the amp last
            //     result = vbReplace(result, "&amp;", "&");
            // }
            //return result;
        }
        //
        //========================================================================
        /// <summary>
        /// legacy indent - now returns source
        /// </summary>
        /// <param name="Source"></param>
        /// <param name="depth"></param>
        /// <returns></returns>
        public static string nop(string Source, int depth = 1) {
            return Source;
            //string temphtmlIndent = null;
            //int posStart = 0;
            //int posEnd = 0;
            //string pre = null;
            //string post = null;
            //string target = null;
            ////
            //posStart = vbInstr(1, Source, "<![CDATA[", 1);
            //if (posStart == 0) {
            //    //
            //    // no cdata
            //    //
            //    posStart = vbInstr(1, Source, "<textarea", 1);
            //    if (posStart == 0) {
            //        //
            //        // no textarea
            //        //
            //        string replaceText = "\r\n" + new string(Convert.ToChar("\t"), (depth + 1));
            //        temphtmlIndent = vbReplace(Source, "\r\n\t", replaceText);
            //    } else {
            //        //
            //        // text area found, isolate it and indent before and after
            //        //
            //        posEnd = vbInstr(posStart, Source, "</textarea>", 1);
            //        pre = Source.Left( posStart - 1);
            //        if (posEnd == 0) {
            //            target = Source.Substring(posStart - 1);
            //            post = "";
            //        } else {
            //            target = Source.Substring(posStart - 1, posEnd - posStart + ((string)("</textarea>")).Length);
            //            post = Source.Substring((posEnd + ((string)("</textarea>")).Length) - 1);
            //        }
            //        temphtmlIndent = nop(pre) + target + nop(post);
            //    }
            //} else {
            //    //
            //    // cdata found, isolate it and indent before and after
            //    //
            //    posEnd = vbInstr(posStart, Source, "]]>", 1);
            //    pre = Source.Left( posStart - 1);
            //    if (posEnd == 0) {
            //        target = Source.Substring(posStart - 1);
            //        post = "";
            //    } else {
            //        target = Source.Substring(posStart - 1, posEnd - posStart + ((string)("]]>")).Length);
            //        post = Source.Substring(posEnd + 2);
            //    }
            //    temphtmlIndent = nop(pre) + target + nop(post);
            //}
            ////    kmaIndent = Source
            ////    If vbInstr(1, kmaIndent, "<textarea", vbTextCompare) = 0 Then
            ////        kmaIndent = vbReplace(Source, vbCrLf & vbTab, vbCrLf & vbTab & vbTab)
            ////    End If
            //return temphtmlIndent;
        }
        //
        //========================================================================================================
        /// <summary>
        /// convert byte array to string
        /// </summary>
        /// <param name="Bytes"></param>
        /// <returns></returns>
        public static string byteArrayToString(byte[] Bytes) {
            return System.Text.UTF8Encoding.ASCII.GetString(Bytes);
        }
        //
        //========================================================================================================
        /// <summary>
        /// RFC1123 is "ddd, dd MMM yyyy HH':'mm':'ss 'GMT'"
        /// convert a date to a string with RFC1123 format as is used in http last-modified header, "day, DD-Mon-YYYY HH:MM:SS GMT" 
        /// Note: it does NOT perform a time change, just converts to the formatted string
        /// </summary>
        /// <param name="DateValue"></param>
        /// <returns></returns>
        //
        public static string GetRFC1123PatternDateFormat(DateTime DateValue) {
            //
            return DateValue.ToString("R");
            //string tempGetGMTFromDate = null;
            //int WorkLong = 0;
            ////
            //tempGetGMTFromDate = "";
            //if (dateController.IsDate(DateValue)) {
            //    switch ((int)DateValue.DayOfWeek) {
            //        case 0:
            //            tempGetGMTFromDate = "Sun, ";
            //            break;
            //        case 1:
            //            tempGetGMTFromDate = "Mon, ";
            //            break;
            //        case 2:
            //            tempGetGMTFromDate = "Tue, ";
            //            break;
            //        case 3:
            //            tempGetGMTFromDate = "Wed, ";
            //            break;
            //        case 4:
            //            tempGetGMTFromDate = "Thu, ";
            //            break;
            //        case 5:
            //            tempGetGMTFromDate = "Fri, ";
            //            break;
            //        case 6:
            //            tempGetGMTFromDate = "Sat, ";
            //            break;
            //    }
            //    //
            //    WorkLong = DateValue.Day;
            //    if (WorkLong < 10) {
            //        tempGetGMTFromDate = tempGetGMTFromDate + "0" + WorkLong.ToString() + " ";
            //    } else {
            //        tempGetGMTFromDate = tempGetGMTFromDate + WorkLong.ToString() + " ";
            //    }
            //    //
            //    switch (DateValue.Month) {
            //        case 1:
            //            tempGetGMTFromDate = tempGetGMTFromDate + "Jan ";
            //            break;
            //        case 2:
            //            tempGetGMTFromDate = tempGetGMTFromDate + "Feb ";
            //            break;
            //        case 3:
            //            tempGetGMTFromDate = tempGetGMTFromDate + "Mar ";
            //            break;
            //        case 4:
            //            tempGetGMTFromDate = tempGetGMTFromDate + "Apr ";
            //            break;
            //        case 5:
            //            tempGetGMTFromDate = tempGetGMTFromDate + "May ";
            //            break;
            //        case 6:
            //            tempGetGMTFromDate = tempGetGMTFromDate + "Jun ";
            //            break;
            //        case 7:
            //            tempGetGMTFromDate = tempGetGMTFromDate + "Jul ";
            //            break;
            //        case 8:
            //            tempGetGMTFromDate = tempGetGMTFromDate + "Aug ";
            //            break;
            //        case 9:
            //            tempGetGMTFromDate = tempGetGMTFromDate + "Sep ";
            //            break;
            //        case 10:
            //            tempGetGMTFromDate = tempGetGMTFromDate + "Oct ";
            //            break;
            //        case 11:
            //            tempGetGMTFromDate = tempGetGMTFromDate + "Nov ";
            //            break;
            //        case 12:
            //            tempGetGMTFromDate = tempGetGMTFromDate + "Dec ";
            //            break;
            //    }
            //    //
            //    tempGetGMTFromDate = tempGetGMTFromDate + encodeText(DateValue.Year) + " ";
            //    //
            //    WorkLong = DateValue.Hour;
            //    if (WorkLong < 10) {
            //        tempGetGMTFromDate = tempGetGMTFromDate + "0" + WorkLong.ToString() + ":";
            //    } else {
            //        tempGetGMTFromDate = tempGetGMTFromDate + WorkLong.ToString() + ":";
            //    }
            //    //
            //    WorkLong = DateValue.Minute;
            //    if (WorkLong < 10) {
            //        tempGetGMTFromDate = tempGetGMTFromDate + "0" + WorkLong.ToString() + ":";
            //    } else {
            //        tempGetGMTFromDate = tempGetGMTFromDate + WorkLong.ToString() + ":";
            //    }
            //    //
            //    WorkLong = DateValue.Second;
            //    if (WorkLong < 10) {
            //        tempGetGMTFromDate = tempGetGMTFromDate + "0" + WorkLong.ToString();
            //    } else {
            //        tempGetGMTFromDate = tempGetGMTFromDate + WorkLong.ToString();
            //    }
            //    //
            //    tempGetGMTFromDate = tempGetGMTFromDate + " GMT";
            //}
            //
            //return tempGetGMTFromDate;
        }
        //
        //========================================================================================================
        /// <summary>
        /// convert the enum status into a displayable caption
        /// </summary>
        /// <param name="ApplicationStatus"></param>
        /// <returns></returns>
        public static string GetApplicationStatusMessage(appConfigModel.appStatusEnum ApplicationStatus) {
            string tempGetApplicationStatusMessage = null;

            switch (ApplicationStatus) {
                case appConfigModel.appStatusEnum.ok:
                    tempGetApplicationStatusMessage = "Application OK";
                    break;
                case appConfigModel.appStatusEnum.maintenance:
                    tempGetApplicationStatusMessage = "Application building";
                    break;
                default:
                    tempGetApplicationStatusMessage = "Unknown status code [" + ApplicationStatus + "], see trace log for details";
                    break;
            }
            return tempGetApplicationStatusMessage;
        }
        //
        //========================================================================================================
        /// <summary>
        /// Legacy spacer
        /// </summary>
        /// <param name="Width"></param>
        /// <param name="Height"></param>
        /// <returns></returns>
        public static string nop2(int Width, int Height) {
            return "<!-- removed spacer -->"; // "<img alt=\"space\" src=\"/ccLib/images/spacer.gif\" width=\"" + Width + "\" height=\"" + Height + "\" border=\"0\">";
        }
        //
        //========================================================================================================
        /// <summary>
        /// Convert the href and src links in html content to full urls that include the protocol and domain 
        /// </summary>
        /// <param name="htmlContent"></param>
        /// <param name="urlProtocolDomainSlash"></param>
        /// <returns></returns>
        public static string convertLinksToAbsolute(string htmlContent, string urlProtocolDomainSlash) {
            string result = htmlContent;
            try {
                result = result.Replace(" href=\"", " href=\"/");
                result = result.Replace(" href=\"/http", " href=\"http");
                result = result.Replace(" href=\"/mailto", " href=\"mailto");
                result = result.Replace(" href=\"//", " href=\"" + urlProtocolDomainSlash);
                result = result.Replace(" href=\"/?", " href=\"" + urlProtocolDomainSlash + "?");
                result = result.Replace(" href=\"/", " href=\"" + urlProtocolDomainSlash);
                //
                result = result.Replace(" href=", " href=/");
                result = result.Replace(" href=/\"", " href=\"");
                result = result.Replace(" href=/http", " href=http");
                result = result.Replace(" href=//", " href=" + urlProtocolDomainSlash);
                result = result.Replace(" href=/?", " href=" + urlProtocolDomainSlash + "?");
                result = result.Replace(" href=/", " href=" + urlProtocolDomainSlash);
                //
                result = result.Replace(" src=\"", " src=\"/");
                result = result.Replace(" src=\"/http", " src=\"http");
                result = result.Replace(" src=\"//", " src=\"" + urlProtocolDomainSlash);
                result = result.Replace(" src=\"/?", " src=\"" + urlProtocolDomainSlash + "?");
                result = result.Replace(" src=\"/", " src=\"" + urlProtocolDomainSlash);
                //
                result = result.Replace(" src=", " src=/");
                result = result.Replace(" src=/\"", " src=\"");
                result = result.Replace(" src=/http", " src=http");
                result = result.Replace(" src=//", " src=" + urlProtocolDomainSlash);
                result = result.Replace(" src=/?", " src=" + urlProtocolDomainSlash + "?");
                result = result.Replace(" src=/", " src=" + urlProtocolDomainSlash);
            } catch (Exception) {
                throw new ApplicationException("Error in ConvertLinksToAbsolute");
            }
            return result;
        }
        //
        //========================================================================================================
        /// <summary>
        /// return an array of strings split on new line (crlf)
        /// </summary>
        /// <param name="textToSplit"></param>
        /// <returns></returns>
        public static string[] splitNewLine(string textToSplit) {
            return textToSplit.Split(new[] { windowsNewLine, macNewLine, unixNewLine }, StringSplitOptions.None);
        }
        //
        //========================================================================================================
        /// <summary>
        /// legacy encode - not referenced but the decode is still used, so this will be needed
        /// </summary>
        /// <param name="Arg"></param>
        /// <returns></returns>
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
        //========================================================================================================
        /// <summary>
        /// Decodes an argument parsed from an AddonConstructorString for all non-allowed characters.
        ///       AddonConstructorString is a & delimited string of name=value[selector]descriptor
        ///       to get a value from an AddonConstructorString, first use getargument() to get the correct value[selector]descriptor
        ///       then remove everything to the right of any '['
        ///       call encodeAddonConstructorargument before parsing them together
        ///       call decodeAddonConstructorArgument after parsing them apart
        ///       Arg0,Arg1,Arg2,Arg3,Name=Value&Name=VAlue[Option1|Option2]
        ///       This routine is needed for all Arg, Name, Value, Option values
        /// </summary>
        /// <param name="EncodedArg"></param>
        /// <returns></returns>
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
        //
        // ====================================================================================================
        /// <summary>
        /// return argument for separateUrl
        /// </summary>
        public class urlDetailsClass {
            public string protocol = "";
            public string host = "";
            public string port = "";
            public List<String> pathSegments = new List<String>();
            public string filename = "";
            public string queryString = "";
            public string unixPath() { return String.Join("/", pathSegments); }
            public string dosPath() { return String.Join("\\", pathSegments); }
        }
        //
        // ====================================================================================================
        /// <summary>
        /// split a source Url into its components. Url and Uri are always UNIX slashed.
        /// </summary>
        /// <param name="sourceUrl"></param>
        public static urlDetailsClass splitUrl(string sourceUrl) {
            var urlDetails = new urlDetailsClass();
            string path = "";
            splitUrl(sourceUrl, ref urlDetails.protocol, ref urlDetails.host, ref urlDetails.port, ref path, ref urlDetails.filename, ref urlDetails.queryString);
            foreach (string segment in path.Split('/')) {
                if (!string.IsNullOrEmpty(segment)) { urlDetails.pathSegments.Add(segment); }
            }
            return urlDetails;
        }
        //
        // ====================================================================================================
        /// <summary>
        /// separate a source Url into its components. Querystring includes questionmark
        /// </summary>
        public static void splitUrl(string sourceUrl, ref string protocol, ref string host, ref string path, ref string page, ref string queryString) {
            string port = "";
            splitUrl(sourceUrl, ref protocol, ref host, ref port, ref path, ref page, ref queryString);
            ////
            //// -- Divide the URL into URLHost, URLPath, and URLPage
            //string WorkingURL = convertToUnixSlash( sourceUrl);
            ////
            //// -- Get Protocol (before the first :)
            //int Position = vbInstr(1, WorkingURL, ":");
            //if (Position != 0) {
            //    protocol = WorkingURL.Left( Position + 2);
            //    WorkingURL = WorkingURL.Substring(Position + 2);
            //}
            ////
            //// -- compatibility fix
            //if (vbInstr(1, WorkingURL, "//") == 1) {
            //    if (string.IsNullOrEmpty(protocol)) {
            //        protocol = "http:";
            //    }
            //    protocol = protocol + "//";
            //    WorkingURL = WorkingURL.Substring(2);
            //}
            ////
            //// -- Get QueryString
            //Position = vbInstr(1, WorkingURL, "?");
            //if (Position > 0) {
            //    queryString = WorkingURL.Substring(Position - 1);
            //    WorkingURL = WorkingURL.Left( Position - 1);
            //}
            ////
            //// -- separate host from pathpage
            //Position = vbInstr(WorkingURL, "/");
            //if ((Position == 0) && (string.IsNullOrEmpty(protocol))) {
            //    //
            //    // -- Page without path or host
            //    page = WorkingURL;
            //    path = "";
            //    host = "";
            //} else if (Position == 0) {
            //    //
            //    // -- host, without path or page
            //    page = "";
            //    path = "/";
            //    host = WorkingURL;
            //} else {
            //    //
            //    // -- host with a path (at least)
            //    path = WorkingURL.Substring(Position - 1);
            //    host = WorkingURL.Left( Position - 1);
            //    //
            //    // -- separate page from path
            //    Position = path.LastIndexOf("/") + 1;
            //    if (Position == 0) {
            //        //
            //        // -- no path, just a page
            //        page = path;
            //        path = "/";
            //    } else {
            //        page = path.Substring(Position);
            //        path = path.Left( Position);
            //    }
            //}
        }
        //
        //================================================================================================================
        /// <summary>
        /// Separate a URL into its host, path, page parts. Protocol includes ://, path includes leading and trailing slash, querystring includes questionmark
        /// </summary>
        /// <param name="sourceURL"></param>
        /// <param name="protocol"></param>
        /// <param name="host"></param>
        /// <param name="port"></param>
        /// <param name="path"></param>
        /// <param name="page"></param>
        /// <param name="queryString"></param>
        public static void splitUrl(string sourceURL, ref string protocol, ref string host, ref string port, ref string path, ref string page, ref string queryString) {
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
            iURLWorking = sourceURL;
            Position = vbInstr(1, iURLWorking, "://");
            if (Position != 0) {
                iURLProtocol = iURLWorking.Left(Position + 2);
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
                iURLHost = iURLHost.Left(Position - 1);
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
                    iURLPath = iURLPath.Left(Position);
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
                iURLHost = iURLHost.Left(Position - 1);
            }
            Position = vbInstr(1, iURLPage, "?");
            if (Position > 0) {
                iURLQueryString = iURLPage.Substring(Position - 1);
                iURLPage = iURLPage.Left(Position - 1);
            }
            protocol = iURLProtocol;
            host = iURLHost;
            port = iURLPort;
            path = iURLPath;
            page = iURLPage;
            queryString = iURLQueryString;
        }
        //
        //================================================================================================================
        /// <summary>
        /// deprecated, use native date conversions - convert a GMT date string to a datetime object. I honestly cannot find any kind of backup for this format
        /// </summary>
        /// <param name="GMTDate"></param>
        /// <returns></returns>
        public static DateTime deprecatedDecodeGMTDate(string GMTDate) {
            DateTime result = default(DateTime);
            try {
                if (!string.IsNullOrEmpty(GMTDate)) {
                    double HourPart = encodeNumber(GMTDate.Substring(5, 11));
                    if (dateController.IsDate(HourPart)) {
                        double YearPart = encodeNumber(GMTDate.Substring(17, 8));
                        if (dateController.IsDate(YearPart)) {
                            result = DateTime.FromOADate(YearPart + (HourPart + 4) / 24);
                        }
                    }
                }
            } catch (Exception) {
                throw;
            }
            return result;
        }
        // 
        //=================================================================================
        /// <summary>
        /// Get the value of a name in a string of name value pairs parsed with vrlf and =
        ///   ex delimiter '&' -> name1=value1&name2=value2"
        ///   There can be no extra spaces between the delimiter, the name and the "="
        /// </summary>
        /// <param name="name"></param>
        /// <param name="nameValueString"></param>
        /// <param name="defaultValue"></param>
        /// <param name="delimiter"></param>
        /// <returns></returns>
        public static string getValueFromNameValueString(string name, string nameValueString, string defaultValue, string delimiter) {
            string result = defaultValue;
            try {
                //
                // determine delimiter
                if (string.IsNullOrEmpty(delimiter)) {
                    //
                    // If not explicit
                    if (vbInstr(1, nameValueString, "\r\n") != 0) {
                        //
                        // crlf can only be here if it is the delimiter
                        delimiter = "\r\n";
                    } else {
                        //
                        // either only one option, or it is the legacy '&' delimit
                        delimiter = "&";
                    }
                }
                string WorkingString = nameValueString;
                if (!string.IsNullOrEmpty(WorkingString)) {
                    WorkingString = delimiter + WorkingString + delimiter;
                    int ValueStart = vbInstr(1, WorkingString, delimiter + name + "=", 1);
                    if (ValueStart != 0) {
                        int NameLength = name.Length;
                        bool IsQuoted = false;
                        ValueStart = ValueStart + delimiter.Length + NameLength + 1;
                        if (WorkingString.Substring(ValueStart - 1, 1) == "\"") {
                            IsQuoted = true;
                            ValueStart = ValueStart + 1;
                        }
                        int ValueEnd = 0;
                        if (IsQuoted) {
                            ValueEnd = vbInstr(ValueStart, WorkingString, "\"" + delimiter);
                        } else {
                            ValueEnd = vbInstr(ValueStart, WorkingString, delimiter);
                        }
                        if (ValueEnd == 0) {
                            result = WorkingString.Substring(ValueStart - 1);
                        } else {
                            result = WorkingString.Substring(ValueStart - 1, ValueEnd - ValueStart);
                        }
                    }
                }
            } catch (Exception) {
                throw;
            }
            return result;
        }
        //
        //=================================================================================
        /// <summary>
        /// Get a Random Long Value
        /// </summary>
        /// <param name="core"></param>
        /// <returns></returns>
        public static int GetRandomInteger(coreController core) {
            return core.random.Next(Int32.MaxValue);
        }
        //
        //=================================================================================
        /// <summary>
        /// return a string from an integer, padded left with 0. If the number is longer then the width, the full number is returned.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="digitCount"></param>
        /// <returns></returns>
        public static string getIntegerString(int value, int digitCount) {
            if (sizeof(int) <= digitCount) {
                return value.ToString().PadLeft(digitCount, '0');
            } else {
                return value.ToString();
            }
        }
        //
        //==========================================================================================
        /// <summary>
        /// Test if a test string is in a delimited string
        /// </summary>
        /// <param name="stringToSearch"></param>
        /// <param name="find"></param>
        /// <param name="Delimiter"></param>
        /// <returns></returns>
        public static bool isInDelimitedString(string stringToSearch, string find, string Delimiter) {
            return ((Delimiter + stringToSearch + Delimiter).ToLower().IndexOf((Delimiter + find + Delimiter).ToLower()) >= 0);
        }
        //========================================================================
        /// <summary>
        /// Encode a string to be used as a path or filename in a url. only what is to the left of the question mark.
        /// </summary>
        /// <param name="Source"></param>
        /// <returns></returns>
        public static string encodeURL(string Source) {
            return WebUtility.UrlEncode(Source);
        }
        //
        //========================================================================
        /// <summary>
        /// It is prefered to encode the request variables then assemble then into a query string. This routine parses them out and encodes them.
        /// </summary>
        /// <param name="Source"></param>
        /// <returns></returns>
        public static string encodeQueryString(string Source) {
            string result = "";
            try {
                if (!string.IsNullOrWhiteSpace(Source)) {
                    string[] QSSplit = Source.Split('&');
                    for (int QSPointer = 0; QSPointer <= QSSplit.GetUpperBound(0); QSPointer++) {
                        string NV = QSSplit[QSPointer];
                        if (!string.IsNullOrEmpty(NV)) {
                            string[] NVSplit = NV.Split('=');
                            if (NVSplit.GetUpperBound(0) == 0) {
                                NVSplit[0] = encodeRequestVariable(NVSplit[0]);
                                result = result + "&" + NVSplit[0];
                            } else {
                                NVSplit[0] = encodeRequestVariable(NVSplit[0]);
                                NVSplit[1] = encodeRequestVariable(NVSplit[1]);
                                result = result + "&" + NVSplit[0] + "=" + NVSplit[1];
                            }
                        }
                    }
                    if (!string.IsNullOrEmpty(result)) {
                        result = result.Substring(1);
                    }
                }
            } catch (Exception) {
                throw;
            }
            return result;
        }
        //
        //========================================================================
        /// <summary>
        /// encode the name or value part of a querystring, to be parsed with & and = 
        /// </summary>
        /// <param name="Source"></param>
        /// <returns></returns>
        public static string encodeRequestVariable(string Source) {
            if (Source == null) {
                return "";
            }
            return System.Uri.EscapeDataString(Source);
        }
        //
        //========================================================================
        /// <summary>
        /// return the string that would result from encoding the string
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string decodeResponseVariable(string source) {
            string result = source;
            //
            int Position = vbInstr(1, result, "%");
            while (Position != 0) {
                string ESCString = result.Substring(Position - 1, 3);
                string Digit0 = vbUCase(ESCString.Substring(1, 1));
                string Digit1 = vbUCase(ESCString.Substring(2, 1));
                if (((string.CompareOrdinal(Digit0, "0") >= 0) && (string.CompareOrdinal(Digit0, "9") <= 0)) || ((string.CompareOrdinal(Digit0, "A") >= 0) && (string.CompareOrdinal(Digit0, "F") <= 0))) {
                    if (((string.CompareOrdinal(Digit1, "0") >= 0) && (string.CompareOrdinal(Digit1, "9") <= 0)) || ((string.CompareOrdinal(Digit1, "A") >= 0) && (string.CompareOrdinal(Digit1, "F") <= 0))) {
                        int ESCValue = 0;
                        try {
                            ESCValue = Convert.ToInt32(ESCString.Substring(1), 16);
                        } catch {
                            // do nothing -- just put a 0 in as the escape code was not valid, a data problem not a code problem
                        }
                        result = result.Left(Position - 1) + Convert.ToChar(ESCValue) + result.Substring(Position + 2);
                    }
                }
                Position = vbInstr(Position + 1, result, "%");
            }
            //
            return result;
        }
        //   
        //========================================================================
        /// <summary>
        /// Converts a querystring from an Encoded URL (with %20 and +), to non incoded (with spaced)
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string decodeURL(string source) {
            return WebUtility.UrlDecode(source);
            //string tempDecodeURL = null;
            //// ##### removed to catch err<>0 problem //On Error //Resume Next
            ////
            //int Position = 0;
            //string ESCString = null;
            //int ESCValue = 0;
            //string Digit0 = null;
            //string Digit1 = null;
            ////Dim iURL As String
            ////
            ////iURL = Source
            //// plus to space only applies for query component of a URL, but %99 encoding works for both
            ////DecodeURL = vbReplace(iURL, "+", " ")
            //tempDecodeURL = source;
            //Position = vbInstr(1, tempDecodeURL, "%");
            //while (Position != 0) {
            //    ESCString = tempDecodeURL.Substring(Position - 1, 3);
            //    Digit0 = vbUCase(ESCString.Substring(1, 1));
            //    Digit1 = vbUCase(ESCString.Substring(2, 1));
            //    if (((string.CompareOrdinal(Digit0, "0") >= 0) && (string.CompareOrdinal(Digit0, "9") <= 0)) || ((string.CompareOrdinal(Digit0, "A") >= 0) && (string.CompareOrdinal(Digit0, "F") <= 0))) {
            //        if (((string.CompareOrdinal(Digit1, "0") >= 0) && (string.CompareOrdinal(Digit1, "9") <= 0)) || ((string.CompareOrdinal(Digit1, "A") >= 0) && (string.CompareOrdinal(Digit1, "F") <= 0))) {
            //            ESCValue = int.Parse("&H" + ESCString.Substring(1));
            //            tempDecodeURL = vbReplace(tempDecodeURL, ESCString, Convert.ToChar(ESCValue));
            //        }
            //    }
            //    Position = vbInstr(Position + 1, tempDecodeURL, "%");
            //}
            ////
            //return tempDecodeURL;
        }
        //
        //========================================================================
        /// <summary>
        /// return the first of two dates, excluding minValue
        /// </summary>
        /// <param name="Date0"></param>
        /// <param name="Date1"></param>
        /// <returns></returns>
        public static DateTime getFirstNonZeroDate(DateTime Date0, DateTime Date1) {
            if (Date0 == DateTime.MinValue) {
                if (Date1 == DateTime.MinValue) {
                    //
                    // Both 0, return 0
                    //
                    return DateTime.MinValue;
                } else {
                    //
                    // Date0 is NullDate, return Date1
                    //
                    return Date1;
                }
            } else {
                if (Date1 == DateTime.MinValue) {
                    //
                    // Date1 is nulldate, return Date0
                    //
                    return Date0;
                } else if (Date0 < Date1) {
                    //
                    // Date0 is first
                    //
                    return Date0;
                } else {
                    //
                    // Date1 is first
                    //
                    return Date1;
                }
            }
        }
        //
        //========================================================================
        //
        public static int getFirstNonZeroInteger(int Integer1, int Integer2) {
            if (Integer1 == 0) {
                if (Integer2 == 0) {
                    //
                    // Both 0, return 0
                    return 0;
                } else {
                    //
                    // Integer1 is 0, return Integer2
                    return Integer2;
                }
            } else {
                if (Integer2 == 0) {
                    //
                    // Integer2 is 0, return Integer1
                    return Integer1;
                } else if (Integer1 < Integer2) {
                    //
                    // Integer1 is first
                    return Integer1;
                } else {
                    //
                    // Integer2 is first
                    return Integer2;
                }
            }
        }
        //
        //========================================================================
        /// <summary>
        /// splitDelimited, returns the result of a Split, except it honors quoted text, if a quote is found, it is assumed to also be a delimiter ( 'this"that"theother' = 'this "that" theother' )
        /// </summary>
        /// <param name="WordList"></param>
        /// <param name="Delimiter"></param>
        /// <returns></returns>
        public static string[] SplitDelimited(string WordList, string Delimiter) {
            string[] Out = new string[1];
            int OutPointer = 0;
            if (!string.IsNullOrEmpty(WordList)) {
                string[] QuoteSplit = stringSplit(WordList, "\"");
                int QuoteSplitCount = QuoteSplit.GetUpperBound(0) + 1;
                bool InQuote = (string.IsNullOrEmpty(WordList.Left(1)));
                int OutSize = 1;
                for (int QuoteSplitPointer = 0; QuoteSplitPointer < QuoteSplitCount; QuoteSplitPointer++) {
                    string Fragment = QuoteSplit[QuoteSplitPointer];
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
                            string[] SpaceSplit = Fragment.Split(Delimiter.ToCharArray());
                            int SpaceSplitCount = SpaceSplit.GetUpperBound(0) + 1;
                            for (int SpaceSplitPointer = 0; SpaceSplitPointer < SpaceSplitCount; SpaceSplitPointer++) {
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
            return Out;
        }
        //
        //========================================================================
        /// <summary>
        /// return Yes if true, No if false
        /// </summary>
        /// <param name="Key"></param>
        /// <returns></returns>
        public static string getYesNo(bool Key) {
            if (Key) {
                return "Yes";
            }
            return "No";
        }
        //
        // ====================================================================================================
        /// <summary>
        /// remove the host and approotpath, leaving the "active" path and all else
        /// </summary>
        /// <param name="url"></param>
        /// <param name="urlPrefix"></param>
        /// <returns></returns>
        public static string removeUrlPrefix(string url, string urlPrefix) {
            string result = url;
            if (!string.IsNullOrEmpty(url) & !string.IsNullOrEmpty(urlPrefix)) {
                if (vbInstr(1, result, urlPrefix, 1) == 1) {
                    result = result.Substring(urlPrefix.Length);
                }
            }
            return result;
        }
        //
        // ====================================================================================================
        /// <summary>convert links as follows
        ///   Preserve URLs that do not start HTTP or HTTPS
        ///   Preserve URLs from other sites (offsite)
        ///   Preserve HTTP://ServerHost/ServerVirtualPath/Files/ in all cases
        ///   Convert HTTP://ServerHost/ServerVirtualPath/folder/page -> /folder/page
        ///   Convert HTTP://ServerHost/folder/page -> /folder/page
        /// </summary>
        /// <param name="url"></param>
        /// <param name="domain"></param>
        /// <param name="appVirtualPath">App virtualPath is typically /appName,  </param>
        /// <returns></returns>
        public static string ConvertLinkToShortLink(string url, string domain, string appVirtualPath) {
            //
            string BadString = "";
            string GoodString = "";
            string Protocol = "";
            string WorkingLink = url;
            //
            // ----- Determine Protocol
            if (vbInstr(1, WorkingLink, "HTTP://", 1) == 1) {
                //
                // HTTP
                //
                Protocol = WorkingLink.Left(7);
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
                GoodString = Protocol + domain;
                if (WorkingLink.IndexOf(GoodString, System.StringComparison.OrdinalIgnoreCase) != -1) {
                    //
                    // URL starts with Protocol ServerHost
                    //
                    GoodString = Protocol + domain + appVirtualPath + "/files/";
                    if (WorkingLink.IndexOf(GoodString, System.StringComparison.OrdinalIgnoreCase) != -1) {
                        //
                        // URL is in the virtual files directory
                        //
                        BadString = GoodString;
                        GoodString = appVirtualPath + "/files/";
                        WorkingLink = vbReplace(WorkingLink, BadString, GoodString, 1, 99, 1);
                    } else {
                        //
                        // URL is not in files virtual directory
                        //
                        BadString = Protocol + domain + appVirtualPath + "/";
                        GoodString = "/";
                        WorkingLink = vbReplace(WorkingLink, BadString, GoodString, 1, 99, 1);
                        //
                        BadString = Protocol + domain + "/";
                        GoodString = "/";
                        WorkingLink = vbReplace(WorkingLink, BadString, GoodString, 1, 99, 1);
                    }
                }
            }
            return WorkingLink;
        }
        //
        // ====================================================================================================
        /// <summary>
        /// todo Please write what this does
        /// </summary>
        /// <param name="url"></param>
        /// <param name="virtualPath"></param>
        /// <param name="appRootPath"></param>
        /// <param name="serverHost"></param>
        /// <returns></returns>
        public static string encodeVirtualPath(string url, string virtualPath, string appRootPath, string serverHost) {
            string result = url;
            bool VirtualHosted = false;
            if ((result.IndexOf(serverHost, System.StringComparison.OrdinalIgnoreCase) != -1) || (url.IndexOf("/") + 1 == 1)) {
                //If (InStr(1, EncodeAppRootPath, ServerHost, vbTextCompare) <> 0) And (InStr(1, Link, "/") <> 0) Then
                //
                // This link is onsite and has a path
                //
                VirtualHosted = (appRootPath.IndexOf(virtualPath, System.StringComparison.OrdinalIgnoreCase) != -1);
                if (VirtualHosted && (url.IndexOf(appRootPath, System.StringComparison.OrdinalIgnoreCase) + 1 == 1)) {
                    //
                    // quick - virtual hosted and link starts at AppRootPath
                    //
                } else if ((!VirtualHosted) && (url.Left(1) == "/") && (url.IndexOf(appRootPath, System.StringComparison.OrdinalIgnoreCase) + 1 == 1)) {
                    //
                    // quick - not virtual hosted and link starts at Root
                    //
                } else {
                    urlDetailsClass urlDetails = splitUrl(url);
                    string path = urlDetails.unixPath();
                    //splitUrl(Link, ref Protocol, ref Host, ref Path, ref Page, ref QueryString);
                    if (VirtualHosted) {
                        //
                        // Virtual hosted site, add VirualPath if it is not there
                        //
                        if (vbInstr(1, urlDetails.unixPath(), appRootPath, 1) == 0) {
                            if (path == "/") {
                                path = appRootPath;
                            } else {
                                path = appRootPath + path.Substring(1);
                            }
                        }
                    } else {
                        //
                        // Root hosted site, remove virtual path if it is there
                        //
                        if (vbInstr(1, path, appRootPath, 1) != 0) {
                            path = vbReplace(path, appRootPath, "/");
                        }
                    }
                    result = urlDetails.protocol + urlDetails.host + path + urlDetails.filename + urlDetails.queryString;
                }
            }
            return result;
        }
        //
        // ====================================================================================================
        /// <summary>
        /// create a linked string. replace the start <link> tag with the provided anchorTag, and convert the end </link> tag to an </a>
        /// </summary>
        /// <param name="AnchorTag">The anchor tag to be inserted (a complete <a> tag)</param>
        /// <param name="AnchorText">A string containing a <link>toBeAnchored</link> tag set</param>
        /// <returns></returns>
        public static string getLinkedText(string AnchorTag, string AnchorText) {
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
                        tempGetLinkedText = tempGetLinkedText.Left(LinkPosition - 1) + "</A>" + tempGetLinkedText.Substring(LinkPosition + 6);
                        LinkPosition = UcaseAnchorText.LastIndexOf("<LINK>", LinkPosition - 2) + 1;
                        if (LinkPosition != 0) {
                            tempGetLinkedText = tempGetLinkedText.Left(LinkPosition - 1) + iAnchorTag + tempGetLinkedText.Substring(LinkPosition + 5);
                        }
                        LinkPosition = UcaseAnchorText.LastIndexOf("</LINK>", LinkPosition - 1) + 1;
                    }
                }
            }
            //
            return tempGetLinkedText;
        }
        //
        // ====================================================================================================
        /// <summary>
        /// encode initial caps
        /// </summary>
        /// <param name="Source"></param>
        /// <returns></returns>
        public static string encodeInitialCaps(string Source) {
            string result = "";
            if (!string.IsNullOrEmpty(Source)) {
                string[] SegSplit = Source.Split(' ');
                int SegMax = SegSplit.GetUpperBound(0);
                if (SegMax >= 0) {
                    for (int SegPtr = 0; SegPtr <= SegMax; SegPtr++) {
                        SegSplit[SegPtr] = vbUCase(SegSplit[SegPtr].Left(1)) + vbLCase(SegSplit[SegPtr].Substring(1));
                    }
                }
                result = string.Join(" ", SegSplit);
            }
            return result;
        }
        //
        // ====================================================================================================
        /// <summary>
        /// attempt to make the word from plural to singular
        /// </summary>
        /// <param name="PluralSource"></param>
        /// <returns></returns>
        public static string getSingular_Sortof(string PluralSource) {
            string result = PluralSource;
            bool UpperCase = false;
            if (result.Length > 1) {
                string LastCharacter = result.Substring(result.Length - 1);
                UpperCase = (LastCharacter == LastCharacter.ToUpper());
                if (vbUCase(result.Substring(result.Length - 3)) == "IES") {
                    result = result.Left(result.Length - 3) + (UpperCase ? "Y" : "y");
                } else if (vbUCase(result.Substring(result.Length - 2)) == "SS") {
                    // nothing
                } else if (vbUCase(result.Substring(result.Length - 1)) == "S") {
                    result = result.Left(result.Length - 1);
                }
            }
            return result;
        }
        //
        // ====================================================================================================
        /// <summary>
        /// encode a string to be used as a javascript single quoted string. 
        /// For example, if creating a string "alert('" + ex.Message + "');"; 
        /// </summary>
        /// <param name="Source"></param>
        /// <returns></returns>
        public static string EncodeJavascriptStringSingleQuote(string Source) {
            return HttpUtility.JavaScriptStringEncode(Source);
            //string tempEncodeJavascript = null;
            ////
            //tempEncodeJavascript = Source;
            //tempEncodeJavascript = vbReplace(tempEncodeJavascript, "\\", "\\\\");
            //tempEncodeJavascript = vbReplace(tempEncodeJavascript, "'", "\\'");
            ////EncodeJavascript = vbReplace(EncodeJavascript, "'", "'+""'""+'")
            //tempEncodeJavascript = vbReplace(tempEncodeJavascript, "\r\n", "\\n");
            //tempEncodeJavascript = vbReplace(tempEncodeJavascript, "\r", "\\n");
            //return vbReplace(tempEncodeJavascript, "\n", "\\n");
            ////
        }
        //
        // ====================================================================================================
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
        // ====================================================================================================
        //
        public static int encodeInteger(object expression) {
            if (expression == null) {
                return 0;
            } else {
                double number = 0;
                string tmpString = expression.ToString();
                if (String.IsNullOrWhiteSpace(tmpString)) {
                    return 0;
                } else {
                    int result = 0;
                    if (Int32.TryParse(tmpString, out result)) {
                        return result;
                    } else if (Double.TryParse(tmpString, out number)) {
                        return Convert.ToInt32(number);
                    } else {
                        return 0;
                    }
                }
            }

        }
        //
        // ====================================================================================================
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
                    returnFirstLine = Body.Left(EOL);
                    Body = Body.Substring(BOL - 1);
                } else {
                    returnFirstLine = Body;
                    Body = "";
                }
            } catch (Exception) { }
            return returnFirstLine;
        }
        //
        // ====================================================================================================
        //
        public static string runProcess(coreController core, string Cmd, string Arguments = "", bool WaitForReturn = false) {
            string returnResult = "";
            Process p = new Process();
            //
            logController.logError(core, "ccCommonModule.runProcess, cmd=[" + Cmd + "], Arguments=[" + Arguments + "], WaitForReturn=[" + WaitForReturn + "]");
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
        // ====================================================================================================
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
        // ====================================================================================================
        //   use only internally
        //       decode an argument removed from a name=value& string
        //       see encodeNvaArgument for details on how to use this
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
        //
        // ====================================================================================================
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
        // ====================================================================================================
        //
        public static bool IsNull(object source) {
            return (source == null);
        }
        //
        // ====================================================================================================
        //
        public static bool isMissing(object source) {
            return false;
        }
        //
        // ====================================================================================================
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
        //
        // ====================================================================================================
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
        // ====================================================================================================
        // the the name of the current executable
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
        //
        // ====================================================================================================
        public static bool isInStr(int start, string haystack, string needle) {
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
                    if (normalizedRoute.Left(1).Equals("/")) {
                        normalizedRoute = normalizedRoute.Substring(1);
                    }
                    if (normalizedRoute.Substring(normalizedRoute.Length - 1, 1) == "/") {
                        normalizedRoute = normalizedRoute.Left(normalizedRoute.Length - 1);
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
                if ((Source.Length == 38) && (Source.Left(1) == "{") && (Source.Substring(Source.Length - 1) == "}")) {
                    //
                    // Good to go
                    //
                    returnValue = true;
                } else if ((Source.Length == 36) && (Source.IndexOf(" ") == -1)) {
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
        //
        // ====================================================================================================
        public static int vbInstr(string string1, string string2, int text1Binary2) {
            return vbInstr(1, string1, string2, text1Binary2);
        }
        //
        // ====================================================================================================
        //
        public static int vbInstr(string string1, string string2) {
            return vbInstr(1, string1, string2, 2);
        }
        //
        // ====================================================================================================
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
        // ====================================================================================================
        //
        public static string vbReplace(string expression, string find, int replacement) { return vbReplace(expression, find, replacement.ToString()); }
        //
        // ====================================================================================================
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
        // ====================================================================================================
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
                returnIsGuid = (guid.Length == 38) && (guid.Left(1) == "{") && (guid.Substring(guid.Length - 1) == "}");
            } catch (Exception ex) {
                throw (ex);
            }
            return returnIsGuid;
        }
        //
        //========================================================================
        // main_encodeCookieName, replace invalid cookie characters with %hex
        //
        public static string main_encodeCookieName(string Source) {
            return encodeURL(Source);
        }
        //
        // ====================================================================================================
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
        //
        public static string main_GetNameValue_Internal(coreController core, string NameValueString, string Name) {
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
        //
        //========================================================================
        //   convert a virtual file into a Link usable on the website:
        //       convert all \ to /
        //       if it includes "://", leave it along
        //       if it starts with "/", it is already root relative, leave it alone
        //       else (if it start with a file or a path), add the serverFilePath
        //
        public static string getCdnFileLink(coreController core, string virtualFile) {
            string returnLink = virtualFile;
            returnLink = genericController.vbReplace(returnLink, "\\", "/");
            if (genericController.vbInstr(1, returnLink, "://") != 0) {
                //
                // icon is an Absolute URL - leave it
                //
                return returnLink;
            } else if (returnLink.Left(1) == "/") {
                //
                // icon is Root Relative, leave it
                //
                return returnLink;
            } else {
                //
                // icon is a virtual file, add the serverfilepath
                //
                return core.appConfig.cdnFileUrl + returnLink;
            }
        }
        //
        // ====================================================================================================
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
                        result = result.Left(LinkPosition - 1) + "</a>" + result.Substring(LinkPosition + 6);
                        LinkPosition = UcaseAnchorText.LastIndexOf("<LINK>", LinkPosition - 2) + 1;
                        if (LinkPosition != 0) {
                            result = result.Left(LinkPosition - 1) + iAnchorTag + result.Substring(LinkPosition + 5);
                        }
                        LinkPosition = UcaseAnchorText.LastIndexOf("</LINK>", LinkPosition - 1) + 1;
                    }
                }
            }
            return result;
        }
        //
        // ====================================================================================================
        //
        public static string convertNameValueDictToREquestString(Dictionary<string, string> nameValueDict) {
            string requestFormSerialized = "";
            if (nameValueDict.Count > 0) {
                foreach (KeyValuePair<string, string> kvp in nameValueDict) {
                    requestFormSerialized += "&" + encodeURL(kvp.Key.Left(255)) + "=" + encodeURL(kvp.Value.Left(255));
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
        /// <summary>
        /// return the path of a pathFilename.
        /// myfilename.txt returns empty
        /// mypath\ returns mypath\
        /// mypath\myfilename returns mypath
        /// mypath\more\myfilename returns mypath\more\
        /// </summary>
        /// <param name="pathFilename"></param>
        /// <returns></returns>
        public static string getPath(string pathFilename) {
            string result = pathFilename;
            if (string.IsNullOrEmpty(result)) {
                return "";
            } else {
                int slashpos = convertToDosSlash(pathFilename).LastIndexOf("\\");
                if (slashpos < 0) {
                    //
                    // -- pathFilename is all filename
                    return "";
                }
                if (slashpos == pathFilename.Length) {
                    //
                    // -- pathfilename is all path
                    return pathFilename;
                } else {
                    //
                    // -- divide path and filename and return just path
                    return pathFilename.Left(slashpos + 1);
                }
            }
        }
        //
        //========================================================================
        // EncodeHTML
        //
        //   Convert all characters that are not allowed in HTML to their Text equivalent
        //   in preperation for use on an HTML page
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
        /// <param name="core"></param>
        /// <param name="SrcOptionList"></param>
        /// <returns></returns>
        public static Dictionary<string, string> convertAddonArgumentstoDocPropertiesList(coreController core, string SrcOptionList) {
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
                                key = key.Left(Pos - 1);
                            }
                            returnList.Add(key, value);
                        }
                    }
                }
            } catch (Exception ex) {
                core.handleException(ex);
                throw;
            }
            return returnList;
        }
        //
        // ====================================================================================================
        /// <summary>
        /// descramble a phrase using twoWayDecrypt. If decryption fails, attempt legacy scramble. If no decryption works, return original scrambled source
        /// </summary>
        /// <param name="core"></param>
        /// <param name="Copy"></param>
        /// <returns></returns>
        public static string TextDeScramble(coreController core, string Copy) {
            string result = "";
            try {
                result = securityController.twoWayDecrypt(core, Copy);
            } catch (Exception) {
                //
                if (!core.siteProperties.allowLegacyDescrambleFallback) {
                    //
                    throw;
                } else {
                    //
                    // -- decryption failed, true legacy descramble
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
                        if (Source.Left(2) != "_a") {
                            result = Copy;
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
                                result += Microsoft.VisualBasic.Strings.Chr(CValue);
                            }
                            //
                            // Test mod
                            //
                            if ((crc % 9).ToString() != Source.Substring(Source.Length - 1, 1)) {
                                //
                                // Nope - set it back to the input
                                //
                                result = Copy;
                            }
                        }
                    } catch (Exception ex) {
                        core.handleException(ex);
                        throw;
                    }
                }
            }
            return result;
        }
        //
        //=============================================================================
        // 
        public static string TextScramble(coreController core, string Copy) {
            return securityController.twoWayEncrypt(core, Copy);
        }
        //
        //====================================================================================================
        /// <summary>
        /// remove html script start and end tags from a string - presumably a javascript string
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string removeScriptTag(string source) {
            string result = source;
            int StartPos = genericController.vbInstr(1, result, "<script", 1);
            if (StartPos != 0) {
                int EndPos = genericController.vbInstr(StartPos, result, "</script", 1);
                if (EndPos != 0) {
                    EndPos = genericController.vbInstr(EndPos, result, ">", 1);
                    if (EndPos != 0) {
                        result = result.Left(StartPos - 1) + result.Substring(EndPos);
                    }
                }
            }
            return result;
        }
        //
        // ====================================================================================================
        /// <summary>
        /// split a string
        /// </summary>
        /// <param name="src"></param>
        /// <param name="delimiter"></param>
        /// <returns></returns>
        public static string[] stringSplit(string src, string delimiter) {
            return src.Split(new[] { delimiter }, StringSplitOptions.None);
        }
        //
        // ====================================================================================================
        /// <summary>
        /// Execute an external program synchonously
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        public static string executeCommandSync(string command) {
            string result = "";
            try {
                System.Diagnostics.ProcessStartInfo procStartInfo = new System.Diagnostics.ProcessStartInfo("%comspec%", "/c " + command);
                procStartInfo.RedirectStandardOutput = true;
                procStartInfo.UseShellExecute = false;
                procStartInfo.CreateNoWindow = true;
                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                proc.StartInfo = procStartInfo;
                proc.Start();
                result = proc.StandardOutput.ReadToEnd();
            } catch (Exception) { }
            return result;
        }
        //
        //====================================================================================================
        /// <summary>
        /// return true if strings match and neither is null
        /// </summary>
        /// <param name="source1"></param>
        /// <param name="source2"></param>
        /// <returns></returns>
        public static bool textMatch(string source1, string source2) {
            if ((source1 == null) || (source2 == null)) {
                return false;
            } else {
                return (source1.ToLower() == source2.ToLower());
            }
        }

    }
}
