using System;

using MailKit.Net.Smtp;
using MailKit;
using MimeKit;
using System.Net;
using MailKit.Security;
using System.Collections.Generic;
using MailKit.Net.Pop3;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;
using Discord;
using System.Text.RegularExpressions;
using System.Linq;
using NUnit.Framework;
using Discord.WebSocket;
using System.Timers;
using HtmlAgilityPack;

//MailKit documentation http://www.mimekit.net/docs/html/Introduction.htm
//Json.NET documentation https://www.newtonsoft.com/json/help/html/Introduction.htm

/*
 * Email service and everything related to it regularly check for emails,
 * when one is recieved, the program creates a message and sends it to the 
 * Discord section.
 * 
 * The discord section will send a message to a specific server and channel 
 * based on information extraced from the email.
 */


namespace AutomaticAnnouncements
{
	class Program
	{
		//If true, changes announcement channel and removes some of the first emails
		public static readonly bool debug = false;

		private static int numberOfEmailsToCheck = 10;
		private static readonly Timer timer = new Timer(2 * 6 * 10 * 1000) { AutoReset = true, }; //When testing, don't set this to less than 10 sec to avoid annoying things getting on top of each other
		private DiscordSocketClient _client;

		// To avoid checking every every email recieved since the start of time, this stores the x last ones
		// There *is* a risk that too many emails will be recieved at the same time, and some get ignored,
		// but that is extremly unlikely
		private List<EmailMessage> checkedEmails = new List<EmailMessage>(numberOfEmailsToCheck);

		private readonly Stack<Tuple<ulong, string>> messageStack = new Stack<Tuple<ulong, string>>();
		private int checkIndex = 0;

		static void Main() => new Program().MainAsync().GetAwaiter().GetResult();

		// Sets up the email service to recieve and check emails
		public EmailService SetupEmail()
		{
			EmailConfiguration config = ExternalData.GetEmailConfiguration();
			EmailService service = new EmailService(config);
			return service;
		}

		// Sends an email, unused
		//public void SendEmail(EmailService service)
		//{
		//	EmailMessage message = new EmailMessage
		//	{
		//		FromAddresses = new List<EmailAddress>() { new EmailAddress("TorNiklas", "torniklas@outlook.com") },
		//		ToAddresses	  = new List<EmailAddress>() { new EmailAddress("TorNiklas", "torniklas@outlook.com") },
		//		Subject       = "This day is nice",
		//		Content       = "You'd be surprised, it really is."
		//	};
		//	service.Send(message);
		//}

		// Checks for new emails, pushes any new emails to messageStack
		public void StackNewEmailsAsync(EmailService service)
		{
			ulong serverID;
			ulong channelID;
			string message ="";
			string chapterTitle;
			string novelTitle = "";
			string url;
			string mention;

			List<EmailMessage> emails = service.ReceiveLatestEmail(numberOfEmailsToCheck);
			foreach (EmailMessage email in emails)
			{
				if (checkedEmails.Contains(email)) { continue;  }

				string announcements = "Announcements";
				string patreon = "Patreon";

				if(debug)
				{
					announcements = "FAnnouncements";
					patreon = "FPatreon";
				}

				// Filter emails based on sender
				switch (email.FromAddresses[0].Address)
				{
					// Redirected
					case "torniklas@outlook.com":
						if (email.Subject.Contains("Novels by Mecanimus "))
						{
							// All emails from this address are on a certain format, the top code here 
							//is to extrace certain information from those emails
							HtmlDocument doc = new HtmlDocument();
							doc.LoadHtml(email.Content);

							chapterTitle = email.Subject.Split("\"")[1];

							string link = doc.DocumentNode.SelectNodes("//a")[2].Attributes["href"].Value;

							try
							{
								HttpWebRequest req = (HttpWebRequest)WebRequest.Create(link);
								req.AllowAutoRedirect = true;
								var myResp = req.GetResponse(); // This will fail
								url = myResp.ResponseUri.ToString();

								//string author = email.Subject.Split("\"")[0];
								//WebClient client = new WebClient();

								////this doesn't work for some reason; 403 forbidden
								//string jsonString = client.DownloadString("https://www.patreon.com/api/posts?include=user.null%2Caccess_rules.tier.null%2Cattachments.null%2Caudio.null%2Cimages.null%2Cpoll.choices.null%2Cpoll.current_user_responses.null&fields[user]=full_name%2Cimage_url%2Curl&fields[post]=comment_count%2Ccontent%2C%2Ccurrent_user_can_view%2Cembed%2Cimage%2Cis_paid%2Clike_count%2Cmin_cents_pledged_to_view%2Cpatreon_url%2Cpatron_count%2Cpledge_url%2Cpost_file%2Cpost_type%2Cpublished_at%2Cteaser_text%2Ctitle%2Cupgrade_url%2Curl&fields[reward]=[]&fields[access-rule]=access_rule_type%2Camount_cents%2Cpost_count&fields[media]=download_url%2Cimage_urls%2Cmetadata&filter[campaign_id]=3125991&filter[contains_exclusive_posts]=true&filter[is_draft]=false&page[size]=10&sort=-published_at&json-api-use-default-includes=false&json-api-version=1.0");
								//client.Dispose();

								//dynamic content = JObject.Parse(jsonString);
								//JObject attributes = content["data"][0]["attributes"];
								//string id = (string)content["data"][0]["id"];
								//chapterTitle = (string)attributes["title"];
								//url = (string)attributes["url"];
							}
							catch (WebException e)
							{
								url = e.Response.ResponseUri.ToString().Split("?")[0];
							}

							// Chapter title contains novel title, so this is fine
							Regex regex = new Regex(@"^[\w\s]+(?=\s\d+:)");
							Match match = regex.Match(chapterTitle);
							JObject jid;
							if (match.Success) //Is chapter
							{
								novelTitle = match.Value;
								jid = (JObject)ExternalData.GetSection("Novels")[novelTitle];
								if (jid == null)
								{
									Console.WriteLine("Failed for " + novelTitle);
									break;
								}
								serverID = (ulong)jid["Server"];
								channelID = (ulong)jid[patreon];
								mention = (string)jid["PatreonMention"];
								message = MakeMessage(mention, chapterTitle, url);
							}
							else //Is announcement
							{
								//General place for announcements
								jid = (JObject)ExternalData.GetSection("Novels")["A Journey of Black and Red"];
								serverID = (ulong)jid["Server"];
								channelID = (ulong)jid[patreon];
								mention = (string)jid["PatreonMention"];
								message = MakeMessage(mention, chapterTitle, url, "announcement");
							}
							ReportCheckedEmail(email);
							messageStack.Push(new Tuple<ulong, string>(channelID, message));
						}
						break;

					case "noreply@royalroad.com":
						if (email.Subject.StartsWith("New Chapter of "))
					//case "automaticannouncements@gmail.com":
					//	if (email.Subject.Contains("New Chapter of "))
					//case "automaticannouncements@outlook.com":
					//	if (email.Subject.Contains("New Chapter of "))
						{
							try
							{
								// All emails from this address are on a certain format, the top code here 
								//is to extrace certain information from those emails
								HtmlDocument doc = new HtmlDocument();
								doc.LoadHtml(email.Content);

								//Console.WriteLine(doc.DocumentNode.SelectNodes("//a")[3].Attributes["href"].Value);

								//for (int i = 0; i < doc.DocumentNode.SelectNodes("//a").Count; i++)
								//{
								//	Console.WriteLine(i + ": " + doc.DocumentNode.SelectNodes("//a")[i].Attributes["href"].Value);
								//}

								chapterTitle = doc.DocumentNode.SelectNodes("//td")[15].InnerText.Split("\n")[74].Trim();
								string link = doc.DocumentNode.SelectNodes("//a")[1].Attributes["href"].Value;
								novelTitle = email.Subject.Split("New Chapter of ")[1];

								HttpWebRequest req = (HttpWebRequest)WebRequest.Create(link);
								req.AllowAutoRedirect = true;
								HttpWebResponse myResp = (HttpWebResponse)req.GetResponse();
								url = myResp.ResponseUri.ToString();

								JObject jid = (JObject)ExternalData.GetSection("Novels")[novelTitle];
								serverID = (ulong)jid["Server"];
								channelID = (ulong)jid[announcements];
								mention = (string)jid["Mention"];

								// Pushes a message with extracted onformaion to messageStack
								message = MakeMessage(mention, chapterTitle, url);
								ReportCheckedEmail(email);
								messageStack.Push(new Tuple<ulong, string>(channelID, message));
							}
							catch (Exception e)
							{
								Console.WriteLine("Update failed for " + novelTitle);
								Console.WriteLine(e);
							}
						}
						break;

					case "bingo@patreon.com":
						//Console.WriteLine("Mail from patreon");
						//Console.WriteLine(email.Subject);
						//Console.WriteLine(email.Content);
						break;

					default:
						//Console.WriteLine("Unrecognised email adress");
						//Console.WriteLine(email.Subject);
						//Console.WriteLine(email.FromAddresses[0].Address);
						break;
				}
			}
		}

		// Sends stacked messages in messageStack
		public async void PopMessageStack(DiscordSocketClient client) {
			while (messageStack.Count > 0) 
			{
				Tuple<ulong, string> tuple = messageStack.Pop();
				if (tuple == null) continue;
				await (client.GetChannel(tuple.Item1) as IMessageChannel).SendMessageAsync(tuple.Item2);
			}
		}

		//Depricated (I think)
		// Don't want to resend old messages when the system starts,
		// this prevents that
		public void ListOldEmails(EmailService service)
		{
			checkedEmails = service.ReceiveLatestEmail(numberOfEmailsToCheck);

			//Remove some emails. Only for testing purposes
			if (debug)
			{
				int numOfEmailsToRemove = 1;

				var newCheckedEmails = checkedEmails.GetRange(numOfEmailsToRemove, checkedEmails.Count - numOfEmailsToRemove);
				for (int i = 0; i < checkedEmails.Count; i++) 
				{
					if(i < newCheckedEmails.Count)
					{
						checkedEmails[i] = newCheckedEmails[i];
					}
					else
					{
						checkedEmails[i] = null;
					}
				}
			}

			checkedEmails.Reverse();
		}

		// When at the end of the list, start at the beginning again and overwrite the oldest one
		public void ReportCheckedEmail(EmailMessage email)
		{
			checkedEmails[checkIndex] = email;
			checkIndex = ++checkIndex % numberOfEmailsToCheck;
		}

		// Creates the message to be sent
		public string MakeMessage(string mention, string chapterTitle, string url, string chapterType = "chapter")
		{
			return mention + "\nHEAR YE, HEAR YE!\nNew " + chapterType + ":** " + chapterTitle + "\n**" + url;
		}



		private Task Log(LogMessage msg)
		{
			Console.WriteLine(msg.ToString());
			return Task.CompletedTask;
		}

		// Sets up and starts everything, making sure the program check for emails regularly
		private async Task MainAsync()
		{
			string token = (string)ExternalData.GetSection("Discord")["Token"];
			DiscordSocketConfig _config = new DiscordSocketConfig { MessageCacheSize = 100 };
			_client = new DiscordSocketClient(_config);
			_client.Log += Log;

			var service = SetupEmail();
			ListOldEmails(service);
			timer.Elapsed += (source, e) =>
			{
				StackNewEmailsAsync(service);
				PopMessageStack(_client);
				Console.WriteLine("Last pop: " + e.SignalTime);
			};

			await _client.LoginAsync(TokenType.Bot, token);
			await _client.StartAsync();

			timer.Enabled = true;
			await Task.Delay(-1);
			
		}
	}
}

