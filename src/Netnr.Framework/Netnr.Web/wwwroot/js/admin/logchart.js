function loadFlow(type) {
    fetch('/Admin/QueryLogReportFlow?type=' + type)
        .then(x => x.json())
        .then(function (data) {
            var categories = [], pv = [], ip = [], id = [];
            data.forEach(x => categories.push(x.time) && pv.push(x.pv) && ip.push(x.ip));
            series = [{ name: "PV", data: pv }, { name: "IP", data: ip }];
            Highcharts.chart('chart1', {
                chart: {
                    type: 'line'
                },
                title: {
                    text: 'Flow'
                },
                xAxis: {
                    categories: categories
                },
                yAxis: {
                    title: {
                        text: 'Total'
                    }
                },
                plotOptions: {
                    line: {
                        dataLabels: {
                            // 开启数据标签
                            enabled: true
                        },
                        // 关闭鼠标跟踪，对应的提示框、点击事件会失效
                        enableMouseTracking: false
                    }
                },
                series: series,
                credits: {
                    enabled: false
                }
            });
        })
}

function loadTop(type, field) {
    fetch('/Admin/QueryLogReportTop?type=' + type + '&field=' + field)
        .then(x => x.json())
        .then(function (data) {
            var arr = [];
            var totalpv = 0;
            data.forEach(x => totalpv += x.pv);
            data.forEach(x => x.p = (x.pv / totalpv * 100).toFixed(2) + "%");

            switch (field) {
                case "url":
                    {
                        arr.push('<ul>');

                        data.forEach(function (x) {
                            arr.push('<li>');
                            var url = location.origin + x.url;
                            arr.push('<a target="_blank" href="' + url + '">' + url + '</a>');
                            arr.push(' &nbsp; ' + x.pv);
                            arr.push(' &nbsp; ' + x.p);
                            arr.push('</li>');
                        });
                        arr.push('</ul>');
                    }
                    break;
                case "referer":
                    {
                        arr.push('<ul>');
                        data.forEach(function (x) {
                            arr.push('<li>');
                            if (x.url == "") {
                                arr.push('unknown');
                            } else {
                                arr.push('<a target="_blank" href="' + x.url + '">' + x.url + '</a>');
                            }
                            arr.push(' &nbsp; ' + x.pv);
                            arr.push(' &nbsp; ' + x.p);
                            arr.push('</li>');
                        });
                        arr.push('</ul>');
                    }
                    break;
            }

            var msg = "<h4>Top 20【" + field + "】</h4>" + arr.join('');
            $('#field' + field).html(msg);
        });
}

loadFlow(6);
loadTop(6, 'url');
loadTop(6, 'referer');
$('#setime').change(function () {
    loadFlow(this.value);
    loadTop(this.value, 'url');
    loadTop(this.value, 'referer');
})