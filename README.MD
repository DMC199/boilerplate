DMC199
--------
DMC199 is the Clackamas Community College Course working to create a Hololens based experience 
for use within a transmission automotive course.  The transmission we used was 4T60E transmission.
The reason for targetting the transmission course was because a transmission is complicated and 
encapsulated (you cannot watch it work).  We believe we can enhance that experience.  

Using Git to get the project
---------

In the Windows command prompt navigate to the directory where you want to clone it and type

>git clone https://github.com/DMC199/boilerplate.git

If you want to checkout the non-master branch try 

> git checkout -b develop remotes/origin/develop

If you need to see what branches are available 

> git branch -a 

Building the Project
---------

1. Open Unity
2. Naviagate to the Menu File | Build Settings
3. Verify the correct scenes have been added and selected in the proper order
4. Verify the settings are correct [setting screen shot](readme-images/build-settings.png)
    * the Windows Store Platform is selected 
    * use Universal 10
    * D3D
    * Local Machine
    * Unity C# Project
5. Click Build
6. This will prompt you for the build folder.  Create a folder named *Build* and Select Folder
7. Grab a cup of coffee
8. Verify there was not an error, it will open the folder that contains the Build folder
9. Open the Build folder
10. Open the SharedHolograms Visual Studio Project Solution
11. Verify the correct platform is selected (x86) and the correct target (Hololens Emulator or Device)
12. Select Debug | Start without Debugging (Ctrl+F5)
	
Subsequent Builds
--------

On future builds, if you only modified scripts repeat steps 11 & 12.

Running the Project
--------
The project with Sharing requires a Holotooolkit sharing service to be running on a Windows PC on your 
local network.  To launch, select the Holotooolkit | Launch Sharing Service or connect to the service 
running on the instructors PC.

	
Troubleshooting
--------

If you cannot find the device, try relaunching Visual Studio. 