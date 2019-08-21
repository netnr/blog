using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Baidu.Aip.Speech;
using System.ComponentModel;
using Baidu.Aip.Ocr;
using Netnr.Func.ViewModel;

namespace Netnr.Web.Controllers
{
    public class LabController : Controller
    {
        [Description("实验室")]
        public IActionResult Index()
        {
            return View();
        }

        #region 百度API
        //string APP_ID = GlobalTo.GetValue("ApiKey:BaiduAip:APP_ID");
        string API_KEY = GlobalTo.GetValue("ApiKey:BaiduAip:API_KEY");
        string SECRET_KEY = GlobalTo.GetValue("ApiKey:BaiduAip:SECRET_KEY");
        #endregion

        [Description("语音合成页面")]
        public IActionResult Speech()
        {
            return View();
        }

        /// <summary>
        /// 语音合成
        /// txt String 合成的文本，使用UTF-8编码，请注意文本长度必须小于1024字节
        /// spd	Int	语速，取值0-9，默认为5中语速
        /// pit Int 音调，取值0-9，默认为5中语调
        /// vol Int 音量，取值0-15，默认为5中音量
        /// per Int 发音人选择, 0为女声，1为男声，3为情感合成-度逍遥，4为情感合成-度丫丫，默认为普通女
        /// </summary>
        [Description("语音合成接口")]
        [ResponseCache(Duration = 60)]
        public FileResult SpeechAPI(string txt, int spd = 5, int pit = 5, int vol = 5, int per = 0)
        {
            // 可选参数
            var option = new Dictionary<string, object>()
            {
                {"spd", spd},
                {"pit", pit},
                {"vol", vol},
                {"per", per}
            };

            var client = new Tts(API_KEY, SECRET_KEY);
            TtsResponse result = client.Synthesis(txt, option);

            return result.Success ? File(result.Data, "audio/mp3", Math.Abs(txt.GetHashCode()).ToString() + ".mp3") : null;
        }

        [Description("文字识别页面")]
        public IActionResult Ocr()
        {
            return View();
        }

        [Description("文字识别接口")]
        [ResponseCache(Duration = 60)]
        public ActionResultVM OcrAPI(string image_base64, string image_url)
        {
            var vm = new ActionResultVM();

            try
            {
                var client = new Ocr(API_KEY, SECRET_KEY)
                {
                    Timeout = 60000
                };

                var options = new Dictionary<string, object>{
                    {"language_type", "CHN_ENG"},
                    { "detect_direction", "true"},
                    { "detect_language", "true"},
                    { "probability", "true"}
                };

                if (!string.IsNullOrWhiteSpace(image_url))
                {
                    var result = client.GeneralBasicUrl(image_url, options);
                    vm.Set(ARTag.success);
                    vm.data = result;
                }
                else
                {
                    var image = Convert.FromBase64String(image_base64);
                    var result = client.GeneralBasic(image, options);
                    vm.Set(ARTag.success);
                    vm.data = result;
                }
            }
            catch (Exception ex)
            {
                vm.Set(ex);
            }

            return vm;
        }
    }
}