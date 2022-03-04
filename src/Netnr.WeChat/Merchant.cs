using Netnr.WeChat.Helpers;
using System;
using System.IO;
using System.Text;

namespace Netnr.WeChat
{
    /// <summary>
    /// 
    /// </summary>
    public class Merchant
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="access_token"></param>
        /// <param name="content">
        /// 参见官方文档
        /// </param>
        /// <returns>
        /// {
        ///"errcode": 0,
        ///"errmsg": "success",
        ///"product_id": "pDF3iYwktviE3BzU3BKiSWWi9Nkw"
        /// }
        /// </returns>
        public static string Create(string access_token, object content)
        {
            var result = NetnrCore.HttpTo.Post(string.Format("https://api.weixin.qq.com/merchant/create?access_token={0}", access_token), NetnrCore.ToJson(content));
            return result;
        }
        /// <summary>
        /// 删除商品
        /// </summary>
        /// <param name="access_token"></param>
        /// <param name="product_id">商品ID</param>
        /// <returns>
        /// {
        ///"errcode":0,
        ///"errmsg":"success"
        ///}
        ///</returns>
        public static string Del(string access_token, string product_id)
        {
            var content = new StringBuilder();
            content.Append("{")
                   .Append('"' + "product_id" + '"' + ": " + '"' + product_id + '"')
                   .Append("}");

            var result = NetnrCore.HttpTo.Post(string.Format("https://api.weixin.qq.com/merchant/del?access_token={0}", access_token), content.ToString());
            return result;
        }
        /// <summary>
        /// 修改商品
        /// 
        /// 备注：
        /// 1.product_id表示要更新的商品的ID，其他字段说明请参考增加商品接口。
        ///2.从未上架的商品所有信息均可修改，否则商品的名称(name)、商品分类(category)、商品属性(property)这三个字段不可修改。
        /// </summary>
        /// <param name="access_token"></param>
        /// <param name="content"></param>
        /// <returns>
        /// {
        /// "errcode":0,
        /// "errmsg":"success"
        /// }
        /// </returns>
        public static string Update(string access_token, object content)
        {
            var result = NetnrCore.HttpTo.Post(string.Format("https://api.weixin.qq.com/merchant/update?access_token={0}", access_token), NetnrCore.ToJson(content));
            return result;
        }

        /// <summary>
        /// 查询
        /// </summary>
        /// <param name="access_token"></param>
        /// <param name="product_id"></param>
        /// <returns>
        /// 商品详细信息,
        /// 具体请参见官方文档</returns>
        public static string Get(string access_token, string product_id)
        {
            var content = new StringBuilder();
            content.Append("{")
                   .Append('"' + "product_id" + '"' + ": " + '"' + product_id + '"')
                   .Append("}");
            var result = NetnrCore.HttpTo.Post(string.Format("https://api.weixin.qq.com/merchant/get?access_token={0}", access_token),
                         content.ToString());
            return result;
        }

        /// <summary>
        /// 获取指定状态的所有商品
        /// </summary>
        /// <param name="access_token"></param>
        /// <param name="status">
        /// 商品状态(0-全部, 1-上架, 2-下架)
        /// </param>
        /// <returns>
        /// 商品列表信息,
        /// 具体请参见官方文档</returns>
        public static string GetByStatus(string access_token, int status)
        {

            var content = new StringBuilder();
            content.Append("{")
                   .Append('"' + "status" + '"' + ": " + status)
                   .Append("}");
            var result = NetnrCore.HttpTo.Post(string.Format("https://api.weixin.qq.com/merchant/getbystatus?access_token={0}", access_token),
                         content.ToString());
            return result;
        }
        /// <summary>
        /// 商品上下架
        /// </summary>
        /// <param name="access_token"></param>
        /// <param name="product_id">商品id</param>
        /// <param name="status">
        /// 商品上下架标识(0-下架, 1-上架)
        /// </param>
        /// <returns>
        /// {
        ///"errcode":0,
        ///"errmsg":"success"
        ///}
        /// </returns>
        public static string ModProductStatus(string access_token, string product_id, int status)
        {

            var content = new StringBuilder();
            content.Append("{")
                   .Append('"' + "product_id" + '"' + ": " + '"' + product_id + '"').Append(",")
                   .Append('"' + "status" + '"' + ": " + status)
                   .Append("}");
            var result = NetnrCore.HttpTo.Post(string.Format("https://api.weixin.qq.com/merchant/modproductstatus?access_token={0}", access_token),
                         content.ToString());
            return result;
        }


        /// <summary>
        /// 商品管理 分类
        /// </summary>
        public class Category
        {
            /// <summary>
            /// 获取指定分类的所有子分类
            /// </summary>
            /// <param name="access_token"></param>
            /// <param name="cate_id"></param>
            /// <returns></returns>
            public static string GetSub(string access_token, string cate_id)
            {

                var content = new StringBuilder();
                content.Append("{")
                       .Append('"' + "cate_id" + '"' + ": " + cate_id)
                       .Append("}");
                var result = NetnrCore.HttpTo.Post(string.Format("https://api.weixin.qq.com/merchant/category/getsub?access_token={0}", access_token),
                             content.ToString());
                return result;
            }
            /// <summary>
            /// 获取指定子分类的所有SKU
            /// </summary>
            /// <param name="access_token"></param>
            /// <param name="cate_id">分类id</param>
            /// <returns></returns>
            public static string GetSKU(string access_token, string cate_id)
            {

                var content = new StringBuilder();
                content.Append("{")
                       .Append('"' + "cate_id" + '"' + ": " + cate_id)
                       .Append("}");
                var result = NetnrCore.HttpTo.Post(string.Format("https://api.weixin.qq.com/merchant/category/getsku?access_token={0}", access_token),
                             content.ToString());
                return result;
            }
            /// <summary>
            /// 获取指定分类的所有属性
            /// </summary>
            /// <param name="access_token"></param>
            /// <param name="cate_id"></param>
            /// <returns></returns>
            public static string GetProperty(string access_token, string cate_id)
            {

                var content = new StringBuilder();
                content.Append("{")
                       .Append('"' + "cate_id" + '"' + ": " + cate_id)
                       .Append("}");
                var result = NetnrCore.HttpTo.Post(string.Format("https://api.weixin.qq.com/merchant/category/getproperty?access_token={0}", access_token),
                             content.ToString());
                return result;
            }
        }
    }

    /// <summary>
    ///功能接口
    /// </summary>
    public class Common
    {
        /// <summary>
        /// 上传图片
        /// </summary>
        /// <param name="access_token"></param>
        /// <param name="fileName">图片名称,如1.jpg</param>
        /// <param name="inputStream">图片名称,如1.jpg</param>
        /// <returns>
        /// {
        ///"errcode":0,
        ///"errmsg":"success", 
        ///"image_url": "http://mmbiz.qpic.cn/mmbiz/4whpV1VZl2ibl4JWwwnW3icSJGqecVtRiaPxwWEIr99eYYL6AAAp1YBo12CpQTXFH6InyQWXITLvU4CU7kic4PcoXA/0"
        ///}
        /// </returns>
        public static string Upload_Img(string access_token, string fileName, Stream inputStream)
        {
            var url = string.Format("https://api.weixin.qq.com/merchant/common/upload_img?access_token={0}&filename={1}", access_token, fileName);
            var returnText = Util.HttpRequestPost(url, "filename", fileName, inputStream);
            return returnText;
        }
    }

    /// <summary>
    /// 邮费模板管理接口
    /// </summary>
    public class Express
    {
        /// <summary>
        /// 增加邮费模板
        /// </summary>
        /// <param name="access_token"></param>
        /// <param name="content">
        /// 邮费模版，具体请参见官方文档
        /// </param>
        /// <returns>
        /// {
        ///  "errcode": 0,
        /// "errmsg": "success"， 
        /// "template_id": 123456
        /// }
        /// </returns>
        public static string Add(string access_token, object content)
        {
            var result = NetnrCore.HttpTo.Post(string.Format("https://api.weixin.qq.com/merchant/express/add?access_token={0}", access_token), NetnrCore.ToJson(content));
            return result;
        }
        /// <summary>
        /// 删除邮费模板
        /// </summary>
        /// <param name="access_token"></param>
        /// <param name="template_id">邮费模版id</param>
        /// <returns>
        /// {
        /// "errcode": 0,
        ///"errmsg": "success"
        ///}
        /// </returns>
        public static string Del(string access_token, string template_id)
        {
            var content = new StringBuilder();
            content.Append("{")
                   .Append('"' + "template_id" + '"' + ": " + template_id)
                   .Append("}");
            var result = NetnrCore.HttpTo.Post(string.Format("https://api.weixin.qq.com/merchant/express/del?access_token={0}", access_token), content.ToString());
            return result;
        }

        /// <summary>
        /// 修改邮费模板
        /// </summary>
        /// <param name="access_token"></param>
        /// <param name="template_id">邮费模板ID</param>
        /// <param name="delivery_template">邮费模板信息(字段说明详见增加邮费模板)</param>
        /// <returns>
        /// {
        /// "errcode": 0,
        ///"errmsg": "success"
        ///}
        /// </returns>
        public static string Del(string access_token, int template_id, object delivery_template)
        {
            var content = new StringBuilder();
            content.Append("{")
                   .Append('"' + "template_id" + '"' + ": " + template_id).Append(",")
                   .Append('"' + "delivery_template" + '"' + ": ").Append(NetnrCore.ToJson(delivery_template))
                   .Append("}");
            var result = NetnrCore.HttpTo.Post(string.Format("https://api.weixin.qq.com/merchant/express/del?access_token={0}", access_token), content.ToString());
            return result;
        }
        /// <summary>
        ///获取指定ID的邮费模板
        /// </summary>
        /// <param name="access_token"></param>
        /// <param name="template_id">邮费模板ID</param>
        /// <returns>邮费模版详细信息，详细请参见官方文档</returns>
        public static string Del(string access_token, int template_id)
        {
            var content = new StringBuilder();
            content.Append("{")
                   .Append('"' + "template_id" + '"' + ": " + template_id)
                   .Append("}");
            var result = NetnrCore.HttpTo.Post(string.Format("https://api.weixin.qq.com/merchant/express/del?access_token={0}", access_token), content.ToString());
            return result;
        }

        /// <summary>
        /// 获取所有邮费模板
        /// </summary>
        /// <param name="access_token"></param>
        /// <returns></returns>
        public static string GetAll(string access_token)
        {
            var result = NetnrCore.HttpTo.Get(string.Format("https://api.weixin.qq.com/merchant/express/getall?access_token={0}", access_token));
            return result;
        }

    }

    /// <summary>
    /// 分组管理接口
    /// </summary>
    public class Group
    {
        /// <summary>
        /// 增加分组
        /// </summary>
        /// <param name="access_token"></param>
        /// <param name="content">
        /// {
        ///  "group_detail" : {
        ///   "group_name": "测试分组", 
        ///   "product_list" : [
        ///       "pDF3iY9cEWyMimNlKbik_NYJTzYU", 
        ///      "pDF3iY4kpZagQfwJ_LVQBaOC-LsM"
        ///  ]
        ///  }
        /// }
        /// 
        /// group_name	分组名称
        /// product_list	商品ID集合
        /// </param>
        /// <returns>
        /// {
        /// "errcode":0,
        /// "errmsg":"success",
        /// "group_id": 19
        ///}
        /// </returns>
        public static string Add(string access_token, object content)
        {
            var result = NetnrCore.HttpTo.Post(string.Format("https://api.weixin.qq.com/merchant/group/add?access_token={0}", access_token), NetnrCore.ToJson(content));
            return result;
        }
        /// <summary>
        /// 删除分组
        /// </summary>
        /// <param name="access_token"></param>
        /// <param name="group_id">分组ID</param>
        /// <returns>
        /// {
        /// "errcode":0,
        /// "errmsg":"success"
        /// }
        /// </returns>
        public static string Del(string access_token, int group_id)
        {
            var content = new StringBuilder();
            content.Append("{")
                   .Append('"' + "group_id" + '"' + ": " + group_id)
                   .Append("}");
            var result = NetnrCore.HttpTo.Post(string.Format("https://api.weixin.qq.com/merchant/group/del?access_token={0}", access_token), content.ToString());
            return result;
        }
        /// <summary>
        /// 修改分组属性
        /// </summary>
        /// <param name="access_token"></param>
        /// <param name="group_id"></param>
        /// <param name="group_name"></param>
        /// <returns>
        /// {
        ///  "errcode":0,
        /// "errmsg":"success"
        /// }
        /// </returns>
        public static string PropertyMod(string access_token, int group_id, string group_name)
        {
            var content = new StringBuilder();
            content.Append("{")
                   .Append('"' + "group_id" + '"' + ": " + group_id).Append(",")
                   .Append('"' + "group_name" + '"' + ": " + '"' + group_name + '"')
                   .Append("}");
            var result = NetnrCore.HttpTo.Post(string.Format("https://api.weixin.qq.com/merchant/group/propertymod?access_token={0}", access_token), content.ToString());
            return result;
        }
        /// <summary>
        /// 修改分组商品
        /// </summary>
        /// <param name="access_token"></param>
        /// <param name="group_id"></param>
        /// <param name="product">分组商品信息, 数据示例：
        /// [
        /// {
        /// 	"product_id": "pDF3iY-CgqlAL3k8Ilz-6sj0UYpk",
        /// 	"mod_action": 1
        /// }, 
        /// {
        /// 	"product_id": "pDF3iY-RewlAL3k8Ilz-6sjsepp9",
        /// 	"mod_action": 0
        /// }, 
        /// ]
        /// </param>
        /// <returns>
        /// {
        ///  "errcode":0,
        /// "errmsg":"success"
        /// }
        /// </returns>
        public static string ProductMod(string access_token, int group_id, object product)
        {
            var content = new StringBuilder();
            content.Append("{")
                   .Append('"' + "group_id" + '"' + ": " + group_id).Append(",")
                   .Append('"' + "product" + '"' + ": ").Append(NetnrCore.ToJson(product))
                   .Append("}");
            var result = NetnrCore.HttpTo.Post(string.Format("https://api.weixin.qq.com/merchant/group/productmod?access_token={0}", access_token), content.ToString());
            return result;
        }
        /// <summary>
        /// 获取所有分组
        /// </summary>
        /// <param name="access_token"></param>
        /// <returns>
        /// {
        /// "errcode": 0,
        /// "errmsg": "success",
        /// "groups_detail": [
        /// 	{
        /// 	  "group_id": 200077549,
        /// 	  "group_name": "最新上架"
        /// 	},
        /// 	{
        ///   "group_id": 200079772,
        ///   "group_name": "全球热卖"
        /// }
        /// ]
        /// }
        /// </returns>
        public static string GetAll(string access_token)
        {
            var result = NetnrCore.HttpTo.Get(string.Format("https://api.weixin.qq.com/merchant/group/getall?access_token={0}", access_token));
            return result;
        }

        /// <summary>
        /// 根据分组ID获取分组信息
        /// </summary>
        /// <param name="access_token"></param>
        /// <param name="group_id">分组ID</param>
        /// <returns>
        /// {
        ///"errcode": 0,
        ///"errmsg": "success",
        ///"group_detail": {
        ///	"group_id": 200077549,
        ///	"group_name": "最新上架",
        ///"product_list": 
        ///[
        ///  "pDF3iYzZoY-Budrzt8O6IxrwIJAA",
        ///  "pDF3iY3pnWSGJcO2MpS2Nxy3HWx8",
        ///  "pDF3iY33jNt0Dj3M3UqiGlUxGrio"
        ///]
        ///}
        ///}
        /// </returns>
        public static string GetById(string access_token, int group_id)
        {

            var content = new StringBuilder();
            content.Append("{")
                   .Append('"' + "group_id" + '"' + ": " + group_id).Append(",")
                   .Append("}");
            var result = NetnrCore.HttpTo.Post(string.Format("https://api.weixin.qq.com/merchant/group/getbyid?access_token={0}", access_token),
                         content.ToString());
            return result;
        }

    }

    /// <summary>
    /// 订单管理
    /// </summary>
    public class Order
    {
        /// <summary>
        /// 根据订单ID获取订单详情
        /// </summary>
        /// <param name="access_token"></param>
        /// <param name="order_id">订单ID</param>
        /// <returns>订单详情，具体请参见官方文档</returns>
        public static string GetById(string access_token, string order_id)
        {
            var content = new StringBuilder();
            content.Append("{")
                   .Append('"' + "order_id" + '"' + ": " + '"' + order_id + '"')
                  .Append("}");
            var result = NetnrCore.HttpTo.Post(string.Format("https://api.weixin.qq.com/merchant/order/getbyid?access_token={0}", access_token),
                         content.ToString());
            return result;
        }

        /// <summary>
        ///据订单状态/创建时间获取订单详情
        /// </summary>
        /// <param name="access_token"></param>
        /// <param name="status">订单状态(不带该字段-全部状态, 2-待发货, 3-已发货, 5-已完成, 8-维权中, )</param>
        /// <param name="begintime">订单创建时间起始时间(不带该字段则不按照时间做筛选)</param>
        /// <param name="endtime">订单创建时间终止时间(不带该字段则不按照时间做筛选)</param>
        /// <returns>订单详情,参见官方文档</returns>
        public static string GetByFilter(string access_token, int status, long begintime = 0, long endtime = 0)
        {
            var content = new StringBuilder();
            content.Append("{")
                   .Append('"' + "status" + '"' + ": " + status);
            if (begintime > 0 || endtime > 0)
            {
                content.Append(",");
                if (begintime <= 0) begintime = NetnrCore.ToTimestamp(DateTime.MinValue);
                if (endtime <= 0) endtime = NetnrCore.ToTimestamp(DateTime.Now);
                content.Append('"' + "begintime" + '"' + ": " + begintime).Append(",");
                if (endtime <= 0) endtime = NetnrCore.ToTimestamp(DateTime.Now);
                content.Append('"' + "endtime" + '"' + ": " + endtime);
            }
            content.Append("}");
            var result = NetnrCore.HttpTo.Post(string.Format("https://api.weixin.qq.com/merchant/order/getbyfilter?access_token={0}", access_token),
                         content.ToString());
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="access_token"></param>
        /// <param name="order_id">订单ID</param>
        /// <param name="need_delivery">商品是否需要物流(0-不需要，1-需要，无该字段默认为需要物流)</param>
        /// <param name="is_others">是否为6.4.5表之外的其它物流公司(0-否，1-是，无该字段默认为不是其它物流公司)
        /// 6.4.5附：物流公司ID
        /// 物流公司	ID
        /// =============================
        /// 邮政EMS	Fsearch_code
        /// 申通快递	002shentong
        /// 中通速递	066zhongtong
        /// 圆通速递	056yuantong
        /// 天天快递	042tiantian
        /// 顺丰速运	003shunfeng
        /// 韵达快运	059Yunda
        /// 宅急送	064zhaijisong
        /// 汇通快运	020huitong
        /// 易迅快递	zj001yixun
        /// ============================
        /// </param>
        /// <param name="delivery_company">
        /// 物流公司ID(参考《物流公司ID》；
        /// 当need_delivery为0时，可不填本字段；
        /// 当need_delivery为1时，该字段不能为空；
        /// 当need_delivery为1且is_others为1时，本字段填写其它物流公司名称)</param>
        /// <param name="delivery_track_no">
        /// 运单ID(
        /// 当need_delivery为0时，可不填本字段；
        /// 当need_delivery为1时，该字段不能为空；
        /// )
        /// </param>
        /// <returns>
        /// {
        ///"errcode": 0,
        ///"errmsg": "success"
        ///}
        ///</returns>
        public static string SetDelivery(string access_token, string order_id, int need_delivery, int is_others, string delivery_track_no = "", string delivery_company = "")
        {
            var content = new StringBuilder();
            content.Append("{")
                   .Append('"' + "order_id" + '"' + ": " + '"' + order_id + '"').Append(",")
                   .Append('"' + "need_delivery" + '"' + ": " + need_delivery).Append(",")
                   .Append('"' + "is_others" + '"' + ": " + is_others);
            if (need_delivery == 1)
            {
                content.Append(",")
                       .Append('"' + "delivery_company" + '"' + ": " + '"' + delivery_company + '"').Append(",")
                       .Append('"' + "delivery_track_no" + '"' + ": " + '"' + delivery_track_no + '"');
            }
            content.Append("}");
            var result = NetnrCore.HttpTo.Post(string.Format("https://api.weixin.qq.com/merchant/order/setdelivery?access_token={0}", access_token),
                         content.ToString());
            return result;
        }

        /// <summary>
        /// 关闭订单
        /// </summary>
        /// <param name="access_token"></param>
        /// <param name="order_id">订单ID</param>
        /// <returns>
        /// {
        ///"errcode": 0,
        ///"errmsg": "success"
        ///}
        ///</returns>
        public static string Close(string access_token, string order_id)
        {
            var content = new StringBuilder();
            content.Append("{")
                   .Append('"' + "order_id" + '"' + ": " + '"' + order_id + '"')
                  .Append("}");
            var result = NetnrCore.HttpTo.Post(string.Format("https://api.weixin.qq.com/merchant/order/close?access_token={0}", access_token),
                         content.ToString());
            return result;
        }

    }

    /// <summary>
    /// 货架管理接口
    /// </summary>
    public class Shelf
    {
        /// <summary>
        /// 增加货架
        /// </summary>
        /// <param name="access_token"></param>
        /// <param name="content">货架详情，参见官方文档
        /// 注意：货架有五个控件，每个控件post的数据都不一样。
        /// </param>
        /// <returns>
        /// {
        ///"errcode":0,
        ///"errmsg":"success",
        ///"shelf_id": 12
        ///}
        /// </returns>
        public static string Add(string access_token, object content)
        {
            var result = NetnrCore.HttpTo.Post(string.Format("https://api.weixin.qq.com/merchant/shelf/add?access_token={0}", access_token), NetnrCore.ToJson(content));
            return result;
        }
        /// <summary>
        /// 删除货架
        /// </summary>
        /// <param name="access_token"></param>
        /// <param name="shelf_id">货架ID</param>
        /// <returns>
        /// {
        /// "errcode":0,
        ///"errmsg":"success"
        ///}
        /// </returns>
        public static string Del(string access_token, int shelf_id)
        {

            var content = new StringBuilder();
            content.Append("{")
                   .Append('"' + "shelf_id" + '"' + ": " + shelf_id)
                   .Append("}");
            var result = NetnrCore.HttpTo.Post(string.Format("https://api.weixin.qq.com/merchant/shelf/del?access_token={0}", access_token),
                         content.ToString());
            return result;
        }

        /// <summary>
        /// 修改货架
        /// </summary>
        /// <param name="access_token"></param>
        /// <param name="shelf_id">货架ID</param>
        /// <param name="shelf_data">货架详情(字段说明详见增加货架)</param>
        /// <param name="shelf_banner">货架banner(图片需调用图片上传接口获得图片Url填写至此，否则修改货架失败)</param>
        /// <param name="shelf_name">货架名称</param>
        /// <returns>
        /// {
        /// "errcode":0,
        ///"errmsg":"success"
        ///}
        /// </returns>
        public static string Mod(string access_token, int shelf_id, object shelf_data, string shelf_banner, string shelf_name)
        {
            var content = new StringBuilder();
            content.Append("{")
                   .Append('"' + "shelf_id" + '"' + ": " + shelf_id).Append(",")
                   .Append('"' + "shelf_data" + '"' + ": " + NetnrCore.ToJson(shelf_data)).Append(",")
                   .Append('"' + "shelf_banner" + '"' + ": " + '"' + shelf_banner + '"').Append(",")
                   .Append('"' + "shelf_name" + '"' + ": " + '"' + shelf_name + '"')
                  .Append("}");
            var result = NetnrCore.HttpTo.Post(string.Format("https://api.weixin.qq.com/merchant/shelf/mod?access_token={0}", access_token), content.ToString());
            return result;
        }

        /// <summary>
        /// 获取所有货架
        /// </summary>
        /// <param name="access_token"></param>
        /// <returns></returns>
        public static string GetAll(string access_token)
        {
            var result = NetnrCore.HttpTo.Get(string.Format("https://api.weixin.qq.com/merchant/shelf/getall?access_token={0}", access_token));
            return result;
        }
        /// <summary>
        /// 根据货架ID获取货架信息
        /// </summary>
        /// <param name="access_token"></param>
        /// <param name="shelf_id">货架ID</param>
        /// <returns>
        /// {
        /// "errcode": 0,
        /// "errmsg": "success",
        /// "shelf_info": {
        /// 	"module_infos": [...]
        /// },
        /// "shelf_banner": "http://mmbiz.qpic.cn/mmbiz/4whpV1VZl2ibp2DgDXiaic6WdflMpNdInS8qUia2BztlPu1gPlCDLZXEjia2qBdjoLiaCGUno9zbs1UyoqnaTJJGeEew/0",
        /// "shelf_name": "新建货架",
        /// "shelf_id": 97
        /// }
        /// </returns>
        public static string GetById(string access_token, int shelf_id)
        {

            var content = new StringBuilder();
            content.Append("{")
                   .Append('"' + "shelf_id" + '"' + ": " + shelf_id)
                  .Append("}");
            var result = NetnrCore.HttpTo.Post(string.Format("https://api.weixin.qq.com/merchant/shelf/getbyid?access_token={0}", access_token),
                         content.ToString());
            return result;
        }
    }

    /// <summary>
    /// 库存管理接口
    /// </summary>
    public class Stock
    {
        /// <summary>
        /// 增加库存
        /// </summary>
        /// <param name="access_token"></param>
        /// <param name="product_id">商品ID</param>
        /// <param name="sku_info">sku信息,格式"id1:vid1;id2:vid2",如商品为统一规格，则此处赋值为空字符串即可</param>
        /// <param name="quantity">增加的库存数量</param>
        /// <returns>
        /// {
        /// "errcode":0,
        /// "errmsg":"success"
        /// }
        /// </returns>
        public static string Add(string access_token, string product_id, string sku_info, int quantity)
        {

            var content = new StringBuilder();
            content.Append("{")
                   .Append('"' + "product_id" + '"' + ": " + '"' + product_id + '"').Append(",")
                   .Append('"' + "sku_info" + '"' + ": " + '"' + sku_info + '"').Append(",")
                   .Append('"' + "quantity" + '"' + ": " + quantity).Append(",")
                   .Append("}");
            var result = NetnrCore.HttpTo.Post(string.Format("https://api.weixin.qq.com/merchant/stock/add?access_token={0}", access_token),
                         content.ToString());
            return result;
        }
        /// <summary>
        /// 减少库存
        /// </summary>
        /// <param name="access_token"></param>
        /// <param name="product_id">商品ID</param>
        /// <param name="sku_info">sku信息,格式"id1:vid1;id2:vid2"</param>
        /// <param name="quantity">减少的库存数量</param>
        /// <returns>
        /// {
        /// "errcode":0,
        /// "errmsg":"success"
        /// }
        /// </returns>
        public static string Reduce(string access_token, string product_id, string sku_info, int quantity)
        {

            var content = new StringBuilder();
            content.Append("{")
                   .Append('"' + "product_id" + '"' + ": " + '"' + product_id + '"').Append(",")
                   .Append('"' + "sku_info" + '"' + ": " + '"' + sku_info + '"').Append(",")
                   .Append('"' + "quantity" + '"' + ": " + quantity).Append(",")
                   .Append("}");
            var result = NetnrCore.HttpTo.Post(string.Format("https://api.weixin.qq.com/merchant/stock/reduce?access_token={0}", access_token),
                         content.ToString());
            return result;
        }
    }
}
