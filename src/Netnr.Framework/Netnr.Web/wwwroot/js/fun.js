/**\

Author：netnr
Date：2018-12-01

\**/

(function (console) {

    if (console) {

        var outs = [], fi = function () { return { msg: "", style: "" } };

        var oi = fi();
        oi.msg = "NET牛人";
        oi.style = "padding:10px 40px 10px;line-height:50px;background:url('https://www.netnr.com/favicon.svg') no-repeat;background-size:15% 100%;font-size:1.8rem;color:#009a61";
        outs.push(oi);

        oi = fi();
        oi.msg = "https://www.netnr.com";
        oi.style = "background-image:-webkit-gradient( linear, left top, right top, color-stop(0, #f22), color-stop(0.15, #f2f), color-stop(0.3, #22f), color-stop(0.45, #2ff), color-stop(0.6, #25e),color-stop(0.75, #4f2), color-stop(0.9, #f2f), color-stop(1, #f22) );color:transparent;-webkit-background-clip: text;font-size:1.5em;"
        outs.push(oi);

        oi = fi();
        oi.msg = "\r\n源码：\r\nhttps://github.com/netnr/blog\r\n\r\nGitHub：\r\nhttps://github.com/netnr\r\n\r\n码云：\r\nhttps://gitee.com/netnr\r\n\r\nQ群：83084426";
        outs.push(oi);

        if (!("ActiveXObject" in window)) {
            outs.map(function (x) {
                console.log("%c" + x.msg, x.style);
            });
        }

        //耗时
        if (window.performance) {
            window.funsi = setInterval(function () {
                var t = performance.timing;
                if (t.loadEventEnd) {
                    console.table({
                        load: t.loadEventEnd - t.navigationStart,
                        ready: t.domComplete - t.responseEnd,
                        request: t.responseEnd - t.requestStart
                    })
                    clearInterval(window.funsi);
                }
            }, 10)
        }
    }

})(window.console);