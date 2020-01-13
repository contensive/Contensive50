function getContent()

	dim stream
	dim cs
	dim copyName
	dim copyGuid
	dim copyBody
	dim criteria
	dim copyId
	'
	' try legacy name first, then use instanceId
	'
	copyId = 0
	stream = ""
	copyName = cp.doc.gettext("tickerName")
	if copyName <> "" then
		criteria = "(Name=" & ccLib.EncodeSQLText(copyName) & ")"
	else
		copyGuid = cp.doc.gettext( "InstanceId" )
		if copyGuid = "-2" then
			copyGuid = "created in admin site for member " & ccLib.MemberId
		end if
		if copyGuid <> "" then
			criteria = "(ccguid=" & ccLib.EncodeSQLText(copyGuid) & ")"
		end if
	end if
	if criteria <> "" then
		cs = ccLib.OpenCSContent("Copy Content", criteria)
		if not ccLib.isCSOK( cs ) then
			call ccLib.closecs( cs ) 
			cs = ccLib.InsertCsRecord("Copy Content")
			if ccLib.CSOK(cs) then
				if copyName="" then
					copyName = "Custom Content Scroll for page " & ccLib.RenderedPageName
				end if
				call ccLib.setCs( CS, "name", copyName )
				call ccLib.setCs( CS, "ccGuid", copyGuid )
			end if
		end if
		if ccLib.CSOK(cs) then
			copyBody = ccLib.GetCSText(cs, "Copy")
			copyId = ccLib.getcsinteger( cs, "id" )
		end if
		call ccLib.CloseCS(cs)
	end if
	'
	if (copyid<>0) and (ccLib.isEditing( "copy Content" )) then
		stream = stream & "<div>" & ccLib.getRecordEditLink( "copy content", copyId, false ) & "&nbsp;&nbsp;&nbsp;&nbsp;Edit the scrolling content</div>"
	end if
	stream = stream & "<div class=""scrollContainer"">"
	stream = stream & "<marquee behavior=""scroll"" direction=""up"" scrollamount=""1"" height=""" & cp.doc.gettext("height") & """ width=""" & cp.doc.gettext("width") & """>"
	stream = stream & copyBody
	stream = stream & "</marquee>"
	stream = stream & "</div>"
	'
	getContent = stream

end function