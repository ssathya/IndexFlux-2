
### Installation of MongoDB
We'll first install MongoDB on our application/Db server. The server we are running is Ubuntu 18.04 LTS.  Ubuntu in its distribution does include MongoDB but it what gets installed is 3.6.x. As of the writing of this document, the latest version of MongoDB is 4.0.10 and hence we start with adding GPG keys to import MongoDB keys to the server and adding MongoDB list file that Ubuntu likes.

    sudo apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv 68818C72E52529D4

    sudo echo "deb http://repo.mongodb.org/apt/ubuntu bionic/mongodb-org/4.0 multiverse" | sudo tee /etc/apt/sources.list.d/mongodb-org-4.0.list

We update the repository and install MongoDB as follows:

    sudo apt update && sudo apt install -y mongodb-org

Next, we make sure MongoDB is added as a service to be started at boot time:

    sudo systemctl start mongod
    sudo systemctl enable mongod

Run netstat command to check that MongoDB has been started and serving on its default port 27017.

    sudo netstat -plntu

   

>  tcp     0 0 0.0.0.0:27017 0.0.0.0:* LISTEN 940/mongod

You see a line something like above - congradulations you are on track.

## House Keeping
#### Create administrator account
We'll use the CLI interface for MongoDB on the same machine we've installed our database server.

    mongo

You should be in mongo shell; when you issue the command for the first time the shell gives you some helpful tips and warning messages. As long as you don't see any error messages we are doing okay.

Create administrator as follows in MongoDB shell:

    db.createUser({user:"admin", pwd:"Your Secret password", roles:[{role:"root", db:"admin"}]})





### Installation of MongoDB
We'll first install MongoDB on our application/Db server. The server we are running is Ubuntu 18.04 LTS.  Ubuntu in its distribution does include MongoDB but it what gets installed is 3.6.x. As of the writing of this document, the latest version of MongoDB is 4.0.10 and hence we start with adding GPG keys to import MongoDB keys to the server and adding MongoDB list file that Ubuntu likes.

    sudo apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv 68818C72E52529D4

    sudo echo "deb http://repo.mongodb.org/apt/ubuntu bionic/mongodb-org/4.0 multiverse" | sudo tee /etc/apt/sources.list.d/mongodb-org-4.0.list

We update the repository and install MongoDB as follows:

    sudo apt update && sudo apt install -y mongodb-org

Next, we make sure MongoDB is added as a service to be started at boot time:

    sudo systemctl start mongod
    sudo systemctl enable mongod

Run netstat command to check that MongoDB has been started and serving on its default port 27017.
    sudo netstat -plntu
   >  tcp     0 0 0.0.0.0:27017 0.0.0.0:* LISTEN 940/mongod

You see a line something like above - congradulations you are on track.

## House Keeping
#### Create administrator account
We'll use the CLI interface for MongoDB on the same machine we've installed our database server.

    mongo

You should be in mongo shell; when you issue the command for the first time the shell gives you some helpful tips and warning messages. As long as you don't see any error messages we are doing okay.

Create administrator as follows in MongoDB shell:

    db.createUser({user:"admin", pwd:"Your Secret password", roles:[{role:"root", db:"admin"}]})

Hope you didn't get any error messages and if yes let us make sure you have created the admin user. 
Exit out of mongo shell by typing **exit** and try to login as admin.

     mongo -u admin -p "Your Secret Password" --authenticationDatabase admin
Hopefully, you successfully logged in. Exit out of mongo shell and let us enable authentication.
#### Enable authentication
We enable authentication by updating the property 'ExecStart' in mongod.service file.  This file is located at /lib/systemd/system folder. Edit the file as follows:

    sudo vi /lib/systemd/system/mongod.service.   
    #Yes I'm from Unix world. So vi :)

Around line 9 or 10 locate property 'ExecStart' and add the option '--auth'. After you edit the file ExecStart should look as follows:

    ExecStart=/usr/bin/mongod --auth --config /etc/mongod.conf

Having made configuration changes let us restart the server as follows:

    sudo service mongd restart 
Just to be double sure lets run:

    sudo service mongd status
Make sure its running and there are no errors. Also make sure you can login to mongo by issuing the command:

    mongo -u admin -p "Your Secret Password" --authenticationDatabase admin

So far we have been doing everything on the database server but we are not going to log into the database server to do day to day activity. Our database requests would come either from our client application or services running on a different server; to do so we need to enable external access and configure the UFW firewall.

Ubuntu comes with an inbuilt firewall, UFW, and we are going to enable this to act as a firewall; this is on top of NACL and Security group rules. 

*If you are not conversant with UFW, like me, **first enable ssh** before using it. I enabled UFW before enabling ssh and lost a couple of VMs as I could not do ssh into that server anymore.*

    sudo uwf status
Most probably you might get the following result:

    Stauts: inactive
Either way run enable ssh before doing anything else:

    sudo ufw allow ssh
    sudo ufw enable
    sudo ufw status
You might see a result as follows:

    Status: active
    
    To                         Action      From
    --                         ------      ----
    22/tcp                     ALLOW       Anywhere
    22/tcp (v6)                ALLOW       Anywhere (v6)
If `22/tcp` is __allow__: good you are not going to lose your machine!
The syntax for UFW is

    sudo ufw allow from <target> to <destination> port <port number>
Ideally you would do the following to allow access to your mongoDB instance:  
`sudo ufw allow from` *My first slected IP address* `to any port 27017`  
`sudo ufw allow from` *My next IP address* `to any port 27017`  
`sudo ufw allow from` *My next IP address* `to any port 27017`

Well I'm lazy so I'm going to open up 27017 for world access(I'm going to protect my MongoDB using Security Group firewall):

    sudo ufw allow 27017

Just one more change:
MongoDB listens to localhost by default, to make the database accessible from outside, we  
have to reconfigure it to listen on the server IP address too.  
Open the mongod.conf file in nano editor:  
```
sudo nano /etc/mongod.conf  
```
and add the IP address of the server in the bind_ip line like this:  
```
    net:  
    port: 27017  
    bindIp: 127.0.0.1,192,168.1.100  
```
Replace 192.168.1.100 with the IP of your server, then restart MongoDB to apply the  
changes.

    sudo service mongod restart  

Now you can access the MongoDB database server over the network.

