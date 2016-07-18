@{
    Layout = "post";
    Title = "F# workshop in a browser";
    Date = "2016-07-18T07:17:04";
    Tags = "";
    Description = "";
}

Abstract

<!--more-->

## Idea

I've once attended an awesome F# Coding Dojo conducted by Mathias Brandewinder ([@@brandewinder](https://twitter.com/brandewinder)) in Łódź, Poland.
It was titled "Introduction to Machine Learning with F#" and it came from the series of [F# Coding Dojos](http://c4fsharp.net/#fsharp-coding-dojos) powered by Community for F# ([@@c4fsharp](https://twitter.com/c4fsharp)).

Apart from the subject of this dojo was really interesting and the problem to solve quite exciting, one thing that I enjoyed most was the idea of **Guided Script**.
It based on placing learning materials, short language reference and the actual code all within single script file, in a spirit of [Literate Programming](https://en.wikipedia.org/wiki/Literate_programming).
The script used in "Introduction to Machine Learning with F#" looked like [this](https://github.com/c4fsharp/Dojo-Digits-Recognizer/blob/master/Dojo/GuidedScript.fsx).
For those of you who aren't familiar with them, I highly recommend to go check out rest of the [Dojos](https://github.com/c4fsharp).

Recently I've volunteered to lead a series of **F# workshops** at our company in Gdańsk.
The main purpose of such series would be to introduce colleagues with C# background to world of F# and Functional Programming. 
In order to put emphasis on learning FP concepts and their practical use, I've decided to prepare the series from scratch by myself.

Because I wanted to make those meetings as attractive as possible, I planned to adapt the Guided Script format. 
In addition to that, I needed to prepare some slides to demonstrate and explain FP concepts.
Choice of tool for the presentation was easy - many members of F# Community make use of [FsReveal](http://fsprojects.github.io/FsReveal/), which "allows you to write beautiful slides in Markdown and brings C# and F# to the reveal.js web presentation framework".
Once I've realized that FsReveal allows to write slides in a standard F# script (.fsx) file, I came up with an idea to combine it with Guided Script.

## Realization

## Summary