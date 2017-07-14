//
// -- Child Page Sorting
jQuery(function () {
    jQuery(".ccChildList").sortable({
        stop: function (event, ui) {
            var e, c, s;
            s = jQuery(this).attr('id');
            e = document.getElementById(listId);
            for (i = 0; i < e.childNodes.length; i++) {
                c = e.childNodes[i];
                if (c.id) { s += ',' + c.id }
            }
            cj.ajax.addon('ChildListResort', 'sortlist=' + s);
        }
    });
});
