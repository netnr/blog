function time(format) {
    function t(n) { return n < 10 ? "0" + n : n }
    var d = new Date(),
        y = d.getFullYear(),
        m = t(d.getMonth() + 1),
        day = t(d.getDate()),
        h = t(d.getHours()),
        min = t(d.getMinutes()),
        s = t(d.getSeconds()),
        f = d.getMilliseconds();
    return !arguments.length ? (y + "-" + m + "-" + day + " " + h + ":" + min + ":" + s) :
        format.replace("yyyy", y).replace("MM", m).replace("dd", day).replace("HH", h).replace("mm", min).replace("ss", s).replace("ff", f)
};

var RVT = $('input[name="__RequestVerificationToken"]').val();
$("#spdate").html(time("yyyy-MM-dd HH:mm"));

$("#txtSearch").keydown(function (e) {
    e = e || window.event;
    var keys = e.keyCode || e.which || e.charCode;
    keys == 13 && $("#btnSearch")[0].click();
})
$("#btnSearch").click(QueryNotepad);
QueryNotepad();
function QueryNotepad() {
    $.ajax({
        url: "/tool/QueryNote",
        type: "post",
        data: {
            KeyWords: $("#txtSearch").val(),
            __RequestVerificationToken: RVT
        },
        dataType: "json",
        success: function (data) {
            var htm = [];
            htm.push('<ul id="ulNote" class="ulNote">');
            $(data).each(function () {
                htm.push('<li name="' + this.NotepadId + '" title="' + this.Ndate + '">' + this.Ntitle + '<em class="deleteCss">×</em></li>');
            });
            $("#noteTitle").html(htm.join('') + "</ul>");
        },
        error: function (ex) {
            if (ex.status == 401) {
                $("#noteTitle").html('<div style="line-height:200px;text-align:center">请登录</div>');
            } else {
                jz.alert("网络错误");
            }
        },
        complete: function () {
            $("#noteTitle").find("li").each(function () {
                if ($("#btnSave").attr("name") == this.getAttribute("name")) {
                    this.style.color = "blue";
                    return false;
                }
            });
        }
    });
}

$("#noteTitle").click(function (e) {
    e = e || window.event;
    var target = e.target || e.srcElement;
    if (target.nodeName == "LI") {
        $("#noteTitle").find("li").each(function () {
            this.style.color = "#333";
        });

        var id = $(target).css("color", "blue").attr("name");
        $.ajax({
            url: "/tool/QueryNoteOne",
            type: "post",
            data: {
                NotepadId: id,
                __RequestVerificationToken: RVT
            },
            dataType: "json",
            success: function (data) {
                $("#txtTitle").val(data.Ntitle);
                $("#txtContent").val(data.Ncontent);
                $("#spdate").html(data.Ndate);

                $("#btnSave").attr("name", id);
                $("#txtContent")[0].focus();
                $("#snum").html($("#txtContent").val().length);

                if ($(document).width() < 768) {
                    $("#divLeft").attr("data-show", 0);
                    $("#divLeft")[0].className += " d-none d-sm-block";
                    $("#divRight")[0].className = $("#divRight")[0].className.replace(" d-none d-sm-block", '');
                }
            },
            error: function () {
                jz.alert("网络错误");
            }
        })
    } else if (target.nodeName == "EM") {
        jz.confirm({
            content: "确定删除该条记事？",
            single: true,
            ok: function () {
                $.ajax({
                    url: "/tool/DelNote",
                    type: "post",
                    data: {
                        NotepadId: $(target).parent().attr("name"),
                        __RequestVerificationToken: RVT
                    },
                    success: function (data) {
                        data == "success" && QueryNotepad();
                        jz.alert(data == "success" ? "删除成功" : "删除失败");
                    },
                    error: function () {
                        jz.alert("网络错误");
                    }
                })

                target.parentElement.style.color == "blue" && $("#btnNew")[0].click();
            }
        })
    }
});

$("#btnNew").click(function () {
    $("#noteTitle").find("li").each(function () {
        this.style.color = "#333";
    });

    $("#txtContent").val("");
    $("#spdate").html(time());

    $("#btnSave").attr("name", 0);
    $("#txtTitle").val("");
    $("#txtContent")[0].focus();
    $("#snum").html(0);
});

$('#txtTitle').keydown(function (e) {
    e = e || window.event;
    if (e.keyCode == 13) {
        setTimeout(function () {
            $("#txtContent")[0].focus();
        }, 100);
    }
})[0].focus();

$(document).keydown(function (e) {
    e = e || window.event;
    var keys = e.keyCode || e.which || e.charCode;
    if (keys == 13 && e.ctrlKey) { $("#btnSave")[0].click(); }
}).dblclick(function () {
    if ($("#divLeft").attr("data-show") == 0) {
        $("#divLeft").attr("data-show", 1);
        $("#divRight")[0].className += " d-none d-sm-block";
        $("#divLeft")[0].className = $("#divRight")[0].className.replace(" d-none d-sm-block", '');
    }
});
$(window).resize(function () {
    if ($(window).width() < 769) {
        $("#divLeft")[0].className = "col-md-3 col-sm-4";
        $("#divRight")[0].className = "col-md-9 col-sm-8 d-none d-sm-block";
    }
});

$("#btnSave").click(function () {
    if ($("#txtContent").val() == "") {
        jz.msg("先写点什么了再保存哦");
    } else {
        this.disabled = true;
        var nid = this.name;
        $.ajax({
            url: "/tool/SaveNote",
            type: "post",
            data: {
                NotepadId: nid,
                NDate: $('#spdate').html(),
                NTitle: $("#txtTitle").val(),
                NContent: $("#txtContent").val(),
                __RequestVerificationToken: RVT
            },
            dataType: 'json',
            success: function (data) {
                if (nid == 0) {
                    $("#btnSave").attr("name", data.NotepadId);
                }
                jz.alert("保存成功", { time: 3 });
                QueryNotepad();
            },
            error: function (ex) {
                jz.alert("o(︶︿︶)o 唉 保存失败");
            },
            complete: function () {
                $("#btnSave")[0].disabled = false;
            }
        })
    }
});

$("#txtContent").keyup(function () {
    $("#snum").html(this.value.length);
}).blur(function () {
    $("#snum").html(this.value.length);
}).keydown(function (e) {
    e = e || event;
    currKey = e.keyCode || e.which || e.charCode;
    if (currKey == 119) {
        var position = 0;
        if (document.selection) { //for IE
            $("#txtContent")[0].focus();
            var sel = document.selection.createRange();
            sel.moveStart('character', -$("#txtContent").val().length);
            position = sel.text.length;
        } else if ($("#txtContent")[0].selectionStart || $("#txtContent")[0].selectionStart == '0') {
            position = $("#txtContent")[0].selectionStart;
        }
        var valTemp = $("#txtContent").val();
        $("#txtContent").val(valTemp.substring(0, position) + time() + valTemp.substring(position));

    }
    else {
        if (-[1,]) {  //IE9++
            if (currKey == 9) {
                e.preventDefault();
                var start = this.selectionStart, end = this.selectionEnd,
                    text = this.value, tab = '　　';
                text = text.substr(0, start) + tab + text.substr(start);
                this.value = text;
                this.selectionStart = start + tab.length;
                this.selectionEnd = end + tab.length;
            }
        }
        else {
            var code, sel, tmp, r;
            event.returnValue = false;
            sel = event.srcElement.document.selection.createRange();
            r = event.srcElement.createTextRange();

            if (currKey == 9) {
                if (sel.getClientRects().length > 1) {
                    code = sel.text;
                    tmp = sel.duplicate();
                    tmp.moveToPoint(r.getBoundingClientRect().left, sel.getClientRects()[0].top);
                    sel.setEndPoint("startToStart", tmp);
                    sel.text = "　　" + sel.text.replace(/\r\n/g, "\r\t");
                    code = code.replace(/\r\n/g, "\r\t");
                    r.findText(code);
                    r.select();
                }
                else {
                    sel.text = "　　";
                    sel.select();
                }
            }
            else {
                event.returnValue = true;
            }
        }
    }
});