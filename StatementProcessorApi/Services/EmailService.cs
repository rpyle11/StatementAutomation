using Microsoft.Extensions.Options;
using StatementProcessorApi.Models;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using System.Runtime;
using System.Text.RegularExpressions;

namespace StatementProcessorApi.Services
{
    public class EmailService(IOptions<AppSettings> settings, ILogService logService)
        : IEmailService
    {
       

        public async Task<bool> SendMessage(EmailFields emData, string? appUser)
        {
            try
            {
                var message = new MailMessage();
                var client = new SmtpClient();


                //set sender's address
                if (!string.IsNullOrEmpty(emData.FromAddress))
                {
                    if (settings.Value.AppLogFromEmail != null)
                        message.From = VerifyEmailAddress(emData.FromAddress)
                            ? new MailAddress(emData.FromAddress)
                            : new MailAddress(settings.Value.AppLogFromEmail);
                }
                else
                {
                    if (settings.Value.AppLogFromEmail != null) message.From = new MailAddress(settings.Value.AppLogFromEmail);
                }



                //allow multiple "to" address
                if (!string.IsNullOrWhiteSpace(emData.ToAddress))
                {
                    if (settings.Value.EmailAddrSep != null)
                        foreach (var address in emData.ToAddress.Split(char.Parse(settings.Value.EmailAddrSep)))
                        {
                            if (VerifyEmailAddress(address))
                            {
                                message.To.Add(new MailAddress(address));
                            }
                            else
                                throw new FormatException($"Invalid To email address {address}");
                        }
                }

                //allow multiple "CC" address
                if (!string.IsNullOrWhiteSpace(emData.CcAddress))
                {
                    if (settings.Value.EmailAddrSep != null)
                        foreach (var address in emData.CcAddress.Split(char.Parse(settings.Value.EmailAddrSep)))
                        {
                            if (VerifyEmailAddress(address))
                            {
                                message.CC.Add(new MailAddress(address));
                            }

                            else
                                throw new FormatException($"Invalid CC email address {address}");
                        }
                }

                if (emData.Attachments is { Count: > 0 })
                {
                    foreach (var attachment in emData.Attachments)
                    {
                        message.Attachments.Add(new Attachment(attachment!));
                    }

                }

                message.IsBodyHtml = emData.UseHtml;
                message.Subject = emData.Subject;
                message.Body = emData.MessageBody;

                if (settings.Value.SmtpServer != null) client.Host = settings.Value.SmtpServer;
                if (settings.Value.EmailAuthenticate)
                    client.Credentials = CredentialCache.DefaultNetworkCredentials;

                await client.SendMailAsync(message);

                message.Dispose();
                client.Dispose();


                return true;
            }
            catch (Exception ex)
            {

                var logMsg = $"Error: {ex.Message}";

                if (ex.InnerException != null && !string.IsNullOrEmpty(ex.InnerException.Message))
                {
                    logMsg += $"Inner Message {ex.InnerException.Message}";
                }

                await logService.LogAlert(new AppLog
                {
                    AppUser = appUser,
                    LogMsg =
                        $"{ex.GetType().Name} Error in {MethodName.GetMethodName(MethodBase.GetCurrentMethod())}, {logMsg}",
                    MessageType = AppLog.MessageTypeEnum.Error,
                    SendEmail = true,
                });
            }

            return false;

        }


        private bool VerifyEmailAddress(string emailAddress)
        {
            if (emailAddress.Length <= 0) return false;
            if (settings.Value.EmailRegExp == null) return false;
            var checkMatch = Regex.Match(emailAddress, settings.Value.EmailRegExp);

            return checkMatch.Success;

        }
    }
}
