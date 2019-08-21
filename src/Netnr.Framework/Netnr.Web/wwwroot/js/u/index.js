var isme = $('#hid_isme').val() == 1;
var UpdateUserPhoto = {}
//获取头像
$('#btnGetGp').click(function () {
    if ($('#txtUserMail').val().trim() == "") {
        jz.alert("请输入邮箱");
        return;
    }
    var gp = "https://www.gravatar.com/avatar/" + md5($('#txtUserMail').val());
    var img = new Image();
    img.onload = function () {
        $('#imgPreviewPhoto').attr('src', this.src);
        UpdateUserPhoto.type = "link";
        UpdateUserPhoto.source = this.src;
        $('#btnGetGp').attr('disabled', false).html('获取头像')
    }
    img.onerror = function () {
        jz.msg("获取头像失败");
        $('#btnGetGp').attr('disabled', false).html('获取头像')
    }
    $('#btnGetGp').attr('disabled', true).html('正在获取')
    img.src = gp + "?s=400";
})
$('#userp').click(function () {
    isme && $('#ModalUserPhoto').modal();
})
$('#fileUploadPhoto').change(function () {
    var file = this.files[0];
    if (file) {
        var err = [];
        if (file.type.indexOf('image') != 0) {
            err.push("请选择图片")
        }
        if (file.size > 1024 * 1024 * 2) {
            err.push("大小限制2M")
        }
        if (err.length) {
            jz.alert(err.join('<br/>'));
        } else {
            var reader = new FileReader();
            reader.onload = function (ev) {
                var res = ev.target.result;
                $('#imgPreviewPhoto').attr('src', res);
                UpdateUserPhoto.type = "file";
                UpdateUserPhoto.source = res;
            }
            reader.readAsDataURL(file);
        }
    }
})
$('#btnSaveUserPhoto').click(function () {
    $('#btnSaveUserPhoto').html('处理中');
    $('#btnSaveUserPhoto')[0].disabled = true;
    $.ajax({
        url: '/User/UpdateUserPhoto',
        type: "post",
        data: UpdateUserPhoto,
        dataType: 'json',
        success: function (data) {
            if (data.code == 200) {
                location.reload(false);
            } else {
                jz.alert(data.msg);
            }
        },
        error: function (ex) {
            if (ex.status == 401) {
                jz.alert("请登录");
            } else {
                jz.alert("网络错误");
            }
        },
        complete: function () {
            $('#btnSaveUserPhoto').html('保存');
            $('#btnSaveUserPhoto')[0].disabled = false;
        }
    });
});

//编辑个性签名
$('#btnsayedit').click(function () {
    var sbc = $('#saybody').children();
    sbc.eq(0).addClass('d-none');
    sbc.eq(1).removeClass('d-none');
});
$('#btnsaycancel').click(function () {
    var sbc = $('#saybody').children();
    sbc.eq(1).addClass('d-none');
    sbc.eq(0).removeClass('d-none');
});
$('#btnsaysave').click(function () {
    var sbc = $('#saybody').children(), us = sbc.eq(1).find('textarea').val();
    $.ajax({
        url: '/User/UpdateUserSay',
        data: {
            UserSay: us
        },
        dataType: 'json',
        success: function (data) {
            if (data.code == 200) {
                sbc.eq(1).addClass('d-none');
                sbc.eq(0).removeClass('d-none').text(us);
            } else {
                jz.alert('保存失败');
            }
        },
        error: function (ex) {
            if (ex.status == 401) {
                jz.alert("请登录");
            } else {
                jz.alert("网络错误");
            }
        }
    });
});