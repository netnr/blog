var pObject = {
    bucket: "netnr",
    opt: {
        fileInputID: "fileInput",
        //错误
        onError: function (errObj) {
            $('#divProgress').hide();
            jz.confirm({
                title: '上传错误',
                content: errObj.errMsg,
                cancel: false
            });
            $('#' + pObject.opt.fileInputID).val('');
            document.title = document.otitle;
        },
        //进度
        onProgress: function (ps) {
            if (ps.status == 1) {
                if (!document.otitle) {
                    document.otitle = document.title;
                }
                document.title = parseFloat(ps.progress).toFixed(2) + "%";
                $('#divProgress').show().css('width', ps.progress + "%");
            } else {
                document.title = document.otitle;
            }
        }
    },
    upParam: {
        bucketName: '',
        objectName: '',
        token: '',
        trunkSize: 128 * 1024
    },
    keys: {
        VAFT: $('input[name="__RequestVerificationToken"]').val(),
        domain: function () { return 'http://nos.netnr.top/' },
        domainOrigin: function () { return 'https://' + pObject.bucket + '.nos-eastchina1.126.net/' },
        AccessKey: '982d0be641b64a49b3b9e5cd20a6c6e4',
        SecretKey: '66a1c251f8de4ce1a082ce42aa00e148'
    },
    token: function (filename) {
        var tp = new Date();
        tp = tp.setDate(tp.getDate() + 1);
        tp = Number(tp.toString().substr(0, 10));
        var vu = { "Bucket": pObject.bucket, "Object": filename, "Expires": tp },
            encodedPutPolicy = CryptoJS.enc.Base64.stringify(CryptoJS.enc.Utf8.parse(JSON.stringify(vu))),
            sign = CryptoJS.HmacSHA256(encodedPutPolicy, this.keys.SecretKey),
            encodedSign = CryptoJS.enc.Base64.stringify(sign);
        return "UPLOAD " + this.keys.AccessKey + ":" + encodedSign + ":" + encodedPutPolicy;

    }
}, FileCommand = {
    //列表
    list: function () {
        $('#sploading').show();
        $.ajax({
            url: "/Store/NEAPI/list",
            type: 'post',
            data: {
                keywords: $('#txtSearch').val(),
                bucket: pObject.bucket,
                __RequestVerificationToken: pObject.keys.VAFT
            },
            dataType: 'json',
            success: function (data) {
                var htm = [], i = 0, len = data.length;
                if (len) {
                    //排序
                    data.sort(function (a, b) {
                        return a.Key.toLowerCase() > b.Key.toLowerCase();
                    });

                    htm.push('<table class="table tbList">');
                    for (; i < len;) {
                        var item = data[i++], name = item.Key, size = ConvertSize(item.Size), date = item.LastModified;
                        date = date.substring(0, 16).replace('T', ' ');
                        htm.push('<tr>'
                            + '<td style="width:25px"><input type="checkbox" name="ChkList" /></td>'
                            + '<td class="item-tool"><i class="fa fa-file text-muted"></i> &nbsp;<a href="javascript:void(0);" class="it-name">' + name + '</a>'
                            + '<div class="it-control"><a href="' + (pObject.keys.domain() + name) + '" class="ic-down" target="_blank">下载</a><a href="javascript:void(0)" class="ic-del fa fa-remove"></a></div></td>'
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
    //存在
    exists: function (name, callback) {
        $.ajax({
            url: "/Store/NEAPI/exists",
            type: 'post',
            data: {
                __RequestVerificationToken: pObject.keys.VAFT,
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
    //上传
    upload: function (file) {
        file && this.exists(file.name, function () {
            FileCommand.upObj = Uploader(pObject.opt);
            FileCommand.upObj.addFile(file);
            pObject.upParam.objectName = file.name;
            pObject.upParam.bucketName = pObject.bucket;
            pObject.upParam.token = pObject.token(file.name);
            FileCommand.upObj.upload(pObject.upParam, function (ps) {
                $('#divProgress').hide();
                if (ps.status == 2) {
                    FileCommand.list();
                    jz.msg("上传已完成");
                    $('#' + pObject.opt.fileInputID).val('');
                }
            });
        });
    },
    //删除
    del: function (names) {
        $.ajax({
            url: "/Store/NEAPI/del",
            type: 'post',
            data: {
                __RequestVerificationToken: pObject.keys.VAFT,
                keywords: location.hash,
                bucket: pObject.bucket,
                key: names.join(',')
            },
            success: function (data) {
                if (data == "success") {
                    FileCommand.list();
                    jz.msg("操作成功");
                } else {
                    jz.msg("操作失败");
                }
            }
        })
    }
}

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
    var target = e.target || e.srcElement;
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
            var path = $(target).text();
            ViewFileInfo($(target).text(), pObject.keys.domain() + path, pObject.keys.domainOrigin() + path);
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