//-----------------------------------------------------------------------
// <copyright file="MailSender.cs" company="Beatsbury Software">
//     Copyright (c) Beatsbury Software. All rights reserved.
// </copyright>
// <author>Beatsbury</author>
//-----------------------------------------------------------------------

namespace FileSystemLogger
{
    using System;
    using System.ComponentModel;
    using System.Net;
    using System.Net.Mail;

    /// <summary>
    /// Mail sender class utilizing the SmtpClient type
    /// </summary>
    public class MailSender
    {
        #region Fields

        private SmtpClient client;

        #endregion Fields

        #region Constructors

        public MailSender(string mailServer = "smtp.gmail.com",
                                    int smtpPort = 587, string login = "nanatsuno@gmail.com",
                                    string password = "zpdzdkzflrmfooan", bool sslEnabled = true,
                                    string senderAddress = "nanatsuno@gmail.com",
                                    string recipientAddress = /*"s.semerikov@ablcompany.ru"*/"s.semerikov@ablcompany.ru", //"nanatsuno@gmail.com",
                                    string messageSubject = "", string messageBody = ""
                                    /*string mailServer = "mail.ablcompany.ru",
                                    int smtpPort = 25, string login = "s.semerikov@ablcompany.ru",
                                    string password = "rjvgfyb", bool sslEnabled = true,
                                    string senderAddress = "s.semerikov@ablcompany.ru",
                                    string recipientAddress = "beatsbury@hotmail.com",
                                    string messageSubject = "!", string messageBody = ""*/)
        {
            //_mailSender.mailServer = "mail.ablcompany.ru";
            //_mailSender.SmtpPort = 25;
            //_mailSender.sslEnabled = true;
            //_mailSender.login = "s.semerikov@ablcompany.ru";
            //_mailSender.password = "rjvgfyb";
            //_mailSender.senderAddress = "s.semerikov@ablcompany.ru";
            //_mailSender.recipientAddress = "s.semerikov@ablcompany.ru";
            //_mailSender.messageSubject = "!";
            //_mailSender.messageBody = "";
            MailServer = mailServer;
            SmtpPort = smtpPort;
            Login = login;
            Password = password;
            SslEnabled = sslEnabled;
            SenderAddress = senderAddress;
            RecipientAddress = recipientAddress;
            MessageSubject = messageSubject;
            MessageBody = messageBody;
        }

        #endregion Constructors

        #region Properties

        public string Login { get; private set; }

        public string MailServer { get; private set; }

        public string MessageBody { get; set; }

        public string MessageSubject { get; set; }

        public string Password { get; private set; }

        public string RecipientAddress { get; private set; }

        public string SenderAddress { get; private set; }

        public int SmtpPort { get; private set; }

        public bool SslEnabled { get; private set; }

        private string Error { get; set; }

        /// <summary>
        /// fields here
        /// </summary>
        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        private string Success { get; set; }

        #endregion Properties

        #region Methods

        public void Dispose()
        {
            client = null;
        }

        //private NetworkCredential credential;
        public void SendMail()
        {
            ServicePointManager.ServerCertificateValidationCallback +=
                (sender, cert, chain, sslPolicyErrors) => true;
            client = new SmtpClient(MailServer)
            {
                Port = SmtpPort,
                Credentials = new NetworkCredential(Login, Password),
                EnableSsl = SslEnabled
            };
            var mailSender = new MailAddress(SenderAddress);
            var mailRecipient = new MailAddress(RecipientAddress); //support@ablcompany.ru
            var message = new MailMessage(mailSender, mailRecipient)
            {
                Subject = MessageSubject,
                Body = MessageBody
            };
            Success = "Mail sent successfully to : " + mailRecipient.Address;

            try
            {
                client.Send(message);
                client.SendCompleted += SendCompleted;
                //message.Dispose();
            }
            catch (Exception ex)
            {
                Error = "MAIL ERROR: " + ex.Message;
                Program.LogLines.Add(Error);
            }
        }

        private static void SendCompleted(object sender, AsyncCompletedEventArgs e)
        {
            //Program._logLines.Add(success);
            Program.LogLines.Add(e.UserState as string);
        }

        #endregion Methods
    }
}