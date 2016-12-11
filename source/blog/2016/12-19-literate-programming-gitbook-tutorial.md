@{
    Layout = "post";
    Title = "Literate programming GitBook tutorial";
    Date = "2016-12-19T07:50:48";
    Tags = "";
    Description = "";
}

Abstract
Think about the title - it doesn't sound too good...

<!--more-->

<div class="message">

This post is part of the [F# Advent Calendar 2016](https://sergeytihon.wordpress.com/2016/10/23/f-advent-calendar-in-english-2016/) initiative - make sure to go check out rest of posts as well. 
I'd like to thank [Sergey Tihon](https://twitter.com/sergey_tihon) for organising F# Advent. Previous edition gave birth to this blog, encouraging me to write [my first post ever](/blog/2015/12-21-property-based-testing-xslt/index.html).

</div>

## Motivation

More than a year ago, as part of my master thesis I've written [Suave Music Store tutorial](https://www.gitbook.com/book/theimowski/suave-music-store).
To my delight, this GitBook turned out to be a pretty good starting point for developers wanting to learn [Suave](https://suave.io/) for web development.
As I'm not probably the best technical writer, I was wondering what could have made this resource that successful.
I arrived at a conclusion, that what was special about format of this tutorial when compared to other tutorials available on web was the close **connection of source code to content**, which guided step-by-step, change-by-change through the process of creating a web app.

What does this mean in practice?
Let's have a look at following example from tutorial:

![GitHub tag hyperlink](github_tag_hyperlink.png)

In addition to thorough explanation of every single change, I included (at the end of each main section) a hyperlink to git tag pointing to GitHub **commit** with state of the codebase at the exact point of tutorial. Thanks to this, the reader could have a look at original sources in case he got stuck during his experiments.

To maintain the tutorial I decided to keep track of two separate repositories: [first](https://github.com/theimowski/SuaveMusicStore) for the source code of the app, and [second](https://github.com/theimowski/SuaveMusicStoreTutorial) for the GitBook tutorial content. 
I could have probably done the same with a single git repository, however then I'd go for separate branches for source and content so that I could easily make distinction between source and content related changes.
Also one of my main goals was to enable the reader go checkout the source repository and **browse through the commit history**, so I had to keep the history as clean as possible, with no junk commits in between.

![commit_history.png](commit_history.png)

As seen in above screenshot, git commit history for the source repository was linear, with few changes between commits so that the reader could easily browse through it.

Keeping sync between those two repos wasn't very comfortable. While copying pieces of code between source and content or fixing typos were rather kind of usual activities, one of the hardest issue to deal with was **amending the source code**.
To ensure my initial goal of allowing reader to browse through each commit, I had to overwrite remote git history, which meant the (in)famous ``git push --force``, as well as recreating proper git tags, so that the hyperlinks pointed to right commit.

I initially based the tutorial on **Suave 0.28.1**, which back then was the latest version. 
When **Suave 1.0** got released, I wanted to update the content accordingly. 
I even got [help](https://github.com/theimowski/SuaveMusicStoreTutorial/pull/11) from the OSS community to do so, but then I got back to the problem of sync with source code git history.
What I ended up was a new git branch which I **rebased** in [interactive](https://git-scm.com/docs/git-rebase#git-rebase---interactive) mode from the root commit of source code repository.
This required quite a lot effort, but I managed to retain my original goal.

Now that **Suave 2.0** is close to its stable release, I'd like to publish new version of the tutorial. 
I figured out a **rather unusual idea**, which I'm going to describe in this entry.

> Note: The idea is still a work in progress, and I'm not even sure where it goes. It might happen that I decide at certain point that the solution isn't satisfactory and drop it completely. Anyways I thought it can be worth sharing with public - feedback can really do a great job.

## The idea

The idea is based on following assumptions: 

* unit of change, a **git commit** maps to a single section (or nested subsection) of tutorial,
* every commit contains only those changes that are described in corresponding (sub)section,
* content of the section is stored in **commit message**, formatted in markdown.

![git_log_reverse_oneline.png](git_log_reverse_oneline.png)

![commit_is_section.png](commit_is_section.png)

---

Downsides:

* No easy way to contribute to repository (amending commits)
* Currently no support for multilang (mention japanese) 