loadCatalog();
function loadCatalog() {
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

            var tree = (function (json, deep, ptitle, ptree) {
                var arr = [], deep = deep || 0, ptitle = ptitle || [], ptree = ptree || [];
                for (var i = 0; i < json.length; i++) {
                    var ji = json[i], child = ji.children;
                    if (!ji.IsCatalog) {
                        continue
                    }
                    ptree.push(i);
                    arr.push({
                        DsdId: ji.DsdId,
                        DsdPid: ji.DsdPid,
                        DsdOrder: ji.DsdOrder,
                        CurrTitle: ji.DsdTitle,
                        DsdTitle: ptitle.join('') + ji.DsdTitle,
                        TreeTag: ptree.join('.')
                    });
                    ptitle.push(" — ");
                    if (child) {
                        deep += 1;
                        var arrc = arguments.callee(child, deep, ptitle, ptree);
                        if (arrc.length) {
                            arr = arr.concat(arrc);
                        }
                        deep -= 1;
                    }
                    ptitle.length = deep;
                    ptree.length = deep;
                }
                return arr;
            })(data) || [];

            var htm = [];
            $.each(tree, function (k, v) {
                htm.push('<tr>');
                htm.push('<td>' + v.DsdTitle + '</td>');
                htm.push('<td>' + v.DsdOrder + '</td>');
                htm.push('<td data-id="' + v.DsdId + '" data-pid="' + v.DsdPid + '" data-title="' + v.CurrTitle + '" data-tree="' + v.TreeTag + '">');
                htm.push('<a href="javascript:void(0)" data-cmd="edit" class="mr-3">编辑</a>');
                htm.push('<a href="javascript:void(0)" data-cmd="del" >删除</a>');
                htm.push('</td>');
                htm.push('</tr>');
            });
            $('#tlist').html(htm.join(''));
        },
        error: function () {
            alert('error');
        }
    })
}

var MenuTree = [];
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

            MenuTree = (function (json, deep, ptitle, ptree) {
                var arr = [], deep = deep || 0, ptitle = ptitle || [], ptree = ptree || [];
                for (var i = 0; i < json.length; i++) {
                    var ji = json[i], child = ji.children;
                    if (!ji.IsCatalog) {
                        continue
                    }
                    ptitle.push(ji.DsdTitle);
                    ptree.push(i);
                    arr.push({
                        DsdId: ji.DsdId,
                        DsdTitle: ptitle.join(' / '),
                        TreeTag: ptree.join('.')
                    });
                    if (child) {
                        deep += 1;
                        var arrc = arguments.callee(child, deep, ptitle, ptree);
                        if (arrc.length) {
                            arr = arr.concat(arrc);
                        }
                        deep -= 1;
                    }
                    ptitle.length = deep;
                    ptree.length = deep;
                }
                return arr;
            })(data) || [];

            var dmarr = [], dv = '';
            dmarr.push('<a class="dropdown-item" data-value="" >无</a>');
            $.each(MenuTree, function (i, v) {
                dmarr.push('<a class="dropdown-item" data-tree="' + this.TreeTag + '" data-value="' + this.DsdId + '" >' + this.DsdTitle + '</a>');
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
                        var isloop = false;
                        if ($('#txtDsdPid').attr('data-type') == "edit") {
                            var currtag = ($('#DsdPid').attr('data-tree') || '').split('.');
                            var treetag = (target.getAttribute('data-tree') || '').split('.');

                            if (treetag.length > currtag.length) {
                                treetag.length = currtag.length;
                                if (treetag.join('') == currtag.join('')) {
                                    isloop = true;
                                    jz.msg("不能循环引用");
                                }
                            }
                        }
                        if (!isloop) {
                            $(that).prev().val(key);
                            ddi.value = target.innerHTML;
                            $(this).find('a').removeClass('text-primary');
                            $(target).addClass('text-primary');
                        }
                    } else {
                        jz.msg("不能选择自己");
                    }
                }
            });
        }, 10)
    }
});

//添加目录
function AddCatalog() {
    $('#txtDsdPid').attr('data-type', 'add');
    $('#DsdPid').attr('data-tree', '').val('');
    $('#DsdId').val('');
    $('#mbform')[0].reset();
    $('#myModalLabel_1').html('添加目录');
    $('#myModal_1').modal();
}

//点击行
$('#tlist').click(function (e) {
    e = e || window.event;
    var target = e.target || e.srcElement;
    if (target.nodeName == "A") {
        var col3 = $(target).parent();
        switch (target.getAttribute('data-cmd')) {
            case 'edit':
                {
                    $('#txtDsdPid').attr('data-type', 'edit');

                    $('#myModalLabel_1').html('编辑目录');
                    $('#myModal_1').modal();
                    $('#DsdId').val(col3.attr('data-id'));
                    $('#DsdTitle').val(col3.attr('data-title'));

                    var pid = col3.attr('data-pid');
                    $('#DsdPid').val(pid);
                    var das = $('#dmbox').find('a');
                    das.removeClass('text-primary');
                    das.filter('[data-value="' + pid + '"]').addClass('text-primary')

                    $('#txtDsdPid').val('无');
                    $.each(MenuTree, function () {
                        if (this.DsdId == pid) {
                            $('#txtDsdPid').val(this.DsdTitle);
                            $('#DsdPid').attr('data-tree', col3.attr('data-tree'));
                            return false;
                        }
                    })

                    $('#DsdOrder').val(col3.prev().html().trim());
                }
                break;
            case 'del':
                jz.confirm('确定删除（包括子目录）？', {
                    ok: function () {
                        $.ajax({
                            url: "/doc/code/DelCatalog?id=" + col3.attr('data-id'),
                            data: {
                                code: $('#DsCode').val()
                            },
                            dataType: 'json',
                            success: function (data) {
                                if (data.code == 200) {
                                    jz.msg("操作成功");
                                    loadCatalog();
                                    loadMenuTree();
                                }
                            }
                        })
                    }
                })
                break;
        }
    }
});

//保存
$('#btnSave_1').click(function () {
    if ($('#DsdTitle').val() == "") {
        jz.alert("标题 必填");
    } else {
        $.ajax({
            url: "/doc/code/SaveCatalog",
            data: $('#mbform').serialize(),
            dataType: 'json',
            success: function (data) {
                if (data.code == 200) {
                    $('#myModal_1').modal('hide');
                    loadCatalog();
                    loadMenuTree();
                } else {
                    jz.alert(data.msg)
                }
            }
        })
    }
});