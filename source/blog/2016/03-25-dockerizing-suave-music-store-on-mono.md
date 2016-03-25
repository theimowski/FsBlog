@{
    Layout = "post";
    Title = "Dockerizing Suave Music Store on mono";
    Date = "2016-03-25T11:02:56";
    Tags = "Suave.IO,F#,Docker,Mono,Linux,Postgres,Paket";
    Description = "";
}

I've written the [Suave Music Store GitBook](https://theimowski.gitbooks.io/suave-music-store), which helped quite a few people to get started with the [Suave.IO](https://suave.io/) library for creating web applications in F#.
However the original version of tutorial didn't really feel x-platform, as it was designed to work on Windows only. 
I decided to revisit the tutorial and add 3 more sections describing how to convert the app to run on [mono](http://www.mono-project.com/) and place inside a [Docker](https://www.docker.com/) container. 

<!--more-->

I've decided to split the desired goal into 3 parts:

1. Converting form NuGet restore client to Paket
2. Changing the database from SQL Server to Postgres
3. Setting up docker containers

If you don't care much about details, and just want to check out the final code - follow [this](https://github.com/theimowski/SuaveMusicStore/tree/docker) link. 

## Moving to paket

First step was to move away from the NuGet client, as it can bring some pain in conjunction with mono.
Also Paket has a number of advantages comparing to the NuGet client - you can read about them [here](https://fsprojects.github.io/Paket/faq.html). 

[Moving to paket section](https://theimowski.gitbooks.io/suave-music-store/content/en/moving_to_paket.html)

## Using postgres



[Using postgres section](https://theimowski.gitbooks.io/suave-music-store/content/en/using_postgres.html)

## Bring docker into play

[Dockerizing on mono section](https://theimowski.gitbooks.io/suave-music-store/content/en/dockerizing_mono.html)

## Summary

I get really excited when I think about that I managed to make it work, specially since I was a total noob with *nix stuff and Docker too (well ok, I probably still remain one).
Go check out 3 new sections (10.2 - 10.4) of the Suave Music Tutorial [here](https://theimowski.gitbooks.io/suave-music-store) to read more details about the conversion.
Till next time!