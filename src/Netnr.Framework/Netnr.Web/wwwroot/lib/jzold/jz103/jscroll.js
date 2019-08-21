/*
 * 2016-2-25
 * netnr.com
 * 周华
 */

(function (window) {
    var jc = function (id) { return document.getElementById(id) }
    jc.on = function (type, fn, obj) {
        if (obj.addEventListener) {
            obj.addEventListener(type, fn, false);
            (type == "mousewheel" && typeof (onmousewheel) == "undefined") && obj.addEventListener("DOMMouseScroll", fn, false);
        }
        else if (obj.attachEvent) { obj.attachEvent("on" + type, function () { fn.call(obj, arguments) }); }
    }
    jc.event = function (e) { return window.event || e }
    jc.init = function (ops) { return new init(ops) }
    //初始化
    function init(ops) {
        var scrollId = ops.id,//id
            scrollBarW = ops.barwidth == undefined ? 10 : ops.barwidth,//滚动条宽度
            scrollBarH = ops.barheight == undefined ? 60 : ops.barheight,//滚动条高度
            scrollBarC = ops.barcolor == undefined ? "#099" : ops.barcolor,//滚动条颜色
            scrollBarB = ops.barbgcolor == undefined ? "#fff" : ops.barbgcolor,//滚动条背景色
            scrollH = ops.scrollheight == undefined ? 80 : ops.scrollheight,//每次滚动高度
            visualH = jc(scrollId).offsetHeight,//自身可见高度
            contentH = jc(scrollId).scrollHeight,//内容真实高度
            contentMaxH = contentH - visualH,//内容可滚动高度
            barMaxH = visualH - scrollBarH;//滚动条可滚动高度

        //前提
        jc(scrollId).style.position = "relative";
        if (navigator.userAgent.match(/(iPhone|iPod|Android|ios)/i)) {
            scrollBarW = 0; jc(scrollId).style.overflow = "auto";
        } else { jc(scrollId).style.overflow = "hidden"; }

        //添加工具
        jc(scrollId).innerHTML = '<div class="jscrollcontent" style="top: 0;left: 0;bottom: 0;right: ' + scrollBarW + 'px;position: absolute;">'
                               + jc(scrollId).innerHTML + '</div><div class="jscrollbox" style="top: 0;right: 0;bottom: 0;position: absolute;width:'
                               + scrollBarW + 'px;background-color:' + scrollBarB + '">'
                               + '<div class="jcscrollbar" style="top: 0;width:100%;position: absolute;background-color: ' + scrollBarC + ';height:' + scrollBarH + 'px;">'
                               + '</div></div>';

        //鼠标拖动
        jc.on("mousedown", function (e) {
            var target = jc.event(e).srcElement || jc.event(e).target;
            //滚动条对象
            if (target.className == "jcscrollbar") {
                var xdY = jc.event(e).clientY - target.offsetTop;
                document.onmousemove = function (e) {
                    var y = jc.event(e).clientY - xdY;
                    y <= 0 && (y = 0);
                    y > barMaxH && (y = barMaxH);
                    target.style.top = y + "px";
                    contentH > visualH && (target.parentNode.previousSibling.style.top = -(contentH - visualH) * (y / barMaxH) + "px");
                    return false;
                }
                document.onmouseup = function () {
                    document.onmousemove = null;
                    document.onmouseup = null;
                    this.releaseCapture && this.releaseCapture()
                }
                this.setCapture && this.setCapture();
            }
        }, jc(scrollId));

        //鼠标点击
        jc.on("click", function (e) {
            var target = jc.event(e).srcElement || jc.event(e).target;
            //滚动条框对象
            if (target.className == "jscrollbox") {
                var xdY = jc.event(e).clientY - target.offsetTop,
                    top = Math.abs(target.getBoundingClientRect().top - xdY);//距离顶部的距离
                top <= 0 && (top = 0);
                top > barMaxH && (top = barMaxH);
                target.firstChild.style.top = top + "px";
                contentH > visualH && (target.previousSibling.style.top = -(contentH - visualH) * (top / barMaxH) + "px");
            }
        }, jc(scrollId));

        //滚轮
        jc.on("mousewheel", function (e) {
            if (contentH > visualH) {
                var ccH = parseInt(this.firstChild.style.top.replace('px', '')),//当前内容高度
                    cbH = Math.abs(parseInt(this.lastChild.firstChild.style.top.replace('px', '')));//当前滚动条高度
                //向下滚动
                if (jc.event(e).wheelDelta == -120 || jc.event(e).detail == 3) {
                    jc.scrollrun("box", this.firstChild, 1, scrollH, contentMaxH);
                    var bh = (ccH - scrollH) / -contentMaxH;//滚动比例
                    bh >= 1 && (bh = 1);
                    var callH = (this.offsetHeight - scrollBarH) * bh - cbH;//滚动条滚动高度
                    jc.scrollrun("bar", this.lastChild.firstChild, 1, callH, barMaxH);
                } else {
                    jc.scrollrun("box", this.firstChild, 0, scrollH, contentMaxH);
                    var hh = (ccH + scrollH) >= 0 ? -0 : ccH + scrollH;
                    var bh = hh / -contentMaxH;//滚动比例
                    var callH = cbH - (this.offsetHeight - scrollBarH) * bh
                    jc.scrollrun("bar", this.lastChild.firstChild, 0, callH, barMaxH);
                }
            }
        }, jc(scrollId));


        //滑动 目标类型、对象、方向(1下)、高度、可滚动最大高度
        jc.scrollrun = function (type, target, fx, height, maxH) {
            var top = parseInt(target.style.top.replace('px', ''));
            arguments.length == 7 && (top = arguments[5]); //初始当前高度
            var oneh = arguments.length == 5 ? 1 : arguments[6];//动态高度   
            oneh > height && (oneh = height);
            if (fx == 1) {
                if (type == "bar") { target.style.top = (top + oneh) > maxH ? maxH + "px" : top + oneh + "px"; }
                else { target.style.top = (top - oneh) < -maxH ? -maxH + "px" : top - oneh + "px"; }
            } else {
                if (type == "bar") { target.style.top = (top - oneh) < 0 ? 0 + "px" : top - oneh + "px"; }
                else { target.style.top = (top + oneh) > 0 ? 0 + "px" : top + oneh + "px"; }
            } oneh < height && setTimeout(function () { jc.scrollrun(type, target, fx, height, maxH, top, oneh + 3); }, 1);
        }
    }

    window.jc = jc;
})(window)