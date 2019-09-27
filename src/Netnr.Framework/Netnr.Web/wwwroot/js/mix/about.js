$.ajax({
    url: "/mix/AboutServerStatus",
    type: 'post',
    data: {
        __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
    },
    dataType: 'json',
    success: function (data) {
        if (data.error != 0) {
            return false;
        }

        var htm = [];
        htm.push('<table class="table table-bordered">');

        htm.push('<tr>');
        htm.push('<td>状态</td>');
        htm.push('<td>' + data.ve_status + '</td>');
        htm.push('</tr>');

        htm.push('<tr>');
        htm.push('<td>系统</td>');
        htm.push('<td>' + data.os + '</td>');
        htm.push('</tr>');

        htm.push('<tr>');
        htm.push('<td>虚拟类型</td>');
        htm.push('<td>' + data.vm_type + '</td>');
        htm.push('</tr>');

        htm.push('<tr>');
        htm.push('<td>物理位置</td>');
        htm.push('<td>' + data.node_datacenter + '</td>');
        htm.push('</tr>');

        htm.push('<tr>');
        htm.push('<td>平均负载</td>');
        htm.push('<td>' + data.load_average + '</td>');
        htm.push('</tr>');

        htm.push('<tr>');
        htm.push('<td>运行内存</td>');
        var p1 = data.plan_ram / 1024 / 1024, p2 = (data.mem_available_kb / 1024).toFixed(0);
        p2 = p1 - p2;
        var p3 = (p2 / p1 * 100).toFixed(0);
        var pp = '<div class="progress" style="margin:0"><div class="progress-bar bg-warning" aria-valuenow="' + p3 + '" aria-valuemin="0" aria-valuemax="100" style="width: ' + p3 + '%;">' + p3 + '%</div></div>';
        htm.push('<td>' + pp + p2 + ' / ' + p1 + ' MB</td>');
        htm.push('</tr>');

        htm.push('<tr>');
        htm.push('<td>交换区</td>');
        p1 = (data.swap_total_kb / 1024).toFixed(0);
        p2 = (data.swap_available_kb / 1024).toFixed(0);
        p2 = p1 - p2;
        p3 = (p2 / p1 * 100).toFixed(0);
        pp = '<div class="progress" style="margin:0"><div class="progress-bar bg-warning" aria-valuenow="' + p3 + '" aria-valuemin="0" aria-valuemax="100" style="width: ' + p3 + '%;">' + p3 + '%</div></div>';
        htm.push('<td>' + pp + p2 + ' / ' + p1 + ' MB</td>');
        htm.push('</tr>');

        htm.push('<tr>');
        htm.push('<td>磁盘空间</td>');
        p1 = data.plan_disk / 1024 / 1024 / 1024;
        p2 = (data.ve_used_disk_space_b / 1024 / 1024 / 1024).toFixed(2);
        p3 = (p2 / p1 * 100).toFixed(0);
        pp = '<div class="progress" style="margin:0"><div class="progress-bar bg-warning" aria-valuenow="' + p3 + '" aria-valuemin="0" aria-valuemax="100" style="width: ' + p3 + '%;">' + p3 + '%</div></div>';
        htm.push('<td>' + pp + p2 + ' / ' + p1 + ' GB</td>');
        htm.push('</tr>');

        htm.push('<tr>');
        htm.push('<td>流量<br/><small class="text-muted">每月10号重置</small></td>');
        p1 = data.plan_monthly_data / 1024 / 1024 / 1024;
        p2 = (data.data_counter / 1024 / 1024 / 1024).toFixed(2);
        p3 = (p2 / p1 * 100).toFixed(2);
        pp = '<div class="progress" style="margin:0"><div class="progress-bar bg-warning" aria-valuenow="' + p3 + '" aria-valuemin="0" aria-valuemax="100" style="width: ' + p3 + '%;">' + p3 + '%</div></div>';
        htm.push('<td>' + pp + p2 + ' / ' + p1 + ' GB</td>');
        htm.push('</tr>');

        htm.push('</table>');
        $('#divAs').html(htm.join(''));
    },
    error: function () {
        $('#divAs').html('<h4 class="text-center text-danger">获取服务器信息异常</h4>');
    }
});