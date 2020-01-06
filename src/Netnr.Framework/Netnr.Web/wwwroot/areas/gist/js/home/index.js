var editor = null;
require(['vs/editor/editor.main'], function () {
    var modesIds = monaco.languages.getLanguages().map(function (lang) { return lang.id }).sort();

    var te = $("#editor"), selang = $('#selanguage'), languagehtm = [];
    for (var i = 0; i < modesIds.length; i++) {
        var mo = modesIds[i];
        languagehtm.push('<option>' + mo + '</option>');
    }

    selang.children()[0].innerHTML = languagehtm.join('');

    selang.val(selang.attr('data-value'));

    editor = monaco.editor.create(te[0], {
        value: $('#hidContent').val(),
        language: selang.val(),
        automaticLayout: true,
        scrollWidth: 5,
        scrollbar: {
            verticalScrollbarSize: 6,
            horizontalScrollbarSize: 6
        },
        theme: $('#setheme').attr('data-value'),
        minimap: {
            enabled: false
        }
    });

    selang.change(function () {
        var oldModel = editor.getModel();
        var newModel = monaco.editor.createModel(editor.getValue(), this.value);
        editor.setModel(newModel);
        if (oldModel) {
            oldModel.dispose();
        }
    });

    $('#setheme').change(function () {
        monaco.editor.setTheme(this.value);
    });
});

//保存
function SaveGist(type, that) {
    var gc = editor.getValue(), arrv = gc.split('\n'), row = arrv.length, msg = [];
    var post = {
        GistCode: $('#hidCode').val(),
        GistRemark: $('#txtRemark').val(),
        GistFilename: $('#txtFilename').val(),
        GistLanguage: $('#selanguage').val(),
        GistTheme: $('#setheme').val(),
        GistContent: editor.getValue(),
        GistContentPreview: arrv.slice(0, 10).join('\n'),
        GistRow: row,
        GistOpen: 1
    };

    if (post.GistRemark.trim() == "") {
        msg.push('Gist description');
    }
    if (post.GistFilename.trim() == "") {
        msg.push('Filename including extension');
    }
    if (post.GistContent.trim() == "") {
        msg.push('Gist content');
    }

    if (post.GistContent.length > 10000 * 50) {
        msg.push('Gist content is too long ( less than 500000 )');
    }

    if (msg.length) {
        alert(msg.join('\r\n'));
        return false;
    }

    that.disabled = true;
    $.ajax({
        url: '/gist/home/SaveGist',
        data: post,
        type: "post",
        dataType: 'json',
        success: function (data) {
            if (data.code == 200) {
                location.href = "/gist/code/" + data.data;
            } else if (data.code == 403) {
                alert("It's not belongs to you");
            } else {
                alert('fail');
            }
        },
        error: function (ex) {
            if (ex.status == 401) {
                alert("Please login first");
            } else {
                alert('error');
            }
        },
        complete: function () {
            that.disabled = false;
        }
    })
}

$('#sfs').click(function () {
    var ed = $('#editor');
    if (ed.hasClass('ged-efull')) {
        ed.addClass('card-body').removeClass('ged-efull');
    } else {
        ed.removeClass('card-body').addClass('ged-efull');
    }
});
