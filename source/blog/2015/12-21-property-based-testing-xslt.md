@{
    Layout = "post";
    Title = "Property-based testing XSLT";
    Date = "2015-12-21T07:11:55";
    Tags = "XSLT,F#,Property-based testing,Functional programming";
    Description = "";
}

Property-based testing is a wonderful tool to verify correctness of your programs. 
However, some people struggle with finding a valid case to use the technique.
In this entry, I'll present a rather __exotic example__ of applying property-based testing to check results of running XSL transformations.

<!--more-->

<div class="message">

This post is part of the [F# Advent Calendar in English 2015](https://sergeytihon.wordpress.com/tag/fsadvent/) action. 
Big thanks go to [Sergey Tihon](https://twitter.com/sergey_tihon) for organizing such a great initiative.
Make sure to go check out rest of posts as well.

<!-- mention suave in japanese -->

</div>

I've been constantly postponing creation of my own blog.
There were [books](http://amzn.com/1617292397) and [video courses](http://pluralsight.com/courses/get-involved) which did encourage me to start, however I wasn't confident enough to do so.
The [F# Advent](https://sergeytihon.wordpress.com/tag/fsadvent/) event convinced me to break the ice - "at least I'll have any readers", I thought.   
This is my first blog post ever, so chances are it's not gonna be the best F# article you've ever read.
Anyway bear with me - I think I've got something interesting to share.

My job is to develop and maintain a Content Management System (CMS). 
The company I work for is a big corporation and a big corporation often equals __enterprise__ software, that's why we deal __a lot__ with the programmers' (especially Java) beloved format, XML.  

Crucial part of this system is its publishing capability.

[Presentation](http://theimowski.com/PropertyBasedTestsWithFSharp)