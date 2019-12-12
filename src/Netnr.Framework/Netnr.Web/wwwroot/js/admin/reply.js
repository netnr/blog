//状态
z.DC["dataurl_urstatus"] = {
    data: [
        { value: 1, text: "✔" },
        //不在列表显示
        { value: 2, text: "BLOCK" }
    ],
    init: function () {
        this.onHidePanel = function () {
            gd1.func('endEdit', gd1.ei);
        }
    }
};

var gd1 = z.Grid();
gd1.url = "/admin/queryreplylist";
gd1.autosizePid = "#PGrid1";
gd1.pageSize = 100;
gd1.multiSort = true;
gd1.sortName = "UrCreateTime";
gd1.sortOrder = "desc";
gd1.autoRowHeight = false;
gd1.columns = [[
    { title: "UID", field: "UserId", width: 60, sortable: true, align: "center" },
    { title: "昵称", field: "Nickname", width: 120, sortable: true, align: "center" },
    { title: "<b class='orange'>匿名昵称</b>", field: "UrAnonymousName", width: 120, sortable: true, align: "center", FormType: "text" },
    { title: "<b class='orange'>匿名邮箱</b>", field: "UrAnonymousMail", width: 180, sortable: true, align: "center", FormType: "text" },
    { title: "<b class='orange'>匿名链接</b>", field: "UrAnonymousLink", width: 160, sortable: true, align: "center", FormType: "text" },
    { title: "ID", field: "UrId", width: 60, sortable: true, align: "center" },
    { title: "目标分类", field: "UrTargetType", width: 120, sortable: true, align: "center" },
    { title: "目标ID", field: "UrTargetId", width: 80, sortable: true, align: "center" },
    {
        title: "内容", field: "UrContent", width: 60, align: "center", formatter: function (value) {
            return '<a href="javascript:void(0)" class="btn btn-sm btn-outline-secondary fa fa-eye"></a>';
        }
    },
    { title: "创建时间", field: "UrCreateTime", width: 160, sortable: true, align: "center" },
    {
        title: "<b class='orange'>状态</b>", field: "UrStatus", width: 120, sortable: true, align: "center", FormType: "combobox", FormUrl: "dataurl_urstatus", formatter: function (value) {
            switch (Number(value)) {
                case 1: value = "✔"; break;
                case 2: value = "BLOCK"; break;
            }
            return value
        }
    }
]];
//编辑
gd1.onClickCell = function (index, field, value) {
    setTimeout(function () {
        if (field == "UrContent") {
            if (!gd1.viewContent) {
                var vc = document.createElement('div');
                var htm = [];
                htm.push('<div class="modal fade"><div class="modal-dialog modal-dialog-scrollable modal-dialog-centered"><div class="modal-content modal-lg">');
                htm.push('<div class="modal-header"><h5 class="modal-title">内容</h5><button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button></div>');
                htm.push('<div class="modal-body"></div>')
                htm.push('</div></div></div>');
                vc.innerHTML = htm.join('');
                document.body.appendChild(vc);
                gd1.viewContent = vc;
            }
            $(gd1.viewContent).children().modal().find('.modal-body').html(value);
        } else {
            z.GridEditor(gd1, index, field);
        }
    }, 20)
}
gd1.onBeforeLoad = function (row, param) {
    param.pe1 = $('#txtSearch').val().trim();
}
//结束编辑保存
gd1.onEndEdit = function (index, row, changes) {
    if (JSON.stringify(changes) != "{}") {
        $.ajax({
            url: "/admin/ReplyAdminSave",
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