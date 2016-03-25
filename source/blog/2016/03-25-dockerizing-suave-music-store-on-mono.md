@{
    Layout = "post";
    Title = "Dockerizing Suave Music Store on mono";
    Date = "2016-03-25T11:02:56";
    Tags = "Suave.IO,F#,Docker,Mono,Linux,Postgres";
    Description = "";
}

I've written the [Suave Music Store GitBook](https://theimowski.gitbooks.io/suave-music-store), which helped quite a few people to get started with the [Suave.IO](https://suave.io/) library for creating web applications in F#.
However the original version of tutorial didn't really feel x-platform, as it was designed to work on Windows only. 
I decided to revisit the tutorial and add 3 more sections describing how to convert the app to run on [mono](http://www.mono-project.com/) and place inside a [Docker](https://www.docker.com/) container. 

<!--more-->
