# Markdown File

```mermaid
sequenceDiagram
User ->> Dialog Flow: How is Oracle doing today?
Dialog Flow -->> Web API: Get me the quote for Oracle?
Web API -->> External Service: Details of Oracle's last trade
External Service --x Web API: Latest trade details
Web API --x Dialog Flow: Oracle's latest price
Note left of Web API: Return value or<br/> error message
Dialog Flow -x User: Text to speach
```
