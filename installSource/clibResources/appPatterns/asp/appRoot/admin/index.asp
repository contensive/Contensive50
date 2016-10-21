<% option explicit %>
<!-- #include virtual="/includes/ContensiveConfig42.asp" -->
<%
'
dim cp
Set cp = CreateObject( "Contensive.Processor.CPClass" )
response.write getContensivePage( cp, true )
call cp.dispose()
set cp = nothing
%>
