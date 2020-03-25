var pObject = {
    opt: {
        fileInputID: "fileInput"
    },
    bucket: "netnr",
    keys: {
        VAFT: $('input[name="__RequestVerificationToken"]').val()
    },
    paths: [],
    path: function () {
        return "/" + this.paths.join('/');
    },
    pathView: function () {
        if (this.paths.length) {
            var htm = [], i = 0, len = this.paths.length;
            //htm.push('<a href="javascript:void(0);" data-cmd="upper">↰ 上一级</a>');
            htm.push('<a href="javascript:void(0);" data-cmd="root">/</a>');
            for (; i < len - 1;) {
                htm.push('<a href="javascript:void(0);">' + this.paths[i++] + '</a>/');
            }
            htm.push('<span>' + this.paths[len - 1] + '</span>');
            $('#divNavPath').html(htm.join('')).show();
        } else {
            $('#divNavPath').html('');
        }
    }
}, FileCommand = {
    //列表
    list: function () {
        window.loading = true;
        $('#sploading').show();
        $.ajax({
            url: "/Store/QQAPI/list",
            type: 'post',
            data: {
                keywords: $('#txtSearch').val(),
                bucket: pObject.bucket,
                path: pObject.path(),
                __RequestVerificationToken: pObject.keys.VAFT
            },
            dataType: 'json',
            success: function (data) {
                var htm = [], htm_folder = [], htm_file = [], i = 0, len = data.code;
                if (len == 0) {
                    data = data.data.infos;
                    len = data.length;
                    if (len) {
                        //排序
                        data.sort(function (a, b) {
                            return a.name.toLowerCase() > b.name.toLowerCase();
                        });

                        htm.push('<table class="table tbList">');
                        for (; i < len;) {
                            var item = data[i++], name = item.name, date = item.ctime, size = '-';
                            date = FormatTime("yyyy-MM-dd HH:mm", date * 1000);

                            //文件
                            if ("filesize" in item) {
                                size = ConvertSize(item.filesize);
                                var originurl = item.source_url.replace("http://", "https://");
                                var url = "http://cos.netnr.top" + originurl.substring(originurl.indexOf('/', 10));
                                htm_file.push('<tr>'
                                    + '<td style="width:25px"><input type="checkbox" name="ChkList" /></td>'
                                    + '<td class="item-tool"><i class="fa fa-file text-muted"></i> &nbsp;<a href="javascript:void(0);" class="it-name" data-mime="file" data-url="' + url + '" data-originurl="' + originurl + '" >' + name + '</a>'
                                    + '<div class="it-control"><a href="' + url + '" class="ic-down" target="_blank">下载</a><a href="javascript:void(0)" class="ic-del fa fa-remove"></a></div></td>'
                                    + '<td class="text-muted">' + size + '</td>'
                                    + '<td class="d-none d-sm-block text-muted text-right">' + date + '</td>'
                                    + '</tr>');
                            } else {
                                htm_folder.push('<tr>'
                                    + '<td style="width:25px"><input type="checkbox" name="ChkList" /></td>'
                                    + '<td class="item-tool"><i class="fa fa-folder text-primary"></i> &nbsp;<a href="javascript:void(0);" class="it-name text-primary" data-mime="folder">' + name.substring(0, name.length - 1) + '</a>'
                                    + '<div class="it-control"><a href="javascript:void(0)" class="ic-del fa fa-remove"></a></div></td>'
                                    + '<td class="text-muted">' + size + '</td>'
                                    + '<td class="d-none d-sm-block text-muted text-right">' + date + '</td>'
                                    + '</tr>');
                            }
                        }
                        htm.push(htm_folder.join('') + htm_file.join(''));
                        htm.push('</table>');
                    } else {
                        htm.push('<div class="h3 text-center">咣 ~</div>');
                    }

                    pObject.pathView();
                } else {
                    htm.push('<div class="h3 text-center">咣 ~</div>');
                }
                $('#divBucket').html(htm.join(''));

                AutoHeight();
            },
            error: function () {
                jz.msg("网络错误")
            },
            complete: function () {
                window.loading = false;
                $('#sploading').hide();
                localStorage["store_qq_path"] = pObject.paths.join(',');
            }
        });
    },
    //存在
    exists: function (name, callback) {
        $.ajax({
            url: "/Store/QQAPI/exists",
            type: 'post',
            data: {
                __RequestVerificationToken: pObject.keys.VAFT,
                key: name,
                path: pObject.path(),
                bucket: pObject.bucket
            },
            success: function (data) {
                if (data == "0") {
                    callback(0);
                } else if (data == "1") {
                    jz.confirm({
                        title: '文件已经存在',
                        content: name,
                        okValue: '继续上传并覆盖',
                        ok: function () {
                            callback(1);
                        }
                    });
                }
            },
            error: function () {
                callback(0);
            }
        })
    },
    //上传
    upload: function (file) {
        file && this.exists(file.name, function (rt) {
            var path = pObject.path();
            if (path != "/") {
                path += "/";
            }

            //上传 ok、error、progress、bucket、path、file、cover
            CC().sliceUploadFile(function () {
                $('#divProgress').hide();
                FileCommand.list();
                jz.msg("上传已完成");
                $('#' + pObject.opt.fileInputID).val('');
                document.title = document.otitle;
            }, function (result) {
                result = result || {};
                $('#divProgress').hide();
                jz.confirm({
                    title: '上传错误',
                    content: result.responseText || '上传失败',
                    cancel: false
                });
                $('#' + pObject.opt.fileInputID).val('');
                document.title = document.otitle;
            }, function (ps) {
                if (!document.otitle) {
                    document.otitle = document.title;
                }
                document.title = (ps * 100).toFixed(2) + "%";
                $('#divProgress').show().css('width', (ps * 100) + "%");
            }, pObject.bucket, path + file.name, file, rt == 1 ? 0 : 1);
        });
    },
    //删除
    del: function (files, folder) {
        $.ajax({
            url: "/Store/QQAPI/del",
            type: 'post',
            data: {
                __RequestVerificationToken: pObject.keys.VAFT,
                keywords: location.hash,
                bucket: pObject.bucket,
                path: pObject.path(),
                files: files.join(','),
                folder: folder.join(',')
            },
            success: function (data) {
                if (data == "success") {
                    FileCommand.list();
                    jz.msg("操作成功");
                } else if (data == "notallow") {
                    jz.msg("你无权限操作");
                } else {
                    FileCommand.list();
                    jz.msg("删除失败（或文件夹有文件，不能删除）");
                }
            }
        })
    },
    //新建文件夹
    newfolder: function (name, callback) {
        $.ajax({
            url: "/Store/QQAPI/newfolder",
            type: 'post',
            data: {
                __RequestVerificationToken: pObject.keys.VAFT,
                folder: name,
                path: pObject.path(),
                bucket: pObject.bucket
            },
            dataType: 'json',
            success: function (data) {
                callback(data.code == "0" ? 1 : 0);
            },
            error: function () {
                callback(0);
            }
        })
    },
}

//上传SDK
function CC() {
    return new CosCloud({
        appid: 1251421615,
        bucket: pObject.bucket,
        region: 'tj',
        getAppSign: function (callback) {
            var that1 = this;
            $.ajax({
                url: "/Store/QQSignature",
                type: 'post',
                data: {
                    __RequestVerificationToken: pObject.keys.VAFT,
                    bucket: pObject.bucket
                },
                success: function (data) {
                    callback(data);
                }
            });
        },
        getAppSignOnce: function (callback) {
            var that2 = this;
            $.ajax({
                url: "/Store/QQSignatureOnce",
                type: 'post',
                data: {
                    __RequestVerificationToken: pObject.keys.VAFT,
                    bucket: pObject.bucket,
                    path: pObject.path()
                },
                success: function (data) {
                    callback(data);
                }
            });
        }
    });
}

//删除
$('#btnDel').click(function () {
    var files = [], folder = [], names = [];
    $('#divBucket').find('input[type="checkbox"]:checked').each(function () {
        var ja = $(this).parent().parent().find('a').first();
        if (ja.attr('data-mime') == "file") {
            files.push(ja.text());
        } else {
            folder.push(ja.text());
        }
        names.push(ja.text());
    });
    if (names.length) {
        jz.confirm({
            title: '删除以下列表',
            content: names.join('<br/>'),
            ok: function () {
                FileCommand.del(files, folder);
            }
        })
    } else {
        jz.msg("请选择删除的文件");
    }
});

//新建文件夹
$('#btnNewFolder').click(function () {
    var jnf = jz.confirm({
        title: '新建文件夹',
        content: '<input class="form-control" placeholder="数字、英文及下划线" />',
        ok: function () {
            vv();
            return false;
        }
    }), txt = $(jnf).find('input');
    txt.keydown(function (e) {
        e = e || window.event;
        if ((e.keyCode || e.which || e.charCode) == 13) {
            vv();
        }
    });
    txt[0].focus();
    function vv() {
        if ($.trim(txt.val()) == "") {
            jz.tip({
                target: txt[0],
                content: "请输入文件夹",
                align: "left",
                focus: true,
                blank: true,
                time: 4
            });
        } else {
            FileCommand.newfolder(txt.val(), function (rt) {
                if (rt == 1) {
                    FileCommand.list();
                    jnf.fn.remove();
                } else {
                    jz.msg("操作失败");
                }
            });
        }
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
            var mime = target.getAttribute('data-mime');
            if (mime == "file") {
                ViewFileInfo($(target).text(), target.getAttribute('data-url'), target.getAttribute('data-originurl'));
            } else if (mime == "folder" && window.loading == false) {
                pObject.paths.push($(target).text());
                FileCommand.list();
            }
        } else if (target.className.indexOf('ic-del') >= 0) {
            var ja = $(target).parent().prev(), name = ja.text(), mime = ja.attr('data-mime');
            jz.confirm({
                title: '是否删除文件' + (mime == "file" ? "" : "夹"),
                content: '<span style="color:#3B8CFF">' + name + '</span>',
                ok: function () {
                    var files = [], folder = [];
                    if (mime == "file") {
                        files.push(name);
                    } else {
                        folder.push(name);
                    }
                    FileCommand.del(files, folder);
                }
            });
        }
    }
});

//点击路径
$('#divNavPath').click(function (e) {
    e = e || window.event;
    var target = e.target || e.srcElement;
    if (target.nodeName == "A") {
        //上一级
        if (target.getAttribute('data-cmd') == "upper") {
            pObject.paths = pObject.paths.slice(0, pObject.paths.length - 1);
            FileCommand.list();
        } else {
            var cmd = target.innerHTML, nps = [];
            $('#divNavPath').find('a').each(function (k, v) {
                k && nps.push($.trim($(this).text()));
                if (target == this) {
                    return false;
                }
            });
            pObject.paths = nps;
            FileCommand.list();
        }
    }
});

//退格
$(window).keydown(function (e) {
    e = e || window.event;
    if ((e.keyCode || e.which || e.charCode) == 8) {
        if (document.activeElement.nodeName != "INPUT") {
            stopDefault(e);
            pObject.paths = pObject.paths.slice(0, pObject.paths.length - 1);
            FileCommand.list();
        }
    }
})

//记录路径
var sqp = localStorage["store_qq_path"] || "";
if (sqp != "") {
    pObject.paths = sqp.split(',')
}