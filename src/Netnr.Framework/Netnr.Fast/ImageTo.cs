using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace Netnr.Fast
{
    /// <summary>
    /// 图片操作
    /// </summary>
    public class ImageTo
    {
        /// <summary>
        /// 生成图片验证码
        /// </summary>
        /// <param name="code">随机码</param>
        public static byte[] CreateImg(string code)
        {
            Random random = new Random();

            //为验证码插入空格
            for (int i = 0; i < 2; i++)
            {
                code = code.Insert(random.Next(code.Length - 1), " ");
            }

            //验证码颜色集合  
            Color[] c = { Color.LightBlue, Color.LightCoral, Color.LightGreen, Color.LightPink, Color.LightSkyBlue, Color.LightSteelBlue, Color.LightSalmon };

            //定义图像的大小，生成图像的实例  
            Bitmap Img = new Bitmap(code.Length * 22, 38);
            Graphics g = Graphics.FromImage(Img);

            g.Clear(Color.White);//背景设为白色  

            //验证码绘制在g中   
            for (int i = 0; i < code.Length; i++)
            {
                Font f = new Font(FontFamily.GenericSerif, 24, (FontStyle.Italic | FontStyle.Bold));//字体  
                Brush b = new SolidBrush(c[random.Next(c.Length - 1)]);//颜色  

                //控制验证码不在同一高度
                int ii = random.Next(20) * (random.Next(1) % 2 == 0 ? -1 : 1) + 10;
                g.DrawString(code.Substring(i, 1), f, b, (i * 20), ii);//绘制一个验证字符
            }

            //在随机位置画背景点  
            for (int i = 0; i < 200; i++)
            {
                int x = random.Next(Img.Width);
                int y = random.Next(Img.Height);
                g.DrawRectangle(new Pen(c[random.Next(c.Length - 1)], 0), x, y, 1, 1);
            }

            MemoryStream ms = new MemoryStream();
            Img.Save(ms, ImageFormat.Jpeg);

            //回收资源  
            g.Dispose();
            Img.Dispose();
            return ms.ToArray();
        }

        /// <summary>
        /// 生成缩略图
        /// </summary>
        /// <param name="oldImgPath">原图片地址</param>
        /// <param name="newMinImgPath">缩略图地址</param>
        /// <param name="newImgName">生成图片名称</param>
        /// <param name="width">缩略图宽度</param>
        /// <param name="height">缩略图高度</param>
        /// <param name="model">生成缩略的模式: wh|width|height|cut </param>
        public static void MinImg(string oldImgPath, string newMinImgPath, string newImgName, int width, int height, string model)
        {
            Image ImgBox = Image.FromFile(oldImgPath);

            int minWidth = width;      //缩略图的宽度
            int minHeight = height;    //缩略图的高度

            int x = 0;
            int y = 0;

            int oldWidth = ImgBox.Width;    //原始图片的宽度
            int oldHeight = ImgBox.Height;  //原始图片的高度

            switch (model.ToLower())
            {
                case "wh":      //指定高宽缩放,可能变形
                    break;
                case "width":       //指定宽度,高度按照比例缩放
                    minHeight = ImgBox.Height * width / ImgBox.Width;
                    break;
                case "height":       //指定高度,宽度按照等比例缩放
                    minWidth = ImgBox.Width * height / ImgBox.Height;
                    break;
                case "cut":
                    if (ImgBox.Width / (double)ImgBox.Height > minWidth / (double)minHeight)
                    {
                        oldHeight = ImgBox.Height;
                        oldWidth = ImgBox.Height * minWidth / minHeight;
                        y = 0;
                        x = (ImgBox.Width - oldWidth) / 2;
                    }
                    else
                    {
                        oldWidth = ImgBox.Width;
                        oldHeight = oldWidth * height / minWidth;
                        x = 0;
                        y = (ImgBox.Height - oldHeight) / 2;
                    }
                    break;
                default:
                    break;
            }

            //新建一个bmp图片
            Image bitmap = new Bitmap(minWidth, minHeight);

            //新建一个画板
            Graphics graphic = Graphics.FromImage(bitmap);

            //设置高质量查值法
            graphic.InterpolationMode = InterpolationMode.High;

            //设置高质量，低速度呈现平滑程度
            graphic.SmoothingMode = SmoothingMode.HighQuality;

            //清空画布并以透明背景色填充
            graphic.Clear(Color.Transparent);

            //在指定位置并且按指定大小绘制原图片的指定部分
            graphic.DrawImage(ImgBox, new Rectangle(0, 0, minWidth, minHeight), new Rectangle(x, y, oldWidth, oldHeight), GraphicsUnit.Pixel);

            try
            {
                bitmap.Save(newMinImgPath + newImgName, ImgBox.RawFormat);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                ImgBox.Dispose();
                bitmap.Dispose();
                graphic.Dispose();
            }

        }

        /// <summary>
        /// 在图片上添加文字水印
        /// </summary>
        /// <param name="path">要添加水印的图片路径</param>
        /// <param name="syPath">生成的水印图片存放的位置</param>
        /// <param name="syWord">水印文字</param>
        public static void AddTxt(string path, string syPath, string syWord = "netnr.com")
        {
            Image image = Image.FromFile(path);

            //新建一个画板
            Graphics graphic = Graphics.FromImage(image);
            graphic.DrawImage(image, 0, 0, image.Width, image.Height);

            //设置字体
            Font f = new Font("Verdana", 60);

            //设置字体颜色
            Brush b = new SolidBrush(Color.Green);

            graphic.DrawString(syWord, f, b, 35, 35);
            graphic.Dispose();

            //保存文字水印图片
            image.Save(syPath);
            image.Dispose();

        }

        /// <summary>
        /// 在图片上添加图片水印
        /// </summary>
        /// <param name="path">原服务器上的图片路径</param>
        /// <param name="syPicPath">水印图片的路径</param>
        /// <param name="waterPicPath">生成的水印图片存放路径</param>
        public static void AddImg(string path, string syPicPath, string waterPicPath)
        {
            Image image = Image.FromFile(path);
            Image waterImage = Image.FromFile(syPicPath);
            Graphics graphic = Graphics.FromImage(image);
            graphic.DrawImage(waterImage, new Rectangle(image.Width - waterImage.Width, image.Height - waterImage.Height, waterImage.Width, waterImage.Height), 0, 0, waterImage.Width, waterImage.Height, GraphicsUnit.Pixel);
            graphic.Dispose();

            image.Save(waterPicPath);
            image.Dispose();
        }
    }
}