using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutomaticAnnouncements
{
	public class EmailMessage
	{
		public EmailMessage()
		{
			ToAddresses = new List<EmailAddress>();
			FromAddresses = new List<EmailAddress>();
		}

		public List<EmailAddress> ToAddresses { get; set; }
		public List<EmailAddress> FromAddresses { get; set; }
		public string Subject { get; set; }
		public string Content { get; set; }

		public static bool operator ==(EmailMessage obj1, EmailMessage obj2)
		{
			if (ReferenceEquals(obj1, obj2)) { return true; }

			if (obj1 is null || obj2 is null) { return false; }

			return  obj1.ToAddresses.All(obj2.ToAddresses.Contains)
					&& obj1.FromAddresses.All(obj2.FromAddresses.Contains)
					&& obj1.Subject == obj2.Subject
					&& obj1.Content == obj2.Content;
		}

		// this is second one '!='
		public static bool operator !=(EmailMessage obj1, EmailMessage obj2)
		{
			return !(obj1 == obj2);
		}

		public bool Equals(EmailMessage other)
		{
			if (other is null) { return false; }
			if (ReferenceEquals(this, other)) { return true; }

			return	ToAddresses.All(other.ToAddresses.Contains)
					&& FromAddresses.All(other.FromAddresses.Contains)
					&& Subject == other.Subject
					&& Content == other.Content;
		}

		public override bool Equals(object obj)
		{
			if (obj is null) { return false; }

			return ReferenceEquals(this, obj) ? true : obj.GetType() == GetType() && Equals((EmailMessage)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = ToAddresses.GetHashCode();
				hashCode = (hashCode * 397) ^ FromAddresses.GetHashCode();
				hashCode = (hashCode * 397) ^ Subject.GetHashCode();
				hashCode = (hashCode * 397) ^ Content.GetHashCode();
				return hashCode;
			}
		}
		public override string ToString()
		{
			return "Subject: " + Subject;
		}
	}
}
