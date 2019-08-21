//搜索高亮
var sk = $.trim($('#txtSearch').val()).toLowerCase();
if (sk != "") {
    $('.uw-box').find('.uw-title').each(function () {
        var nt = this.children[0], ntval = nt.innerHTML, newval = [];
        for (var i in ntval) {
            var c = ntval[i];
            if (sk.indexOf(c.toLowerCase()) >= 0) {
                c = '<b class="skh">' + c + '</b>';
            }
            newval.push(c);
        }
        nt.innerHTML = newval.join('');
    });
}

//Emoji表情
$.getJSON("/lib/emoji/emoji.json", null, function (data) {
    var arri = Math.random().toString().substring(2) + Math.random().toString().substring(2);
    $('span.uw-emoji').each(function (i) {
        this.innerHTML = data.svg + data.data[arri[i]];
    });
});