using System;
using System.Collections.Generic;
using System.Text;
using MailKit.Net.Smtp;
using MailKit;
using MimeKit;
using System.Linq;
using MimeKit.Text;
using MailKit.Net.Pop3;

namespace AutomaticAnnouncements
{
	public interface IEmailService
	{
		void Send(EmailMessage emailMessage);
		List<EmailMessage> ReceiveEmail(int maxCount = 10);
	}

	public class EmailService : IEmailService
	{
		private readonly IEmailConfiguration _emailConfiguration;

		public EmailService(IEmailConfiguration emailConfiguration)
		{
			_emailConfiguration = emailConfiguration;
		}

		public void Send(EmailMessage emailMessage)
		{
			var message = new MimeMessage();
			message.To.AddRange(emailMessage.ToAddresses.Select(x => new MailboxAddress(x.Name, x.Address)));
			message.From.AddRange(emailMessage.FromAddresses.Select(x => new MailboxAddress(x.Name, x.Address)));

			message.Subject = emailMessage.Subject;
			//We will say we are sending HTML. But there are options for plaintext etc. 
			message.Body = new TextPart(TextFormat.Html)
			{
				Text = emailMessage.Content
			};

			//Be careful that the SmtpClient class is the one from Mailkit not the framework!
			using SmtpClient emailClient = new SmtpClient();
			//The last parameter here is to use SSL (Which you should!)
			emailClient.Connect(_emailConfiguration.SmtpServer, _emailConfiguration.SmtpPort, true);

			//Remove any OAuth functionality as we won't be using it. 
			emailClient.AuthenticationMechanisms.Remove("XOAUTH2");

			emailClient.Authenticate(_emailConfiguration.SmtpUsername, _emailConfiguration.SmtpPassword);

			emailClient.Send(message);

			emailClient.Disconnect(true);

		}

		public List<EmailMessage> ReceiveEmail(int maxCount = 10)
		{
			using Pop3Client emailClient = new Pop3Client();
			emailClient.Connect(_emailConfiguration.PopServer, _emailConfiguration.PopPort, true);

			emailClient.AuthenticationMechanisms.Remove("XOAUTH2");

			emailClient.Authenticate(_emailConfiguration.PopUsername, _emailConfiguration.PopPassword);

			List<EmailMessage> emails = new List<EmailMessage>();
			for (int i = 0; i < emailClient.Count && i < maxCount; i++)
			{
				var message = emailClient.GetMessage(i);
				var emailMessage = new EmailMessage
				{
					Content = !string.IsNullOrEmpty(message.HtmlBody) ? message.HtmlBody : message.TextBody,
					Subject = message.Subject
				};
				emailMessage.ToAddresses.AddRange(message.To.Select(x => (MailboxAddress)x).Select(x => new EmailAddress { Address = x.Address, Name = x.Name }));
				emailMessage.FromAddresses.AddRange(message.From.Select(x => (MailboxAddress)x).Select(x => new EmailAddress { Address = x.Address, Name = x.Name }));
				emails.Add(emailMessage);
			}

			return emails;
		}

		public List<EmailMessage> ReceiveLatestEmail(int maxCount = 10)
		{
			using Pop3Client emailClient = new Pop3Client();
			emailClient.Connect(_emailConfiguration.PopServer, _emailConfiguration.PopPort, true);

			emailClient.AuthenticationMechanisms.Remove("XOAUTH2");

			emailClient.Authenticate(_emailConfiguration.PopUsername, _emailConfiguration.PopPassword);

			List<EmailMessage> emails = new List<EmailMessage>();
			for (int i = emailClient.Count - 1; i >= 0 && i >= emailClient.Count - maxCount; i--)
			{
				var message = emailClient.GetMessage(i);
				var emailMessage = new EmailMessage
				{
					Content = !string.IsNullOrEmpty(message.HtmlBody) ? message.HtmlBody : message.TextBody,
					Subject = message.Subject
				};
				emailMessage.ToAddresses.AddRange(message.To.Select(x => (MailboxAddress)x).Select(x => new EmailAddress { Address = x.Address, Name = x.Name }));
				emailMessage.FromAddresses.AddRange(message.From.Select(x => (MailboxAddress)x).Select(x => new EmailAddress { Address = x.Address, Name = x.Name }));
				emails.Add(emailMessage);
			}

			return emails;
		}

		//public void ConfigureServices(IServiceCollection services)
		//{
		//	services.AddMvc();
		//	services.AddSingleton<IEmailConfiguration>(Configuration.GetSection("EmailConfiguration").Get<EmailConfiguration>());
		//}
	}
}
