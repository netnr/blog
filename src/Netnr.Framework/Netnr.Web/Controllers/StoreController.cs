using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Qcloud.Shared.Api;
using Qcloud.Shared.Common;
using Netnr.Web.Filters;
using Netease.Cloud.NOS;
using Qiniu.Util;
using System.ComponentModel;

namespace Netnr.Web.Controllers
{
    /// <summary>
    /// 存储
    /// </summary>
    public class StoreController : Controller
    {
        /// <summary>
        /// 存储首页
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            return Redirect("/store/qnkodo");
        }

        #region QQ对象存储

        /// <summary>
        /// QQ对象存储
        /// </summary>
        /// <returns></returns>
        [FilterConfigs.LocalAuth]
        public IActionResult QQCos()
        {
            return View();
        }

        /// <summary>
        /// 秘钥
        /// </summary>
        public class AccessCOS
        {
            public static int APPID => Convert.ToInt32(GlobalTo.GetValue("ApiKey:AccessCOS:APPID"));
            public static string SecretId => GlobalTo.GetValue("ApiKey:AccessCOS:SecretId");
            public static string SecretKey => GlobalTo.GetValue("ApiKey:AccessCOS:SecretKey");
        }

        #region 签名

        /// <summary>
        /// 签名
        /// </summary>
        /// <param name="bucket">桶名</param>
        /// <param name="path">路径</param>
        /// <returns></returns>
        [ValidateAntiForgeryToken]
        [ResponseCache(Duration = 10)]
        public string QQSign(string bucket, string path = "")
        {
            Dictionary<string, string> dic = new Dictionary<string, string>
            {
                { "Signature", QQSignature(bucket) },
                { "SignatureOnce", QQSignatureOnce(bucket, path) }
            };
            return dic.ToJson();
        }

        /// <summary>
        /// 多次有效签名
        /// </summary>
        /// <param name="bucket">桶名</param>
        /// <returns></returns>
        [ResponseCache(Duration = 60)]
        public string QQSignature(string bucket)
        {
            long es = DateTime.Now.AddHours(12).ToTimestamp();
            string result = Sign.Signature(AccessCOS.APPID, AccessCOS.SecretId, AccessCOS.SecretKey, es, bucket);
            return result;
        }

        /// <summary>
        /// 单次有效签名
        /// </summary>
        /// <param name="bucket">桶名</param>
        /// <param name="path">路径</param>
        /// <returns></returns>
        public string QQSignatureOnce(string bucket, string path)
        {
            string result = Sign.SignatureOnce(AccessCOS.APPID, AccessCOS.SecretId, AccessCOS.SecretKey, path, bucket);
            return result;
        }
        #endregion

        /// <summary>
        /// QCloud COS API
        /// </summary>
        /// <param name="bucket">桶</param>
        /// <param name="path">文件（夹）路径</param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public string QQAPI(string bucket, string path)
        {
            var cos = new CosCloud(AccessCOS.APPID, AccessCOS.SecretId, AccessCOS.SecretKey, UrlType.HB);

            string cmd = RouteData.Values["id"]?.ToString().ToLower();
            string result = "fail";

            switch (cmd)
            {
                #region 列表
                case "list":
                    try
                    {
                        //关键字搜索
                        string keywords = Request.Form["keywords"].ToString();
                        var folderlistParasDic = new Dictionary<string, string>
                        {
                            { CosParameters.PARA_NUM, "1000" },
                            { CosParameters.PARA_ORDER, "0" },
                            { CosParameters.PARA_PATTERN, FolderPattern.PATTERN_BOTH },
                            { CosParameters.PARA_PREFIX, keywords }
                        };
                        //桶、路径下
                        result = cos.GetFolderList(bucket, path, folderlistParasDic);
                        result = System.Net.WebUtility.UrlDecode(result);
                    }
                    catch (Exception)
                    {
                        result = "{}";
                    }
                    break;
                #endregion

                #region 存在
                case "exists":
                    try
                    {
                        result = "0";

                        //文件名搜索
                        string keywords = Request.Form["key"].ToString();
                        var folderlistParasDic = new Dictionary<string, string>
                        {
                            { CosParameters.PARA_NUM, "1000" },
                            { CosParameters.PARA_ORDER, "0" },
                            { CosParameters.PARA_PATTERN, FolderPattern.PATTERN_BOTH },
                            { CosParameters.PARA_PREFIX, keywords }
                        };
                        //桶、路径下
                        result = cos.GetFolderList(bucket, path, folderlistParasDic);
                        JObject jo = JObject.Parse(result);
                        if (jo["code"].ToStringOrEmpty() == "0")
                        {
                            var infos = jo["data"]["infos"];
                            foreach (var item in infos)
                            {
                                foreach (JProperty jp in item)
                                {
                                    if (jp.Name == "name" && jp.Value.ToStringOrEmpty().Equals(keywords))
                                    {
                                        result = "1";
                                        goto eachflag;
                                    }
                                }
                            }
                        }
                        eachflag: if (result == "1")
                        {
                            result = "1";
                        }
                        else
                        {
                            result = "0";
                        }
                    }
                    catch (Exception)
                    {
                        result = "0";
                    }
                    break;
                #endregion

                #region 新建文件夹
                case "newfolder":
                    try
                    {
                        path = path.EndsWith("/") ? path : path + "/";
                        string folder = Request.Form["folder"].ToString();
                        result = cos.CreateFolder(bucket, path + folder);
                    }
                    catch (Exception)
                    {
                        result = "{}";
                    }
                    break;
                #endregion

                #region 删除
                case "del":
                    {
                        path = path.EndsWith("/") ? path : path + "/";

                        //是否都删除成功
                        bool b1 = false;
                        //是否有部分失败
                        bool b2 = false;

                        List<string> files = Request.Form["files"].ToString().Split(',').ToList();
                        List<string> folder = Request.Form["folder"].ToString().Split(',').ToList();

                        foreach (var item in files)
                        {
                            try
                            {
                                if (!string.IsNullOrWhiteSpace(item))
                                {
                                    JObject jo = jo = JObject.Parse(cos.DeleteFile(bucket, path + item));
                                    if (jo["code"].ToStringOrEmpty() == "0")
                                    {
                                        b1 = true;
                                    }
                                    else
                                    {
                                        b2 = true;
                                    }
                                }
                            }
                            catch (Exception)
                            {
                            }
                        }
                        foreach (var item in folder)
                        {
                            try
                            {
                                if (!string.IsNullOrWhiteSpace(item))
                                {
                                    JObject jo = JObject.Parse(cos.DeleteFolder(bucket, path + System.Net.WebUtility.UrlEncode(item)));
                                    if (jo["code"].ToStringOrEmpty() == "0")
                                    {
                                        b1 = true;
                                    }
                                    else
                                    {
                                        b2 = true;
                                    }
                                }
                            }
                            catch (Exception)
                            {
                            }
                        }
                        if (b2)
                        {
                            result = "partialfail";
                        }
                        else
                        {
                            if (b1)
                            {
                                result = "success";
                            }
                            else
                            {
                                result = "fail";
                            }
                        }
                    }
                    break;
                #endregion
            }


            return result;
        }

        #endregion

        #region 163yun.com NOS 对象存储

        /// <summary>
        /// 秘钥
        /// </summary>
        public class AccessNOS
        {
            public static string AccessKeyId => GlobalTo.GetValue("ApiKey:AccessNOS:accessKeyId");
            public static string AccessKeySecret => GlobalTo.GetValue("ApiKey:AccessNOS:accessKeySecret");
            public static string EndPoint => GlobalTo.GetValue("ApiKey:AccessNOS:endpoint");
        }

        /// <summary>
        /// NOS 对象存储
        /// </summary>
        /// <returns></returns>
        [FilterConfigs.LocalAuth]
        public IActionResult NENos()
        {
            return View();
        }

        /// <summary>
        /// NEAPI
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public string NEAPI()
        {
            string cmd = RouteData.Values["id"]?.ToString().ToLower();
            string bucket = Request.Form["bucket"].ToString();
            string result = "fail";

            var nosClient = new NosClient(AccessNOS.EndPoint, AccessNOS.AccessKeyId, AccessNOS.AccessKeySecret, new ClientConfiguration()
            {
                MaxErrorRetry = 2,
                MaxConnections = 200,
                ConnectionTimeout = 50000
            });

            switch (cmd)
            {
                #region 列举文件
                case "list":
                    var listObjectsRequest = new ListObjectsRequest(bucket)
                    {
                        MaxKeys = 1000,                        
                        Prefix = Request.Form["keywords"].ToString()
                    };
                    try
                    {
                        ObjectListing objList = nosClient.ListObjects(listObjectsRequest);
                        result = objList.ObjectSummarise.ToJson();
                    }
                    catch (Exception ex)
                    {
                        Core.ConsoleTo.Log(ex);
                        result = "[]";
                    }
                    break;
                #endregion

                #region 存在文件
                case "exists":
                    try
                    {
                        string key = Request.Form["key"].ToString();
                        bool b = nosClient.DoesObjectExist(bucket, key);
                        result = b ? "1" : "0";
                    }
                    catch (Exception)
                    {
                        result = "error";
                    }
                    break;
                #endregion

                #region 删除文件
                case "del":
                    try
                    {
                        var keys = Request.Form["key"].ToString().Split(',').ToList();
                        if (keys.Count == 1)
                        {
                            nosClient.DeleteObject(bucket, keys[0]);
                        }
                        else if (keys.Count > 1)
                        {
                            nosClient.DeleteObjects(bucket, keys, false);
                        }
                        result = "success";
                    }
                    catch (Exception)
                    {
                        result = "fail";
                    }
                    break;
                    #endregion
            }

            return result;
        }

        #endregion

        #region Qiniu对象存储

        /// <summary>
        /// Qiniu对象存储
        /// </summary>
        /// <returns></returns>
        [FilterConfigs.LocalAuth]
        public IActionResult QNKodo()
        {
            ViewData["DateUnix"] = DateTime.Now.AddHours(1).ToTimestamp();

            if (FilterConfigs.HelpFuncTo.LocalIsAuth(Request.Cookies["sk"] ?? ""))
            {
                ViewData["LocalIsAuth"] = 1;
            }

            return View();
        }

        public class AccessQN
        {
            public static string AK => GlobalTo.GetValue("ApiKey:AccessQN:AK");
            public static string SK => GlobalTo.GetValue("ApiKey:AccessQN:SK");
        }

        /// <summary>
        /// 获取Token
        /// </summary>
        /// <param name="type">命令类型</param>
        /// <returns></returns>
        [ValidateAntiForgeryToken]
        public IActionResult QNToken(string type)
        {
            var mac = new Mac(AccessQN.AK, AccessQN.SK);
            Auth auth = new Auth(mac);

            string result = string.Empty;
            switch (type)
            {
                case "upload":
                    {
                        string uploadConfig = Request.Form["uploadConfig"];
                        result = auth.CreateUploadToken(uploadConfig);
                    }
                    break;
                case "down":
                    result = auth.CreateDownloadToken(Request.Form["url"]);
                    break;
            }

            return Content(result);
        }

        /// <summary>
        /// Qiniu API
        /// </summary>
        /// <returns></returns>
        [ValidateAntiForgeryToken]
        public IActionResult QNAPI()
        {
            string cmd = RouteData.Values["id"]?.ToString().ToLower();
            //桶
            string bucketName = Request.Form["bucket"].ToString();

            var mac = new Mac(AccessQN.AK, AccessQN.SK);
            Qiniu.Common.Config.SetZone(Qiniu.Common.ZoneID.CN_East, true);
            var bucketManager = new Qiniu.RS.BucketManager(mac);

            var exc = new List<string>() { "index.html", "favicon.svg", "favicon.ico" };

            string result = string.Empty;

            switch (cmd)
            {
                case "list":
                    {
                        string KeyWords = Request.Form["keywords"].ToString();

                        var list = bucketManager.ListFiles(bucketName, KeyWords, null, 1000, null);

                        result = list.ToJson();
                    }
                    break;

                case "fetch":
                    {
                        string url = Request.Form["url"].ToString();

                        string filename = url.Split('/').ToList().LastOrDefault() ?? "";
                        int fn = filename.LastIndexOf('.');
                        string ext = string.Empty;
                        string name = filename.Substring(0, fn == -1 ? filename.Length : fn);
                        string nameps = "(fetch_" + DateTime.Now.ToTimestamp() + ")";
                        if (fn >= 0)
                        {
                            ext = filename.Substring(fn);
                        }
                        var rt = bucketManager.Fetch(url, bucketName, name + nameps + ext);
                        result = rt.ToJson();
                    }
                    break;

                case "del":
                    {
                        var keys = Request.Form["key"].ToString().Split(',').ToList();
                        var listOp = new List<string>();
                        foreach (string key in keys)
                        {
                            if (!exc.Contains(key.ToLower()))
                            {
                                listOp.Add(bucketManager.DeleteOp(bucketName, key));
                            }
                        }
                        if (listOp.Count > 0)
                        {
                            var rt = bucketManager.Batch(listOp.ToArray());
                            result = rt.ToJson();
                        }
                        else
                        {
                            result = new
                            {
                                Result = "NotAllow"
                            }.ToJson();
                        }
                    }
                    break;
            }

            return Content(result);
        }

        #endregion

        #region Upyun云存储

        /// <summary>
        /// Upyun云存储
        /// </summary>
        /// <returns></returns>
        public IActionResult UPUss()
        {
            return View();
        }

        #endregion

    }
}