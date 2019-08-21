/*                                      *\
 *  Date：2017-06-13
 *  Author：Tomorrow
 *  Site：http://www.netnr.com
\*                                      */

(function (window) {

    var jz = function (selector) { return new jz.fn.init(selector); }, arr = [];

    jz.fn = jz.prototype = {
        init: function (selector) {
            if (!selector) {
                return this;
            }

            var len, elem, match;

            if (typeof selector === "string") {
                len = selector.length;
                //HTML Label
                if (len > 2 && selector[0] === "<" && selector[len - 1] === ">") {
                    var tep = document.createElement('div'), nodes, cdf = document.createDocumentFragment(), i = 0;
                    tep.innerHTML = selector;
                    nodes = tep.childNodes;
                    for (; i < nodes.length; i++) {
                        var _newcn = nodes[i].cloneNode(true);
                        cdf.appendChild(_newcn);
                        this[i] = _newcn;
                    }
                    this.length = nodes.length;
                    return this;
                }
                //#ID
                if (selector[0] === "#" && len > 1) {
                    elem = document.getElementById(selector.substring(1));
                    if (elem) {
                        this[0] = elem;
                        this.length = 1;
                        return this;
                    }
                }
                //.CLASS
                if (selector[0] === "." && len > 1) {

                }
                //*
                if (selector === "*") {
                    match = document.getElementsByTagName('*');
                    var i = 0, mlen = match.length;
                    for (; i < mlen; i++) {
                        this[i] = match[i];
                    }
                    this.length = mlen;
                    return this;
                }
                //noteName

                //DOMElement
            } else if (selector.nodeType) {
                this[0] = selector;
                this.length = 1;
                return this;
            } else if (typeof selector === "function") {

            }

            //数组或伪数组
            if (selector !== undefined) {
                var i = 0, match = selector || [];
                for (; i < match.length; i++) {
                    this[i] = match[i];
                }
                this.length = match.length;
            }

            return this;
        },
        length: 0,
        push: arr.push,
        slice: arr.slice,
        splice: arr.splice,
        constructor: jz,
        //转为数组
        toArray: function () {
            return this.slice.call(this);
        },
        //获取数组指定项为DOM对象
        get: function (num) {
            if (num == null) {
                return this.toArray();
            } else {
                return this[num];
            }
        },
        //遍历
        each: function (callback) {
            jz.each(this, callback);
            return this;
        },
        //第一个对象
        first: function () {
            return this.eq(0);
        },
        //最后一个对象
        last: function () {
            return this.eq(this.length - 1);
        },
        //获取指定索引的对象
        eq: function (i) {
            var len = this.length, j = +i + (i < 0 ? len : 0);
            return this.deassign([this[i]]);
        },
        //重新赋值 jz对象
        deassign: function (match) {
            var newjz = this.constructor();
            this.prevObject = this;

            match = match || [], len = match.length, i = 0;
            for (; i < len; i++) {
                newjz[i] = match[i];
            }
            newjz.length = len;
            return newjz;
        },
        //事件添加处理程序
        on: function (type, callback) {
            jz.each(this, function () {
                jz.on(type, callback, this);
            });
        },
        //事件移除处理程序
        off: function (type, callback) {
            jz.each(this, function () {
                jz.off(type, callback, this);
            });
        },
        //文本值改变事件
        input: function (callback) {
            if (jz.oldIE()) {
                jz.each(this, function () {
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
                jz.each(this, function () {
                    this.nodeType == 1 && jz.on("input", callback, this);
                });
            }
            return this;
        },
        //末尾追加
        append: function (t) {
            if (typeof t === "string") {
                j(t)
                var tep = document.createElement("div"), nodes, cdf = document.createDocumentFragment(), i = 0;
                tep.innerHTML = t;
                nodes = tep.childNodes;
                for (; i < nodes.length ; i++) {
                    cdf.appendChild(nodes[i].cloneNode(true));
                }
                t = cdf;
            }
            jz.each(this, function () {
                this.nodeType == 1 && this.appendChild(t);
            });
            return this;
        },
        //删除元素
        remove: function () {
            jz.each(this, function () {
                if (this.nodeType == 1) {
                    try { this.parentElement.removeChild(this); } catch (e) { }
                }
            });
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
        isEmpty: function (s) { return jz.trim(this[0].value) == "" ? true : false },
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
        },
        //text取值、赋值
        text: function (text) {
            if (this.length) {
                if (arguments.length) {
                    jz.oldIE() ? jz.each(this, function () {
                        this.nodeType == 1 && (this.innerText = text);
                    }) : jz.each(this, function () {
                        this.nodeType == 1 && (this.textContent = text);
                    });
                }
                return jz.oldIE() ? this[0].innerText : this[0].textContent;
            }
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
            }
        },
        //删除特性值
        removeAttr: function (name) {
            if (this.length) {
                jz.each(this, function () {
                    this.removeAttribute(name);
                    /* IE8-- 只是移除了class属性，而className还在*/
                    name.toLowerCase() == "class" && jz.oldIE() && this.removeAttribute('className');
                });
                return this;
            }
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
            };
        },
        //添加样式
        addClass: function (className) {
            jz.each(this, function () {
                var cn = " " + this.className + " ", cns = className.split(" ") || [];
                $.each(cns, function () {
                    while (cn.indexOf(" " + this + " ") < 0) {
                        cn += this + " ";
                    }
                });
                this.className = jz.trim(cn);
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
                this.className = jz.trim(cn);
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
            return this.deassign(jz.arrayUnique(match));
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
            return this.deassign(jz.arrayUnique(match));
        },
        //之前的所有兄弟节点
        prevAll: function () {
            var match = [];
            jz.each(this, function () {
                match = match.concat(jz.dir(this, "previousSibling"));
            });
            return this.deassign(jz.arrayUnique(match));
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
            return this.deassign(jz.arrayUnique(match));
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
            return this.deassign(jz.arrayUnique(match));
        },
        //全部父级节点
        parents: function (nodeName) {
            var match = [];
            jz.each(this, function () {
                match = match.concat(jz.dir(this, "parentNode"));
            });
            return this.deassign(jz.arrayUnique(match));
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
        //Form表单序列化为数组
        serializeArray: function () {
            if (this.length) {
                var match = [];
                jz.each(this[0], function () {
                    if (this.name.length) {
                        var obj = {};
                        obj["name"] = this.name;
                        obj["value"] = this.value;
                        match.push(obj);
                    }
                });
                return match;
            };
        },
        //Form表单序列化
        serialize: function () {
            var match = this.serializeArray();
            if (match && match.length) {
                var rs = [];
                jz.each(match, function () {
                    rs.push(this.name + "=" + encodeURIComponent(this.value));
                });
                return rs.join('&');
            } return "";
        },
        //宽高、边距、滚动条间距、内容宽高
        px: function () {
            return jz.px(this[0]);
        }
    };

    jz.fn.init.prototype = jz.prototype;

    //遍历 object、array
    jz.each = function (object, callback) {
        var k, i = 0, len = object.length, isObj = len === undefined || typeof object == "function";
        if (isObj) {
            for (k in object) {
                if (callback.call(object[k], k, object[k]) === false) {
                    break;
                }
            }
        } else {
            for (; i < len;) {
                if (callback.call(object[i], i, object[i++]) === false) {
                    break;
                }
            }
        }
    };

    //事件添加处理程序
    jz.on = function (type, callback, obj) {
        if (obj.addEventListener) {
            obj.addEventListener(type, callback, false);
        } else if (obj.attachEvent) {
            obj['e' + type + callback] = callback;/*对象某属性等于该处理程序 this 指向对象本身*/
            obj[type + callback] = function () { obj['e' + type + callback]() }
            obj.attachEvent("on" + type, obj[type + callback]);
        } else {
            obj["on" + type] = callback
        }
    };

    //移除事件的处理程序
    jz.off = function (type, callback, obj) {
        if (obj.removeEventListener) {
            obj.removeEventListener(type, callback, false);
        } else if (obj.detachEvent) {
            obj.detachEvent("on" + type, obj[type + callback]); obj[type + callback] = null
        }
    };

    //检索一个节点某个方向的节点 dir可选值：parentNode nextSibling previousSibling
    jz.dir = function (elem, dir) {
        var match = [], cur = elem[dir];
        while (cur && cur.nodeType != 9) {
            cur.nodeType == 1 && match.push(cur);
            cur = cur[dir];
        }
        return match;
    };

    //添加处理事件
    jz.each(("blur focus focusin focusout load resize scroll unload click dblclick "
           + "mousedown mouseup mousemove mouseover mouseout mouseenter mouseleave "
           + "change select submit keydown keypress keyup error contextmenu").split(" ")
           , function (i, name) {
               jz.fn[name] = function (callback) {
                   jz.each(this, function () { jz.on(name, callback, this); });
                   return this;
               }
           });

    //数组去重
    jz.arrayUnique = function (array) {
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
    };

    //去空
    jz.trim = function (s) { s.replace(/(^\s+)|(\s+$)/g, "") };

    //判断数组
    jz.isArray = function (t) { return Object.prototype.toString.call(t) === "[object Array]" };

    //判断是键值对
    jz.isKV = function (t) { return t && typeof (t) == "object" && Object.prototype.toString.call(t).toLowerCase() == "[object object]" && !t.length };

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

    //随机数字 长度（默认4位）1到15
    jz.random = function (len) { len = arguments.length ? len > 15 ? 15 : len : 4; return Math.random().toString().substr(2, len) };

    /* IE8- */
    jz.oldIE = function () { return typeof document.createElement == "object" || false };

    //格式化JSON
    jz.parseJSON = function (s) { return jz.oldIE() ? (new Function("return " + s))() : JSON.parse(s) };

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
    };

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
    };

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
        }, dde = document.documentElement, dbd = document.body;
        if (element === window || element === document) {
            result.width = dde.clientWidth || dbd.clientWidth;
            result.height = dde.clientHeight || dbd.clientHeight;
            result.scrollTop = dde.scrollTop || dbd.scrollTop;
            result.scrollLeft = dde.scrollLeft || dbd.scrollLeft;
            result.scrollWidth = dde.scrollWidth || dbd.scrollWidth;
            result.scrollHeight = dde.scrollHeight || dbd.scrollHeight;
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