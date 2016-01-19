@{
    Layout = "post";
    Title = "Continuous delivery with FAKE, Paket and TeamCity";
    Date = "2016-01-18T05:31:38";
    Tags = "Continuous delivery,FAKE,Paket,TeamCity,F#,Git";
    Description = "";
}

Efficient continuous delivery process can speed up greatly development cycle, improve feedback loop and help to manage automatic deployments.     
In this entry I'll present how one can configure a continuous delivery chain using combination of following tools: [FAKE](http://fsharp.github.io/FAKE/), [Paket](http://fsprojects.github.io/Paket/) and [TeamCity](https://www.jetbrains.com/teamcity/).

<!--more-->

## Assumptions

Let's start with defining goals of the process as described in this post:

* Each push to master branch has to trigger a build on the build server,
* The build server builds up a package, runs a suite of tests upon that package and exposes the package as a build artifact with its specific version,
* The package can be deployed to `TEST` environment only if the build succeeded,
* The package can be deployed to `PROD` environment only if it was already deployed to `TEST` before (and tested there),
* In order to verify which version is in what environment, all runs are marked with the build version,
* There are scripts for both build and deploy steps, written with help of common tools.

For the sake of this example, we'll build a very simple package containing a single file.
The deploy part boils down to firing a HTTP POST request with contents of the file in the request's body.
This minimal setup can be later extended to more sophisticated use cases.  

## Build scripts

In the first turn, [bootstrap Paket](http://fsprojects.github.io/Paket/installation.html#Installation-per-repository).
Then create `paket.dependencies` to pull `FAKE` library as well as other packages for testing:


```cmd
source http://nuget.org/api/v2

nuget FAKE

// Tests
nuget FsCheck
nuget FsCheck.Xunit
nuget xunit
nuget xunit.runners
```

Next, create `build.fsx` FAKE script:

```
#r @@"packages/FAKE/tools/FakeLib.dll"

open Fake

Target "Clean" (fun _ ->
    CleanDirs ["./build/"]
)

Target "Test" (fun _ ->
    // not relevant here
    DoNothing () 
)

Target "Build" (fun _ ->
    "./src/file" CopyFile "./build/file"
)

"Clean"
    ==> "Test"
    ==> "Build"

RunTargetOrDefault "Build"

```

Target called "Build" as you can see only copies file from `src` to `build` directory, but you can imagine that there occurs some process of building. 
Finally add `build.cmd` helper script (Windows, for Unix you can create corresponing .sh script):

```
@@echo off
cls

.paket/paket.bootstrapper.exe
if errorlevel 1 (
  exit /b %errorlevel%
)

.paket/paket.exe restore
if errorlevel 1 (
  exit /b %errorlevel%
)

packages/FAKE/tools/FAKE.exe build.fsx %*
```

<div class="message">

Alternatively, you can use [ProjectScaffold](https://github.com/fsprojects/ProjectScaffold) to bootstrap your codebase to use Paket + FAKE. I deliberately chose to configure it myself, as ProjectScaffold by default contains some funky stuff I didn't need, e.g. creating and publishing NuGet package for your project. 

</div>

## Deployment scripts

Now, let's move to creating scripts for automatic deployment