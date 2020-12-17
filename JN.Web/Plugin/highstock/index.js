
//简单K线图{传进去数据：访问地址，转进币种类型，转进分钟类型    转回来数据：1时间，2开盘价格，3最高价，4最低价，5关盘价，6当天交易量}
function getKline(url, c, m, idname) {
    $.post(url, { curid: c, m: m, count: 300 }, function (data) {
    //$.getJSON(url, { code: c, m: m ,count:300}, function (data) {//'https://data.jianshukeji.com/jsonp?filename=json/aapl-ohlcv.json&callback=?'
        var ohlc = [],
            volume = [],
            dataLength = data.Data.length,
            // set the allowed units for data grouping
            groupingUnits = [[
                'week',                         // unit name
                [1]                             // allowed multiples
            ], [
                'month',
                [1, 2, 3, 4, 6]
            ]],
            i = 0;
        for (i; i < dataLength; i += 1) {
            ohlc.push([
                data.Data[i].XTime, // the date
                data.Data[i].Open, // open
                data.Data[i].Hight, // high
                data.Data[i].Lowest, // low
                data.Data[i].Close // close
            ]);
            volume.push([
                data.Data[i].XTime, // the date
                data.Data[i].Volumns // the volume
            ]);
        }

        // create the chart
        $(idname).highcharts('StockChart', {
            xAxis: {

                // 如果X轴刻度是日期或时间，该配置是格式化日期及时间显示格式
                dateTimeLabelFormats: {
                    second: '%Y-%m-%d %H:%M:%S',
                    minute: ' %H:%M',
                    hour: '%Y-%m-%d %H:%M',
                    day: '%Y-%m',
                    week: '%Y-%m',
                    month: '%Y-%m',
                    year: '%Y'
                }
                // endOntick: true,
            },
            yAxis: [{
               
                title: {
                    text: '价格'
                },
                height: '60%',
                lineWidth: 1,
                opposite:false,
                minorGridLineColor: '#F0F0F0',
                minorGridLineDashStyle: 'longdash',
                minorTickInterval: 'auto'
            }, {
               
                title: {
                    text: '成交量'
                },
                top: '65%',
                height: '35%',
                offset: 0,
                opposite: false,
                lineWidth: 1
            }],
            rangeSelector: {
                buttonTheme: {
                display: 'none' // 不显示按钮
            },
                selected: 1,
                inputEnabled: false // 不显示日期输入框
                //buttons: [
                //      {
                //          type: 'minute',
                //          count: 1,
                //          text: '300s'
                //      },
                //      {
                //          type: 'hour',
                //          count: 1,
                //          text: '1h'
                //      },                                        
                   
                //   {
                //       type: 'hour',
                //       count: 5,
                //       text: '5h'
                //   }, {
                //       type : 'day',
                //       count : 1,
                //       text : '1d'
                //   }, 
                // {
                //     type: 'week',
                //     count : 1,
                //     text : '1w'
                // },
                //{
                //    type : 'all',
                //    count : 1,
                //    text : '所有'
                //}
                //],
                //selected : 0,
                //inputEnabled : false
            },
            tooltip: {
                shared: true,
                useHTML: true
            },
            series: [{
                type: 'candlestick',
                name: '价格',
                color: 'green',
                lineColor: 'green',
                upColor: 'red',
                upLineColor: 'red',
                tooltip: {
                    //animation: true,
                    shared: true,
                    useHTML: true,
                    headerFormat: '<small>{point.key}</small><br/><table>',
                    pointFormat: '<tr><td style="color: {series.color}" colspan="2">{series.name } </td></tr>' +
                    '<tr><td>开盘:</td><td style="text-align: right">{point.open}</td></tr>' +
                    '<tr><td>最高:</td><td style="text-align: right">{point.high}</td></tr>' +
                    '<tr><td>最低:</td><td style="text-align: right">{point.low}</td></tr>' +
                    '<tr><td>收盘:</td><td style="text-align: right">{point.close}</td></tr>',
                    footerFormat: '</table>',
                    valueDecimals: 2,
                    xDateFormat: '%Y-%m-%d %H:%M'
                },
                data: ohlc,
                dataGrouping: {
                    units: groupingUnits
                }
            }, {
                type: 'column',
                name: '成交量',
                data: volume,
                yAxis: 1,
                dataGrouping: {
                    units: groupingUnits
                }
            }],
            navigation: {
                buttonOptions: {
                    enabled: false
                }
            }
                });
    });
}