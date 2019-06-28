
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

