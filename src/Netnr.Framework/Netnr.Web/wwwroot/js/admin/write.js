//状态
z.DC["dataurl_uwstatus"] = {
    data: [
        { value: 1, text: "✔" },
        //不在列表显示
        { value: 2, text: "BLOCK" },
        //不允许编辑、删除
        { value: -1, text: "LOCK" }
    ],
    init: function () {
        this.onHidePanel = function () {
            gd1.func('endEdit', gd1.ei);
        }
    }
};

//公开
z.DC["dataurl_uwopen"] = {
    data: [
        { value: 1, text: "✔" },
        { value: 2, text: "Private" }
    ],
    init: function () {
        this.onHidePanel = function () {
            gd1.func('endEdit', gd1.ei);
        }
    }
};

var gd1 = z.Grid();
gd1.url = "/admin/querywritelist";
gd1.autosizePid = "#PGrid1";
gd1.pageSize = 100;
gd1.multiSort = true;
gd1.sortName = "UwCreateTime";
gd1.sortOrder = "desc";
gd1.columns = [[
    { title: "UID", field: "UserId", width: 60, sortable: true, align: "center" },
    { title: "昵称", field: "Nickname", width: 150, sortable: true, align: "center" },
    { title: "ID", field: "UwId", width: 60, sortable: true, align: "center" },
    {
        title: "标题", field: "UwTitle", width: 400, sortable: true, formatter: function (value, row) {
            return '<a href="/home/list/' + row.UwId + '" target="_blank">' + value + '</a>';
        }
    },
    { title: "创建时间", field: "UwCreateTime", width: 160, sortable: true, align: "center" },
    { title: "<b class='orange'>回复</b>", field: "UwReplyNum", width: 70, sortable: true, align: "center", FormType: "text" },
    { title: "<b class='orange'>浏览</b>", field: "UwReadNum", width: 70, sortable: true, align: "center", FormType: "text" },
    { title: "<b class='orange'>点赞</b>", field: "UwLaud", width: 70, sortable: true, align: "center", FormType: "text" },
    { title: "<b class='orange'>收藏</b>", field: "UwMark", width: 70, sortable: true, align: "center", FormType: "text" },
    {
        title: "<b class='orange'>公开</b>", field: "UwOpen", width: 90, sortable: true, align: "center", FormType: "combobox", FormUrl: "dataurl_uwopen", formatter: function (value) {
            switch (Number(value)) {
                case 1: value = "✔"; break;
                case 2: value = "Private"; break;
            }
            return value
        }
    },
    {
        title: "<b class='orange'>状态</b>", field: "UwStatus", width: 120, sortable: true, align: "center", FormType: "combobox", FormUrl: "dataurl_uwstatus", formatter: function (value) {
            switch (Number(value)) {
                case 1: value = "✔"; break;
                case 2: value = "BLOCK"; break;
                case -1: value = "ReadOnly"; break;
            }
            return value
        }
    }
]];
//编辑
gd1.onClickCell = function (index, field, value) {
    setTimeout(function () {
        z.GridEditor(gd1, index, field);
    }, 20)
}
gd1.onBeforeLoad = function (row, param) {
    param.pe1 = $('#txtSearch').val().trim();
}
//结束编辑保存
gd1.onEndEdit = function (index, row, changes) {
    if (JSON.stringify(changes) != "{}") {
        $.ajax({
            url: "/admin/WriteAdminSave",
            type: "post",
            data: row,
            dataType: 'json',
            success: function (data) {
                if (data.code != 200) {
                    gd1.load();
                }
                jz.msg("第 " + (index + 1) + " 行，" + data.msg);
            }
        })
    }
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