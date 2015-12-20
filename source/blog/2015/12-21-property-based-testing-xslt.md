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

## Prelude

I've been constantly postponing creation of my own blog.
There were [books](http://amzn.com/1617292397) and [video courses](http://pluralsight.com/courses/get-involved) which did encourage me to start, however I wasn't confident enough to do so.
The [F# Advent](https://sergeytihon.wordpress.com/tag/fsadvent/) event convinced me to break the ice - "at least I'll have any readers", I thought.   
This is my first blog post ever, so chances are it's not gonna be the best F# article you've ever read.
Anyway bear with me - I think I've got something interesting to share.

## Automatic publishing

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
Verbose syntax (xsl transformation itself must be a valid XML), dynamic typing, immature tooling or template matching ambiguity are IMO the biggest cons of working with XSLT. 
It is based on __functional concepts__ though, which after a while makes it a bit more attractive than it initially seemed to be.
Be aware, one day I might even happen to write a post or two on XSLT only (that's what they call [Stockholm syndrome](https://en.wikipedia.org/wiki/Stockholm_syndrome), isn't it?).
I'm playing a devil's advocate, you may think, but there's one gloomy thing about this [DSL](https://en.wikipedia.org/wiki/Domain-specific_language) I'll have to admit: 
XSLT can get really __hard to maintain__ and tricky after it reaches a certain level of complexity.
That's why we have automated tests suite in our code-base, just to address XSL transformations.
Majority of them are written in F# using a powerful library for property-based testing, [FsCheck](https://fscheck.github.io/FsCheck/).

> If you're new to the concept of property-based testing and using FsCheck library, I highly recommend reading [this](http://fsharpforfunandprofit.com/posts/property-based-testing/) introductory article.

## DITA XML

Let's have a quick glance at the DITA XML standard first, to grasp the idea of how the documents are stored.

    [lang=xml]
    <topic>
        <title>My first publication</title>
        <body>
            <p>Hello <b>world!</b></p>
        </body>
    </topic>

Above snippet describes a basic document, which contains a `title` element as well as `body` and a `p` (paragraph) inside that body.
Both `title` and `body` are enclosed in root `topic` element.
Such notation may look familiar to you already - DITA XML is akin to HTML markup.
Inside the `body` we can also have such elements as `image`, `table`, `h1`, `h2`, etc. 

> To make things simple, snippets that follow are "slimmed" versions of original code.
> I was too lazy to verify if after slimming they even compile correctly - so please treat them as __pseudo-code__.
> After all the idea itself is most important. 

## Generator

FsCheck can automatically generate random input for tests as long as generators for corresponding data types are registered within the assembly.
The library comes with a few pre-registered generators for: primitive types, F# records or Discriminated Unions.
However, in order to generate more fancy data structures, we have to do some manual work.

    let title = gen {
        let! contents = contents
        return XElement("title", contents)
    }
    
    let body = gen {
        let! items = Gen.oneOf [ p; table; image ] |> Gen.listOf
        return XElement("body", items)
    }
    
    let topic = gen {
        let! title = title
        let! body = body
        return XElement("topic", title, body)
    }

To produce DITA XML documents, I use the XML object model from `System.Xml.Linq` namespace and `gen` computation expression from FsCheck.
Given such granular generators, it's very convenient to compose them together - e.g. `topic` element generator makes use of `title` and `body` element generators.

<!-- TODO: more on generators? -->

## Tests

Implementation of a couple of helper functions is skipped for brevity as well, let's just assume we are given the following:

    /// takes path to the XSLT file and input document
    /// returns the result of transformation
    val xsltTransform : string -> XDocument -> XDocument

    /// pretty-print XML Document for inspection
    val format : XDocument -> string
    
    /// checks whether XML cocument conforms to the provided XML Schema
    val conformsToSchema : XDocument -> bool
    
    /// generic function to traverse the XML tree
    /// can evaluate to string, bool, or "evaluator" (iterable sequence of nodes)
    /// use with caution! - it throws exceptions e.g. in case of invalid casts
    val xpath<'a> : string -> XNode -> 'a
    
    /// takes input and output XML document
    /// returns sequence of pairs of objects (input * output)
    /// where an object is XML element representing a table or image
    val allObjects : XDocument * XDocument -> seq<XElement * XElement>

    /// determines proper width for "layout" attribute
    /// e.g. "narrow" can be "80mm" and "medium" can be "120mm" 
    val layoutToWidth : string -> string

All tests are written with the help of [FsCheck.Xunit](https://fscheck.github.io/FsCheck/RunningTests.html) adapter - each property is a separate test marked with `[<Property>]` attribute.

### Conforming to XML schema

First test verifies if for any valid input XML (determined by our generator), output of the transformation conforms to a XML Schema provided by the vendor of PDF rendition software.

    [<Property>]
    let ``modifier XML conforms to schema`` topic =
        let output = xsltTransform "topic.xslt" topic
        (format output) @@| (conformsToSchema output)
        
The `@@|` operator used in line 4 allows to "label" failing properties.
If for some reason the transform produces illegal XML and the property does not hold, 
test failure report will include both input `topic`, as well as `output`.
This allows to quickly spot the cause of failure.

> In its extended version, this test can also report schema validation error messages, e.g. that a specific element is not a valid child of its parent. 
        
Thanks to the above test, we can eliminate any issue related to producing XML with invalid schema, which would always result in rendition failure. 
XML Schema safety within XSLT can also be guaranteed with [Schema-Aware XSLT](http://www.stylusstudio.com/schema-aware.html).
While Schema-Aware XSLT processors usually require a commercial license, we can maintain the schema-conforming test in our code-base for free.

### Bolded text

Next test makes sure that all text enclosed in the `<b>` tag should be indeed __bold__ in the output PDF:

    [<Property>]
    let ``if text node under "b" element then richtext has bold`` (topic) =
        let output = xsltTransform "topic.xslt" topic
        let textNodes = topic  |> xpath "//text()"
        let richtexts = output |> xpath "//RICHTEXT"
    
        (format output) @@|
            ((textNodes, richtexts)
            ||> Seq.zip
            |> Seq.filter (fst >> xpath "boolean(ancestor::b)")
            |> Seq.forAll (snd >> xpath "@@BOLD = 'TRUE'"))
            
The provided schema specifices `RICHTEXT` elements as containers for text.
All text within a single `RICHTEXT` is formatted in uniform fashion.
Therefore, `RICHTEXT` elements form a flattened list of formatted text.
To verify that `@@BOLD` attribute is given on a proper `RICHTEXT`, we first collect all `textNodes` from input and `richtexts` from the output (lines 4,5).
In next step, we use `Seq.zip` to create a sequence of pairs of corresponding items (line 9).
Then in line 10 we filter those pairs, where text node has `<b>` ancestor tag.
Finally we check with `Seq.forAll` that all `RICHTEXT` elements have `@@BOLD` attribute set to `TRUE` (line 11). 

Likewise, we can write tests for checking other types of text formatting, i.e. _italic_,<ins>underline</ins>,<sup>superscript</sup> or <sub>subscript</sub>. 

### Width of images and tables

Third, and the last property-based test presented in this entry checks if every "object" (image or table) with specified layout has correct width in output:

    [<Property>]
    let ``objects with specified layout have correct width`` topic =
        let output = xsltTransform "topic.xslt" topic
        let pairs = allObjects(topic, output)
                    |> Seq.filter (fst >> xpath "@@layout != ''")
        let attributeValues : seq<string * string> = 
            pairs |> Seq.map (fun (i,c) -> 
                                    xpath "string(@@layout)" i, 
                                    xpath "string(*/@@WIDTH)" c)

        (format output) @@| 
            (attributeValues 
             |> Seq.forall (fun (a,b) -> layoutToWidth a = b))

Symbol `pairs` in line 4 is bound to sequence of pairs of input * output objects, filtered to only those which have `@@layout` attribute (which don't fall back to the default layout). 
This sequence is then mapped in lines 6-9 to a pair of input's `@@layout` and outputs's `@@WIDTH` attributes.
Finally, in lines 11-13, with the help of `Seq.forall` and `layoutToWidth` helper function, all pairs are checked for correct width.

At first the test may seem needless, because it concerns a simple mapping from one value to another.
In practice however it turned out quite useful mainly for __regression__ purposes, whereas restructuring the code or introducing new features could break this property.  

## Shrinker

Another important concept of property-based testing is shrinking.
In short, shrinkers allow to find __minimal input__ which fails the test.
Let's consider the property which checked proper "boldness" of a piece of text.
If we spoil code which turns on the `@@BOLD` attribute, that property could fail for example on following input:

    [lang=xml]
    <topic>
        <title>My first publication</title>
        <body>
            <image href="unicorn.pdf">
            <p>Hello <b>world!</b></p>
            <table>
                <title>table</title>
                <tbody>
                    <row>
                        <entry>aaa</entry>
                    </row>
                </tbody>
            </table>
        </body>
    </topic>

It can be hard at once to say what exactly caused the test to fail, as this XML document is already quite complex.
How about shrinking the above XML to a smaller version:
    
    [lang=xml]
    <topic>
        <title/>
        <body>
            <p><b>w</b></p>
        </body>
    </topic>

The latter, shrinked data shows much clearer where the transform went wrong.
It would be also hard to find smaller input, which still makes the test light red.

Implementing a shrinker for XML document turned out to be quite __challenging__ and won't be described here, but maybe one day I'll put up a separate post dedicated only to this issue. 

## Conclusions

I'd like to thank [Sergey Tihon](https://twitter.com/sergey_tihon) once again for organizing [F# Advent](https://sergeytihon.wordpress.com/tag/fsadvent/) - if not this opportunity the blog wouldn't see the daylight for a long time.

If you found this entry interesting, you may check out my [presentation](http://theimowski.com/PropertyBasedTestsWithFSharp) on this topic (created with [FsReveal](https://github.com/fsprojects/FsReveal)) which I prepared for my colleagues at work.
That's it for now - Till next time!