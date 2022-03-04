using Microsoft.AspNetCore.Authorization;
using Netnr.Blog.Data;
using Netnr.Blog.Domain;
using Netnr.Blog.Application.ViewModel;
using Markdig;
using Netnr.SharedFast;

namespace Netnr.Blog.Web.Areas.Doc.Controllers
{
    [Area("Doc")]
    public class CodeController : Controller
    {
        public ContextBase db;

        public CodeController(ContextBase cb)
        {
            db = cb;
        }

        /// <summary>
        /// 目录页面
        /// </summary>
        /// <param name="code">分享码</param>
        /// <returns></returns>
        public IActionResult Index(string code)
        {
            //文档集编号
            var DsCode = RouteData.Values["id"]?.ToString();
            //页编码
            var DsdId = RouteData.Values["sid"]?.ToString() ?? "";

            if (string.IsNullOrWhiteSpace(DsCode))
            {
                return Redirect("/doc");
            }

            var sck = "SharedCode_" + DsCode;
            //有分享码
            if (!string.IsNullOrWhiteSpace(code))
            {
                Response.Cookies.Append(sck, code);
            }
            else
            {
                code = Request.Cookies[sck]?.ToString();
            }

            //跳转带斜杠
            if (DsCode.Length == 19 && DsdId == "" && !Request.Path.Value.EndsWith("/"))
            {
                return Redirect(Request.Path.Value + "/");
            }

            var baseUrl = string.Join("/", Request.Path.Value.Split('/').ToList().Take(4)) + "/";

            var ds = db.DocSet.Find(DsCode);
            if (ds == null)
            {
                return Content("bad");
            }

            //分享码
            var isShare = !string.IsNullOrWhiteSpace(ds.Spare1) && ds.Spare1 == code;

            if (!isShare && ds.DsOpen != 1)
            {
                if (HttpContext.User.Identity.IsAuthenticated)
                {
                    var uinfo = Apps.LoginService.Get(HttpContext);
                    if (uinfo.UserId != ds.Uid)
                    {
                        return Content("unauthorized");
                    }
                }
                else
                {
                    return Content("unauthorized");
                }
            }


            var ismd = DsdId.EndsWith(".md");
            var listSeo = new List<string>();

            if (DsdId.StartsWith("README") || string.IsNullOrWhiteSpace(DsdId))
            {
                var ct = new List<string>
                    {
                        "# " + ds.DsName,
                        string.IsNullOrWhiteSpace(ds.DsRemark)? "" : $"#### {ds.DsRemark}",
                        "",
                        ds.DsCreateTime.Value.ToString("yyyy-MM-dd HH:mm:ss")
                    };
                var ctmd = string.Join(Environment.NewLine, ct);
                listSeo.Add(Markdown.ToHtml(ctmd));

                if (ismd)
                {
                    return Content(ctmd);
                }
            }

            if (DsdId.StartsWith("_sidebar") || DsdId.Length == 19)
            {
                //文档集目录
                var DocTree = db.DocSetDetail
                    .Where(x => x.DsCode == DsCode)
                    .OrderBy(x => x.DsdOrder)
                    .Select(x => new DocTreeVM
                    {
                        DsdId = x.DsdId,
                        DsdPid = x.DsdPid,
                        DsCode = x.DsCode,
                        DsdTitle = x.DsdTitle,
                        DsdOrder = x.DsdOrder,
                        IsCatalog = string.IsNullOrEmpty(x.DsdContentMd)
                    }).ToList();

                var ct = TreeToMd(baseUrl, DocTree, Guid.Empty.ToString());
                //ct.RemoveRange(0, 10);
                var ctmd = string.Join(Environment.NewLine, ct);
                listSeo.Add(Markdown.ToHtml(ctmd).Replace(".md\"", "\""));

                if (ismd)
                {
                    return Content(ctmd);
                }
            }

            if (DsdId.Length == 19 || DsdId.Length == 22)
            {
                var mdid = DsdId.Replace(".md", "");

                var mdmo = db.DocSetDetail.FirstOrDefault(x => x.DsdId == mdid);

                var uptime = "<span title='" + mdmo.DsdCreateTime.Value.ToString("yyyy-MM-dd HH:mm:ss") + "'><i class='fa fa-clock-o'></i> 创建于：" + mdmo.DsdCreateTime.Value.ToString("yyyy年MM月dd日") + "</span>";
                if (mdmo.DsdUpdateTime != mdmo.DsdCreateTime)
                {
                    uptime += " &nbsp; <span title='" + mdmo.DsdUpdateTime.Value.ToString("yyyy-MM-dd HH:mm:ss") + "'><i class='fa fa-clock-o'></i> 更新于：" + mdmo.DsdUpdateTime.Value.ToString("yyyy年MM月dd日") + "</span>";
                }
                uptime += " &nbsp; <span><i class='fa fa-sort-amount-asc'></i> 排序号：" + mdmo.DsdOrder + "</span>";
                var mdtitle = string.Join(Environment.NewLine, new List<string>()
                    {
                        "<h1>"+mdmo.DsdTitle+"</h1>",
                        uptime,
                        "<hr/>",
                        "",
                        ""
                    });

                listSeo.Add(mdtitle + mdmo.DsdContentHtml);

                if (ismd)
                {
                    return Content(mdtitle + mdmo.DsdContentMd);
                }
            }

            //文档标题
            ViewData["Title"] = ds.DsName;
            ViewData["DocSeo"] = string.Join(Environment.NewLine, listSeo);

            return View();
        }

        /// <summary>
        /// 树转markdown
        /// </summary>
        /// <param name="baseUrl"></param>
        /// <param name="list"></param>
        /// <param name="pid"></param>
        /// <param name="listNo"></param>
        /// <returns></returns>
        private List<string> TreeToMd(string baseUrl, List<DocTreeVM> list, string pid, List<string> listNo = null)
        {
            var ct = new List<string>();

            listNo ??= new List<string>();

            var rdt = list.Where(x => x.DsdPid == pid).ToList();

            for (int i = 0; i < rdt.Count; i++)
            {
                //数据行
                var dr = rdt[i];
                var di = "";
                foreach (var item in listNo)
                {
                    di += "  ";
                }

                //序号
                var ino = string.Join(".", listNo);
                if (ino != "")
                {
                    ino += ".";
                }
                ino += (i + 1) + "、";

                if (dr.IsCatalog)
                {
                    ct.Add($"{di}- {ino}{dr.DsdTitle}");
                    ct.Add("");
                }
                else
                {
                    ct.Add($"{di}- [{ino}{dr.DsdTitle}]({baseUrl}{dr.DsdId}.md)");
                }

                var nrdt = list.Where(x => x.DsdPid == dr.DsdId).ToList();
                if (nrdt.Count > 0)
                {
                    listNo.Add((i + 1).ToString());
                    var sct = TreeToMd(baseUrl, list, dr.DsdId, listNo);
                    listNo.RemoveAt(listNo.Count - 1);
                    ct.AddRange(sct);
                }
            }

            return ct;
        }

        /// <summary>
        /// 新增、编辑一条
        /// </summary>
        /// <param name="dsdid">详情ID</param>
        /// <returns></returns>
        [Authorize]
        public IActionResult Edit(string dsdid)
        {
            var code = RouteData.Values["id"]?.ToString();

            var uinfo = Apps.LoginService.Get(HttpContext);

            var ds = db.DocSet.Find(code);
            if (ds?.Uid != uinfo.UserId)
            {
                return Content("unauthorized");
            }

            var mo = new DocSetDetail
            {
                DsCode = code
            };

            if (!string.IsNullOrWhiteSpace(dsdid))
            {
                mo = db.DocSetDetail.FirstOrDefault(x => x.DsdId == dsdid);
            }

            return View(mo);
        }

        /// <summary>
        /// 保存
        /// </summary>
        /// <param name="mo"></param>
        /// <returns></returns>
        [Authorize]
        public SharedResultVM Save(DocSetDetail mo)
        {
            var vm = Apps.LoginService.CompleteInfoValid(HttpContext);
            if (vm.Code == 200)
            {
                var uinfo = Apps.LoginService.Get(HttpContext);

                var ds = db.DocSet.Find(mo.DsCode);
                if (ds?.Uid != uinfo.UserId)
                {
                    vm.Set(SharedEnum.RTag.unauthorized);
                }
                else
                {
                    mo.DsdUpdateTime = DateTime.Now;
                    mo.Uid = uinfo.UserId;

                    if (string.IsNullOrWhiteSpace(mo.DsdPid))
                    {
                        mo.DsdPid = Guid.Empty.ToString();
                    }

                    if (!mo.DsdOrder.HasValue)
                    {
                        mo.DsdOrder = 99;
                    }

                    if (string.IsNullOrWhiteSpace(mo.DsdId))
                    {
                        mo.DsdId = Core.UniqueTo.LongId().ToString();
                        mo.DsdCreateTime = mo.DsdUpdateTime;

                        db.DocSetDetail.Add(mo);
                    }
                    else
                    {
                        //查询原创建时间
                        var currmo = db.DocSetDetail.FirstOrDefault(x => x.DsdId == mo.DsdId);
                        if (currmo != null)
                        {
                            currmo.DsdTitle = mo.DsdTitle;
                            currmo.DsdPid = mo.DsdPid;
                            currmo.DsdOrder = mo.DsdOrder;
                            currmo.DsdContentMd = mo.DsdContentMd;
                            currmo.DsdContentHtml = mo.DsdContentHtml;

                            db.DocSetDetail.Update(currmo);
                        }
                    }

                    int num = db.SaveChanges();
                    vm.Set(num > 0);
                    vm.Data = mo.DsdId;
                }
            }

            return vm;
        }

        /// <summary>
        /// 删除
        /// </summary>
        /// <param name="dsdid"></param>
        /// <returns></returns>
        [Authorize]
        public IActionResult Del(string dsdid)
        {
            var code = RouteData.Values["id"]?.ToString();

            var uinfo = Apps.LoginService.Get(HttpContext);

            if (!string.IsNullOrWhiteSpace(dsdid))
            {
                var ds = db.DocSet.Find(code);
                if (ds?.Uid != uinfo.UserId)
                {
                    return Content("unauthorized");
                }

                var mo = db.DocSetDetail.Find(dsdid);
                db.DocSetDetail.Remove(mo);

                db.SaveChanges();
            }

            return Redirect("/doc/code/" + code);
        }

        /// <summary>
        /// 目录
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public IActionResult Catalog()
        {
            var code = RouteData.Values["id"]?.ToString();

            var uinfo = Apps.LoginService.Get(HttpContext);

            var ds = db.DocSet.Find(code);
            if (ds?.Uid != uinfo.UserId)
            {
                return Content("unauthorized");
            }

            return View();
        }

        /// <summary>
        /// 保存目录
        /// </summary>
        /// <param name="mo"></param>
        /// <returns></returns>
        [Authorize]
        public SharedResultVM SaveCatalog(DocSetDetail mo)
        {
            var vm = Apps.LoginService.CompleteInfoValid(HttpContext);
            if (vm.Code == 200)
            {
                var uinfo = Apps.LoginService.Get(HttpContext);

                var ds = db.DocSet.Find(mo.DsCode);
                if (ds?.Uid != uinfo.UserId)
                {
                    vm.Set(SharedEnum.RTag.unauthorized);
                    return vm;
                }

                mo.DsdOrder ??= 99;
                mo.DsdUpdateTime = DateTime.Now;
                if (string.IsNullOrWhiteSpace(mo.DsdPid))
                {
                    mo.DsdPid = Guid.Empty.ToString();
                }

                if (string.IsNullOrWhiteSpace(mo.DsdId))
                {
                    mo.DsdId = Guid.NewGuid().ToString();
                    mo.DsdCreateTime = mo.DsdUpdateTime;
                    mo.Uid = uinfo.UserId;


                    db.DocSetDetail.Add(mo);
                }
                else
                {
                    var currmo = db.DocSetDetail.FirstOrDefault(x => x.DsdId == mo.DsdId);
                    currmo.DsdTitle = mo.DsdTitle;
                    currmo.DsdOrder = mo.DsdOrder;
                    currmo.DsdPid = mo.DsdPid;

                    db.DocSetDetail.Update(currmo);
                }
                int num = db.SaveChanges();
                vm.Set(num > 0);
            }

            return vm;
        }

        /// <summary>
        /// 删除目录
        /// </summary>
        /// <param name="code"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize]
        public SharedResultVM DelCatalog(string code, string id)
        {
            var vm = new SharedResultVM();

            var uinfo = Apps.LoginService.Get(HttpContext);

            var ds = db.DocSet.Find(code);
            if (ds?.Uid != uinfo.UserId)
            {
                vm.Set(SharedEnum.RTag.unauthorized);
                return vm;
            }

            var listdsd = db.DocSetDetail.Where(x => x.DsCode == code && string.IsNullOrEmpty(x.DsdContentMd)).ToList();
            var removelist = Core.TreeTo.FindToTree(listdsd, "DsdPid", "DsdId", new List<string> { id });
            removelist.Add(listdsd.FirstOrDefault(x => x.DsdId == id));
            db.DocSetDetail.RemoveRange(removelist);

            int num = db.SaveChanges();
            vm.Set(num > 0);

            return vm;
        }

        /// <summary>
        /// 导出
        /// </summary>
        public FileResult Export()
        {
            var code = RouteData.Values["id"]?.ToString();
            var list = db.DocSetDetail.Where(x => x.DsCode == code).OrderBy(x => x.DsdOrder).Select(x => new
            {
                x.DsdId,
                x.DsdPid,
                x.DsdTitle,
                x.DsdOrder,
                IsCatalog = string.IsNullOrEmpty(x.DsdContentMd),
                x.DsdContentHtml
            }).ToList();

            var htmlbody = ListTreeEach(list, "DsdPid", "DsdId", new List<string> { Guid.Empty.ToString() });

            //读取模版
            var tm = Core.FileTo.ReadText(GlobalTo.WebRootPath + "/template/htmltoword.html");
            tm = tm.Replace("@netnrmd@", htmlbody);

            //文件名
            var filename = db.DocSet.Where(x => x.DsCode == code).FirstOrDefault()?.DsName.Replace(" ", "") ?? "netnrdoc";

            return File(Encoding.Default.GetBytes(tm), "application/msword", filename + ".doc");
        }

        /// <summary>
        /// 生成Html
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="pidField"></param>
        /// <param name="idField"></param>
        /// <param name="startPid"></param>
        /// <param name="listNo"></param>
        /// <param name="deep"></param>
        /// <returns></returns>
        private string ListTreeEach<T>(List<T> list, string pidField, string idField, List<string> startPid, List<int> listNo = null, int deep = 1)
        {
            StringBuilder sbTree = new();

            var rdt = list.Where(x => startPid.Contains(x.GetType().GetProperty(pidField).GetValue(x, null).ToString())).ToList();

            for (int i = 0; i < rdt.Count; i++)
            {
                var dr = rdt[i];
                var drgt = dr.GetType();

                //H标签
                var hn = deep;
                if (deep > 6)
                {
                    hn = 6;
                }

                //序号
                if (listNo == null)
                {
                    listNo = new List<int> { i + 1 };
                }
                else
                {
                    listNo.Add(i + 1);
                }

                //标题
                var title = drgt.GetProperty("DsdTitle").GetValue(dr, null).ToString();
                sbTree.AppendLine($"<h{hn}>{string.Join(".", listNo)}、{title}</h{hn}>");

                //是目录
                var iscatalog = Convert.ToBoolean(drgt.GetProperty("IsCatalog").GetValue(dr, null));
                if (!iscatalog)
                {
                    sbTree.AppendLine(drgt.GetProperty("DsdContentHtml").GetValue(dr, null).ToString());
                }

                var pis = drgt.GetProperties();

                var pi = pis.FirstOrDefault(x => x.Name == idField);
                startPid.Clear();
                var id = pi.GetValue(dr, null).ToString();
                startPid.Add(id);

                var nrdt = list.Where(x => x.GetType().GetProperty(pidField).GetValue(x, null).ToString() == id.ToString()).ToList();

                if (nrdt.Count > 0)
                {
                    string rs = ListTreeEach(list, pidField, idField, startPid, listNo, deep + 1);

                    //子数组源于递归
                    sbTree.AppendLine(rs);
                }

                while (listNo.Count > deep - 1)
                {
                    listNo.RemoveAt(listNo.Count - 1);
                }
            }

            return sbTree.ToString();
        }

        /// <summary>
        /// 目录树
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public SharedResultVM MenuTree()
        {
            var vm = new SharedResultVM();

            try
            {
                var code = RouteData.Values["id"]?.ToString();

                var list = db.DocSetDetail.Where(x => x.DsCode == code).OrderBy(x => x.DsdOrder).Select(x => new
                {
                    x.DsdId,
                    x.DsdPid,
                    x.DsdTitle,
                    x.DsdOrder,
                    IsCatalog = string.IsNullOrEmpty(x.DsdContentMd)
                }).ToList();

                var listtree = Core.TreeTo.ListToTree(list, "DsdPid", "DsdId", new List<string> { Guid.Empty.ToString() });
                if (string.IsNullOrWhiteSpace(listtree))
                {
                    vm.Set(SharedEnum.RTag.lack);
                }
                else
                {
                    vm.Data = listtree.ToJArray();
                    vm.Set(SharedEnum.RTag.success);
                }
            }
            catch (Exception ex)
            {
                vm.Set(ex);
                Apps.FilterConfigs.WriteLog(HttpContext, ex);
            }

            return vm;
        }
    }
}