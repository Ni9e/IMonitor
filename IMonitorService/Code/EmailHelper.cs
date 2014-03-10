using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;

namespace IMonitorService.Code
{
    public class EmailHelper
    {
        public static EmailFrom From { get; set; }
        public static string To { get; set; }
        public static List<string> Cc { get; set; }

        public EmailHelper(EmailFrom emailFrom, string to, List<string> cc)
        {
            From = emailFrom;
            To = to;
            Cc = cc;
        }

        public bool SendMail(string subject, string mailBody)
        {
            bool IsSuccess = false;
            try
            {
                SmtpClient smtp = new SmtpClient();
                smtp.DeliveryMethod = SmtpDeliveryMethod.Network;
                smtp.EnableSsl = false;
                smtp.Host = From.Host;
                smtp.Port = From.Port;
                smtp.Credentials = new NetworkCredential(From.Name, From.Password);

                MailMessage mm = new MailMessage();
                mm.Priority = MailPriority.High;
                mm.From = new MailAddress(From.Name, "门店监控系统", Encoding.GetEncoding(936)); // 936编码防止中文乱码
                foreach (string cc in Cc)
                {
                    mm.CC.Add(new MailAddress(cc));
                }
                mm.To.Add(new MailAddress(To, "", Encoding.GetEncoding(936)));
                mm.Subject = subject;
                mm.SubjectEncoding = Encoding.GetEncoding(936);
                mm.IsBodyHtml = true; //邮件正文是否是HTML格式
                mm.BodyEncoding = Encoding.GetEncoding(936);
                mm.Body = mailBody;
                ////邮件正文
                //mm.Attachments.Add( new Attachment( @"d:a.doc", System.Net.Mime.MediaTypeNames.Application.Rtf ) );
                ////添加附件，第二个参数，表示附件的文件类型，可以不用指定
                ////可以添加多个附件
                //mm.Attachments.Add( new Attachment( @"d:b.doc") );
                smtp.Send(mm); //发送邮件，如果不返回异常， 则大功告成了。
                IsSuccess = true;

            }
            catch (System.Exception ex)
            {
                IsSuccess = false;
                Console.WriteLine(ex.Message);
            }
            return IsSuccess;
        }

        //public static bool SendMail(string Email, string EPassword, string MailServer, int MailServerPort, string MailTo, string MailToName, string MailTitle, string MailBody)
        //{

        //    bool request = false;
        //    try
        //    {
        //        SmtpClient smtp = new SmtpClient(); //实例化一个SmtpClient
        //        smtp.DeliveryMethod = SmtpDeliveryMethod.Network; //将smtp的出站方式设为 Network
        //        smtp.EnableSsl = false;//smtp服务器是否启用SSL加密
        //        smtp.Host = MailServer; //指定 smtp 服务器地址
        //        smtp.Port = MailServerPort;             //指定 smtp 服务器的端口，默认是25，如果采用默认端口，可省去
        //        //如果需要认证，则用下面的方式
        //        smtp.Credentials = new NetworkCredential(Email, EPassword);
        //        MailMessage mm = new MailMessage(); //实例化一个邮件类
        //        mm.Priority = MailPriority.High; //邮件的优先级，分为 Low, Normal, High，通常用 Normal即可
        //        mm.From = new MailAddress(Email, "公司网站在线留言", Encoding.GetEncoding(936));
        //        //收件方看到的邮件来源；
        //        //第一个参数是发信人邮件地址
        //        //第二参数是发信人显示的名称
        //        //第三个参数是 第二个参数所使用的编码，如果指定不正确，则对方收到后显示乱码
        //        //936是简体中文的codepage值

        //        //mm.ReplyTo = new MailAddress("test_box@gmail.com", "我的接收邮箱", Encoding.GetEncoding(936));
        //        ////ReplyTo 表示对方回复邮件时默认的接收地址，即：你用一个邮箱发信，但却用另一个来收信
        //        ////上面后两个参数的意义， 同 From 的意义
        //        //mm.CC.Add("a@163.com,b@163.com,c@163.com");
        //        ////邮件的抄送者，支持群发，多个邮件地址之间用 半角逗号 分开

        //        ////当然也可以用全地址，如下：
        //        //mm.CC.Add(new MailAddress("a@163.com", "抄送者A", Encoding.GetEncoding(936)));
        //        //mm.CC.Add(new MailAddress("b@163.com", "抄送者B", Encoding.GetEncoding(936)));
        //        //mm.CC.Add(new MailAddress("c@163.com", "抄送者C", Encoding.GetEncoding(936)));

        //        //mm.Bcc.Add("d@163.com,e@163.com");
        //        ////邮件的密送者，支持群发，多个邮件地址之间用 半角逗号 分开

        //        ////当然也可以用全地址，如下：
        //        //mm.CC.Add(new MailAddress("d@163.com", "密送者D", Encoding.GetEncoding(936)));
        //        //mm.CC.Add(new MailAddress("e@163.com", "密送者E", Encoding.GetEncoding(936)));
        //        //mm.Sender = new MailAddress("xxx@xxx.com", "邮件发送者", Encoding.GetEncoding(936));
        //        ////可以任意设置，此信息包含在邮件头中，但并不会验证有效性，也不会显示给收件人
        //        ////说实话，我不知道有啥实际作用，大家可不理会，也可不写此项
        //        //mm.To.Add("g@163.com,h@163.com");
        //        //邮件的接收者，支持群发，多个地址之间用 半角逗号 分开

        //        //当然也可以用全地址添加

        //        mm.To.Add(new MailAddress(MailTo, MailToName, Encoding.GetEncoding(936)));
        //        mm.Subject = MailTitle; //邮件标题
        //        mm.SubjectEncoding = Encoding.GetEncoding(936);
        //        // 这里非常重要，如果你的邮件标题包含中文，这里一定要指定，否则对方收到的极有可能是乱码。
        //        // 936是简体中文的pagecode，如果是英文标题，这句可以忽略不用
        //        mm.IsBodyHtml = true; //邮件正文是否是HTML格式
        //        mm.BodyEncoding = Encoding.GetEncoding(936);
        //        //邮件正文的编码， 设置不正确， 接收者会收到乱码

        //        mm.Body = MailBody;
        //        ////邮件正文
        //        //mm.Attachments.Add( new Attachment( @"d:a.doc", System.Net.Mime.MediaTypeNames.Application.Rtf ) );
        //        ////添加附件，第二个参数，表示附件的文件类型，可以不用指定
        //        ////可以添加多个附件
        //        //mm.Attachments.Add( new Attachment( @"d:b.doc") );
        //        smtp.Send(mm); //发送邮件，如果不返回异常， 则大功告成了。
        //        request = true;
        //    }
        //    catch
        //    {
        //        request = false;
        //    }
        //    return request;
        //}
    }
}
