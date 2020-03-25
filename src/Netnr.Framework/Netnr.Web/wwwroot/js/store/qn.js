var pObject = {
    bucket: "netnr",
    domain: "http://kodo.netnr.top/",
    opt: {
        fileInputID: "fileInput"
    },
    VAFT: $('input[name="__RequestVerificationToken"]').val()
}, FileCommand = {
    //列表
    list: function () {
        $('#sploading').show();
        $.ajax({
            url: "/Store/QNAPI/list",
            type: 'post',
            data: {
                keywords: $('#txtSearch').val(),
                bucket: pObject.bucket,
                __RequestVerificationToken: pObject.VAFT
            },
            dataType: 'json',
            success: function (data) {
                if (data.Code != 200) {
                    console.log(data);
                    return false;
                }
                data = data.Result.Items;
                var htm = [], i = 0, len = data.length;
                if (len) {
                    //排序
                    data.sort(function (a, b) {
                        return a.Key.toLowerCase() > b.Key.toLowerCase();
                    });

                    htm.push('<table class="table tbList">');
                    for (; i < len;) {
                        var item = data[i++], name = item.Key, size = ConvertSize(item.Fsize), date = String(item.PutTime);
                        date = FormatTime("yyyy-MM-dd HH:mm:ss", new Date(date.substring(0, 10) * 1000));
                        htm.push('<tr>'
                            + '<td style="width:25px"><input type="checkbox" name="ChkList" /></td>'
                            + '<td class="item-tool"><i class="fa fa-file text-muted"></i> &nbsp;<a href="javascript:void(0);" class="it-name">' + name + '</a>'
                            + '<div class="it-control"><a href="' + (pObject.domain + name) + '" class="ic-down" target="_blank">下载</a><a href="javascript:void(0)" class="ic-del fa fa-remove"></a></div></td>'
                            + '<td class="text-muted">' + size + '</td>'
                            + '<td class="d-none d-sm-block text-muted text-right">' + date + '</td>'
                            + '</tr>');
                    }
                    htm.push('</table>');
                } else {
                    htm.push('<div class="h3 text-center">咣 ~</div>');
                }
                $('#divBucket').html(htm.join(''));

                AutoHeight();
            },
            error: function () {
                jz.msg("网络错误");
            },
            complete: function () {
                $('#sploading').hide();
            }
        });
    },
    //上传
    upload: function (file) {
        $('#upKey').val(file.name);

        var fd = new FormData();
        fd.append('file', file);
        fd.append("key", $('#upKey').val());
        fd.append("token", $('#upToken').val());

        //上传
        var xhr = new XMLHttpRequest();
        xhr.upload.onprogress = function (event) {
            if (event.lengthComputable) {
                var per = (event.loaded / event.total) * 100;
                if (!document.otitle) {
                    document.otitle = document.title;
                }
                document.title = per.toFixed(2) + "%";
                $('#divProgress').show().css('width', per + "%");
                if (per >= 100) {
                    document.title = document.otitle;
                    $('#divProgress').hide().css('width', "0");
                }
            }
        };

        xhr.open("post", "//upload.qiniup.com", true);
        xhr.setRequestHeader("X-Requested-With", "XMLHttpRequest");
        xhr.send(fd);
        xhr.onreadystatechange = function (e) {
            if (xhr.readyState == 4) {
                if (xhr.status == 200) {
                    try {
                        var json = $.parseJSON(xhr.responseText);
                        if ("key" in json && "hash" in json) {
                            callback(200);
                        } else if ("error" in json) {
                            jz.msg(json.error);
                            document.title = document.otitle;
                        } else {
                            callback();
                        }
                    } catch (e) {
                        callback();
                    }
                } else {
                    $('#divProgress').hide().css('width', "0");
                    jz.msg("上传异常");
                }
            }

        }
    },
    //存在
    exists: function (name, callback) {
        $.ajax({
            url: "/Store/QNAPI/exists",
            type: 'post',
            data: {
                __RequestVerificationToken: pObject.VAFT,
                key: name,
                bucket: pObject.bucket
            },
            success: function (data) {
                if (data == "0") {
                    callback();
                } else if (data == "1") {
                    jz.confirm({
                        title: '文件已经存在',
                        content: name,
                        okValue: '继续上传并覆盖',
                        ok: function () {
                            callback();
                        }
                    });
                }
            },
            error: function () {
                callback();
            }
        })
    },
    //删除
    del: function (names) {
        $.ajax({
            url: "/Store/QNAPI/del",
            type: 'post',
            data: {
                __RequestVerificationToken: pObject.VAFT,
                keywords: location.hash,
                bucket: pObject.bucket,
                key: names.join(',')
            },
            dataType: 'json',
            success: function (data) {
                data = data.Result;
                if (data == "NotAllow") {
                    jz.msg('拒绝访问，不允许操作');
                } else {
                    var failN = 0, successN = 0;
                    $(data).each(function () {
                        if (this.Code != 200) {
                            failN++;
                        } else {
                            successN++;
                        }
                    });

                    if (failN) {
                        jz.msg('操作失败，已删除 ' + successN + ' 个，失败 ' + failN + ' 个');
                    } else {
                        FileCommand.list();
                        jz.msg('操作成功');
                    }
                }
            }
        })
    },
    //抓取
    fetch: function () {
        var url = $('#txtFetch').val();
        if ($.trim(url) == "" || $('#btnFetch')[0].disabled) {
            return false;
        }
        $('#btnFetch').val('正在抓取资源')[0].disabled = true;
        $.ajax({
            url: "/Store/QNAPI/fetch",
            type: 'post',
            data: {
                __RequestVerificationToken: pObject.VAFT,
                url: url,
                bucket: pObject.bucket
            },
            dataType: 'json',
            success: function (data) {
                $('#btnFetch').val('抓取网络资源')[0].disabled = false;
                if (data.Code == 200) {
                    data = data.Result;
                    var htm = [];
                    htm.push("<p><b>抓取成功</b></p>");
                    htm.push("名称：" + data.key);
                    htm.push("大小：" + ConvertSize(data.fsize));
                    htm.push("类型：" + data.mimeType);
                    jz.confirm({
                        content: htm.join('<br/>'),
                        cancel: false
                    });
                    $('#txtFetch').val('');
                    FileCommand.list();
                } else {
                    jz.msg('抓取失败');
                }
            },
            error: function () {
                $('#btnFetch').val('抓取网络资源')[0].disabled = false;
                jz.msg('网络错误');
            }
        })
    }
}

//抓取
$('#btnFetch').click(FileCommand.fetch);
$('#txtFetch').keydown(function (e) {
    e = e || window.event;
    var key = e.keyCode || e.which || e.charCode;
    if (key == 13) {
        $('#btnFetch')[0].click();
    }
})

//删除
$('#btnDel').click(function () {
    var names = [];
    $('#divBucket').find('input[type="checkbox"]:checked').each(function () {
        names.push($(this).parent().parent().find('a').first().text());
    });
    if (names.length) {
        jz.confirm({
            title: '删除以下文件',
            content: names.join('<br/>'),
            ok: function () {
                FileCommand.del(names);
            }
        })
    } else {
        jz.msg("请选择删除的文件");
    }
});

//点击列表
$('#divBucket').click(function (e) {
    e = e || window.event;
    var target = e.target || e.srcElement, trs = $(this).find('tr');
    if (target.nodeName == "TD") {
        var tr = $(target).parent();
        var chk = tr.find('input').get(0);
        chk.checked = !chk.checked;
        chk.checked ? tr.addClass('in') : tr.removeClass('in');
    } else if (target.nodeName == "INPUT") {
        if (target.checked) {
            $(target).parent().parent().addClass('in');
        } else {
            $(target).parent().parent().removeClass('in');
        }
    } else if (target.nodeName == "A") {
        if (target.className.indexOf('it-name') >= 0) {
            var durl = pObject.domain + $(target).text();
            ViewFileInfo($(target).text(), durl);            
        } else if (target.className.indexOf('ic-del') >= 0) {
            var name = $(target).parent().prev().text();
            jz.confirm({
                title: '是否删除文件',
                content: '<span style="color:#3B8CFF">' + name + '</span>',
                ok: function () {
                    FileCommand.del([name]);
                }
            });
        }
    }
});

function callback(val) {
    switch (val) {
        case 200:
            jz.msg("上传成功");
            FileCommand.list();
            $("#upFile").val('');
            break;
        case 614:
            jz.msg("文件已存在");
            break;
        default:
            jz.msg("上传失败");
    }
}

$.ajax({
    url: '/Store/QNToken',
    type: 'post',
    data: {
        type: "upload",
        uploadConfig: JSON.stringify({
            "scope": pObject.bucket,
            "deadline": parseInt($('#upToken').attr('data-unix'))
        }),
        __RequestVerificationToken: pObject.VAFT
    },
    success: function (data) {
        $('#upToken').val(data);
    }
});