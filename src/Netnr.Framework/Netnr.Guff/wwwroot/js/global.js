var guff = {
    //接口
    host: "https://www.netnr.com/",
    //页名
    pn: location.pathname.substring(location.pathname.lastIndexOf('/') + 1).toLowerCase(),
    //缓存
    dc: {},
    //初始化
    init: function () {
        //个人登录加载
        guff.queryUserInfo().then(x => x.json()).then(res => {
            guff.setLogin(res.code == 200);
            if (res.code == 200) {
                guff.dc["userinfo"] = res.data;
                document.getElementById('aus').href = guff.host + "user/setting";
            } else {
                guff.loginFirst();
            }
        }).catch(() => {
            guff.setLogin();
            guff.loginFirst();
        });

        //点击
        $(document).click(function (e) {
            e = e || window.event;
            var target = e.target || e.srcElement;

            //回复保存
            if ((target.className || "").indexOf("btnSaveReply") >= 0) {
                guff.saveReply();
            }
        });
    },
    /**
    * Cookie获取、设置、删除
    * @param {string} key 键
    * @param {string} value 值
    * @param {number} time 过期时间（默认不指定过期时间），单位：毫秒，小于0删除
    */
    cookie: function (key, value, time) {
        if (arguments.length == 1) {
            var arr, reg = new RegExp("(^| )" + key + "=([^;]*)(;|$)");
            if (arr = document.cookie.match(reg)) {
                return arr[2];
            }
            return "";
        } else {
            var kv = key + "=" + value + ";path=/;";
            if (time != undefined) {
                var d = new Date();
                d.setTime(d.getTime() + time);
                kv += "expires=" + d.toGMTString()
            }
            document.cookie = kv;
        }
    },
    /**
     * 多项输入
     * @param {any} elem
     */
    muInput: function (elem) {
        elem.addEventListener('input', function () {
            this.value = this.value.replace(/，/g, ',').replace(/,,/g, ',');
        }, false);
        elem.addEventListener('blur', function () {
            var val = this.value.replace(/  /g, ' ').replace(/,,/g, ','), arr = [];
            val.split(',').forEach(k => {
                if (k != "" && arr.indexOf(k) == -1) {
                    arr.push(k.trim());
                }
            });
            this.value = arr.join(',');
        }, false);
    },
    /**
     * 获取Url参数值
     * @param {any} name 参数名
     * @param {any} origin 自定义源，可选，默认window.location.search
     */
    getUrlParam: function (name, origin) {
        if (arguments.length == 1) {
            origin = window.location.search;
        }
        var reg = new RegExp("(^|&)" + name + "=([^&]*)(&|$)");
        var r = origin.substr(1).match(reg);
        if (r != null) return decodeURIComponent(r[2]); return null;
    },
    /**
     * 设置Url参数
     * @param {any} name 参数名
     * @param {any} value 值
     * @param {any} origin 自定义源，可选，默认window.location.search
     */
    setUrlParam: function (name, value, origin) {
        if (arguments.length == 2) {
            origin = window.location.search;
        }
        var pars = [], ise = false;
        origin.substring(1).split('&').forEach(pg => {
            if (pg != "") {
                var kvs = pg.split('=');
                if (kvs[0] == name) {
                    kvs[1] = value;
                    ise = true;
                }
                pars.push(kvs[0] + '=' + encodeURIComponent(kvs[1]))
            }
        })
        if (!ise) {
            pars.push(name + '=' + encodeURIComponent(value))
        }
        return pars.join('&');
    },
    /**
     * fetch 封装
     * @param {any} url 源
     * @param {any} data 数据
     * @param {any} type 请求类型 默认GET
     */
    fetch: function (url, data, type) {
        var ops = {
            method: type || 'GET',
            credentials: 'include',
            headers: {}
        };
        if (ops.method.toLowerCase() == "post") {
            ops.body = data;
            ops.headers['Content-Type'] = 'application/x-www-form-urlencoded';
        }
        return fetch(url, ops)
    },
    /**
     * 对象转URL参数
     * @param {any} obj 对象
     */
    objAsUrlParam: function (obj) {
        var pms = [];
        for (var i in obj) {
            var iv = obj[i];
            if (iv == null) {
                iv = "";
            }
            pms.push(i + '=' + encodeURIComponent(iv));
        }
        return pms.join('&');
    },
    /**
     * html编码
     * @param {any} html
     */
    htmlEncode: function (html) {
        var temp = document.createElement("div");
        (temp.textContent != undefined) ? (temp.textContent = html) : (temp.innerText = html);
        return temp.innerHTML;
    },
    /**
     * html解码
     * @param {any} text
     */
    htmlDecode: function (text) {
        var temp = document.createElement("div");
        temp.innerHTML = text;
        return temp.innerText || temp.textContent;
    },
    /**
     * 获取用户信息
     */
    queryUserInfo: function () {
        return guff.fetch(guff.host + "api/v1/userinfo");
    },
    /**
     * 设置是否登录
     * @param {any} isLogin
     */
    setLogin: function (isLogin) {
        var loginbox = document.getElementById('ulogin');
        if (loginbox) {
            loginbox = loginbox.children;
            if (isLogin) {
                loginbox[0].className = "d-none";
                loginbox[1].className = "";
            } else {
                loginbox[0].className = "";
                loginbox[1].className = "d-none";
            }
            loginbox[2].className = "d-none";
        }
    },
    /**
     * 登录链接
     */
    loginLink: function () {
        return document.getElementById('ulogin').children[0].children[0].href;
    },
    /**
     * 请先登录
     */
    loginFirst: function () {
        if (["publish", "me", "melaud", "mereply"].indexOf(guff.pn) >= 0) {
            lbox = document.createElement("div");
            lbox.style.cssText = "position:fixed;top:0;left:0;bottom:0;right:0;background-color:#e0e0e0;z-index:9999;text-align:center;padding-top:48vh;"
            lbox.innerHTML = '<span class="rounded bg-light p-3">请先 <a href="' + guff.loginLink() + '">登录 / 注册</a></span>';
            document.body.appendChild(lbox);
        }
    },
    /**
     * 构建页链接
     * @param {any} page 页码
     * @param {any} ut 分类
     */
    buildPage: function (page, ut) {
        switch (ut) {
            case "reply":
                return "javascript:guff.queryReply('" + guff.dc.replyid + "'," + page + "," + guff.dc.replynum + ")";
            default:
                return '#' + guff.setUrlParam('page', page, location.hash);
        }
    },
    /**
     * PageVM 对象生成分页组件
     * @param {any} pvm 后台 PageVM 分页视图信息
     * @param {any} ut 分类
     */
    viewPaging: function (pvm, ut) {
        var pag = pvm.Pag;
        if (!pag) {
            return '';
        }

        var htm = [];
        if (pag.PageTotal > 1) {
            htm.push('<ul class="pagination justify-content-center my-3">');

            if (pag.PageNumber > 1) {
                htm.push('<li class="page-item">');
                htm.push('<a class="page-link" href="' + this.buildPage(pag.PageNumber - 1, ut) + '">');
                htm.push('<span>&laquo;</span>');
                htm.push('</a>');
                htm.push('</li>');
            }
            if (pag.PageNumber > 3) {
                htm.push('<li class="page-item">');
                htm.push('<a class="page-link" href="' + this.buildPage(1, ut) + '">1</a>');
                htm.push('</li>');
                if (pag.PageNumber - 3 > 2) {
                    htm.push('<li class="page-item disabled">');
                    htm.push('<a class="page-link">...</a>');
                    htm.push('</li>');
                }
                else if (pag.PageNumber - 3 == 2) {
                    htm.push('<li class="page-item">');
                    htm.push('<a class="page-link" href="' + this.buildPage(2, ut) + '">2</a>');
                    htm.push('</li>');
                }
            }
            if (pag.PageNumber - 2 > 0) {
                htm.push('<li class="page-item">');
                htm.push('<a class="page-link" href="' + this.buildPage(pag.PageNumber - 2, ut) + '">');
                htm.push(pag.PageNumber - 2);
                htm.push('</a>');
                htm.push('</li>');
            }
            if (pag.PageNumber - 1 > 0) {
                htm.push('<li class="page-item">');
                htm.push('<a class="page-link" href="' + this.buildPage(pag.PageNumber - 1, ut) + '">');
                htm.push(pag.PageNumber - 1);
                htm.push('</a>');
                htm.push('</li>');
            }

            htm.push('<li class="page-item active"><a class="page-link" href="' + this.buildPage(pag.PageNumber, ut) + '">' + pag.PageNumber + '</a></li>');

            if (pag.PageNumber + 1 <= pag.PageTotal) {
                htm.push('<li class="page-item">');
                htm.push('<a class="page-link" href="' + this.buildPage(pag.PageNumber + 1, ut) + '">');
                htm.push(pag.PageNumber + 1);
                htm.push('</a>');
                htm.push('</li>');
            }
            if (pag.PageNumber + 2 <= pag.PageTotal) {
                htm.push('<li class="page-item">');
                htm.push('<a class="page-link" href="' + this.buildPage(pag.PageNumber + 2, ut) + '">');
                htm.push(pag.PageNumber + 2);
                htm.push('</a>');
                htm.push('</li>');
            }
            if (pag.PageNumber + 3 <= pag.PageTotal) {
                if (pag.PageNumber + 3 < pag.PageTotal - 1) {
                    htm.push('<li class="page-item disabled">');
                    htm.push('<a class="page-link">...</a>');
                    htm.push('</li>');
                }
                else if (pag.PageNumber + 3 == pag.PageTotal - 1) {
                    htm.push('<li class="page-item">');
                    htm.push('<a class="page-link" href="' + this.buildPage(pag.PageTotal - 1, ut) + '">');
                    htm.push(pag.PageTotal - 1);
                    htm.push('</a>');
                    htm.push('</li>');
                }
                htm.push('<li class="page-item">');
                htm.push('<a class="page-link" href="' + this.buildPage(pag.PageTotal, ut) + '">');
                htm.push(pag.PageTotal);
                htm.push('</a>');
                htm.push('</li>');
            }
            if (pag.PageNumber < pag.PageTotal) {
                htm.push('<li class="page-item">');

                htm.push('<a class="page-link" href="' + this.buildPage(pag.PageNumber + 1, ut) + '">');
                htm.push('<span>&raquo;</span>');
                htm.push('</a>');
                htm.push('</li>');
            }
            htm.push('</ul>');
        }

        return htm.join('');
    },
    /**
     * 显示加载层
     * @param {any} isHide
     */
    viewLoading: function (isHide) {
        document.getElementById("boxloading").style.visibility = isHide ? "hidden" : "visible";
    },
    /**
     * 渲染一条
     * @param {any} item
     */
    viewItem: function (item) {
        var htm = [];

        htm.push('<li class="list-group-item mwa-100 p-4">')

        if (item.GrContent) {
            htm.push('<div class="mb-3">' + guff.htmlEncode(item.GrContent) + '</div>');
        }

        if (item.GrImage) {
            var urls = item.GrImage.split(',') || [];
            urls.forEach(u => {
                htm.push('<div class="mb-3"><img src="' + u.replace(/"/g, "") + '" onerror="this.remove()"/></div>');
            })
        }

        if (item.GrAudio) {
            var urls = item.GrAudio.split(',') || [];
            urls.forEach(u => {
                htm.push('<div class="mb-3"><audio class="w-100" src="' + u.replace(/"/g, "") + '" controls /></div>');
            })
        }

        if (item.GrVideo) {
            var urls = item.GrVideo.split(',') || [];
            urls.forEach(u => {
                htm.push('<div class="mb-3"><video class="w-100" src="' + u.replace(/"/g, "") + '" controls /></div>');
            })
        }

        if (item.GrRemark) {
            htm.push('<div class="mb-3">' + guff.htmlEncode(item.GrRemark) + '</div>');
        }

        htm.push('<div class="mt-3">');
        htm.push('<a href="javascript:guff.connectionLaud(\'' + item.GrId + '\',\'' + (item.Spare1 || "") + '\')" id="laud_' + item.GrId + '" class="mr-2 badge badge-light' + (item.Spare1 == "laud" ? " text-white bg-success" : "") + '" title="' + (item.Spare1 == "laud" ? "取消点赞" : "点赞") + '"><i class="fa fa-fw fa-thumbs-up"></i><span>' + item.GrLaud + '</span></a>')
        htm.push('<a href="javascript:guff.queryReply(\'' + item.GrId + '\',1,' + (item.GrReplyNum || 0) + ')" id="reply_' + item.GrId + '" class="mr-2 text-muted badge badge-light" title="回复"><i class="fa fa-fw fa-commenting-o"></i><span>' + item.GrReplyNum + '</span></a>');
        htm.push('<a href="javascript:guff.setGoLink(\'uid\',' + item.Uid + ')" class="mr-2 text-muted badge badge-light" title="发帖人"><i class="fa fa-fw fa-user"></i>' + item.Spare3 + '</a>');
        htm.push('<span class="mr-3 text-muted badge badge-light" title="时间"><i class="fa fa-fw fa-clock-o"></i>' + item.GrCreateTime + '</span>');

        if (item.GrTypeName && item.GrTypeValue) {
            var nv = item.GrTypeName + '/' + item.GrTypeValue;
            htm.push('<a href="javascript:guff.setGoLink(\'nv\',\'' + encodeURIComponent(nv) + '\')" class="mr-2 text-muted badge badge-light" title="分类名/分类值">' + guff.htmlEncode(nv) + '</a>');
        }

        item.GrObject && htm.push('<a href="javascript:guff.setGoLink(\'obj\',\'' + encodeURIComponent(item.GrObject) + '\')" class="mr-2 text-muted badge badge-light" title="对象">' + guff.htmlEncode(item.GrObject) + '</a>');
        (item.GrTag || '').split(',').forEach(t => {
            (t && t != '') && htm.push('<a href="javascript:guff.setGoLink(\'tag\',\'' + encodeURIComponent(t) + '\')" class="mr-2 text-muted badge badge-light" title="标签">' + guff.htmlEncode(t) + '</a>');
        })

        if (item.Spare2 == "owner") {
            htm.push('<a href="javascript:guff.deleteOne(\'' + item.GrId + '\')" class="float-right badge badge-light" title="删除"><i class="fa fa-fw fa-trash-o"></i></a>');
            htm.push('<a href="' + guff.loginLink().replace("login", "publish#id=" + item.GrId) + '" class="float-right badge badge-light" title="编辑"><i class="fa fa-fw fa-edit"></i></a>');
        }

        if (arguments.length == 2) {
            htm.push('<a href="' + guff.loginLink().replace("login", "detail#id=" + item.GrId) + '" class="float-right badge badge-light" title="只查看这一条"><i class="fa fa-fw fa-eye"></i></a>')
        }

        htm.push('</div>');

        htm.push('</li>')

        return htm.join('')
    },
    /**
     * PageVM 对象生成列表
     * @param {any} pvm 后台 PageVM 分页视图信息
     */
    viewList: function (pvm) {
        var htm = [];
        htm.push('<ul class="list-group">');
        //分页列表
        var rows = pvm.Rows;
        if (rows) {
            if (rows.length) {
                for (var i = 0, len = rows.length; i < len; i++) {
                    var item = rows[i];
                    htm.push(guff.viewItem(item, i));
                }
            } else {
                htm.push('<li class="list-group-item"><p class="text-center py-5">无 ^_^</p></li>');
            }
        } else {
            //单个
            htm.push(guff.viewItem(pvm));
        }

        htm.push('</ul>');
        return htm.join('');
    },
    /**
     * 查询列表
     * @param {any} page 分页
     */
    queryList: function () {
        var uri;
        switch (guff.pn) {
            case "":
            case "top":
            case "image":
            case "audio":
            case "video":
            case "me":
            case "melaud":
            case "mereply":
                {
                    var uid = guff.getUrlParam("uid", location.hash);
                    var nv = guff.getUrlParam("nv", location.hash);
                    var tag = guff.getUrlParam("tag", location.hash);
                    var obj = guff.getUrlParam("obj", location.hash);

                    uri = guff.host + "api/v1/guff/list?" + guff.objAsUrlParam({
                        category: guff.pn,
                        uid: uid,
                        nv: nv,
                        tag: tag,
                        obj: obj,
                        page: guff.getUrlParam("page", location.hash) || 1
                    });
                }
                break;
            case "detail":
                {
                    var did = guff.getUrlParam("id", location.hash);
                    if (did && did.length == 19) {
                        uri = guff.host + "api/v1/guff/detail?" + guff.objAsUrlParam({
                            id: did
                        });
                    } else {
                        jz.alert('无效的ID');
                        return false;
                    }
                }
                break;
        }

        //显示加载
        guff.viewLoading();

        //列表
        var blist = document.getElementById('boxlist');
        //分页
        var bp = document.getElementById("boxpaging");

        guff.fetch(uri).then(x => x.json()).then(res => {
            //关闭加载
            guff.viewLoading(true);

            guff.dc.pagedata = res;
            if (res.code == 200) {
                //最大页码
                if (res.data.Pag && res.data.Pag.PageTotal > 1 && res.data.Pag.PageNumber > res.data.Pag.PageTotal) {
                    //设置当前页为最大页码
                    location.hash = guff.setUrlParam('page', res.data.Pag.PageTotal, location.hash);
                    guff.queryList();
                    return false;
                }

                window.scrollTo(0, 0);

                //列表
                var vlist = guff.viewList(res.data);
                blist.innerHTML = vlist;
                //图片
                if (!blist.bindClick) {
                    blist.bindClick = true;

                    blist.onclick = function (e) {
                        e = e || window.event;
                        var target = e.target || e.srcElement;
                        if (target.nodeName == "IMG") {
                            var image = new Image();
                            image.src = target.src;
                            image.onload = function () {
                                if (this.width > 300) {
                                    var im = document.createElement('div');
                                    im.style.cssText = "position:fixed;width:100%;height:100%;top:0;left:0;right:0;bottom:0;z-index:999;background-color:rgba(0, 0, 0, .6);padding:15px;opacity:0;transition: opacity .4s";
                                    im.className = "nav flex-column justify-content-center";
                                    im.onclick = function (e) {
                                        e = e || window.event;
                                        if ((e.target || e.srcElement).nodeName != "IMG") {
                                            im.style.opacity = 0;
                                            setTimeout(function () {
                                                try {
                                                    document.body.removeChild(im)
                                                } catch (e) { }
                                            }, 400);
                                        }
                                    }
                                    image.className = "m-auto rounded mw-100 mh-100";
                                    im.appendChild(image);
                                    document.body.appendChild(im);
                                    setTimeout(function () {
                                        im.style.opacity = 1;
                                    }, 50)
                                }
                            }
                        }
                    }
                }

                //分页
                var vp = guff.viewPaging(res.data);
                bp.innerHTML = vp;

                //点击分页
                if (!bp.bindClick) {
                    bp.bindClick = true;

                    bp.onclick = function () {
                        setTimeout(function () {
                            guff.queryList();
                        }, 10)
                    }

                    window.onhashchange = function () {
                        guff.queryList();
                    }
                }
            }
        }).catch(err => {
            console.log(err);

            //关闭加载
            guff.viewLoading(true);

            blist.innerHTML = "<p class='text-center py-5'>抱歉，加载失败</p>";
        });
    },
    /**
     * 查询热门标签
     */
    queryHotTag: function () {
        var tbox = document.getElementById('boxhottag');
        if (tbox) {
            var uri = guff.host + "api/v1/guff/hottag";
            guff.fetch(uri).then(x => x.json()).then(res => {
                if (res.code == 200) {
                    var htm = [];
                    res.data.forEach(t => {
                        htm.push('<a href="#tag=' + encodeURIComponent(t) + '" class="mr-2 text-muted badge badge-light" title="标签">' + t + '</a>');
                    });
                    tbox.innerHTML = htm.join('');
                } else {
                    tbox.innerHTML = "<p class='text-center mt-3'>抱歉，加载失败</p>";
                }
            }).catch(err => {
                console.log(err);
                tbox.innerHTML = "<p class='text-center mt-3'>抱歉，加载失败</p>";
            });
        }
    },
    /**
     * 点赞
     * @param {any} id
     * @param {any} laud
     */
    connectionLaud: function (id, laud) {
        var islaud = laud != "laud";
        if (guff.dc["userinfo"]) {
            if (guff.dc["requestaction"] == null) {
                var uri = guff.host + "api/v1/guff/connection?" + guff.objAsUrlParam({
                    type: islaud ? "add" : "cancel",
                    ac: 1,
                    id: id
                });

                guff.dc["requestaction"] = "connectionLaud";

                guff.fetch(uri).then(x => x.json()).then(res => {
                    console.log(res);
                    guff.dc["requestaction"] = null;
                    if (res.code == 200) {
                        guff.setLaud(id, islaud);
                    } else {
                        jz.alert(res.msg);
                    }
                }).catch(err => {
                    guff.dc["requestaction"] = null;
                    console.log(err);
                    jz.alert('网络错误');
                });
            } else {
                console.log(guff.dc["requestaction"]);
                jz.alert('操作太快了');
            }
        } else {
            jz.alert('请先登录');
        }
    },
    /**
     * 设置点赞
     * @param {any} id
     * @param {any} islaud 是点赞
     */
    setLaud: function (id, islaud) {
        var an = $('#laud_' + id), sn = an.find('span');
        if (islaud) {
            an.addClass('text-white bg-success');
            sn.html(parseInt(sn.html()) + 1);
        } else {
            an.removeClass('text-white bg-success');
            sn.html(parseInt(sn.html()) - 1);
        }
        an[0].title = islaud ? "取消点赞" : "点赞";
        an[0].href = 'javascript:guff.connectionLaud("' + id + '","' + (islaud ? "laud" : "") + '")';
    },
    /**
     * 设置点赞
     * @param {any} id
     * @param {any} num 回复数
     */
    setReply: function (id, num) {
        var an = $('#reply_' + id), sn = an.find('span');
        sn.html(num);
        an[0].href = 'javascript:guff.queryReply("' + id + '",1,' + num + ')';
    },
    /**
     * 跳转
     * @param {any} type 类型
     * @param {any} v 值
     */
    setGoLink: function (type, v) {
        if (guff.pn == "detail") {
            location.href = "/#" + type + "=" + v;
        } else {
            location.hash = type + "=" + v;
        }
        if (guff.dc.replybox) {
            $(guff.dc.replybox).children().modal('hide');
        }
    },
    /**
     * 回复列表
     * @param {any} pvm
     */
    viewReplyList: function (pvm) {
        var htm = [];
        htm.push('<div class="row mb-3">');

        if (pvm) {
            htm.push('<div class="col-md-12"><ul class="list-unstyled">');
            for (var i = 0, len = pvm.Rows.length; i < len; i++) {
                var item = pvm.Rows[i], rn = ((pvm.Pag.PageNumber - 1) * pvm.Pag.PageSize + i + 1), upsrc;
                if (item.Spare3) {
                    upsrc = "https://www.gravatar.com/avatar/" + item.Spare3 + "?r=pg"
                } else {
                    upsrc = "https://www.netnr.com/gs/static/avatar/" + item.Spare2;
                }
                htm.push('<li class="media uwo-reply-item my-3" id="no_' + rn + '">');
                htm.push('<img class="imgup" src="' + upsrc + '" alt="头像" onerror="this.src=\'/favicon.svg\';this.onerror=null;">');
                htm.push('<div class="media-body ml-2" id="r_' + item.UrTargetId + '">');
                if (item.Uid == 0) {
                    if (item.UrAnonymousLink) {
                        htm.push('<a href="' + item.UrAnonymousLink.replace('"', '') + '" target="_blank" >' + guff.htmlEncode(item.UrAnonymousName) + '</a>');
                    } else {
                        htm.push('<a href="javascript:void(0);" >' + guff.htmlEncode(item.UrAnonymousName) + '</a>');
                    }
                    htm.push('<span class="badge badge-light" title="匿名用户">Guest</span>');
                } else {
                    htm.push('<a href="javascript:guff.setGoLink(\'uid\',\'' + item.Uid + '\')">' + guff.htmlEncode(item.Spare1) + '</a>');
                }
                htm.push('<small class="text-muted ml-3">' + item.UrCreateTime + '</small>');
                htm.push('<a class="float-right badge badge-light">' + rn + '</a>');
                htm.push('<div class="mt-2">');
                if (item.UrStatus == 1) {
                    htm.push('<div class="markdown-body display-section">' + guff.htmlEncode(item.UrContent) + '</div>');
                } else {
                    htm.push('<em class="badge badge-secondary" title="该信息已被屏蔽">block</em>');
                }
                htm.push('</div></div>');
                htm.push('</li>');
            }
            htm.push('</ul></div>');
        } else {
            htm.push('<div class="col-md-12"><p class="text-center py-3">无 ^_^</p></div>');
        }

        htm.push('</div>')

        return htm.join('');
    },
    /**
     * 查询回复
     * @param {any} id
     * @param {any} page 页码
     * @param {any} num 回复数
     */
    queryReply: function (id, page, num) {
        //记录当前回复信息
        guff.dc.replyid = id;
        guff.dc.replypage = page;
        guff.dc.replynum = num;

        if (!guff.dc.replybox) {
            var htm = [];
            htm.push('<div class="modal fade" data-backdrop="static"><div class="modal-dialog modal-dialog-scrollable modal-dialog-centered modal-xl"><div class="modal-content">');
            htm.push('<div class="modal-header"><h5 class="modal-title">回复</h5><button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button></div>');
            htm.push('<div class="modal-body min100"><div class="spinner-border"><span class="sr-only">Loading...</span></div></div>')

            htm.push('<div class="p-1">');
            htm.push('<form id="rwform" onsubmit="return false">');
            htm.push('<input type="hidden" name="id" value="' + id + '">');
            if (!guff.dc["userinfo"]) {
                htm.push('<div class="row">');

                htm.push('<div class="col-lg-4 mb-1"><div class="input-group"><div class="input-group-prepend"><div class="input-group-text">昵称</div></div>');
                htm.push('<input class="form-control" name="UrAnonymousName" placeholder="请输入昵称" maxlength="20"></div></div>');

                htm.push('<div class="col-lg-4 mb-1"><div class="input-group"><div class="input-group-prepend"><div class="input-group-text">邮箱</div></div>');
                htm.push('<input class="form-control" name="UrAnonymousMail" placeholder="根据邮箱从 Gravatar 获取头像" maxlength="50"></div></div>');

                htm.push('<div class="col-lg-4 mb-1"><div class="input-group"><div class="input-group-prepend"><div class="input-group-text">链接</div></div>');
                htm.push('<input class="form-control" name="UrAnonymousLink" placeholder="请输入链接，以 http 开头" maxlength="50"></div></div>');

                htm.push('</div>');
            }
            htm.push('<div class="row">');
            htm.push('<div class="col-md-12"><div class="input-group"><textarea class="form-control" name="UrContent" placeholder="请输入回复内容"></textarea>');
            htm.push('<div class="input-group-prepend"><button class="btn btn-warning btnSaveReply"><i class="fa fa-fw fa-send btnSaveReply"></i>回复</button></div></div></div></div>');
            htm.push('</div>');

            htm.push('</div></form></div>');
            htm.push('</div></div></div>');

            guff.dc.replybox = document.createElement('div');
            document.body.appendChild(guff.dc.replybox);
            guff.dc.replybox.innerHTML = htm.join('');
        }

        var rmb = $(guff.dc.replybox).find('.modal-body');

        if (num == 0) {
            rmb.html(guff.viewReplyList());
        } else {

            if (guff.dc["requestaction"] == null) {
                var uri = guff.host + "api/v1/guff/replylist?" + guff.objAsUrlParam({
                    id: id,
                    page: page
                });
                guff.dc["requestaction"] = "queryReply";
                guff.fetch(uri).then(x => x.json()).then(res => {
                    guff.dc["requestaction"] = null;
                    if (res.code == 200) {
                        rmb.scrollTop(0);
                        rmb.html(guff.viewReplyList(res.data) + guff.viewPaging(res.data, 'reply'));
                    }
                }).catch(err => {
                    guff.dc["requestaction"] = null;
                    console.log(err);
                    jz.alert('网络错误');
                })
            } else {
                jz.alert('操作太快了');
            }
        }

        if (!guff.dc["userinfo"]) {
            //读取 匿名信息
            var lsar = localStorage.getItem('AnonymousReply');
            try {
                lsar = lsar == null ? {} : JSON.parse(lsar);
                if (lsar.name) {
                    $('input[name="UrAnonymousName"]').val(lsar.name || "");
                }
                if (lsar.link) {
                    $('input[name="UrAnonymousLink"]').val(lsar.link || "");
                }
                if (lsar.mail) {
                    $('input[name="UrAnonymousMail"]').val(lsar.mail || "");
                }
            } catch (e) { }
        }

        if (page == 1) {
            //显示模态
            $(guff.dc.replybox.children[0]).modal();
        }
    },
    /**
     * 回复
     */
    saveReply: function () {
        if (guff.dc["requestaction"] == null) {
            var err = [];
            var id = $('input[name="id"]').val(),
                content = $('textarea[name="UrContent"]').val(),
                aname = $('input[name="UrAnonymousName"]').val(),
                alink = $('input[name="UrAnonymousLink"]').val(),
                amail = $('input[name="UrAnonymousMail"]').val();
            if (!guff.dc["userinfo"]) {
                if (aname.trim() == "") {
                    err.push('昵称不能为空');
                }
                if (alink != "" && alink.toLowerCase().indexOf("http") != 0) {
                    err.push('链接请以 http 开头');
                }
                if (amail == "" || !/^([a-zA-Z]|[0-9])(\w|\-)+@[a-zA-Z0-9]+\.([a-zA-Z]{2,4})$/.test(amail)) {
                    err.push('邮箱格式有误');
                }
            }
            if (content.trim().length < 1) {
                err.push('回复内容不能为空');
            }
            if (err.length) {
                jz.alert(err.join('<hr/>'));
                return false;
            }

            guff.dc["requestaction"] = "saveReply";

            var uri = guff.host + "api/v1/guff/replyadd";
            guff.fetch(uri, $('#rwform').serialize(), "post").then(x => x.json()).then(res => {
                guff.dc["requestaction"] = null;
                console.log(res);
                if (res.code == 200) {
                    var rn = guff.dc.replynum += 1;
                    //回复跳转最后一页
                    guff.queryReply(id, guff.dc.replypage, rn);
                    //设置回复数
                    guff.setReply(id, rn);

                    //保存 匿名信息
                    localStorage.setItem('AnonymousReply', JSON.stringify({
                        name: aname,
                        mail: amail,
                        link: alink
                    }));

                    $('textarea[name="UrContent"]').val('');
                } else {
                    jz.alert(res.msg);
                }
            }).catch(err => {
                console.log(err);
                guff.dc["requestaction"] = null;
                jz.alert('网络错误');
            })
        } else {
            jz.alert('操作太快了');
        }
    },
    /**
     * 删除一条
     * @param {any} id
     */
    deleteOne: function (id) {
        jz.confirm('确定删除吗？', {
            ok: function () {
                var uri = guff.host + "api/v1/guff/delete?id=" + encodeURIComponent(id);
                guff.fetch(uri).then(x => x.json()).then(res => {
                    if (res.code == 200) {
                        switch (guff.pn) {
                            case "detail":
                                location.href = "/";
                                break;
                            default:
                                guff.queryList();
                                break;
                        }
                    } else {
                        jz.alert(res.msg);
                    }
                }).catch(err => {
                    console.log(err);
                    jz.alert('网络错误');
                });
            }
        })
    }
}

if (["login", "logout"].indexOf(guff.pn) == -1) {
    guff.init();
}