
# Application flow
We have installed all the necessary software on to a server that will be used with our application. This will be a good time to talk about our application.

Let us take a simple flow; user requests the quote for a company, say Oracle.
![Quote Request Flow](https://ssathya.github.io/IndexFlux-2/docs/QuoteRequest.png)

The above is a simple request that shows how a request is sent to WEB API application (Server application) when a request is made.

A user starts his session with Index Flux by saying the keyword "Hey Google! Talk to Index Flux" and the session starts with the welcome message. Say the user is interested in Oracle's quote and requests for its quote by saying "How is Oracle doing today?" Dialogflow, the tool that is used to develop the conversation with user, parses the user query and invokes the appropriate intent. For this application an intent called "stockQuote" is invoked that understands phrases like "How is Microsoft doing?", "Get me the stock quote for Google", "What is the latest price for Citigroup", etc.  A handful of phrases have been programmed to identify your query and pickes proper noun in your phrase and marks them as the requested parameter. 

All intents, that fetch external data, in this application will invoke a Webhook request - our Server application. The fulfilment request sent by Dialogflow has many values packed into a JSON document and we'll use the values in this JSON document to fulfill the request.

The application serves mutiple intents namely Get Fundamentals for a firm, Get Market summary, Stock quote, get News, and randomly select firms with good fundamentals. By design DialogFlow routes all requests to a single API call to our application. To maintain modularity the Server application on getting a POST webhook request identifies the intent and calls another API within the application, as if the request is coming from an external service, which actuall processes the message and returns the requested value.
![Call Sequence](https://ssathya.github.io/IndexFlux-2/docs/CallSequence.png)
