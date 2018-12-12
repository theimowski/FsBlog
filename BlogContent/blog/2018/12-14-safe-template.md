@{
    Layout = "post";
    Title = "SAFE Template - a year retrospective";
    Date = "2018-12-14T04:15:13";
    Tags = "F#, SAFE, Saturn, Azure, Fable, Elmish, Fulma";
    Description = "";
}

In this blog entry, we'll take a look at [SAFE Template](https://github.com/SAFE-Stack/SAFE-template) - how it got created, what's the current state and what does it take to release a stable 1.0 version.

<!--more-->

<div class="message">

This post is part of the [F# Advent Calendar 2018](https://sergeytihon.com/2018/10/22/f-advent-calendar-in-english-2018/) initiative - make sure to go check out rest of posts as well.

</div>

## Motivation

SAFE is an end-to-end F# stack for web development, that emphasizes type-safety. If you've never heard about it, visit the [homepage](https://safe-stack.github.io) and [docs](https://safe-stack.github.io/docs) to read more.

SAFE Stack as an initiative has been officially launched in [September 2017](https://compositional-it.com/blog/2017/09-22-safe-release/index.html) at Fable Conf 2017.
[SAFE BookStore](https://github.com/SAFE-Stack/SAFE-BookStore) example app has been around even before that, but it used a different code name. At certain point, the maintainers of SAFE BookStore [decided](https://github.com/SAFE-Stack/SAFE-BookStore/issues/122) it would be nice to have a minimal application, possibly as a template for other people to bootstrap their own SAFE projects.

Since I really liked the idea of combining F# for both server and client side code, and establishing the "SAFE Stack" seemed a good selling point to me, I gave it a go.
That's how [SAFE Template](https://github.com/SAFE-Stack/SAFE-template) was born.

## Templating engine

Together with the [new .NET SDK](https://dotnet.microsoft.com/download), there appeared a [.NET Templating Engine](https://github.com/dotnet/templating) which served the sole purpose of creating new .NET projects from scratch. As the ecosystem is adapting this concept quickly, it seemed to be the obvious way to distribute the SAFE Template.

Working with the templating engine was not *that* straight-forward, mainly due to the fact there is still little documentation and available resources on the topic. To me, most helpful appeared following sites: 

* [.NET Templates samples](https://github.com/dotnet/dotnet-template-samples) - a GitHub repository with dozen of sample projects showcasing various features of .NET Templating engine,
* [Runnable Project Templates Wiki](https://github.com/dotnet/templating/wiki/%22Runnable-Project%22-Templates) - and "in-depth" wiki documentation on important details for developing a .NET template.

## Current state

As of now, SAFE Template has reached version **0.39** and has passed **10.000** of downloads on NuGet

![10K downloads](10K.png)

It comes with a wide variety of options, all of which you can browse [here](https://safe-stack.github.io/docs/template-overview/).
You can also type `dotnet new safe --help` to see the options in your console.

Obviously every new feature to the template might potentially bring more attraction.
However also each new option adds a significant amount of complexity for template to maintain. That's why always we think twice before adding anything new.

We still haven't released version 1.0, so there's still a room for breaking changes.

## Road to 1.0

What do breaking changes in context of a .NET template mean anyway? Good question! There's a couple of things that we'd like to **make stable** before reaching 1.0:

* **CLI interface** - when a template option is present in version 1, we won't change / remove it,
* **Tooling** - we'd like to use both FAKE and Paket as dotnet tools, however as of now we still haven't decided on the final recommended set up for these,
* **Scripts** - ideally we'd like to have a standard set of FAKE Build targets for both production / development builds,
* **WebPack configuration** - this area is still a bit unknown to me, but hopefully we can unify with [webpack-config-template](https://github.com/fable-compiler/webpack-config-template/blob/master/webpack.config.js), which is a recommended configuration for standalone Fable apps.

In addition to that there are also some **key points** that we want to address with 1.0:

* **better adoption** - that's probably quite clear that users may be afraid of using any tool which hasn't reached a stable version yet,
* **Saturn 1.0** - as [Saturn](https://saturnframework.org/) is now a recommended server option and it's currently in version 0.8, we'd like to wait before it reaches a stable version as well,
* **outstanding issues** - obviously, along the way we'll make our best to fix all bugs that arise.

## Credits

I'd like to thank all [28 contributors](https://github.com/SAFE-Stack/SAFE-template/graphs/contributors) (as of time of writing - hopefully there'll be more!) to the SAFE template (order by no. commits):

1. [theimowski](https://github.com/theimowski) (me)
1. [isaacabraham](https://github.com/isaacabraham)
1. [Zaid-Ajaj](https://github.com/Zaid-Ajaj)
1. [theprash](https://github.com/theprash)
1. [0x53A](https://github.com/0x53A)
1. [forki](https://github.com/forki)
1. [vasily-kirichenko](https://github.com/vasily-kirichenko)
1. [psfinaki](https://github.com/psfinaki)
1. [MangelMaxime](https://github.com/MangelMaxime)
1. [AkosLukacs](https://github.com/AkosLukacs)
1. [WalternativE](https://github.com/WalternativE)
1. [matthid](https://github.com/matthid)
1. [CallumVass](https://github.com/CallumVass)
1. [jeremyabbott](https://github.com/jeremyabbott)
1. [marcpiechura](https://github.com/marcpiechura)
1. [rmunn](https://github.com/rmunn)
1. [kunjee17](https://github.com/kunjee17)
1. [vilinski](https://github.com/vilinski)
1. [colinbull](https://github.com/colinbull)
1. [nojaf](https://github.com/nojaf)
1. [MNie](https://github.com/MNie)
1. [t-smirnov](https://github.com/t-smirnov)
1. [landy](https://github.com/landy)
1. [dsyme](https://github.com/dsyme)
1. [pkese](https://github.com/pkese)
1. [JorgeVV](https://github.com/JorgeVV)
1. [Slesa](https://github.com/Slesa)
1. [alfonsogarciacaro](https://github.com/alfonsogarciacaro)

That's all for now. Till next time!