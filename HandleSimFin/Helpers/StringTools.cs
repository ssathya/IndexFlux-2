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
			MD5CryptoServiceProvider objHashMD5 = new MD5CryptoServiceProvider();

			var txtArray = Encoding.ASCII.GetBytes(text);
			return Convert.ToBase64String(new TripleDESCryptoServiceProvider
			{
				Key = objHashMD5.ComputeHash(Encoding.ASCII.GetBytes(topSecret)),
				Mode = CipherMode.ECB
			}.CreateEncryptor()
				.TransformFinalBlock(txtArray, 0, txtArray.Length));
		}

		public static string Derypt(this string text)
		{
			try
			{
				MD5CryptoServiceProvider objHashMD5 = new MD5CryptoServiceProvider();
				var byteHash = objHashMD5.ComputeHash(Encoding.ASCII.GetBytes(topSecret));

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
				return num.ToString("0,,,.###B", CultureInfo.InvariantCulture);
			}
			else
			if (num > 999999 || num < -999999)
			{
				return num.ToString("0,,.##M", CultureInfo.InvariantCulture);
			}
			else
			if (num > 999 || num < -999)
			{
				return num.ToString("0,.#K", CultureInfo.InvariantCulture);
			}
			else
			{
				return num.ToString(CultureInfo.InvariantCulture);
			}
		}

		#endregion Public Methods
	}
}