@{
    Layout = "post";
    Title = "Paket 'why' command";
    Date = "2016-10-30T07:27:55";
    Tags = "Paket,F#,FAKE";
    Description = "";
}

In this entry I introduce a brand new command called **"why"** in [Paket](http://fsprojects.github.io/Paket/) dependency manager.
It was inspired by a recent project of Facebook, [Yarn](https://yarnpkg.com/) and aims to display **user-friendly** reason for a specific package to be under dependency management.
The command is available in [Paket 3.26](http://fsprojects.github.io/Paket/release-notes.html).

<!--more-->

## Inspiration

Lately Facebook has released a new JavaScript dependency manager on top of [NPM](https://www.npmjs.com/).
It's called [Yarn](https://www.npmjs.com/) and as per the docs, it is supposed to provide 

> "Fast, reliable, and secure dependency management." 

The product comes with a plenty of commands, one of which is called ["why"](https://yarnpkg.com/en/docs/cli/why).

![twitter_yarn.png](twitter_yarn.png)

[Steffen's tweet](https://twitter.com/sforkmann/status/786097754628055040) was a motiviation for me to contribute once again to Paket, also cause I would find interest in such feature myself.

## Implementation

If a specified package is listed in [paket.lock](http://fsprojects.github.io/Paket/lock-file.html) file (which basically means it's under Paket management control), then reason for its presence is one of the following:

    [lang=fsharp]
    // In context of FAKE project dependencies
    type Reason =
    // e.g. Argu - specified in paket.dependencies
    // is not a dependency of any other package
    | TopLevel
    // e.g. Microsoft.AspNet.Razor - specified in paket.dependencies
    // but also a dependency of other package(s)
    | Direct of DependencyChain list
    // e.g. Microsoft.AspNet.Mvc - not specified in paket.dependencies
    // a dependency of other package(s)
    | Transitive of DependencyChain list

1. `TopLevel` stands for a package which is not a dependency of any other packages controlled by Paket (hence "top-level"). It must be listed in [paket.dependencies](http://fsprojects.github.io/Paket/dependencies-file.html) - we call these "direct" dependencies. 
2. `Direct` is also a direct dependency however contrary to `TopLevel`, there's at least one other package managed by Paket that depends on `Direct`.
Note that `TopLevel` will always be a direct package, but `Direct` won't always be "top-level".
3. `Transitive` means an "indirect" dependency. The sole reason it's kept track of by Paket is because it's a dependency of some other package.

## Example

As example, let's have a look at dependencies of [FAKE](http://fsharp.github.io/FAKE/) project:

    [lang=paket]
    source http://nuget.org/api/v2

    nuget Knockout
    nuget Argu
    nuget Nancy.Viewengines.Razor
    nuget Microsoft.AspNet.Razor 2.0.30506
    nuget Microsoft.AspNet.WebPages 2.0.30506
    
    // more dependencies ...

The [paket.dependencies](https://github.com/fsharp/FAKE/blob/master/paket.dependencies) file for FAKE enlists some dependencies - we say that these are "direct" dependencies.

Dependency resolution for FAKE project can be found in [paket.lock](https://github.com/fsharp/FAKE/blob/master/paket.lock):

    [lang=paket]
    NUGET
      remote: http://www.nuget.org/api/v2
        Argu (2.1)
        AspNetMvc (4.0.20710)
          Microsoft.AspNet.Mvc (>= 4.0.20710 < 4.1)
        Knockout (0.0.1)
          AspNetMvc (>= 4.0)
        Microsoft.AspNet.Mvc (4.0.40804)
          Microsoft.AspNet.Razor (>= 2.0.20710)
          Microsoft.AspNet.WebPages (>= 2.0.20710)
        Microsoft.AspNet.Razor (2.0.30506)
        Microsoft.AspNet.WebPages (2.0.30506)
          Microsoft.AspNet.Razor (>= 2.0.20710)
          Microsoft.Web.Infrastructure (>= 1.0)
        Nancy.Viewengines.Razor (1.4.3)
          Microsoft.AspNet.Razor
          Microsoft.AspNet.Razor (>= 2.0.30506) - framework: >= net40
          Nancy (>= 1.4.3)

        // more resolution entries ...

Note that above is just an excerpt from the whole file. 
While paket.lock is very precise on the dependencies resolution, it might be hard for a human to comprehend what is the dependency chain for a given package.

Thanks to the **"why"** command, we can get a nice overview of dependencies resolved by Paket:

![argu](argu.png)

In case of `Argu`, it turns out to be a top-level (hence also direct) dependency in FAKE.

![razor.png](razor.png)

`Microsoft.AspNet.Razor` on the other hand is not a top-level dependency.
For example there's `Microsoft.AspNet.Mvc` which depends on `Microsoft.AspNet.Razor` package.
Even though it's not top-level, it is still a direct dependency, because as we saw before it's included into paket.dependencies manifest.

By default "why" command will display only shortest dependency chain from any top-level package down to the specified one.
If however we're interested in more details, we can print more details by adding `--details` argument to the command:

![razor.details.png](razor.details.png)

The `--details` flag displays also information about version requirements as well as framework constraints if any.

What about transitive dependencies? In FAKE project, `Microsoft.AspNet.Mvc` is an example of transitive dependency:

![mvc.png](mvc.png)

## Summary

The new **"why"** command allows to easily determine why a given package is under Paket control.
This in turn can help us better understand our dependencies within a project and enable smoother management.

The command is now available in [Paket 3.26](http://fsprojects.github.io/Paket/release-notes.html).

Till next time!