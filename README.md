## Netnr.Blog
个人站点

### 框架组件
- jQuery + Bootstrap
- .NET （latest）
- EF + Linq
- 支持：SQLServer、MySQL、PostgreSQL、SQLite 等
- ==================================
- Baidu-AI （OCR 免费额度）
- DeviceDetector.NET （日志分析设备来源、爬虫）
- FluentScheduler（定时任务）
- IP2Region （IP 区域查询）
- Markdig （markdown 解析）
- Netease.Cloud.Nos（网易对象存储）
- Qcloud.Shared.NetCore（腾讯对象存储）
- Qiniu.Shared（七牛对象存储）
- SkiaSharp （验证码）
- SkiaSharp.QrCode （二维码）
- Swashbuckle.AspNetCore（Swagger 生成接口）
- jieba.NET（搜索分词）
- MailKit（邮箱验证）
- Netnr.Core（公共类库）
- Netnr.Login（第三方登录）
- Netnr.WeChat（微信公众号）

### 功能模块
- 登录、注册（第三方直接登录：QQ、微博、GitHub、淘宝、Microsoft）
- 文章：发布文章（Markdown 编辑器）
- 留言：文章留言，根据邮箱从 Gravatar 获取头像
- 公众号：（玩具）
- Gist：代码片段，自动同步 GitHub、Gitee
- Run：在线运行 HTML 代码，写 demo 用
- Doc：文档管理，API 说明文档
- Draw：绘制，集成开源项目 mxGraph、百度脑图
- Note：记事本（Markdown 编辑器）
- 存储：腾讯云 COS、网易云 NOS、七牛云 KODO
- 备份：自动备份数据库到私有 GitHub、Gitee
- 管理：文章、留言管理、日志记录、日志统计、新增标签等

### FQA
- 先修改配置，appsettings.json 修改为自己对应的参数
  - 数据库连接、域名、资源路径、三方登录 Key、接口密钥、邮箱、备份数据库的私有仓库
  - 首次运行项目自动导入示例数据，账号：`netnr`，密码：`123456`，示例数据存放在 `db` 目录
- 后台管理
  - 管理员登录后访问 `/admin`
  - 文章管理、回复管理、日志列表、日志图表、键值标签
  - 添加文章标签 `/admin/keyvalues`
  - 标签表 （Tags） 依赖键值表 （KeyValues） 和键值同义词表 （KeyValueSynonym）
  - 如输入 `javascript`，从百科抓取该词描述（抓取失败机率高，需重试），（可选）添加同义词 `js`，再添加 `javascript` 到标签
- Markdown 编辑器 <https://md.js.org>