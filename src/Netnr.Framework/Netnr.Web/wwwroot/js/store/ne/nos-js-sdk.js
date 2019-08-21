function Uploader(options){
	'use strict';
	var defaults = {
        /**
         * 请求协议类型，默认http
         * @type {String}
         */
        protocol: location.protocol.substring(0, location.protocol.length - 1) || 'https',
		/**
		 * 获取dns列表的URL
		 * @type {String}
		 */
		urlDns: 'http://lbs-eastchina1.126.net/lbs',
		/**
		 * 重试次数，默认重试2次
		 * @type {Number}
		 */
		retryCount: 2,
		/**
		 * 添加文件Input ID
		 * @type {String}
		 */
		fileInputID: 'fileInput',
		/**
		 * ajax超时时间
		 * @type {Number}
		 */
		timeout: 50000,
		/**
		 * 错误处理回调
		 * @param  {object} errObj带errCode和errMsg或只带errMsg的Object对象或XHR错误对象
		 * @return {void}
		 */
		onError: function(errObj){
			console.log(errObj);
		},
		/**
		 * 上传进度回调
		 * @param  {Object} curFile 文件对象
		 * @return {void}
		 */
		onProgress: function(curFile){
			console.log(curFile.status);
			console.log(curFile.progress);
		},
	},
	opts,
	service;

	opts = $.extend({}, defaults, options);

	service = {
		/**
		 * 接口版本号，当前支持1.0
		 * @type {String}
		 */
		version: '1.0',
		/**
		 * 上传的文件对象
		 * @type {fileObj}
		 */
		uploadFile: null,
		/**
		 * 边缘节点列表
		 * @type {Array}
		 */
		edgeNodeList: null,
		/**
		 * 当前上传文件使用的边缘节点在edgeNodeList中的序号
		 * @type {Number}
		 */
		dnsRetryCount: 0,
		/**
		 * 当前上传分片使用的边缘节点剩余的重试次数
		 * @type {Number}
		 */
		uTRetryCount: opts.retryCount,
		/**
		 * 获取断点失败剩余重试次数
		 * @type {Number}
		 */
		gORetryCount: opts.retryCount,
		/**
		 * 查询DNS失败剩余重试次数
		 * @type {Number}
		 */
		gDRetryCount: opts.retryCount,
		/**
		 * 创建ajax对象
		 * @return {void}
		 */
		createAjax: function() {
	        var xmlhttp = {};
	        if (window.XMLHttpRequest) {
	            xmlhttp = new XMLHttpRequest();
	        } else {
	            xmlhttp = new ActiveXObject("Microsoft.XMLHTTP");
	        }
	        return xmlhttp;
	    },
	    /**
         * 清除本地存储
         * @param {Object} fileKey 文件标识符
         */
        clearStorage: function(bucketName, objectName, fileKey) {
            localStorage.removeItem(fileKey + '_progress');
            localStorage.removeItem(fileKey + '_' + bucketName + '_' + objectName + '_context');
        },
        /**
		 * 添加文件
		 * @method addFile
		 * @param {Element}   file 需要上传的文件
		 * @param {Function} callback  成功回调函数
		 *  	回调函数中的参数包括：
		 *      fileObj: 文件对象
		 * 			文件对象中包含的属性:
		 * 				fileKey {String} 文件名与文件大小的MD5值
		 * 				file {File} 文件对象
		 * 				fileName {String} 文件名
		 * 				status {Number} 文件状态（ 0 未上传  1 正在上传  2 已上传）
		 * 				progress {Number} 上传进度（保留两位小数的百分比）
		 * @return {void}
		 */
		addFile: function(file, callback) {
			if(file){
				var fileKey = CryptoJS.MD5(file.name + ':' + file.size).toString(),
	                fileObj;

	            fileObj = {
	                fileKey: fileKey,
	                file: file,
	                fileName: file.name,
	                status: 0,
	                progress: localStorage.getItem(fileKey + '_progress') || 0
	            };
				if(service.uploadFile)
					service.uploadFile = null;
				service.uploadFile = $.extend(true, {}, fileObj);
	            if(callback)
	            	callback(fileObj);
			}
       },
        /**
         * 根据fileKey获取指定文件对象
         * @method getFile
         * @param  {String} fileKey 文件名和文件大小md5值
         * @return {Obejct}         文件对象
         */
        getFile: function(fileKey) {
            var curFile;
            if (service.uploadFile.fileKey === fileKey) {
                curFile = service.uploadFile;
            }
            return curFile;
        },
		/**
		 * 查询DNS
		 * @method getDNS
		 * @param  {Object}   bucketname    桶名
		 * @param  {Function} callback 成功回调函数
		 * 		回调函数参数包括：
		 * 		upload： 返回最优的边缘节点列表，加速效果最好的在前面
		 *      lbs: 返回的最优的DNS地址，后续可以使用该地址进行上传
		 * @return {void}
		 */

		getDNS: function(bucketname, callback){
			if(bucketname === undefined || bucketname === null || bucketname === ''){
				bucketname = '';
			}

			if (service.edgeNodeList) {//已缓存则直接取缓存数据
                callback(service.edgeNodeList);
            } else {
                if(opts.protocol==='https'){
                    let centerUrl = ['https://nosup-eastchina1.126.net']
                    service.edgeNodeList = centerUrl;
                    callback(centerUrl, opts.urlDns);
                }else{
                    $.ajax({
                        type: 'get',
                        url: opts.urlDns,
                        data: {
                            version: service.version,
                            bucketname: bucketname,
                        },
                        dataType: 'json',
                        success: function(data, s, xhr) {
                            if (data.code) {
                                opts.onError({
                                    errCode: data.Code,
                                    errMsg: data.Message
                                });
                            } else {

                                service.edgeNodeList = data.upload;
                                callback(data.upload, data.lbs);
                            }
                        },
                        error: function(xhr, s, err) {
                            if(xhr.status.toString().match(/^5/) && service.gDRetryCount > 0 && service.gDRetryCount < opts.retryCount + 1){
    	                		service.getDNS(bucketname, callback);
    	                		service.gDRetryCount--;
    	                	}
    	                	else{
    	                		opts.onError(xhr.responseText);
    	                		service.gDRetryCount = opts.retryCount;
    	                	}
                        }
                    });
                }
            }
		},
		/**
         * 获取上传断点位置
         * @method getOffset
         * @param  {Object}   param  AJAX参数
         * 		param属性有：
         * 		serveIp {String} IP地址
         * 		fileKey {String} 文件名与文件大小的MD5值
         * 		bucketName {String} 桶名
         * 		objectName {String} 对象名
         * 		token {String} 上传凭证
         * @param  {Function} callback 获取成功回调
         * 		回调函数中的参数：
         * 		offset {long} 当前分片在整个对象中的起始偏移量
         * @return {void}
         */
        getOffset: function(param, callback) {
            var context;
            context = localStorage.getItem(param.fileKey + '_' + param.bucketName + '_' + param.objectName + '_context');
            if (!context) {
                return callback(0);
            }
            $.ajax({
                type: 'get',
                url: param.serveIp + '/' + param.bucketName + '/' + encodeURIComponent(param.objectName) + '?uploadContext',
                data: {
                    version: service.version,
                    context: context
                },
                dataType: 'json',
              	beforeSend: function(xhr) {
                	xhr.setRequestHeader('x-nos-token', param.token);
	            },
                success: function(data, s, xhr) {
                    if (data.errCode) {
                        opts.onError({
                            errCode: data.errCode,
                            errMsg: data.errMsg
                        });
                    } else {
                        callback(data.offset);
                    }
                },
                error: function(xhr, s, err) {
                	if(xhr.status.toString().match(/^5/) && service.gORetryCount > 0 && service.gORetryCount < opts.retryCount + 1){
                		service.getOffset(param, callback);
                		service.gORetryCount--;
                	}
                	else{
                		opts.onError(xhr.responseText);
                		service.gORetryCount = opts.retryCount;
                		if(xhr.status === 404){
	                        service.clearStorage(param.bucketName, param.objectName, param.fileKey);
	                    }
                	}
                }
            });
        },
		/**
         * 上传分片
         * @method uploadTrunk
         * @param  {Object}   param     AJAX参数
         *  	param中的属性有：
         * 		serveIp {String} IP地址
         * 		bucketName {String} 桶名
         * 		objectName {String} 对象名
         * 		token {String} 上传凭证
         * @param  {Object}   trunkData 分片数据
         * 		trunkData属性有：
         * 		file {File} File对象
         *      fileKey {String} 文件名和文件大小的MD5值
         *      offset {long} 当前分片在整个对象中的起始偏移量
         *      trunkSize {long} 分片大小
         *      trunkEnd {long} 分片结束位置
         *      context: {String} 上传上下文
         * @param  {Function} callback  文件（非分片）上传成功回调函数
         * 		回调函数参数：
         * 		trunkData {Object} 分片数据
         * @return {void}
         */
        uploadTrunk: function(param, trunkData, callback) {
            var xhr,
                xhrParam = '',
                curFile,
                context,
                blobSlice = File.prototype.mozSlice || File.prototype.webkitSlice || File.prototype.slice;

            curFile = service.getFile(trunkData.fileKey);
            context = localStorage.getItem(trunkData.fileKey + '_' + param.bucketName + '_' + param.objectName + '_context');

            if (curFile.xhr) {
                xhr = curFile.xhr;
            } else {
                xhr = service.createAjax();
                xhr.timeout = opts.timeout;
                curFile.xhr = xhr;
            };

            xhr.upload.onprogress = function(e) {//上传进度处理函数
                var progress = 0;

                if (e.lengthComputable) {
                    progress = (trunkData.offset + e.loaded) / trunkData.file.size;
                    curFile.progress = (progress * 100).toFixed(2);

                    if (progress > 0 && progress < 1) {
                        curFile.status = 1;
                        $('#' + opts.fileInputID).attr("disabled", true);
                    } else if (progress === 1) {
                        curFile.status = 2;
                        $('#' + opts.fileInputID).attr("disabled", false);
                    }
                    localStorage.setItem(trunkData.fileKey + '_progress', curFile.progress);
                    opts.onProgress(curFile);
                } else {
                    console.log('浏览器不支持进度事件',e)
                    opts.onError({
                        errMsg: '浏览器不支持进度事件'
                    });
                }
            };

            xhr.onreadystatechange = function() {
                if (xhr.readyState !== 4) {
                    return;
                }

                var result;
                try {
                    result = JSON.parse(xhr.responseText);
                } catch (e) {
	                result = {
                        errMsg: '未知错误'
                    };
               	}

                if (xhr.status === 200) {
                	if(curFile.file.size === 0){
                		curFile.status = 2;
                		curFile.progress = 100.00;
                		opts.onProgress(curFile);
                	};
                    localStorage.setItem(trunkData.fileKey + '_' + param.bucketName + '_' + param.objectName + '_context', result.context);

                    if (result.offset < trunkData.file.size) {//上传下一片
                        service.uploadTrunk(param, $.extend({}, trunkData, {
                            offset: result.offset,
                            trunkEnd: result.offset + trunkData.trunkSize,
                            context: context || result.context
                        }), callback);
                    } else {//单文件上传结束
                          callback(trunkData);
                    }
                } else if(xhr.status.toString().match(/^5/)) {
            		//服务器出错重试
            		if(service.uTRetryCount < opts.retryCount + 1 && service.uTRetryCount >0){//同一个边缘节点重试两次
                		service.getOffset({
                			serveIp: param.serveIp,
                			bucketName: param.bucketName,
                			objectName: param.objectName,
                			token: param.token,
                			fileKey: trunkData.fileKey
                		},function(offset){
                			service.uploadTrunk(param, $.extend({}, trunkData, {
                				offset: offset,
                				trunkEnd: offset + trunkData.trunkSize,
                            	context: localStorage.getItem(trunkData.fileKey + '_' + param.bucketName + '_' + param.objectName + '_context') || '',
                			}), callback);
                		});
                		service.uTRetryCount--;
                	} else if(service.dnsRetryCount < service.edgeNodeList.length-1){//重试边缘节点
                		service.uTRetryCount = opts.retryCount;
                		service.dnsRetryCount++;
                		var param1 = $.extend({}, param, {
                			serveIp: service.edgeNodeList[service.dnsRetryCount],
                		});
                		service.getOffset({
                			serveIp: param1.serveIp,
                			bucketName: param1.bucketName,
                			objectName: param1.objectName,
                			token: param1.token,
                			fileKey: trunkData.fileKey
                		},function(offset){
                			service.uploadTrunk(param1, $.extend({}, trunkData, {
                				offset: offset,
                				trunkEnd: offset + trunkData.trunkSize,
                            	context: localStorage.getItem(trunkData.fileKey + '_' + param.bucketName + '_' + param.objectName + '_context') || '',
                			}), callback);
                		});
                	} else {//重试完输出错误信息
                		service.dnsRetryCount = 0;
        				opts.onError({
	                        errCode: result.errCode,
	                        errMsg: result.errMsg
	                    });

	                   	$('#' + opts.fileInputID).attr("disabled", false);
                		service.clearStorage(param.bucketName, param.objectName, trunkData.fileKey);
                	}
                }else{
                	$('#' + opts.fileInputID).attr("disabled", false);
	              	if(xhr.status){
	                    service.clearStorage(param.bucketName, param.objectName, trunkData.fileKey);
	                    opts.onError({
	                        errCode: result.errCode,
	                        errMsg: result.errMsg
		                });
	                } else {
	               		 console.log('上传已暂停');
	                }
                }
            };

            xhrParam = '?offset=' + trunkData.offset + '&complete=' + (trunkData.trunkEnd >= trunkData.file.size) + '&context=' + (context || trunkData.context) + '&version=' + service.version;
            xhr.open('post', param.serveIp + '/' + param.bucketName + '/' + encodeURIComponent(param.objectName) + xhrParam);
            xhr.setRequestHeader('x-nos-token', param.token);
            xhr.send(blobSlice.call(trunkData.file, trunkData.offset, trunkData.trunkEnd));
        },
        /**
         * 暂停上传
         * @method pauseUpload
         * @return {void}
         */
        pauseUpload: function(){
        	var xhr;
        	if (!service.uploadFile) {
        		opts.onError({
        			errMsg: '未选择需上传的文件',
        		});
            } else if(!service.uploadFile.xhr) {
            	opts.onError({
            		errMsg: '当前文件未开始上传',
            	});
            } else if(service.uploadFile.status < 2){
            	xhr = service.uploadFile.xhr;
            	xhr.abort();
            	$('#' + opts.fileInputID).attr("disabled", false);
            } else {
            	opts.onError({
            		errMsg: '当前文件已完成上传',
            	});
            }
        },
		/**
		 * 上传文件
		 * @param {Object} param 参数
		 * 		param属性有：
		 * 			bucketName {String} 桶名
		 * 			objectName {String} 对象名
		 * 			trunkSize {long} 分片大小
		 * 			token {Object} 上传凭证
		 * @return {void}
		 */
        upload: function(param, callback) {
        	if(!param.bucketName && param.bucketName !== 0){
        		opts.onError({
					errMsg: '桶名不能为空',
				});
        		return;
        	};
        	if(!param.objectName && param.objectName !== 0){
        		opts.onError({
					errMsg: '对象名不能为空',
				});
        		return;
        	};
        	if(!param.token){
        		opts.onError({
        			errMsg: '上传凭证不能为空',
        		});
        		return;
        	}
        	if(!param.trunkSize)
        		param.trunkSize = 128 * 1024;
			if(!service.uploadFile){
				opts.onError({
					errMsg: '未选择需上传的文件',
				});
				return;
			}

			var curFile = service.uploadFile;
			service.getDNS(param.bucketName, function(edgeNodeList, lbs) {
				if(edgeNodeList.length === 0){
					opts.onError({
						errMsg: '暂无边缘节点',
					});
					return;
				}
                service.getOffset({
                    serveIp: edgeNodeList[0],
                    bucketName: param.bucketName,
                    objectName: param.objectName,
                    fileKey: curFile.fileKey,
                    token: param.token,
                }, function(offset) {
                    service.uploadTrunk({
                        serveIp: edgeNodeList[0],
                        bucketName: param.bucketName,
                        objectName: param.objectName,
                        token: param.token,
                    }, {
                        file: curFile.file,
                        fileKey: curFile.fileKey,
                        offset: offset || 0,
                        trunkSize: param.trunkSize,
                        trunkEnd: (offset || 0) + param.trunkSize,
                        context: ''
                    }, function(trunkData) {
                        service.clearStorage(param.bucketName, param.objectName, trunkData.fileKey);
                        if(callback)
                        	callback(curFile);
                    });
                });
            });
        },
	};

	return service;
}
