using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Netnr.Data;
using Netnr.Domain;
using Netnr.Func.ViewModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Netnr.Web.Areas.Doc.Controllers
{
    [Area("Doc")]
    public class CodeController : Controller
    {
        [Description("目录页面")]
        public IActionResult Index()
        {
            var vm = new DocTreeViewVM
            {
                //文档集编号
                DsCode = RouteData.Values["id"]?.ToString(),
                //页编码
                DsdId = RouteData.Values["sid"]?.ToString()
            };

            if (string.IsNullOrWhiteSpace(vm.DsCode))
            {
                return Redirect("/doc");
            }

            using (var db = new ContextBase())
            {
                var ds = db.DocSet.Find(vm.DsCode);
                if (ds == null)
                {
                    return Content("bad");
                }

                if (ds.DsOpen != 1)
                {
                    if (HttpContext.User.Identity.IsAuthenticated)
                    {
                        var uinfo = new Func.UserAuthAid(HttpContext).Get();
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

                //文档集目录
                vm.DocTree = db.DocSetDetail
                    .Where(x => x.DsCode == vm.DsCode)
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

                //未指定一项
                if (string.IsNullOrWhiteSpace(vm.DsdId))
                {
                    vm.DsdTitle = ds.DsName;
                    vm.DsdContentHtml = ds.DsRemark;
                }
                else
                {
                    //一项
                    if (vm.DocTree.Count > 0)
                    {
                        var queryView = db.DocSetDetail
                            .Where(x => x.DsdId == vm.DsdId)
                            .Select(x => new
                            {
                                x.DsdTitle,
                                x.DsdContentHtml
                            }).FirstOrDefault();

                        if (queryView != null)
                        {
                            vm.DsdTitle = queryView.DsdTitle;
                            vm.DsdContentHtml = queryView.DsdContentHtml;
                        }
                    }
                }
            }

            return View(vm);
        }

        [Description("新增、编辑一条")]
        [Authorize]
        public IActionResult Edit(string dsdid)
        {
            var code = RouteData.Values["id"]?.ToString();

            var uinfo = new Func.UserAuthAid(HttpContext).Get();

            using (var db = new ContextBase())
            {
                var ds = db.DocSet.Find(code);
                if (ds?.Uid != uinfo.UserId)
                {
                    return Content("unauthorized");
                }
            }

            var mo = new DocSetDetail
            {
                DsCode = code
            };

            if (!string.IsNullOrWhiteSpace(dsdid))
            {
                using var db = new ContextBase();
                mo = db.DocSetDetail.Where(x => x.DsdId == dsdid).FirstOrDefault();
            }

            return View(mo);
        }

        [Description("保存")]
        [Authorize]
        public ActionResultVM Save(DocSetDetail mo)
        {
            var vm = new ActionResultVM();

            var uinfo = new Func.UserAuthAid(HttpContext).Get();

            using (var db = new ContextBase())
            {
                var ds = db.DocSet.Find(mo.DsCode);
                if (ds?.Uid != uinfo.UserId)
                {
                    vm.Set(ARTag.unauthorized);
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
                        mo.DsdCreateTime = DateTime.Now;

                        db.DocSetDetail.Add(mo);
                    }
                    else
                    {
                        db.DocSetDetail.Update(mo);
                    }

                    int num = db.SaveChanges();
                    vm.Set(num > 0);
                    vm.data = mo.DsdId;
                }
            }

            return vm;
        }

        [Description("删除")]
        [Authorize]
        public IActionResult Del(string dsdid)
        {
            var code = RouteData.Values["id"]?.ToString();

            var uinfo = new Func.UserAuthAid(HttpContext).Get();

            if (!string.IsNullOrWhiteSpace(dsdid))
            {
                using var db = new ContextBase();
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

        [Description("目录")]
        [Authorize]
        public IActionResult Catalog()
        {
            var code = RouteData.Values["id"]?.ToString();

            var uinfo = new Func.UserAuthAid(HttpContext).Get();

            using (var db = new ContextBase())
            {
                var ds = db.DocSet.Find(code);
                if (ds?.Uid != uinfo.UserId)
                {
                    return Content("unauthorized");
                }
            }

            return View();
        }

        [Description("保存目录")]
        [Authorize]
        public string SaveCatalog(DocSetDetail mo)
        {
            var uinfo = new Func.UserAuthAid(HttpContext).Get();

            using var db = new ContextBase();
            var ds = db.DocSet.Find(mo.DsCode);
            if (ds?.Uid != uinfo.UserId)
            {
                return "unauthorized";
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
                var currmo = db.DocSetDetail.Where(x => x.DsdId == mo.DsdId).FirstOrDefault();
                currmo.DsdTitle = mo.DsdTitle;
                currmo.DsdOrder = mo.DsdOrder;
                currmo.DsdPid = mo.DsdPid;

                db.DocSetDetail.Update(currmo);
            }
            int num = db.SaveChanges();

            return num > 0 ? "success" : "fail";
        }

        [Description("删除目录")]
        [Authorize]
        public string DelCatalog(string code, string id)
        {
            var uinfo = new Func.UserAuthAid(HttpContext).Get();

            using var db = new ContextBase();
            var ds = db.DocSet.Find(code);
            if (ds?.Uid != uinfo.UserId)
            {
                return "unauthorized";
            }

            var listdsd = db.DocSetDetail.Where(x => x.DsCode == code && string.IsNullOrEmpty(x.DsdContentMd)).ToList();
            var removelist = Core.TreeTo.FindToTree(listdsd, "DsdPid", "DsdId", new List<string> { id });
            removelist.Add(listdsd.Where(x => x.DsdId == id).FirstOrDefault());
            db.DocSetDetail.RemoveRange(removelist);

            db.SaveChanges();

            return "success";
        }

        [Description("导出")]
        public IActionResult Export()
        {
            var code = RouteData.Values["id"]?.ToString();
            using var db = new ContextBase();
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
            var tm = Core.FileTo.ReadText(GlobalTo.WebRootPath + "/template/", "htmltoword.html");
            tm = tm.Replace("@netnrmd@", htmlbody);

            //文件名
            var filename = db.DocSet.Where(x => x.DsCode == code).FirstOrDefault()?.DsName ?? "netnrdoc";

            return File(Encoding.Default.GetBytes(tm), "application/msword", filename + ".doc");
        }

        private string ListTreeEach<T>(List<T> list, string pidField, string idField, List<string> startPid, List<int> listNo = null, int deep = 1)
        {
            StringBuilder sbTree = new StringBuilder();

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

                var pi = pis.Where(x => x.Name == idField).FirstOrDefault();
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

        [Description("目录树")]
        [Authorize]
        public string MenuTree()
        {
            var code = RouteData.Values["id"]?.ToString();

            using var db = new ContextBase();
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
                listtree = "[]";
            }
            return listtree;
        }

    }
}