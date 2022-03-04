using Netnr.Blog.Data;
using Netnr.Core;
using Netnr.SharedFast;

namespace Netnr.Blog.Web.Controllers.api
{
    /// <summary>
    /// 尬服 guff.ltd 提供所有接口支持
    /// </summary>
    [Route("api/v1/guff/[action]")]
    [Apps.FilterConfigs.AllowCors]
    public class GuffController : ControllerBase
    {
        public ContextBase db;

        public GuffController(ContextBase cb)
        {
            db = cb;
        }

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
        public SharedResultVM List(string category, string q, int uid, string nv, string tag, string obj, int page = 1)
        {
            return SharedResultVM.Try(vm =>
            {
                //所属用户
                var OwnerId = 0;

                if (uid != 0)
                {
                    OwnerId = uid;
                }

                var uinfo = Apps.LoginService.Get(HttpContext);

                if (new List<string> { "me", "melaud", "mereply" }.Contains(category))
                {
                    if (uinfo.UserId == 0)
                    {
                        vm.Set(SharedEnum.RTag.unauthorized);
                    }
                    else
                    {
                        if (category == "me")
                        {
                            OwnerId = uinfo.UserId;
                        }

                        var pvm = Application.CommonService.GuffQuery(category, q, nv, tag, obj, OwnerId, uinfo.UserId, page);
                        vm.Data = pvm;

                        vm.Set(SharedEnum.RTag.success);
                    }
                }
                else
                {
                    var pvm = Application.CommonService.GuffQuery(category, q, nv, tag, obj, OwnerId, uinfo.UserId, page);
                    vm.Data = pvm;

                    vm.Set(SharedEnum.RTag.success);
                }

                return vm;
            });
        }

        /// <summary>
        /// 查询一条
        /// </summary>
        /// <param name="id">ID</param>
        /// <returns></returns>
        [HttpGet]
        public SharedResultVM Detail(string id)
        {
            return SharedResultVM.Try(vm =>
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    vm.Set(SharedEnum.RTag.invalid);
                }
                else
                {
                    var ctype = Application.EnumService.ConnectionType.GuffRecord.ToString();

                    var uinfo = Apps.LoginService.Get(HttpContext);

                    var query = from a in db.GuffRecord
                                join b in db.UserInfo on a.Uid equals b.UserId
                                join c in db.UserConnection.Where(x => x.UconnTargetType == ctype && x.UconnAction == 1 && x.Uid == uinfo.UserId) on a.GrId equals c.UconnTargetId into cg
                                from c1 in cg.DefaultIfEmpty()
                                where a.GrId == id
                                select new
                                {
                                    a,
                                    UconnTargetId = c1 == null ? null : c1.UconnTargetId,
                                    b.Nickname
                                };
                    var qm = query.FirstOrDefault();
                    if (qm == null)
                    {
                        vm.Set(SharedEnum.RTag.invalid);
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

                            vm.Data = qm.a;

                            vm.Set(SharedEnum.RTag.success);
                        }
                        else
                        {
                            vm.Set(SharedEnum.RTag.unauthorized);
                        }
                    }
                }

                return vm;
            });
        }

        /// <summary>
        /// 新增
        /// </summary>
        /// <param name="mo"></param>
        /// <returns></returns>
        [HttpPost]
        public SharedResultVM Add([FromForm] Domain.GuffRecord mo)
        {
            var vm = new SharedResultVM();

            try
            {
                var uinfo = Apps.LoginService.Get(HttpContext);

                if (string.IsNullOrWhiteSpace(mo.GrContent) && string.IsNullOrWhiteSpace(mo.GrImage) && string.IsNullOrWhiteSpace(mo.GrAudio) && string.IsNullOrWhiteSpace(mo.GrVideo))
                {
                    vm.Code = 1;
                    vm.Msg = "内容不能为空（内容、图片、音频、视频 至少有一项有内容）";
                }
                else if (string.IsNullOrWhiteSpace(mo.GrTag))
                {
                    vm.Code = 2;
                    vm.Msg = "标签不能为空";
                }
                else if (uinfo.UserId == 0)
                {
                    vm.Set(SharedEnum.RTag.unauthorized);
                }
                else
                {
                    vm = Apps.LoginService.CompleteInfoValid(HttpContext);
                    if (vm.Code == 200)
                    {
                        var now = DateTime.Now;

                        mo.Uid = uinfo.UserId;
                        mo.GrId = UniqueTo.LongId().ToString();
                        mo.GrCreateTime = now;
                        mo.GrUpdateTime = now;
                        mo.GrStatus = 1;
                        mo.GrReadNum = 0;
                        mo.GrLaud = 0;
                        mo.GrMark = 0;
                        mo.GrReplyNum = 0;
                        mo.GrOpen ??= 1;

                        mo.GrTypeName = ParsingTo.JsSafeJoin(mo.GrTypeName);
                        mo.GrTypeValue = ParsingTo.JsSafeJoin(mo.GrTypeValue);
                        mo.GrObject = ParsingTo.JsSafeJoin(mo.GrObject);
                        mo.GrImage = ParsingTo.JsSafeJoin(mo.GrImage);
                        mo.GrAudio = ParsingTo.JsSafeJoin(mo.GrAudio);
                        mo.GrVideo = ParsingTo.JsSafeJoin(mo.GrVideo);
                        mo.GrFile = ParsingTo.JsSafeJoin(mo.GrFile);
                        mo.GrTag = ParsingTo.JsSafeJoin(mo.GrTag);

                        db.GuffRecord.Add(mo);

                        int num = db.SaveChanges();

                        vm.Data = mo.GrId;
                        vm.Set(num > 0);
                    }
                }
            }
            catch (Exception ex)
            {
                vm.Set(ex);
                Apps.FilterConfigs.WriteLog(HttpContext, ex);
            }

            return vm;
        }

        /// <summary>
        /// 修改
        /// </summary>
        /// <param name="mo"></param>
        /// <returns></returns>
        [HttpPost]
        public SharedResultVM Update([FromForm] Domain.GuffRecord mo)
        {
            var vm = new SharedResultVM();

            try
            {
                var uinfo = Apps.LoginService.Get(HttpContext);

                if (string.IsNullOrWhiteSpace(mo.GrContent) && string.IsNullOrWhiteSpace(mo.GrImage) && string.IsNullOrWhiteSpace(mo.GrAudio) && string.IsNullOrWhiteSpace(mo.GrVideo))
                {
                    vm.Code = 1;
                    vm.Msg = "内容不能为空（内容、图片、音频、视频 至少有一项有内容）";
                }
                else if (string.IsNullOrWhiteSpace(mo.GrTag))
                {
                    vm.Code = 2;
                    vm.Msg = "标签不能为空";
                }
                else if (uinfo.UserId == 0)
                {
                    vm.Set(SharedEnum.RTag.unauthorized);
                }
                else
                {
                    var currMo = db.GuffRecord.Find(mo.GrId);

                    if (currMo == null)
                    {
                        vm.Set(SharedEnum.RTag.invalid);
                    }
                    else if (currMo.Uid != uinfo.UserId)
                    {
                        vm.Set(SharedEnum.RTag.unauthorized);
                    }
                    else
                    {
                        currMo.GrTypeName = ParsingTo.JsSafeJoin(mo.GrTypeName);
                        currMo.GrTypeValue = ParsingTo.JsSafeJoin(mo.GrTypeValue);
                        currMo.GrObject = ParsingTo.JsSafeJoin(mo.GrObject);

                        currMo.GrContent = mo.GrContent;
                        currMo.GrContentMd = mo.GrContentMd;

                        currMo.GrImage = ParsingTo.JsSafeJoin(mo.GrImage);
                        currMo.GrAudio = ParsingTo.JsSafeJoin(mo.GrAudio);
                        currMo.GrVideo = ParsingTo.JsSafeJoin(mo.GrVideo);
                        currMo.GrFile = ParsingTo.JsSafeJoin(mo.GrFile);
                        currMo.GrRemark = mo.GrRemark;

                        currMo.GrTag = mo.GrTag;
                        currMo.GrUpdateTime = DateTime.Now;
                        currMo.GrOpen = mo.GrOpen ?? 1;

                        db.Update(currMo);

                        int num = db.SaveChanges();

                        vm.Data = mo.GrId;
                        vm.Set(num > 0);
                    }
                }
            }
            catch (Exception ex)
            {
                vm.Set(ex);
                Apps.FilterConfigs.WriteLog(HttpContext, ex);
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
        public SharedResultVM Connection(string type, int ac, string id)
        {
            return SharedResultVM.Try(vm =>
            {
                var uinfo = Apps.LoginService.Get(HttpContext);

                if (uinfo.UserId == 0)
                {
                    vm.Set(SharedEnum.RTag.unauthorized);
                }
                else if (string.IsNullOrWhiteSpace(id))
                {
                    vm.Set(SharedEnum.RTag.invalid);
                }
                else if (!new List<string> { "add", "cancel" }.Contains(type))
                {
                    vm.Set(SharedEnum.RTag.invalid);
                }
                else if (!new List<int> { 1, 2 }.Contains(ac))
                {
                    vm.Set(SharedEnum.RTag.invalid);
                }
                else
                {
                    var currMo = db.GuffRecord.Find(id);

                    if (currMo == null)
                    {
                        vm.Set(SharedEnum.RTag.invalid);
                    }
                    else
                    {
                        var ctype = Application.EnumService.ConnectionType.GuffRecord.ToString();
                        switch (type)
                        {
                            case "add":
                                {
                                    if (db.UserConnection.Any(x => x.Uid == uinfo.UserId && x.UconnTargetType == ctype && x.UconnTargetId == id && x.UconnAction == ac))
                                    {
                                        vm.Set(SharedEnum.RTag.exist);
                                    }
                                    else
                                    {
                                        //关联记录
                                        var ucmo = new Domain.UserConnection()
                                        {
                                            UconnId = UniqueTo.LongId().ToString(),
                                            Uid = uinfo.UserId,
                                            UconnTargetType = Application.EnumService.ConnectionType.GuffRecord.ToString(),
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
                                        vm.Set(SharedEnum.RTag.invalid);
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

                return vm;
            });
        }

        /// <summary>
        /// 添加回复
        /// </summary>
        /// <param name="mo">内容，仅限内容字段必填，支持匿名回复</param>
        /// <param name="id">ID</param>
        /// <returns></returns>
        [HttpPost]
        public SharedResultVM ReplyAdd([FromForm] Domain.UserReply mo, [FromForm] string id)
        {
            var vm = new SharedResultVM();

            try
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    vm.Set(SharedEnum.RTag.invalid);
                }
                else if (string.IsNullOrWhiteSpace(mo.UrContent))
                {
                    vm.Set(SharedEnum.RTag.invalid);
                    vm.Msg = "回复内容不能为空";
                }
                else
                {
                    vm = Apps.LoginService.CompleteInfoValid(HttpContext);
                    if (vm.Code == 200)
                    {
                        var uinfo = Apps.LoginService.Get(HttpContext);

                        var guffmo = db.GuffRecord.Find(id);
                        if (guffmo == null)
                        {
                            vm.Set(SharedEnum.RTag.invalid);
                        }
                        else
                        {
                            mo.Uid = uinfo.UserId;
                            mo.UrTargetType = Application.EnumService.ConnectionType.GuffRecord.ToString();
                            mo.UrTargetId = id;
                            mo.UrCreateTime = DateTime.Now;
                            mo.UrStatus = 1;
                            mo.UrTargetPid = 0;

                            mo.UrAnonymousLink = ParsingTo.JsSafeJoin(mo.UrAnonymousLink);

                            db.UserReply.Add(mo);

                            guffmo.GrReplyNum += 1;
                            db.GuffRecord.Update(guffmo);

                            int num = db.SaveChanges();
                            vm.Set(num > 0);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                vm.Set(ex);
                Apps.FilterConfigs.WriteLog(HttpContext, ex);
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
        public SharedResultVM ReplyList(string id, int page = 1)
        {
            return SharedResultVM.Try(vm =>
            {
                var uinfo = Apps.LoginService.Get(HttpContext);

                var pag = new SharedPaginationVM
                {
                    PageNumber = Math.Max(page, 1),
                    PageSize = 10
                };

                var list = Application.CommonService.ReplyOneQuery(Application.EnumService.ReplyType.GuffRecord, id, pag);
                //匿名用户，生成邮箱MD5加密用于请求头像
                foreach (var item in list)
                {
                    if (item.Uid == 0 && !string.IsNullOrWhiteSpace(item.UrAnonymousMail))
                    {
                        item.Spare3 = CalcTo.MD5(item.UrAnonymousMail);
                    }
                }

                var pvm = new SharedPageVM()
                {
                    Rows = list,
                    Pag = pag
                };
                vm.Data = pvm;

                vm.Set(SharedEnum.RTag.success);

                return vm;
            });
        }

        /// <summary>
        /// 删除一条
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpGet]
        public SharedResultVM Delete(string id)
        {
            return SharedResultVM.Try(vm =>
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    vm.Set(SharedEnum.RTag.invalid);
                }
                else
                {
                    var uinfo = Apps.LoginService.Get(HttpContext);
                    if (uinfo.UserId != 0)
                    {
                        var mo = db.GuffRecord.Find(id);

                        if (mo == null)
                        {
                            vm.Set(SharedEnum.RTag.invalid);
                        }
                        else
                        {
                            if (mo.Uid != uinfo.UserId)
                            {
                                vm.Set(SharedEnum.RTag.unauthorized);
                            }
                            else if (mo.GrStatus == -1)
                            {
                                vm.Set(SharedEnum.RTag.refuse);
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
                        vm.Set(SharedEnum.RTag.unauthorized);
                    }
                }

                return vm;
            });
        }

        /// <summary>
        /// 热门标签
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [ResponseCache(Duration = 60)]
        public SharedResultVM HotTag()
        {
            return SharedResultVM.Try(vm =>
            {
                var listTags = db.GuffRecord.OrderByDescending(x => x.GrCreateTime).Select(x => x.GrTag).Take(1000).ToList();
                var orderTags = string.Join(",", listTags).Split(',').GroupBy(x => x).OrderByDescending(x => x.Count()).Select(x => x.Key).Take(20).ToList();

                vm.Data = orderTags;
                vm.Set(SharedEnum.RTag.success);

                return vm;
            });
        }
    }
}