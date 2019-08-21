/* 
 *  周华
 *  2016-03-22
 *  Version:1.0.3
 */

(function () {
    var jz = function (t) { return new jz.prototype.init(t) };

    /* IE8- */
    jz.OldIE = function () { return typeof document.createElement == "object" || false };

    //字符串两边去空 ie8-
    if (jz.OldIE()) { String.prototype.trim = function () { return this.replace(/(^\s+)|(\s+$)/g, "") } };

    //添加事件处理程序
    jz.on = function (type, fn, obj) {
        if (obj.addEventListener) {
            type == "mousewheel" && typeof (onmousewheel) == "undefined" && (type = "DOMMouseScroll");
            obj.addEventListener(type, fn, false);
        } else if (obj.attachEvent) {
            obj['e' + type + fn] = fn;/*对象某属性等于该处理程序 this 指向对象本身*/
            obj[type + fn] = function () { obj['e' + type + fn]() }
            obj.attachEvent("on" + type, obj[type + fn]);
        } else { obj["on" + type] = fn }
    };

    /*移除事件处理程序*/
    jz.unon = function (type, fn, obj) {
        if (obj.removeEventListener) {
            (type == "mousewheel" && typeof (onmousewheel) == "undefined") && (type = "DOMMouseScroll");
            obj.removeEventListener(type, fn, false);
        } else if (obj.detachEvent) {
            obj.detachEvent("on" + type, obj[type + fn]); obj[type + fn] = null
        }
    };

    //对象拓展
    jz.fn = jz.prototype = {
        init: function (t) { this[0] = typeof (t) === "object" ? t : document.getElementById(t); return this },
        //遍历数组、字符串
        each: function (fn) { jz.each(this[0], fn); return this },
        //事件绑定处理程序
        on: function (type, fn) { jz.on(type, fn, this[0]); return this },
        //事件解绑处理程序
        unon: function (type, fn) { jz.unon(type, fn, this[0]); return this },
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
            } this[0].appendChild(t); return this;
        },
        //删除元素 倒计时 秒
        remove: function (time) { jz.remove(this[0], time) },
        //文本值改变事件
        input: function (fn) {
            if (jz.OldIE()) {
                var that = this[0];
                this[0].attachEvent("onpropertychange", function () {
                    if (event.propertyName.toLowerCase() == "value") { fn.apply(that, arguments); }
                });
            } else { jz.on("input", fn, this[0]); }
            return this;
        },
        //点击对象的空白地方 默认移除 hid隐藏
        blankClick: function (type) { jz.blankClick(this[0], type); return this },
        //模拟placeholder
        hint: function (msg) { jz.hint(this[0], msg); return this },
        //取值/赋值
        val: function (value) { return arguments.length ? this[0].value = value : this[0].value },
        //是否为空
        isEmpty: function (s) { return this[0].value.trim() == "" ? true : false },
        //显示
        show: function (s) { this[0].style.display = arguments.length ? s : "block"; return this },
        //隐藏
        hide: function () { this[0].style.display = "none"; return this },
        //html取值、赋值
        html: function (s) { return arguments.length ? this[0].innerHTML = s : this[0].innerHTML },
        //text取值、赋值
        text: function (s) {
            arguments.length && (jz.OldIE() ? this[0].innerText = s : this[0].textContent = s);
            return jz.OldIE() ? this[0].innerText : this[0].textContent;
        },
        //获取元素集
        find: function (nodeName) { var g = this[0].getElementsByTagName(nodeName); g.each = function (fn) { jz.each(g, fn) }; return g; },
        //特性值
        attr: function (name, value) { arguments.length == 2 && (this[0].setAttribute(name, value)); return this[0].getAttribute(name) },
        //样式取值、赋值
        css: function (name, value) { if (arguments.length == 1) { return this[0].style[name] } this[0].style[name] = value; return this },
        //拖动
        drag: function (dragT) { jz.drag(this[0], dragT); return this },
        //滚动条高度
        scrollH: function (v) { return jz.scrollH(this[0], v) },
        //水平滚动条左间距
        scrollW: function (v) { return jz.scrollW(this[0], v) },
        //可视高度
        visualH: function () { return jz.visualH(this[0]) },
        //可视宽度
        visualW: function () { return jz.visualW(this[0]) },
        //内容高度
        contentH: function () { return jz.contentH(this[0]) },
        //内容宽度
        contentW: function () { return jz.contentW(this[0]) },
        //序列化参数
        serialize: function () { return jz.serialize(this[0]) },
        //元素位置
        xy: function () { return jz.xy(this[0]) }
    };

    //为了使jz实例对象继承jz原型初始的里的方法和属性，通过把jz原型赋给jz对象
    jz.prototype.init.prototype = jz.prototype;

    //判断数组
    jz.isArray = function (t) { return Object.prototype.toString.call(t) === "[object Array]" };

    //判断是键值对
    jz.isKV = function (t) { return typeof (t) == "object" && Object.prototype.toString.call(t).toLowerCase() == "[object object]" && !t.length }

    //遍历对象 字符串或数组
    jz.each = function (obj, fn) {
        //this指向obj[i]; i是第一个参数 obj[i]第二个 ...
        var i = 0, len = obj.length;
        for (; i < len; i++) { if (fn.call(obj[i], i, obj[i]) == false) { break } }
    };

    //添加处理事件
    jz.each(("blur focus focusin focusout load resize scroll unload click dblclick "
           + "mousedown mouseup mousemove mouseover mouseout mouseenter mouseleave "
           + "change select submit keydown keypress keyup error contextmenu").split(" ")
           , function (i, name) {
               jz.fn[name] = function (fn) { jz.on(name, fn, this[0]); return this }
           });

    //删除元素 对象，倒计时
    jz.remove = function (t, time) {
        if (typeof time === "number") { setTimeout(function () { re(); }, time * 1000) } else { re() }
        function re() { try { jz(t)[0].parentElement.removeChild(t) } catch (e) { } }
    };

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
            var v = '', s = document.cookie;
            if (s.indexOf(k) > -1) {
                jz(s.split(';')).each(function (i, name) {
                    if (this.split('=')[0].trim() == k) {
                        v = this.split('=')[1];
                        return false
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

    //点击对象的空白地方 事件 默认移除
    jz.blankClick = function (t, type) {
        setTimeout(function () {
            jz(t).click(function (event) { jz.stopEvent(event); });
            jz(document).click(function () {
                switch (type) {
                    case "hid": jz(t)[0].style.display = "none"; break;
                    default: jz(t).remove();
                } jz(document).unon("click", arguments.callee);
            })
        }, 200)
    };

    //placeholder标签兼容
    jz.placeholder = function () {
        if ('placeholder' in document.createElement('input')) { return }
        jz(document).find("input").each(function () {
            if (this.getAttribute("placeholder") && this.getAttribute("placeholder").trim() != "") {
                jz.hint(this, this.getAttribute("placeholder"));
            }
        });
        jz(document).find("textarea").each(function () {
            if (this.getAttribute("placeholder") && this.getAttribute("placeholder").trim() != "") {
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
        input.input(function () { lbl.style.display = this.value.length ? "none" : "block"; });

        if (window.addEventListener) { window.addEventListener("resize", autoxy, false); }
        else { window.attachEvent("onresize", autoxy); }

        setTimeout(function () { lbl.style.display = input.val().length ? "none" : "block"; }, 800);

        function autoxy() {
            lbl.style.top = (input.xy().sTop + (input[0].offsetHeight - lbl["selfH"]) / 2) + "px";
            lbl.style.left = (input.xy().sLeft + 12) + "px";
        }
    }

    //获取位置
    jz.xy = function (t) {
        var top, left,
            tl = jz(t)[0].getBoundingClientRect();
        top = tl.top;
        left = tl.left;
        return { top: top, left: left, sTop: top + jz.scrollH(document), sLeft: left + jz.scrollW(document) }
    };

    //获取或设置水平滚动条左间距
    jz.scrollW = function (t, v) {
        if (t != document) {
            return arguments.length == 2 ? jz(t)[0].scrollLeft = v : jz(t)[0].scrollLeft;
        } else {
            if (arguments.length == 2) {
                return document.documentElement.scrollLeft = document.body.scrollLeft = v
            } else {
                return document.documentElement.scrollLeft || document.body.scrollLeft
            }
        }
    };

    //获取或设置滚动条高度
    jz.scrollH = function (t, v) {
        if (t != document) {
            return arguments.length == 2 ? jz(t)[0].scrollTop = v : jz(t)[0].scrollTop;
        } else {
            if (arguments.length == 2) {
                return document.documentElement.scrollTop = document.body.scrollTop = v;
            } else {
                return document.documentElement.scrollTop || document.body.scrollTop
            }
        }
    };

    //可视宽度
    jz.visualW = function (t) {
        if (t != document) { return jz(t)[0].offsetWidth }
        else { return document.documentElement.clientWidth || document.body.clientWidth }
    };

    //可视高度
    jz.visualH = function (t) {
        if (t != document) { return jz(t)[0].offsetHeight }
        else { return document.documentElement.clientHeight || document.body.clientHeight }
    };

    //内容宽度
    jz.contentW = function (t) {
        if (t != document) { return jz(t)[0].scrollWidth; }
        else { return document.documentElement.scrollWidth || document.body.scrollWidth }
    };

    //内容高度
    jz.contentH = function (t) {
        if (t != document) { return jz(t)[0].scrollHeight; }
        else { return document.documentElement.scrollHeight || document.body.scrollHeight }
    };

    //输出<style>样式
    jz.writestyle = function (css) {
        var s = document.createElement("style");
        s.type = "text/css";
        s.styleSheet ? s.styleSheet.cssText = css : s.innerHTML = css;
        document.getElementsByTagName("HEAD")[0].appendChild(s);
    }

    //拖动 对象、可拖动区域（默认对象）
    jz.drag = function (t, dragT) {
        var disX = dixY = 0, id = jz(t)[0], dragId = dragT != undefined ? jz(dragT)[0] : id;
        dragId.style.cursor = "move";
        dragId.onmousedown = function (e) {
            disX = jz.event(e).clientX - id.offsetLeft;
            disY = jz.event(e).clientY - id.offsetTop;
            document.onmousemove = function (e) {
                var x = jz.event(e).clientX - disX,
                    y = jz.event(e).clientY - disY,
                    maxX = jz.visualW(document) - id.offsetWidth + jz.scrollW(document),
                    maxY = jz.visualH(document) - id.offsetHeight + jz.scrollH(document);
                x <= 0 && (x = 0);
                y <= 0 && (y = 0);
                x >= maxX && (x = maxX);
                y >= maxY && (y = maxY);
                id.style.left = x + "px";
                id.style.top = y + "px";
                return false
            };
            document.onmouseup = function () {
                document.onmousemove = null;
                document.onmouseup = null;
                this.releaseCapture && this.releaseCapture()
            };
            this.setCapture && this.setCapture();
            return false;
        }
    };

    //随机数字 长度（默认4位）1到15
    jz.random = function (len) { len = arguments.length ? len > 15 ? 15 : len : 4; return Math.random().toString().substr(2, len) };

    //格式化JSON
    jz.parseJSON = function (s) { return jz.OldIE() ? (new Function("return " + s))() : JSON.parse(s) };

    //加载图片 base64编码
    jz.imgdata = 'data:image/gif;base64,R0lGODlhJQAlAJECAL3L2AYrTv///wAAACH/C05FVFNDQVBFMi4wAwEAAA'
                + 'Ah+QQFCgACACwAAAAAJQAlAAACi5SPqcvtDyGYIFpF690i8xUw3qJBwUlSadmcLqYmGQu6KDIeM13beG'
                + 'zYWWy3DlB4IYaMk+Dso2RWkFCfLPcRvFbZxFLUDTt21BW56TyjRep1e20+i+eYMR145W2eefj+6VFmgT'
                + 'Qi+ECVY8iGxcg35phGo/iDFwlTyXWphwlm1imGRdcnuqhHeop6UAAAIfkEBQoAAgAsEAACAAQACwAAAg'
                + 'WMj6nLXAAh+QQFCgACACwVAAUACgALAAACFZQvgRi92dyJcVJlLobUdi8x4bIhBQAh+QQFCgACACwXAB'
                + 'EADAADAAACBYyPqcsFACH5BAUKAAIALBUAFQAKAAsAAAITlGKZwWoMHYxqtmplxlNT7ixGAQAh+QQFCg'
                + 'ACACwQABgABAALAAACBYyPqctcACH5BAUKAAIALAUAFQAKAAsAAAIVlC+BGL3Z3IlxUmUuhtR2LzHhsi'
                + 'EFACH5BAUKAAIALAEAEQAMAAMAAAIFjI+pywUAIfkEBQoAAgAsBQAFAAoACwAAAhOUYJnAagwdjGq2am'
                + 'XGU1PuLEYBACH5BAUKAAIALBAAAgAEAAsAAAIFhI+py1wAIfkEBQoAAgAsFQAFAAoACwAAAhWUL4AIvd'
                + 'nciXFSZS6G1HYvMeGyIQUAIfkEBQoAAgAsFwARAAwAAwAAAgWEj6nLBQAh+QQFCgACACwVABUACgALAA'
                + 'ACE5RgmcBqDB2MarZqZcZTU+4sRgEAIfkEBQoAAgAsEAAYAAQACwAAAgWEj6nLXAAh+QQFCgACACwFAB'
                + 'UACgALAAACFZQvgAi92dyJcVJlLobUdi8x4bIhBQAh+QQFCgACACwBABEADAADAAACBYSPqcsFADs=';

    //加载图片 默认无遮罩层 返回标识
    jz.loading = function (mask) {
        var img = document.createElement("img"), sj = jz.random();
        img.src = jz.imgdata;
        img.style.cssText = "position:fixed;top:50%;left:50%;margin:-13px 0 0 -13px;z-index:99999";
        img.id = "jzloading" + sj;
        document.body.appendChild(img);
        if (mask) {
            var mask = document.createElement("div");
            mask.style.cssText = "position:fixed;top:0;left:0;right:0;bottom:0;z-index:99998;opacity: 0.4;background-color:white;filter:alpha(opacity=40)";
            document.body.appendChild(mask);
            mask.id = "jzmask" + sj;
        } return sj
    };

    //关闭加载图片
    jz.loadclose = function (n) { jz("jzloading" + n).remove(); jz("jzmask" + n).remove() };

    /*
    url:"http://netnr.com?id=id",  //url地址
    type:"post",                  //默认get
    async:false,                 //默认异步
    data:"post提交内容",        //post内容
    loading:true,               //加载图片
    dataType:"json/xml",        //默认文本
    success:function(val){},    //返回内容
    error:function(num){}       //错误回调
    complete:function           //完成回调
    */
    jz.ajax = function (ops) {
        var xhr = (window.XMLHttpRequest) ? (new XMLHttpRequest()) : (new ActiveXObject("Microsoft.XMLHTTP")),
        type = ops.type == undefined ? "get" : ops.type.toLowerCase(), //默认GET
        async = ops.async == undefined ? true : ops.async, //默认异步
        data = ops.data == undefined ? null : ops.data,  //发送内容
        dataType = ops.dataType == undefined ? "" : ops.dataType.toLowerCase(),//格式化
        jzload = "jzload" + jz.random();
        if (jz.isKV(data)) { var d = ""; for (var i in data) { d += ('&' + i + '=' + encodeURIComponent(data[i])); } data = d.substr(1); }
        //显示加载图片
        ops.loading && (jzload = jz.loading(1));
        //状态改变事件
        xhr.onreadystatechange = function () {
            if (xhr.readyState == 4) {
                jz.loadclose(jzload);/*删除加载图片*/
                if (xhr.status == 200) {
                    var val = xhr.responseText;
                    /****成功回调****/
                    if (ops.success != undefined) {
                        if (dataType == "json") { try { val = jz.parseJSON(val); } catch (e) { }; ops.success(val) }
                        else if (dataType == "xml") { ops.success(xhr.responseXML) }
                        else { ops.success(val); }
                    }
                }
                else { if (ops.error != undefined) { ops.error(xhr.status) } }
                /****错误回调****/
                if (ops.complete != undefined) { ops.complete() }
                /****完成回调****/
            }
        };
        xhr.open(type, ops.url, async);
        if (type == "get") { xhr.send(data) }
        else {
            xhr.setRequestHeader("Content-Type", "application/x-www-form-urlencoded");
            xhr.send(data)
        }
    };


    //序列化参数
    jz.serialize = function (t) {
        var fs = jz(t).find("*"), i = 0, f = '', len = fs.length;
        for (; i < len; i++) {
            (("text hidden password textarea select-one".indexOf(fs[i].type) > -1 && fs[i].name != "")
            || ("radio checkbox".indexOf(fs[i].type) > -1 && fs[i].checked && fs[i].name != "")) &&
            (f += ("&" + fs[i].name + "=" + encodeURIComponent(fs[i].value)))
        } return f.substr(1)
    };

    window.j = jz

})(window);