# Project Notes

My first C# project. So taking notes on how to set up C# project.

Create a new C# class library project named DnDUtil in the current directory, outputs `dll`.

`dotnet new classlib -n DnDUtil`

Tell .NET where to look. Add the official NuGet and BepInEx sources.

```sh
dotnet nuget add source https://api.nuget.org/v3/index.json -n nuget.org
dotnet nuget add source https://nuget.bepinex.dev/v3/index.json -n bepinex
```

Get the packages

```sh
dotnet add package BepInEx.Core --version 5.4.21
dotnet add package BepInEx.Analyzers --version 1.0.8
dotnet add package HarmonyX --version 2.10.1
```

Link the project to On-Together DLLs in `Managed` dir.

Create main solution 
```sh
dotnet new sln -n AndrewMod
dotnet sln add DnDUtil
``` 

Testing
```sh
dotnet new nunit -n DnDUtil.Tests  
dotnet sln add DnDUtil.Tests
dotnet add DnDUtil.Tests reference DnDUtil 
dotnet add DnDUtil.Tests package Moq
```
