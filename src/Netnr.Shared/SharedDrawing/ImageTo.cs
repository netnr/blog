#if Full || Drawing

using SkiaSharp;

namespace Netnr.SharedDrawing
{
    /// <summary>
    /// 引用组件：SkiaSharp、SkiaSharp.NativeAssets.Linux
    /// 跨平台
    /// </summary>
    public class ImageTo
    {
        /// <summary>
        /// 生成图片验证码
        /// </summary>
        /// <param name="code">随机码</param>
        public static byte[] Captcha(string code)
        {
            var random = new Random();

            //为验证码插入空格
            for (int i = 0; i < 2; i++)
            {
                code = code.Insert(random.Next(code.Length - 1), " ");
            }

            //验证码颜色集合  
            SKColor[] colors = { SKColors.LightBlue, SKColors.LightCoral, SKColors.LightGreen, SKColors.LightPink, SKColors.LightSkyBlue, SKColors.LightSteelBlue, SKColors.LightSalmon };

            //旋转角度
            int randAngle = 40;

            using SKBitmap bitmap = new(code.Length * 22, 38);
            using SKCanvas canvas = new(bitmap);
            //背景设为白色
            canvas.Clear(SKColors.White);

            //在随机位置画背景点
            for (int i = 0; i < 200; i++)
            {
                int x = random.Next(0, bitmap.Width);
                int y = random.Next(0, bitmap.Height);

                var paint = new SKPaint() { Color = colors[random.Next(colors.Length)] };
                canvas.DrawRect(new SKRect(x, y, x + 2, y + 2), paint);
            }

            //验证码绘制
            for (int i = 0; i < code.Length; i++)
            {
                //角度
                float angle = random.Next(-randAngle, randAngle);

                //不同高度
                int ii = random.Next(20) * (random.Next(1) % 2 == 0 ? -1 : 1) + 20;

                SKPoint point = new(18, 20);

                canvas.Translate(point);
                canvas.RotateDegrees(angle);

                var textPaint = new SKPaint()
                {
                    TextAlign = SKTextAlign.Center,
                    Color = colors[random.Next(colors.Length)],
                    TextSize = 28,
                    IsAntialias = true,
                    FakeBoldText = true
                };

                canvas.DrawText(code.Substring(i, 1), new SKPoint(0, ii), textPaint);
                canvas.RotateDegrees(-angle);
                canvas.Translate(0, -point.Y);
            }

            canvas.Translate(-4, 0);

            using var image = SKImage.FromBitmap(bitmap);
            using var ms = new MemoryStream();
            image.Encode(SKEncodedImageFormat.Jpeg, 90).SaveTo(ms);

            return ms.ToArray();
        }

        /// <summary>
        /// 缩略图
        /// </summary>
        /// <param name="bitmap"></param>
        /// <param name="width">宽度，null 根据高度自适应</param>
        /// <param name="height">高度，null 根据宽度自适应</param>
        public static SKBitmap Resize(SKBitmap bitmap, int? width = null, int? height = null)
        {
            //缩略大小
            double scale = 1;
            if (height.HasValue)
            {
                scale = (double)height.Value / bitmap.Height;
            }
            if (width.HasValue)
            {
                scale = (double)width.Value / bitmap.Width;
            }
            if (!width.HasValue)
            {
                width = Convert.ToInt32(bitmap.Width * scale);
            }
            if (!height.HasValue)
            {
                height = Convert.ToInt32(bitmap.Height * scale);
            }

            //调整大小
            var scaled = bitmap.Resize(new SKImageInfo(width.Value, height.Value), SKFilterQuality.High);
            return scaled;
        }

        /// <summary>
        /// 缩略图
        /// </summary>
        /// <param name="imgPath"></param>
        /// <param name="width">宽度，null 根据高度自适应</param>
        /// <param name="height">高度，null 根据宽度自适应</param>
        public static SKBitmap Resize(string imgPath, int? width = null, int? height = null)
        {
            SKBitmap bitmap = SKBitmap.Decode(imgPath);
            return Resize(bitmap, width, height);
        }

        /// <summary>
        /// 缩略图
        /// </summary>
        /// <param name="imgPath">图片完整路径</param>
        /// <param name="newPath">新图片存储路径</param>
        /// <param name="width">宽度，null 根据高度自适应</param>
        /// <param name="height">高度，null 根据宽度自适应</param>
        /// <param name="quality">质量，1-100，默认 90</param>
        public static void Resize(string imgPath, string newPath, int? width = null, int? height = null, int quality = 90)
        {
            Save(Resize(imgPath, width, height), newPath, quality);
        }

        /// <summary>
        /// 水印
        /// </summary>
        /// <param name="bitmap"></param>
        /// <param name="text">文字</param>
        /// <param name="paint">绘画信息</param>
        /// <param name="point">位置</param>
        public static SKBitmap WatermarkForText(SKBitmap bitmap, string text, Action<SKPaint> paint = null, Action<SKPoint> point = null)
        {
            using SKCanvas canvas = new(bitmap);

            var textPaint = new SKPaint()
            {
                IsAntialias = true,
                FakeBoldText = true,
                TextAlign = SKTextAlign.Right,
                TextSize = 48,
                Color = SKColors.Orange
            };

            paint?.Invoke(textPaint);

            var skp = new SKPoint(bitmap.Width * .95f, bitmap.Height * .95f);

            point?.Invoke(skp);

            canvas.DrawText(text, skp, textPaint);

            return bitmap;
        }

        /// <summary>
        /// 水印
        /// </summary>
        /// <param name="imgPath"></param>
        /// <param name="newPath"></param>
        /// <param name="text"></param>
        /// <param name="paint">绘画信息</param>
        /// <param name="point">位置</param>
        /// <returns></returns>
        public static void WatermarkForText(string imgPath, string newPath, string text, Action<SKPaint> paint = null, Action<SKPoint> point = null)
        {
            SKBitmap bitmap = SKBitmap.Decode(imgPath);
            Save(WatermarkForText(bitmap, text, paint, point), newPath, 100);
        }

        /// <summary>
        /// 保存图
        /// </summary>
        /// <param name="bitmap"></param>
        /// <param name="newPath">新图片存储路径</param>
        /// <param name="quality">质量，1-100，默认 90</param>
        public static void Save(SKBitmap bitmap, string newPath, int quality = 90)
        {
            using SKImage image = SKImage.FromBitmap(bitmap);

            var ext = Path.GetExtension(newPath).TrimStart('.').ToLower();
            if (ext == "jpg")
            {
                ext = "Jpeg";
            }
            Enum.TryParse(ext, true, out SKEncodedImageFormat eif);

            var data = image.Encode(eif, quality);

            //保存
            using var stream = new FileStream(newPath, FileMode.Create, FileAccess.Write);
            data.SaveTo(stream);
        }
    }
}

#endif