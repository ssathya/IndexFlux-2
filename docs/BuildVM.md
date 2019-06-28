# Hardware selection

Selection of hardware would seem to look a solution that was used during the 90s but considering total cost and usage
 I had to select a VM that hosts the database as well. 

Ideally one would have selected services like Lambda or Azure functions to serve Google Actions requests 
but the application needs a database as well. 

Serverless technology would have been ideal and actually would have saved me from writing this document 
as well but for an application that is infrequently used serverless would actually become a hassle. For the first release, 
I had used Azure functions but cold start was too long that Google Actions would often time-out or the first response would 
take more than 4 to 5 seconds which will not be acceptable for everyday use.

Once the decision to run on a dedicated server was made it was obvious that the hardware will be under-utilized 
and decided to host the database as well on the virtual machine. 

## Hardware selected
As the usage for the application is very low I decided to go with the smallest hardware that Google offers: 
f1-micro. Initially I wanted to use AWS t3-small but comparing the costs I did the initial release on AWS Lightsail 
machine. T3 machine and Lightsail  costs almost identical but Lightsail gives storage for the same $$ and decided 
to go with Lightsail. One big limitation I saw with Lightsail was AWS IAM policies cannot be applied on Lightsail. 

Soon after I released the V2 of Indexflux I realized Google offers one instance of f1-micro for free 
and decided on moving to GCP.