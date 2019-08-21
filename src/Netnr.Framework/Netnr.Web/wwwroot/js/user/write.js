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
    { title: "创建时间", field: "UwCreateTime", width: 150, sortable: true, align: "center" },
    { title: "更新时间", field: "UwUpdateTime", width: 150, sortable: true, align: "center" },
    { title: "回复", field: "UwReplyNum", width: 60, sortable: true, align: "center" },
    { title: "浏览", field: "UwReadNum", width: 60, sortable: true, align: "center" },
    { title: "点赞", field: "UwLaud", width: 60, sortable: true, align: "center" },
    { title: "收藏", field: "UwMark", width: 60, sortable: true, align: "center" },
    {
        title: "公开", field: "UwOpen", width: 60, sortable: true, align: "center", formatter: function (value) {
            return value == 1 ? "✔" : "✘";
        }
    },
    {
        title: "状态", field: "UwStatus", width: 60, sortable: true, align: "center", formatter: function (value) {
            return value == 1 ? "✔" : "✘";
        }
    }
]];
gd1.onDblClickRow = function () {
    $('#btnEdit')[0].click();
}

gd1.load();

//编辑
$('#btnEdit').click(function () {
    var rowData = gd1.func('getSelected');
    if (rowData) {
        $.ajax({
            url: "/user/writeone",
            type: 'post',
            data: {
                UwId: rowData.UwId
            },
            dataType: 'json',
            success: function (data) {
                var tags = data.tags, data = data.item;
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
        jz.confirm({
            content: "确定删除该文章（含回复内容）？",
            ok: function () {
                $.ajax({
                    url: "/user/writedel",
                    type: 'post',
                    data: {
                        UwId: rowData.UwId
                    },
                    success: function (data) {
                        if (data == "success") {
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

$(window).on('load resize', function () {
    mdautoheight();
});

$('#ModalWrite').on('shown.bs.modal', function () {
    mdautoheight();
})

function mdautoheight() {
    var vh = $(window).height() - nmd.obj.container.offset().top - 85;
    nmd.height(Math.max(100, vh));
}