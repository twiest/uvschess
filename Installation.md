# Setting up UvsChess #

UvsChess was created with [Visual C# 2008 Express](http://www.microsoft.com/express/vcsharp/). Students using Windows should use Visual Studio 2008 (Express or Professional). Students running Mac OSX and Linux should use [MonoDevelop](http://www.monodevelop.com/Download)

Students should download the zip file of UvsChess and extract it on their local machine. Students should not check out the project via svn, as svn is sometimes unstable and broken. Once the archive is extracted, the student should open `UvsChess.sln` with their IDE (Visual Studio for Windows, MonoDevelop for Linux, Mac).

The project should compile out of the box.

You will need to rename your assembly. Every student with have a project name StudentAI.dll even though the AI inside the dlls will have different names. There is a known bug with renaming assembly names in Visual Studio. You will need to do this manually. In StudentAI.csproj, change the value of the following xml element.
```
<AssemblyName>StudentAI</AssemblyName>
```
The AssemblyName element occurs three times in StudentAI.csproj. Be sure to change them all.


# Contributing #

We welcome suggestions, patches, bug fixes, and the like!  We are always interested in what students think of UvsChess. If you come across any bugs or would like to request a new feature, let us know about them by [entering an issue](http://code.google.com/p/uvschess/issues/entry) in our Issue Tracker.


If you would like to contribute or compile the latest and greatest revision, see the [Source tab](http://code.google.com/p/uvschess/source/checkout) for information on checking out the project.

If you would like to contribute a bug fix or a new feature, feel free to email the [mailing list](mailto:uvschess@googlegroups.com) with a patch.