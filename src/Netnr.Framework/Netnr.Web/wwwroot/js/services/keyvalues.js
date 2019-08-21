var cmd = {
    grab: function (arr, len, index) {
        var ck = arr[index];
        $.ajax({
            url: "/services/keyvalues/grab",
            data: {
                Key: ck
            },
            type: 'post',
            dataType: 'json',
            success: function (data) {
                console.log(data);
                var wm = ck + "：" + data[0] + " （ " + (data[1].KeyValue || data[1]).substring(0, 30) + " ）";
                $('#txtrt1').val($('#txtrt1').val() + wm + "\n");
                index++;
                if (index < len) {
                    cmd.grab(arr, len, index);
                }
            },
            error: function (ex) {
                console.log(ex);
            }
        })
    },
    synonym: function (arr) {
        $.ajax({
            url: "/services/keyvalues/synonym",
            data: {
                keys: arr.join(',')
            },
            type: 'post',
            dataType: 'json',
            success: function (data) {
                console.log(data);
                var wm = data[0] + " （ " + data[1] + " ）";
                $('#txtrt2').val(wm);
            },
            error: function (ex) {
                console.log(ex);
            }
        })
    },
    addtag: function (arr) {
        $.ajax({
            url: "/services/keyvalues/addtag",
            data: {
                tags: arr.join(',')
            },
            type: 'post',
            dataType: 'json',
            success: function (data) {
                console.log(data);
                var wm = data[0] + " （ " + data[1] + " ）";
                $('#txtrt3').val(wm);
            },
            error: function (ex) {
                console.log(ex);
            }
        })
    }
};

//Grab
$('#btnRun1').click(function () {
    var tgs = $.trim($('#txt1').val());
    if (tgs != "") {
        var listTag = tgs.split('\n'), len = listTag.length;
        $('#txtrt1').val('');
        cmd.grab(listTag, len, 0);
    }
});

//Synonym
$('#btnRun2').click(function () {
    var tgs = $.trim($('#txt2').val());
    if (tgs != "") {
        var listTag = tgs.split('\n')
        cmd.synonym(listTag);
    }
});

//Tags
$('#btnRun3').click(function () {
    var tgs = $.trim($('#txt3').val());
    if (tgs != "") {
        var listTag = tgs.split('\n')
        cmd.addtag(listTag);
    }
});