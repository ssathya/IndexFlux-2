using System;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace HandleSimFin.Helpers
{
	public static class StringTools
	{

		#region Private Fields

		private static readonly string topSecret = "My name is Bond; James Bond!";

		#endregion Private Fields


		#region Public Methods

		public static string Crypt(this string text)
		{
			
			using (var objHashMD5 = new MD5CryptoServiceProvider())
			{
				var txtArray = Encoding.ASCII.GetBytes(text);
				return Convert.ToBase64String(new TripleDESCryptoServiceProvider
				{
					Key = objHashMD5.ComputeHash(Encoding.ASCII.GetBytes(topSecret)),
					Mode = CipherMode.ECB
				}.CreateEncryptor()
					.TransformFinalBlock(txtArray, 0, txtArray.Length));
			}

		}

		public static string Derypt(this string text)
		{
			try
			{
				byte[] byteHash;
				using (var objHashMD5 = new MD5CryptoServiceProvider())
				{
					byteHash = objHashMD5.ComputeHash(Encoding.ASCII.GetBytes(topSecret));
				}
				

				return Encoding.ASCII.GetString(new TripleDESCryptoServiceProvider
				{
					Key = byteHash,
					Mode = CipherMode.ECB
				}.CreateDecryptor().TransformFinalBlock(Convert.FromBase64String(text), 0, Convert.FromBase64String(text).Length));
			}
			catch (Exception ex)
			{
				Console.WriteLine("Data decryption failed");
				Console.WriteLine(ex.Message);
			}
			return null;
		}

		public static bool IsNullOrWhiteSpace(this string value)
		{
			return string.IsNullOrWhiteSpace(value);
		}

		public static string ReplaceFirst(this string text, string search, string replace)
		{
			int pos = text.IndexOf(search);
			if (pos < 0)
			{
				return text;
			}
			return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
		}
		public static string ToKMB(this decimal num)
		{
			if (num > 999999999 || num < -999999999)
			{
				return num.ToString("0,,,.### Billions", CultureInfo.InvariantCulture);
			}
			else
			if (num > 999999 || num < -999999)
			{
				return num.ToString("0,,.## Millions", CultureInfo.InvariantCulture);
			}
			else
			if (num > 999 || num < -999)
			{
				return num.ToString("0,.# Thousands", CultureInfo.InvariantCulture);
			}
			else
			{
				return num.ToString(CultureInfo.InvariantCulture);
			}
		}
		public static string TruncateAtWord(this string value, int length)
		{
			if (value == null || value.Length < length || value.IndexOf(" ", length) == -1)
				return value;

			return value.Substring(0, value.IndexOf(" ", length));
		}

		#endregion Public Methods
	}
}