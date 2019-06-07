using Google.Cloud.Dialogflow.V2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Google.Cloud.Dialogflow.V2.Intent.Types;

namespace DataProvider.Extensions
{
    public static class Utilities
	{
		public static string ConvertToSSML(string unformatedMsg)
		{
			StringBuilder tempValue = new StringBuilder();
			tempValue.Append("<speak>");
			tempValue.Append(unformatedMsg);
			tempValue.Append("</speak>");
			tempValue.Replace("\r", "");
			tempValue.Replace("\n\n", "\n");
			tempValue.Replace("\n", @"<break strength='x - strong' time='500ms' />");
			return tempValue.ToString();
		}
		public static void PlaceStandardHeaders(WebhookResponse returnValue)
		{

			var platform = new Message
			{
				Platform = Message.Types.Platform.ActionsOnGoogle
			};
			// returnValue.FulfillmentText = "";
			returnValue.FulfillmentMessages.Add(platform);
		}
		public static string EndOfCurrentRequest()
		{
			return "\nAnything more? For help say help or say 'bye bye' to quit\n";
		}
		public static string ErrorReturnMsg()
		{
			return "\nWe are experiencing public issues; be assured we'll resolve as soon as possible\n" +
				EndOfCurrentRequest();
		}
	}
}
