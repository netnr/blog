function loadOSinfo() {
    $.ajax({
        url: "/Mix/AboutServerStatus",
        type: 'post',
        data: {
            __nolog: "true",
            __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
        },
        dataType: 'json',
        success: function (data, _status, xhr) {
            if (data.code == 200) {
                var ssinfo = ' Duration: ' + document.getElementById("hid_rt").value + ' Days\n\n';
                xhr.getAllResponseHeaders().replace(/server: (.*)/, function () {
                    ssinfo += ' Server: ' + arguments[1] + "\n\n";
                })
                ssinfo += data.data.trim();
                $('.nr-ss').html(ssinfo).css('white-space', 'pre-line');
            } else {
                $('.nr-ss').html('<h4 class="text-center text-danger">获取服务器信息异常</h4>');
            }

            //自动刷新
            setTimeout(loadOSinfo, 1000 * 10);
        },
        error: function () {
            $('.nr-ss').html('<h4 class="text-center text-danger">获取服务器信息异常</h4>');

            //自动刷新
            setTimeout(loadOSinfo, 1000 * 10);
        }
    });
}

loadOSinfo();