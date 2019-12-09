using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Netnr.Data;
using Newtonsoft.Json.Linq;
using Netnr.Web.Filters;
using System.IO;
using Netnr.WeChat;
using Netnr.WeChat.Entities;
using Microsoft.Extensions.Hosting;
using System.Threading;
using Qcloud.Shared.Api;
using System.ComponentModel;
using Netnr.Func.ViewModel;
using Microsoft.AspNetCore.Http;

namespace Netnr.Web.Controllers
{
    /// <summary>
    /// 服务、对接
    /// </summary>
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
                    string postStr = System.Text.Encoding.UTF8.GetString(myByteArray);

                    #region 解密
                    if (safeMode)
                    {
                        var wxBizMsgCrypt = new WeChat.Helpers.Crypto.WXBizMsgCrypt(Token, EncodingAESKey, AppID);
                        var ret = wxBizMsgCrypt.DecryptMsg(msg_signature, timestamp, nonce, postStr, ref decryptMsg);
                        //解密失败
                        if (ret != 0)
                        {
                            Core.ConsoleTo.Log(DateTime.Now.ToLocalTime() + "微信解密失败");
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
                        Core.ConsoleTo.Log(DateTime.Now.ToLocalTime() + "微信加密失败");
                    }
                }
                else
                {
                    result = response;
                }
                #endregion
            }

            //输出
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(result);
            await Response.Body.WriteAsync(buffer, 0, buffer.Length);
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
                var myDomain = "https://www.netnr.com";//请更改成你的域名
                string myPic = myDomain + "/favicon.ico";

                var mb = message.Body;
                var openId = mb.GetText("FromUserName");
                var myUserName = mb.GetText("ToUserName");

                var news = new WeChatNews
                {
                    title = "NET牛人（Gist,Run,Doc,Draw）",
                    description = "NET牛人，技术分享博客、代码片段、在线运行代码、接口文档、绘制 等等",
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
                                repmsg = "记事\n" + myDomain + "/tool/note";
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

        #region WebHook

        /// <summary>
        /// WebHook
        /// </summary>
        /// <returns></returns>
        public ActionResultVM WebHook()
        {
            var vm = new ActionResultVM();

            try
            {
                if (Request.Method == "POST")
                {
                    using var ms = new MemoryStream();
                    Request.Body.CopyTo(ms);
                    string postStr = System.Text.Encoding.UTF8.GetString(ms.ToArray());

                    //new WebHookService(postStr);

                    vm.data = postStr;
                    vm.Set(ARTag.success);
                }
            }
            catch (Exception ex)
            {
                vm.Set(ex);
                Core.ConsoleTo.Log(ex);
            }

            return vm;
        }

        public class WebHookService
        {
            /// <summary>
            /// 推送的JSON包
            /// </summary>
            public JObject JoPush { get; set; }

            /// <summary>
            /// 邮箱
            /// </summary>
            public string Pemail { get; set; }

            /// <summary>
            /// 仓库名
            /// </summary>
            public string PrepositoryName { get; set; }

            /// <summary>
            /// 提交的信息
            /// </summary>
            public string PcommitMessage { get; set; }

            /// <summary>
            /// clone url
            /// </summary>
            public string PgitUrl { get; set; }

            /// <summary>
            /// 仓库主页链接
            /// </summary>
            public string PhomePage { get; set; }

            /// <summary>
            /// 构造 部署
            /// </summary>
            /// <param name="postStr">推送的JSON包</param>
            public WebHookService(string postStr)
            {
                JoPush = JObject.Parse(postStr);
                if (JoPush.ContainsKey("pusher"))
                {
                    Pemail = JoPush["pusher"]["email"].ToString();
                    PrepositoryName = JoPush["repository"]["name"].ToString();
                    PcommitMessage = JoPush["commits"][0]["message"].ToString();
                    PgitUrl = JoPush["repository"]["clone_url"].ToString();
                    PhomePage = JoPush["repository"]["homepage"].ToString();
                    string configEmail = GlobalTo.GetValue("WebHook:GitHub:Email");
                    string configNotDeploy = GlobalTo.GetValue("WebHook:GitHub:NotDeploy");
                    if (Pemail == configEmail && !PcommitMessage.ToLower().Contains(configNotDeploy))
                    {
                        Deploy();
                    }
                }
            }

            /// <summary>
            /// 部署
            /// </summary>
            public void Deploy()
            {
                //根目录
                string domainPath = GlobalTo.GetValue("WebHook:GitHub:DomainRootPath");

                //子域名&文件夹
                string subdomain = PrepositoryName;
                if (!string.IsNullOrWhiteSpace(PhomePage))
                {
                    subdomain = PhomePage.Replace("//", "^").Split('^')[1].Split('.')[0];
                }

                string path = domainPath + subdomain;

                //异步
                ThreadPool.QueueUserWorkItem(callBack =>
                {
                    if (!Directory.Exists(path))
                    {
                        string cmd = CmdFor.GitClone(PgitUrl, path);
                        Core.CmdTo.Shell(cmd);
                    }
                    else
                    {
                        string cmd = CmdFor.GitPull(path);
                        Core.CmdTo.Shell(cmd);
                    }
                });
            }

            /// <summary>
            /// 命令
            /// </summary>
            public class CmdFor
            {
                public static string GitClone(string giturl, string path)
                {
                    return $"git clone {giturl} {path}";
                }

                public static string GitPull(string path)
                {
                    return $"cd {path} && git pull origin master";
                }
            }
        }

        #endregion

        #region 百科字典

        /// <summary>
        /// 字典
        /// </summary>
        /// <returns></returns>
        [FilterConfigs.LocalAuth]
        public IActionResult KeyValues()
        {
            string cmd = RouteData.Values["id"]?.ToString();
            if (cmd != null)
            {
                string result = string.Empty;
                var rt = new List<object>
                {
                    0,
                    "fail"
                };

                try
                {
                    switch (cmd)
                    {
                        case "grab":
                            {
                                string key = Request.Form["Key"].ToString();
                                string api = $"https://baike.baidu.com/api/openapi/BaikeLemmaCardApi?scope=103&format=json&appid=379020&bk_key={key.ToEncode()}&bk_length=600";
                                string apirt = Core.HttpTo.Get(api);
                                if (apirt.Length > 100)
                                {
                                    using var db = new ContextBase();
                                    var kvMo = db.KeyValues.Where(x => x.KeyName == key).FirstOrDefault();
                                    if (kvMo == null)
                                    {
                                        kvMo = new Domain.KeyValues
                                        {
                                            KeyId = Guid.NewGuid().ToString(),
                                            KeyName = key.ToLower(),
                                            KeyValue = apirt
                                        };
                                        db.KeyValues.Add(kvMo);
                                    }
                                    else
                                    {
                                        kvMo.KeyValue = apirt;
                                        db.KeyValues.Update(kvMo);
                                    }

                                    rt[0] = db.SaveChanges();
                                    rt[1] = kvMo;
                                }
                                else
                                {
                                    rt[0] = 0;
                                    rt[1] = apirt;
                                }
                            }
                            break;
                        case "synonym":
                            {
                                var keys = Request.Form["keys"].ToString().Split(',').ToList();

                                string mainKey = keys.First().ToLower();
                                keys.RemoveAt(0);

                                var listkvs = new List<Domain.KeyValueSynonym>();
                                foreach (var key in keys)
                                {
                                    var kvs = new Domain.KeyValueSynonym
                                    {
                                        KsId = Guid.NewGuid().ToString(),
                                        KeyName = mainKey,
                                        KsName = key.ToLower()
                                    };
                                    listkvs.Add(kvs);
                                }

                                using var db = new ContextBase();
                                var mo = db.KeyValueSynonym.Where(x => x.KeyName == mainKey).FirstOrDefault();
                                if (mo != null)
                                {
                                    db.KeyValueSynonym.Remove(mo);
                                }
                                db.KeyValueSynonym.AddRange(listkvs);
                                int oldrow = db.SaveChanges();
                                rt[0] = 1;
                                rt[1] = " 受影响 " + oldrow + " 行";
                            }
                            break;
                        case "addtag":
                            {
                                var tags = Request.Form["tags"].ToString().Split(',').ToList();

                                if (tags.Count > 0)
                                {
                                    using var db = new ContextBase();
                                    var mt = db.Tags.Where(x => tags.Contains(x.TagName)).ToList();
                                    if (mt.Count == 0)
                                    {
                                        var listMo = new List<Domain.Tags>();
                                        var tagHs = new HashSet<string>();
                                        foreach (var tag in tags)
                                        {
                                            if (tagHs.Add(tag))
                                            {
                                                var mo = new Domain.Tags
                                                {
                                                    TagName = tag.ToLower(),
                                                    TagStatus = 1,
                                                    TagHot = 0,
                                                    TagIcon = tag.ToLower() + ".svg"
                                                };
                                                listMo.Add(mo);
                                            }
                                        }
                                        tagHs.Clear();

                                        //新增&刷新缓存
                                        db.Tags.AddRange(listMo);
                                        rt[0] = db.SaveChanges();

                                        Func.Common.TagsQuery(false);

                                        rt[1] = "操作成功";
                                    }
                                    else
                                    {
                                        rt[0] = 0;
                                        rt[1] = "标签已存在：" + mt.ToJson();
                                    }
                                }
                                else
                                {
                                    rt[0] = 0;
                                    rt[1] = "新增标签不能为空";
                                }
                            }
                            break;
                    }
                }
                catch (Exception ex)
                {
                    rt[1] = ex.Message;
                    rt.Add(ex.StackTrace);
                }

                result = rt.ToJson();
                return Content(result);
            }
            return View();
        }

        #endregion

        #region 任务

        /// <summary>
        /// 任务项
        /// </summary>
        public enum TaskItem
        {
            /// <summary>
            /// 备份
            /// </summary>
            Backup,
            /// <summary>
            /// 代码片段同步
            /// </summary>
            GistSync,
            /// <summary>
            /// 链接替换
            /// </summary>
            ReplaceLink,
            /// <summary>
            /// 处理操作记录
            /// </summary>
            HOR
        }

        /// <summary>
        /// 需要处理的事情
        /// </summary>
        /// <param name="ti"></param>
        /// <returns></returns>
        [ResponseCache(Duration = 60)]
        [FilterConfigs.LocalAuth]
        public ActionResultVM ExecTask(TaskItem? ti)
        {
            var vm = new ActionResultVM();

            try
            {
                if (!ti.HasValue)
                {
                    ti = (TaskItem)Enum.Parse(typeof(TaskItem), RouteData.Values["id"]?.ToString(), true);
                }

                switch (ti)
                {
                    default:
                        vm.Set(ARTag.invalid);
                        break;

                    case TaskItem.Backup:
                        vm = Func.TaskAid.BackupDataBase();
                        break;

                    case TaskItem.GistSync:
                        vm = Func.TaskAid.GistSync();
                        break;

                    case TaskItem.ReplaceLink:
                        vm = Func.TaskAid.ReplaceLink();
                        break;

                    case TaskItem.HOR:
                        vm = Func.TaskAid.HandleOperationRecord();
                        break;
                }
            }
            catch (Exception ex)
            {
                vm.Set(ex);
            }

            return vm;
        }
        #endregion
    }
}