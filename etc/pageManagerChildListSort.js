function saveSortable(listId){
	var e,c,s;
	s=listId;
	e=document.getElementById(listId);
	for (i=0;i<e.childNodes.length;i++){
		c=e.childNodes[i];
		if(c.id) {s+=','+c.id }
	}
	cj.ajax.addon('ChildListResort','sortlist='+s)
}
jQuery(document).ready(function() {
	jQuery(".ccEditWrapper .ccChildList").sortable({
			stop: function(event, ui) {
				saveSortable(jQuery(this).attr('id')); 
			}
	});
})





