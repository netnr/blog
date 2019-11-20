using System;
using MimeKit;
using MailKit.Net.Smtp;

namespace Netnr.Func
{
    /// <summary>
    /// 邮件辅助
    /// </summary>
    public class MailAid
    {
        /// <summary>
        /// 发送邮件
        /// </summary>
        /// <param name="ToMail">送达邮件地址</param>
        /// <param name="Title">标题</param>
        /// <param name="Content">内容</param>
        /// <returns></returns>
        public static ActionResultVM Send(string ToMail, string Title, string Content)
        {
            var vm = new ActionResultVM();

            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(GlobalTo.GetValue("MailKit:FromAddress")));
                message.To.Add(new MailboxAddress(ToMail));
                message.Subject = Title;
                message.Body = new BodyBuilder()
                {
                    HtmlBody = Content
                }.ToMessageBody();

                using var client = new SmtpClient();
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                client.Connect(GlobalTo.GetValue("MailKit:Host"), GlobalTo.GetValue<int>("MailKit:Port"), true);
                client.Authenticate(GlobalTo.GetValue("MailKit:Auth:UserName"), GlobalTo.GetValue("MailKit:Auth:Password"));
                client.Send(message);
                client.Disconnect(true);

                vm.Set(ARTag.success);
            }
            catch (Exception ex)
            {
                vm.Set(ex);
            }

            return vm;
        }
    }
}
