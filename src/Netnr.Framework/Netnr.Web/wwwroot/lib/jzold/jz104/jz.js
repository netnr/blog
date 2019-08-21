/*
 *  时间：2016-08-07
 *  作者：周华
 */

(function (window) {

    var jz = function (t) { return new jz.fn.init(t); };
    //版本号
    jz.version = 104;

    /* IE8- */
    jz.OldIE = function () { return typeof document.createElement == "object" || false };

    //字符串两边去空 ie8-
    if (jz.OldIE()) { String.prototype.trim = function () { return this.replace(/(^\s+)|(\s+$)/g, "") } };

    //添加事件处理程序
    jz.on = function (type, fn, obj) {
        if (obj.addEventListener) {
            type == "mousewheel" && typeof (onmousewheel) == "undefined" && (type = "DOMMouseScroll");/*兼容FireFox滚轮事件*/
            obj.addEventListener(type, fn, false);
        } else if (obj.attachEvent) {
            obj['e' + type + fn] = fn;/*对象某属性等于该处理程序 this 指向对象本身*/
            obj[type + fn] = function () { obj['e' + type + fn]() }
            obj.attachEvent("on" + type, obj[type + fn]);
        } else { obj["on" + type] = fn }
    };

    /*移除事件处理程序*/
    jz.off = function (type, fn, obj) {
        if (obj.removeEventListener) {
            (type == "mousewheel" && typeof (onmousewheel) == "undefined") && (type = "DOMMouseScroll");/*兼容FireFox滚轮事件*/
            obj.removeEventListener(type, fn, false);
        } else if (obj.detachEvent) {
            obj.detachEvent("on" + type, obj[type + fn]); obj[type + fn] = null
        }
    };

    //原型拓展
    jz.fn = jz.prototype = {
        init: function (t) {
            switch (typeof t) {
                case 'string':
                    if (document.getElementById(t)) {
                        this.length = 1;
                        this[0] = document.getElementById(t);
                    } else {
                        this.length = 0;
                    }
                    return this;
                    break;
                case 'object':
                    this.length = t.length || 1;
                    if (this.length > 1) {
                        for (var i = 0; i < this.length; i++) {
                            this[i] = t[i];
                        }
                    } else {
                        this[0] = t;
                    }
                    return this;
                    break;
                default:
                    this.length = t.length || 1;
                    this[0] = t;
                    return this;
                    break;
            }
        },
        //遍历
        each: function (fn) { jz.each(this, fn); return this },
        //事件绑定处理程序
        on: function (type, fn) {
            jz.each(this, function () {
                jz.on(type, fn, this);
            });
            return this;
        },
        //事件解绑处理程序
        off: function (type, fn) {
            jz.each(this, function () {
                jz.off(type, fn, this);
            });
            return this;
        },
        //末尾追加
        append: function (t) {
            if (typeof t === "string") {
                var tmp = document.createElement("div"), nodes, cdf = document.createDocumentFragment(), i = 0, len;
                tmp.innerHTML = t;
                nodes = tmp.childNodes;
                len = nodes.length;
                for (; i < len ; i++) {
                    cdf.appendChild(nodes[i].cloneNode(true));
                }
                t = cdf;
            }
            jz.each(this, function () {
                this.nodeType == 1 && this.appendChild(t);
            });
            return this;
        },
        //删除元素 倒计时 秒
        remove: function (time) {
            jz.each(this, function () {
                this.nodeType == 1 && jz.remove(this, time);
            });
        },
        //文本值改变事件
        input: function (fn) {
            if (jz.OldIE()) {
                jz.each(this, function () {
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
                jz.each(this, function () {
                    this.nodeType == 1 && jz.on("input", fn, this);
                });
            }
            return this;
        },
        //取值/赋值
        val: function (value) {
            if (arguments.length) {
                jz.each(this, function () {
                    this.nodeType == 1 && (this.value = value);
                });
            }
            return this[0].value;
        },
        //是否为空
        isEmpty: function (s) { return this[0].value.trim() == "" ? true : false },
        //显示
        show: function (s) {
            jz.each(this, function () {
                this.nodeType == 1 && (this.style.display = s || "block");
            });
            return this;
        },
        //隐藏
        hide: function (s) {
            jz.each(this, function () {
                this.nodeType == 1 && (this.style.display = s || "none");
            });
            return this;
        },
        //html取值、赋值
        html: function (html) {
            if (this.length) {
                arguments.length && jz.each(this, function () {
                    this.nodeType == 1 && (this.innerHTML = html);
                });
                return this[0].innerHTML;
            }
            return undefined;
        },
        //text取值、赋值
        text: function (text) {
            if (this.length) {
                if (arguments.length) {
                    jz.OldIE() ? jz.each(this, function () {
                        this.nodeType == 1 && (this.innerText = text);
                    }) : jz.each(this, function () {
                        this.nodeType == 1 && (this.textContent = text);
                    });
                }
                return jz.OldIE() ? this[0].innerText : this[0].textContent;
            }
            return undefined;
        },
        //特性值
        attr: function (name, value) {
            if (this.length) {
                if (arguments.length == 2) {
                    jz.each(this, function () {
                        this.setAttribute(name, value);
                    });
                    return this;
                } else {
                    return this[0].getAttribute(name)
                }
            } return undefined;
        },
        //删除特性值
        removeAttr: function (name) {
            if (this.length) {
                jz.each(this, function () {
                    this.removeAttribute(name);
                    /* IE8-- 只是移除了class属性，而className还在*/
                    name.toLowerCase() == "class" && jz.OldIE() && this.removeAttribute('className');
                });
                return this;
            } return undefined;
        },
        //样式取值、赋值
        css: function (name, value) {
            if (this.length) {
                if (arguments.length == 2) {
                    jz.each(this, function () {
                        this.style[name] = value;
                    });
                    return this;
                } else {
                    return this[0].style[name];
                }
            } return undefined;
        },
        //添加样式
        addClass: function (className) {
            jz.each(this, function () {
                var cn = " " + this.className + " ";
                jz.each(className.split(" "), function () {
                    while (cn.indexOf(" " + this + " ") < 0) {
                        cn += this + " ";
                    }
                });
                this.className = cn.trim();
            });
            return this;
        },
        //删除样式
        removeClass: function (className) {
            jz.each(this, function () {
                var cn = " " + this.className + " ";
                jz.each(className.split(" "), function () {
                    while (cn.indexOf(" " + this + " ") >= 0) {
                        cn = cn.replace(" " + this + " ", " ");
                    }
                });
                this.className = cn.trim();
            });
            return this;
        },
        //样式是否存在
        hasClass: function (className) {
            var result = false;
            jz.each(this, function () {
                var cn = this.className;
                if (cn) {
                    if ((" " + cn + " ").indexOf(" " + className + " ") >= 0) {
                        result = true;
                        return false;
                    }
                }
            });
            return result;
        },
        //重新赋值 jz对象
        deassign: function (match) {
            var match = match || [], that = this;
            for (var i = 0; i < this.length; i++) {
                try {
                    this[i] = undefined;
                    delete this[i];
                } catch (e) { }
            }
            jz.each(match, function () {
                that[arguments[0]] = this;
            });
            this.length = match.length;
            return this;
        },
        //获取元素集
        find: function (nodeName) {
            var match = [];
            jz.each(this, function () {
                jz.each(this.getElementsByTagName('*'), function () {
                    (nodeName == "*" || ("," + nodeName + ",").toUpperCase().indexOf("," + this.nodeName + ",") >= 0) && match.push(this);
                });
            });
            return this.deassign(match);
        },
        //之后的所有兄弟节点
        nextAll: function () {
            var match = [];
            jz.each(this, function () {
                match = match.concat(jz.dir(this, "nextSibling"));
            });
            return this.deassign(jz.ArrayUnique(match));
        },
        //下一个兄弟节点
        next: function (nodeName) {
            var match = [];
            jz.each(this, function () {
                var m = jz.dir(this, "nextSibling");
                if (m.length) {
                    (!nodeName || nodeName == "*" || ("," + nodeName + ",").toUpperCase().indexOf("," + m[0].nodeName + ",") >= 0) && match.push(m[0]);
                }
            });
            return this.deassign(jz.ArrayUnique(match));
        },
        //之前的所有兄弟节点
        prevAll: function () {
            var match = [];
            jz.each(this, function () {
                match = match.concat(jz.dir(this, "previousSibling"));
            });
            return this.deassign(jz.ArrayUnique(match));
        },
        //上一个兄弟节点
        prev: function (nodeName) {
            var match = [];
            jz.each(this, function () {
                var m = jz.dir(this, "previousSibling");
                if (m.length) {
                    (!nodeName || nodeName == "*" || ("," + nodeName + ",").toUpperCase().indexOf("," + m[0].nodeName + ",") >= 0) && match.push(m[0]);
                }
            });
            return this.deassign(jz.ArrayUnique(match));
        },
        //父级节点
        parent: function (nodeName) {
            var match = [];
            jz.each(this, function () {
                var m = jz.dir(this, "parentNode");
                if (m.length) {
                    (!nodeName || nodeName == "*" || ("," + nodeName + ",").toUpperCase().indexOf("," + m[0].nodeName + ",") >= 0) && match.push(m[0]);
                }
            });
            return this.deassign(jz.ArrayUnique(match));
        },
        //全部父级节点
        parents: function (nodeName) {
            var match = [];
            jz.each(this, function () {
                match = match.concat(jz.dir(this, "parentNode"));
            });
            return this.deassign(jz.ArrayUnique(match));
        },
        //子节点
        children: function (nodeName) {
            var match = [];
            jz.each(this, function () {
                jz.each(this.children, function () {
                    (!nodeName || nodeName == "*" || ("," + nodeName + ",").toUpperCase().indexOf("," + this.nodeName + ",") >= 0) && match.push(this);
                });
            });
            return this.deassign(match);
        },
        //第一个节点
        first: function () {
            return this.deassign([this[0]]);
        },
        //最后一个节点
        last: function () {
            return this.deassign([this[this.length - 1]]);
        },
        //Form表单序列化为数组
        serializeArray: function () {
            if (this.length) {
                var arr = [];
                jz.each(this[0], function () {
                    if (this.name.length) {
                        var obj = {};
                        obj["name"] = this.name;
                        obj["value"] = this.value;
                        arr.push(obj);
                    }
                });
                return arr;
            } return undefined;
        },
        //Form表单序列化
        serialize: function () {
            var arr = this.serializeArray();
            if (arr && arr.length) {
                var rs = [];
                jz.each(arr, function () {
                    rs.push(this.name + "=" + encodeURIComponent(this.value));
                });
                return rs.join('&');
            } return "";
        },
        //仿placeholder
        hint: function (msg) {
            jz.each(this, function () {
                jz.hint(this, msg);
            });
            return this;
        },
        //宽高、边距、滚动条间距、内容宽高
        px: function () {
            return jz.px(this[0]);
        }
    };

    //jz实例对象继承jz原型初始里的方法和属性，jz原型赋给jz对象
    jz.prototype.init.prototype = jz.prototype;

    //遍历
    jz.each = function (obj, fn) {
        //this指向obj[i]; i是第一个参数 obj[i]第二个 ...
        var i = 0, len = obj.length;
        for (; i < len; i++) {
            if (fn.call(obj[i], i, obj[i]) == false) { break }
        }
    };

    //检索一个节点某个方向的节点 dir可选值：parentNode nextSibling previousSibling
    jz.dir = function (t, dir) {
        var match = [], cur = t[dir];
        while (cur && cur.nodeType != 9) {
            cur.nodeType == 1 && match.push(cur);
            cur = cur[dir];
        }
        return match;
    }

    //添加处理事件
    jz.each(("blur focus focusin focusout load resize scroll unload click dblclick "
           + "mousedown mouseup mousemove mouseover mouseout mouseenter mouseleave "
           + "change select submit keydown keypress keyup error contextmenu").split(" ")
           , function (i, name) {
               jz.fn[name] = function (fn) {
                   jz.each(this, function () { jz.on(name, fn, this); });
                   return this
               }
           });

    //数组去重
    jz.ArrayUnique = function (array) {
        var match = [];
        for (var i = 0; i < array.length; i++) {
            var has = false;
            for (var u = 0; u < match.length; u++) {
                if (match[u] == array[i]) {
                    has = true;
                    break;
                }
            }
            !has && match.push(array[i]);
        }
        return match;
    }

    //判断数组
    jz.isArray = function (t) { return Object.prototype.toString.call(t) === "[object Array]" };

    //判断是键值对
    jz.isKV = function (t) { return t && typeof (t) == "object" && Object.prototype.toString.call(t).toLowerCase() == "[object object]" && !t.length }

    //event
    jz.event = function (e) { return e || window.event };

    //target
    jz.target = function (e) { return jz.event(e).srcElement || jz.event(e).target };

    //阻止事件冒泡
    jz.stopEvent = function (e) { if (e && e.stopPropagation) { e.stopPropagation() } else { window.event.cancelBubble = true } };

    //阻止浏览器默认行为
    jz.stopDefault = function (e) { if (e && e.preventDefault) { e.preventDefault() } else { window.event.returnValue = false } };

    //按键ASCII值
    jz.keys = function (e) { return jz.event(e).keyCode || jz.event(e).which || jz.event(e).charCode };

    //删除元素 对象，倒计时
    jz.remove = function (t, time) {
        if (typeof time === "number") { setTimeout(function () { re(); }, time * 1000) } else { re() }
        function re() { try { t.parentElement.removeChild(t); } catch (e) { } };
    };

    //读取或设置cookie 键 值(需手动转码) 有效期(设置时，默认不指定过期时间，单位天,0代表删除)
    jz.cookie = function (k, v, day) {
        if (arguments.length == 1) {
            var v = '', s = document.cookie, arr = s.split(';');
            if (s.indexOf(k) > -1) {
                jz.each(arr, function (i, name) {
                    var key = name.split('=')[0].trim();
                    if (key == k) {
                        v = name.replace(key + "=", "");
                        return false;
                    }
                });
            } return v
        } else {
            var d = new Date(), c = k + "=" + v + ";path=/;";
            if (day == 0) {
                d.setTime(d.getTime() - 1); c += "expires=" + d.toGMTString()
            } else if (day != undefined) {
                d.setTime(d.getTime() + day * 24 * 60 * 60 * 1000); c += "expires=" + d.toGMTString()
            } document.cookie = c;
            return this;
        }
    };

    //转码 To Unicode
    jz.unicode = function (s) {
        var val = "", i = 0, c, len = s.length;
        for (; i < len; i++) {
            c = s.charCodeAt(i).toString(16);
            while (c.length < 4) { c = '0' + c; } val += '\\u' + c
        } return val
    };

    //转码 Unicode To STR-CN
    jz.ununicode = function (s) { return eval("'" + s + "'"); };/*return unescape(str.replace(/\u/g, "%u"))*/

    //转码 To ASCII
    jz.ascii = function (s) {
        var val = "", i = 0, len = s.length;
        for (; i < len; i++) { val += "&#" + s[i].charCodeAt() + ";"; }
        return val
    };

    //转码 ASCII To STR-CN
    jz.unascii = function toAsciiUn(s) {
        var val = "", strs = s.match(/&#(\d+);/g);
        if (strs != null) {
            for (var i = 0, len = strs.length; i < len; i++) {
                val += String.fromCharCode(strs[i].replace(/[&#;]/g, ''));
            }
        } return val
    };

    //placeholder标签兼容
    jz.placeholder = function () {
        if ('placeholder' in document.createElement('input')) { return }
        jz(document).find('input,textarea').each(function () {
            if (this.getAttribute("placeholder") != null && this.getAttribute("placeholder").trim() != "") {
                jz.hint(this, this.getAttribute("placeholder"));
            }
        });
    };

    //文本提示
    jz.hint = function (t, msg) {
        var input = jz(t), lbl = document.createElement("label");
        lbl.id = "jzhint" + jz.random();
        input.attr("hintid", lbl.id);
        lbl.innerHTML = msg;
        document.body.appendChild(lbl);
        lbl["selfH"] = Math.min(lbl.offsetHeight, 20);

        jz(lbl).mouseover(autoxy);
        jz(lbl).click(function () { input[0].focus(); });
        lbl.onselectstart = function () { return false };
        lbl.style.cssText = "position:absolute;-moz-user-select:none;color:#aaa;cursor:text;display:none;z-index:2";
        setTimeout(function () { autoxy(); }, 10);
        input.input(function () {
            lbl.style.display = this.value.length ? "none" : "block";
        });

        if (window.addEventListener) { window.addEventListener("resize", autoxy, false); }
        else { window.attachEvent("onresize", autoxy); }

        setTimeout(function () { lbl.style.display = input.val().length ? "none" : "block"; }, 800);

        function autoxy() {
            lbl.style.top = (input.px().top + jz.px(document).scrollTop + (input[0].offsetHeight - lbl["selfH"]) / 2) + "px";
            lbl.style.left = (input.px().left + jz.px(document).scrollLeft + 12) + "px";
        }
    }

    //当前时间 自定义格式 yyyy-MM-dd HH:mm:ss
    jz.time = function (format) {
        function t(n) { return n < 10 ? "0" + n : n }
        var d = new Date(),
            y = d.getFullYear(),
            m = t(d.getMonth() + 1),
            day = t(d.getDate()),
            h = t(d.getHours()),
            min = t(d.getMinutes()),
            s = t(d.getSeconds()),
            f = d.getMilliseconds();
        return !arguments.length ? (y + "-" + m + "-" + day + " " + h + ":" + min + ":" + s) :
                format.replace("yyyy", y).replace("MM", m).replace("dd", day).replace("HH", h).replace("mm", min).replace("ss", s).replace("ff", f)
    };

    //随机数字 长度（默认4位）1到15
    jz.random = function (len) { len = arguments.length ? len > 15 ? 15 : len : 4; return Math.random().toString().substr(2, len) };

    //格式化JSON
    jz.parseJSON = function (s) { return jz.OldIE() ? (new Function("return " + s))() : JSON.parse(s) };

    /*
    url:"http://netnr.com?id=id",  //url地址
    type:"post",                  //默认get
    async:true,                 //默认异步
    data:"post提交内容",        //post内容
    dataForm:FormData           //HTML5表单对象 有值时忽略 type、data
    dataType:"json/xml",        //默认文本
    percent:function(p){},      //有HTML5文件上传的真实进度回调 0-100
    success:function(val){},    //返回内容
    error:function(xhr){}       //错误回调
    complete:function           //完成回调
    */
    jz.ajax = function (ops) {
        var xhr = (window.XMLHttpRequest) ? (new XMLHttpRequest()) : (new ActiveXObject("Microsoft.XMLHTTP")),
            type = ops.type || "get",   //默认GET
            async = ops.async == undefined ? true : ops.async, //默认异步
            data = ops.data || null,  //发送内容
            dataType = ops.dataType || "text";//格式化

        if (jz.isKV(data)) { var d = ""; for (var i in data) { d += ('&' + i + '=' + encodeURIComponent(data[i])); } data = d.substr(1); }

        //HTML5 FormData 上传进度监听
        if (ops.dataForm) {
            xhr.upload.onprogress = function (event) {
                if (event.lengthComputable) {
                    var per = (event.loaded / event.total) * 100;
                    typeof ops.percent == "function" && ops.percent(per.toFixed(0));
                }
            };
        }

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
                                    val = jz.parseJSON(val);
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

        //HTML5 发送FormData
        if (ops.dataForm) {
            xhr.open("post", ops.url, async);
            xhr.setRequestHeader("X-Requested-With", "XMLHttpRequest");
            xhr.send(ops.dataForm)
        } else {
            xhr.open(type, ops.url, async);
            type == "post" && xhr.setRequestHeader("Content-Type", "application/x-www-form-urlencoded");
            xhr.send(data);
        }
    };

    //输出<style>样式
    jz.writeStyle = function (css) {
        var s = document.createElement("style");
        s.type = "text/css";
        s.styleSheet ? s.styleSheet.cssText = css : s.innerHTML = css;
        document.getElementsByTagName("HEAD")[0].appendChild(s);
    }

    //输出<script>脚本 加载完成回调
    jz.getScript = function (src, fn) {
        var s = document.createElement("SCRIPT"); s.src = src; s.type = "text/javascript";
        document.getElementsByTagName("HEAD")[0].appendChild(s);
        //加载完成回调
        if (fn != undefined) {
            s.onload = s.onreadystatechange = function () {
                if (!this.readyState || this.readyState == "loaded" || this.readyState == "complete")
                { fn(); }
            }
        }
    }

    //宽高、边距、滚动条间距、内容宽高
    jz.px = function (element) {
        var result = {
            //宽
            width: null,
            //高
            height: null,
            //上边距
            top: null,
            //左边距
            left: null,
            //垂直滚动条上间距
            scrollTop: null,
            //水平滚动条左间距
            scrollLeft: null,
            //内容高度
            scrollHeight: null,
            //内容宽度
            scrollWidth: null
        };
        if (element === window || element === document) {
            result.width = document.documentElement.clientWidth || document.body.clientWidth;
            result.height = document.documentElement.clientHeight || document.body.clientHeight;
            result.scrollTop = document.documentElement.scrollTop || document.body.scrollTop;
            result.scrollLeft = document.documentElement.scrollLeft || document.body.scrollLeft;
            result.scrollWidth = document.documentElement.scrollWidth || document.body.scrollWidth;
            result.scrollHeight = document.documentElement.scrollHeight || document.body.scrollHeight;
        } else {
            result.width = element.offsetWidth;
            result.height = element.offsetHeight;
            var mg = element.getBoundingClientRect();
            result.top = mg.top;
            result.left = mg.left;
            result.scrollTop = element.scrollTop;
            result.scrollLeft = element.scrollLeft;
            result.scrollWidth = element.scrollWidth;
            result.scrollHeight = element.scrollHeight;
        }
        return result;
    }

    window.j = jz;

})(window);
