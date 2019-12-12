using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Netnr.Data;
using Netnr.Func.ViewModel;
using Newtonsoft.Json.Linq;

namespace Netnr.Web.Controllers
{
    /// <summary>
    /// 尬服 guff.ltd 提供所有接口支持
    /// </summary>
    [Route("api/v1/[controller]/[action]")]
    [Filters.FilterConfigs.AllowCors]
    public partial class GuffController : ControllerBase
    {
        /// <summary>
        /// 列表
        /// </summary>
        /// <param name="category">类别，可选，支持 text、image、audio、video、me（我的）、melaud（我点赞的）、mereply（我回复的）</param>
        /// <param name="q">搜索</param>
        /// <param name="uid">用户ID</param>
        /// <param name="nv">分类名/分类值</param>
        /// <param name="tag">标签</param>
        /// <param name="obj">对象</param>
        /// <param name="page">页码</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResultVM List(string category, string q, int uid, string nv, string tag, string obj, int page = 1)
        {
            var vm = new ActionResultVM();

            try
            {
                //所属用户
                var OwnerId = 0;

                if (uid != 0)
                {
                    OwnerId = uid;
                }

                var uinfo = new Func.UserAuthAid(HttpContext).Get();

                if (new List<string> { "me", "melaud", "mereply" }.Contains(category))
                {
                    if (uinfo.UserId == 0)
                    {
                        vm.Set(ARTag.unauthorized);
                    }
                    else
                    {
                        if (category == "me")
                        {
                            OwnerId = uinfo.UserId;
                        }

                        var pvm = Func.Common.GuffQuery(category, q, nv, tag, obj, OwnerId, uinfo.UserId, page);
                        vm.data = pvm;

                        vm.Set(ARTag.success);
                    }
                }
                else
                {
                    var pvm = Func.Common.GuffQuery(category, q, nv, tag, obj, OwnerId, uinfo.UserId, page);
                    vm.data = pvm;

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

        /// <summary>
        /// 查询一条
        /// </summary>
        /// <param name="id">ID</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResultVM Detail(string id)
        {
            var vm = new ActionResultVM();

            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    vm.Set(ARTag.invalid);
                }
                else
                {
                    var ctype = Func.EnumAid.ConnectionType.GuffRecord.ToString();

                    var uinfo = new Func.UserAuthAid(HttpContext).Get();

                    using var db = new ContextBase();
                    var query = from a in db.GuffRecord
                                join b in db.UserInfo on a.Uid equals b.UserId
                                join c in db.UserConnection.Where(x => x.UconnTargetType == ctype && x.UconnAction == 1 && x.Uid == uinfo.UserId) on a.GrId equals c.UconnTargetId into cg
                                from c1 in cg.DefaultIfEmpty()
                                where a.GrId == id
                                select new
                                {
                                    a,
                                    c1.UconnTargetId,
                                    b.Nickname
                                };
                    var qm = query.FirstOrDefault();
                    if (qm == null)
                    {
                        vm.Set(ARTag.invalid);
                    }
                    else
                    {
                        if (qm.a.GrOpen == 1 || uinfo.UserId == qm.a.Uid)
                        {
                            // 阅读 +1
                            qm.a.GrReadNum += 1;
                            db.Update(qm.a);
                            db.SaveChanges();

                            qm.a.Spare1 = string.IsNullOrEmpty(qm.UconnTargetId) ? "" : "laud";
                            qm.a.Spare2 = (uinfo.UserId == qm.a.Uid) ? "owner" : "";
                            qm.a.Spare3 = qm.Nickname;

                            vm.data = qm.a;

                            vm.Set(ARTag.success);
                        }
                        else
                        {
                            vm.Set(ARTag.unauthorized);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                vm.Set(ex);
                Core.ConsoleTo.Log(ex);
            }

            return vm;
        }

        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="mo"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResultVM Add(Domain.GuffRecord mo)
        {
            var vm = new ActionResultVM();

            try
            {
                var uinfo = new Func.UserAuthAid(HttpContext).Get();

                if (string.IsNullOrWhiteSpace(mo.GrContent) && string.IsNullOrWhiteSpace(mo.GrImage) && string.IsNullOrWhiteSpace(mo.GrAudio) && string.IsNullOrWhiteSpace(mo.GrVideo))
                {
                    vm.code = 1;
                    vm.msg = "内容不能为空（内容、图片、音频、视频 至少有一项有内容）";
                }
                else if (string.IsNullOrWhiteSpace(mo.GrTag))
                {
                    vm.code = 2;
                    vm.msg = "标签不能为空";
                }
                else if (uinfo.UserId == 0)
                {
                    vm.Set(ARTag.unauthorized);
                }
                else
                {
                    using var db = new ContextBase();

                    if (db.UserInfo.Find(uinfo.UserId).UserMailValid != 1)
                    {
                        vm.code = 1;
                        vm.msg = "请先验证邮箱";
                    }
                    else
                    {
                        var now = DateTime.Now;

                        mo.Uid = uinfo.UserId;
                        mo.GrId = Core.UniqueTo.LongId().ToString();
                        mo.GrCreateTime = now;
                        mo.GrUpdateTime = now;
                        mo.GrStatus = 1;
                        mo.GrReadNum = 0;
                        mo.GrLaud = 0;
                        mo.GrMark = 0;
                        mo.GrReplyNum = 0;
                        mo.GrOpen ??= 1;

                        mo.GrTypeName = Fast.ParsingTo.JsSafeJoin(mo.GrTypeName);
                        mo.GrTypeValue = Fast.ParsingTo.JsSafeJoin(mo.GrTypeValue);
                        mo.GrObject = Fast.ParsingTo.JsSafeJoin(mo.GrObject);
                        mo.GrImage = Fast.ParsingTo.JsSafeJoin(mo.GrImage);
                        mo.GrAudio = Fast.ParsingTo.JsSafeJoin(mo.GrAudio);
                        mo.GrVideo = Fast.ParsingTo.JsSafeJoin(mo.GrVideo);
                        mo.GrFile = Fast.ParsingTo.JsSafeJoin(mo.GrFile);
                        mo.GrTag = Fast.ParsingTo.JsSafeJoin(mo.GrTag);

                        db.GuffRecord.Add(mo);

                        int num = db.SaveChanges();

                        vm.data = mo.GrId;
                        vm.Set(num > 0);
                    }
                }
            }
            catch (Exception ex)
            {
                vm.Set(ex);
                Core.ConsoleTo.Log(ex);
            }

            return vm;
        }

        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="mo"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResultVM Update(Domain.GuffRecord mo)
        {
            var vm = new ActionResultVM();

            try
            {
                var uinfo = new Func.UserAuthAid(HttpContext).Get();

                if (string.IsNullOrWhiteSpace(mo.GrContent) && string.IsNullOrWhiteSpace(mo.GrImage) && string.IsNullOrWhiteSpace(mo.GrAudio) && string.IsNullOrWhiteSpace(mo.GrVideo))
                {
                    vm.code = 1;
                    vm.msg = "内容不能为空（内容、图片、音频、视频 至少有一项有内容）";
                }
                else if (string.IsNullOrWhiteSpace(mo.GrTag))
                {
                    vm.code = 2;
                    vm.msg = "标签不能为空";
                }
                else if (uinfo.UserId == 0)
                {
                    vm.Set(ARTag.unauthorized);
                }
                else
                {
                    using var db = new ContextBase();
                    var currMo = db.GuffRecord.Find(mo.GrId);

                    if (currMo == null)
                    {
                        vm.Set(ARTag.invalid);
                    }
                    else
                    {
                        currMo.GrTypeName = Fast.ParsingTo.JsSafeJoin(mo.GrTypeName);
                        currMo.GrTypeValue = Fast.ParsingTo.JsSafeJoin(mo.GrTypeValue);
                        currMo.GrObject = Fast.ParsingTo.JsSafeJoin(mo.GrObject);

                        currMo.GrContent = mo.GrContent;
                        currMo.GrContentMd = mo.GrContentMd;

                        currMo.GrImage = Fast.ParsingTo.JsSafeJoin(mo.GrImage);
                        currMo.GrAudio = Fast.ParsingTo.JsSafeJoin(mo.GrAudio);
                        currMo.GrVideo = Fast.ParsingTo.JsSafeJoin(mo.GrVideo);
                        currMo.GrFile = Fast.ParsingTo.JsSafeJoin(mo.GrFile);
                        currMo.GrRemark = mo.GrRemark;

                        currMo.GrTag = mo.GrTag;
                        currMo.GrUpdateTime = DateTime.Now;
                        currMo.GrOpen = mo.GrOpen ?? 1;

                        db.Update(currMo);

                        int num = db.SaveChanges();

                        vm.data = mo.GrId;
                        vm.Set(num > 0);
                    }
                }
            }
            catch (Exception ex)
            {
                vm.Set(ex);
                Core.ConsoleTo.Log(ex);
            }

            return vm;
        }

        /// <summary>
        /// 连接：点赞、收藏 
        /// </summary>
        /// <param name="type">add添加，cancel取消</param>
        /// <param name="ac">1点赞，2收藏</param>
        /// <param name="id">ID</param>
        /// <returns></returns>
        [HttpGet]
        public ActionResultVM Connection(string type, int ac, string id)
        {
            var vm = new ActionResultVM();

            try
            {
                var uinfo = new Func.UserAuthAid(HttpContext).Get();

                if (uinfo.UserId == 0)
                {
                    vm.Set(ARTag.unauthorized);
                }
                else if (string.IsNullOrWhiteSpace(id))
                {
                    vm.Set(ARTag.invalid);
                }
                else if (!new List<string> { "add", "cancel" }.Contains(type))
                {
                    vm.Set(ARTag.invalid);
                }
                else if (!new List<int> { 1, 2 }.Contains(ac))
                {
                    vm.Set(ARTag.invalid);
                }
                else
                {
                    using var db = new ContextBase();
                    var currMo = db.GuffRecord.Find(id);

                    if (currMo == null)
                    {
                        vm.Set(ARTag.invalid);
                    }
                    else
                    {
                        var ctype = Func.EnumAid.ConnectionType.GuffRecord.ToString();
                        switch (type)
                        {
                            case "add":
                                {
                                    if (db.UserConnection.Any(x => x.Uid == uinfo.UserId && x.UconnTargetType == ctype && x.UconnTargetId == id && x.UconnAction == ac))
                                    {
                                        vm.Set(ARTag.exist);
                                    }
                                    else
                                    {
                                        //关联记录
                                        var ucmo = new Domain.UserConnection()
                                        {
                                            UconnId = Core.UniqueTo.LongId().ToString(),
                                            Uid = uinfo.UserId,
                                            UconnTargetType = Func.EnumAid.ConnectionType.GuffRecord.ToString(),
                                            UconnTargetId = id,
                                            UconnAction = ac,
                                            UconnCreateTime = DateTime.Now
                                        };

                                        db.Add(ucmo);

                                        switch (ac)
                                        {
                                            case 1:
                                                currMo.GrLaud += 1;
                                                break;
                                            case 2:
                                                currMo.GrMark += 1;
                                                break;
                                        }
                                        db.Update(currMo);

                                        int num = db.SaveChanges();

                                        vm.Set(num > 0);
                                    }
                                }
                                break;

                            case "cancel":
                                {
                                    var curruc = db.UserConnection.FirstOrDefault(x => x.Uid == uinfo.UserId && x.UconnTargetType == ctype && x.UconnTargetId == id && x.UconnAction == ac);
                                    if (curruc == null)
                                    {
                                        vm.Set(ARTag.invalid);
                                    }
                                    else
                                    {
                                        db.Remove(curruc);

                                        switch (ac)
                                        {
                                            case 1:
                                                currMo.GrLaud -= 1;
                                                break;
                                            case 2:
                                                currMo.GrMark -= 1;
                                                break;
                                        }
                                        db.Update(currMo);

                                        int num = db.SaveChanges();

                                        vm.Set(num > 0);
                                    }
                                }
                                break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                vm.Set(ex);
                Core.ConsoleTo.Log(ex);
            }

            return vm;
        }

        /// <summary>
        /// 添加回复
        /// </summary>
        /// <param name="mo">内容，仅限内容字段必填，支持匿名回复</param>
        /// <param name="id">ID</param>
        /// <returns></returns>
        [HttpPost]
        public ActionResultVM ReplyAdd(Domain.UserReply mo, string id)
        {
            var vm = new ActionResultVM();

            try
            {
                var uinfo = new Func.UserAuthAid(HttpContext).Get();

                if (HttpContext.User.Identity.IsAuthenticated)
                {
                    mo.Uid = uinfo.UserId;
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(mo.UrAnonymousName) || !Fast.ParsingTo.IsMail(mo.UrAnonymousMail))
                    {
                        vm.Set(ARTag.invalid);
                        vm.msg = "昵称、邮箱不能为空";

                        return vm;
                    }

                    mo.Uid = 0;
                }

                if (string.IsNullOrWhiteSpace(id))
                {
                    vm.Set(ARTag.invalid);
                }
                else if (string.IsNullOrWhiteSpace(mo.UrContent))
                {
                    vm.Set(ARTag.invalid);
                    vm.msg = "回复内容不能为空";
                }
                else
                {
                    using var db = new ContextBase();
                    var guffmo = db.GuffRecord.Find(id);
                    if (guffmo == null)
                    {
                        vm.Set(ARTag.invalid);
                    }
                    else
                    {
                        mo.Uid = uinfo.UserId;
                        mo.UrTargetType = Func.EnumAid.ConnectionType.GuffRecord.ToString();
                        mo.UrTargetId = id;
                        mo.UrCreateTime = DateTime.Now;
                        mo.UrStatus = 1;
                        mo.UrTargetPid = 0;

                        mo.UrAnonymousLink = Fast.ParsingTo.JsSafeJoin(mo.UrAnonymousLink);

                        db.UserReply.Add(mo);

                        guffmo.GrReplyNum += 1;
                        db.GuffRecord.Update(guffmo);

                        int num = db.SaveChanges();
                        vm.Set(num > 0);
                    }
                }
            }
            catch (Exception ex)
            {
                vm.Set(ex);
                Core.ConsoleTo.Log(ex);
            }

            return vm;
        }

        /// <summary>
        /// 回复列表
        /// </summary>
        /// <param name="id"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResultVM ReplyList(string id, int page = 1)
        {
            var vm = new ActionResultVM();

            try
            {
                var uinfo = new Func.UserAuthAid(HttpContext).Get();

                var pag = new PaginationVM
                {
                    PageNumber = Math.Max(page, 1),
                    PageSize = 10
                };

                var list = Func.Common.ReplyOneQuery(Func.EnumAid.ReplyType.GuffRecord, id, pag);
                //匿名用户，生成邮箱MD5加密用于请求头像
                foreach (var item in list)
                {
                    if (item.Uid == 0 && !string.IsNullOrWhiteSpace(item.UrAnonymousMail))
                    {
                        item.Spare3 = Core.CalcTo.MD5(item.UrAnonymousMail);
                    }
                }

                var pvm = new PageVM()
                {
                    Rows = list,
                    Pag = pag
                };
                vm.data = pvm;

                vm.Set(ARTag.success);
            }
            catch (Exception ex)
            {
                vm.Set(ex);
                Core.ConsoleTo.Log(ex);
            }

            return vm;
        }

        /// <summary>
        /// 删除一条
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public ActionResultVM Delete(string id)
        {
            var vm = new ActionResultVM();

            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    vm.Set(ARTag.invalid);
                }
                else
                {
                    var uinfo = new Func.UserAuthAid(HttpContext).Get();
                    if (uinfo.UserId != 0)
                    {
                        using var db = new ContextBase();
                        var mo = db.GuffRecord.Find(id);

                        if (mo == null)
                        {
                            vm.Set(ARTag.invalid);
                        }
                        else
                        {
                            if (mo.Uid != uinfo.UserId)
                            {
                                vm.Set(ARTag.unauthorized);
                            }
                            else if (mo.GrStatus == -1)
                            {
                                vm.Set(ARTag.refuse);
                            }
                            else
                            {
                                db.Remove(mo);
                                int num = db.SaveChanges();

                                vm.Set(num > 0);
                            }
                        }
                    }
                    else
                    {
                        vm.Set(ARTag.unauthorized);
                    }
                }
            }
            catch (Exception ex)
            {
                vm.Set(ex);
                Core.ConsoleTo.Log(ex);
            }

            return vm;
        }

        /// <summary>
        /// 热门标签
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ResponseCache(Duration = 60)]
        public ActionResultVM HotTag()
        {
            var vm = new ActionResultVM();

            try
            {
                using var db = new ContextBase();
                var listTags = db.GuffRecord.OrderByDescending(x => x.GrCreateTime).Select(x => x.GrTag).Take(1000).ToList();
                var orderTags = string.Join(",", listTags).Split(',').GroupBy(x => x).OrderByDescending(x => x.Count()).Select(x => x.Key).Take(20).ToList();

                vm.data = orderTags;
                vm.Set(ARTag.success);
            }
            catch (Exception ex)
            {
                vm.Set(ex);
                Core.ConsoleTo.Log(ex);
            }

            return vm;
        }
    }
}