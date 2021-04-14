using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SearchEngine.Controllers
{
    public class FeedbackController : Controller
    {
        public MessageSenderOptions Options { get; }

        public FeedbackController(IOptions<MessageSenderOptions> optionsAccessor)
        {
            Options = optionsAccessor.Value;
        }

        [HttpPost]
        public IActionResult Index(string message, string url)
        {
            if (message.Length < 20)
                return Content("Please enter at least 20 characters");

            var clientIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            if(Sources.RequestTime.ContainsKey(clientIpAddress) && (DateTime.Now - Sources.RequestTime[clientIpAddress]).TotalSeconds < 30)
                return Content($"Please wait {30 - (int)((DateTime.Now - Sources.RequestTime[clientIpAddress]).TotalSeconds)} seconds");
            Sources.RequestTime[clientIpAddress] = DateTime.Now;

            SendEmailAsync("Docs feedback", $"Docs feedback:<br/>{message}<br/><br/>{url}");
            return Content("Thank you for your feedback!");
        }

        private Task SendEmailAsync(string subject, string htmlMessage)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(Options.FromDisplayName, Options.FromAddress));
            message.To.Add(new MailboxAddress(Options.ToAddress, Options.ToAddress));
            message.Subject = subject;
            message.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = htmlMessage
            };
            using var client = new SmtpClient();
            client.Connect(Options.Host, Options.Port, false);
            client.Authenticate(Options.FromAddress, Options.EmailPassword);
            client.Send(message);
            client.Disconnect(true);
            return Task.Delay(0);
        }
    }
}
