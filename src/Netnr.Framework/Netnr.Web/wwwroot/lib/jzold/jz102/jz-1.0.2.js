/*
 *  周华
 *  2015-12-8
 *  http://jz.netnr.com
 *  Version:1.0.2
 *  说明：添加处理方法及返回jz对象 参照jQuery   该封装实现常用的方法级弹窗。

 *  只需引入js文件 无需引入样式jz-1.0.2.css

 *  添加常用事件处理、转码、Cookie、载入文件（css/js）、拖动、加载提示、Ajax、Placeholder模拟等。
 *  普通消息提示、模拟confirm、弹出iframe、tip小提示

 */


(function (window) {

    var jz = function (t) { return new jz.prototype.init(t); };

    /* IE8- */
    jz.OldIE = function () { return typeof document.createElement == "object" || false };

    //字符串两边去空 ie8-
    if (jz.OldIE()) {
        String.prototype.trim = function () { return this.replace(/(^\s+)|(\s+$)/g, ""); };
    };

    //截断 显示长度 末尾追加
    String.prototype.maxlen = function (len, s) {
        s = arguments.length == 2 ? s : "";
        return this.length > len ? this.substring(0, len) + s : this;
    };

    //截断 显示中文长度 （字符串2个视为一个中文长度）
    String.prototype.maxlencn = function (len, s) {
        if (this.length <= len) { return this; } else {
            var i = sumN = 0, s = arguments.length == 2 ? s : "";
            for (; i < this.substring(0, len).length; i++) {
                if (this.charCodeAt(i) < 127) { sumN += 1; }
            }
            return len + sumN - 1 > this.length ? this : this.substring(0, len + sumN - 1) + s;
        }
    };

    /*添加事件处理程序*/
    jz.addHandler = function (type, fn, obj) {
        if (obj.addEventListener) { obj.addEventListener(type, fn, false); }
        else if (obj.attachEvent) {
            obj['e' + type + fn] = fn;/*对象某属性等于该处理程序 this 指向对象本身*/
            obj[type + fn] = function () { obj['e' + type + fn](); };
            obj.attachEvent("on" + type, obj[type + fn]);
        } else { obj["on" + type] = fn; }
    };

    /*移除事件处理程序*/
    jz.removeHandler = function (type, fn, obj) {
        if (obj.removeEventListener) { obj.removeEventListener(type, fn, false); }
        else if (obj.detachEvent) {
            obj.detachEvent("on" + type, obj[type + fn]); obj[type + fn] = null;
        }
    };

    /*DOM加载完成事件*/
    jz.ready = function (fn) {
        if (window.addEventListener) {
            window.addEventListener("DOMContentLoaded", function () { fn() }, false);
        } else {
            document.attachEvent("onreadystatechange", function () {
                if (document.readyState === "complete") { setTimeout(function () { fn() }, 1); }
            })
        }
    };

    //为jz对象添加属性方法
    jz.fn = jz.prototype = {
        init: function (t) { this[0] = typeof (t) === "object" ? t : document.getElementById(t); return this; },
        //添加处理事件 类型、函数
        addHandler: function (type, fn) { jz.addHandler(type, fn, this[0]); return this },
        //移除处理事件 类型、函数
        removeHandler: function (type, fn) { jz.removeHandler(type, fn, this[0]); return this },
        //添加对象或HTML
        append: function (t) {
            if (typeof t === "string") {
                var tmp = document.createElement("div"); tmp.innerHTML = t; t = tmp;
            } this[0].appendChild(t); return this
        },
        //移除对象 倒计时秒、回调函数、指定父对象移除 默认 document
        remove: function (time, fn, pT) { jz.remove(this[0], time, fn, pT); return this },
        //点击空白事件（不包括自身）
        blankClick: function (type) { jz.blankClick(this[0], type); return this; },
        //拖动 指定拖动触发对象 默认自身
        drag: function (dragT) { jz.drag(this[0], dragT); return this },

        //文本值改变事件
        input: function (fn) {
            if (jz.OldIE()) {
                var that = this[0];
                this[0].attachEvent("onpropertychange", function () {
                    if (event.propertyName.toLowerCase() == "value") { fn.apply(that, arguments); }
                });
            } else { jz.addHandler("input", fn, this[0]); }
            return this;
        },

        //取值/赋值
        val: function (value) { return arguments.length ? this[0].value = value : this[0].value; },
        //是否为空
        isEmpty: function (s) { return this[0].value.trim() == "" ? true : false; },
        //显示 默认 display="" 可传block等
        show: function (s) { this[0].style.display = arguments.length ? s : ""; return this; },
        //隐藏
        hide: function () { this[0].style.display = "none"; return this; },
        //html取值、赋值
        html: function (s) { return arguments.length ? this[0].innerHTML = s : this[0].innerHTML; },
        //text取值、赋值
        text: function (s) {
            arguments.length && (jz.OldIE() ? this[0].innerText = s : this[0].textContent = s);
            return jz.OldIE() ? this[0].innerText : this[0].textContent;
        },
        //对象集
        gebtn: function (f) { return this[0].getElementsByTagName(f); },
        //特性值
        attr: function (name, value) {
            arguments.length == 2 && (this[0].setAttribute(name, value));
            return this[0].getAttribute(name);
        },
        //模拟 placeholder
        hint: function (text) { jz.hint(text, this[0]); return this }
    };

    jz.prototype.init.prototype = jz.prototype;

    //遍历集 第一个参数索引 第二个值 return false 跳出循环
    jz.each = function (obj, callback) {
        for (var i = 0; i < obj.length; i++) {
            if (callback.call(obj[i], i, obj[i]) == false) { break; };
        }   //this指向obj[i]; i是第一个参数 obj[i]第二个 ...
    };

    //添加处理事件
    jz.each(("blur focus focusin focusout load resize scroll unload click dblclick "
		   + "mousedown mouseup mousemove mouseover mouseout mouseenter mouseleave "
		   + "change select submit keydown keypress keyup error contextmenu").split(" ")
		   , function (i, name) {
		       jz.fn[name] = function (fn) {
		           jz.addHandler(name, fn, this[0]); return this;
		       }
		   }
		);


    /*jz对象 扩展*/

    //获取event 在IE里面event是window对象的属性
    jz.getEvent = function (e) { return window.event || e; };

    //获取event目标
    jz.getTarget = function (e) { return jz.getEvent(e).target || jz.getEvent(e).srcElement; };

    //阻止事件冒泡
    jz.stopEvent = function (e) {
        if (e && e.stopPropagation) { e.stopPropagation(); } else { window.event.cancelBubble = true; }
    };

    //阻止浏览器默认行为
    jz.stopDefault = function (e) {
        if (e && e.preventDefault) { e.preventDefault(); } else { window.event.returnValue = false; }
    };

    //按键ASCII值
    jz.keys = function (e) {
        return jz.getEvent(e).keyCode || jz.getEvent(e).which || jz.getEvent(e).charCode;
    };

    //保存cookie 名 内容 天(默认关闭浏览器过期) escape编码
    jz.cookieSave = function (n, txt, day) {
        var c = n + "=" + escape(txt) + ";path=/;";
        if (day != undefined) {
            var d = new Date(); d.setTime(d.getTime() + day * 24 * 60 * 60 * 1000); c += "expires=" + d.toGMTString();
        } document.cookie = c;
    };

    //读取cookie 名
    jz.cookieGet = function (n) {
        var val = '', s = document.cookie;
        if (s.indexOf(n) > -1) {
            var cook = s.split(';'), i = 0;
            for (; i < cook.length; i++) {
                if ((cook[i].split('=')[0]).trim() == n) {
                    val = cook[i].split('=')[1];
                    break;
                }
            }
        } return unescape(val);
    };

    //删除cookie 名
    jz.cookieDel = function (n) {
        var d = new Date(); d.setTime(d.getTime() - 1);
        document.cookie = n + "=0;path=/;expires=" + d.toGMTString();
    };

    //转码 To Unicode
    jz.unicode = function (s) {
        var val = "", i = 0, c;
        for (; i < s.length; i++) {
            c = s.charCodeAt(i).toString(16);
            while (c.length < 4) { c = '0' + c; } val += '\\u' + c;
        }; return val;
    };

    //转码 Unicode To STR-CN
    jz.ununicode = function (s) { return eval("'" + s + "'"); };//return unescape(str.replace(/\u/g, "%u"))


    //转码 To ASCII
    jz.ascii = function (s) {
        var val = "", i = 0;
        for (; i < s.length; i++) { val += "&#" + s[i].charCodeAt() + ";"; }
        return val;
    };

    //转码 ASCII To STR-CN
    jz.unascii = function toAsciiUn(s) {
        var val = "", strs = s.match(/&#(\d+);/g);
        if (strs != null) {
            for (var i = 0; i < strs.length; i++) {
                val += String.fromCharCode(strs[i].replace(/[&#;]/g, ''));
            }
        } return val;
    };

    //当前时间 自定义格式 yyyy-MM-dd HH:mm:ss
    jz.time = function (format) {
        function t(n) { return n < 10 ? "0" + n : n; }
        var d = new Date(),
            y = d.getFullYear(),
            m = t(d.getMonth() + 1),
            day = t(d.getDate()),
            h = t(d.getHours()),
            min = t(d.getMinutes()),
            s = t(d.getSeconds()),
            f = d.getMilliseconds();
        return !arguments.length ? (y + "-" + m + "-" + day + " " + h + ":" + min + ":" + s) :
                format.replace("yyyy", y).replace("MM", m).replace("dd", day).replace("HH", h).replace("mm", min).replace("ss", s).replace("ff", f);
    };

    //加载样式 css路径 默认删除存在
    jz.loadcss = function (href, del) {
        if (arguments.length == 1 || del) {
            var ls = document.getElementsByTagName("LINK"), i = 0;
            for (; i < ls.length; i++) {
                if (ls[i].href.indexOf(href) > -1) {
                    document.getElementsByTagName("HEAD")[0].removeChild(ls[i]); break;
                }
            }
        }

        var s = document.createElement("LINK");
        s.href = href;
        s.rel = "stylesheet";
        document.getElementsByTagName("HEAD")[0].appendChild(s);
    };

    //加载js 路径 加载完成回调 （删除已存在但有缓存）
    jz.loadjs = function (src, fn) {
        jz.deljs(src);
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

    //删除js indexOf(src) 检索匹配 (有缓存)
    jz.deljs = function (src) {
        var scrs = document.getElementsByTagName("SCRIPT"), i = 0;
        for (; i < scrs.length; i++) {
            if (scrs[i].src.indexOf(src) > -1) {
                document.getElementsByTagName("HEAD")[0].removeChild(scrs[i]); break;
            }
        }
    };

    //随机数字 长度（默认4位）1到15
    jz.random = function (len) {
        len = arguments.length ? len > 15 ? 15 : len : 4;
        return Math.random().toString().substr(2, len);
    };

    //删除对象 t：移除对象   time：倒计时(秒,number类型 占位传 null)删除  fn：移除回调函数  pT：父对象
    jz.remove = function (t, time, fn, pT) {
        if (typeof time === "number") { setTimeout(function () { re(); }, time * 1000) } else { re(); }
        function re() {
            t = typeof t === "object" ? t : document.getElementById(t);
            try {
                if (pT == undefined) { document.body.removeChild(t); } else { pT.removeChild(t); }
                if (fn != undefined) { fn(); }
            } catch (e) { }
        }
    };

    //点击某对象的空白地方 事件 默认移除
    jz.blankClick = function (t, type) {
        setTimeout(function () {
            jz(t).click(function (event) { jz.stopEvent(event); });
            jz.addHandler("click", function () {
                switch (type) {
                    case "hid": jz(t).hide(); break;
                    default: jz(t).remove();
                }
                jz.removeHandler("click", arguments.callee, document);
            }, document);
        }, 200)
    }

    //拖动
    jz.drag = function (t, dragT) {
        var disX = dixY = 0, id = jz(t)[0], dragId = dragT != undefined ? jz(dragT)[0] : id;
        dragId.style.cursor = "move";
        dragId.onmousedown = function (e) {
            disX = jz.getEvent(e).clientX - id.offsetLeft;
            disY = jz.getEvent(e).clientY - id.offsetTop;
            document.onmousemove = function (e) {
                var x = jz.getEvent(e).clientX - disX,
                    y = jz.getEvent(e).clientY - disY,
                    maxX = jz.visualW() - id.offsetWidth + jz.scrollW(),
                    maxY = jz.visualH() - id.offsetHeight + jz.scrollH();
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
            return false
        };
    };

    //自身src
    jz.baseUrl = function () {
        var s = document.getElementsByTagName("SCRIPT"), c = s[s.length - 1];
        return c.src.substr(0, c.src.lastIndexOf('/') + 1);
    };
    jz.baseUrlState = jz.baseUrl();

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
    //加载图片
    jz.loading = function (mask) {
        var img = document.createElement("img"), sj = jz.random();
        img.src = jz.imgdata;
        img.style.cssText = "position:fixed;top:50%;left:50%;margin:-13px 0 0 -13px;z-index:" + jz.art.zindex + 999;
        img.id = "jzloading" + sj;
        document.body.appendChild(img);
        if (mask) {
            var mask = jz.art.mask();
            mask.style.zIndex = jz.art.zindex + 998;
            document.body.appendChild(mask);
            mask.id = "jzartmask" + sj;
        } return sj;
    };
    jz.loadingclose = function (n) { jz("jzloading" + n).remove(); jz("jzartmask" + n).remove(); };


    /*
	url:"http://netnr.com?id=id",  //url地址
	type:"post",                  //默认get
	async:false,                 //默认异步
	data:"post提交内容",        //post内容
	loading:true,               //加载图片
	format:"json/xml",          //默认文本
	success:function(val){},    //返回内容
	error:function(num){}       //错误回调
	complete:function           //完成回调
	*/
    jz.Ajax = function (ops) {
        var xhr = (window.XMLHttpRequest) ? (new XMLHttpRequest()) : (new ActiveXObject("Microsoft.XMLHTTP")),
		type = ops.type == undefined ? "get" : ops.type, //默认GET
		async = ops.async == undefined ? true : ops.async, //默认异步
		data = ops.data == undefined ? null : ops.data,  //发送内容
		format = ops.format == undefined ? "" : ops.format,//格式化
        jzload = "jzload" + jz.random();
        //显示加载图片
        if (ops.loading) { jzload = jz.loading(1); }
        //状态改变事件
        xhr.onreadystatechange = function () {
            if (xhr.readyState == 4) {
                jz.loadingclose(jzload);/*删除加载图片*/
                if (xhr.status == 200) {
                    var val = xhr.responseText;
                    /****成功回调****/
                    if (ops.success != undefined) {
                        if (format.toLowerCase() == "json") {
                            try {
                                val = eval("(" + val + ")");
                                if (jz.OldIE()) { val = typeof (val) === "object" ? val : eval("(" + val + ")"); }
                                else { val = typeof (val) === "object" ? val : JSON.parse(val); }
                            } catch (e) { };
                            ops.success(val);
                        }
                        else if (format.toLowerCase() == "xml") { ops.success(xhr.responseXML); }
                        else { ops.success(val); }
                    }
                }
                else { if (ops.error != undefined) { ops.error(xhr.status) } }
                /****错误回调****/
                if (ops.complete != undefined) { ops.complete(); }
                /****完成回调****/
            }
        };
        xhr.open(type, ops.url, async);
        if (type.toLowerCase() == "get") { xhr.send(data); }
        else {
            xhr.setRequestHeader("Content-Type", "application/x-www-form-urlencoded");
            xhr.send(data);
        }
    };

    //获取水平滚动条宽度
    jz.scrollW = function () { return document.documentElement.scrollLeft || document.body.scrollLeft; };

    //获取垂直滚动条高度
    jz.scrollH = function () { return document.documentElement.scrollTop || document.body.scrollTop; };

    //可视宽度
    jz.visualW = function () { return document.documentElement.clientWidth || document.body.clientWidth; };

    //可视高度
    jz.visualH = function () { return document.documentElement.clientHeight || document.body.clientHeight; };

    //模拟 placeholder
    jz.hint = function (text, t) {
        var input = typeof (t) === "object" ? t : document.getElementById(t),

        label = document.createElement("label");
        label.id = "jzhint" + jz.random();
        input.setAttribute("hintid", label.id);//对象设置特性值 hintid 等于 提示文字Id
        label.innerHTML = text;
        document.body.appendChild(label);
        label["_selfHeight"] = Math.min(label.offsetHeight, 20);

        jz(label).mouseover(autoxy);
        jz(label).click(function () { input.focus(); });
        label.onselectstart = function () { return false };
        label.style.cssText = "position:absolute;-moz-user-select:none;color:#aaa;cursor:text;display:none";
        setTimeout(function () { autoxy(); }, 10);

        jz(input).input(function () { label.style.display = this.value.length ? "none" : "block"; });
        if (window.addEventListener) { window.addEventListener("resize", autoxy, false); }
        else { window.attachEvent("onresize", autoxy); }

        setTimeout(function () { label.style.display = input.value.length ? "none" : "block"; }, 800);

        function autoxy() {
            label.style.top = (jz.xypx(input).top + (input.offsetHeight - label["_selfHeight"]) / 2) + "px";
            label.style.left = (jz.xypx(input).left + 6) + "px";
        }
    };


    //获取对象 (左/上)边距 页面加载完成后获取确保精准（可用setTimeout）
    jz.xypx = function (t) {
        t = typeof (t) === "object" ? t : document.getElementById(t);
        var x, y;

        if (document.body.getBoundingClientRect) {
            var html = document.documentElement, body = document.body, pos = t.getBoundingClientRect(),
                ct = html.clientTop || body.clientTop,
                cl = html.clientLeft || body.clientLeft;
            x = pos.left + jz.scrollW() - cl,
            y = pos.top + jz.scrollH() - ct;
        } else {
            function top(t) {
                var offset = t.offsetTop;
                if (t.offsetParent != null) offset += arguments.callee(t.offsetParent);
                return offset;
            }
            function left(t) {
                var offset = t.offsetLeft;
                if (t.offsetParent != null) offset += arguments.callee(t.offsetParent);
                return offset;
            }
            x = left(t);
            y = top(t);
        }
        return { top: y, left: x };
    };

    //置顶
    jz.scrollToH = function (time) {
        time == undefined && (time = 500);
        var one = arguments.length == 1 ? jz.scrollH() / time * 20 : arguments[1];
        time = arguments.length == 1 ? time / 20 : time;
        time = jz.OldIE() ? time - 2 : time - 0.4;
        window.scrollTo(0, one * time);
        time > 0 && (setTimeout(function () { jz.scrollToH(time, one); }, 1));
    }





    /******************* 弹窗 *******************************/

    //载入弹窗样式    
    jz.loadcss(jz.baseUrlState + "jz-1.0.2.css");
    //弹窗基础仓库
    jz.art = {
        //弹出层包
        wrap: function (obj) {
            var art = document.createElement("div");
            art.id = "jzart" + jz.random();
            art.className = "jzart";
            if (obj.fixed) { art.style.position = "fixed"; }
            return art;
        },
        //头部
        top: function () {
            var top = document.createElement("div");
            top.id = "jzarttop" + jz.random();
            top.className = "jzart-top";
            return top;
        },
        //头部 - 标题
        title: function (obj) {
            var title = document.createElement("div");
            title.className = "jzart-top-title";
            title.innerHTML = obj.title == undefined ? "消息" : obj.title;
            return title;
        },
        //头部 - 关闭
        close: function () {
            var close = document.createElement("a");
            close.href = "javascript:void(0);";
            close.id = "jzarttopclose" + jz.random();
            close.className = "jzart-top-cloase";
            close.innerHTML = "×";
            close.title = "关闭";
            return close;
        },
        //底部按钮包
        bottom: function () {
            var bottom = document.createElement("div");
            bottom.className = "jzart-bottom";
            return bottom;
        },
        //底部 - 确定按钮
        ok: function (obj) {
            var ok = document.createElement("a");
            ok.href = "javascript:void(0);";
            ok.id = "jzartok" + jz.random();
            ok.innerHTML = obj.okValue == undefined ? "确定" : obj.okValue;
            ok.className = "art-ok";
            ok.hideFocus = true;
            return ok;
        },
        //底部 - 取消按钮
        cancel: function (obj) {
            var cancel = document.createElement("a");
            cancel.href = "javascript:void(0);";
            cancel.id = "jzartcancel" + jz.random();
            cancel.innerHTML = obj.cancelValue == undefined ? "取消" : obj.cancelValue;
            cancel.className = "art-cancel";
            cancel.hideFocus = true;
            return cancel;
        },
        //文本内容
        content: function (obj) {
            var content = document.createElement("div");
            content.className = "jzart-content";
            content.innerHTML = obj.content == undefined ? "" : obj.content;
            return content;
        },
        //弹窗页面
        iframe: function (obj) {
            var iframe = document.createElement("iframe");
            iframe.src = obj.src;
            iframe.className = "jzart-iframe";
            iframe.frameBorder = "0";
            iframe.scrolling = obj.scrolling ? "auto" : "no";
            if (typeof obj.width === "number") { iframe.style.width = obj.width + "px"; }
            if (typeof obj.height === "number") { iframe.style.height = obj.height + "px"; }
            return iframe;
        },
        //居中
        center: function (t) {
            if (t.style.position == "fixed") {
                t.style.top = (jz.visualH() / 2) - (t.offsetHeight / 2) + "px";
                t.style.left = (jz.visualW() / 2) - (t.offsetWidth / 2) + "px";
            } else {
                t.style.top = (jz.visualH() / 2) + jz.scrollH() - (t.offsetHeight / 2) + "px";
                t.style.left = (jz.visualW() / 2) + jz.scrollW() - (t.offsetWidth / 2) + "px";
            }
        },
        //遮罩层
        mask: function () {
            var mask = document.createElement("div");
            mask.className = "jzart-mask";
            return mask;
        },
        msgId: "",
        tipId: "",
        zindex: 8888
    };


    /*  弹窗实现 参数列表：
    
     *  title：标题    text/html（jz.msg默认无）
     *  content：文本信息    text/html（jz.msg、jz.confirm 显示的文本或html，jz.iframe不传）
     *  time：倒计时关闭 单位：秒     number（jz.msg默认4秒，jz.confirm、jz.iframe默认不关闭）
     *  blank：点击空白关闭    bool（默认false，此关闭不触发弹窗关闭事件 有遮罩层失效）     
     *  mask：遮罩层    bool（默认 false）
     *  fixed：绝对位置  bool（position:fixed）
     *  single：只弹出一个    bool
     *  drag：拖动     bool（jz.msg默认false，jz.confirm、jz.iframe 默认true）     
     *  ok：确定回调     function（确定回调）
     *  okValue：确定按钮文本      text（默认“确定”，有确定按钮则生效）     
     *  cancel：取消回调  function/bool（取消回调/不显示取消按钮）     
     *  cancelValue：取消按钮文本      text（默认“取消”,有取消按钮则生效）     
     *  close；窗口关闭回调    function/bool（关闭回调/不显示，jz.confirm没有，合并为cancel事件）
     *  src：弹窗地址    text（jz.msg、jz.confirm不传）
     *  width：弹窗宽度 单位：px     number（默认 400）
     *  height: 弹窗高度 单位：px      number（默认 200）
     *  scrolling: 弹窗滚动条    bool（默认false）
     */
    jz.msg = function (obj) {
        //外包
        var msg = jz.art.wrap(obj), id = msg.id;
        //弹窗堆叠顺序
        msg.style.zIndex = jz.art.zindex;
        //活动窗口 顶部显示
        jz(msg).mousedown(function () { jz.art.zindex += 1; this.style.zIndex = jz.art.zindex; });
        //头部 有标题的前提
        if (obj.title != undefined) {
            //头部包
            var top = jz.art.top(),
            //标题
            title = jz.art.title(obj);
            //显示关闭按钮
            if (obj.close != false) {
                var close = jz.art.close();
                top.appendChild(close);
                //关闭回调
                jz(close).click(function () {
                    if ((obj.close == undefined || obj.close() != false)) {
                        jz(id).remove(); jz(id + "mask").remove();
                    }
                })
            }
            //载入头部
            top.appendChild(title);
            msg.appendChild(top);
        }
        //载入内容 没传入 src的前提
        if (obj.content != undefined && obj.src == undefined) {
            var content = jz.art.content(obj);
            msg.appendChild(content);
        }
        //载入iframe
        if (obj.src != undefined) {
            var iframe = jz.art.iframe(obj);
            msg.appendChild(iframe);
            //关闭按钮的id 与 iframe的name属性值一样 
            //子页面调用关闭 获取window.name值 模拟点击父页面id等于window.name的关闭按钮 触发关闭回调
            if (close) { close.id = iframe.name = "jzartiframe" + jz.random(); }
        }
        //底部按钮包 有标题的前提 没传入 src
        if (obj.title != undefined && obj.src == undefined) {
            var bottom = jz.art.bottom();
            //显示取消按钮
            if (obj.cancel != false) {
                //取消按钮
                var cancel = jz.art.cancel(obj);
                bottom.appendChild(cancel);
                //取消回调
                jz(cancel).click(function () {
                    if ((obj.cancel == undefined || obj.cancel() != false)) {
                        jz(id).remove(); jz(id + "mask").remove();
                    }
                })
            }
            //确定按钮
            var ok = jz.art.ok(obj);
            bottom.appendChild(ok);
            //确定回调
            jz(ok).click(function () {
                if ((obj.ok == undefined || obj.ok() != false)) {
                    jz(id).remove(); jz(id + "mask").remove();
                }
            });
            //载入底部按钮包
            msg.appendChild(bottom);
        }
        //显示遮罩层
        if (obj.mask) {
            var mask = jz.art.mask();
            mask.id = id + "mask";
            mask.style.zIndex = jz.art.zindex - 1;
            document.body.appendChild(mask);
        }
        //弹出一个
        if (obj.single != false) { jz(jz.art.msgId).remove(); jz(jz.art.msgId + "mask").remove(); jz.art.msgId = id; }
        //显示弹窗
        document.body.appendChild(msg);
        //默认4秒关闭
        if (obj.time == undefined) { jz(id).remove(4); jz(id + "mask").remove(4); }
        else { if (obj.time != 0) { jz(id).remove(obj.time); jz(id + "mask").remove(obj.time); } }
        //点击空白关闭
        if (obj.blank && !obj.mask) { jz(id).blankClick() }
        //拖动
        if (obj.drag) { if (obj.title != undefined) { jz(id).drag(top); } else { jz(id).drag(); } }
        //动态居中
        jz.art.center(msg);
        if (window.addEventListener) {
            window.addEventListener("resize", function () { jz.art.center(msg) }, false);
        } else { window.attachEvent("onresize", function () { jz.art.center(msg) }); }
        return id;
    };

    /*  confirm  */
    jz.confirm = function (obj) {
        return jz.msg({
            title: obj.title == undefined ? "消息" : obj.title,
            content: obj.content,
            time: obj.time == undefined ? 0 : obj.time,
            single: obj.single == undefined ? false : obj.single,
            drag: obj.drag == undefined ? true : obj.drag,
            blank: obj.blank,
            fixed: obj.fixed,
            mask: obj.mask,
            ok: obj.ok,
            okValue: obj.okValue,
            cancel: obj.cancel,
            cancelValue: obj.cancelValue,
            close: obj.cancel
        })
    };

    /*  子页面回调 iframe关闭事件 子页面传入window.name  title:false则失效 */
    jz.closeback = function (name) { try { jz(name)[0].click(); } catch (e) { } };
    /*  iframe  */
    jz.iframe = function (obj) {
        return jz.msg({
            title: obj.title == false ? undefined : obj.title == undefined ? "窗口" : obj.title,
            src: obj.src,
            time: obj.time == undefined ? 0 : obj.time,
            single: obj.single,
            drag: obj.drag == undefined ? true : obj.drag,
            blank: obj.blank,
            fixed: obj.fixed,
            mask: obj.mask,
            width: obj.width,
            height: obj.height,
            scrolling: obj.scrolling,
            close: obj.close
        })
    };

    /*  小提示 tip  参数列表：
     *  target：id/object（提示目标id或对象）
     *  content：提示信息    text/html
     *  single：只弹出一个    bool
     *  time：倒计时关闭 单位：秒     number（默认不关闭）
     *  blank：点击空白关闭    bool（默认false）
     *  focus：焦点选中目标    bool（默认false）
     */
    jz.tip = function (obj) {
        //提示目标
        var tar = jz(obj.target)[0],
        //提示包
        tip = jz.art.wrap(obj), id = tip.id;
        //弹窗堆叠顺序
        tip.style.zIndex = jz.art.zindex;
        //内容
        var content = jz.art.content(obj),
            sj1 = document.createElement("em"),//三角符号
            sj2 = document.createElement("em"), align = 'bottom';//方向        
        jz.each("top right left bottom".split(" "), function (i, name) {
            if (obj.align == name) { align = name; return false; }
        })

        tip.appendChild(content);
        tip.appendChild(sj1);
        tip.appendChild(sj2);
        sj1.className = 'tip-em em-' + align + '1';
        sj2.className = 'tip-em em-' + align + '2';

        //活动窗口 顶部显示
        jz(tip).mousedown(function () { jz.art.zindex += 1; this.style.zIndex = jz.art.zindex; });
        //载入提示
        document.body.appendChild(tip);
        //弹出一个
        if (obj.single != false) { jz(jz.art.tipId).remove(); jz.art.tipId = id; }
        //倒计时关闭
        if (obj.time != undefined && obj.time != 0) { jz(id).remove(obj.time); }
        //点击空白关闭
        if (obj.blank) { jz(id).blankClick() }
        //选中焦点
        if (obj.focus) { tar.focus() }

        function autowh(t) {
            var oH = tar.offsetHeight, oW = tar.offsetWidth,
                x = jz.xypx(tar).left, y = jz.xypx(tar).top;

            switch (obj.align) {
                case "top":
                    t.style.top = y - tip.offsetHeight - 13 + "px";
                    t.style.left = (oW > 40 ? x : x - 25 + (oW / 2)) + "px";
                    break;
                case "right":
                    tip.style.top = oH > 40 ? y + "px" : y - 25 + (oH / 2) + "px";
                    tip.style.left = x + oW + 13 + "px";
                    break;
                case "left":
                    tip.style.top = oH > 40 ? y + "px" : y - 25 + (oH / 2) + "px";
                    tip.style.left = x - tip.offsetWidth - 13 + "px";
                    break;
                default: //默认下方
                    tip.style.top = y + oH + 13 + "px";
                    tip.style.left = oW > 40 ? x + "px" : x - 25 + (oW / 2) + "px";
                    break;  //对象小于50 三角符号指向对象中间
            }
        }; autowh(tip);

        if (window.addEventListener) {
            window.addEventListener("resize", function () { autowh(tip) }, false);
        } else { window.attachEvent("onresize", function () { autowh(tip) }); }

        return id;
    };

    window.jz = jz;

})(window)