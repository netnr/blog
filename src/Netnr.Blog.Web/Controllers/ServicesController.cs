using System.Net;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using Netnr.Core;
using Netnr.SharedFast;
using Netnr.WeChat;
using Netnr.WeChat.Entities;
using Netnr.Blog.Application;
using Netnr.Blog.Data;

namespace Netnr.Blog.Web.Controllers
{
    /// <summary>
    /// 服务
    /// </summary>
    [Route("[Controller]/[action]")]
    public class ServicesController : Controller
    {
        #region 微信公众号

        /// <summary>
        /// 开发者接管
        /// </summary>
        /// <param name="signature"></param>
        /// <param name="timestamp"></param>
        /// <param name="nonce"></param>
        /// <param name="echostr"></param>
        /// <param name="encrypt_type"></param>
        /// <param name="msg_signature"></param>
        [ApiExplorerSettings(IgnoreApi = true)]
        public async void WeChat(string signature, string timestamp, string nonce, string echostr, string encrypt_type, string msg_signature)
        {
            string result = string.Empty;

            //微信后台验证地址（使用Get），微信后台的“接口配置信息”的Url
            if (Request.Method.ToLower() == "get")
            {
                var Token = GlobalTo.GetValue("ApiKey:WeChatMP:Token");

                if (Netnr.WeChat.Helpers.Util.CheckSignature(signature, timestamp, nonce, Token))
                {
                    //返回随机字符串则表示验证通过
                    result = echostr;
                }
                else
                {
                    result = "参数错误！";
                }
            }
            //处理请求
            else
            {
                WeChatMessage message = null;
                var safeMode = encrypt_type == "aes";

                var Token = string.Empty;
                var EncodingAESKey = string.Empty;
                var AppID = string.Empty;

                if (safeMode)
                {
                    Token = GlobalTo.GetValue("ApiKey:WeChatMP:Token");
                    EncodingAESKey = GlobalTo.GetValue("ApiKey:WeChatMP:EncodingAESKey");
                    AppID = GlobalTo.GetValue("ApiKey:WeChatMP:AppID");
                }

                using (var ms = new MemoryStream())
                {
                    await Request.Body.CopyToAsync(ms);
                    var myByteArray = ms.ToArray();

                    var decryptMsg = string.Empty;
                    string postStr = Encoding.UTF8.GetString(myByteArray);

                    Console.WriteLine(postStr);

                    #region 解密
                    if (safeMode)
                    {
                        var wxBizMsgCrypt = new WeChat.Helpers.Crypto.WXBizMsgCrypt(Token, EncodingAESKey, AppID);
                        var ret = wxBizMsgCrypt.DecryptMsg(msg_signature, timestamp, nonce, postStr, ref decryptMsg);
                        //解密失败
                        if (ret != 0)
                        {
                            Apps.FilterConfigs.WriteLog(HttpContext, new Exception("微信解密失败"));
                        }
                    }
                    else
                    {
                        decryptMsg = postStr;
                    }
                    #endregion

                    message = WeChatMessage.Parse(decryptMsg);
                }
                var response = new WeChatExecutor().Execute(message);

                #region 加密
                if (safeMode)
                {
                    var wxBizMsgCrypt = new WeChat.Helpers.Crypto.WXBizMsgCrypt(Token, EncodingAESKey, AppID);
                    var ret = wxBizMsgCrypt.EncryptMsg(response, timestamp, nonce, ref result);
                    if (ret != 0)//加密失败
                    {
                        Apps.FilterConfigs.WriteLog(HttpContext, new Exception("微信加密失败"));
                    }
                }
                else
                {
                    result = response;
                }
                #endregion
            }

            Console.WriteLine(result);

            //输出
            byte[] buffer = Encoding.UTF8.GetBytes(result);
            await Response.Body.WriteAsync(buffer.AsMemory(0, buffer.Length));
            await Response.Body.FlushAsync();
        }

        public class WeChatExecutor : IWeChatExecutor
        {
            /// <summary>
            /// 处理微信消息
            /// </summary>
            /// <param name="message"></param>
            /// <returns>已经打包成xml的用于回复用户的消息包</returns>
            public string Execute(WeChatMessage message)
            {
                var myDomain = GlobalTo.GetValue("Common:Domain");
                string myPic = $"{myDomain}/favicon.svg";

                var mb = message.Body;
                var openId = mb.GetText("FromUserName");
                var myUserName = mb.GetText("ToUserName");

                var news = new WeChatNews
                {
                    title = GlobalTo.GetValue("Common:ChineseName") + "（Gist,Run,Doc,Draw）",
                    description = GlobalTo.GetValue("Common:ChineseName") + "，技术分享博客、代码片段、在线运行代码、接口文档、绘制 等等",
                    picurl = myPic,
                    url = myDomain
                };

                //默认首页
                string result = ReplayPassiveMessage.RepayNews(openId, myUserName, news);

                switch (message.Type)
                {
                    //文字消息
                    case WeChatMessageType.Text:
                        {
                            string Content = mb.GetText("Content");
                            string repmsg = string.Empty;

                            if ("sj".Split(' ').ToList().Contains(Content))
                            {
                                repmsg = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                            }
                            else if ("xh".Split(' ').ToList().Contains(Content))
                            {
                                repmsg = "笑话\nhttps://ss.netnr.com/qiushibaike";
                            }
                            else if ("note".Split(' ').ToList().Contains(Content))
                            {
                                repmsg = $"记事\n{myDomain}/tool/note";
                            }
                            else if ("gist".Split(' ').ToList().Contains(Content))
                            {
                                repmsg = $"代码片段\n{myDomain}/gist/discover";
                            }
                            else if ("doc".Split(' ').ToList().Contains(Content))
                            {
                                repmsg = $"文档\n{myDomain}/doc/discover";
                            }
                            else if ("cp lottery".Split(' ').ToList().Contains(Content))
                            {
                                repmsg = "彩票\nhttps://ss.netnr.com/lottery";
                            }

                            if (!string.IsNullOrWhiteSpace(repmsg))
                            {
                                result = ReplayPassiveMessage.RepayText(openId, myUserName, repmsg);
                            }
                        }
                        break;
                }
                return result;
            }
        }

        #endregion

        /// <summary>
        /// 数据库导出
        /// </summary>
        /// <param name="zipName">文件名</param>
        /// <returns></returns>
        [HttpGet]
        [Apps.FilterConfigs.IsAdmin]
        public SharedResultVM DatabaseExport(string zipName = "db/backup.zip")
        {
            return SharedResultVM.Try(vm =>
            {
                var edb = new SharedDataKit.TransferVM.ExportDatabase
                {
                    ZipPath = PathTo.Combine(GlobalTo.ContentRootPath, zipName),
                    ReadConnectionInfo = new SharedDataKit.TransferVM.ConnectionInfo()
                    {
                        ConnectionString = SharedDbContext.FactoryTo.GetConn().Replace("Filename=", "Data Source="),
                        ConnectionType = GlobalTo.TDB
                    }
                };

                vm = SharedDataKit.DataKit.ExportDatabase(edb);

                return vm;
            });
        }

        /// <summary>
        /// 数据库备份到Git
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Apps.FilterConfigs.IsAdmin]
        public SharedResultVM DatabaseBackupToGit()
        {
            return SharedResultVM.Try(vm =>
            {
                if (GlobalTo.GetValue<bool>("ReadOnly"))
                {
                    vm.Set(SharedEnum.RTag.refuse);
                    return vm;
                }

                var now = $"{DateTime.Now:yyyyMMdd_HHmmss}";

                var db = ContextBaseFactory.CreateDbContext();
                var database = db.Database.GetDbConnection().Database;

                var createScript = db.Database.GenerateCreateScript();

                //备份创建脚本
                try
                {
                    var b1 = Convert.ToBase64String(Encoding.UTF8.GetBytes(createScript));
                    var p1 = $"{database}/backup_{now}.sql";

                    vm.Log.Add(PutGitee(b1, p1, now));
                    vm.Log.Add(PutGitHub(b1, p1, now));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    vm.Log.Add(ex.Message);
                }

                Thread.Sleep(1000 * 1);

                //备份数据
                var zipPath = $"db/backup_{now}.zip";
                if (DatabaseExport(zipPath).Code == 200)
                {
                    var ppath = PathTo.Combine(GlobalTo.ContentRootPath, zipPath);

                    try
                    {
                        var b2 = Convert.ToBase64String(System.IO.File.ReadAllBytes(ppath));
                        var p2 = $"{database}/backup_{now}.zip";

                        vm.Log.Add(PutGitee(b2, p2, now));
                        vm.Log.Add(PutGitHub(b2, p2, now));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                        vm.Log.Add(ex.Message);

                        System.IO.File.Delete(ppath);
                    }
                    finally
                    {
                        System.IO.File.Delete(ppath);
                    }
                }

                return vm;
            });
        }

        /// <summary>
        /// 推送到GitHub
        /// </summary>
        /// <param name="content">内容 base64</param>
        /// <param name="path">路径</param>
        /// <param name="message"></param>
        /// <param name="token"></param>
        /// <param name="or"></param>
        /// <returns></returns>
        private static string PutGitHub(string content, string path, string message = "m", string token = null, string or = null)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                token = GlobalTo.GetValue("ApiKey:GitHub:GistToken");
            }
            if (string.IsNullOrWhiteSpace(or))
            {
                or = GlobalTo.GetValue("Work:BackupToGit:or");
            }

            var put = $"https://api.github.com/repos/{or}/contents/{path}";

            var hwr = HttpTo.HWRequest(put, "PUT", Encoding.UTF8.GetBytes(new { message, content }.ToJson()));

            hwr.Headers.Set("Accept", "application/vnd.github.v3+json");
            hwr.Headers.Set("Authorization", $"token {token}");
            hwr.Headers.Set("Content-Type", "application/json");
            hwr.UserAgent = "Netnr Agent";

            var result = HttpTo.Url(hwr);

            return result;
        }

        /// <summary>
        /// 推送到Gitee
        /// </summary>
        /// <param name="content">内容 base64</param>
        /// <param name="path">路径</param>
        /// <param name="message"></param>
        /// <param name="token"></param>
        /// <param name="or"></param>
        /// <returns></returns>
        private static string PutGitee(string content, string path, string message = "m", string token = null, string or = null)
        {
            if (string.IsNullOrWhiteSpace(token))
            {
                token = GlobalTo.GetValue("ApiKey:Gitee:GistToken");
            }
            if (string.IsNullOrWhiteSpace(or))
            {
                or = GlobalTo.GetValue("Work:BackupToGit:or");
            }

            var listor = or.Split('/');
            var owner = listor.First();
            var repo = listor.Last();
            var uri = $"https://gitee.com/api/v5/repos/{owner}/{repo}/contents/{path}";

            var hwr = HttpTo.HWRequest(uri, "POST", Encoding.UTF8.GetBytes(new { access_token = token, message, content }.ToJson()));
            hwr.Headers.Set("Content-Type", "application/json");

            var result = HttpTo.Url(hwr);

            return result;
        }

        /// <summary>
        /// 数据库导出（示例数据）
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Apps.FilterConfigs.IsAdmin]
        public SharedResultVM DatabaseExportDemo()
        {
            var vm = new SharedResultVM();

            try
            {
                var export_before = "db/backup_demo_before.zip";
                var export_demo = "db/backup_demo.zip";

                //备份
                if (DatabaseExport(export_before).Code == 200)
                {
                    //清理仅保留示例数据

                    using var db = ContextBaseFactory.CreateDbContext();

                    db.UserInfo.RemoveRange(db.UserInfo.ToList());
                    db.UserInfo.Add(new Domain.UserInfo()
                    {
                        UserId = 1,
                        UserName = "netnr",
                        UserPwd = "e10adc3949ba59abbe56e057f20f883e",//123456
                        UserCreateTime = DateTime.Now
                    });

                    db.UserConnection.RemoveRange(db.UserConnection.ToList());
                    db.UserMessage.RemoveRange(db.UserMessage.ToList());
                    db.UserReply.RemoveRange(db.UserReply.Where(x => x.UrTargetId != "117").ToList());
                    db.UserWriting.RemoveRange(db.UserWriting.Where(x => x.UwId != 117).ToList());
                    db.UserWritingTags.RemoveRange(db.UserWritingTags.Where(x => x.UwId != 117).ToList());

                    db.Tags.RemoveRange(db.Tags.Where(x => x.TagId != 58 && x.TagId != 96).ToList());

                    db.Run.RemoveRange(db.Run.OrderBy(x => x.RunCreateTime).Skip(1).ToList());

                    db.OperationRecord.RemoveRange(db.OperationRecord.ToList());

                    db.Notepad.RemoveRange(db.Notepad.ToList());

                    db.KeyValues.RemoveRange(db.KeyValues.Where(x => x.KeyName != "https" && x.KeyName != "browser").ToList());
                    db.KeyValueSynonym.RemoveRange(db.KeyValueSynonym.ToList());

                    db.GuffRecord.RemoveRange(db.GuffRecord.ToList());

                    db.Gist.RemoveRange(db.Gist.Where(x => x.GistCode != "5373307231488995367").ToList());
                    db.GistSync.RemoveRange(db.GistSync.Where(x => x.GistCode != "5373307231488995367").ToList());

                    db.GiftRecord.RemoveRange(db.GiftRecord.ToList());
                    db.GiftRecordDetail.RemoveRange(db.GiftRecordDetail.ToList());

                    db.Draw.RemoveRange(db.Draw.Where(x => x.DrId != "d4969500168496794720" && x.DrId != "m4976065893797151245").ToList());

                    db.DocSet.RemoveRange(db.DocSet.Where(x => x.DsCode != "4840050256984581805" && x.DsCode != "5036967707833574483").ToList());
                    db.DocSetDetail.RemoveRange(db.DocSetDetail.Where(x => x.DsCode != "4840050256984581805" && x.DsCode != "5036967707833574483").ToList());

                    var num = db.SaveChanges();

                    //导出示例数据
                    vm = DatabaseExport(export_demo);

                    //导入恢复
                    if (DatabaseImport(export_before, true).Code == 200)
                    {
                        var fullPath = PathTo.Combine(GlobalTo.ContentRootPath, "db", export_before);
                        System.IO.File.Delete(fullPath);
                    }
                }
            }
            catch (Exception ex)
            {
                vm.Set(ex);
            }

            return vm;
        }

        /// <summary>
        /// 数据库导入
        /// </summary>
        /// <param name="zipName">文件名</param>
        /// <param name="clearTable">清空表，默认 false</param>
        /// <returns></returns>
        [HttpGet]
        [Apps.FilterConfigs.IsAdmin]
        public SharedResultVM DatabaseImport(string zipName = "db/backup.zip", bool clearTable = false)
        {
            return SharedResultVM.Try(vm =>
            {
                var idb = new SharedDataKit.TransferVM.ImportDatabase
                {
                    WriteConnectionInfo = new SharedDataKit.TransferVM.ConnectionInfo
                    {
                        ConnectionType = GlobalTo.TDB,
                        ConnectionString = SharedDbContext.FactoryTo.GetConn().Replace("Filename=", "Data Source=")
                    },
                    ZipPath = PathTo.Combine(GlobalTo.ContentRootPath, zipName),
                    WriteDeleteData = clearTable
                };

                vm = SharedDataKit.DataKit.ImportDatabase(idb);

                return vm;
            });
        }

        /// <summary>
        /// Gist 代码片段，同步到 GitHub、Gitee
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Apps.FilterConfigs.IsAdmin]
        public SharedResultVM GistSync()
        {
            var vm = new SharedResultVM();

            try
            {
                using var db = ContextBaseFactory.CreateDbContext();

                //同步用户ID
                int UserId = GlobalTo.GetValue<int>("Work:GistSync:UserId");

                //日志
                var listLog = new List<object>() { "Gist代码片段同步" };

                var listGist = db.Gist.Where(x => x.Uid == UserId).OrderBy(x => x.GistCreateTime).ToList();

                var codes = listGist.Select(x => x.GistCode).ToList();

                var listGs = db.GistSync.Where(x => x.Uid == UserId).ToList();

                //执行命令记录
                var dicSync = new Dictionary<string, string>();

                foreach (var gist in listGist)
                {
                    var gs = listGs.FirstOrDefault(x => x.GistCode == gist.GistCode);
                    //新增
                    if (gs == null)
                    {
                        dicSync.Add(gist.GistCode, "add");
                    }
                    else if (gs?.GsGitHubTime != gist.GistUpdateTime || gs?.GsGiteeTime != gist.GistUpdateTime)
                    {
                        dicSync.Add(gist.GistCode, "update");
                    }
                }

                //删除
                var delCode = listGs.Select(x => x.GistCode).Except(listGist.Select(x => x.GistCode)).ToList();

                var token_gh = GlobalTo.GetValue("ApiKey:GitHub:GistToken");
                var token_ge = GlobalTo.GetValue("ApiKey:Gitee:GistToken");

                listLog.Add("同步新增、修改：" + dicSync.Count + " 条");
                listLog.Add(dicSync);

                //同步新增、修改
                if (dicSync.Count > 0)
                {
                    foreach (var key in dicSync.Keys)
                    {
                        var st = dicSync[key];
                        var gist = listGist.FirstOrDefault(x => x.GistCode == key);
                        var gs = listGs.FirstOrDefault(x => x.GistCode == key);

                        #region 发送主体
                        var jo = new JObject
                        {
                            ["access_token"] = token_ge,//only gitee 

                            ["description"] = gist.GistRemark,
                            ["public"] = gist.GistOpen == 1
                        };

                        var jc = new JObject
                        {
                            ["content"] = gist.GistContent
                        };

                        var jf = new JObject
                        {
                            [gist.GistFilename] = jc
                        };

                        jo["files"] = jf;

                        byte[] sendData = Encoding.UTF8.GetBytes(jo.ToJson());
                        #endregion

                        switch (st)
                        {
                            case "add":
                                {
                                    var gsmo = new Domain.GistSync()
                                    {
                                        GistCode = key,
                                        Uid = UserId,
                                        GistFilename = gist.GistFilename
                                    };

                                    //GitHub
                                    {
                                        var hwr = HttpTo.HWRequest("https://api.github.com/gists", "POST", sendData);
                                        hwr.Headers.Add(HttpRequestHeader.Authorization, "token " + token_gh);
                                        hwr.ContentType = "application/json";
                                        hwr.UserAgent = "Netnr Agent";

                                        var rt = HttpTo.Url(hwr);

                                        gsmo.GsGitHubId = rt.ToJObject()["id"].ToString();
                                        gsmo.GsGitHubTime = gist.GistUpdateTime;
                                    }

                                    //Gitee
                                    {
                                        var hwr = HttpTo.HWRequest("https://gitee.com/api/v5/gists", "POST", sendData);
                                        hwr.ContentType = "application/json";

                                        var rt = HttpTo.Url(hwr);

                                        gsmo.GsGiteeId = rt.ToJObject()["id"].ToString();
                                        gsmo.GsGiteeTime = gist.GistUpdateTime;
                                    }

                                    _ = db.GistSync.Add(gsmo);
                                    _ = db.SaveChanges();

                                    listLog.Add("新增一条成功");
                                    listLog.Add(gsmo);
                                }
                                break;
                            case "update":
                                {
                                    if (gs.GistFilename != gist.GistFilename)
                                    {
                                        jo["files"][gs.GistFilename] = null;
                                        gs.GistFilename = gist.GistFilename;
                                    }

                                    //GitHub
                                    {
                                        var hwr = HttpTo.HWRequest("https://api.github.com/gists/" + gs.GsGitHubId, "PATCH", sendData);
                                        hwr.Headers.Add(HttpRequestHeader.Authorization, "token " + token_gh);
                                        hwr.ContentType = "application/json";
                                        hwr.UserAgent = "Netnr Agent";

                                        _ = HttpTo.Url(hwr);

                                        gs.GsGitHubTime = gist.GistUpdateTime;
                                    }

                                    //Gitee
                                    {
                                        var hwr = HttpTo.HWRequest("https://gitee.com/api/v5/gists/" + gs.GsGiteeId, "PATCH", sendData);
                                        hwr.ContentType = "application/json";

                                        _ = HttpTo.Url(hwr);

                                        gs.GsGiteeTime = gist.GistUpdateTime;
                                    }

                                    _ = db.GistSync.Update(gs);
                                    _ = db.SaveChanges();

                                    listLog.Add("更新一条成功");
                                    listLog.Add(gs);
                                }
                                break;
                        }

                        Thread.Sleep(1000 * 2);
                    }
                }

                listLog.Add("同步删除：" + delCode.Count + " 条");
                listLog.Add(delCode);

                //同步删除
                if (delCode.Count > 0)
                {
                    foreach (var code in delCode)
                    {
                        var gs = listGs.FirstOrDefault(x => x.GistCode == code);

                        var dc = "00".ToCharArray();

                        #region GitHub
                        var hwr_gh = HttpTo.HWRequest("https://api.github.com/gists/" + gs.GsGitHubId, "DELETE");
                        hwr_gh.Headers.Add(HttpRequestHeader.Authorization, "token " + token_gh);
                        hwr_gh.UserAgent = "Netnr Agent";
                        var resp_gh = (HttpWebResponse)hwr_gh.GetResponse();
                        if (resp_gh.StatusCode == HttpStatusCode.NoContent)
                        {
                            dc[0] = '1';
                        }
                        #endregion

                        #region Gitee
                        var hwr_ge = HttpTo.HWRequest("https://gitee.com/api/v5/gists/" + gs.GsGiteeId + "?access_token=" + token_ge, "DELETE");
                        var resp_ge = (HttpWebResponse)hwr_ge.GetResponse();
                        if (resp_ge.StatusCode == HttpStatusCode.NoContent)
                        {
                            dc[1] = '1';
                        }
                        #endregion

                        if (string.Join("", dc) == "11")
                        {
                            _ = db.GistSync.Remove(gs);
                            _ = db.SaveChanges();

                            listLog.Add("删除一条成功");
                            listLog.Add(gs);
                        }
                        else
                        {
                            listLog.Add("删除一条异常");
                            listLog.Add(dc);
                        }

                        Thread.Sleep(1000 * 2);
                    }
                }

                listLog.Add("完成同步");

                vm.Set(SharedEnum.RTag.success);
                vm.Data = listLog;

            }
            catch (Exception ex)
            {
                vm.Set(ex);
                ConsoleTo.Log(ex);
            }

            return vm;
        }

        /// <summary>
        /// 处理操作记录
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Apps.FilterConfigs.IsAdmin]
        public SharedResultVM HandleOperationRecord()
        {
            var vm = new SharedResultVM();

            try
            {
                using var db = ContextBaseFactory.CreateDbContext();

                //处理Guff查询记录数
                var ctype = EnumService.ConnectionType.GuffRecord.ToString();
                var listOr = db.OperationRecord.Where(x => x.OrType == ctype && x.OrMark == "default").ToList();
                if (listOr.Count > 0)
                {
                    var listAllId = string.Join(",", listOr.Select(x => x.OrSource).ToList()).Split(',').ToList();
                    var listid = listAllId.Distinct();

                    var listmo = db.GuffRecord.Where(x => listid.Contains(x.GrId)).ToList();
                    foreach (var item in listmo)
                    {
                        item.GrReadNum += listAllId.GroupBy(x => x).FirstOrDefault(x => x.Key == item.GrId).Count();
                    }
                    db.GuffRecord.UpdateRange(listmo);

                    db.OperationRecord.RemoveRange(listOr);

                    int num = db.SaveChanges();

                    vm.Set(num > 0);
                    vm.Data = "处理操作记录，受影响行数：" + num;
                }
                else
                {
                    vm.Set(SharedEnum.RTag.lack);
                }
            }
            catch (Exception ex)
            {
                vm.Set(ex);
            }

            return vm;
        }

        /// <summary>
        /// WebHook
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<SharedResultVM> WebHook()
        {
            var vm = new SharedResultVM();

            try
            {
                if (Request.Method == "POST")
                {
                    using var ms = new MemoryStream();
                    await Request.Body.CopyToAsync(ms);
                    string postStr = Encoding.UTF8.GetString(ms.ToArray());
                    Console.WriteLine(postStr);

                    //TO DO
                    var jo = postStr.ToJObject();
                    var respName = jo["repository"]["name"].ToString();
                    var projectPath = $"/package/site/{respName}";
                    if (Directory.Exists(projectPath) && Directory.Exists(Path.Combine(projectPath, ".git")))
                    {
                        var arg = $"git -C \"{projectPath}\" pull --all";
                        Console.WriteLine(arg);
                        var cr = CmdTo.Execute(arg);
                        var rt = cr.CrOutput + cr.CrError;
                        Console.WriteLine(rt);
                        vm.Log.AddRange(rt.Split('\n'));
                    }
                    else
                    {
                        var errorMsg = $"{projectPath} Not Exists(.git)";
                        Console.WriteLine(errorMsg);
                        vm.Log.Add(errorMsg);
                    }

                    vm.Set(SharedEnum.RTag.success);
                }
            }
            catch (Exception ex)
            {
                vm.Set(ex);
            }

            return vm;
        }

        /// <summary>
        /// 静态资源使用分析（已处理到 2021-07）
        /// </summary>
        /// <param name="rootPath">静态资源根目录</param>
        /// <returns></returns>
        [HttpGet]
        public SharedResultVM StaticResourceUsageAnalysis(string rootPath = @"D:\ROOM\static")
        {
            return SharedResultVM.Try(vm =>
            {
                vm.LogEvent(le =>
                {
                    Console.WriteLine(le.NewItems[0]);
                });

                var db = ContextBaseFactory.CreateDbContext();
                var uws = db.UserWriting.ToList();
                var urs = db.UserReply.ToList();
                var runs = db.Run.ToList();

                var filesPath = Directory.GetFiles(rootPath, "*", SearchOption.AllDirectories).ToList();
                foreach (var path in filesPath)
                {
                    if (!path.Contains(".git") && !path.Contains($@"{rootPath}\static\"))
                    {
                        var filename = Path.GetFileName(path);

                        if (!uws.Any(x => x.UwContent.Contains(filename)) && !urs.Any(x => x.UrContent != null && x.UrContent.Contains(filename)) && !runs.Any(x => x.RunContent1.Contains(filename) || x.RunContent2.Contains(filename)))
                        {
                            vm.Log.Add(path);
                        }
                    }
                }

                return vm;
            });
        }
    }
}