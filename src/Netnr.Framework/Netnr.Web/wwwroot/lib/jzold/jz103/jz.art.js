(function (jz) {

    if (typeof (jz) != "function") {return}

    //输出样式
    jz.writestyle('.jzart{color:#333;display:block;min-width:10px;font-size:14px;line-height:150%;position:absolute;white-space:nowrap;background-color:#fff;border:1px solid #b4b4b4;-webkit-border-radius:4px;-moz-border-radius:4px;border-radius:4px;-webkit-box-shadow:0 0 3px #b4b4b4;-moz-box-shadow:0 0 3px #b4b4b4;box-shadow:0 0 3px #b4b4b4;font-family:"Helvetica Neue",Helvetica,Arial;}.jzart a,.jzart input{outline:none;resize:none;}.jzart .jzart-top{height:50px;padding:0 15px;min-width:150px;line-height:50px;white-space:nowrap;border-bottom:1px solid #ddd;}.jzart .jzart-top .jzart-top-title{font-size:15px;font-weight:600;margin-right:30px;white-space:nowrap;}.jzart .jzart-top .jzart-top-cloase{border:none;float:right;color:#b4b4b4;cursor:pointer;font-size:32px;line-height:45px;text-decoration:none;vertical-align:middle;}.jzart .jzart-top .jzart-top-cloase:hover{color:#333;text-decoration:none;}.jzart .jzart-content{_width:20px;margin:20px 25px;word-break:break-all;word-wrap:break-word;}.jzart .jzart-iframe{margin:20px;width:400px;border:none;height:200px;}.jzart .jzart-bottom{min-width:150px;text-align:right;margin:30px 25px 20px 35px;}.jzart .art-ok{color:#fff;font-size:15px;cursor:pointer;line-height:1.4;margin-left:15px;text-align:center;white-space:nowrap;display:inline-block;padding:0.4em 0.85em;text-decoration:none;background-color:#099;border:1px solid transparent;border-radius:3px;-moz-border-radius:3px;-webkit-border-radius:3px;}.jzart .art-ok:link,.jzart .art-ok:visited{color:#fff;background-color:#099;}.jzart .art-ok:hover,.jzart .art-ok:active{color:#fff;text-decoration:none;background-color:#008C8C;}.jzart .art-cancel{color:#333;font-size:15px;cursor:pointer;line-height:1.4;text-align:center;white-space:nowrap;display:inline-block;padding:0.44em 0.88em;text-decoration:none;background-color:#fff;border:1px solid #cccccc;border-radius:3px;-moz-border-radius:3px;-webkit-border-radius:3px;}.jzart .art-cancel:link,.jzart .art-cancel:visited{color:#333;text-decoration:none;background-color:#fff;}.jzart .art-cancel:hover,.jzart .art-cancel:active{color:#333;text-decoration:none;background-color:#f2f2f2;}.jzart .tip-em{width:0;height:0;font-size:0;line-height:0;border-width:9px;position:absolute;border-style:dashed;border-color:transparent;}.jzart .em-top1{left:15px;bottom:-18px;border-top-style:solid;border-top-color:#b4b4b4;}.jzart .em-top2{left:15px;bottom:-16px;border-top-color:#fff;border-top-style:solid;}.jzart .em-right1{top:15px;left:-18px;border-right-style:solid;border-right-color:#b4b4b4;}.jzart .em-right2{top:15px;left:-16px;border-right-color:#fff;border-right-style:solid;}.jzart .em-left1{top:15px;right:-18px;border-left-style:solid;border-left-color:#b4b4b4;}.jzart .em-left2{top:15px;right:-16px;border-left-color:#fff;border-left-style:solid;}.jzart .em-bottom1{top:-18px;left:15px;border-bottom-style:solid;border-bottom-color:#b4b4b4;}.jzart .em-bottom2{top:-16px;left:15px;border-bottom-color:#fff;border-bottom-style:solid;}.jzart-mask{top:0;left:0;right:0;bottom:0;opacity:0.4;position:fixed;background-color:white;filter:alpha(opacity=40);}');

    //弹窗基础仓库
    jz.art = {
        //叠堆起始值
        zindex: 8888,
        //创建
        create: function (e, c) {
            var o = document.createElement(e);
            c != undefined && c != "" && (o.className = c);
            return o;
        },
        //弹出层包
        wrap: function (obj) {
            var art = this.create('div', 'jzart');
            art.id = "jzart" + jz.random();
            if (obj.fixed) { art.style.position = "fixed"; }
            return art;
        },
        //头部
        top: function () {
            var top = this.create('div', 'jzart-top');
            top.id = "jzarttop" + jz.random();
            return top;
        },
        //头部 - 标题
        title: function (obj) {
            var title = this.create('div', 'jzart-top-title');
            title.innerHTML = obj.title == undefined ? "消息" : obj.title;
            return title;
        },
        //头部 - 关闭
        close: function () {
            var close = this.create('a', 'jzart-top-cloase');
            close.href = "javascript:void(0);";
            close.id = "jzarttopclose" + jz.random();
            close.innerHTML = "×";
            close.title = "关闭";
            return close;
        },
        //底部按钮包
        bottom: function () {
            return this.create('div', 'jzart-bottom');
        },
        //底部 - 确定按钮
        ok: function (obj) {
            var ok = this.create('a', 'art-ok');
            ok.href = "javascript:void(0);";
            ok.id = "jzartok" + jz.random();
            ok.innerHTML = obj.okValue == undefined ? "确定" : obj.okValue;
            return ok;
        },
        //底部 - 取消按钮
        cancel: function (obj) {
            var cancel = this.create('a', 'art-cancel');
            cancel.href = "javascript:void(0);";
            cancel.id = "jzartcancel" + jz.random();
            cancel.innerHTML = obj.cancelValue == undefined ? "取消" : obj.cancelValue;
            return cancel;
        },
        //文本内容
        content: function (obj) {
            var content = this.create('div', 'jzart-content');
            obj.width != undefined && (content.style.minWidth = obj.width + "px");
            content.innerHTML = obj.content == undefined ? "" : obj.content;
            return content;
        },
        //弹窗页面
        iframe: function (obj) {
            var iframe = this.create('iframe', 'jzart-iframe');
            iframe.src = obj.src;
            iframe.frameBorder = "0";
            iframe.scrolling = obj.scrolling ? "auto" : "no";
            if (typeof obj.width === "number") { iframe.style.width = obj.width + "px"; }
            if (typeof obj.height === "number") { iframe.style.height = obj.height + "px"; }
            return iframe;
        },
        //居中
        center: function (t) {
            if (t.style.position == "fixed") {
                t.style.top = (jz.visualH(document) / 2) - (t.offsetHeight / 2) + "px";
                t.style.left = (jz.visualW(document) / 2) - (t.offsetWidth / 2) + "px";
            } else {
                t.style.top = (jz.visualH(document) / 2) + jz.scrollH(document) - (t.offsetHeight / 2) + "px";
                t.style.left = (jz.visualW(document) / 2) + jz.scrollW(document) - (t.offsetWidth / 2) + "px";
            }
        },
        //遮罩层
        mask: function () {
            var mask = this.create('div', 'jzart-mask');
            return mask;
        },
        msgId: "",
        tipId: ""
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
                });
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
            width: obj.width,
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
        jz.each("top right left bottom".split(" "), function () {
            if (obj.align == this) { align = this; return false; }
        });

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
                x = jz.xy(tar).sLeft, y = jz.xy(tar).sTop;

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
   
})(j)