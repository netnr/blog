//初始化MarkDown
require(['vs/editor/editor.main'], function () {
    window.nmd = new netnrmd('#editor', {
        storekey: "md_autosave_" + location.pathname.replace("/", "").toLowerCase()
    });

    nmd.setmd(nmd.obj.mebox.attr('data-value'));

    $(window).on('load resize', function () {
        var vh = $(window).height() - nmd.obj.container.offset().top - 15;
        nmd.height(Math.max(100, vh));
    })

    //快捷键
    nmd.obj.me.addCommand(monaco.KeyMod.CtrlCmd | monaco.KeyCode.KEY_S, function () {
        $('#btnSave')[0].click();
    })
});

loadMenuTree();
function loadMenuTree() {
    $.ajax({
        url: "/doc/code/menutree/" + $('#DsCode').val(),
        dataType: 'json',
        success: function (data) {
            if (data.code == 200) {
                data = data.data;
            } else if (data.code == 94) {
                data = [];
            } else {
                jz.msg('fail');
                return false;
            }

            var tree = (function (json, deep, ptitle) {
                var arr = [], deep = deep || 0, ptitle = ptitle || [];
                for (var i = 0; i < json.length; i++) {
                    var ji = json[i], child = ji.children;
                    if (!ji.IsCatalog) {
                        continue
                    }
                    var obj = {};
                    ptitle.push(ji.DsdTitle);
                    obj[ji.DsdId] = ptitle.join(' / ');
                    arr.push(obj);
                    if (child) {
                        deep += 1;
                        var arrc = arguments.callee(child, deep, ptitle);
                        if (arrc.length) {
                            arr = arr.concat(arrc);
                        }
                        deep -= 1;
                    }
                    ptitle.length = deep;
                }
                return arr;
            })(data) || [];

            var dmarr = [], dv = '', dvkey = $('#txtDsdPid').prev().val();
            dmarr.push('<a class="dropdown-item" data-value="" >无</a>');
            $.each(tree, function (i, v) {
                for (var j in v) {
                    var ac = '';
                    if (dvkey == j) {
                        dv = v[j];
                        ac = 'text-primary';
                    }
                    dmarr.push('<a class="dropdown-item ' + ac + '" data-value="' + j + '" >' + v[j] + '</a>');

                }
            });
            $('#txtDsdPid').val(dv);
            $('#dmbox').html(dmarr.join(''));
        },
        error: function () {
            jz.alert('error')
        }
    })
}
//点击目录
$('#txtDsdPid').click(function (e) {
    var that = this;
    if (!that.dropdownbind) {
        that.dropdownbind = 1;
        setTimeout(function () {
            var dd = $(that).data('bs.dropdown'),
                ddi = dd["_element"], ddm = dd["_menu"];
            ddi.readOnly = true;
            $(ddm).click(function (e) {
                e = e || window.event;
                var target = e.target || e.srcElement;
                if (target.nodeName == "A") {
                    var key = target.getAttribute('data-value');
                    if (key == "" || $('#DsdId').val() != key) {
                        $(that).prev().val(key);
                        ddi.value = target.innerHTML;
                        $(this).find('a').removeClass('text-primary');
                        $(target).addClass('text-primary');
                    }
                }
            });
        }, 10)
    }
});

//保存
$('#btnSave').click(function () {
    var err = [];
    if ($('#DsdTitle').val().trim() == "") {
        err.push('标题 必填')
    }
    if (nmd.getmd().trim() == "") {
        err.push("内容 必填");
    }
    if (err.length) {
        jz.alert(err.join('</br>'));
        return false;
    }

    var post = {};
    var sbox = $('#savebox');
    sbox.find('input').each(function () {
        post[this.name] = this.value;
    });

    post["DsdContentMd"] = nmd.getmd();
    post["DsdContentHtml"] = nmd.gethtml();

    $('#btnSave')[0].disabled = true;
    $.ajax({
        url: "/doc/code/save/" + $('#DsCode').val(),
        type: 'post',
        data: post,
        dataType: 'json',
        success: function (data) {
            jz.alert(data.msg);
            if (data.code == 200) {
                $('#DsdId').val(data.data);
            }
        },
        error: function () {
            jz.alert('error')
        },
        complete: function () {
            $('#btnSave')[0].disabled = false;
        }
    })
});

//模版命令
$('#btnbox').click(function (e) {
    e = e || window.event;
    var target = e.target || e.srcElement;
    if (target.nodeName == "BUTTON") {
        var md = target.getAttribute('data-md');
        if (md != null && md != "") {
            InsertTemplateMd(md);
        }
    }
});
//插入模版
function InsertTemplateMd(md) {
    var docstop = $(document).scrollTop();
    $.ajax({
        url: "/areas/doc/template/" + md + ".md?v1",
        success: function (data) {
            netnrmd.insertAfterText(nmd.obj.me, data);
        },
        complete: function () {
            window.scrollTo(0, docstop);
        }
    })
}