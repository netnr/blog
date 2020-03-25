//搜索高亮
var sk = $.trim($('#txtSearch').val()).toLowerCase();
if (sk != "") {
    $('.uw-box').find('.uw-title').each(function () {
        var nt = this.getElementsByTagName('a')[0], ntval = nt.innerHTML, newval = [];
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
$('span.uw-emoji').each(function (i) {
    this.innerHTML = '<img src="https://cdn.jsdelivr.net/gh/netnr/emoji/emoji/wangwang/' + Math.floor(Math.random() * 98) + '.gif" alt="emoji" />';
});