// MurphySecurity is a simple home security system designed to be used with Raspberry pi
// Copyright (C) 2021 Jérémy LEPROU jerem.jlr@gmail.com
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published
// by the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY, without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.

using System;
using System.IO;
using System.Collections.Concurrent;
using System.Net.Mail;
using System.Net;
using System.ComponentModel;
using System.Threading;

namespace MurphySecurity.Emails
{
    public static class EmailsSender
    {
        private const string EMAILSLIST_PATH = "wwwroot/Data/emailslist.txt";
        private const string SENDER_PATH = "wwwroot/Data/sender.txt";
        private const string IP_PATH = "wwwroot/Data/ip.txt";
        private const string EMAILSENDER_DEFAULT = "";
        private const string PASSWORDSENDER_DEFAULT = "";
        private static readonly object _locker = new object();
        private static string _ip = "";
        private static int _dailyEmailCount = 0;

        #region Private
        static EmailsSender()
        {
            LoadIP();
            LoadSender();
            LoadEmails();
            _ipChecker.DoWork += IPCheck;
            _ipChecker.RunWorkerAsync();
        }

        private static void LoadIP()
        {
            try
            {
                string[] readInfo = File.ReadAllText(IP_PATH).Split(new[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                _ip = readInfo[0];
            }
            catch (Exception exception)
            {
                Console.WriteLine("Error loading IP : " + exception.Message);
            }
        }

        private static void LoadSender()
        {
            try
            {
                string[] readInfo = File.ReadAllText(SENDER_PATH).Split(new[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                EmailSender = readInfo[0];
                PasswordSender = readInfo[1];
            }
            catch (Exception exception)
            {
                Console.WriteLine("Error loading sender : " + exception.Message);
            }
        }

        private static void LoadEmails()
        {
            EmailsBag = new ConcurrentBag<string>();
            try
            {
                string[] readInfo = File.ReadAllText(EMAILSLIST_PATH).Split(new[] { "\n", "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string email in readInfo)
                    EmailsBag.Add(email);
            }
            catch (Exception exception)
            {
                Console.WriteLine("Error loadiing emails : " + exception.Message);
            }
        }

        private static void SaveSender()
        {
            lock (_locker)
            {
                try
                {
                    string toWrite = EmailSender + "\n" + PasswordSender;
                    File.WriteAllText(SENDER_PATH, toWrite);
                }
                catch (Exception exception)
                {
                    Console.WriteLine("Error saving sender : " + exception.Message);
                }
            }
        }

        private static void SaveIP(string ip)
        {
            lock (_locker)
            {
                try
                {
                    string toWrite = ip;
                    File.WriteAllText(IP_PATH, toWrite);
                }
                catch (Exception exception)
                {
                    Console.WriteLine("Error saving IP : " + exception.Message);
                }
            }
        }

        private static void SendIPEmail(string ip)
        {
            string subject = "MURPHY SECURITY NEW IP : " + ip;
            string message = "Power cut or internet cut changed your ip, new ip is : " + ip + "\n" + "Direct link : https://" + ip;
            SendEmailAlert(subject, message);
        }

        /// <summary>
        /// Uses the gmail email sender to send an email to itself, it is needed because if no email is sent for a period of time,
        /// gmail automatically turns the security back on. Which prevents this application from using gmail emails to send emails.
        /// </summary>
        private static void SendDailyGmailEmail()
        {
            MailAddress fromAddress = new MailAddress(EmailSender, "MURPHY SECURITY");
            MailAddress toAddress = new MailAddress(EmailSender, "To User");
            string fromPassword = PasswordSender;
            string subject = "Daily email";
            string body = "Daily email to keep the gmail security off.";

            using SmtpClient smtp = new SmtpClient()
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword),
                Timeout = 20000
            };
            using MailMessage message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body
            };

            smtp.Send(message);
        }

        /// <summary>
        /// Returns the current server ip
        /// </summary>
        /// <returns></returns>
        private static string GetIP()
        {
            String address = "";
            WebRequest request = WebRequest.Create("http://checkip.dyndns.org/");
            using (WebResponse response = request.GetResponse())
            using (StreamReader stream = new StreamReader(response.GetResponseStream()))
                address = stream.ReadToEnd();
            int first = address.IndexOf("Address: ") + 9;
            int last = address.LastIndexOf("</body>");
            address = address.Substring(first, last - first);
            return address;
        }

        private static BackgroundWorker _ipChecker = new BackgroundWorker();
        /// <summary>
        /// Continuously checks if the ip has changed, if it's the case, sends emails with the new ip
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void IPCheck(object sender, DoWorkEventArgs e)
        {
            //Auto restarts the process if an exception was raised
            while (true)
            {
                try
                {
                    while (true)
                    {
                        string current_ip = GetIP();
                        if (!_ip.Equals(current_ip))
                        {
                            SendIPEmail(current_ip);
                            SaveIP(current_ip);
                            _ip = current_ip;
                        }
                        if (_dailyEmailCount == 1440)
                        {
                            SendDailyGmailEmail();
                            _dailyEmailCount = 0;
                        }
                        _dailyEmailCount++;
                        Thread.Sleep(60000);
                    }
                }
                catch (Exception exception)
                {
                    Console.WriteLine("ERROR IN IP/DAILY EMAIL SENDER : " + exception.Message);
                }
                Thread.Sleep(10000);
            }
        }
        #endregion

        #region Public
        public enum SendMethod
        {
            Gmail,
            SendGrid
        }
        public static SendMethod SelectedMethod { get; set; }
        public static ConcurrentBag<string> EmailsBag { get; private set; }
        public static string EmailSender { get; private set; }
        public static string PasswordSender { get; private set; }
        public static string APIKey { get; private set; }

        /// <summary>
        /// A method to trigger the static constructor if needed.
        /// </summary>
        public static void Trigger() { Console.WriteLine("EmailSender initiated"); }

        /// <summary>
        /// Saves the email list in a .txt file
        /// </summary>
        public static void SaveEmails()
        {
            //To avoid possible conflict if 2 updates happen at the same time
            lock (_locker)
            {
                try
                {
                    string toWrite = string.Join("\n", EmailsSender.EmailsBag.ToArray());
                    File.WriteAllText(EmailsSender.EMAILSLIST_PATH, toWrite);
                }
                catch (Exception exception)
                {
                    Console.WriteLine("Error saving emails : " + exception.Message);
                }
            }
        }

        public static void SetSenderToDefault()
        {
            lock (_locker)
            {
                EmailSender = EMAILSENDER_DEFAULT;
                PasswordSender = PASSWORDSENDER_DEFAULT;
            }
            SaveSender();
        }

        public static void ChangeSender(string email, string password)
        {
            lock (_locker)
            {
                EmailSender = email;
                PasswordSender = password;
            }
            SaveSender();
        }

        /// <summary>
        /// Builds an email with the given subject and body then sends it to all the emails in the bag
        /// </summary>
        /// <param name="messageSubject"></param>
        /// <param name="messageBody"></param>
        public static void SendEmailAlert(string messageSubject, string messageBody)
        {
            lock (_locker)
            {
                try
                {
                    foreach (string email in EmailsBag)
                    {
                        MailAddress fromAddress = new MailAddress(EmailSender, "MURPHY SECURITY");
                        MailAddress toAddress = new MailAddress(email, "To User");
                        string fromPassword = PasswordSender;
                        string subject = messageSubject;
                        string body = messageBody;

                        using SmtpClient smtp = new SmtpClient()
                        {
                            Host = "smtp.gmail.com",
                            Port = 587,
                            EnableSsl = true,
                            DeliveryMethod = SmtpDeliveryMethod.Network,
                            Credentials = new NetworkCredential(fromAddress.Address, fromPassword),
                            Timeout = 20000
                        };
                        using MailMessage message = new MailMessage(fromAddress, toAddress)
                        {
                            Subject = subject,
                            Body = body
                        };

                        smtp.Send(message);
                    }
                }
                catch (Exception exception)
                {
                    Console.WriteLine("Error trying to send an email : " + exception.Message);
                }
            }
        }
        #endregion
    }
}
