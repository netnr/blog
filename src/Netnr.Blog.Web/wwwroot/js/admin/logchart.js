function loadPVUV(type, group) {
    $.getJSON('/Admin/QueryLogStatsPVUV?type=' + type + '&LogGroup=' + group, null, function (res) {
        var categories = [], pv = [], uv = [], spv = 0, suv = 0;
        $.each((res.Data || []), function () {
            spv += this.pv;
            suv += this.ip;
            categories.push(this.time);
            pv.push(this.pv);
            uv.push(this.ip);
        });
        series = [{ name: "PV", data: pv }, { name: "UV", data: uv }];
        Highcharts.chart('chart1', {
            chart: {
                type: 'line'
            },
            title: {
                text: 'PV（' + spv + '） / UV（' + suv + '）'
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

function loadTop(type, field, group) {
    $.getJSON('/Admin/QueryLogReportTop?type=' + type + '&field=' + field + '&LogGroup=' + group, null, function (res) {
        var data = res.Data || [], arr = [], total = 0;
        $.each(data, function () {
            total += this.total
        })
        $.each(data, function () {
            this.y = (this.total / total * 100).toFixed(2) * 1;
            this.name = this.field;
            this.p = this.y + "%";
        })

        switch (field) {
            case "LogAction":
                {
                    arr.push('<ul>');

                    $.each(data, function () {
                        arr.push('<li>');
                        var url = location.origin + "/" + this.field;
                        arr.push('<a target="_blank" href="' + url + '">' + url + '</a>');
                        arr.push(' &nbsp; ' + this.total);
                        arr.push(' &nbsp; ' + this.p);
                        arr.push('</li>');
                    })
                    arr.push('</ul>');
                }
                break;
            case "LogReferer":
                {
                    arr.push('<ul>');

                    $.each(data, function () {
                        arr.push('<li>');
                        if (this.field == "") {
                            arr.push('unknown');
                        } else {
                            arr.push('<a target="_blank" href="' + this.field + '">' + this.field + '</a>');
                        }
                        arr.push(' &nbsp; ' + this.total);
                        arr.push(' &nbsp; ' + this.p);
                        arr.push('</li>');
                    });
                    arr.push('</ul>');
                }
                break;
            case "LogSystemName":
            case "LogBrowserName":
                {
                    Highcharts.chart('field' + field, {
                        chart: {
                            plotBackgroundColor: null,
                            plotBorderWidth: null,
                            plotShadow: false,
                            type: 'pie'
                        },
                        title: {
                            text: '【' + field + '】Total：' + total
                        },
                        tooltip: {
                            pointFormat: '{series.name}: <b>{point.percentage:.1f}%</b>'
                        },
                        plotOptions: {
                            pie: {
                                allowPointSelect: true,
                                cursor: 'pointer',
                                dataLabels: {
                                    enabled: true,
                                    format: '<b>{point.name}</b>: {point.percentage:.1f} %',
                                    style: {
                                        color: (Highcharts.theme && Highcharts.theme.contrastTextColor) || 'black'
                                    }
                                }
                            }
                        },
                        series: [{
                            name: 'Brands',
                            colorByPoint: true,
                            data: data
                        }],
                        credits: {
                            enabled: false
                        }
                    });
                }
        }

        if (arr.length) {
            var msg = "<h4 class='text-center'>【" + field + "】Total：" + total + "</h4>" + arr.join('');
            $('#field' + field).html(msg);
        }

    });
}

function init() {
    var st = $('#setime').val(), sg = $('#segroup').val();

    loadPVUV(st, sg);
    loadTop(st, 'LogAction', sg);
    loadTop(st, 'LogReferer', sg);
    loadTop(st, 'LogSystemName', sg);
    loadTop(st, 'LogBrowserName', sg);
}
init();

$('#setime').change(function () {
    init();
})
$('#segroup').change(function () {
    init();
})