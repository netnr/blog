var gd1 = z.Grid();
gd1.url = "/Admin/QueryLog";
gd1.pageSize = 100;
gd1.sortName = "LogCreateTime";
gd1.sortOrder = "desc";
gd1.columns = [[
    { title: "QID", field: "LogRequestId", width: 180 },
    { title: "账号", field: "LogName", width: 160 },
    { title: "昵称", field: "LogNickname", width: 100 },
    {
        title: "动作", field: "LogAction", width: 150, formatter: function (value) {
            try {
                return decodeURIComponent(value)
            } catch (e) {
                return value;
            }
        }
    },
    { title: "内容", field: "LogContent", width: 150 },
    {
        title: "链接", field: "LogUrl", width: 300, formatter: function (value) {
            try {
                return decodeURIComponent(value)
            } catch (e) {
                return value;
            }
        }
    },
    { title: "IP", field: "LogIp", width: 150 },
    { title: "引荐", field: "LogReferer", width: 300 },
    { title: "时间", field: "LogCreateTime", width: 180 },
    { title: "浏览器", field: "LogBrowserName", width: 150 },
    { title: "操作系统", field: "LogSystemName", width: 150 },
    { title: "组", field: "LogGroup", width: 80 },
    { title: "级别", field: "LogLevel", width: 80 },
    { title: "备注", field: "LogRemark", width: 100 }
]];
gd1.onDblClickRow = function () {
    var msg = JSON.stringify(gd1.func("getSelected"), null, 4);
    $.each(gd1.columns[0], function () {
        msg = msg.replace('"' + this.field + '"', '"' + this.title + '"')
    });

    jz.popup({
        title: "查看明细",
        content: "<pre class='h6' style='white-space:pre-wrap'>" + msg + "</pre>",
        drag: 1,
        footer: false,
        blank: 1,
        mask: .5
    });
}

gd1.load();