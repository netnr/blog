using FluentScheduler;
using Microsoft.EntityFrameworkCore;
using Netnr.Data;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;

namespace Netnr.Func
{
    /// <summary>
    /// 定时任务
    /// </summary>
    public class TaskAid
    {
        /// <summary>
        /// 任务组件
        /// </summary>
        public class TaskComponent
        {
            /// <summary>
            /// 任务注册
            /// </summary>
            public class Reg : Registry
            {
                /// <summary>
                /// 构造
                /// </summary>
                public Reg()
                {
                    Schedule<BackupDataBaseJob>().ToRunEvery(2).Days().At(5, 5);

                    Schedule<GistSyncJob>().ToRunEvery(2).Hours();

                    Schedule<HandleOperationRecordJob>().ToRunEvery(30).Minutes();
                }
            }

            /// <summary>
            /// 数据库备份任务
            /// </summary>
            public class BackupDataBaseJob : IJob
            {
                void IJob.Execute()
                {
                    Core.ConsoleTo.Log(BackupDataBase().ToJson());
                }
            }

            /// <summary>
            /// Gist同步任务
            /// </summary>
            public class GistSyncJob : IJob
            {

                void IJob.Execute()
                {
                    Core.ConsoleTo.Log(GistSync().ToJson());
                }
            }

            /// <summary>
            /// 处理操作记录
            /// </summary>
            public class HandleOperationRecordJob : IJob
            {

                void IJob.Execute()
                {
                    Core.ConsoleTo.Log(HandleOperationRecord().ToJson());
                }
            }
        }

        /// <summary>
        /// 备份数据库
        /// </summary>
        public static ActionResultVM BackupDataBase()
        {
            var vm = new ActionResultVM();

            try
            {
                var listMsg = new List<object>();

                var kp = "Work:BackupDataBase:SQLServer:";

                if (GlobalTo.GetValue<bool>(kp + "enable") == true)
                {
                    //执行命令
                    using var db = new ContextBase();
                    using var conn = db.Database.GetDbConnection();
                    conn.Open();
                    var connCmd = conn.CreateCommand();
                    connCmd.CommandText = GlobalTo.GetValue(kp + "cmd");
                    int en = connCmd.ExecuteNonQuery();

                    listMsg.Add(en);

                    vm.Set(ARTag.success);
                    vm.data = listMsg;
                }
                else
                {
                    vm.Set(ARTag.lack);
                }
            }
            catch (Exception ex)
            {
                vm.Set(ex);
            }

            return vm;
        }

        /// <summary>
        /// Gist代码片段，同步到GitHub、Gitee
        /// </summary>
        /// <param name="UserId"></param>
        /// <returns></returns>
        public static ActionResultVM GistSync(int UserId = 1)
        {
            var vm = new ActionResultVM();

            try
            {
                //日志
                var listLog = new List<object>();

                using var db = new ContextBase();
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

                        //发送主体
                        #region MyRegion
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
                                        var hwr = Core.HttpTo.HWRequest("https://api.github.com/gists", "POST", jo.ToJson());
                                        hwr.Headers.Add(HttpRequestHeader.Authorization, "token " + token_gh);
                                        hwr.ContentType = "application/json";
                                        hwr.UserAgent = GlobalTo.GetValue("UserAgent");

                                        var rt = Core.HttpTo.Url(hwr);

                                        gsmo.GsGitHubId = rt.ToJObject()["id"].ToString();
                                        gsmo.GsGitHubTime = gist.GistUpdateTime;
                                    }

                                    //Gitee
                                    {
                                        var hwr = Core.HttpTo.HWRequest("https://gitee.com/api/v5/gists", "POST", jo.ToJson());
                                        hwr.ContentType = "application/json";

                                        var rt = Core.HttpTo.Url(hwr);

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
                                        var hwr = Core.HttpTo.HWRequest("https://api.github.com/gists/" + gs.GsGitHubId, "PATCH", jo.ToJson());
                                        hwr.Headers.Add(HttpRequestHeader.Authorization, "token " + token_gh);
                                        hwr.ContentType = "application/json";
                                        hwr.UserAgent = GlobalTo.GetValue("UserAgent");

                                        _ = Core.HttpTo.Url(hwr);

                                        gs.GsGitHubTime = gist.GistUpdateTime;
                                    }

                                    //Gitee
                                    {
                                        var hwr = Core.HttpTo.HWRequest("https://gitee.com/api/v5/gists/" + gs.GsGiteeId, "PATCH", jo.ToJson());
                                        hwr.ContentType = "application/json";

                                        _ = Core.HttpTo.Url(hwr);

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
                        var hwr_gh = Core.HttpTo.HWRequest("https://api.github.com/gists/" + gs.GsGitHubId, "DELETE");
                        hwr_gh.Headers.Add(HttpRequestHeader.Authorization, "token " + token_gh);
                        hwr_gh.UserAgent = GlobalTo.GetValue("UserAgent");
                        var resp_gh = (HttpWebResponse)hwr_gh.GetResponse();
                        if (resp_gh.StatusCode == HttpStatusCode.NoContent)
                        {
                            dc[0] = '1';
                        }
                        #endregion

                        #region Gitee
                        var hwr_ge = Core.HttpTo.HWRequest("https://gitee.com/api/v5/gists/" + gs.GsGiteeId + "?access_token=" + token_ge, "DELETE");
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

                vm.Set(ARTag.success);
                vm.data = listLog;
            }
            catch (Exception ex)
            {
                vm.Set(ex);
                Console.WriteLine(ex);
                if (ex.InnerException != null)
                {
                    Console.WriteLine(ex.InnerException.Message);
                }
                Core.ConsoleTo.Log(ex, true);
            }

            return vm;
        }

        /// <summary>
        /// 替换链接
        /// </summary>
        /// <returns></returns>
        public static ActionResultVM ReplaceLink()
        {
            var vm = new ActionResultVM();

            try
            {
                var linka = "https://gs.zme.ink/";
                var linkb = "https://static.netnr.com/";

                using var db = new ContextBase();
                var list1 = db.UserWriting.Where(x => x.UwContent.Contains(linka) || x.UwContentMd.Contains(linka)).ToList();
                var list2 = db.UserReply.Where(x => x.UrContent.Contains(linka) || x.UrContentMd.Contains(linka)).ToList();
                var needo = false;
                foreach (var item in list1)
                {
                    item.UwContent = item.UwContent.Replace(linka, linkb);
                    item.UwContentMd = item.UwContentMd.Replace(linka, linkb);
                }
                foreach (var item in list2)
                {
                    item.UrContent = item.UrContent.Replace(linka, linkb);
                    item.UrContentMd = item.UrContentMd.Replace(linka, linkb);
                }
                if (list1.Count > 0)
                {
                    db.UserWriting.UpdateRange(list1);
                    needo = true;
                }
                if (list2.Count > 0)
                {
                    db.UserReply.UpdateRange(list2);
                    needo = true;
                }

                int num = 0;
                if (needo)
                {
                    num = db.SaveChanges();
                }

                vm.Set(ARTag.success);
                vm.data = "受影响行数：" + num;
            }
            catch (Exception ex)
            {
                vm.Set(ex);
            }

            return vm;
        }

        /// <summary>
        /// 处理操作记录
        /// </summary>
        /// <returns></returns>
        public static ActionResultVM HandleOperationRecord()
        {
            var vm = new ActionResultVM();

            try
            {

                using var db = new ContextBase();

                //处理Guff查询记录数
                var ctype = EnumAid.ConnectionType.GuffRecord.ToString();
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
                    vm.data = "受影响行数：" + num;
                }
                else
                {
                    vm.Set(ARTag.lack);
                }
            }
            catch (Exception ex)
            {
                vm.Set(ex);
            }

            return vm;
        }
    }
}
