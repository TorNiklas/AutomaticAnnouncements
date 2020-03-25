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
		//void Send(EmailMessage emailMessage);
		//List<EmailMessage> ReceiveEmail(int maxCount = 10);
		List<EmailMessage> ReceiveLatestEmail(int maxCount = 10);
	}

	// Everything to do with the emails
	public class EmailService : IEmailService
	{
		private readonly IEmailConfiguration _emailConfiguration;

		public EmailService(IEmailConfiguration emailConfiguration)
		{
			_emailConfiguration = emailConfiguration;
		}

		// Function for sending emails, currently unused
		//public void Send(EmailMessage emailMessage)
		//{
		//	var message = new MimeMessage();
		//	message.To.AddRange(emailMessage.ToAddresses.Select(x => new MailboxAddress(x.Name, x.Address)));
		//	message.From.AddRange(emailMessage.FromAddresses.Select(x => new MailboxAddress(x.Name, x.Address)));

		//	message.Subject = emailMessage.Subject;			message.Body = new TextPart(TextFormat.Html)
		//	{
		//		Text = emailMessage.Content
		//	};

		//	using SmtpClient emailClient = new SmtpClient();

		//	//The last parameter is to use SSL 
		//	emailClient.Connect(_emailConfiguration.SmtpServer, _emailConfiguration.SmtpPort, true);

		//	//Remove OAuth functionality as it's not needed
		//	emailClient.AuthenticationMechanisms.Remove("XOAUTH2");

		//	emailClient.Authenticate(_emailConfiguration.SmtpUsername, _emailConfiguration.SmtpPassword);
		//	emailClient.Send(message);
		//	emailClient.Disconnect(true);
		//}

		// Retrieves the least recent emails, currently unsused
		//public List<EmailMessage> ReceiveEmail(int maxCount = 10)
		//{
		//	using Pop3Client emailClient = new Pop3Client();
		//	emailClient.Connect(_emailConfiguration.PopServer, _emailConfiguration.PopPort, true);

		//	//Remove any OAuth functionality, then authenticate 
		//	emailClient.AuthenticationMechanisms.Remove("XOAUTH2");
		//	emailClient.Authenticate(_emailConfiguration.PopUsername, _emailConfiguration.PopPassword);

		//	List<EmailMessage> emails = new List<EmailMessage>();
		//	for (int i = 0; i < emailClient.Count && i < maxCount; i++)
		//	{
		//		var message = emailClient.GetMessage(i);
		//		var emailMessage = new EmailMessage
		//		{
		//			Content = !string.IsNullOrEmpty(message.HtmlBody) ? message.HtmlBody : message.TextBody,
		//			Subject = message.Subject
		//		};
		//		emailMessage.ToAddresses.AddRange(message.To.Select(x => (MailboxAddress)x).Select(x => new EmailAddress { Address = x.Address, Name = x.Name }));
		//		emailMessage.FromAddresses.AddRange(message.From.Select(x => (MailboxAddress)x).Select(x => new EmailAddress { Address = x.Address, Name = x.Name }));
		//		emails.Add(emailMessage);
		//	}

		//	return emails;
		//}

		// Retrieves the most recent emails
		public List<EmailMessage> ReceiveLatestEmail(int maxCount = 10)
		{
			using Pop3Client emailClient = new Pop3Client();
			emailClient.Connect(_emailConfiguration.PopServer, _emailConfiguration.PopPort, true);

			//Remove any OAuth functionality, then authenticate 
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
	}
}
