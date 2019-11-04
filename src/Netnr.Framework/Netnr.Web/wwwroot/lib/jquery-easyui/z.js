/*                      *\
    Author：netnr
    Date：2019-09-04

    z.Grid：datagrid、treegrid、propertygrid、datalist
    z.Combo：combobox、combotree、tree

    z.Grid z.Combo 说明：属性、事件、方法与EasyUI一致，不同说明：
        ·type属性为上面对应的类型，z.Grid默认datagrid，z.Combo默认Combobox；
        ·使用自写的ajax请求，即使用本地数据的方式，load方法加载并调用bind方法；
        ·所以刷新不要用EasyUI提供的方法，用load方法，本地刷新用bind方法；
        ·方法调用统一用func，添加onComplete完成回调事件（支持多个回调）；
        ·autosize配置完成自动调整大小，ColFormat列配置显示格式化（选择公共格式化或自定义格式化方法自动回调）

    z.FormXXX系列方法：常用的表单方法

    z.SqlQuery查询条件组合

    z.DC：缓存

\*                      */

(function (window, undefined) {

    var z = function () { };

    //全局唯一 取值++ 模拟随机数 等
    z.index = 1;

    /**
     * event
     * @param {event} e 事件流
     */
    z.event = function (e) { return e || window.event };

    /**
     * target
     * @param {event} e 事件流
     */
    z.target = function (e) { return z.event(e).target || z.event(e).srcElement };

    /**
     * 阻止事件冒泡
     * @param {event} e 事件流
     */
    z.stopEvent = function (e) { if (e && e.stopPropagation) { e.stopPropagation() } else { window.event.cancelBubble = true } };

    /**
     * 阻止浏览器默认行为
     * @param {event} e 事件流
     */
    z.stopDefault = function (e) { if (e && e.preventDefault) { e.preventDefault() } else { window.event.returnValue = false } };

    /**
     * 按键ASCII值
     * @param {event} e 事件流
     */
    z.key = function (e) { return z.event(e).keyCode || z.event(e).which || z.event(e).charCode };

    /** 
     * PC、Mobile判断 
     */
    z.isPC = function () { return !navigator.userAgent.match(/(iPhone|iPod|Android|ios)/i) }

    /**
     * 载入配置
     * @param {any} that this对象
     * @param {any} options 配置项
     */
    z.thisInit = function (that, options) {
        for (var i in options) {
            that[i] = options[i];
        }
        return that;
    }

    /**
     * 隐去/恢复 对象属性值
     * @param {any} that 对象
     * @param {any} name 属性名称
     * @param {any} ishide 隐去/恢复
     */
    z.hideAttr = function (that, name, ishide) {
        ishide = ishide == null ? true : ishide;
        if (ishide) {
            that["_" + name] = that[name];
            delete that[name];
        } else {
            that[name] = that["_" + name]
            delete that["_" + name];
        }
    }

    /**
     * Grid：datagrid、treegrid、propertygrid、datalist
     */
    z.Grid = function () { return new z.Grid.fn.init() };

    z.Grid.fn = z.Grid.prototype = {
        init: function () {
            return z.thisInit(this, this.defaultOptions = {
                isinit: true,/*是初始化*/
                id: "#Grid1",/*容器#ID*/
                pageNumber: 1,/*页码*/
                pageSize: 30,/*页量*/
                pageList: [10, 30, 50, 100, 200],/*可选页量*/
                total: 0,/*总条数*/
                type: "datagrid",/*类型：datagrid、treegrid、propertygrid、datalist*/
                method: "POST",
                pagination: true,/*启用分页*/
                rownumbers: true,/*显示行号*/
                rownumberWidth: 30,/*行号宽度*/
                singleSelect: true,/*单选*/
                autoRowHeight: false,/*自动行高*/
                frozenColumns: [[]],/*冻结列*/
                loadMsg: "加载中...",/*加载提示*/
                queryParams: {},/*请求参数*/
                queryMark: true, /*启用查询标记*/
                autosize: "xy",/*调整大小，请参考z.GridAuto方法*/
                autosizePid: "#myBody",/*自适应父容器#ID*/
                onBeforeBind: function (ops) { },/*绑定前回调，return false阻止绑定*/
                sortName: null,/*排序字段，支持多个，逗号分割*/
                sortOrder: "asc",/*排序类型，支持多个，逗号分割*/
                completeIndex: 0,/*完成事件索引，用于支持多次调用*/
                /*完成事件回调，支持多次调用 (注意死循环，在完成事件重绑数据需加标记跳出循环绑定)*/
                onComplete: function (fn) {
                    this.completeIndex += 1;
                    this["_onComplete" + this.completeIndex] = fn;
                },
                //绑定
                bind: function () {
                    var that = this;

                    that.target = $(that.id);

                    //复选框
                    var hone = that.frozenColumns[0];
                    for (var i in hone) {
                        if ("checkbox" in hone[i]) {
                            hone.splice(i, 1);
                            break;
                        }
                    }
                    that.checkbox && that.frozenColumns[0].splice(0, 0, { field: "ck", checkbox: true });

                    //自定义页量处理
                    if (that.pagination && that.isinit) {
                        if (("," + that.pageList.join(',') + ",").indexOf("," + that.pageSize + ",") == -1) {
                            that.pageList.push(that.pageSize);
                            that.pageList = that.pageList.sort(function (x, y) {
                                return x > y;
                            });
                        }
                    }

                    //绑定前回调
                    if (that.onBeforeBind.call(that, that) == false) { return false; }

                    z.hideAttr(this, 'url');

                    //绑定
                    that.target[that.type](that);

                    z.hideAttr(this, 'url', false);

                    //分页
                    if (that.pagination) {
                        that.func('options').pageNumber = that.pageNumber;

                        that.func('getPager').pagination({
                            pageNumber: that.pageNumber,
                            pageSize: that.pageSize,
                            total: that.total,
                            links: 7,
                            layout: ['list', 'sep', 'first', 'prev', 'links', 'next', 'last', 'sep', 'refresh', 'info'],
                            onSelectPage: function (pageNumber, pageSize) {
                                that.pageNumber = pageNumber || 1;
                                that.pageSize = pageSize;
                                that.load();
                            }
                        });
                    }

                    //调整大小
                    if (that.target.data('autosize') != 1) {
                        z.GridAuto(that);
                        $(window).resize(function () { z.GridAuto(that) });
                        that.target.data('autosize', 1);
                    } else {
                        //刷新内部会调整大小，所以需要再次调用调整大小
                        if ("treegrid,propertygrid".indexOf(that.type) >= 0) {
                            z.GridAuto(that);
                        }
                    }

                    //查询标记
                    if (that.queryMark && z.GridQueryMark) {
                        z.GridQueryMark(that);
                    }

                    //执行完成事件
                    for (var i = 1; i < that.completeIndex + 1; i++) {
                        typeof that["_onComplete" + i] == "function" && that["_onComplete" + i](that);
                    }

                    that.isinit = false;

                    return this;
                },
                //载入
                load: function () {
                    var that = this;

                    that.target = $(that.id);

                    if (that.p == null) {
                        that.p = {};
                        that.p.sortName = that.sortName;
                        that.p.sortOrder = that.sortOrder;
                    }

                    //点击排序
                    if (typeof that.onSortColumn != "function") {
                        that.onSortColumn = function (sort, order) {
                            that.sortName = sort;
                            that.sortOrder = order;
                            that.load();
                        };
                    }

                    //参数
                    that.queryParams = {
                        pagination: that.pagination == true ? 1 : 0,
                        sort: that.sortName || that.p.sortName,
                        order: that.sortOrder || that.p.sortOrder,
                        page: that.pageNumber,
                        rows: that.pageSize,
                        columnsExists: that.columnsExists = (that.columns && that.columns.length) ? 1 : 0
                    };

                    //行号宽度自适应            
                    that.rownumberWidth = Math.max(30, (that.pageNumber * that.pageSize).toString().length * 12);

                    //请求前回调且修改别名（因两次）
                    if (typeof that.onBeforeLoad == "function") {
                        that._onBeforeLoad = that.onBeforeLoad;
                        delete that.onBeforeLoad;
                    }
                    if (typeof that._onBeforeLoad == "function") {
                        var rownode;
                        try { rownode = that.func('getSelected'); } catch (e) { }
                        if (that._onBeforeLoad(rownode, that.queryParams) == false) { return false; }
                    }

                    //载入提示
                    z.GridLoading(that);
                    try { that.target.datagrid("loading") } catch (e) { }

                    $.ajax({
                        url: that.url,
                        type: that.method,
                        data: that.queryParams,
                        dataType: "json",
                        success: function (data) {
                            that.dataCache = data;

                            that.total = data.total || 0;
                            that.data = data.data || [];

                            //创建表头信息
                            if (!that.columnsExists) {
                                var columns = [], frozens = [];
                                $(data.columns).each(function (k, v) {
                                    var column = {
                                        field: this.ColField,
                                        title: this.ColTitle,
                                        width: this.ColWidth == 0 ? null : this.ColWidth,
                                        hidden: this.ColHide >= 1 ? true : false,
                                        halign: "center",
                                        align: this.ColAlign == 2 ? "center" : this.ColAlign == 3 ? "right" : "left",
                                        sortable: this.ColSort == 1 ? true : false,
                                        FormType: this.FormType,
                                        FormUrl: this.FormUrl,
                                        FormRequired: this.FormRequired == 1,
                                        FormPlaceholder: this.FormPlaceholder || "",
                                        ColQuery: this.ColQuery == null ? 0 : this.ColQuery,
                                        ColRelation: this.ColRelation,
                                        formatter: function (value, row) { return z.GridFormat(value, row, v) }
                                    }
                                    if (this.ColFrozen == 1) { frozens.push(column); }
                                    else { columns.push(column); }
                                });
                                if (columns.length) {
                                    that.columns = [];
                                    that.columns.push(columns);
                                }

                                if (frozens.length) {
                                    that.frozenColumns = [];
                                    that.frozenColumns.push(frozens);
                                }
                                that.columnsExists = 1;
                            }
                            that.bind();
                        },
                        error: function (ex) {
                            that.total = 0;
                            that.data = [];
                            //错误回调
                            typeof that.onLoadError == "function" && that.onLoadError(ex);
                        }
                    });
                    return this;
                },
                //方法
                func: function () {
                    return $(this.id)[this.type](arguments[0], arguments[1]);
                }
            });
        }
    };

    z.Grid.fn.init.prototype = z.Grid.fn;

    /**
     * Grid格式化
     * @param {any} value 值
     * @param {any} row 行数据
     * @param {any} v 表配置行数据
     */
    z.GridFormat = function (value, row, v) {
        switch ($.trim(v.ColFormat)) {
            case "col_custom_":
                try { return eval("col_custom_" + v.ColField.toLowerCase())(value, row, v); } catch (e) { return value; }
                break;
            case "19":
                return value = value == "1" ? '正常' : '<span style="background-color:#F89406;color:#fff;padding:4px">停用</span>';
            case "18":
                return value = value == "1" ? "✘" : "✔";
            case "17":
                return value = value == "1" ? "✔" : "✘";
            //精确两位  带￥
            case "16":
                if (value != undefined && value != "") {
                    value = isNaN(parseFloat(value)) ? "￥ 0.00" : '￥ ' + parseFloat(value).toFixed(2);
                }
                break;
            //精确两位
            case "15":
                if (value != undefined && value != "") {
                    value = isNaN(parseFloat(value)) ? "0.00" : parseFloat(value).toFixed(2);
                }
                break;
            //yyyy-MM-dd
            case "14":
                if (value && value.length > 10) {
                    value = value.substring(0, 10);
                    value[4] = "-";
                    value[7] = "-";
                }
                break;
            //yyyy-MM-dd HH:mm:ss
            case "13":
                if (value && value.length >= 19) {
                    value = value.substring(0, 19);
                    value[4] = "-";
                    value[7] = "-";
                }
                break;
            //HH:mm:ss
            case "12":
                if (value && value.length >= 8) {
                    value = value.substring(0, 8);
                }
                break;
            //yyyy-MM
            case "11":
                if (value && value.length > 7) {
                    value = value.substring(0, 7);
                    value[4] = "-";
                }
                break;
        }
        return value;
    }

    /**
     * Grid调整大小
     * @param {any} gd z.Grid方法返回的对象
     */
    z.GridAuto = function (gd) {
        if (!$(gd.id).length) { return gd; }

        var h = $(window).height() - $(gd.id).parents('.datagrid').offset().top - 10;
        h < 150 && (h = 150);
        var ro = { width: null, height: null };
        switch (gd.autosize) {
            /*宽度自适应，高度保持沉底*/
            case 'xy':
                ro.width = $(gd.autosizePid).width();
                ro.height = h;
                break;
            /*宽度自适应，高度不变*/
            case 'x':
                ro.width = $(gd.autosizePid).width();
                ro.height = null;
                break;
            /*宽度不变，高度保持沉底*/
            case 'y':
                ro.width = null;
                ro.height = h;
                break;
            /*宽高自适应父容器*/
            case 'p':
                ro.width = $(gd.autosizePid).width();
                ro.height = $(gd.autosizePid).height();
                break;
            default:
                ro = null;
        }
        $(gd.id).datagrid('resize', ro);

        return gd;
    }

    /**
     * Grid载入
     * @param {any} gd z.Grid方法返回的对象
     */
    z.GridLoading = function (gd) {
        var target = $(gd.id);
        if (target.hasClass('loadingimg') && target.data('loadingimg') != 1) {
            target.html('<img src="/images/loading.svg" />').show().data('loadingimg', 1);
        }
    }

    /**
     * Grid编辑配置
     * @param {any} gd z.Grid方法返回的对象
     * @param {any} index 行索引
     * @param {any} field 字段
     * @param {object} row 行数据（可选，传入则根据行数据的配置进行编辑，不传根据标题配置进行编辑）
     */
    z.GridEditor = function (gd, index, field, row) {

        //结束编辑
        if (gd.ei != null) {
            gd.func('endEdit', gd.ei);
            gd.ei = null;
        }

        //获取所有列、清空所有编辑仅保留一个编辑
        var cfs = gd.func('getColumnFields'), column;
        $(cfs).each(function () {
            if (this == field) {
                column = gd.func('getColumnOption', field);
            } else {
                gd.func('getColumnOption', this).editor = null
            }
        });
        var ec = row || column;

        if (column) {
            switch (ec.FormType) {
                case 'combotree':
                case 'combobox':
                    {
                        var lowerUrl = String(ec.FormUrl).toLowerCase(), ops = z.DC[lowerUrl] || {};

                        if (!(lowerUrl in z.DC)) {
                            $.ajax({
                                url: lowerUrl,
                                type: ops.method || "POST",
                                data: ops.queryParams || {},
                                dataType: "json",
                                success: function (data) {
                                    z.DC[lowerUrl] = {};
                                    z.DC[lowerUrl].data = data;
                                    column.editor = { type: ec.FormType, options: z.DC[lowerUrl] };
                                    gd.func('beginEdit', gd.ei = index);
                                }
                            })
                        } else {
                            var cb = z.Combo();
                            cb.data = z.DC[lowerUrl].data;
                            cb.height = 33;
                            if (ec.FormType == "combobox") {
                                cb.panelHeight = Math.min(7, cb.data.length) * cb.height + 5;
                            }
                            cb.editable = false;
                            cb.type = ec.FormType;
                            //不显示清除按钮
                            cb.icons = null;
                            cb.onLoadSuccess = function () {
                                var that = $(this);
                                setTimeout(function () {
                                    switch (cb.type) {
                                        case "combobox":
                                            that[ec.FormType]('showPanel');
                                            that[ec.FormType]('textbox')[0].focus();
                                            break;
                                        case "combotree":
                                            //该类型，暂未找到默认打开下拉面板和选中文本框的方法
                                            that[ec.FormType]('showPanel');
                                            break;
                                        default:
                                    }
                                }, 0);
                            }
                            if (typeof z.DC[lowerUrl].init == "function") {
                                z.DC[lowerUrl].init.call(cb, cb);
                            }
                            z.DC[lowerUrl].obj = cb;
                            column.editor = { type: ec.FormType, options: cb };
                            gd.func('beginEdit', gd.ei = index);
                        }
                    }
                    break;
                case 'datetime':
                case 'date':
                case 'time':
                case 'password':
                case 'checkbox':
                case 'tip':
                case 'text':
                case 'modal':
                    var opt = { url: ec.FormUrl, type: ec.FormType };
                    column.editor = { type: ec.FormType, options: opt };
                    gd.func('beginEdit', gd.ei = index);
                    break;
            }
        }
    }

    /**
     * 点空白结束Grid编辑
     * @param {any} gd z.Grid方法返回的对象
     */
    z.GridEditorBlank = function (gd) {
        $(document).mousedown(function (e) {
            if (gd.ei != null) {
                var target = e.target || window.event.srcElement;
                if (!gd.func('getPanel').find('table.datagrid-btable')[1].contains(target)) {
                    gd.func('endEdit', gd.ei);
                    gd.ei = null;
                }
            }
        })
    }

    /**
     * Combo：combobox、combotree、tree
     */
    z.Combo = function () { return new z.Combo.fn.init() };

    z.Combo.fn = z.Combo.prototype = {
        init: function () {
            return z.thisInit(this, this.defaultOptions = {
                method: "POST",/*请求类型*/
                queryParams: {},/*请求参数*/
                type: "combobox",/*类型：combobox、combotree tree*/
                //默认拓展清除按钮
                icons: [{
                    iconCls: 'comboclear',
                    handler: function (e) {
                        var tt = $(e.data.target);
                        tt[tt.data().textbox.options.type]('clear');
                    }
                }],
                onBeforeBind: function (ops) { },/*绑定前回调，return false阻止绑定*/
                completeIndex: 0,/*完成事件索引，用于支持多次调用*/
                /*完成事件回调，支持多次调用 (注意死循环，在完成事件重绑数据需加标记跳出循环绑定)*/
                onComplete: function (fn) {
                    this.completeIndex += 1;
                    this["_onComplete" + this.completeIndex] = fn;
                },
                //绑定
                bind: function () {
                    var that = this;

                    if (typeof that.onBeforeLoad == "function") {
                        that._onBeforeLoad = that.onBeforeLoad;
                        delete that.onBeforeLoad;
                    }

                    //绑定前回调
                    if (that.onBeforeBind(that) == false) { return false; }

                    z.hideAttr(this, 'url')

                    $(that.id)[that.type](that);

                    z.hideAttr(this, 'url', false)

                    //拷贝输入框提示信息
                    $(that.id).next().find('input').first().attr('placeholder', $(that.id).attr('placeholder'));

                    //执行完成事件
                    for (var i = 1; i < that.completeIndex + 1; i++) {
                        typeof that["_onComplete" + i] == "function" && that["_onComplete" + i](that);
                    }

                    return this;
                },
                //载入
                load: function () {
                    var that = this;

                    //请求前回调且修改别名（因两次）
                    if (typeof that.onBeforeLoad == "function") {
                        that._onBeforeLoad = that.onBeforeLoad;
                        delete that.onBeforeLoad;
                        if (that._onBeforeLoad(that.queryParams) == false) { return false; }
                    }

                    $.ajax({
                        url: that.url,
                        type: that.method,
                        data: that.queryParams,
                        dataType: "json",
                        success: function (data) {
                            that.dataCache = data;
                            that.data = data || [];
                            that.bind();
                        },
                        error: function (ex) {
                            that.data = [];
                            typeof that.onLoadError == "function" && that.onLoadError(ex);
                        }
                    });
                    return this;
                },
                //方法
                func: function () {
                    return $(this.id)[this.type](arguments[0], arguments[1]);
                }
            })
        }
    };

    z.Combo.fn.init.prototype = z.Combo.fn;

    /**
     * 表单类型映射
     */
    z.FormTypeMap = {
        datetime: "datetimebox",
        date: "datebox",
        time: "timespinner"
    }

    /**
     * Tree模糊选中
     * 一个节点的子节点有部分选中，赋值后子节点被全部选中，用此方法处理，示例参考角色权限设置
     * @param {any} cb z.Combo方法返回的对象
     * @param {any} values 设置的值
     */
    z.TreeVagueCheck = function (cb, values) {
        var nodes = cb.func('getChildren');
        if (nodes.length != values.length) {
            var arrid = [];
            $(nodes).each(function () {
                if (values.indexOf(this.id) == -1) {
                    cb.func('uncheck', cb.func('find', this.id).target);
                }
            });
        }
    }

    /**
     * .formui 单据 请求返回数据源&回调
     */
    z.FormAttrAjax = function () {
        $('form.formui').find('*').each(function () {
            var that = $(this), type = that.attr("data-type"), url = that.attr('data-url');
            if (type) {
                if (url) {
                    url = url.toLowerCase();
                    if (!(url in z.DC)) {
                        z.DC[url] = {};
                    }
                    if (z.DC[url].data) {
                        z.FormAttrBind(that);
                    } else {
                        $.ajax({
                            url: url,
                            type: "post",
                            dataType: "json",
                            success: function (data) {
                                z.DC[url].data = data;
                                z.FormAttrBind(that);
                            },
                            error: function (e) {
                                z.DC[url].data = [];
                                z.FormAttrBind(that);
                            }
                        })
                    }
                } else {
                    z.FormAttrBind(that);
                }
            }
        })
    }

    /**
     * 表单类型源绑定
     * data-type：扩展类型：combotree、combobox、datetime、date、time、modal
     * data-url：请求url 【combo类型时：且为本地数据 z.DC对象的键，绑定源为 z.DC[data-url] 不区分大小写】
     * data-state：状态，绑定后值为 bind
     * @param {any} target 带特性值的节点
     */
    z.FormAttrBind = function (target) {
        target = $(target);
        var dtype = target.attr("data-type"), url = target.attr('data-url'), state = target.attr('data-state');

        if (state) { return false; }

        switch (dtype) {
            case 'combotree':
            case 'combobox':
                if (!url) { return false }
                url = url.toLowerCase();
                if (url in z.DC) {
                    var cb = z.Combo();
                    cb.id = target;
                    cb.data = z.DC[url].data;
                    cb.height = 33;
                    if (dtype == "combobox") {
                        cb.panelHeight = Math.min(7, cb.data.length) * cb.height + 5;
                    }
                    cb.editable = false;
                    cb.type = dtype;
                    if (typeof z.DC[url].init == "function") {
                        z.DC[url].init.call(cb, cb);
                    }
                    cb.DCkey = url;
                    cb.bind();
                    z.DC[url].obj = cb;
                    cb.func('textbox').attr('placeholder', target.attr('placeholder'));
                    //存储创建的对象
                    target[0].zobj = cb;
                    target.attr('data-state', 'bind');
                }
                break;
            case 'datetime':
            case 'date':
            case 'time':
                var dtfn = z.FormTypeMap[dtype];
                target[dtfn]({ showSeconds: true });
                target.data('textbox').textbox.find('input').first().attr('placeholder', target.attr('placeholder'));
                break;
            case 'modal':
                {
                    if (!url) { return false }

                    var mo = new z.Modal();

                    mo.src = url;
                    mo.size = 4;
                    mo.title = '<i class="fa fa-search blue"></i><span>选择<span>';
                    mo.cancelText = '<i class="fa fa-close"></i> 取消';
                    mo.cancelClick = function () { this.hide() }
                    mo.okText = '<i class="fa fa-check"></i> 选择并关闭';

                    //不允许输入
                    mo.editable = false;

                    //每次刷新
                    mo.autoRefresh = false;

                    url = url.toLowerCase();

                    if (url in z.DC) {
                        if (typeof z.DC[url].init == "function") {
                            z.DC[url].init.call(mo, mo);
                        }
                    }

                    if (!mo.editable) {
                        target[0].readOnly = true;
                    }

                    z.DC[url].obj = mo;

                    //为图标绑定点击事件
                    target.next().click(function () {
                        if (!mo.modal) {
                            mo.append();
                            mo.modal.attr('data-backdrop', 'static');
                            z.FormAutoHeight();
                        } else {
                            //自动刷新
                            if (mo.autoRefresh) {
                                var ifr = mo.modal.find('iframe');
                                ifr[0].src = ifr[0].src;
                            }
                        }
                        mo.guide = target;
                        mo.guidetype = "form";
                        mo.show();
                    });

                    //模拟点击浏览图标
                    target.click(function () {
                        if (!mo.editable) {
                            target.next()[0].click();
                        }
                    });

                    target.attr('data-state', 'bind');
                }
                break;
        }
    }

    /**
     * 必填验证
     * @param {string} color 边框颜色 (reset 重置边框)
     * @param {any} FormId 表单#ID 默认 #fv_form_1
     * @param {boolean} dialog 弹出提示 true不弹出
     */
    z.FormRequired = function (color, FormId, dialog) {
        var result = true;
        var form = $(FormId || "#fv_form_1"), arrLab = [];
        form.find('label.required').each(function () {
            var ipt = $(this).parent().find('input,select,textarea').first(),
                dtype = ipt.attr('data-type') || ipt[0].type, val = '', colorTarget;
            switch (dtype) {
                case 'combobox':
                case 'combotree':
                    val = ipt[dtype](ipt[dtype]("options").multiple ? 'getValues' : 'getValue');
                    colorTarget = ipt.next();
                    break;
                case 'datetime':
                case 'date':
                case 'time':
                    val = ipt[z.FormTypeMap[dtype]]('getValue');
                    colorTarget = ipt.next();
                    break;
                case 'file':
                    val = $('#hid_' + ipt[0].id).val();
                    colorTarget = ipt;
                    break;
                default:
                    val = ipt.val();
                    colorTarget = ipt;
                    break;
            }
            if (color == "reset") {
                colorTarget.css('border-color', '');
            } else {
                if (val && val.length) {
                    colorTarget.css('border-color', '');
                } else {
                    arrLab.push($(this).text());
                    colorTarget.css('border-color', color || "red");
                    result = false;
                }
            }
        });
        if (!result && !dialog) {
            var mo = art('<div style="font-size:initial;">' + arrLab.join('</br>') + '</div>');
            mo.modal.find('h4.modal-title').html('<b class="red">✶ 为必填</b>');
        }
        return result;
    }

    /**
     * 查找树节点
     * @param {any} data 层级树
     * @param {any} value 值
     * @param {any} key 键
     */
    z.FindTreeNode = function (data, value, key) {
        var node, len = data.length;
        for (var i = 0; i < len; i++) {
            var item = data[i];
            if (item[key] == value) {
                node = item;
            } else if (item.children) {
                node = arguments.callee(item.children, value, key);
            }
            if (node) {
                break;
            }
        }
        if (node) {
            return node;
        }
    }

    /**
     * 回填表单，查找ID为key的节点并赋值
     * @param {object} rowData Grid（选中的）一行数据
     */
    z.FormEdit = function (rowData) {
        for (var i in rowData) {
            var t = $('#' + i), rdi = rowData[i], dtype = t.attr('data-type') || t.attr('type'), durl = t.attr('data-url');
            switch (dtype) {
                case 'combobox':
                    if (rdi != null) {
                        if (t[dtype]("options").multiple) {
                            var arr = [];
                            if ($.trim(rdi) != "") {
                                arr = $.trim(rdi).split(',');
                            }
                            t[dtype]('setValues', arr);
                        } else {
                            t[dtype]('setValue', rdi.toString());
                        }
                    }
                    break;
                case 'combotree':
                    t[dtype]('reset');
                    t[dtype]('textbox').val(t[dtype]('getText'));

                    var oda = z.DC[durl.toLowerCase()].obj.data;
                    if (t[dtype]("options").multiple && rdi != null) {
                        var arr = [];
                        if ($.trim(rdi) != "") {
                            arr = $.trim(rdi).split(',');
                        }
                        var nodes = [];
                        for (var u = 0; u < arr.length; u++) {
                            var node = z.FindTreeNode(oda, arr[u], "id");
                            if (node) {
                                nodes.push(node);
                            }
                        }
                        if (nodes.length) {
                            t[dtype]('setValues', nodes);
                        }
                    } else {
                        var node = z.FindTreeNode(oda, rdi, "id");
                        if (node) {
                            t[dtype]('setValue', node);
                        }
                    }
                    break;
                case 'checkbox':
                case 'radio':
                    t[0].checked = rdi == 1;
                    break;
                case 'datetime':
                case 'date':
                case 'time':
                    t[z.FormTypeMap[dtype]]('setValue', rdi || "");
                    break;
                case 'file':
                    $('#hid_' + t[0].id).val(rdi);
                    break;
                default:
                    try { t.val(rdi); } catch (e) { }
                    break;
            }
        }
    }

    /**
     * 编辑表单组装成JSON对象 
     * @param {any} FormId 表单#ID 默认 #fv_form_1
     */
    z.FormToJson = function (FormId) {
        var jform = $(FormId || '#fv_form_1'), arrData = jform.serializeArray(), row = {};
        jform.find('input').each(function () {
            if ("checkbox,radio".indexOf(this.type) >= 0) {
                arrData.push({ name: this.name, value: this.checked ? 1 : 0 });
            }
        });

        $(arrData).each(function () {
            var ipt = $('#' + this.name), dtype = ipt.attr('data-type') || ipt.attr('type');
            switch (dtype) {
                case 'combobox':
                case 'combotree':
                    if (ipt[dtype]("options").multiple) {
                        var arr = row[this.name] || [];
                        arr.push(this.value);
                        row[this.name] = arr;
                    } else {
                        row[this.name] = this.value;
                    }
                    break;
                case 'datetime':
                case 'date':
                case 'time':
                    row[this.name] = ipt[z.FormTypeMap[dtype]]('getValue');
                    break;
                case 'checkbox':
                case 'radio':
                    row[this.name] = ipt[0].checked ? 1 : 0;
                    break;
                case 'file':
                    try { row[this.name] = $('#hid_' + ipt[0].id).val(); } catch (e) { }
                    break;
                default:
                    dtype && (row[this.name] = this.value);
                    break;
            }
        });
        for (var i in row) {
            var item = row[i];
            if ($.isArray(item)) {
                row[i] = item.join(',');
            }
        }
        return row;
    }

    /**
     * 清空Form
     * @param {any} FormId 表单#ID 默认 #fv_form_1
     */
    z.FormClear = function (FormId) {
        var fm = $(FormId || "#fv_form_1");
        if (fm.length) {
            fm[0].reset();
            fm.find('label.control-label').each(function () {
                var ipt = $(this).next().find('input,select,textarea').first(), dtype = ipt.attr('data-type');
                switch (dtype) {
                    case "combobox":
                    case "combotree":
                        ipt[dtype]('reset');
                        ipt[dtype]('textbox').val(ipt[dtype]('getText'));
                        break;
                    case "datetime":
                    case "date":
                    case "time":
                        ipt[z.FormTypeMap[dtype]]('setValue', '');
                        break;
                }
            });
        }
    }

    /**
     * 禁用Form 
     * @param {boolean} dd true禁用 false启用
     * @param {any} FormId 表单#ID 默认 #fv_form_1
     */
    z.FormDisabled = function (dd, FormId) {
        var fm = $(FormId || "#fv_form_1");
        fm.find('input,select,textarea').each(function () {
            var ipt = $(this), dtype = ipt.attr('data-type'), icon = ipt.next();
            switch (dtype) {
                case "combobox":
                case "combotree":
                    ipt[dtype](dd ? 'disable' : 'enable');
                    break;
                case "datetime":
                case "date":
                case "time":
                    if (dd) {
                        ipt[z.FormTypeMap[dtype]]('options').disabled = true
                        icon.find('.textbox-icon').hide();
                    } else {
                        ipt[z.FormTypeMap[dtype]]('options').disabled = false;
                        icon.find('.textbox-icon').show();
                    }
                    break;
                default:
                    ipt.attr('disabled', dd);
                    if (icon.length) {
                        if (dd) {
                            icon.hide();
                        } else {
                            icon.show();
                        }
                    }
                    break;
            }
        });
        var footer = fm.parent().next();
        if (dd) {
            footer.addClass('hidden')
        } else {
            footer.removeClass('hidden');
        }
        z.FormAutoHeight();
    }

    /**
     * 模态框表单调整高度
     */
    z.FormAutoHeight = function () {
        var ch = $(window).height();
        //模态框高度自适应
        var mah = $('div.modalautoheight').each(function () {
            var mitem = $(this), tps = mitem.find('.modalscroll'), hf = 125;
            //是否有页脚
            if (mitem.find('div.modal-footer').hasClass('hidden')) {
                hf -= 51;
            }
            //有自定义出现滚动条的节点
            if (tps.length) {
                //面板
                var dh = 65;
                //iframe
                if (tps[0].nodeName == "IFRAME") {
                    dh = 5;
                }
                tps.css('max-height', ch - hf - dh);
            } else {
                //默认模态框主体
                mitem.find('div.modal-body').css('max-height', ch - hf);
            }
        });
    }

    /**
     * 表单标题设置
     * @param {any} ops 
     * icon 标题图标
     * title 标题
     * id 标题容器ID或对象
     * required 是否显示必填的提示文字
     */
    z.FormTitle = function (ops) {
        var icon = ops.icon, required = ops.required == false ? false : true;
        icon == 0 && (icon = "fa-plus blue");
        icon == 1 && (icon = "fa-edit orange");
        icon == 2 && (icon = "fa-credit-card blue");
        var htm = [];
        htm.push('<i class="fa ' + icon + '" ></i><span>' + ops.title + '</span>');
        required && htm.push(' <em>✶ 为必填</em>');
        $(ops.id || "#fv_title_1").html(htm.join(''));
    }

    //Modal
    z.Modal = function () { return new z.Modal.prototype.init() };

    z.Modal.fn = z.Modal.prototype = {
        init: function () {
            z.index += 1;
            //ID
            this.id = "myModal_" + z.index;
            return this;
        },
        //ok按钮文本
        okText: "确定",
        //cancel按钮文本
        cancelText: "取消",
        //Ok点击回调
        okClick: null,
        //Cancel点击回调
        cancelClick: null,
        //标题内容
        title: "消息",
        //主体内容
        content: "",
        //iframe地址
        src: "",
        //iframe高度
        heightIframe: 900,
        //iframe加载完成回调
        complete: null,
        //模态大小 默认2 可选：1|2|3
        size: 2,
        //显示右上角关闭按钮
        showClose: true,
        //显示页脚
        showFooter: true,
        //显示Cancel按钮
        showCancel: true,
        //追加到Body
        append: function () {
            var that = this, size = 'modal-dialog', htm = [];
            switch (this.size) {
                case 1: size += " modal-sm"; break;
                case 3: size += " modal-lg"; break;
                case 4: size += " modal-full"; break;
            }
            htm.push('<div class="' + size + '"><div class="modal-content"><div class="modal-header">');
            if (that.showClose) {
                htm.push('<button class="close" data-dismiss="modal" aria-label="Close"><span>&times;</span></button>');
            }
            htm.push('<h4 class="modal-title">' + this.title + '</h4></div><div class="modal-body">' + this.content);
            htm.push('</div><div class="modal-footer"><button class="btn btn-white btnCancel" >' + this.cancelText + '</button>');
            htm.push('<button class="btn btn-primary btnOk" >' + this.okText + '</button></div></div></div>');
            var mo = document.createElement('div');
            mo.id = this.id;
            mo.className = "modal fade modalautoheight";
            mo.setAttribute('role', 'dialog');
            mo.innerHTML = htm.join('');
            document.body.appendChild(mo);
            this.modal = $(mo);

            if (this.src.length) {
                var ifr = document.createElement('iframe');
                ifr.src = this.src;
                ifr.frameBorder = 0;
                ifr.scrolling = "auto";
                ifr.className = "modalscroll";
                ifr.style.cssText = "width:100%;height:" + this.heightIframe + "px;";
                $(mo).find('div.modal-body').css('padding', 0).append(ifr);
                $(ifr).load(function () {
                    typeof that.complete == "function" && that.complete.call(that);
                })
            }

            var footer = $(mo).find('div.modal-footer'),
                btnOk = $(mo).find('button.btnOk'),
                btnCancel = $(mo).find('button.btnCancel');
            !this.showFooter && footer.hide();
            !this.showCancel && btnCancel.hide();

            btnOk.click(function () {
                typeof that.okClick == "function" && that.okClick.call(that, btnOk)
            });
            btnCancel.click(function () {
                typeof that.cancelClick == "function" && that.cancelClick.call(that, btnCancel)
            });

            return this;
        },
        //显示模态
        show: function () { $('#' + this.id).modal(); return this; },
        //隐藏模态
        hide: function () { $('#' + this.id).modal('hide'); return this; },
        //移除模态
        remove: function () {
            var mo = $('#' + this.id);
            mo.on('hidden.bs.modal', function () { mo.remove() });
            mo.modal('hide');
            return this;
        }
    };

    z.Modal.fn.init.prototype = z.Modal.fn;


    //Sql查询
    z.SqlQuery = function () { return new z.SqlQuery.fn.init() }

    z.SqlQuery.fn = z.SqlQuery.prototype = {
        init: function () {
            this.wheres = [];
            return this;
        },
        //返回空的项条件
        item: function () {
            return {
                field: "",
                relation: "",
                value: ""
            }
        },
        //填充查询条件
        wheres: [],
        //查询条件序列化为字符串
        stringify: function () {
            return JSON.stringify(this.wheres);
        },
        //私有属性（做说明）
        private: {
            //关系符说明
            relations: {
                Equal: '{0} = {1}',
                NotEqual: '{0} != {1}',
                LessThan: '{0} < {1}',
                GreaterThan: '{0} > {1}',
                LessThanOrEqual: '{0} <= {1}',
                GreaterThanOrEqual: '{0} >= {1}',
                BetweenAnd: '{0} >= {1} AND {0} <= {2}',
                Contains: '%{0}%',
                StartsWith: '{0}%',
                EndsWith: '%{0}',
                In: 'IN',
                NotIn: 'NOT IN'
            },
            //示例格式 
            //id1='1' AND id2 IN('1','2') AND id2 LIKE '%5%' AND id3>='5' AND id3<='10'
            wheres: [
                {
                    field: "id1",
                    relation: "Equal",
                    value: 1
                },
                {
                    field: "id2",
                    relation: "In",
                    value: [1, 2]
                },
                {
                    field: "id2",
                    relation: "Contains",
                    value: "5"
                },
                {
                    field: "id3",
                    relation: "BetweenAnd",
                    value: [5, 10]
                }
            ]
        }
    };

    z.SqlQuery.fn.init.prototype = z.SqlQuery.fn;

    //全屏
    z.FullScreen = {
        //iframe全屏
        iframe: function (ifs) {
            z.ifs = !(z.ifs || false);
            if (ifs != null) {
                z.ifs = ifs;
            }
            if (z.ifs) {
                this.open();
            } else {
                this.close();
            }
        },
        //打开
        open: function () {
            var element = document.documentElement;
            if (element.requestFullscreen) {
                element.requestFullscreen();
            } else if (element.msRequestFullscreen) {
                element.msRequestFullscreen();
            } else if (element.mozRequestFullScreen) {
                element.mozRequestFullScreen();
            } else if (element.webkitRequestFullscreen) {
                element.webkitRequestFullscreen();
            }
        },
        //关闭
        close: function () {
            if (document.exitFullscreen) {
                document.exitFullscreen();
            } else if (document.msExitFullscreen) {
                document.msExitFullscreen();
            } else if (document.mozCancelFullScreen) {
                document.mozCancelFullScreen();
            } else if (document.webkitExitFullscreen) {
                document.webkitExitFullscreen();
            }
        },
        //是否打开
        isopen: function () {
            return document.isFullScreen || document.mozIsFullScreen || document.webkitIsFullScreen
        }
    };

    //数据缓存，以 url 为键时是小写
    z.DC = {};

    //操作按钮容器
    z.ButtonBox = '#BtnMenu';

    //按钮触发标识
    z.btnTrigger = null;

    /*操作按钮事件*/
    z.button = function (type, fn) { $(z.ButtonBox).on(type, fn) }

    /**
     * 模拟操作按钮点击
     * @param {string} type 按钮标识，不要m_
     */
    z.buttonClick = function (type) {
        var usable = false;
        $(z.ButtonBox).find('button').each(function () {
            if (this.id.toLowerCase().replace('m_', '') == type) {
                usable = true;
                if ($(this).hasClass('disabled')) {
                    usable = false;
                }
                return false;
            }
        });
        if (usable) {
            z.btnTrigger = type;
            $(z.ButtonBox).trigger(type);
        }
    }

    window.z = z;

})(window, undefined);

/*Ext*/

if ($.fn.datagrid) {
    var ge = $.fn.datagrid.defaults.editors;

    //text 文本框
    ge.text.init = function (container, options) {
        var input = $('<input class="datagrid-editable-input" />').appendTo(container);
        input[0].focus();
        return input;
    }

    //password 文本框
    ge.password = {
        init: function (container, options) {
            var input = $('<input class="datagrid-editable-input" type="password" />').appendTo(container);
            input[0].focus();
            return input;
        },
        resize: function (target, width) { target.css('width', width) },
        getValue: function (target) { return target.val() },
        setValue: function (target, value) { target.val(value) }
    };

    //checkbox 复选框
    ge.checkbox = {
        init: function (container, options) {
            var input = $('<input type="checkbox" class="selectbig" value="1" style="margin:4px" />').appendTo(container);
            return input;
        },
        getValue: function (target) { return target[0].checked ? 1 : 0 },
        setValue: function (target, value) { target[0].checked = value }
    };

    //公共部分
    var dtobj = function (key) {
        var ftm = z.FormTypeMap[key];
        return {
            init: function (container, options) {
                var input = $('<input class="datagrid-editable-input" />').appendTo(container);
                input[ftm]();
                try {
                    //为日期面板添加鼠标点击阻止冒泡（因与点击空白结束编辑冲突）
                    input.data('combo').panel.mousedown(function (e) {
                        z.stopEvent(e)
                    });
                } catch (e) { }
                return input;
            },
            resize: function (target, width) { target.css('width', width) },
            getValue: function (target) { return target[ftm]('getValue') },
            setValue: function (target, value) { target[ftm]('setValue', value) }
        }
    }

    //datetime 日期时间
    ge.datetime = dtobj("datetime");

    //date 日期
    ge.date = dtobj("date");

    //time 时间
    ge.time = dtobj("time");

    //modal 模态框
    ge.modal = {
        init: function (container, options) {
            var input = $('<div class="dimodal"><input type="text" class="datagrid-editable-input"><i class="fa fa-search form-control-feedback"></i></div>').appendTo(container);
            var mo = new z.Modal(), url = options.url.toLowerCase();

            if (url in z.DC && z.DC[url].obj) {
                mo = z.DC[url].obj;
            } else {
                mo.src = options.url;
                mo.size = 4;
                mo.title = '<i class="fa fa-search blue"></i><span>选择<span>';
                mo.cancelText = '<i class="fa fa-close"></i> 取消';
                mo.cancelClick = function () { this.hide() }
                mo.okText = '<i class="fa fa-check"></i> 选择并关闭';

                //不允许输入
                mo.editable = false;

                //每次刷新
                mo.autoRefresh = false;
            }

            if (typeof z.DC[url].init == "function") {
                z.DC[url].init.call(mo, mo);
            }

            z.DC[url].obj = mo;

            if (!mo.editable) {
                input.find('input')[0].readOnly = true;
            }

            input.find('i').click(function () {
                if (!mo.modal) {
                    mo.append();
                    mo.modal.attr('data-backdrop', 'static');
                    z.FormAutoHeight();
                }
                mo.show();
            });

            input.find('input').click(function () {
                if (!mo.editable) {
                    input.find('i')[0].click();
                }
            })[0].focus();

            mo.guidetype = "table";

            return mo.guide = input;
        },
        resize: function (target, width) {
            target.find('input').css('width', width)
        },
        getValue: function (target) {
            return $(target).find("input").val()
        },
        setValue: function (target, value) {
            var input = $(target).find("input");
            input.val(value);
        }
    };
}

/*语言包*/
if ($.fn.pagination) {
    $.fn.pagination.defaults.beforePageText = '第';
    $.fn.pagination.defaults.afterPageText = '共 {pages} 页';
    $.fn.pagination.defaults.displayMsg = '显示 <b>{from}</b> 到 <b>{to}</b>，共 <b>{total}</b> 记录';
}
if ($.messager) {
    $.messager.defaults.ok = '确定';
    $.messager.defaults.cancel = '取消';
}
$.map(['validatebox', 'textbox', 'passwordbox', 'filebox', 'searchbox',
    'combo', 'combobox', 'combogrid', 'combotree',
    'datebox', 'datetimebox', 'numberbox',
    'spinner', 'numberspinner', 'timespinner', 'datetimespinner'], function (plugin) {
        if ($.fn[plugin]) {
            $.fn[plugin].defaults.missingMessage = '该输入项为必输项';
        }
    });
if ($.fn.validatebox) {
    $.fn.validatebox.defaults.rules.email.message = '请输入有效的电子邮件地址';
    $.fn.validatebox.defaults.rules.url.message = '请输入有效的URL地址';
    $.fn.validatebox.defaults.rules.length.message = '输入内容长度必须介于{0}和{1}之间';
    $.fn.validatebox.defaults.rules.remote.message = '请修正该字段';
}
if ($.fn.calendar) {
    $.fn.calendar.defaults.weeks = ['日', '一', '二', '三', '四', '五', '六'];
    $.fn.calendar.defaults.months = ['一月', '二月', '三月', '四月', '五月', '六月', '七月', '八月', '九月', '十月', '十一月', '十二月'];
}
if ($.fn.datebox) {
    $.fn.datebox.defaults.currentText = '今天';
    $.fn.datebox.defaults.closeText = '关闭';
    $.fn.datebox.defaults.okText = '确定';
    $.fn.datebox.defaults.formatter = function (date) {
        var y = date.getFullYear();
        var m = date.getMonth() + 1;
        var d = date.getDate();
        return y + '-' + (m < 10 ? ('0' + m) : m) + '-' + (d < 10 ? ('0' + d) : d);
    };
    $.fn.datebox.defaults.parser = function (s) {
        if (!s) return new Date();
        var ss = s.split('-');
        var y = parseInt(ss[0], 10);
        var m = parseInt(ss[1], 10);
        var d = parseInt(ss[2], 10);
        if (!isNaN(y) && !isNaN(m) && !isNaN(d)) {
            return new Date(y, m - 1, d);
        } else {
            return new Date();
        }
    };
}
if ($.fn.datetimebox && $.fn.datebox) {
    $.extend($.fn.datetimebox.defaults, {
        currentText: $.fn.datebox.defaults.currentText,
        closeText: $.fn.datebox.defaults.closeText,
        okText: $.fn.datebox.defaults.okText
    });
}
if ($.fn.datetimespinner) {
    $.fn.datetimespinner.defaults.selections = [[0, 4], [5, 7], [8, 10], [11, 13], [14, 16], [17, 19]]
}

/**
 * 消息提示或询问
 * @param {any} content 提示内容
 * @param {any} fnOk 确认回调
 * @param {any} fnCancel 取消回调 缺省时不显示取消按钮
 */
function art(content, fnOk, fnCancel) {
    var htm = [], ctype = '';
    htm.push('<div class="alert alert-');
    switch (content) {
        case "fail": ctype = "danger"; content = "操作失败"; break;
        case "error": ctype = "danger"; content = "网络错误"; break;
        case "success": ctype = "success"; content = "操作成功"; break;
        case "select": ctype = "warning"; content = "请选择一行再操作"; break;
        default: ctype = arguments.length == 1 ? "info" : "warning"; break;
    };
    htm.push(ctype + '" role="alert" style="font-size:1.6em;margin:0;">' + content + '</div>');

    var mo = z.Modal();
    mo.size = 2;
    mo.content = htm.join('');
    if (arguments.length == 1) {
        mo.showCancel = false;
        mo.okClick = function () { this.remove() };
    } else {
        mo.okClick = function () { this.remove(); fnOk.call(mo); };
        mo.cancelClick = arguments.length == 3 ? function () { this.remove(); fnCancel.call(mo) } : function () { this.remove(); };
    }
    mo.append().show();
    mo.modal.on('hidden.bs.modal', function () { mo.remove() })[0].focus();
    mo.modal.find(arguments.length == 1 ? "button.btnOk" : "button.btnCancel")[0].focus();
    return mo;
}


//DOM载入后执行
setTimeout(function () {
    //样式设置
    try {
        var rf = top.rf,
            fs = rf.ls(rf.key.fontsize),
            ff = rf.ls(rf.key.fontfamily),
            bs = rf.ls(rf.key.btntype);
        fs && (document.body.style["font-size"] = fs),
            ff && (document.body.style["font-family"] = ff);
        bs == 1 && ($(z.ButtonBox).removeClass('bgwhite'));
    } catch (e) { }

    //按钮区点击委托
    $(z.ButtonBox).click(function (e) {
        var target = z.target(e), that = $(this);
        if (target.nodeName == "BUTTON" || target.nodeName == "I") {
            that.find('button').each(function () {
                if (this.contains(target)) {
                    var stop = $(this).hasClass('disabled');
                    if (!stop && !$(this).data('stop')) {
                        z.btnTrigger = this.id.toLowerCase().replace('m_', '');
                        that.trigger(z.btnTrigger);
                    }
                    return false;
                }
            })
        }
    });

    //透明
    document.body.style.opacity = 1;
}, 0);

/**
 * 载入调整窗口大小
 * 表单高度自适应
 */
$(window).on("load resize", function () {
    z.FormAutoHeight();
});