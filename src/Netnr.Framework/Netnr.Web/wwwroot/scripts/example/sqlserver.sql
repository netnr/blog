﻿SELECT '{' + '"UserInfo":' +
       (
           SELECT UserId,
                  UserName = 'netnr',
                  UserPwd = SUBSTRING(sys.fn_sqlvarbasetostr(HASHBYTES('MD5', '123456')), 3, 32),
                  UserCreateTime,
                  UserLoginTime,
                  LoginLimit,
                  UserSign,
                  Nickname,
                  UserPhoto,
                  UserSex,
                  UserMail,
                  UserSay
           FROM UserInfo
           WHERE UserId = 1
           FOR JSON AUTO
       ) + ',' + '"Tags":' +
       (
           SELECT * FROM dbo.Tags WHERE TagId IN ( 58, 96 ) FOR JSON AUTO
       ) + ',' + '"UserWriting":' +
       (
           SELECT * FROM dbo.UserWriting WHERE UwId = 117 FOR JSON AUTO
       ) + ',' + '"UserWritingTags":' +
       (
           SELECT * FROM dbo.UserWritingTags WHERE UwId = 117 FOR JSON AUTO
       ) + ',' + '"UserReply":' +
       (
           SELECT TOP 2
                  *
           FROM dbo.UserReply
           WHERE UrTargetId = 117
           ORDER BY UrCreateTime
           FOR JSON AUTO
       ) + ',' + '"Run":' +
       (
           SELECT TOP 1 * FROM dbo.Run ORDER BY RunCreateTime FOR JSON AUTO
       ) + ',' + '"KeyValues":' +
       (
           SELECT *
           FROM dbo.KeyValues
           WHERE KeyName IN ( 'https', 'browser' )
           FOR JSON AUTO
       ) + ',' + '"Gist":' +
       (
           SELECT *
           FROM dbo.Gist
           WHERE GistCode = '5373307231488995367'
           FOR JSON AUTO
       ) + ',' + '"Draw":' +
       (
           SELECT *
           FROM dbo.Draw
           WHERE DrId IN ( 'd4969500168496794720', 'm4976065893797151245' )
           FOR JSON AUTO
       ) + ',' + '"DocSet":' +
       (
           SELECT *
           FROM dbo.DocSet
           WHERE DsCode IN ( '4840050256984581805', '5036967707833574483' )
           FOR JSON AUTO
       ) + ',' + '"DocSetDetail":' +
       (
           SELECT *
           FROM dbo.DocSetDetail
           WHERE DsCode IN ( '4840050256984581805', '5036967707833574483' )
           FOR JSON AUTO
       ) + '}';