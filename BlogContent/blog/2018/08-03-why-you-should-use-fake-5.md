@{
    Layout = "post";
    Title = "Why you should use FAKE 5";
    Date = "2018-08-03T10:48:42";
    Tags = "F#, FAKE, Paket";
    Description = "";
}

If your application targets .NET, chances are you're using [FAKE](http://fake.build/) to build it. In this entry I'll show you why it's worth to upgrade to version 5 of FAKE.

<!--more-->

## Motivation

[FAKE](http://fake.build/) (F# Make) is a great tool for building .NET applications.
It's been already in the market for a [couple of years now](https://github.com/fsharp/FAKE/commit/0ad4df0c95d866dff495a31eb7a8373aa2e0422a), almost [2000 releases](https://github.com/fsharp/FAKE/releases) and [300 contributors](https://github.com/fsharp/FAKE/graphs/contributors). It is an outstanding community effort to fill in a noticeable gap that MsBuild left in the .NET ecosystem.
Along its **.NET-dedicated** alternatives we can list [PSake](https://psake.readthedocs.io/en/latest/) (PowerShell Make) or [Cake](https://cakebuild.net/) (C# Make).

Because FAKE has been growing rapidly since the beginning and because of its initial design, it started getting into a [technical debt](https://fake.build/fake-fake5-learn-more.html#New-API-Design).
FAKE 4 used to be the most recent stable version and probably remains the most commonly used version in build scripts.

Decision has been made to create a new major release of FAKE, which would fix previous issues as well as add new important enhancements.
Even though the new FAKE 5 has a lot of breaking changes compared to its predecessor, it establishes a clean [migration path](https://fake.build/fake-migrate-to-fake-5.html).

Among the most important advantages for the new version of FAKE, we can list:

* **.NET Core** - FAKE 5 is built on top of .NET Core runtime, meaning that one doesn't need full .NET or Mono installed anymore to run the scripts,
* **Easier installation** - FAKE 5 can be installed as a [global dotnet tool](https://fake.build/fake-gettingstarted.html#Install-FAKE) (my recommendation), which lets you use the same tool across multiple projects,
* **Modularity** - FAKE 5 feels much more lightweight: FAKE 4 binary bundled all the functionalities into a single ~80MB NuGet package, which started to be an issue when you wanted to pull in new version or had tens of different versions for different projects installed. Now each set of functionality comes in a dedicated NuGet package,
* **Paket integration** - You can now embed [Paket references](https://fake.build/fake-gettingstarted.html#Getting-started) into a FAKE script without having separate Paket executable. You can do this having just .NET Core installed, as standalone Paket still requires [full .NET / Mono](https://github.com/fsprojects/Paket/pull/3183).

> Note: if using Paket for FAKE dependencies, consider using [`storage: none`](https://fsprojects.github.io/Paket/dependencies-file.html#Disable-packages-folder) option to disable packages folder and instead reuse NuGet packages between different projects.

## Migration

If you're currently using FAKE 4, there is some work you need to do to upgrade.
As already mentioned, there's a nice guide on migrating to FAKE 5 available [here](https://fake.build/fake-migrate-to-fake-5.html). It boils down to 3 primary steps:

1. Update to legacy FAKE 5,
2. Fix all the (obsolete) warnings,
3. Use new version of FAKE 5.

This guide should be just enough to help you with your migration. I've collected my notes on migrating a specific project below.

### Step 1: Update to legacy FAKE 5

This one is a no-brainer: FAKE 5 is still released as a stand-alone package to mitigate the migration.
Because I use Paket, it was just a matter of invoking `paket update`.
This version of FAKE package is called *legacy*, as from FAKE 6 onwards that package will no longer be available.

> Note: I encourage to use a separate `Build` Paket Group for all dependencies (including FAKE) that are used just for the build process - this gives you a clearer picture of your overall dependencies.

### Step 2: Fix all the (obsolete) warnings

You'll probably spend most of the migration time here.
The [guide](https://fake.build/fake-migrate-to-fake-5.html#Use-the-new-FAKE-API) does already pretty good job describing what this process should look like, so I won't repeat that. Instead let me just emphasize key points to remember about, so that this step doesn't make you a headache:

1. **Namespace conflicts** - beware that FAKE 4 used a lot of `[<AutoOpen>]` attributes to bring in a huge number of functionality OOTB. If the compiler is complaining about used classes, try providing a fully qualified name up until you finally remove the `open Fake` statement,
1. **Common operators** - operators such as `</>`, `!!` or `++` sit now in a separate place. You might find `Fake.IO.FileSystemOperators` and `Fake.IO.Globbing.Operators` namespaces helpful,
1. **Creating targets** - the new way of creating targets by `Target.create` is not compatible with previous approach: if you're using new API to declare target dependencies, make sure you already create targets with new API as well.

### Step 3: Use new version of FAKE 5

There is a [variety of options](https://fake.build/fake-gettingstarted.html#Install-FAKE) here. I'd generally recommend installing FAKE as a global dotnet tool, as you'd be able to reuse it between various projects. Of course keep in mind, you'll need to make the new FAKE available on your build server.

## Summary

FAKE remains my favorite building tool in the .NET ecosystem.
The brand new 5th version brings to the table a plenty of considerable advantages.
Because .NET Core is getting its momentum, I'd highly encourage you to take your time and migrate to FAKE 5 - you won't regret it.

All the credits go to [FAKE contributors](https://github.com/fsharp/FAKE/graphs/contributors)

Till next time!