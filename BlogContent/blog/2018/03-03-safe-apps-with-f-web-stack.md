@{
    Layout = "post";
    Title = "SAFE apps with F# web stack";
    Date = "2018-03-03T03:49:32";
    Tags = "F#, SAFE, Suave, Azure, Fable, Elmish, Fulma";
    Description = "";
}

In this entry I describe my talk on creating web apps in F# and [SAFE Stack](https://safe-stack.github.io) - the motivation behind, as well as format of the talk.

<!--more-->

## Motivation

Web development is still getting its momentum.
Enormous **JavaScript** ecosystem gobble next developers - indeed according to [StackOverflow 2017 Developer Survey Results](https://insights.stackoverflow.com/survey/2017), JS is "the most popular technology" (not going into debates on what that actually means).

Meanwhile, more and more compile-to-javascript solutions contribute to the market, including:

* TypeScript,
* CoffeeScript,
* Dart,
* ...,

and more functional-style languages like:

* Elm,
* PureScript,
* Reason,
* ...,
* ... and **F#**!

[Fable](http://fable.io) - a F# to JavaScript compiler is doing pretty well in convincing web devs to itself.
The project has also been a motivation for a movement in F# community to introduce [SAFE Stack](https://safe-stack.github.io).
Having played with SAFE apps for a while, I decided to create a talk devoted to the topic.

## Talk format

What I found appealing in SAFE, was the development experience: 

* Everything is F#, even bindings to [Bulma](https://bulma.io/) CSS Framework brought by [Fulma](https://github.com/MangelMaxime/Fulma): type-**SAFE**ty for the win!,
* I can write "Isomorphic" code to be shared between client and server,
* Development tools like [dotnet watch](https://github.com/aspnet/DotNetTools), [webpack](https://webpack.js.org/), [webpack-dev-server](https://github.com/webpack/webpack-dev-server), [HMR](https://webpack.js.org/concepts/hot-module-replacement/) which combined together make it so that you even don't have to worry about recompiling the code! Just resave your sources, and observe the changes in the browser.

That's why I decided to make the talk a live coding session, where I could share with the audience just how **easy and joyful** it is to work with SAFE Stack in practice.

Because second letter from the SAFE acronym stands for [Azure](https://azure.microsoft.com), I thought it'd be nice to actually deploy the final app to the cloud, so that the audience can play with it on their mobile phones.

This is how I decided to create during the demo a simple **voting app**, so that listeners can score my talk just after I deploy it to Azure.

## Lambda Days 2018

I had a pleasure to give the talk at [Lambda Days 2018](http://www.lambdadays.org/lambdadays2018), which turned out to be an awsome conference again - big thanks to the organizers for having me!

![lambda days](lambdadays.jpg)

Fortunately everything went fine, and I managed to deliver the voting app to Azure (though I exceeded time limit a bit - sorry for that!).

As can be seen in the mobile screenshots below, overally audience seemed to enjoy the session (some of them even tried to SQL-inject my in-memory DB 😃):

<div>
<img src="lambdadays_results1.jpg" width="48%" style="float:left; padding:5px;" />
<img src="lambdadays_results2.jpg" width="48%" style="padding:5px;" />
</div>


## F# Exchange 2018

![fsharp exchange](fsharp_exchange.png)

This year in April, I'm also excited to join [F# Exchange](https://skillsmatter.com/conferences/9419-f-sharp-exchange-2018) as a speaker.
I'll give the SAFE talk again, but this time will have to adjust a bit to more F#-oriented audience. Also, SAFE is moving forwards very rapidly, so I might consider showcasing some of the brand new capabilites.

Can't wait to meet everyone from the awesome F# community in London. If you want to see creating SAFE app live in action, then I hope to see you there as well!

---

## Resources

* Slides for the talk are available [here](http://theimowski.com/talk-safe-stack),
* Source code of the final solution from Lambda Days 2018 is on [GitHub](https://github.com/theimowski/safe-demo-lambdadays18),
* Video from Lambda Days is now available [here](https://www.youtube.com/watch?v=LBekZt8QB4w),
* Video from F# eXchange 2018 is available [here](https://skillsmatter.com/skillscasts/11308-safe-apps-with-f-web-stack).