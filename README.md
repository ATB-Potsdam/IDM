# IDM
Irrigation water Demand Module: A comprehensive implementation of the FAO56 dual coefficient crop water demand calculation.

This module is intended to be used with the Microsoft .NET framework version 4.5. With minor changes you can compile it for .NET 4.0 too.

# Quick Start
## Install a development environment
We recommend "Visual Studio 2013 Community Edition" or the newer 2015 version, it is free but with registration. The .NET frameworks 4.0 and 4.5 are included.
## Install dependencies
We need "git" at the command line to automatically update revision and build number, you can download and install it from here:
https://git-for-windows.github.io/
For automated documentation of the API we use dogygen, you can find it here:
https://sourceforge.net/projects/doxygen/files/
## Open the project
Within Visual Studio open the solution "ATB_Irrigation-Module_cs"
## Compile the project
Right-Click on "ATB_Irrigation-Module_cs" and choose rebuild
## Add the created dll to the project where you want to use it
In the solution explorer right click on the project and choose "Properties". Go to "References" and add the dll.
## Use functions from the dll
All exported functions are in the namespace "atbApi". Now is the right time to take a look at the documentation with a lot of code examples in the doc directory created by doxygen.

# Credits
![bmbf logo](https://www.runlevel3.de/bmbf-eng_30.png)
