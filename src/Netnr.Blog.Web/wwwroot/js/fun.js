/*
 * netnr
 * 2021-12-13
 */

if (console) {

    var outs = [], domain = "https://www.netnr.com", fi = function () { return { msg: "", style: "" } };

    var oi = fi();
    oi.msg = "NET牛人";
    oi.style = "padding:10px 40px 10px;line-height:50px;background:url('" + domain + "/favicon.svg') no-repeat;background-size:15% 100%;font-size:1.8rem;color:orange";
    outs.push(oi);

    oi = fi();
    oi.msg = domain;
    oi.style = "background-image:-webkit-gradient( linear, left top, right top, color-stop(0, #f22), color-stop(0.15, #f2f), color-stop(0.3, #22f), color-stop(0.45, #2ff), color-stop(0.6, #25e),color-stop(0.75, #4f2), color-stop(0.9, #f2f), color-stop(1, #f22) );color:transparent;-webkit-background-clip: text;font-size:1.5em;"
    outs.push(oi);

    oi = fi();
    var vls = [
        { name: "GitHub", link: "https://github.com/netnr" }
    ];
    for (var i = 0; i < vls.length; i++) {
        var vi = vls[i];
        oi.msg += "\r\n" + vi.name + "：\r\n" + vi.link + "\r\n"
    }
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
                console.log(JSON.stringify({
                    load: t.loadEventEnd - t.navigationStart,
                    ready: t.domComplete - t.responseEnd,
                    request: t.responseEnd - t.requestStart
                }))
                clearInterval(window.funsi);
            }
        }, 10)
    }
}

//节日
if (true) {
    let txt = null, day = (new Date(new Date().valueOf() + 8 * 3600000)).toISOString().substring(5, 10);
    switch (day) {
        case "10-01":
        case "10-02":
        case "10-03":
        case "10-04":
        case "10-05":
        case "10-06":
            txt = "伟大的中华人民共和国万岁!";
            break;
        case "01-01":
            txt = "元旦快乐哟!";
            break;
        case "04-04":
        case "12-13":
            {
                if (day == "12-13") {
                    txt = "对和平的向往和坚守，不延续仇恨!";
                }

                var des = document.documentElement.style;
                des["filter"] = "progid: DXImageTransform.Microsoft.BasicImage(grayscale = 1)";
                des["-webkit-filter"] = "grayscale(100%)";
            }
            break;
    }

    if (txt && (location.pathname == "/" || location.pathname.startsWith("/home"))) {
        var dh = document.createElement('div');
        dh.innerHTML = '<div class="d-none d-md-block text-center h4 py-2 text-warning bg-danger">' + txt.split('').join(' ') + '</div>';
        document.body.insertBefore(dh, document.body.firstChild);
    }
}