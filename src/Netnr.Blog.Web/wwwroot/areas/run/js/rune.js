//Monaco
var editor1, editor2, editor3;
require(['vs/editor/editor.main'], function () {
    var setheme = $('.nrTheme'), theme = setheme.attr('data-value');
    setheme.val(theme);

    $('#rune').children().each(function (i) {
        var that = this;
        var lang = this.getAttribute('data-lang');
        if (i < 3) {
            var tc = htmlDecode(this.innerHTML);
            this.innerHTML = "";

            let editor = monaco.editor.create(this, {
                value: tc,
                language: lang,
                automaticLayout: true,
                theme: theme,
                roundedSelection: true,
                scrollBeyondLastLine: true,
                fontSize: 18,
                scrollbar: {
                    verticalScrollbarSize: 9,
                    horizontalScrollbarSize: 9
                },
                minimap: {
                    enabled: false
                }
            });
            window["editor" + (i + 1)] = editor;

            //快捷键
            editor.addCommand(monaco.KeyMod.CtrlCmd | monaco.KeyCode.KeyM, function () {
                $(that).children('button')[0].click();
            })
            editor.addCommand(monaco.KeyCode.PauseBreak, function () {
                RunPreview();
            })

            //载入最后一次运行
            editor.addAction({
                id: "meid-run-last",
                label: "恢复为最后一次运行",
                keybindings: [monaco.KeyMod.Alt | monaco.KeyCode.KeyL],
                contextMenuGroupId: "me-01",
                run: function () {
                    try {
                        var json = localStorage.getItem("run_last")
                        if (json) {
                            json = JSON.parse(json);

                            editor1.setValue(json.html);
                            editor2.setValue(json.javascript);
                            editor3.setValue(json.css);
                        }
                    } catch (e) {
                        console.debug(e)
                        alert("Fail")
                    }
                }
            });

            //CSS格式化
            if (i == 2) {
                monaco.languages.registerDocumentFormattingEditProvider('css', {
                    provideDocumentFormattingEdits: function (model, options, _token) {
                        return [{
                            text: cssFormatter(model.getValue(), options.tabSize),
                            range: model.getFullModelRange()
                        }];
                    }
                });
                //editor.addCommand(monaco.KeyMod.Alt | monaco.KeyMod.Shift | monaco.KeyCode.KeyF, function () {
                //    editor3.setValue(cssFormatter(editor3.getValue()))
                //})
            }
        }
        $(this).css('padding-left', 0);

        //窗口漂浮按钮
        $('<button class="fre btn btn-outline-success px-1 py-0">' + lang + '<i class="fa fa-arrows-alt fa-fw"></i></button>').appendTo(this).click(function () {
            var re = $('#rune');
            if (re.hasClass('rune-full')) {
                re.removeClass('rune-full');
                $(this).parent().removeClass('rune-full-pre');
            } else {
                re.addClass('rune-full');
                $(this).parent().addClass('rune-full-pre');
            }
        });
    });

    //主题
    $('.nrTheme').change(function () {
        var theme = this.value;
        monaco.editor.setTheme(theme);

        switch (theme) {
            case "vs":
                $('.nrRunBox').removeClass("bg-vs-dark").addClass("bg-vs");
                $('#runav').removeClass("navbar-dark bg-dark").addClass("navbar-light bg-light");
                break;
            default:
                $('.nrRunBox').removeClass("bg-vs").addClass("bg-vs-dark");
                $('#runav').addClass("navbar-dark bg-dark").removeClass("navbar-light bg-light");
                break;
        }
    });

    RunPreview(1);
});

//preview
function RunPreview(isInit) {
    var iframebox = $('.re4');
    iframebox.children('iframe').remove();

    var e1 = editor1.getValue(), e2 = editor2.getValue().trim(), e3 = editor3.getValue().trim();

    //存储运行
    if (!isInit) {
        localStorage.setItem("run_last", JSON.stringify({ html: e1, javascript: e2, css: e3 }))
    }

    var iframe = document.createElement('iframe');
    iframe.name = "Run preview";
    iframebox.append(iframe);
    if (e1.includes("</head>") && e1.includes("</body>")) {
        if (e3 != "") {
            e1 = e1.replace("</head>", `<style>${e3}</style></head>`);
        }
        if (e2 != "") {
            e1 = e1.replace("</body>", `<script>${e2}</script></body>`);
        }
    } else {
        iframe.onload = function () {
            if (e3 != "") {
                var style = document.createElement("STYLE");
                style.innerHTML = e3;
                iframe.contentWindow.document.head.appendChild(style);
            }

            if (e2 != "") {
                var script = document.createElement("SCRIPT");
                script.innerHTML = e2;
                iframe.contentWindow.document.body.appendChild(script);
            }
        }
    }
    iframe.contentWindow.document.open();
    iframe.contentWindow.document.write(e1);
    iframe.contentWindow.document.close();
}

//保存
function SaveRun(st) {
    var msg = [];

    var post = {
        RunCode: $('#hidCode').val(),
        RunRemark: $('#txtRemark').val(),
        RunTheme: $('.nrTheme').val(),
        RunContent1: editor1.getValue(),
        RunContent2: editor2.getValue(),
        RunContent3: editor3.getValue()
    };

    if (post.RunRemark.trim() == "") {
        msg.push('description is empty');
    }
    if (post.RunRemark.trim().length > 150) {
        msg.push('description content is too long ( less than 150 )');
    }
    var rclen = post.RunContent1 + post.RunContent2 + post.RunContent3;
    if (rclen.length == 0) {
        msg.push('code is empty');
    }

    if (rclen > 10000 * 50) {
        msg.push('code content is too long ( less than 500000 )');
    }

    if (msg.length) {
        alert(msg.join('\r\n'));
        return false;
    }

    $.ajax({
        url: '/run/home/SaveRun',
        data: post,
        type: "POST",
        dataType: 'json',
        success: function (data) {
            if (data.code == 200) {
                location.href = "/run/code/" + data.data;
            } else if (data.code == 403) {
                alert("It's not belongs to you");
            } else {
                alert(data.msg);
            }
        },
        error: function (ex) {
            if (ex.status == 401) {
                alert("Please login first");
            } else {
                alert('error');
            }
        }
    })
}

//ref
$('.nrRef').on('keydown', function (e) {
    if (e.keyCode == 13) {
        jsd.search(this.value);
    }
});
$('.nrRefList').click(function (e) {
    var target = e.target;
    if (target.nodeName == "A") {
        jsd.selected(target);
    }
});

var jsd = {
    key: null,
    buildItem: function (name, dir, type) {
        type = type ? type : "file";
        dir = dir ? dir : "";
        var cls = "", empty = [], pn = dir == "" ? 0 : dir.split('/').length - 1;
        while (pn--) {
            empty.push("-")
        }
        if (empty.length) {
            empty = empty.join(' ') + " ";
        } else {
            empty = "";
        }
        if (["file", "version"].indexOf(type) == -1) {
            cls = "disabled";
        }
        return `<a class="dropdown-item ${cls}" data-stopPropagation="true" href="javascript:void(0)" data-type="${type}" data-dir="${dir + name}">${empty}${name}</a>`;
    },
    filesEach: function (files, directoryName) {
        directoryName = directoryName || "";

        var parr = [];
        files.forEach(item => {
            if (item.type == "file") {
                parr.push(jsd.buildItem(item.name, directoryName));
            } else if (item.type == "directory") {
                parr.push(jsd.buildItem(item.name, directoryName, item.type));
                parr = parr.concat(arguments.callee(item.files, directoryName + item.name + "/"));
            }
        })
        return parr;
    },
    search: function (key) {
        $('.nrRef').dropdown('show');

        var vh = Math.max(100, $(window).height() - 100);
        $('.nrRefList').css('max-height', vh);

        jsd.key = key;
        fetch("https://data.jsdelivr.com/v1/package/npm/" + key).then(x => x.json()).then(res => {
            console.log(res);
            var htm = [];
            if (res.versions) {
                res.versions.forEach(item => {
                    htm.push(jsd.buildItem(item, "", "version"));
                })
            } else if (res.files) {
                htm = jsd.filesEach(res.files);

                if (res.default) {
                    if (res.default[0] == "/") {
                        res.default = res.default.substr(1);
                    }
                    htm.splice(0, 0, jsd.buildItem(res.default));
                }
            } else {
                htm.push(jsd.buildItem("empty", "", "empty"));
            }
            $('.nrRefList').html(htm.join(''));
        }).catch(err => {
            console.log(err);
            $('.nrRefList').html(jsd.buildItem("empty", "", "empty"));
        })
    },
    selected: function (item) {
        switch (item.getAttribute('data-type')) {
            case "file":
                {
                    var dir = item.getAttribute('data-dir');
                    var text = `https://cdn.jsdelivr.net/npm/${jsd.key}/${dir}`;
                    switch (dir.split('.').pop()) {
                        case "js":
                            text = `<script src="${text}"></script>`
                            break;
                        case "css":
                            text = `<link rel="stylesheet" href="${text}" />`
                            break;
                    }
                    text += "\n";

                    var gse = editor1.getSelection();
                    var range = new monaco.Range(gse.startLineNumber, gse.startColumn, gse.endLineNumber, gse.endColumn);
                    var op = { identifier: { major: 1, minor: 1 }, range: range, text: text, forceMoveMarkers: true };
                    editor1.executeEdits("", [op]);
                }
                break;
            case "version":
                $('.nrRef').val(jsd.key + "@" + item.getAttribute('data-dir'));
                jsd.search($('.nrRef').val());
                $('.nrRef')[0].focus();
                break;
        }
    }
}

function cssFormatter(css, tabSize) {
    try {
        return beautifier.css(css, {
            indent_size: tabSize || 4,
            "max-preserve-newlines": 2
        });
    } catch (e) {
        console.log(e);
        return null;
    }
}

$("body").on('click', '[data-stopPropagation]', function (e) {
    e.stopPropagation();
});