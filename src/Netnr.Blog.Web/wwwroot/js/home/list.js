var lsar = localStorage.getItem('AnonymousReply');
try {
    lsar = lsar == null ? {} : JSON.parse(lsar);
    $('#WrAnonymousName').val(lsar.name || "");
    $('#WrAnonymousMail').val(lsar.mail || "");
    $('#WrAnonymousLink').val(lsar.link || "");
} catch (e) { }

//目录
var toc = {
    index: 1,
    init: function () {
        var mb = document.querySelector('.markdown-body');
        var nas = mb.getElementsByTagName('*'), hasHeader = false;
        for (var i = 0; i < nas.length; i++) {
            var na = nas[i];
            switch (na.nodeName) {
                case "H1":
                case "H2":
                case "H3":
                case "H4":
                case "H5":
                case "H6":
                    na.id = "toc_" + toc.index++;
                    hasHeader = true;
                    break;
            }
        }

        if (hasHeader) {
            $(mb).parent().addClass('nr-toc');
            tocbot.init({
                tocSelector: '.toc',
                contentSelector: '.markdown-body',
                headingSelector: 'h1, h2, h3, h4, h5, h6',
                hasInnerContainers: true,
            });

        }
    }
}
toc.init();

//初始化MarkDown
if (document.getElementById("replyeditor")) {
    require(['vs/editor/editor.main'], function () {
        window.nmd = new netnrmd('#replyeditor', {
            storekey: "md_autosave_reply"
        });

    });
}

var uid = $('#hid_uid').val();
var wid = $('#hid_wid').val();

//保存
$('#btnReply').click(function () {
    var contentMd = nmd.getmd();
    if (contentMd.length < 1) {
        jz.msg("先写点什么...");
        return false;
    }

    var rname, rlink, rmail;
    //关闭匿名回复
    if (false && document.getElementById('WrAnonymousName')) {
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

    $('#btnReply')[0].disabled = true;

    $.ajax({
        url: "/home/lsitreplysave",
        type: "post",
        data: {
            Uid: uid,
            UrTargetId: wid,
            UrContent: content,
            UrContentMd: contentMd,
            UrAnonymousName: rname,
            UrAnonymousLink: rlink,
            UrAnonymousMail: rmail
        },
        dataType: 'json',
        success: function (data) {
            if (data.code == 200) {
                localStorage.setItem('AnonymousReply', JSON.stringify({
                    name: rname,
                    mail: rmail,
                    link: rlink
                }));
                nmd.setmd('');
                jz.confirm({
                    content: "操作成功",
                    time: 6,
                    cancel: false,
                    remove: function () {
                        location.reload(false);
                    }
                })
            } else {
                jz.msg(data.msg);
            }
        },
        error: function () {
            jz.msg("网络错误");
        },
        complete: function () {
            $('#btnReply')[0].disabled = false;
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
if (location.hostname != "localhost" && sessionStorage.getItem("wid") != wid) {
    sessionStorage.setItem("wid", wid);
    $.get("/home/ListReadPlus/" + wid);
}

//点击图片
$('.markdown-body').click(function (e) {
    e = e || window.event;
    var target = e.target || e.srcElement;
    if (target.nodeName == "IMG") {
        var image = new Image();
        image.src = target.src;
        image.onload = function () {
            if (this.width > 300) {
                var im = document.createElement('div');
                im.style.cssText = "position:fixed;width:100%;height:100%;top:0;left:0;right:0;bottom:0;z-index:999;background-color:rgba(0, 0, 0, .6);padding:15px;opacity:0;transition: opacity .4s";
                im.className = "nav flex-column justify-content-center";
                im.onclick = function (e) {
                    e = e || window.event;
                    if ((e.target || e.srcElement).nodeName != "IMG") {
                        im.style.opacity = 0;
                        setTimeout(function () {
                            try {
                                document.body.removeChild(im)
                            } catch (e) { }
                        }, 400);
                    }
                }
                image.className = "m-auto rounded mw-100 mh-100";
                im.appendChild(image);
                document.body.appendChild(im);
                setTimeout(function () {
                    im.style.opacity = 1;
                }, 50)
            }
        }
    }
})