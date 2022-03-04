#if Full || App

namespace Netnr.SharedApp;
/// <summary>
/// 资源引用
/// </summary>
public class QuoteTo
{
    /// <summary>
    /// 得到html字符串
    /// </summary>
    /// <param name="quotes">引用项，逗号分割，按顺序</param>
    /// <returns></returns>
    public static string Html(string quotes)
    {
        var srServer = SharedFast.GlobalTo.GetValue("StaticResource:Server") ?? "https://s1.netnr.com";

        var vh = new List<string>();

        List<string> listQuote = quotes.Split(',').ToList();
        foreach (var item in listQuote)
        {
            switch (item)
            {
                case "the":
                    vh.Add($@"
                            <!--
                            https://github.com/netnr
                            https://www.netnr.com
                            https://netnr.eu.org
                            {DateTime.Now:yyyy-MM-dd HH:mm:ss}
                            -->
                            ");
                    break;

                case "favicon":
                    vh.Add("<link rel='shortcut icon' href='/favicon.ico' type='image/x-icon' />");
                    break;

                case "blog-seo":
                    vh.Add("<meta name='keywords' content='NET牛人, Netnr, Gist, Run, Doc, Draw' />");
                    vh.Add("<meta name='description' content='NET牛人, Netnr, Gist, Run, Doc, Draw' />");
                    break;

                case "guff-seo":
                    vh.Add("<meta name='keywords' content='Guff,尬服,尬服乐天地' />");
                    vh.Add("<meta name='description' content='Guff,尬服,尬服乐天地' />");
                    break;

                case "ace.css":
                    vh.Add($"<link href='{srServer}/libs/acenav/ace.min.css' rel='stylesheet' />");
                    vh.Add($"<link href='{srServer}/libs/acenav/ace-skins.min.css' rel='stylesheet' async />");
                    break;
                case "ace.js":
                    vh.Add($"<script src='{srServer}/libs/acenav/ace.min.js'></script>");
                    break;

                case "easyui":
                    vh.Add($"<link href='{srServer}/libs/jquery-easyui/themes/metro/easyui.css' rel='stylesheet' />");
                    vh.Add($"<script src='{srServer}/libs/jquery-easyui/jquery.easyui.min.js'></script>");
                    break;

                case "bmob.js":
                    vh.Add($"<script src='{srServer}/libs/mix/bmob.min.js?170'></script>");
                    break;

                case "fast-xml-parser.js":
                    vh.Add($"<script src='{srServer}/libs/mix/fast-xml-parser.min.js'></script>");
                    break;

                case "clean-css.js":
                    vh.Add($"<script src='{srServer}/libs/mix/clean-css.min.js'></script>");
                    break;

                case "svgo.js":
                    vh.Add($"<script src='{srServer}/libs/mix/svgo.min.js'></script>");
                    break;

                case "device-detector-js.js":
                    vh.Add($"<script src='{srServer}/libs/mix/device-detector-js.min.js'></script>");
                    break;

                case "js-yaml.js":
                    vh.Add($"<script src='{srServer}/libs/mix/js-yaml.min.js'></script>");
                    break;

                case "identicon.js":
                    vh.Add($"<script src='{srServer}/libs/mix/identicon.min.js'></script>");
                    break;

                case "text-to-image.js":
                    vh.Add($"<script src='{srServer}/libs/mix/text-to-image.js'></script>");
                    break;

                case "fa.css":
                    vh.Add("<link href='https://npm.elemecdn.com/font-awesome@4.7.0/css/font-awesome.min.css' rel='stylesheet' async />");
                    break;

                case "jquery.js":
                case "jquery3.js":
                    vh.Add("<script src='https://npm.elemecdn.com/jquery@3.6.0/dist/jquery.min.js'></script>");
                    break;

                case "bootstrap3.css":
                    vh.Add("<link href='https://npm.elemecdn.com/bootstrap@3.4.1/dist/css/bootstrap.min.css' rel='stylesheet' />");
                    break;
                case "bootstrap3.js":
                    vh.Add("<script src='https://npm.elemecdn.com/bootstrap@3.4.1/dist/js/bootstrap.min.js'></script>");
                    break;

                case "bootstrap.css":
                case "bootstrap4.css":
                    vh.Add("<link href='https://npm.elemecdn.com/bootstrap@4.6.0/dist/css/bootstrap.min.css' rel='stylesheet' />");
                    break;
                case "bootstrap.js":
                case "bootstrap4.js":
                    vh.Add("<script src='https://npm.elemecdn.com/bootstrap@4.6.0/dist/js/bootstrap.bundle.min.js'></script>");
                    break;

                case "bootstrap5.css":
                    vh.Add("<link href='https://npm.elemecdn.com/bootstrap@5.1.3/dist/css/bootstrap.min.css' rel='stylesheet' />");
                    break;
                case "bootstrap5.js":
                    vh.Add("<script src='https://npm.elemecdn.com/bootstrap@5.1.3/dist/js/bootstrap.bundle.min.js'></script>");
                    break;

                case "swiper.css":
                    vh.Add("<link href='https://npm.elemecdn.com/swiper@8.0.6/swiper-bundle.min.css' rel='stylesheet' />");
                    break;
                case "swiper.js":
                    vh.Add("<script src='https://npm.elemecdn.com/swiper@8.0.6/swiper-bundle.min.js'></script>");
                    break;

                case "jz.js":
                    vh.Add("<script src='https://npm.elemecdn.com/jzjs@2.0.2/2.0.2/jz.min.js'></script>");
                    break;

                case "netnrmd.css":
                    vh.Add("<link href='https://npm.elemecdn.com/netnrmd@3.0.2/src/netnrmd.css' rel='stylesheet' />");
                    break;
                case "netnrmd.js":
                    vh.Add("<script src='https://npm.elemecdn.com/netnrmd@3.0.2/src/netnrmd.bundle.js'></script>");
                    break;

                case "netnrnav.js":
                    vh.Add("<script src='https://npm.elemecdn.com/netnrnav@1.1.2/src/netnrnav.bundle.min.js' ></script>");
                    break;

                case "tocbot.js":
                    vh.Add("<script src='https://npm.elemecdn.com/tocbot@4.18.2/dist/tocbot.min.js'></script>");
                    break;

                case "selectpage":
                    vh.Add("<link href='https://npm.elemecdn.com/selectpage@2.20.0/selectpage.css' rel='stylesheet' />");
                    vh.Add("<script src='https://npm.elemecdn.com/selectpage@2.20.0/selectpage.min.js'></script>");
                    break;

                case "ag-grid-community.js":
                    vh.Add("<script src='https://npm.elemecdn.com/ag-grid-community@27.0.1/dist/ag-grid-community.min.js'></script>");
                    break;

                case "ag-grid-enterprise.js":
                    vh.Add("<script src='https://npm.elemecdn.com/ag-grid-enterprise@27.0.1/dist/ag-grid-enterprise.min.js'></script>");
                    vh.Add("<script>agGrid.LicenseManager.prototype.outputMissingLicenseKey = _ => { }</script>");
                    break;

                case "webuploader.js":
                    vh.Add("<script src='https://npm.elemecdn.com/webuploader@0.1.8/dist/webuploader.html5only.min.js'></script>");
                    break;

                case "ckeditor.js":
                    vh.Add("<script src='https://npm.elemecdn.com/ckeditor@4.12.1/ckeditor.js'></script>");
                    break;

                case "crypto.js":
                    vh.Add("<script src='https://npm.elemecdn.com/crypto-js@4.1.1/crypto-js.js'></script>");
                    break;

                case "md5.js":
                    vh.Add("<script src='https://npm.elemecdn.com/blueimp-md5@2.19.0/js/md5.min.js'></script>");
                    break;

                case "uuid4.js":
                    vh.Add("<script src='https://npm.elemecdn.com/uuid@8.3.2/dist/umd/uuidv4.min.js'></script>");
                    break;

                //生成二维码
                case "qrcode.js":
                    vh.Add("<script src='https://npm.elemecdn.com/qrcode@1.5.0/build/qrcode.js'></script>");
                    break;

                //解析二维码
                case "jsqr.js":
                    vh.Add("<script src='https://npm.elemecdn.com/jsqr@1.4.0/dist/jsQR.js'></script>");
                    break;

                case "sql-formatter.js":
                    vh.Add("<script src='https://npm.elemecdn.com/sql-formatter@4.0.2/dist/sql-formatter.min.js'></script>");
                    break;

                case "highcharts.js":
                    vh.Add("<script src='https://npm.elemecdn.com/highcharts@9.3.3/highcharts.js'></script>");
                    break;

                case "hls.js":
                    vh.Add("<script src='https://npm.elemecdn.com/hls.js@1.1.5/dist/hls.min.js'></script>");
                    break;

                case "watermark.js":
                    vh.Add("<script src='https://npm.elemecdn.com/watermarkjs@2.1.1/dist/watermark.min.js'></script>");
                    break;

                case "nsfwjs":
                    vh.Add("<script src='https://npm.elemecdn.com/@tensorflow/tfjs@3.13.0/dist/tf.min.js'></script>");
                    vh.Add("<script src='https://npm.elemecdn.com/nsfwjs@2.4.1/dist/nsfwjs.min.js'></script>");
                    break;

                case "cropperjs":
                    vh.Add("<link href='https://npm.elemecdn.com/cropperjs@1.5.12/dist/cropper.css' rel='stylesheet' />");
                    vh.Add("<script src='https://npm.elemecdn.com/cropperjs@1.5.12/dist/cropper.min.js'></script>");
                    break;

                case "terser.js":
                    vh.Add("<script src='https://npm.elemecdn.com/terser@5.11.0/dist/bundle.min.js'></script>");
                    break;

                case "html2canvas.js":
                    vh.Add("<script src='https://npm.elemecdn.com/html2canvas@1.4.1/dist/html2canvas.min.js'></script>");
                    break;

                case "asciinema-player.css":
                    vh.Add($"<link href='https://npm.elemecdn.com/asciinema-player@2.6.1/resources/public/css/asciinema-player.css' rel='stylesheet' />");
                    break;
                case "asciinema-player.js":
                    vh.Add("<script src='https://npm.elemecdn.com/asciinema-player@2.6.1/resources/public/js/asciinema-player.js'></script>");
                    break;

                case "api-spec-converter.js":
                    vh.Add("<script src='https://npm.elemecdn.com/api-spec-converter@2.12.0/dist/api-spec-converter.js'></script>");
                    break;

                case "swagger-ui-dist.css":
                    vh.Add("<link href='https://npm.elemecdn.com/swagger-ui-dist@4.5.2/swagger-ui.css' rel='stylesheet' />");
                    break;
                case "swagger-ui-dist.js":
                    vh.Add("<script src='https://npm.elemecdn.com/swagger-ui-dist@4.5.2/swagger-ui-bundle.js'></script>");
                    vh.Add("<script src='https://npm.elemecdn.com/swagger-ui-dist@4.5.2/swagger-ui-standalone-preset.js'></script>");
                    break;

                case "js-beautify":
                    vh.Add("<script src='https://npm.elemecdn.com/js-beautify@1.14.0/js/lib/beautifier.min.js'></script>");
                    break;

                case "lrz.js":
                    vh.Add("<script src='https://npm.elemecdn.com/lrz@4.9.41/dist/lrz.all.bundle.js'></script>");
                    break;

                case "jdenticon.js":
                    vh.Add("<script src='https://npm.elemecdn.com/jdenticon@3.1.1/dist/jdenticon.min.js'></script>");
                    break;

                case "jszip.js":
                    vh.Add("<script src='https://npm.elemecdn.com/jszip@3.7.1/dist/jszip.min.js'></script>");
                    break;

                case "nginxbeautifier":
                    vh.Add("<script src='https://npm.elemecdn.com/nginxbeautifier@1.0.19/nginxbeautifier.js'></script>");
                    break;

                case "monaco-editor":
                    vh.Add("<script src='https://npm.elemecdn.com/monaco-editor@0.32.1/min/vs/loader.js'></script>");
                    vh.Add(@"
                            <script>
                                function htmlDecode(html) {
                                    var a = document.createElement('a');
                                    a.innerHTML = html;
                                    return a.innerText;
                                }

                                require.config({
                                    paths: {
                                        vs: 'https://npm.elemecdn.com/monaco-editor@0.32.1/min/vs'
                                    },
                                    'vs/nls': { availableLanguages: { '*': 'zh-cn' } }
                                });
                            </script>
                        ");
                    break;

                case "loading":
                    vh.Add("<div id='LoadingMask' style='position:fixed;top:0;left:0;bottom:0;right:0;background-color:white;z-index:19999;background-image:url(\"/images/loading.svg\");background-repeat:no-repeat;background-position:48% 45%'></div>");
                    break;
            }
        }

        return (string.Join(Environment.NewLine, vh) + Environment.NewLine).Replace(@"                            ", "");
    }
}
#endif