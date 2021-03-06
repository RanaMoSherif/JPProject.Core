﻿using System.Threading.Tasks;
using JPProject.Sso.Application.Extensions;
using JPProject.Sso.Domain.Interfaces;
using JPProject.Sso.Domain.ViewModels.User;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Logging;
using MimeKit;
using MimeKit.Text;
namespace JPProject.Sso.Application.CloudServices.Email
{
    public class GeneralSmtpService : IEmailService
    {
        private readonly ILogger<GeneralSmtpService> _logger;
        private readonly IGlobalConfigurationSettingsService _globalConfigurationSettingsService;

        public GeneralSmtpService(ILogger<GeneralSmtpService> logger, IGlobalConfigurationSettingsService globalConfigurationSettingsService)
        {
            _logger = logger;
            _globalConfigurationSettingsService = globalConfigurationSettingsService;
        }

        public async Task SendEmailAsync(EmailMessage message)
        {
            var emailConfiguration = await _globalConfigurationSettingsService.GetPrivateSettings();

            if (emailConfiguration == null || !emailConfiguration.SendEmail)
                return;

            var mimeMessage = new MimeMessage();
            mimeMessage.To.Add(new MailboxAddress(message.Email));
            mimeMessage.From.Add(new MailboxAddress(message.Sender.Name, message.Sender.Address));

            if (message.Bcc != null && message.Bcc.IsValid())
                mimeMessage.To.AddRange(message.Bcc.Recipients.ToMailboxAddress());

            mimeMessage.Subject = message.Subject;
            //We will say we are sending HTML. But there are options for plaintext etc. 
            mimeMessage.Body = new TextPart(TextFormat.Html)
            {
                Text = message.Content
            };

            //Be careful that the SmtpClient class is the one from Mailkit not the framework!
            _logger.LogInformation($"Sending e-mail to {message.Email}");
            using (var client = new SmtpClient())
            {
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;

                client.Connect(emailConfiguration.Smtp.Server, emailConfiguration.Smtp.Port, emailConfiguration.Smtp.UseSsl);
                client.AuthenticationMechanisms.Remove("XOAUTH2");
                client.Authenticate(emailConfiguration.Smtp.Username, emailConfiguration.Smtp.Password);

                await client.SendAsync(mimeMessage);
                client.Disconnect(true);
            }
            _logger.LogInformation($"E-mail to {message.Email}: sent!");
        }
    }
}