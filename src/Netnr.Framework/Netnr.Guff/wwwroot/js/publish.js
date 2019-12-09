$('div[data-toggle="tooltip"]').tooltip();

var gp = {
    init: function () {
        var did = guff.getUrlParam('id', location.hash);
        if (did && did.length == 19) {
            gp.detail(did);
        } else {
            $('input,textarea').val('');
        }
    },
    /**
     * 分类
     */
    types: ['Image', 'Audio', 'Video'],
    /**
     * 添加文件
     * @param {any} type 分类
     * @param {any} values 值，逗号分割
     */
    addFile: function (type, values) {
        var hidv = $('input[name="Gr' + type + '"]'), box = hidv.prev();
        if (hidv.val() == "") {
            hidv.val(values);
        } else {
            hidv.val(hidv.val() + "," + values);
        }

        values.split(',').forEach(v => {
            if (v != "") {
                var bi = document.createElement('div'), ics = [];
                bi.className = "rounded";
                switch (type) {
                    case "Image":
                        ics.push('<img data-src="' + v + '" src="' + v + '" onerror="this.remove()" title="图片 ' + v + '" />');
                        break;
                    case "Audio":
                        ics.push('<i data-src="' + v + '" class="fa fa-fw fa-lg fa-play-circle" title="音频 ' + v + '"></i>');
                        break;
                    case "Video":
                        ics.push('<i data-src="' + v + '" class="fa fa-fw fa-lg fa-youtube-play" title="视频 ' + v + '"></i>');
                        break;
                }
                ics.push('<span class="fa fa-remove" title="删除"></span>')
                bi.innerHTML = ics.join('');
                $(bi).find('span').click(function () {
                    jz.confirm('确定删除？', {
                        ok: function () {
                            var src = $(bi).children().first().attr('data-src');
                            var hidv = $('input[name="Gr' + type + '"]');
                            var values = hidv.val().split(',');
                            values.splice(values.indexOf(src), 1);
                            hidv.val(values.join(','));

                            $(bi).remove();
                        }
                    })
                })
                box.append(bi);
            }
        })
    },
    /**
     * 获取文件
     * @param {any} type 分类
     */
    getFiles: function (type) {
        var hidv = $('input[name="Gr' + type + '"]'), box = hidv.prev(), values = [];
        box.find('*[data-src]').each(function () {
            values.push(this.getAttribute('data-src'));
        });
        return values.join(',');
    },
    /**
     * 一条详情
     * @param {any} id
     */
    detail: function (id) {
        guff.fetch(guff.host + 'api/v1/guff/detail?id=' + encodeURIComponent(id)).then(x => x.json()).then(res => {
            if (res.code == 200) {
                gp.id = id;
                for (var i in res.data) {
                    $('input[name="' + i + '"]').val(res.data[i]);
                    $('textarea[name="' + i + '"]').val(res.data[i]);
                }
                gp.types.forEach(f => {
                    //先清空值再赋值
                    $('input[name="Gr' + f + '"]').val('');
                    gp.addFile(f, res.data['Gr' + f] || '');
                })
            } else {
                gp.id = null;
                $('input,textarea').val('');
                jz.alert(res.msg);
            }
        }).catch(err => {
            console.log(err);
            gp.id = null;
            jz.alert('网络错误')
        });
    }
}

gp.types.forEach(t => {
    //上传
    $('#btnUp' + t).change(function () {
        var file = this.files[0];
        if (file) {
            if (t == "Video") {
                jz.alert('抱歉，暂不支持视频上传<br/>请用链接的方式吧');
                return false;
            }

            var err = [];
            var ms = t == "Video" ? 10 : 2;
            if (file.size > 1024 * 1024 * ms) {
                err.push('文件大小限制 ' + ms + 'M');
            }
            if (file.type.indexOf(t.toLowerCase() + "/") != 0) {
                err.push('请选择对应的文件');
            }
            if (err.length) {
                jz.alert(err.join('<br/>'));
            } else {
                $('#loading' + t).removeClass('d-none');

                //上传
                var fd = new FormData();
                fd.append('file', file);
                fd.append('name', file.name);

                var xhr = new XMLHttpRequest();
                xhr.upload.onprogress = function (event) {
                    if (event.lengthComputable) {
                        //上传百分比
                        var per = (event.loaded / event.total) * 100;
                        per = per.toFixed(2) + " %";
                        $('#loading' + t)[0].title = "已上传 " + per;
                        console.log(per);
                    }
                };

                xhr.open("post", "https://gs.zme.ink/big/upload.php", true);
                xhr.setRequestHeader("X-Requested-With", "XMLHttpRequest");
                xhr.send(fd);
                xhr.onreadystatechange = function (e) {
                    if (xhr.readyState == 4) {
                        $('#loading' + t).addClass('d-none');

                        if (xhr.status == 200) {
                            var jv = JSON.parse(xhr.responseText);
                            if (jv.code == 2) {
                                gp.addFile(t, 'https://gs.zme.ink' + jv.url);
                            } else {
                                jz.alert('上传失败');
                            }
                        } else {
                            jz.alert('上传失败');
                        }
                    }
                }
            }
        }
    });

    //直接链接
    $('#btnOne' + t).click(function () {
        var tn = $('#txtOne' + t), err = [];
        if (tn.val() == "") {
            err.push('链接不能为空');
        } else if (tn.val().toLowerCase().indexOf('http') != 0) {
            err.push('链接请以 http 开头');
        }
        if (err.length) {
            jz.alert(err.join('<br/>'));
        } else {
            gp.addFile(t, tn.val());
        }
        tn.val('');
    });
});

guff.muInput($('input[name="GrTag"]')[0]);
guff.muInput($('input[name="GrObject"]')[0]);

//保存
$('#btnSave').click(function () {
    var err = [];

    if ($('textarea[name="GrContent"]').val() == "" && $('input[name="GrImage"]').val() == "" && $('input[name="GrAudio"]').val() == "" && $('input[name="GrVideo"]').val() == "") {
        err.push("内容不能为空（内容、图片、音频、视频 至少有一项有内容）");
    }

    if ($('input[name="GrTag"]').val() == "") {
        err.push("标签不能为空");
    }

    if (err.length) {
        jz.alert(err.join('<hr />'));
        return false;
    }

    $('#btnSave')[0].disabled = true;

    var uri = guff.host + 'api/v1/guff/' + (gp.id ? 'update' : 'add');
    $('input[name="GrId"]').val(gp.id || '');

    guff.fetch(uri, $('#pform').serialize(), "post").then(x => x.json()).then(res => {
        $('#btnSave')[0].disabled = false;
        if (res.code == 200) {
            location.href = guff.loginLink().replace('login', 'detail#id=' + res.data);
        } else {
            jz.alert(res.msg);
        }
    }).catch(err => {
        $('#btnSave')[0].disabled = false;
        console.log(err);
        jz.alert('网络错误')
    });
});

window.onhashchange = function () {
    gp.init();
}

gp.init();