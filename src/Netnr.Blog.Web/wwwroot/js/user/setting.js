//修改个人信息
function ModalUpdateUserInfo() {
    $('#ModalUserInfo').modal();
}
//保存个人信息
$('#btnSaveUserInfo').click(function () {
    var sarr = $('#FormUserInfo').serializeArray();
    if (sarr[0].value == "") {
        jz.msg("账号不能为空");
    } else if (sarr[1].value == "") {
        jz.msg("昵称不能为空");
    } else {
        $.ajax({
            url: "/user/SaveUserInfo",
            data: $('#FormUserInfo').serialize(),
            dataType: 'json',
            success: function (data) {
                if (data.code == 200) {
                    $('#ModalUserInfo').modal('hide');
                    jz.msg("操作成功");
                    setTimeout(function () {
                        location.reload(false);
                    }, 1000)
                } else {
                    jz.alert(data.msg);
                }
            },
            error: function () {
                jz.alert("网络错误");
            }
        });
    }
});

//修改密码
$('#btnUpdatePwd').click(function () {
    var err = [];
    if ($('#txtOldPwd').val() == "") {
        err.push("当前密码不能为空")
    } else if ($('#txtNewPwd1').val().length < 5) {
        err.push("新密码至少5位")
    } else if ($('#txtNewPwd1').val() != $('#txtNewPwd2').val()) {
        err.push("两次输入的密码不一致")
    }
    if (err.length) {
        jz.alert(err.join("<br/>"));
    } else {
        $.ajax({
            url: "/User/UpdatePassword",
            data: {
                oldpwd: $('#txtOldPwd').val(),
                newpwd: $('#txtNewPwd1').val()
            },
            dataType: 'json',
            success: function (data) {
                if (data.code == 200) {
                    $('#txtOldPwd').val('');
                    $('#txtNewPwd1').val('');
                    $('#txtNewPwd2').val('');
                    jz.alert("操作成功")
                } else if (data.code == 401) {
                    jz.alert("当前密码错误")
                } else {
                    jz.alert("修改失败")
                }
            }
        })
    }
});

//提示
$('.fa-question-circle-o').tooltip()