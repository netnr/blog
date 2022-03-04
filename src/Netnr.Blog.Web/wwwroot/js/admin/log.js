var gd1 = z.Grid();
gd1.url = "/Admin/QueryLog";
gd1.pageSize = 100;
gd1.sortName = "LogCreateTime";
gd1.sortOrder = "desc";
gd1.columns = [[
    { title: "账号", field: "LogUid", width: 160 },
    { title: "昵称", field: "LogNickname", width: 120 },
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
        title: "链接", field: "LogUrl", width: 200, formatter: function (value) {
            try {
                return decodeURIComponent(value)
            } catch (e) {
                return value;
            }
        }
    },
    { title: "IP", field: "LogIp", width: 250 },
    { title: "归属地", field: "LogArea", width: 400 },
    { title: "引荐", field: "LogReferer", width: 300 },
    {
        title: "时间", field: "LogCreateTime", width: 200, formatter: function (value) {
            value = new Date((value - 621355968000000000) / 10000).toISOString().replace("T", " ").replace("Z", "");
            return value;
        }
    },
    { title: "浏览器", field: "LogBrowserName", width: 180 },
    { title: "操作系统", field: "LogSystemName", width: 120 },
    { title: "UA", field: "LogUserAgent", width: 180 },
    {
        title: "组", field: "LogGroup", width: 60, formatter: function (value) {
            switch (value) {
                case "1":
                    value = '用户';
                    break;
                case "2":
                    value = '爬虫';
                    break;
            }
            return value;
        }
    },
    {
        title: "级别", field: "LogLevel", width: 60, formatter: function (value) {
            switch (value) {
                case "F": value = "Fatal"; break;
                case "E": value = "Error"; break;
                case "W": value = "Warn"; break;
                case "I": value = "Info"; break;
                case "D": value = "Debug"; break;
                case "A": value = "All"; break;
            }
            return value;
        }
    },
    { title: "备注", field: "LogRemark", width: 100 }
]];
gd1.onDblClickRow = function () {
    var msg = [];
    $.each(gd1.func("getSelected"), function (k, v) {
        msg.push(k + "：" + v);
    });
    msg = msg.join('\r\n');
    $.each(gd1.columns[0], function () {
        msg = msg.replace(this.field + '：', this.title + '（' + this.field + '）：')
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
gd1.onBeforeLoad = function (row, params) {
    params.wheres = JSON.stringify(gd1.listWhere);
}
gd1.onComplete(function () {
    var tds = gd1.func('getPanel').find('tr.datagrid-header-row').children();
    tds.each(function () {
        var field = $(this).attr('field');
        if (field != null) {
            var fp = $(this).children().css('position', 'relative');
            if (!fp.find('.datagrid-filter-icon').length) {
                $('<span class="datagrid-filter-icon fa fa-filter" data-field="' + field + '"></span>').appendTo(fp).click(function (e) {
                    z.stopEvent(e);
                    qw.view(gd1, field);
                })
            }
        }
    });
})

gd1.listWhere = [];

gd1.load();

var qw = {
    relation: [
        { value: '', text: '（请选择关系符）' },
        { value: '=', text: '等于' },
        { value: 'like', text: '包含' },
        { value: '>=', text: '大于等于' },
        { value: '<=', text: '小于等于' },
        { value: '!=', text: '不等于' }
    ],
    relationView: function (dv) {
        var htm = ['<select class="custom-select">'];
        $.each(qw.relation, function () {
            var isdv = dv == this.value ? " selected " : "";
            var vtit = this.value == '' ? this.text : this.value + '（' + this.text + '）';
            htm.push('<option value="' + this.value + '" ' + isdv + '>' + vtit + '</option>');
        });
        return htm.join('');
    },
    getFieldWhere: function (listWhere, field) {
        var fis = null;
        $.each(listWhere, function () {
            if (this[0] == field) {
                fis = this;
                return false;
            }
        });
        return fis;
    },
    viewReady: function (gd) {
        var list = ['<tr><th>列</th><th>关系符</th><th>值</th></tr>'];
        $.each(gd.columns[0], function () {
            var fis = qw.getFieldWhere(gd.listWhere, this.field);
            var f1 = fis ? fis[1] : null;
            var f2 = fis ? fis[2] : '';
            list.push('<tr>');
            list.push(`<td style="vertical-align:middle" data-value="` + this.field + `">` + this.title + `（` + this.field + `）` + `</td>`);
            list.push(`<td>` + qw.relationView(f1) + `</td>`);
            list.push(`<td><input class="form-control" value="` + f2 + `" /></td>`);
            list.push('</tr>');
        })

        return '<table class="table table-bordered">' + list.join('') + '</table>';
    },
    viewPosition: function (field) {
        var td = $(qw.popup.fn.body).find('td[data-value="' + field + '"]');
        qw.popup.fn.body.scrollTop = 0;
        qw.popup.fn.body.scrollTop = td.offset().top - 150;
        var tr = td.parent();
        tr.find('select')[0].selectedIndex = 1;
        tr.find('input')[0].focus();
    },
    view: function (gd, field) {
        var ct = qw.viewReady(gd);

        qw.popup = jz.popup({
            title: "查询面板",
            content: ct,
            height: "90%",
            mask: .5,
            cancelValue: "重置",
            cancel: function () {
                gd1.listWhere = [];
                qw.popup.fn.body.innerHTML = qw.viewReady(gd);
                qw.viewPosition(field);
                return false;
            },
            okValue: "查询",
            ok: function () {
                gd1.listWhere = qw.getListWhere();
                gd1.load();
            }
        });

        qw.viewPosition(field);
    },
    getListWhere: function () {
        var listWhere = [];
        $(qw.popup).find('tr').each(function (i) {
            if (i) {
                var tds = $(this).children();
                var t1 = tds.eq(0).attr('data-value');
                var t2 = tds.eq(1).find('select').val();
                var t3 = tds.eq(2).find('input').val();

                if (t2 != "" || t3 != "") {
                    listWhere.push([t1, t2, t3]);
                }
            }
        });

        return listWhere;
    }
}