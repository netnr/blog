var gd1 = z.Grid();
gd1.url = "/Note/QueryNoteList";
gd1.autosizePid = "#PGrid1";
gd1.pageSize = 100;
gd1.multiSort = true;
gd1.fitColumns = true;
gd1.sortName = "NoteCreateTime";
gd1.sortOrder = "desc";
gd1.columns = [[
    { title: "标题", field: "NoteTitle", width: 480, sortable: true },
    { title: "创建时间", field: "NoteCreateTime", width: 150, sortable: true, align: "center" },
    { title: "更新时间", field: "NoteUpdateTime", width: 150, sortable: true, align: "center" }
]];
gd1.onDblClickRow = function () {
    $('#btnEdit')[0].click();
}
gd1.onBeforeLoad = function (row, param) {
    param.pe1 = $('#txtSearch').val().trim();
}
gd1.load();

//搜索
$('#txtSearch').keydown(function (e) {
    e = e || window.event;
    if (e.keyCode == 13) {
        $('#btnSearch')[0].click();
    }
})
$('#btnSearch').click(function () {
    gd1.pageNumber = 1;
    gd1.load();
});

require(['vs/editor/editor.main'], function () {
    window.nmd = new netnrmd('#mdeditor', {
        storekey: "md_autosave_note",
        //执行命令前回调
        cmdcallback: function (cmd) {
            if (cmd == "full") {
                if (nmd.obj.editor.hasClass('netnrmd-fullscreen')) {
                    $('#ModalNote').addClass('modal');
                } else {
                    $('#ModalNote').removeClass('modal');
                }
            }
        },
        input: function () {
            try {
                $('#spwordcount').html("共 <b>" + nmd.getmd().length + "</b> 个字");
            } catch (e) {
                $('#spwordcount').html("共 <b>0</b> 个字");
            }
        }
    });

    //换行
    nmd.obj.me.updateOptions({
        wordWrap: "on"
    });

    //快捷键
    nmd.obj.me.addCommand(monaco.KeyMod.CtrlCmd | monaco.KeyCode.KEY_S, function () {
        $('#btnSave')[0].click();
    })

    mdautoheight();

    //保存编辑器视图
    var vm = parseInt(localStorage.getItem('note_md_viewmodel'));
    if ([1, 2, 3].indexOf(vm) >= 0) {
        nmd.toggleView(vm);
    }
    window.onbeforeunload = function () {
        localStorage.setItem('note_md_viewmodel', nmd.obj.viewmodel);
    }
});

var noteid;

//新增
$('#btnAdd').click(function () {
    noteid = 0;
    $('#NoteTitle').val('');
    nmd.setmd('');
    $('#sptimeinfo').html('')
    $('#ModalNote').modal();
});

//编辑
$('#btnEdit').click(function () {
    var rowData = gd1.func('getSelected');
    noteid = rowData.NoteId;
    if (rowData) {
        $.ajax({
            url: "/Note/QueryNoteOne?id=" + noteid,
            dataType: 'json',
            success: function (data) {
                if (data.code == 200) {
                    var item = data.data;
                    $('#NoteTitle').val(item.NoteTitle);
                    nmd.setmd(item.NoteContent);
                    $('#sptimeinfo').html("创建时间：" + item.NoteCreateTime + " ， 更新时间：" + (item.NoteUpdateTime || item.NoteCreateTime))
                    $('#ModalNote').modal();
                } else {
                    jz.msg(data.msg);
                }
            },
            error: function () {
                jz.msg("网络错误");
            }
        })
    } else {
        jz.msg("请选择一行再操作");
    }
});

//保存
$('#btnSave').click(function () {
    var title = $('#NoteTitle').val().trim();
    var md = nmd.getmd();
    var errmsg = [];
    if (title == "") {
        errmsg.push("请输入 标题");
    }
    if (md.length < 2) {
        errmsg.push("多写一点内容哦");
    }
    if (errmsg.length > 0) {
        jz.alert(errmsg.join('<br/>'));
        return false;
    }

    $('#btnSave')[0].disabled = true;

    $.ajax({
        url: "/note/savenote",
        type: "post",
        data: {
            NoteTitle: title,
            NoteContent: md,
            NoteId: noteid
        },
        dataType: 'json',
        success: function (data) {
            if (data.code == 200) {
                if (noteid == 0) {
                    noteid = data.data;
                }
                gd1.load();
            }
            jz.msg(data.msg)
        },
        error: function (ex) {
            if (ex.status == 401) {
                jz.msg("请登录");
            } else {
                jz.msg("网络错误");
            }
        },
        complete: function () {
            $('#btnSave')[0].disabled = false;
        }
    });
});

//删除
$('#btnDel').click(function () {
    var rowData = gd1.func('getSelected');
    if (rowData) {
        jz.confirm({
            content: "确定删除？",
            ok: function () {
                $.ajax({
                    url: "/Note/DelNote?id=" + rowData.NoteId,
                    dataType: 'json',
                    success: function (data) {
                        if (data.code == 200) {
                            gd1.load();
                        } else {
                            jz.msg("操作失败");
                        }
                    },
                    error: function () {
                        jz.msg("网络错误");
                    }
                })
            }
        });
    } else {
        jz.msg("请选择一行再操作");
    }
});

$(window).on('resize', function () {
    mdautoheight();
});

$('#ModalNote').on('shown.bs.modal', function () {
    mdautoheight();
    if (noteid == 0) {
        $('#NoteTitle')[0].focus();
    }
})

function mdautoheight() {
    var vh = $(window).height() - 130;
    nmd.height(Math.max(100, vh));
}