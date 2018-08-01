@{
    Layout = "post";
    Title = "Why you should use FAKE 5";
    Date = "2018-08-01T10:48:42";
    Tags = "F#, FAKE, Paket";
    Description = "";
}

If your application targets .NET, chances are you're using [FAKE build](http://fake.build/) to build it. In this entry I'll show you why it's worth to upgrade to version 5 of FAKE.

<!--more-->

## Motivation

//TODO : Share thoughts on why it's worth to upgrade, and show migration on practical example. Plus some subjective opinions.

Advantages of FAKE 5:

* .NET Core - no more full .NET / Mono required
* Easy to install - can use global install for multiple projects
* Modular, more lightweight
* Paket integration

## Migration

There's a nice guide on migrating to FAKE 5 available [here](https://fake.build/fake-migrate-to-fake-5.html). It boils down to 3 primary steps:

1. Update to legacy FAKE 5,
2. Fix all the (obsolete) warnings,
3. Use new version of FAKE 5.

This guide should be just enough to help you with your migration. I've collected my notes on migrating a specific project below.

### Step 1: Update to legacy FAKE 5

This one is a no-brainer: FAKE 5 is still released as a stand-alone package to mitigate the migration.
Because I use Paket, it was just a matter of invoking `paket update`.
This version of FAKE package is called *legacy*, as from FAKE 6 onwards that package will no longer be available.

> NOTE: I encourage to use a separate `Build` Paket Group for all dependencies (including FAKE) that are used just for the build process - this gives you a clearer picture of your overall dependencies.

### Step 2: Fix all the (obsolete) warnings

You'll probably spend most of the migration time here.
The [guide](https://fake.build/fake-migrate-to-fake-5.html#Use-the-new-FAKE-API) does already pretty good job describing what this process should look like, so I won't repeat that. Instead let me just emphasize key points to remember about, so that this step doesn't make you a headache:

1. **Namespace conflicts** - beware that FAKE 4 used a lot of `[<AutoOpen>]` attributes to bring in a huge number of functionality OOTB. If the compiler is complaining about used classes, try providing a fully qualified name up until you finally remove the `open Fake` statement,
1. **Common operators** - operators such as `</>`, `!!` or `++` sit now in a separate place. You might find `Fake.IO.FileSystemOperators` and `Fake.IO.Globbing.Operators` namespaces helpful,
1. **Creating targets** - the new way of creating targets by `Target.create` is not compatible with previous approach: if you're using new API to declare target dependencies, make sure you already create targets with new API as well.

### Step 3: Use new version of FAKE 5

There is a [variety of options](https://fake.build/fake-gettingstarted.html#Install-FAKE) here. I'd generally recommend installing FAKE as a global dotnet tool, as you'd be able to reuse it between various projects. Of course keep in mind, you'll need to make the new FAKE available on your build server.

## Summary

## Resources

* https://github.com/fsharp/FAKE
* http://fake.build/