//Monaco
var editor1, editor2, editor3, deferp4;
require(['vs/editor/editor.main'], function () {
    var theme = $('#bgtheme').find('input:checked').val();
    $('#rune').children().each(function (i) {
        var lang = this.getAttribute('data-lang');
        if (i < 3) {
            var tc = htmlDecodeByRegExp(this.innerHTML);
            this.innerHTML = "";
            window["editor" + (i + 1)] = monaco.editor.create(this, {
                value: htmlDecodeByRegExp(tc),
                language: lang,
                automaticLayout: true,
                theme: theme,
                roundedSelection: true,
                scrollBeyondLastLine: true,
                scrollbar: {
                    verticalScrollbarSize: 6,
                    horizontalScrollbarSize: 6
                },
                minimap: {
                    enabled: false
                }
            });
        }
        $('<button class="fre btn btn-outline-info">' + lang + '<i class="fa fa-arrows-alt fa-fw"></i></button>').appendTo(this).click(function () {
            var re = $('#rune');
            if (re.hasClass('rune-full')) {
                re.removeClass('rune-full');
                $(this).parent().removeClass('rune-full-pre');
            } else {
                re.addClass('rune-full');
                $(this).parent().addClass('rune-full-pre');
            }
        });
    });

    $('#bgtheme').click(function () {
        setTimeout(function () {
            var theme = $('#bgtheme').find('input:checked').val();
            monaco.editor.setTheme(theme);

            var rv = $('#runav')[0], rb = $('#runsub')[0];
            var rvcl = rv.className.replace(/bg-\w+/g, '').replace("navbar-light", "").replace("navbar-dark", "");
            var rbcl = rb.className.replace(/bg-\w+/g, '');
            switch (theme) {
                case "vs":
                    rv.className = rvcl + " navbar-light bg-" + theme;
                    rb.className = rbcl + " bg-" + theme;
                    $(rb).children('.card').removeClass('bg-dark');
                    break;
                case "vs-dark":
                case "hc-black":
                    rb.className = rbcl + " bg-" + theme;
                    $(rb).children('.card').addClass('bg-dark');
                    rv.className = rvcl + " navbar-dark bg-" + theme;
                    break;
            }
        }, 10);
    });

    $('#runep')[0].src = "/run/home/preview";
});

function htmlDecodeByRegExp(str) {
    if (str.length == 0) return "";
    return str.replace(/&lt;/g, "<")
        .replace(/&gt;/g, ">")
        .replace(/&nbsp;/g, " ")
        .replace(/&#39;/g, "\'")
        .replace(/&quot;/g, "\"")
        .replace(/&amp;/g, "&");
}

//preview
function RunPreview() {
    $('#runep')[0].src = $('#runep')[0].src;
}

//保存
function SaveRun(st) {
    var msg = [];

    var post = {
        RunCode: $('#hidCode').val(),
        RunRemark: $('#txtRemark').val(),
        RunTheme: $('#bgtheme').find('input:checked').val(),
        RunContent1: editor1.getValue(),
        RunContent2: editor2.getValue(),
        RunContent3: editor3.getValue()
    };

    if (post.RunRemark.trim() == "") {
        msg.push('description is empty');
    }
    if (post.RunRemark.trim().length > 150) {
        msg.push('description content is too long ( less than 150 )');
    }
    var rclen = post.RunContent1 + post.RunContent2 + post.RunContent3;
    if (rclen.length == 0) {
        msg.push('code is empty');
    }

    if (rclen > 10000 * 50) {
        msg.push('code content is too long ( less than 500000 )');
    }

    if (msg.length) {
        alert(msg.join('\r\n'));
        return false;
    }

    $.ajax({
        url: '/run/home/SaveRun',
        data: post,
        type: "post",
        dataType: 'json',
        success: function (data) {
            if (data.code == 200) {
                location.href = "/run/code/" + data.data;
            } else if (data.code == 403) {
                alert("It's not belongs to you");
            } else {
                alert('fail');
            }
        },
        error: function (ex) {
            if (ex.status == 401) {
                alert("Please login first");
            } else {
                alert('error');
            }
        }
    })
}

$('#runsubc').click(function () {
    var rb = $('#runsub'), re = $('#rune');
    if (rb.hasClass('runsub-fold')) {
        rb.removeClass('runsub-fold');
        re.removeClass('rune-fold');
    } else {
        rb.addClass('runsub-fold');
        re.addClass('rune-fold');
    }
});