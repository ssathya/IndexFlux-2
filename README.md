# Implementation of Index Flux
Index Flux is an action on Google assistant that gets market data (U.S. stock market for now); it sounds like the native task 
that is inbuilt within assistant but Index Flux can do much more.

-   **Get indices values** => Reports about Dow 30, Nasdaq 100 and S&P 500 movements.
-   **Get last price for any listed stock.**
-   **Get fundamentals analysis**, around 2,300 for now, stocks.
-   **Randomly select** 4 **stocks** that have **good fundamentals that you can research** before investing.
-   News headlines from CNBC, Wall Street Journal and couple other news sources.

The application is self-hosted in a VM (considering the cost of DB and wakeup time of functions) and installation 
procedure, though simple, has many steps and is easy to miss. I'm documenting the steps involved in building the server 
and also touch upon how the actual application functions.

[Hardware selection](/docs/BuildVM.md)
