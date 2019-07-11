# Installation of Nginx and web components
## Introduction
This, at least for me, is the most tricky infrastructure for this application. Though every piece is well documented I did not stumble upone one document that integreated Nginx, SSL certificate from Let’s Encrypt, and web api reverse proxy setup for nginx.

This document lists the steps that ware taken to install the above mentioned stack.

## Install Nginx
Installation of nginx on Ubuntu server is a one line command.
```sh
	sudo apt update && sudo apt -y upgrade && sudo apt install nginx
```
*Well I cheated; it was a 3 line command but yet simple and stright forward.*

Make sure you your nginx installation is working but going to 'http://yourserver-ip-address' and  you should see the default nginx page rendered.

Also before going further let's make note of the verison of nginx installed.
```sh
nginx -v
```
As I'm running Ubuntu 18.04 LTS I got the following response:
```
nginx version: nginx/1.14.0 (Ubuntu)
```

## Obtain a static IP address for your server
This procedure would depend on your cloud provider. Follow the steps that are given by your cloud provider and ensure your server has a static IP address before you proceed further. Ensure you have a static IP address; shutdown the server, wait for a minute or so and start the server. Your server should be accessable with the same IP address you did before you shut it down.

## Obtain an DNS name
I'm not sure if all cloud providers provide a default DNS name for your VM instances and for this application, if your cloud provider provides a generic DNS name, I believe it can be used; however, I have not tested using the default DNS name.

We can get a free DNS name  from [Feenom](https://www.freenom.com) (*free for a limited time*) or from [Duck DNS](http://duckdns.org/) (*sub domain did not try this*) and register our server to use the domain name we select. I'm not going to go into the details how we do this as there are many online tutorials how to use Freenom and Duck DNS.

*You may have to wait a few hours for the DNS name to propagate if you are using Freenom; Duck DNS is a sub domain so I think waiting may not be needed.*

## Obtain SSL certificate
Google dialog flow Fulfillment needs secure connection and we'll be using certificate issued by [Let's Encrypt](https://letsencrypt.org/). Again this is a free service and how to obtain a certificate is well [documented](https://certbot.eff.org/lets-encrypt/ubuntubionic-nginx) at their website.

The following were done when installing the certificate on my server.
### Add Certbot PPA
```sh
    sudo apt-get update
    sudo apt-get install software-properties-common
    sudo add-apt-repository universe
    sudo add-apt-repository ppa:certbot/certbot
    sudo apt-get update
```
### Install Certbot
```sh
sudo apt-get install certbot python-certbot-nginx 
```
### Install Certificate
Until now no changes were made to nginx configuration so we can let the bot to update our nginx configuration. We'll let Certbot to update nginx configuration automatically to serve and turning on HTTPS access in a signle step.

Certbot will install new folders and update /etc/nginx/sites-available/default file. Let us make a backup of the file before we do any updates.
```sh
sudo cp /etc/nginx/sites-available/default /etc/nginx/sites-available/default.original 
sudo certbot --nginx
```
During the above steps you'll be asked to provide your domain name, email address, etc. Ensure all information are provided.

Cerbot will install the necessary certificates and update /etc/nginx/sites-available/default file.

As a check if everything went well lets run the following command:
```sh
 sudo nginx -t && sudo certbot renew --dry-run
```
If you see something as follows:
```sh
nginx: the configuration file /etc/nginx/nginx.conf syntax is ok
nginx: configuration file /etc/nginx/nginx.conf test is successful

Saving debug log to /var/log/letsencrypt/letsencrypt.log

- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
Processing /etc/letsencrypt/renewal/YourDomainName.cf.conf
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
Cert not due for renewal, but simulating renewal for dry run
Plugins selected: Authenticator nginx, Installer nginx
Renewing an existing certificate

- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
new certificate deployed with reload of nginx server; fullchain is
/etc/letsencrypt/live/YourDomainName.cf/fullchain.pem
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
** DRY RUN: simulating 'certbot renew' close to cert expiry
**          (The test certificates below have not been saved.)

Congratulations, all renewals succeeded. The following certs have been renewed:
  /etc/letsencrypt/live/YourDomainName.cf/fullchain.pem (success)
** DRY RUN: simulating 'certbot renew' close to cert expiry
**          (The test certificates above have not been saved.)
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
```
Congratulations! You are ready to move on.
### Setup auto certificate renewal 
Certificate issued by Let's Encrypt are valid only for 3 months. We need to setup a process wehreby the certificate is renewed automatically.
I've setup the following cron job to renew my certificate:
```sh
5 4 * * 0 certbot renew --post-hook "systemctl reload nginx"
```
*If you are not sure how to setup the above cron job here the hints*
```sh
sudo crontab -e
# add certbot renew at the bottom of the file and save it.
```

## nginx as reverse proxy
Our nginx server is not going to serve any webpages and it is going to serve only as the reverse proxy for our application.
### Step 1 Create symbolic link.
This step is optional but if you are reading this document then follow along. If you skip this step you might have to adjust code accordingly later.
Believe you've already installed and compiled the DotNet application. If you were following along your DotNet application would be cloned in ~/DotNet/DataProvider folder. You should have already complied your application as follows:
```sh
cd ~/DotNet/DataProvider
dotnet publish --configuration Release
```
Make a symbolic link to the publish folder to /var/www folder.
```sh
sudo ln -s ~/DotNet/DataProvider/ServeData/bin/Release/netcoreapp2.2/publish/ /var/www/ServeData
```
### Step 2 Update /etc/nginx/sites-available/default file
We already made a copy of /etc/nginx/sites-available/default file before we installed our certficates. Certbot updated this file and we are going to make changes to this file. Let us make another copy of this file.
```sh
sudo cp /etc/nginx/sites-available/default /etc/nginx/sites-available/default.certbot.original 
```
*This document is more "Do This" rather than expalining what happens. So please follow the steps below; don't question me.*
The configuration is split into sections and each section is identified as **server**. For the default server (tip search for the following:)
```sh
server {
          listen 80 default_server;
          listen [::]:80 default_server;
		  ......
		  ......
		  server_name _;
```
Remove the refrence to index.
```sh
	index index.html index.htm index.nginx-debian.html;
	#replace as follows:
	# index index.html index.htm index.nginx-debian.html;
```
Set up reverse proxy:
Search for server section that servers your domian name.
```sh
server_name YourDomainName.whatever;
```
*You may have more than one locations that satisfy the above search. Pick the section that has location element within the section.*

Modify section 'location' as follows:
```sh
location / {
                # First attempt to serve request as file, then
                # as directory, then fall back to displaying a 404.
                # try_files $uri $uri/ =404;
                proxy_pass "https://localhost:5001/";
                proxy_http_version 1.1;
                proxy_set_header   Upgrade $http_upgrade;
                proxy_set_header   Connection keep-alive;
                proxy_set_header   Host $host;
                proxy_cache_bypass $http_upgrade;
                proxy_set_header   X-Forwarded-For $proxy_add_x_forwarded_for;
                proxy_set_header   X-Forwarded-Proto $scheme;
        }
```
If you are scratching your head why I'm doing the above changes there is excellent documentation at [ASP.NET Docs](https://docs.asp.net) and search for nginx. You'll be enlightened.
Save the file and before we move further let us make sure whatever we have done will not break nginx.
```sh
sudo nginx -t
```
If you get a response something like below you are good to go.
```sh
nginx: the configuration file /etc/nginx/nginx.conf syntax is ok
nginx: configuration file /etc/nginx/nginx.conf test is successful
```
### Step 3 Ensure reverse proxy setup is working
Let us first restart nginx
```sh
sudo service nginx restart
```
And now let us run netstat -tl; you should get an output something like below:
```sh
netstat -tl
tcp        0      0 0.0.0.0:27017           0.0.0.0:*               LISTEN
tcp        0      0 0.0.0.0:http            0.0.0.0:*               LISTEN
tcp        0      0 localhost:domain        0.0.0.0:*               LISTEN
tcp        0      0 0.0.0.0:ssh             0.0.0.0:*               LISTEN
tcp        0      0 0.0.0.0:https           0.0.0.0:*               LISTEN
tcp6       0      0 [::]:http               [::]:*                  LISTEN
tcp6       0      0 [::]:ssh                [::]:*                  LISTEN
tcp6       0      0 [::]:https              [::]:*                  LISTEN
```
Open a new terminal and ssh to the server. In the new terminal run the following command:
```
# make sure you are the user who has access to AWS; i.e. $HOME/.aws/config and $HOME/.aws/credentials must exist.
/usr/bin/dotnet /var/www/ServeData/ServeData.dll
```
Hopefully your dotnet application is running without any error messages; now lets leave this terminal and go back to our old terminal window and run netstat again:
```sh
 netstat -tl
Active Internet connections (only servers)
Proto Recv-Q Send-Q Local Address           Foreign Address         State
tcp        0      0 localhost:5000          0.0.0.0:*               LISTEN
tcp        0      0 localhost:5001          0.0.0.0:*               LISTEN
tcp        0      0 0.0.0.0:27017           0.0.0.0:*               LISTEN
tcp        0      0 0.0.0.0:http            0.0.0.0:*               LISTEN
tcp        0      0 localhost:domain        0.0.0.0:*               LISTEN
tcp        0      0 0.0.0.0:ssh             0.0.0.0:*               LISTEN
tcp        0      0 0.0.0.0:https           0.0.0.0:*               LISTEN
tcp6       0      0 ip6-localhost:5000      [::]:*                  LISTEN
tcp6       0      0 ip6-localhost:5001      [::]:*                  LISTEN
tcp6       0      0 [::]:http               [::]:*                  LISTEN
tcp6       0      0 [::]:ssh                [::]:*                  LISTEN
tcp6       0      0 [::]:https              [::]:*                  LISTEN
```
If you see something like above you are ready to test your application. This application will not serve without the authentication key so if you open a browser and go to your application URL. You should be greeted with the following text message:

**Not authorized**

Unfortunalty in this case 'Not authorized' means everthing is working fine! Before we go let's go back to our new terminal, stop our Web API application.
## Start web api application when server starts
We need to start our WEB API application when the server starts and also have a watch dog that will restart our application if it fails. Let us create yourdomainname.service in folder /etc/systemd/system and populate the file as follows:
```sh
[Unit]
Description=Event Registration Example

[Service]
WorkingDirectory=/var/www/ServeData
ExecStart=/usr/bin/dotnet /var/www/ServeData/ServeData.dll
Restart=always
# Restart service after 10 seconds if the dotnet service crashes:
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=dotnet-example
User=USER WHO HAS AWS S3 ACCESS 
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false

[Install]
WantedBy=multi-user.target
```
Save the file and run the following command:
```sh
 sudo systemctl enable indexflux.service
 sudo systemctl restart indexflux.service
 netstat -tl
```
You should not see any error messages and your port, 5001, should be reported by netstat. 

Long post. We are done!!!