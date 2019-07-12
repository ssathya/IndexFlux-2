

# Install Application
The server application and batch applications are written in C# that uses .net Core framework. To make the application run on Linux we need to first install .NET Core. The following were done to install the application
## Register Microsoft key and feed
*Almost everything mentioned in this section will be a repeat of what is documented in [Install .NET Core SDL on Linux](https://dotnet.microsoft.com/download/linux-package-manager/ubuntu18-04/sdk-current) site. Don't want running around each time I do a server upgrade so I'm shamelessly copying from the above-mentioned site.*  
Before installing .NET, you'll need to register the Microsoft key, register the product repository, and install required dependencies. This only needs to be done once per machine.

Open a terminal and run the following commands:
```sh
wget -q https://packages.microsoft.com/config/ubuntu/18.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
```
## Install the .NET SDK
Update the products available for installation, then install the .NET SDK.
In your terminal, run the following commands:
```sh
sudo add-apt-repository universe
sudo apt-get install apt-transport-https
sudo apt-get update
sudo apt-get install dotnet-sdk-2.2
```
The last step takes some time and once installed we are all good.

## Install Application
I'm trying to minimize third-party software that is being installed and hence installing the application will be a manual process.
Make a folder Dotnet and ... Instead of putting in words the following commands should be self explanatory.
```sh
cd $HOME
mkdir -p DotNet
cd DotNet
rm -rf DataProvider || true #not needed now but will be doing this later
git clone https://ssathya@dev.azure.com/ssathya/DataProvider/_git/DataProvider
cd DataProvider
sudo service mongod stop # we are using a server with 0.5G memory. Every bit of memory helps.
# we'll have to stop other services, Nginx and Index Flux as well after we install them.
dotnet publish --configuration Release
#Restart all sevices we have stopped; for now mongo is the only one now.
sudo service mongod restart # if you want replace restart with start.
```
In a nutshell, what we have done above is create a folder DotNet if it doesn't exist deleting folder DataProvider if it exists. We are cloning my repository from Azure Devops . Hopefully, git command executes without any error. We next compile a release version of the software; prior building the software we are going to stop MongoDB to avoid swapping memory when compiling. Once the compilation is done we restart MongoDB.
## Install AWS cli
The application has a dependency on AWS S3; I'll leave the dependency for this release as I don't mind the minimal cost involved for this minor update. If you need to install your own version of Indexflux you need to update the following 2 lines in **2 files** (yes my code is not DRY).
```cs
    public static string BucketName = @"talk2control-1";
    public static RegionEndpoint Region = RegionEndpoint.USEast1;
```
Classes that need updates are:
```sh
MongoReadWrite.Extensions.ServiceExtensions
DataProvider.Extensions.ServiceExtensions
```
We install AWS cli as follows:
```sh
sudo apt install awscli
```
Once installed
```sh
aws configure
#provide credentials of a user that can access your S3 bucket.
```
Time to test if it works.
```sh
cd ~/DotNet/DataProvider/MongoReadWrite
dotnet run --configuration Release
echo $?
```
If you don't see any error message and see a '0' displayed on the screen. If you get any error messages, feel free to contact me so that I can update this document after we fix your error.

Also let us check if your WEB API service will work.
```sh
cd ~/DotNet/DataProvider/ServeData
dotnet run --configuration Release
```
The application takes 5 to 10 seconds to start and if you don't see any error messages you can terminate the application. As mentioned above if you get any error messages, feel free to contact me so that I can update this document after we fix your error.
