using System.Collections.Generic;
using System.Net.Mail;

namespace TrackerLibrary
{
	/// <summary>
	/// Represents the functionality for sending an email.
	/// </summary>
	public static class EmailLogic
	{
		/// <summary>
		/// Responsible for gathering email information for the second SendEmail method.
		/// </summary>
		/// <param name="toAddress">The address of the user/team.</param>
		/// <param name="subject">The subject of the email.</param>
		/// <param name="body">The body content of the email.</param>
		public static void SendEmail(string toAddress, string subject, string body)
		{
			SendEmail(new List<string> { toAddress }, new List<string>(), subject, body);
		}

		/// <summary>
		/// Responsible for sending an email to a user/team.
		/// </summary>
		/// <param name="toAddresses">The addresses of the team members.</param>
		/// <param name="bccAddresses">The blind carbon copy addresses.</param>
		/// <param name="subject">The subject of the email.</param>
		/// <param name="body">The body content of the email.</param>
		public static void SendEmail(List<string> toAddresses, List<string> bccAddresses, string subject, string body)
		{
			MailAddress fromMailAddress = new MailAddress(GlobalConfig.AppKeyLookup("senderEmail"), GlobalConfig.AppKeyLookup("senderDisplayName"));

			MailMessage mail = new MailMessage();
			foreach (var toAddress in toAddresses)
			{
				mail.To.Add(toAddress); 
			}
			foreach (var bccAddress in bccAddresses)
			{
				mail.To.Add(bccAddress);
			}
			mail.From = fromMailAddress;
			mail.Subject = subject;
			mail.Body = body;
			mail.IsBodyHtml = true;

			SmtpClient client = new SmtpClient();

			client.Send(mail);
		}
	}
}
