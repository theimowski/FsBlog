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

For the sake of this example, let's imagine that the project aims to build a couple of artifacts (doesn't really matter of what kind), and the deploy part boils down to firing a set of HTTP request (including POST-ing the artifacts) towards a web application hosted on the target environment.
Therefore, don't think of below example in terms of your standard .NET project which builds NuGet packages and publishes them to a NuGet feed or deploys them using e.g. [Octopus Deploy](https://octopus.com/).

## Build

First, we'll want to bootstrap Paket:

* Create `.paket` directory in your root codebase directory,
* Download `paket.bootstrapper.exe` [from here](https://github.com/fsprojects/Paket/releases) and place it in `.paket` directory,
* Create `paket.dependencies` file (contents below),
* Create `build.fsx` script (FAKE script),
* Create helper `build.cmd` script (for X-Plat solution, you may consider creating a corresponding `build.sh` script).

##### Contents of `paket.dependencies`

```cmd
source http://nuget.org/api/v2

nuget FAKE

// Tests
nuget FsCheck
nuget FsCheck.Xunit
nuget xunit
nuget xunit.runners
```

##### Contents of `build.fsx`

```
#r @@"packages/FAKE/tools/FakeLib.dll"

open Fake

...

```

##### Contents of `build.cmd`

```
@@echo off
cls

.paket\paket.bootstrapper.exe
if errorlevel 1 (
  exit /b %errorlevel%
)

.paket\paket.exe restore
if errorlevel 1 (
  exit /b %errorlevel%
)

packages\FAKE\tools\FAKE.exe build.fsx %*
```

<div class="message">

Alternatively, you can use [ProjectScaffold](https://github.com/fsprojects/ProjectScaffold) to bootstrap your codebase to use Paket + FAKE. I deliberately chose to configure it myself, as ProjectScaffold by default contains some funky stuff I didn't need, e.g. creating and publishing NuGet package for your project. 

</div>