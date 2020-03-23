using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AutomaticAnnouncements
{
	static class ExternalData
	{
		private readonly static string path = "..\\..\\..\\config.json";
		public static JToken GetSection(string section)
		{
			JObject json;
			using (StreamReader r = new StreamReader(path))
			{
				json = JObject.Parse(r.ReadToEnd());
			}
			return json[section];
		}

		public static JToken GetSubSection(JToken section, string subSection)
		{
			return section[subSection];
		}

		public static EmailConfiguration GetEmailConfiguration() 
		{
			JObject json;
			using (StreamReader r = new StreamReader(path))
			{
				json = JObject.Parse(r.ReadToEnd());
			}

			EmailConfiguration config = new EmailConfiguration();

			JToken section = json["EmailConfiguration"];
			config.SmtpServer   = (string)section["SmtpServer"];
			config.SmtpPort     =    (int)section["SmtpPort"];
			config.SmtpUsername = (string)section["SmtpUsername"];
			config.SmtpPassword = (string)section["SmtpPassword"];
			config.PopServer    = (string)section["PopServer"];
			config.PopPort      =    (int)section["PopPort"];
			config.PopUsername  = (string)section["PopUsername"];
			config.PopPassword  = (string)section["PopPassword"];
			return config;
		}
	}
}
