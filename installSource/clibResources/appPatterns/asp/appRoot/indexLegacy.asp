<!-- #include virtual="/ccLib/LibAPI33.asp" -->
<%
dim body
'
'	LibApi creates the ccLib object and initializes it within the context, classic asp in this example
'	Contensive init() - Blank AppName will identify App by URL domain name
'
Call ccLib.Init("")
'
' 	getHtmlDoc() builds the complete document.
' 	getHtmlBody() builds the <body></body> content.
'	getHtmlHead() build the <head></head> innerHtml, call after getHtmlBody()
'
body = ccLib.GetHTMLBody()
Response.Write( ccLib.docType )
Response.Write( "<html>" )
Response.Write( "<head>" & ccLib.GetHTMLHead() & "</head>" )
Response.Write( "<body class=ccBodyWeb>" )
Response.Write( body & ccLib.GetClosePage( true, true ))
Response.Write( "</body>" )
Response.Write( "</html>" )
%>


