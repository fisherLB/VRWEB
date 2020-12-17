/// <reference path="mywebupload.js" />


/*
作者：陈詹文
插件描述：
    单图片上传工具，上传过程显示预览图，显示上传进度，超过200k的图片自动压缩后再上传。


*/
$.fn.uploaderBox = function (options) {

    var $box = $(this);
    var btnID = Math.random().toString().replace(".", "");







    var GUID = WebUploader.guid();

    var defaults = {
        pick: {
            id: '#' + btnID,
            label: '点击选择图片',
            multiple: false
        },
        auto: true,
        //dnd: '#dndArea',
        //paste: 'body',
        swf: '/Plugins/webuploader-0.1.5/Uploader.swf',
        //chunked: true, //分片处理大文件
        chunkSize: 1024 * 1024,
        server: '/upload/WebUploader',
        disableGlobalDnd: true,
        threads: 1, //上传并发数http://127.0.0.1:9368/Uploader.swf
        //由于Http的无状态特征，在往服务器发送数据过程传递一个进入当前页面是生成的GUID作为标示
        formData: { guid: GUID },
        fileNumLimit: 300,
        accept: {
            title: '图片文件',
            extensions: 'gif,jpg,jpeg,bmp,png,ico,icon',
            mimeTypes: 'image/*'
        },
        fileSizeLimit: 4 * 1024 * 1024,    // 20 M
        fileSingleSizeLimit: 4 * 1024 * 1024,   // 20 M
        boxWidth: 150,
        inputName: "uploadInput",
        defaultImg: "/images/default.png",
        defaultValue: "/images/default.png",
        //: , //图片在上传前进行压缩




        success: function (file, response) {

        }
    };

    var options = $.extend(defaults, options);
    init();






    window.setTimeout(function () {

        var uploader = new WebUploader.Uploader(options);

        $box.css("width", options.boxWidth);
        $box.css("border", "solid 1px #ddd;padding:5px");
        $box.find(".webuploader-pick").css("width", "100%");
        $box.find(".webuploader-pick").css("border-radius", "0");
        $box.find("#" + btnID).css("width", "100%");



        window.setTimeout(function () {
            $box.find(".webuploader-pick").next("div").css("z-index", 99);

            $box.find(".webuploader-pick").next("div").css("width", "300");

            $box.find(".webuploader-pick").next("div").css("height", "41");

        }, 2000);

        uploader.option("compress", {
            width: 900,
            height: 600,

            // 图片质量，只有type为`image/jpeg`的时候才有效。
            quality: 90,

            // 是否允许放大，如果想要生成小图的时候不失真，此选项应该设置为false.
            allowMagnify: false,

            // 是否允许裁剪。
            crop: false,

            // 是否保留头部meta信息。
            preserveHeaders: true,

            // 如果发现压缩后文件大小比原来还大，则使用原来图片
            // 此属性可能会影响图片自动纠正功能
            noCompressIfLarger: true,

            // 单位字节，如果图片大小小于此值，不会采用压缩。
            compressSize: 150 * 1204
        });

        uploader.option("thumb", {
            width: options.boxWidth,
            height: 110,

            // 图片质量，只有type为`image/jpeg`的时候才有效。
            quality: 70,

            // 是否允许放大，如果想要生成小图的时候不失真，此选项应该设置为false.
            allowMagnify: false,

            // 是否允许裁剪。
            crop: false
            //,

            // 为空的话则保留原有图片格式。
            // 否则强制转换成指定的类型。
            // type: 'image/jpeg'
        });


        uploader.on("fileQueued", function (file) {



            //选择图片后生成缩略图
            uploader.makeThumb(file, function (error, ret) {
                if (error) {
                    //$li.text('预览错误');
                } else {
                    $("#img" + btnID).attr("src", ret);

                }
            });

        });

        uploader.on("uploadProgress", function (file, percentage) {

            $("#" + btnID).find(".webuploader-pick").text("正在上传：(" + percentage + "%)");

        });


        uploader.on('uploadSuccess', function (file, response) {
            $("#" + btnID).find(".webuploader-pick").text("点击选择图片");
            $("#img" + btnID).attr("src", response.url);
            $("input[name='" + options.inputName + "']").val(response.url);
            options.success(file, response);
        });
    }, 500);
    function init() {
        var html = '<div style="overflow:hidden"><input type="hidden" id="' + options.inputName + '" name="' + options.inputName + '" value="' + options.defaultValue + '" /><img id="img' + btnID + '" style="width:' + options.boxWidth + 'px" src="' + options.defaultImg + '" class="img-responsive" /></div><div><div id="' + btnID + '"></div></div>';

        $box.html(html);
    }

}