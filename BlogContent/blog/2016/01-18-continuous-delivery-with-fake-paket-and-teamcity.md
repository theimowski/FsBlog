@{
    Layout = "post";
    Title = "Continuous delivery with FAKE, Paket and TeamCity";
    Date = "2016-01-22T18:24:38";
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

Some TeamCity concepts are used throughout the entry, which are not explained, so in case of any doubts refer to [the docs](https://www.jetbrains.com/teamcity/documentation/).

## Build scripts

In the first turn, we need to [bootstrap Paket](http://fsprojects.github.io/Paket/installation.html#Installation-per-repository).
Then let's create `paket.dependencies` to pull `FAKE` library as well as other packages for testing:


```cmd
source http://nuget.org/api/v2

nuget FAKE

// Tests
nuget FsCheck
nuget FsCheck.Xunit
nuget xunit
nuget xunit.runners
```

Next, let's add `build.fsx` FAKE script:

```
#r @@"packages/FAKE/tools/FakeLib.dll"

open Fake
open Fake.TeamCityHelper

let releaseNotes = File.ReadAllLines "RELEASE_NOTES.md"

let version = 
    releaseNotes
    |> parseAllReleaseNotes
    |> List.head
    |> fun x -> x.SemVer.ToString()

let teamCityIsPresent = TeamCityVersion.IsSome

Target "Clean" (fun _ ->
    CleanDirs ["./build/"]
)

Target "Test" (fun _ ->
    // not relevant here
    DoNothing () 
)

Target "Build" (fun _ ->
    if teamCityIsPresent then
        SetBuildNumber (sprintf "%s.%s" version TeamCityBuildNumber.Value)

    "./src/file" CopyFile "./build/file"
)

"Clean"
    ==> "Test"
    ==> "Build"

RunTargetOrDefault "Build"

```

What "Build" target does in above snippet is copying file from `src` to `build` directory, but you can imagine that there occurs some process of building instead.
In addition to that, "Build" target sets the TeamCity build number to X.X.X.X format, where first three numbers (major, minor, patch) are read from the first line of `RELEASE_NOTES.md` and the last number is taken from the TeamCity build counter (always incremented). 
 
Finally add `build.cmd` helper script (for Unix you can create corresponing .sh script):

```cmd
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

Alternatively, we could have used [ProjectScaffold](https://github.com/fsprojects/ProjectScaffold) to bootstrap the repository to use Paket + FAKE. I deliberately chose to configure it myself, as ProjectScaffold by default contains some funky stuff I didn't need, e.g. creating and publishing NuGet package for your project. 

</div>

## Deployment scripts

Now, let's move to creating scripts for automatic deployment.
For that reason, at the end of `paket.dependencies` file we'll add a [dependency group](http://fsprojects.github.io/Paket/groups.html):

```cmd
group Deploy

    source http://nuget.org/api/v2
    
    nuget FAKE
    nuget Http.fs-prerelease
```

This dependency group will allow to restore packages needed for deploy part only, i.e. FAKE to run the deploy script and a helper HTTP client library, [Http.fs-prerelease](https://github.com/haf/Http.fs).   

Deploy script written in FAKE can look like something between those lines:

```
#r @@"packages/deploy/FAKE/tools/FakeLib.dll"
#r @@"packages/deploy/Http.fs-prerelease/lib/net40/HttpFs.dll"

open Fake
open HttpFs.Client

Target "Deploy" (fun _ ->
    // read optional parameters
    let host = getBuildParamOrDefault "host" "localhost"
    let port = getBuildParamOrDefault "port" "80"
    // Take the file from build and send a HTTP POST request to target machine 
)

RunTargetOrDefault "Deploy"
```

And the corresponding `deploy.cmd` (note the additional `group deploy` for `restore` command and `deploy` in `packages` directory):

```cmd
@@echo off
cls

.paket/paket.bootstrapper.exe
if errorlevel 1 (
  exit /b %errorlevel%
)

.paket/paket.exe restore group deploy
if errorlevel 1 (
  exit /b %errorlevel%
)

packages/deploy/FAKE/tools/FAKE.exe deploy.fsx %*
```

## Building package on TeamCity

Creating appropriate build configuration on TeamCity gets pretty easy now:

1. Attach VCS Root,
2. Add VCS Trigger,
3. Define a single Command Line build step,
4. Specify Artifacts path.

Command for the build step is just `build` (runs the `build.cmd` script): 

![build step](build_step.png)

To create the package, we need:

* deploy scripts `deploy.*` (includes fsx and cmd)
* paket files `paket.*` (includes dependencies and lock file)
* artfiacts from `build/*` directory
* `paket.bootstrapper.exe` 

![artifacts](artifacts.png)

The resulting package will look like below:

![package](package.png)

## TeamCity Deploy chain

Having passed tests and built the package on TeamCity, we can now create a following deployment chain:

![chain](chain.png)

This can be achieved by defining new TeamCity configurations `Deploy TEST` and `Deploy PROD` (below list applies to both configurations):

1. Specify artifact dependency,
2. Specify snapshot dependency,
3. Add single Command Line build step,
4. Fill in appropriate parameters.

Both `TEST` and `PROD` environment need the same artifact dependency built from `Build` configuration:

![depl_artif_dep](depl_artif_dep.PNG)

Note the "Build from the same chain" option. It ensures that the same package is used for both `Deploy` configurations.
In order to unzip contents of the `package.zip` in working directroy, we have to type `package.zip!**` in the "Artifacts Path" field.

Deploy configurations will differ with regards to snapshot dependency. The `Deploy TEST` configuration should depend on `Build`:

![depl snapshot dep](depl_snapshot_dep.png)

and `Deploy PROD` should depend on `Deploy TEST`:

![depl snapshot dep](depl_snapshot_dep_test.png)

For the command line step, we'll just have to call `deploy` (possibly with passing parameters for target environment host and port):

![depl build step](depl_build_step.png)

## Summary

With this setup, every push to the master branch will trigger `Build` configuration.
If the tests pass, `package.zip` gets created and exposed as the configuration's artifact.
Successful `Build` enables next step, which is `Deploy TEST`.
It can be done either manually, or in automatic fashion as well (for instance by attaching a build trigger).
`Deploy PROD` behaves in similar way - it can be run only if `Deploy TEST` was executed successfully.   

It's also useful to subscribe to TeamCity notifications upon successful deployment, so that we're always up-to-date with latest deployments.
As of version 8.1.3 TeamCity supports Email, IDE Notifier, Jabber and Windows Tray notifiactions.