var dir = document.currentScript.src.slice(0, document.currentScript.src.lastIndexOf('/') + 1);
function uploaderCreate(_options) {
    var defaults = {
        MaxNum: 1,//默认一张
        AutoHideArea: true,//超过上传限制后是否隐藏上传域
        MaxSize: 1024 * 1024 * 4,//默认4M,
        Extensions: "gif,jpg,png",//默认格式
        Method: 'post',//默认POST方式提交
        RecordReturnValue: true,//是否记录上传成功的返回值，容器为hidImgSrc
        Url: null,
        $container: null,
        Timeout: 1000 * 10,
        FileVal: 'file',
        CloseImgSrc: dir + '/images/close.png',
        Thumb: true,//是否显示缩略图
        ThumbWidth: 100,
        ThumbHeight: 100,
        ThumbHorizontalShow: true,//缩略图水平显示
        MessageArray: new Array("不能预览", "上传失败", "您最多允许上传", "张图片", "您上传的文件大小超过", "您上传的文件格式不正确，只允许上传", "格式图片"),
        Error: function (message) { alert(message); },//错误消息回调
        Success: function () { return false },//成功回调
        compress: {
            width: 1024,
            height: 1024,
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
            noCompressIfLarger: false,
            // 单位字节，如果图片大小小于此值，不会采用压缩。
            compressSize: 1024 * 1024 * 0.5 //0.5M
        }
    };
    this.options = $.extend({}, defaults, _options);//将一个空对象做为第一个参数
    //判断是否传入请求url
    if (options.Url == null) {
        console.log("Request URL to be empty");
        return;
    }
    //判断是否传入控件容器
    if (options.$container == null) {
        console.log("container to be empty");
        return;
    }
    var uploader = WebUploader.create({
        // 选完文件后，是否自动上传。
        auto: true,
        // swf文件路径
        swf: dir + '/Uploader.swf',
        // 文件接收服务端。
        server: options.Url,
        //请求方式
        method: options.Method,
        //设置文件上传域的name
        fileVal: options.FileVal,
        // 选择文件的按钮。
        pick: options.$container.find('.btnUpPic'),
        // 只允许选择图片文件。
        accept: {
            title: 'Images',
            extensions: options.Extensions,
            mimeTypes: 'image/*'
        },
        //图片上传数量限制
        fileNumLimit: options.MaxNum,
        //图片上传大小限制
        fileSingleSizeLimit: options.MaxSize,
        //上传超时
        timeout: options.Timeout,
        //去重
        duplicate: true,
        //略缩设置
        thumb: {
            width: options.ThumbWidth,
            height: options.ThumbHeight,
            // 图片质量，只有type为`image/jpeg`的时候才有效。
            quality: 100,
            // 是否允许放大，如果想要生成小图的时候不失真，此选项应该设置为false.
            allowMagnify: true,
            // 是否允许裁剪。
            crop: true,
            // 为空的话则保留原有图片格式。
            // 否则强制转换成指定的类型。
            type: 'image/jpeg'
        },
        //图片压缩设置
        compress: options.compress
    });
    // 记录添加的文件数量
    uploader.fileCount = 0;
    //添加私有变量集合
    uploader._options = options;//将一个空对象做为第一个参数

    // 负责view的销毁
    function removeFile(file) {
        uploader.fileCount--;
        var $li = $('#' + file.id);
        var url = $li.data('url');
        if (url != null && url != '' && uploader._options.RecordReturnValue) {
            uploader._options.$container.find('.hidImgSrc').val(uploader._options.$container.find('.hidImgSrc').val().replace("|" + url, ""));
        }
        uploader.removeFile(file);
        $li.remove();
        if (uploader.fileCount < uploader._options.MaxNum && uploader._options.AutoHideArea) {
            uploader._options.$container.find(".upload-area").show();
        }
    }

    // 负责view的销毁所有
    uploader.removeFileAll = function removeFileAll() {
        uploader.fileCount = 0;
        var filelist = uploader.getFiles();
        for (var i = 0; i < filelist.length; i++) {
            var $li = $('#' + filelist[i].id);
            var url = $li.data('url');
            if (url != null && url != '' && uploader._options.RecordReturnValue) {
                uploader._options.$container.find('.hidImgSrc').val(uploader._options.$container.find('.hidImgSrc').val().replace("|" + url, ""));
            }
            uploader.removeFile(filelist[i].id);
            $li.remove();
        }
        if (uploader.fileCount < uploader._options.MaxNum && uploader._options.AutoHideArea) {
            uploader._options.$container.find(".upload-area").show();
        }
    }

    // 当有文件添加进来的时候，创建缩略图
    uploader.on('fileQueued', function (file) {
        if (this._options.Thumb) {
            var stystr = '';
            if (this._options.ThumbHorizontalShow) {
                stystr+='display: inline-block;'
            }
            var $li = $(
                    '<div id="' + file.id + '"style="width:' + this._options.ThumbWidth + 'px;' + stystr + '" class="file-item thumbnail">' +
                        '<img>' +
                    '</div>'
                    ),
                $img = $li.find('img');
            //创建上传按钮
            $btns = $('<div class="file-panel">' +
                '<span><img src="' + this._options.CloseImgSrc + '" ></span>' +
                '</div>').appendTo($li),
            // queueList容器实例
            this._options.$container.find(".queueList").append($li);
            // 创建缩略图
            // 如果为非图片文件，可以不用调用此方法。
            uploader.makeThumb(file, function (error, src) {
                if (error) {
                    $img.replaceWith('<span class="notThumb" style="width:' + this._options.ThumbWidth + 'px;height:' + this._options.ThumbHeight + 'px;display: block;">' + this._options.MessageArray[0] + '</span>');
                    return;
                }
                $img.attr('src', src);
            });
            //绑定按钮事件
            $btns.on('click', 'span', function () {
                var index = $(this).index(),
                    deg;
                switch (index) {
                    case 0:
                        removeFile(file);
                        return;
                }
            });
        }
        uploader.fileCount++;
        if (uploader.fileCount >= this._options.MaxNum && this._options.AutoHideArea) {
            this._options.$container.find(".upload-area").hide();
        }
    });
    // 文件上传开始创建进度条。
    uploader.on('uploadStart', function (file) {
        var $li = $('#' + file.id),
            $percent = $li.find('.progress');
        // 避免重复创建
        if (!$percent.length) {
            $percent = $('<div class="progress" data-percentage="0" >0%</div>')
                    .appendTo($li);
        }
    });
    // 文件上传过程中创建进度条实时显示。
    uploader.on('uploadProgress', function (file, percentage) {
        var $li = $('#' + file.id),
            $percent = $li.find('.progress');
        // 避免重复创建
        if (!$percent.length) {
            $percent = $('<div class="progress" data-percentage="0" >0%</div>')
                    .appendTo($li);
        }
        function uploading() {
            var i = $percent.data('percentage');

            if ($percent.data('percentage') < percentage * 100) {
                $percent.text(++i + '%');
                $percent.data('percentage', i);
                setTimeout(uploading, 30);
            }
        }
        setTimeout(uploading, 30);
    });
    // 文件上传成功，给item添加成功class, 用样式标记上传成功。
    uploader.on('uploadSuccess', function (file, response) {
        var B = false;
        if (typeof this._options.Error === "function") {
            b = this._options.Success(response);
        } else {
            b = defaults.Success(response);
        }
        if (b != false) {
            var $li = $('#' + file.id);
            $li.addClass('upload-state-done');
            $li.find('.progress').remove();//移除进度条
            $li.attr("data-url", b);
            if (this._options.RecordReturnValue)
                this._options.$container.find('.hidImgSrc').val(this._options.$container.find('.hidImgSrc').val() + "|" + b);
        } else {
            var $li = $('#' + file.id),
            $percent = $li.find('.progress');
            // 避免重复创建
            if (!$percent.length) {
                $percent = $('<div class="progress"></div>')
                        .appendTo($li);
            }
            $percent.text(this._options.MessageArray[1]);
            setTimeout(function () { removeFile(file) }, 1000);
        }
    });
    // 文件上传失败，显示上传出错。
    uploader.on('uploadError', function (file) {
        var $li = $('#' + file.id),
            $percent = $li.find('.progress');
        // 避免重复创建
        if (!$percent.length) {
            $percent = $('<div class="progress"></div>')
                    .appendTo($li);
        }
        $percent.text(this._options.MessageArray[1]);
        setTimeout(removeFile(file), 1000);
    });
    //错误捕获
    uploader.onError = function (code) {
        var message = "";
        switch (code) {
            case 'Q_EXCEED_NUM_LIMIT':
                message = this._options.MessageArray[2] + this._options.MaxNum + this._options.MessageArray[3];
                break;
            case 'F_EXCEED_SIZE':
                message = this._options.MessageArray[4] + this._options.MaxSize / 1024 / 1024 + 'MB';
                break;
            case 'Q_TYPE_DENIED':
                message = this._options.MessageArray[5] + this._options.Extensions + this._options.MessageArray[6];
                break;
        }
        if (typeof this._options.Error === "function") {
            this._options.Error(message);
        } else {
            defaults.Error(message);
        }
    };
    return uploader
}