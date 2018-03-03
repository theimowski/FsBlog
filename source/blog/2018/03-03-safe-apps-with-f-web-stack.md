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
* ClojureScript,
* ScalaJs,
* Reason,
* ...,
* ... and **F#**!

[Fable](http://fable.io) - a F# to JavaScript compiler is doing pretty well in convincing web devs to itself.
The project itself has also been a motivation for a movement in F# community to introduce [SAFE Stack](https://safe-stack.github.io).
Having played with SAFE apps for a while, I decided to create a talk devoted to the topic.

## Talk format

What I found appealing in SAFE, was the development experience: 

* Everything is F#, even bindings to [Bulma](https://bulma.io/) CSS Framework brought by [Fulma](https://github.com/MangelMaxime/Fulma): type-**SAFE**ty for the win!,
* I can write "Isomorphic" code to be shared between client and server,
* Development tools like [dotnet watch](https://github.com/aspnet/DotNetTools), [webpack](https://webpack.js.org/), [webpack-dev-server](https://github.com/webpack/webpack-dev-server), [HMR](https://webpack.js.org/concepts/hot-module-replacement/) which combined together make it so that you even don't have to worry about recompiling the code! Just resave your sources, and observe the changes in the browser.

That's why I decided the talk to be a live coding session, where I could share with the audience how **easy and joyful** it is to work with SAFE Stack.

## Lambda Days 2018

I had a pleasure to give the talk at [Lambda Days 2018](http://www.lambdadays.org/lambdadays2018), which turned out to be an awsome conference again!

![lambda days](lambdadays.jpg)

The audience voted for my app:

![lambda days results](lambdadays_results1.jpg)

![lambda days results](lambdadays_results2.jpg)

## F# Exchange 2018

This year in April, I'm excited to join [F# Exchange](https://skillsmatter.com/conferences/9419-f-sharp-exchange-2018) as a speaker.
I'll give the same talk, but will need to adjust to more F# experienced audience.

---

## Resources

* Slides for the talk are available [here](http://theimowski.com/talk-safe-stack),
* Source code of the final solution from Lambda Days 2018 is on [GitHub](https://github.com/theimowski/safe-demo-lambdadays18),
* Video from Lambda Days should be published soon - I'll make sure to point to the URL when it's available.

---


* http://www.lambdadays.org/lambdadays2018
* https://skillsmatter.com/conferences/9419-f-sharp-exchange-2018
* http://theimowski.com/talk-safe-stack/#/
* https://safe-stack.github.io

* https://suave.io
* https://azure.microsoft.com
* http://fable.io
* https://fable-elmish.github.io/elmish
* https://reactjs.org/
* https://github.com/Zaid-Ajaj/Fable.Remoting
* https://github.com/MangelMaxime/Fulma
* https://bulma.io/
* https://dansup.github.io/bulma-templates/
* https://webpack.js.org/
* https://github.com/webpack/webpack-dev-server
* https://chrome.google.com/webstore/detail/redux-devtools/lmhkpmbekcpmknklioeibfkpmmfibljd
* https://chrome.google.com/webstore/detail/react-developer-tools/fmkadmapgofadopljbjfkapdkoienihi

* https://www.microsoft.com/net/core
* https://nodejs.org/
* https://yarnpkg.com/
* https://www.npmjs.com/

* https://github.com/SAFE-Stack/SAFE-BookStore
* https://github.com/SAFE-Stack/SAFE-Nightwatch
* https://github.com/SAFE-Stack/SAFE-ConfPlanner