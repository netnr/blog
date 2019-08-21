/*  tag  *\


\*       */

(function (window, undefined) {

    //t：对象，可以是 #id | .class | Element | nodeName （类似于jQuery选择器）
    //ops：配置
    var tag = function (t, ops) { return new tag.fn.init(t, ops) };

    //版本号
    tag.version = "1.0.0";

    /* IE8- */
    tag.OldIE = function () { return typeof document.createElement == "object" || false };

    //字符串两边去空 ie8-
    if (tag.OldIE()) { String.prototype.trim = function () { return this.replace(/(^\s+)|(\s+$)/g, "") } };

    tag.fn = tag.prototype = {
        init: function (t, ops) {
            if (t == undefined) {
                return null;
            }

            if (typeof t === "string") {
                var that;
                if (t[0] == "#") {
                    //#id
                    that = document.getElementById(t.substring(1));
                    if (that) {
                        this[0] = that;
                        this.length = 1;
                    }
                } else if (t[0] == ".") {
                    //.class
                    var className = t.substring(1).trim(), rt = [];
                    if (tag.OldIE()) {
                        var tns = document.getElementsByTagName("*");
                        tag.each(tns, function () {
                            var cns = this.className.split(' ');
                            tag.each(cns, function () {
                                if (this.trim() == className) {
                                    rt.push(this);
                                    return false;
                                }
                            });
                        });
                    } else {
                        rt = document.getElementsByClassName(className);
                    }
                    var i = 0, len = rt.length;
                    for (; i < len;) {
                        this[i] = rt[i++];
                    }
                    this.length = len;
                } else {
                    //nodeName
                    that = document.getElementsByTagName(t);
                    var i = 0, len = that.length;
                    for (; i < len;) {
                        this[i] = rt[i++];
                    }
                    this.length = len;
                }
            } else if (typeof t === "object") {
                if (t.nodeType == 1) {
                    //Element
                    this[0] = t;
                    this.length = 1;
                } else if (typeof t.OldIE === "function") {
                    //self
                    var i = 0, len = t.length;
                    for (; i < len;) {
                        this[i] = t[i++];
                    }
                    this.length = len;
                }
            }

            if (this.length) {

            }

            return this;
        },
        length: 0,
        //tag配置
        config: {
            //列表行
            limitRow: 7,
            //允许选中数量
            limitSelected: 3,
            //远程源（url、data选一）
            url: null,
            //本地源（url、data选一）
            data: null,
            //是否加载中（url有效）
            isLoad: false,
            //延迟加载（单位：ms）（url有效）
            delay: 500,
            //启用上下切换选中行
            supportKeyUpDown: true,
            //启用退格键删除标签
            supportKeyBackspace: true,

            //列表容器对象
            boxList: null
        },
        //事件添加处理程序
        on: function (type, callback) {
            tag.each(this, function () {
                tag.on(type, callback, this);
            });
        },
        //事件移除处理程序
        off: function (type, callback) {
            tag.each(this, function () {
                tag.off(type, callback, this);
            });
        },
        //文本值改变事件
        input: function (fn) {
            if (tag.OldIE()) {
                tag.each(this, function () {
                    if (this.nodeType == 1) {
                        var that = this;
                        this.attachEvent("onpropertychange", function () {
                            if (event.propertyName.toLowerCase() == "value") {
                                fn.apply(that, arguments);
                            }
                        });
                    }
                });
            } else {
                tag.each(this, function () {
                    this.nodeType == 1 && tag.on("input", fn, this);
                });
            }
            return this;
        },
        //绑定
        bind: function () {

        }
    };

    tag.fn.init.prototype = tag.prototype;

    //事件添加处理程序
    tag.on = function (type, callback, obj) {
        if (obj.addEventListener) {
            obj.addEventListener(type, callback, false);
        } else if (obj.attachEvent) {
            obj['e' + type + callback] = callback;
            obj[type + callback] = function () { obj['e' + type + callback]() }
            obj.attachEvent("on" + type, obj[type + callback]);
        } else {
            obj["on" + type] = callback
        }
    };

    //移除事件的处理程序
    tag.off = function (type, callback, obj) {
        if (obj.removeEventListener) {
            obj.removeEventListener(type, callback, false);
        } else if (obj.detachEvent) {
            obj.detachEvent("on" + type, obj[type + callback]); obj[type + callback] = null
        }
    };

    //遍历
    tag.each = function (obj, fn) {
        //this指向obj[i]; i是第一个参数 obj[i]第二个 ...
        var i = 0, len = obj.length;
        for (; i < len; i++) {
            if (fn.call(obj[i], i, obj[i]) == false) { break }
        }
    };

    //检索一个节点某个方向的节点 dir可选值：parentNode nextSibling previousSibling
    tag.dir = function (t, dir) {
        var match = [], cur = t[dir];
        while (cur && cur.nodeType != 9) {
            cur.nodeType == 1 && match.push(cur);
            cur = cur[dir];
        }
        return match;
    }

    //添加处理事件
    tag.each(("blur focus focusin focusout load resize scroll unload click dblclick "
        + "mousedown mouseup mousemove mouseover mouseout mouseenter mouseleave "
        + "change select submit keydown keypress keyup error contextmenu").split(" ")
        , function (i, name) {
            tag.fn[name] = function (fn) {
                tag.each(this, function () { tag.on(name, fn, this); });
                return this
            }
        });

    //阻止事件冒泡
    tag.stopEvent = function (e) { if (e && e.stopPropagation) { e.stopPropagation() } else { window.event.cancelBubble = true } };

    //阻止浏览器默认行为
    tag.stopDefault = function (e) { if (e && e.preventDefault) { e.preventDefault() } else { window.event.returnValue = false } };

    //按键ASCII值
    tag.key = function (e) { e = e || window.event; return e.keyCode || e.which || e.charCode };

    //格式化JSON
    tag.parseJSON = function (s) { return tag.OldIE() ? (new Function("return " + s))() : JSON.parse(s) };

    //判断是键值对
    tag.isKV = function (t) { return t && typeof (t) == "object" && Object.prototype.toString.call(t).toLowerCase() == "[object object]" && !t.length }

    /*
    url:"http://netnr.com?id=id",  //url地址
    type:"post",                  //默认get
    async:true,                 //默认异步
    data:"post提交内容",        //post内容
    dataForm:FormData           //HTML5表单对象 有值时忽略 type、data
    dataType:"json/xml",        //默认文本
    success:function(val){},    //返回内容
    error:function(xhr){}       //错误回调
    complete:function           //完成回调
    */
    tag.ajax = function (ops) {
        var xhr = (window.XMLHttpRequest) ? (new XMLHttpRequest()) : (new ActiveXObject("Microsoft.XMLHTTP")),
            type = ops.type || "get",   //默认GET
            async = ops.async == undefined ? true : ops.async, //默认异步
            data = ops.data || null,  //发送内容
            dataType = ops.dataType || "text";//格式化

        if (tag.isKV(data)) { var d = ""; for (var i in data) { d += ('&' + i + '=' + encodeURIComponent(data[i])); } data = d.substr(1); }

        //状态改变事件
        xhr.onreadystatechange = function () {
            if (xhr.readyState == 4) {
                if (xhr.status == 200) {
                    var val = xhr.responseText;
                    if (typeof ops.success == "function") {
                        switch (dataType) {
                            case 'json':
                                var pj = true;
                                try {
                                    val = tag.parseJSON(val);
                                } catch (e) {
                                    pj = false;
                                    typeof ops.error == "function" && ops.error(xhr);
                                }
                                pj && ops.success(val);
                                break;
                            case 'xml':
                                ops.success(xhr.responseXML);
                                break;
                            default:
                                ops.success(val);
                                break;
                        }
                    }
                }
                else {
                    typeof ops.error == "function" && ops.error(xhr);
                }

                typeof ops.complete == "function" && ops.complete();
            }
        };

        xhr.open(type, ops.url, async);
        type == "post" && xhr.setRequestHeader("Content-Type", "application/x-www-form-urlencoded");
        xhr.send(data);
    };

    window.tag = tag;

})(window, undefined);


$.prototype.init.prototype["input"] = function (callback) {
    if (!-[1,]) {
        $.each(this, function () {
            if (this.nodeType == 1) {
                var that = this;
                this.attachEvent("onpropertychange", function () {
                    if (event.propertyName.toLowerCase() == "value") {
                        callback.apply(that, arguments);
                    }
                });
            }
        });
    } else {
        $.each(this, function () {
            this.nodeType == 1 && $(this).on("input", callback);
        });
    }
    return this;
}


//列表容器
var tags_list_id = 'tags_list_' + (new Date()).valueOf();

//常用容器
var tags_common_id = tags_list_id.replace('list', 'common');
initTags();

//初始化
function initTags() {
    var box = $('#tags').parent();

    //常用容器
    var dcommon = document.createElement('div');
    dcommon.id = tags_common_id;
    dcommon.className = "tb-common";

    //列表容器
    var dlist = document.createElement('div');
    dlist.id = tags_list_id;
    dlist.className = 'tb-list';

    box.append(dcommon).append(dlist);

    keyboardControl();
}
//标签
$('#tags').input(function () {
    loadListTags(this.value);

    return false;
    if ($.trim(this.value) != "") {
        $('#' + tags_common_id).hide();
    } else {
        $('#' + tags_common_id).show();
    }

}).click(function (e) {
    if (e && e.stopPropagation) { e.stopPropagation() } else { window.event.cancelBubble = true }
}).focus(function () {
    createCommonHtml();
});

//阻止冒泡
function stopBubble(e) {
    if (e && e.stopPropagation) { e.stopPropagation() } else { window.event.cancelBubble = true }
}

//列表对象
$('#' + tags_list_id).click(function (e) {
    e = e || window.event;
    var target = e.target || e.srcElement;
    var cli;
    $(this).find('li').each(function () {
        if (this.contains(target)) {
            cli = this;
            return false;
        }
    });
    if (cli && cli.getAttribute('data-id')) {
        var iss = TagsAdd(cli.getAttribute('data-id'), $(cli).text());
        if (!iss) {
            stopBubble(e);
        }
    }
});

//常用对象
$('#' + tags_common_id).click(function (e) {
    e = e || window.event;
    var target = e.target || e.srcElement;
    if (target.nodeName == "LI" && target.getAttribute('data-id')) {
        TagsAdd(target.getAttribute('data-id'), target.innerHTML);
    }
});

//列表定位
function autoListPosition() {
    var pd = $('#' + tags_list_id), ppd = pd.parent().offset(), ppdL = ppd.left, ppdT = ppd.top,
        pt = $('#tags'), ptL = pt.offset().left, ptT = pt.offset().top;
    pd.css('top', ptT - ppdT + pt.outerHeight() + 3).css('left', ptL - ppdL - 6);
}

//常用定位
function autoCommonPosition() {
    $('#' + tags_common_id).css('top', $('#tags').outerHeight() + 3 + 'px');
}

//加载列表数据
function loadListTags(s) {
    if (s == "") {
        $('#' + tags_list_id).hide();
        return false;
    }
    clearTimeout(window.tagDefer);
    window.tagDefer = setTimeout(function () {
        $.ajax({
            url: "/Home/TagSelectSearch",
            type: 'post',
            data: { keys: s },
            dataType: 'json',
            success: function (data) {
                createListHtml(data);
            },
            error: function () {

            }
        })
    }, 500);
}

//呈现搜索视图
function createListHtml(data) {
    if ($.trim($('#tags').val()) == "") {
        return false;
    }
    var html = [];
    html.push('<ul onselectstart="return false;">');
    if (data.length) {
        for (var i = 0; i < data.length; i++) {
            var item = data[i];
            html.push('<li class="' + (i == 0 ? "active" : "") + '" data-id="' + item.TagId + '">' + item.TagName.replace($('#tags').val(), '<strong>' + $('#tags').val() + '</strong>') + '</li>');
        }
    } else {
        html.push('<li class="active">没找到标签 <strong>' + $('#tags').val() + '</strong></li>');
    }
    html.push('</ul>');
    $('#' + tags_list_id).html(html.join(''));
    $('#' + tags_list_id).show();

    autoListPosition();

    keyboardControl();

    clickBlankClose();
}

//按键选择
function keyboardControl() {
    if ($('#tags').attr('data-eventKeydown') != "1") {
        $('#tags').keydown(function (e) {
            e = e || window.event;
            var keys = e.keyCode || e.which || e.charCode;
            if (keys in { "38": 1, "40": 1, "13": 1, "8": 1 }) {
                if (keys != 8) {
                    if (e && e.preventDefault) { e.preventDefault() } else { window.event.returnValue = false }
                }
                var lis = $('#' + tags_list_id).find('li');
                switch (Number(keys)) {
                    //up
                    case 38:
                        if (lis.length > 1 && !lis.first().hasClass('active')) {
                            lis.each(function () {
                                if ($(this).hasClass('active')) {
                                    $(this).removeClass('active').prev().addClass('active');
                                    return false;
                                }
                            });
                        }
                        break;
                    //down
                    case 40:
                        if (lis.length > 1 && !lis.last().hasClass('active')) {
                            lis.each(function () {
                                if ($(this).hasClass('active')) {
                                    $(this).removeClass('active').next().addClass('active');
                                    return false;
                                }
                            });
                        }
                        break;
                    //enter
                    case 13:
                        if (!lis.end().is(":hidden")) {
                            var cli = lis.end().find('li.active');
                            if (cli.length && cli.attr('data-id')) {
                                HideListAndClearInput();
                                var iss = TagsAdd(cli.attr('data-id'), cli.text());
                            }
                        }
                        break;
                    //退格
                    case 8:
                        {
                            if ($.trim($(this).val()) == "") {
                                var stt = $(this).parent().children('span.tb-tags-i');
                                if (stt.length) {
                                    stt.last().remove();
                                }
                            }
                        }
                        break;
                }
            }
        });
        $('#tags').attr('data-eventKeydown', 1);
    }
}

//呈现常用视图
function createCommonHtml() {
    return false;
    if ($.trim($('#tags').val()) == "") {
        var cd = $.parseJSON($('#tags').attr('data-tagscommon')),
            fj = {}, ht1 = [], ht2 = [];
        $(cd).each(function () {
            var sub = fj[this.TagPid] || [];
            sub.push(this);
            fj[this.TagPid] = sub;
        });
        //大类、子类
        ht1.push('<ul class="nav nav-tabs">');
        ht2.push('<div class="tab-content">');
        $(fj[0]).each(function (i) {
            var id = this.TagId, text = this.TagName, pid = this.TagPid;
            if (i) {
                ht1.push('<li><a href="#tag_' + pid + '_' + id + '" data-toggle="tab">' + text + '</a></li>');
                ht2.push('<div class="tab-pane" id="tag_' + pid + '_' + id + '"><ul>');
            } else {
                ht1.push('<li class="active"><a href="#tag_' + pid + '_' + id + '" data-toggle="tab">' + text + '</a></li>');
                ht2.push('<div class="tab-pane active" id="tag_' + pid + '_' + id + '"><ul>');
            }
            //子类
            $(fj[id]).each(function (j) {
                var id = this.TagId, text = this.TagName, pid = this.TagPid;
                ht2.push('<li data-id="' + id + '">' + text + '</li>');
            });
            ht2.push('</ul></div>');
        });
        ht1.push('</ul>');
        ht2.push('</div>');

        $('#' + tags_common_id).html(ht1.join('') + ht2.join('')).show();

        autoCommonPosition();

        $(document).on('click', function (e) {
            e = e || window.event;
            var target = e.target || e.srcElement;
            if (!$('#' + tags_common_id)[0].contains(target)) {
                $('#' + tags_common_id).hide();
                $(this).off('click', arguments.callee);
            }
        });
    }
}

//添加标签
function TagsAdd(id, text) {
    var spt = $('#tags').parent().children("span");
    if (spt.length < 3) {
        var has = false;
        spt.each(function () {
            if ($(this).attr('data-id') == id) {
                has = true;
                return false;
            }
        });
        if (!has) {
            var sp = document.createElement('span');
            sp.className = "tb-tags-i";
            sp.setAttribute('data-id', id);
            sp.innerHTML = text + '<em class="tb-tags-del" onclick="var es=this.parentElement;es.parentElement.removeChild(es);">×</em>';
            $('#tags').parent()[0].insertBefore(sp, $('#tags')[0]);
            return true;
        }
    }
}

//清空标签
function TagsClear() {
    var spt = $('#tags').parent().children("span");
    spt.remove();
}

//空白地方关闭
function clickBlankClose() {
    if ($('#tags').attr('data-eventCBC') != "1") {
        $(document).on('click', function () {
            HideListAndClearInput();
            $(this).off('click', arguments.callee);
        });
        $('#tags').attr('data-eventCBC', 1)
    }
}

//隐藏列表&清空输入框
function HideListAndClearInput() {
    $('#' + tags_list_id).hide();
    $('#tags').val('');
    $('#tags').attr('data-eventCBC', 0);
}