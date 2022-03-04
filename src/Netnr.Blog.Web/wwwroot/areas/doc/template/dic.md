### SysUser

系统用户表

|字段|类型|主键|自增|必填|默认值|注释|
|----|----|:-----:|:-----:|:-----:|:-----:|-----|
|SuId |varchar(50) |YES | |YES | | |
|SrId |varchar(50) | | | | |角色 |
|SuName |nvarchar(50) | | | | |账号 |
|SuPwd |nvarchar(50) | | | | |密码 |
|SuNickname |nvarchar(50) | | | | |昵称 |
|SuCreateTime |datetime(23) | | | | |创建时间 |
|SuStatus |int(10) | | | |0 |状态，1正常 |
|SuSign |varchar(50) | | | | |登录标识 |
|SuGroup |int(10) | | | | |分组 |


### SysRole

系统角色表

|字段|类型|主键|自增|必填|默认值|注释|
|----|----|:-----:|:-----:|:-----:|:-----:|-----|
|SrId |varchar(50) |YES | |YES | | |
|SrName |nvarchar(200) | | | | |名称 |
|SrStatus |int(10) | | | |0 |状态，1启用 |
|SrDescribe |nvarchar(200) | | | | |描述 |
|SrGroup |int(10) | | | | |分组 |
|SrMenus |nvarchar(-1) | | | | |菜单 |
|SrButtons |nvarchar(-1) | | | | |按钮 |
|SrCreateTime |datetime(23) | | | | |创建时间 |