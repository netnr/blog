var lsar = localStorage.getItem('AnonymousReply');
try {
    lsar = lsar == null ? {} : JSON.parse(lsar);
    $('#WrAnonymousName').val(lsar.name || "");
    $('#WrAnonymousMail').val(lsar.mail || "");
    $('#WrAnonymousLink').val(lsar.link || "");
} catch (e) { }

//初始化MarkDown
var nmd = new netnrmd('#txtReply', {
    storekey: "md_autosave_reply",
    storetime: 2,
    height: 150
});

var uid = $('#hid_uid').val();
var wid = $('#hid_wid').val();

//保存
$('#btnReply').click(function () {
    var contentMd = nmd.getmd();
    if (contentMd.length < 1) {
        jz.msg("先写点什么...");
        return false;
    }
    var rname, rlink;
    if (document.getElementById('WrAnonymousName')) {
        rname = $('#WrAnonymousName').val().trim();
        rlink = $('#WrAnonymousLink').val().trim();
        rmail = $('#WrAnonymousMail').val().trim();
        if (rname.trim() == "") {
            jz.msg("请输入昵称")
            return false;
        }
        if (rmail.trim() == "") {
            jz.msg("请输入邮箱")
            return false;
        }
        if (rlink != "" && rlink.toLowerCase().indexOf('http') != 0) {
            jz.msg("链接地址以 http 开头")
            return false;
        }
    }
    var content = nmd.gethtml();

    $.ajax({
        url: "/home/lsitreplysave",
        type: "type",
        data: {
            Uid: uid,
            UrTargetId: wid,
            UrContent: content,
            UrContentMd: contentMd,
            UrAnonymousName: rname,
            UrAnonymousLink: rlink,
            UrAnonymousMail: rmail
        },
        success: function (data) {
            if (data.code == 200) {
                localStorage.setItem('AnonymousReply', JSON.stringify({
                    name: rname,
                    mail: rmail,
                    link: rlink
                }));
                nmd.clear();
                jz.confirm({
                    content: "操作成功",
                    time: 6,
                    cancel: false,
                    remove: function () {
                        location.reload(false);
                    }
                })
            } else {
                jz.msg("操作失败");
            }
        },
        error: function () {
            jz.msg("网络错误");
        }
    });
});


//点赞
$('#btnLaud').click(function () {
    ListUserConn(1)
})
//收藏
$('#btnMark').click(function () {
    ListUserConn(2)
})
function ListUserConn(action) {
    if (window.RA) {
        console.log('The operation is too fast, try again later（' + window.RA + '）');
        return;
    }
    window.RA = 'Laud||Mark';
    $.ajax({
        url: "/home/ListUserConn/" + wid + "?a=" + action,
        dataType: 'json',
        success: function (data) {
            if (data.code == 200) {
                if (action == 1) {
                    var btnlaud = $('#btnLaud');
                    var btnnum = btnlaud.next();
                    if (data.data == 1) {
                        btnlaud[0].className = btnlaud[0].className.replace("btn-outline-", "btn-");
                        btnlaud.attr('title', '取消点赞');
                        btnnum.html(parseInt(btnnum.html()) + 1);
                    } else {
                        btnlaud[0].className = btnlaud[0].className.replace("btn-", "btn-outline-");
                        btnlaud.attr('title', '点赞');
                        btnnum.html(parseInt(btnnum.html()) - 1);
                    }
                } else {
                    var btnmark = $('#btnMark');
                    var btnnum = btnmark.next();
                    if (data.data == 1) {
                        btnmark[0].className = btnmark[0].className.replace("btn-outline-", "btn-");
                        btnmark.attr('title', '取消收藏');
                        btnnum.html(parseInt(btnnum.html()) + 1);
                    } else {
                        btnmark[0].className = btnmark[0].className.replace("btn-", "btn-outline-");
                        btnmark.attr('title', '收藏');
                        btnnum.html(parseInt(btnnum.html()) - 1);
                    }
                }
            }
        },
        error: function (ex) {
            if (ex.status == 401) {
                jz.alert('请先<a href="/account/login">登录</a>')
            } else {
                jz.alert("网络错误");
            }
        },
        complete: function () {
            window.RA = null;
        }
    })
}

//阅读量
if (localStorage.getItem("wid") != wid) {
    localStorage.setItem("wid", wid);
    $.get("/home/ListReadPlus/" + wid);
}