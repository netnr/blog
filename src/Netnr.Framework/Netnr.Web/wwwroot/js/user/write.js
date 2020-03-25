var gd1 = z.Grid();
gd1.url = "/user/writelist";
gd1.autosizePid = "#PGrid1";
gd1.pageSize = 100;
gd1.multiSort = true;
gd1.sortName = "UwId";
gd1.sortOrder = "desc";
gd1.columns = [[
    { title: "ID", field: "UwId", width: 60, sortable: true, align: "center" },
    {
        title: "标题", field: "UwTitle", width: 660, sortable: true, formatter: function (value, row) {
            return '<a href="/home/list/' + row.UwId + '" target="_blank">' + value + '</a>';
        }
    },
    { title: "创建时间", field: "UwCreateTime", width: 160, sortable: true, align: "center" },
    { title: "更新时间", field: "UwUpdateTime", width: 160, sortable: true, align: "center" },
    { title: "回复", field: "UwReplyNum", width: 60, sortable: true, align: "center" },
    { title: "浏览", field: "UwReadNum", width: 60, sortable: true, align: "center" },
    { title: "点赞", field: "UwLaud", width: 60, sortable: true, align: "center" },
    { title: "收藏", field: "UwMark", width: 60, sortable: true, align: "center" },
    {
        title: "公开", field: "UwOpen", width: 90, sortable: true, align: "center", formatter: function (value) {
            return value == 1 ? "✔" : "✘";
        }
    },
    {
        title: "状态", field: "UwStatus", width: 120, sortable: true, align: "center", formatter: function (value) {
            switch (value) {
                case 1: return "✔"; break;
                case 2: return "BLOCK"; break;
                case -1: return "LOCK"; break;
            }
            return value
        }
    }
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

//编辑
$('#btnEdit').click(function () {
    var rowData = gd1.func('getSelected');
    if (rowData) {
        if (rowData.UwStatus == -1) {
            jz.alert("文章已被锁定，不能操作");
            return false;
        }

        $.ajax({
            url: "/user/writeone?id=" + rowData.UwId,
            dataType: 'json',
            success: function (data) {
                if (data.code == 200) {
                    var tags = data.data.tags, data = data.data.item;
                    $('#btnSaveEdit').attr('data-id', data.UwId);
                    $('#wtitle').val(data.UwTitle);
                    $('#wcategory').val(data.UwCategory);
                    $('#tags').val(data.tags);
                    nmd.setmd(data.UwContentMd);
                    nmd.render();

                    TagsClear();
                    $(tags).each(function () {
                        TagsAdd(this.TagId, this.TagName)
                    });

                    $('#ModalWrite').modal();
                } else {
                    jz.msg("获取文章出错");
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

//删除
$('#btnDel').click(function () {
    var rowData = gd1.func('getSelected');
    if (rowData) {
        if (rowData.UwStatus == -1) {
            jz.alert("文章已被锁定，不能操作");
            return false;
        }

        jz.confirm({
            content: "确定删除该文章（含回复内容）？",
            ok: function () {
                $.ajax({
                    url: "/user/writedel?id=" + rowData.UwId,
                    dataType: 'json',
                    success: function (data) {
                        if (data.code == 200) {
                            gd1.load();
                        } else {
                            jz.msg(data.msg);
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

$(window).on('load resize', function () {
    mdautoheight();
});

$('#ModalWrite').on('shown.bs.modal', function () {
    mdautoheight();
})

function mdautoheight() {
    var vh = $(window).height() - nmd.obj.container.offset().top - 30;
    nmd.height(Math.max(100, vh));
}