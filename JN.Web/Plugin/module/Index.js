
var colorList = ['#c23531', '#2f4554', '#61a0a8', '#d48265', '#91c7ae', '#749f83', '#ca8622', '#bda29a', '#6e7074', '#546570', '#c4ccd3'];
var labelFont = 'bold 12px Sans-serif';

function calculateMA(dayCount, data) {
    var result = [];
    for (var i = 0, len = data.length; i < len; i++) {
        if (i < dayCount) {
            result.push('-');
            continue;
        }
        var sum = 0;
        for (var j = 0; j < dayCount; j++) {
            sum += data[i - j][1];
        }
        result.push((sum / dayCount).toFixed(2));
    }
    return result;
}
//初始化图表
function initChart(json, name) {

    var format = "HH:mm";
    if (!name) {
        name = "1分钟线";
    }
    if (name == "日线") {
        format = "MM-dd";
    }

    var dates = new Array();
    var data = new Array();
    var volumns = new Array();


    for (var i = 0; i < json.length; i++) {

        dates.push(json[i].XTime);
        volumns.push(json[i].Volumns);

        var a = new Array();//open,close,lowest,highest,volume
        a.push(json[i].Open);
        a.push(json[i].Close);
        a.push(json[i].Lowest);
        a.push(json[i].Hight);
        a.push(json[i].Volumns);

        data.push(a);


    }



    var dataMA5 = calculateMA(5, data);
    var dataMA10 = calculateMA(10, data);
    var dataMA20 = calculateMA(20, data);


    option = {
        animation: false,
        color: colorList,
        //title: {
        //    left: 'center',
        //    text: 'K线图'
        //},
        //legend: {
        //    top: 30
        //   // data: ['日K', 'MA5', 'MA10', 'MA20', 'MA30']
        //},
        tooltip: {
            trigger: 'axis',
            axisPointer: {
                animation: false,
                type: 'cross',
                lineStyle: {
                    color: '#376df4',
                    width: 2,
                    opacity: 1
                }
            }
        },
        //tooltip: {
        //    triggerOn: 'none',
        //    transitionDuration: 0,
        //    confine: true,
        //    bordeRadius: 4,
        //    borderWidth: 1,
        //    borderColor: '#333',
        //    backgroundColor: 'rgba(255,255,255,0.9)',
        //    textStyle: {
        //        fontSize: 12,
        //        color: '#333'
        //    },
        //    position: function (pos, params, el, elRect, size) {
        //        var obj = {
        //            top: 60
        //        };
        //        obj[['left', 'right'][+(pos[0] < size.viewSize[0] / 2)]] = 5;
        //        return obj;
        //    }
        //},
        axisPointer: {
            link: [{
                xAxisIndex: [0, 1]
            }]

        },
        dataZoom: [
            {
                type: 'slider',
                xAxisIndex: [0, 1],
                realtime: false,
                start: 0,
                end: 100,
                top: 310,
                height: 20,
                handleIcon: 'M10.7,11.9H9.3c-4.9,0.3-8.8,4.4-8.8,9.4c0,5,3.9,9.1,8.8,9.4h1.3c4.9-0.3,8.8-4.4,8.8-9.4C19.5,16.3,15.6,12.2,10.7,11.9z M13.3,24.4H6.7V23h6.6V24.4z M13.3,19.6H6.7v-1.4h6.6V19.6z',
                handleSize: '120%'
            }, {
                type: 'inside',
                xAxisIndex: [0, 1],
                start: 40,
                end: 30,
                top: 350,
                height: 20
            }],
        xAxis: [{
            type: 'category',
            data: dates,
            boundaryGap: false,
            axisLine: { lineStyle: { color: '#777' } },
            axisLabel: {
                formatter: function (value) {
                    return echarts.format.formatTime(format, value);
                }
            },
            min: 'dataMin',
            max: 'dataMax',
            axisPointer: {
                show: true
            }
        }, {
            type: 'category',
            gridIndex: 1,
            data: dates,
            scale: true,
            boundaryGap: false,
            splitLine: { show: false },
            axisLabel: { show: false },
            axisTick: { show: false },
            axisLine: { lineStyle: { color: '#777' } },
            splitNumber: 20,
            min: 'dataMin',
            max: 'dataMax',
            axisPointer: {
                type: 'shadow',
                label: { show: false },
                triggerTooltip: true,
                handle: {
                    show: false,
                    margin: 30,
                    color: '#B80C00'
                }
            }
        }],
        yAxis: [{
            scale: true,
            splitNumber: 2,
            axisLine: { lineStyle: { color: '#777' } },
            splitLine: { show: true },
            axisTick: { show: false },
            axisLabel: {
                inside: true,
                formatter: '{value}\n'
            }
        }, {
            scale: true,
            gridIndex: 1,
            splitNumber: 2,
            axisLabel: { show: false },
            axisLine: { show: false },
            axisTick: { show: false },
            splitLine: { show: false }
        }],
        grid: [{
            left: 20,
            right: 20,
            top: 110,
            height: 120
        }, {
            left: 20,
            right: 20,
            height: 40,
            top: 260
        }],
        graphic: [{
            type: 'group',
            left: 'center',
            top: 70,
            width: 300,
            bounding: 'raw',
            children: [{
                id: 'MA5',
                type: 'text',
                style: { fill: colorList[1], font: labelFont },
                left: 0
            }, {
                id: 'MA10',
                type: 'text',
                style: { fill: colorList[2], font: labelFont },
                left: 'center'
            }, {
                id: 'MA20',
                type: 'text',
                style: { fill: colorList[3], font: labelFont },
                right: 0
            }]
        }],
        series: [{
            name: '成交量',
            type: 'bar',
            xAxisIndex: 1,
            yAxisIndex: 1,
            itemStyle: {
                normal: {
                    color: '#7fbe9e'
                },
                emphasis: {
                    color: '#140'
                }
            },
            data: volumns
        }
        , {
            type: 'candlestick',
            name: name,
            data: data,
            itemStyle: {
                normal: {
                    color: '#ef232a',
                    color0: '#14b143',
                    borderColor: '#ef232a',
                    borderColor0: '#14b143'
                },
                emphasis: {
                    color: 'black',
                    color0: '#444',
                    borderColor: 'black',
                    borderColor0: '#444'
                }
            }
        }
            //, {
        //    name: 'MA5',
        //    type: 'line',
        //    data: dataMA5,
        //    smooth: true,
        //    showSymbol: false,
        //    lineStyle: {
        //        normal: {
        //            width: 1
        //        }
        //    }
        //}, {
        //    name: 'MA10',
        //    type: 'line',
        //    data: dataMA10,
        //    smooth: true,
        //    showSymbol: false,
        //    lineStyle: {
        //        normal: {
        //            width: 1
        //        }
        //    }
        //}, {
        //    name: 'MA20',
        //    type: 'line',
        //    data: dataMA20,
        //    smooth: true,
        //    showSymbol: false,
        //    lineStyle: {
        //        normal: {
        //            width: 1
        //        }
        //    }
        //}
        ]
    };


    var myChart = echarts.init(document.getElementById('chart'));
    //option.xAxis.axisPointer.handle.show = false;
    myChart.setOption(option);
}




