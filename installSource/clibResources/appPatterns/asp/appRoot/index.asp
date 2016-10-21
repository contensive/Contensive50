<% option explicit %>
<!-- #include virtual="/includes/ContensiveConfig42.asp" -->
<%
'
dim cp,page
Set cp = CreateObject( "Contensive.Processor.CPClass" )
page = getContensivePage( cp, false )
if cp.response.isOpen() Then
	'
	' page is open, modify it
	'
	page = replace( page, "$myCustomTag$", "<div>cp.user.name = " & cp.user.name & "</div>" )
end if
response.write page
cp.dispose()
set cp = nothing
%>
