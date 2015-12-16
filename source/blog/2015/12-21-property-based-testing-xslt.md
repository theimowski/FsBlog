@{
    Layout = "post";
    Title = "Property-based testing XSLT";
    Date = "2015-12-21T07:11:55";
    Tags = "XSLT,F#,Property-based testing,Functional programming";
    Description = "";
}

Property-based testing is a wonderful tool to verify correctness of your programs. 
However, some people struggle with finding reasonable cases to use the technique.
In this entry, I'll try to prove that property-based testing can be used in every-day coding by showing a rather __exotic example__ of applying property-based testing to check results of XSL transformations.

<!--more-->

<div class="message">

This post is part of the [F# Advent Calendar in English 2015](https://sergeytihon.wordpress.com/tag/fsadvent/) action. 
Big thanks go to [Sergey Tihon](https://twitter.com/sergey_tihon) for organizing such a great initiative.
Make sure to go check out rest of posts as well.

</div>

I've been constantly postponing creation of my own blog.
There were [books](http://amzn.com/1617292397) and [video courses](http://pluralsight.com/courses/get-involved) which did encourage me to start, however I wasn't confident enough to do so.
The [F# Advent](https://sergeytihon.wordpress.com/tag/fsadvent/) event convinced me to break the ice - "at least I'll have any readers", I thought.   
This is my first blog post ever, so chances are it's not gonna be the best F# article you've ever read.
Anyway bear with me - I think I've got something interesting to share.

My job is to develop and maintain a __Content Management System__ (CMS). 
The company I work for is a big corporation and because big corporation often equals __enterprise__ software, we deal __a lot__ with the programmers' (especially Java) beloved enterprise format, namely XML.
We store all the documents in the XML format, conforming to the [DITA XML standard](http://dita.xml.org/) (with slight customizations).   

Crucial part of the system is its __publishing__ capability.
In order to render PDF documents, we utilize third-party software.
Within the software, the whole process of rendering printable documents can be cleverly automated by reusing a common template and applying different sets of content to it.
To apply some content to the template, input format of the content must be XML which conforms to a __provided schema__ (other than DITA).
How do we prepare the input to manipulate the template?
XML in, XML out - you guessed it, we do __XSLT__.

XSLT probably doesn't stand for one of the finest tools that every developer likes to work with.
Verbose syntax (which itself must be a valid XML), dynamic typing, immature tooling or template matching ambiguity are IMO the biggest cons of XSLT. 
It is based on __functional concepts__ though, which after a while makes it a bit more attractive than it initially seemed to be.
Be aware, one day I might even happen to write a post or two on XSLT only (that's what they call [Stockholm syndrome](https://en.wikipedia.org/wiki/Stockholm_syndrome), isn't it?).
I'm playing a devil's advocate, you may think, but there's one gloomy thing about this [DSL](https://en.wikipedia.org/wiki/Domain-specific_language) I'll have to admit: 
XSLT can get really __hard to maintain__ and tricky after it reaches a certain level of complexity.
That's why we have automated tests suite in our code-base, just to address XSL transformations.
Majority of them are written in F# using a powerful library for property-based testing, [FsCheck](https://fscheck.github.io/FsCheck/).

<div class="message">

If you're new to the concept of property-based testing and using FsCheck library, I highly recommend reading [this](http://fsharpforfunandprofit.com/posts/property-based-testing/) introductory article.
Code snippets that follow also assume some basic knowledge of traversing the XML tree - don't hesitate to browse the web if you need any revision on this topic.

</div>

## DITA XML

    [lang=xml]
    <topic>
        <title>My first publication</title>
        <body>
            <p>Hello <b>world!</b></p>
        </body>
    </topic>

## Generator

    let title = gen {
        let! contents = contents
        return XElement("title", contents)
    }
    
    let body = gen {
        let! items = Gen.oneOf [ para; table; chart ] |> Gen.listOf
        return XElement("body", items)
    }
    
    let topic = gen {
        let! title = title
        let! body = body
        return XElement("topic", title, body)
    }

## Tests

Let's move straight to the first test. 
I'll skip all the boring boilerplate code necessary for setting up the `System.Xml.Linq` library and helper functions.

<!-- maybe don't skip ? -->

### Conforming to XML schema

    [<Property>]
    let ``modifier XML conforms to schema`` (topic: XDocument) =
        let output = xsltTransform "topic.xslt" topic
        doesNotThrow (fun () -> schema.Validate output)

### Proper bold attribute

    [<Property>]
    let ``if text node under "b" element then richtext has bold`` (topic) =
        let output = xsltTransform "topic.xslt" topic
        let textNodes = topic  |> xpath "//text()"
        let richtexts = output |> xpath "//RICHTEXT"
    
        (textNodes, richtexts)
        ||> Seq.zip
        |> Seq.filter (fst >> xpath "ancestor::b")
        |> Seq.forAll (snd >> xpath "attribute::BOLD = 'TRUE'")

## Shrinker

[Presentation](http://theimowski.com/PropertyBasedTestsWithFSharp)

<!-- mention suave in japanese -->
