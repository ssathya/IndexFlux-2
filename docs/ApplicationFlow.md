
# Application flow
We have installed all the necessary software on to a server that will be used with our application. This will be a good time to talk about our application.

Let us take a simple flow; user requests the quote for a company, say Oracle.
![Quote Request Flow](https://ssathya.github.io/IndexFlux-2/docs/QuoteRequest.png)

The above is a simple request that shows how a request is sent to WEB API application (Server application) when a request is made.

A user starts his session with Index Flux by saying the keyword "Hey Google! Talk to Index Flux" and the session starts with the welcome message. Say the user is interested in Oracle's quote and requests for its quote by saying "How is Oracle doing today?" Dialogflow, the tool that is used to develop the conversation with user, parses the user query and invokes the appropriate intent. For this application an intent called "stockQuote" is invoked that understands phrases like "How is Microsoft doing?", "Get me the stock quote for Google", "What is the latest price for Citigroup", etc.  A handful of phrases have been programmed to identify your query and pickes proper noun in your phrase and marks them as the requested parameter. 

All intents, that fetch external data, in this application will invoke a Webhook request - our Server application. The fulfilment request sent by Dialogflow has many values packed into a JSON document and we'll use the values in this JSON document to fulfill the request.

The application serves mutiple intents namely Get Fundamentals for a firm, Get Market summary, Stock quote, get News, and randomly select firms with good fundamentals. By design DialogFlow routes all requests to a single API call to our application. To maintain modularity the Server application on getting a POST webhook request identifies the intent and calls another API within the application, as if the request is coming from an external service, which actuall processes the message and returns the requested value.
![Call Sequence](https://ssathya.github.io/IndexFlux-2/docs/CallSequence.png)

As mentioned above we'll go over only the Quotes and other sequences are processed similarly.

When Dialogflow calls our application it enters the Server Application via API ***IntendsHandler*** which calls a private method  ExecuteKnownValues: It goes as follows:

```cs
private IActionResult ExecuteKnownValues(GoogleCloudDialogflowV2WebhookRequest value)
		{
			WebhookResponse returnValue = null;
			var baseURL = Request.Host.ToString();
			var keyUsedToAccess = Request.Headers["key"].ToString();

			string intendDisplayName = value.QueryResult.Intent.DisplayName;
			var client = new RestClient("https://" + baseURL);
			var actionLink = BuildActionMethod(intendDisplayName);
			if (string.IsNullOrWhiteSpace(actionLink))
			{
				returnValue = StdErrorMessageGenerator();
			}
			else
			{
				var request = new RestRequest(actionLink, Method.POST);
				request.AddHeader("key", keyUsedToAccess);
				request.AddJsonBody(value);
				var response = client.Execute(request);
				if (response.IsSuccessful)
				{
					var returnMsg = response.Content;
					return new ContentResult
					{
						Content = returnMsg,
						ContentType = "application/json",
						StatusCode = 200
					};
				}
				else
				{
					returnValue = StdErrorMessageGenerator();
				}
			}
			var responseString = returnValue.ToString();
			_log.LogTrace("Completed processing request");
			return new ContentResult
			{
				Content = responseString,
				ContentType = "application/json",
				StatusCode = 200
			};
		}
```
I'll not go into nuts and bolts of the above code but in essence the following happens:

 1. Obtain Intent name
 2. Determine which API to be called based on Intent name
 3. Call the API as if its being called by an external system.
 4. Send back the response; the response returned by the API calls are WebhookResponse. Convert them into ContentResult and back to Dialogflow.

For this specific case, Quotes, the API method */api/StockQuote* will be called and let us see how it is handled.

When the user requests the stock quote for Oracle the user might request "Get me the stock quote for Oracle" or "Get me the stock quote for ORCL"; or "Get me the quote for MSFT" or "Get me the quote for Microsoft". The application first tries to do a unique match on ticker and if it fails a wildcard match on name is made.  We get the quotes from [World Trading](https://www.worldtradingdata.com/) parse the JSON values returned and convert to a string using the code below that can be sent back to user.
```cs
private string BuildOutputMsg(QuotesFromWorldTrading quotes)
		{
			if (quotes.Data.Length == 0)
			{
				return "Could not resolve requested firm or ticker";
			}
			var tmpStr = new StringBuilder();
			DateTime dateToUse = quotes.Data[0].Last_trade_time != null ? (DateTime)quotes.Data[0].Last_trade_time : DateTime.Parse("01-01-2000");

			tmpStr.Append($"As of {dateToUse.ToString("MMMM dd, hh:mm tt")} EST ");
			foreach (var quote in quotes.Data)
			{
				float price = quote.Price != null ? (float)quote.Price : 0;
				quote.Day_change = quote.Day_change == null ? 0 : quote.Day_change;
				var symbol = Regex.Replace(quote.Symbol, ".{1}", "$0 ");
				tmpStr.Append($"{quote.Name} with ticker {symbol} was traded at {price.ToString("c2")}.");
				tmpStr.Append(quote.Day_change > 0 ? " Up by " : " Down by ");
				tmpStr.Append($"{((float)quote.Day_change).ToString("n2")} points.\n\n ");
			}
			return tmpStr.ToString();
		}
```

Note: Instead of reporting the delta in percent, as the native stock quote does, the application reports in actual points.

The message built is packeted as a *WebhookResponse* and returned to the calling API call and back to the user.



