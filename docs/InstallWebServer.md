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
sudo certbot renew --dry-run
```
If you see something as follows:
```sh
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
