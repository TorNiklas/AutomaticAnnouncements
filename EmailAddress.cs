using System;
using System.Collections.Generic;
using System.Text;

namespace AutomaticAnnouncements
{
	public class EmailAddress
	{
		public string Name { get; set; }
		public string Address { get; set; }

		public EmailAddress() { }
		public EmailAddress(string name, string adress) {
			Name = name;
			Address = adress;
		}

		public static bool operator ==(EmailAddress obj1, EmailAddress obj2)
		{
			if (ReferenceEquals(obj1, obj2))
			{
				return true;
			}

			if (obj1 is null)
			{
				return false;
			}
			if (obj2 is null)
			{
				return false;
			}

			return   (obj1.Name == obj2.Name
				   && obj1.Address == obj2.Address);
		}

		// this is second one '!='
		public static bool operator !=(EmailAddress obj1, EmailAddress obj2)
		{
			return !(obj1 == obj2);
		}

		public bool Equals(EmailAddress other)
		{
			if (other is null)
			{
				return false;
			}
			if (ReferenceEquals(this, other))
			{
				return true;
			}

			return    Name.Equals(other.Name)
				   && Address.Equals(other.Address);
		}

		public override bool Equals(object obj)
		{
			if (obj is null)
			{
				return false;
			}
			return ReferenceEquals(this, obj) ? true : obj.GetType() == GetType() && Equals((EmailAddress)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = Name.GetHashCode();
				hashCode = (hashCode * 397) ^ Address.GetHashCode();
				return hashCode;
			}
		}
	}
}
