# Netnr.Framework

> 这是 <https://www.netnr.com> 站点的源代码

### 框架组件
- jQuery + Bootstrap4
- .NET Core (latest)
- EF + Linq
- 支持：SQLServer、MySQL、PostgreSQL、SQLite、InMemory等
- ==========================================
- Baidu.AI（实验室）
- FluentScheduler（定时任务）
- MailKit（邮箱验证）
- Netease.Cloud.Nos（网易对象存储）
- Netnr.Core（公共类库）
- Netnr.Login（第三方登录）
- Netnr.WeChat（微信公众号）
- Qcloud.Shared.NetCore（腾讯对象存储）
- Qiniu.Shared（七牛对象存储）
- sqlite-net-pcl（SQLite，日志）
- Swashbuckle.AspNetCore（Swagger 生成接口）

### 功能模块
- 登录、注册（第三方直接登录：QQ、微博、GitHub、淘宝、Microsoft）
- 文章：发布文章（Markdown编辑器）
- 文章留言：支持匿名留言，根据邮箱从 Gravatar 获取头像
- 公众号：（玩具）
- Gist：代码片段，自动同步GitHub、Gitee
- Run：在线运行HTML代码，写demo用
- Doc：文档管理，API说明文档
- Draw：绘制，集成开源项目 mxGraph、百度脑图
- Note：记事本（Markdown编辑器）
- 存储：云存储，对象存储
- 备份：自动备份数据库
- 日志：访问日志记录、统计

### 更新日志
- <https://www.netnr.com/home/list/131>
- 个站更新后才会更新源代码，非同步更新

### FQA

**示例数据**

第一次运行项目自动写入示例数据，账号：`netnr`，密码：`123456`  
示例数据存放在静态资源wwwroot目录下，访问地址：`{Host}/scripts/example/data.json`

**什么是本地授权码（SK）**  

根据当前时间的小时和分钟数结合配置文件的值进行计算得到的码，时间容差±6  
如：现在是17:10，配置的小时被减数是33，那么33-17=16，配置的分钟被减数是66，那么66-10=56，得到的本地授权码就是1656，当本地授权码超过容差时间会失效

**怎么添加文章标签**  

访问 `{Host}/services/keyvalues` 添加标签，输入本地授权码才能访问，   
标签表(Tags)依赖键值表(KeyValues)和键值同义词表(KeyValueSynonym)  
如输入`javascript`，从百科抓取该词描述（抓取失败机率高，需重试），（可选）添加同义词`js`，再添加 `javascript` 到标签

**Markdown编辑器用的什么**

请看这里：<https://md.netnr.com/>
