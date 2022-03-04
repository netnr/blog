### GET /svg/{wh}

#### 描述（Summary）
生成占位图,默认200x200

#### 参数（Parameters）
| 名称 | 类型 | 位置 | 说明 |
| ---- | ---- | ---- | ---- |
| wh | string | path | **必填** 。 自定义宽高，如 500x309 |

#### 响应（Responses）

```html
Status: 200 Success
```

### POST /svgo

#### 描述（Summary）
svg优化

#### 请求主体（RequestBody）

**multipart/form-data**

| 名称 | 类型 | 说明 |
| ---- | ---- | ---- |
| svgFile | array |  |
| svgJson | string |  |
| merge | int32 | **默认值**："" 。  |

#### 响应（Responses）

```html
Status: 200 Success
```

**application/json**

```json
{
  "code": 0,
  "msg": "string",
  "data": {}
}
```

| 名称 | 类型 | 说明 |
| ---- | ---- | ---- |
| code | integer / int32 |  |
| msg | string |  |
| data | object |  |
