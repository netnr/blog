$(function () {
    //导航菜单选中
    $('#storeMenu').find('a').each(function () {
        if (this.href.indexOf(location.pathname) >= 0) {
            this.children[0].style.borderColor = "#FF03F7";
            return false;
        }
    });

    //获取列表
    FileCommand.list();

    //上传初始化
    $('#' + pObject.opt.fileInputID).change(function () {
        if (this.files.length) {
            var file = this.files[0] || {};
            FileCommand.upload(file)
        }
    });

    //拖拽上传
    $(document).on("dragleave dragenter dragover", function (e) {
        if (e && e.stopPropagation) { e.stopPropagation() } else { window.event.cancelBubble = true }
        if (e && e.preventDefault) { e.preventDefault() } else { window.event.returnValue = false }
    }).on("drop", function (e) {
        if (e && e.preventDefault) { e.preventDefault() } else { window.event.returnValue = false }
        e = e || window.event;
        var files = (e.dataTransfer || e.originalEvent.dataTransfer).files;
        if (files && files.length) {
            FileCommand.upload(files[0]);
        }
    });

    //搜索
    $('#txtSearch').keydown(function (e) {
        e = e || window.event;
        var key = e.keyCode || e.which || e.charCode;
        if (key == 13) {
            $('#btnSearch')[0].click();
        }
    });
    $('#btnSearch').click(function () {
        FileCommand.list();
    })
});

//单位转换
function ConvertSize(num) {
    num = parseFloat(num) || 0;
    var si = 0, su = ["", "K", "M", "G", "T", "P", "E"];
    while (num > 1024) {
        num = num / 1024;
        si++;
    }
    return (num.toFixed(1) == parseInt(num) ? parseInt(num) : num.toFixed(1)) + su[si];
}

//日期格式化
function FormatTime(t, date) {
    var date = new Date(date);
    var o = {
        "M+": date.getMonth() + 1,
        "d+": date.getDate(),
        "H+": date.getHours(),
        "m+": date.getMinutes(),
        "s+": date.getSeconds(),
        "q+": Math.floor((date.getMonth() + 3) / 3),
        "S": date.getMilliseconds()
    };
    if (/(y+)/.test(t)) {
        t = t.replace(RegExp.$1, (date.getFullYear() + "").substr(4 - RegExp.$1.length));
    };
    for (var k in o) {
        if (new RegExp("(" + k + ")").test(t)) {
            t = t.replace(RegExp.$1, (RegExp.$1.length == 1) ? (o[k]) : (("00" + o[k]).substr(("" + o[k]).length)));
        };
    }
    return t;
};

//阻止默认事件
function stopDefault(e) {
    if (e && e.preventDefault) {
        e.preventDefault()
    } else {
        window.event.returnValue = false
    }
};

//显示详情
function ViewFileInfo(filename, url, originurl) {
    var tt = '<textarea class="form-control mb-3" onfocus="this.select()" rows="5">' + url + '</textarea>'
        + '<a href="' + url + '" target="_blank" class="btn btn-warning btn-block">直接下载</a>';
    if (originurl) {
        tt += '<a href="' + originurl + '" target="_blank" class="btn btn-dark btn-block mt-3">原始下载</a>'
    }
    jz.confirm({
        title: '文件：<span class="text-success">' + filename + '</span>',
        content: tt,
        cancel: false,
        single: true,
        okValue: '关闭'
    });
}

//列表高度自适应
function AutoHeight() {
    var dbt = $('#divBucket');
    dbt.css('max-height', $(window).height() - dbt.offset().top - 20);
}
$(window).on('resize load', AutoHeight);