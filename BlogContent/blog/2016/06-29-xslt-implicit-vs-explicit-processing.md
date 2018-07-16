@{
    Layout = "post";
    Title = "XSLT - Implicit vs Explicit processing";
    Date = "2016-06-29T07:26:11";
    Tags = "XSLT";
    Description = "";
}

In this first of a series of posts about XSLT (hopefully to come), I'd like to share my thoughts on 2 different approaches to processing XML documents. From a shorter and **implicit** version I will transit to a bit longer and **explicit** transform, with the latter being easier to reason about. What are the costs of trade?

<!--more-->

## Input

Let's imagine we are given a XML document with a pretty straight-forward structure like following:

    [lang=xml]
    <root>
        <a/>
        <b/>
        <a/>
        <b/>
        <a lang="it"/>
    </root>

We want to transform it so that each letter corresponds to a number:

## Output

    [lang=xml]
    <root>
        <one/>
        <two/>
        <one/>
        <two/>
        <uno/>
    </root>

This way we have a kind of mapping where `a -> one`, `b -> two`, `c -> three` etc.
On top of that we'd like to add support for multi-culture, and e.g. `lang="it"` attribute should output the number in italian (`uno`).

## Identity transform

We can start with an [identity transform](http://www.usingxml.com/Transforms/XslIdentity) - a generic XSLT transform which basically copies the input XML document to the output:

    [lang=xml]
    <xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
        <xsl:template match="@@*|node()">
            <xsl:copy>
                <xsl:apply-templates select="@@*|node()" />
            </xsl:copy>
        </xsl:template>
    </xsl:stylesheet>

> Note: If you're unfamiliar with any of the XSLT language concepts used throughout this post, you can refer to the [W3C documentation on XSLT](http://www.w3schools.com/xsl/).

The identity transform works fine for the `root` element of our XML document.
However, we need to extend the transform to process appropriately letter elements:

    [lang=xml]
    <xsl:stylesheet version="2.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
        <xsl:output encoding="UTF-8" indent="yes" method="xml" version="1.0"/>
        <xsl:template match="a">
            <one/>
        </xsl:template>
        <xsl:template match="a[@@lang = 'it']">
            <uno/>
        </xsl:template>
        <xsl:template match="b">
            <two/>
        </xsl:template>
        <xsl:template match="@@*|node()">
            <xsl:copy>
                <xsl:apply-templates select="@@*|node()" />
            </xsl:copy>
        </xsl:template>
    </xsl:stylesheet>

The above XSLT adds three more templates which translate a letter to a corresponding number in literals.
There's also a transform which matches `a` letters with `@@lang = 'it'` attribute to support different language.

Even though there are two templates which match `<a lang="it"/>` element, the latter one gets a higher priority because it is more specific.

> Note: above transform is just one of many different solutions - I've deliberately chosen it to show differences between what I call implicit and explicit processing in later course.

## More requirements

Now that we've established our basic XSLT transform, it's time to add new features.
First, we'd like to allow a `@@wrap="yes"` attribute on any input letter element:

    [lang=xml]
    <root>
        <a/>
        <b/>
        <a wrap="yes"/>
        <b wrap="yes"/>
        <a wrap="yes" lang="it"/>
    </root>

Such demarcation means that the output number element should be **wrapped** in a `<wrapping></wrapping>` element:

    [lang=xml]
    <root>
        <one/>
        <two/>
        <wrapping>
            <one/>
        </wrapping>
        <wrapping>
            <two/>
        </wrapping>
        <wrapping>
            <uno/>
        </wrapping>
    </root>

We can achieve this result by adding a new specific template to our XSLT, which makes use of the [xsl:next-match](http://www.saxonica.com/html/documentation/xsl-elements/next-match.html) instruction. The instruction applies next (in the order of priority) matching template:

    [lang=xml]
    <xsl:template match="*[@@wrap = 'yes']">
        <wrapping>
            <xsl:next-match/>
        </wrapping>
    </xsl:template>

But we've got an issue here.
If we try to run the transform, it turns out that there are **ambiguous rules**:

    [lang=bash]
    Recoverable error
      XTRE0540: Ambiguous rule match for /root/a[3]
    Matches both "a[@@lang = 'it']" on line 12 of file:/c:/sandbox/xslt/implicit_alt.xslt
    and "*[@@wrap = 'yes']" on line 3 of file:/c:/sandbox/xslt/implicit_alt.xslt

The reason is we have two templates matching `<a wrap="yes" lang="it"/>` element - both have the same priority, because they are equally specific. 

By default, XSLT processing engines will issue a warning but ignore this error (as it's [recoverable](https://www.w3.org/TR/xslt20#errors)), and the behavior is that the template defined later wins. This means that if we added the new template at the beginning of or XSLT file, we'd get invalid output.

To fix this, we can increase the priority of the new template:

    [lang=xml]
    <xsl:template match="*[@@wrap = 'yes']" priority="2">
        <wrapping>
            <xsl:next-match/>
        </wrapping>
    </xsl:template>

What value should be used for the `@@priority` attribute?
It's not that easy to answer this question, as one needs to know how **default priorities** work.
In our case, we can safely assume that value `2` is enough and will make the template more important than the other.

> Note: if you want to learn more about **default priorities** for XSLT templates, refer to [this section of XSLT docs](https://www.w3.org/TR/xslt#conflict).

Next feature we'd like to implement is to allow **ignoring** certain elements - so that they are not processed at all:

    [lang=xml]
    <root>
        <a/>
        <b/>
        <a wrap="yes"/>
        <b wrap="yes"/>
        <a wrap="yes" lang="it"/>
        <a wrap="yes" lang="it" ignore="yes"/>
    </root>

Above XML document should be transformed to following (note the last element is indeed ignored): 

    [lang=xml]
    <root>
        <one/>
        <two/>
        <wrapping>
            <one/>
        </wrapping>
        <wrapping>
            <two/>
        </wrapping>
        <wrapping>
            <uno/>
        </wrapping>
    </root>

To do this we can come up with yet another generic template - a self-closing `xsl-template` element with no content to imitate ignorance:

    [lang=xml]
    <xsl:template match="*[@@ignore = 'yes']"/>

However, we encounter the same issue again. If we tried to run the transform in this form, we'd get following error:

    [lang=bash]
    Recoverable error
      XTRE0540: Ambiguous rule match for /root/a[4]
    Matches both "a[@@lang = 'it']" on line 12 of file:/C:/sandbox/xslt/implicit_alt.xslt
    and "*[@@ignore = 'yes']" on line 8 of file:/C:/sandbox/xslt/implicit_alt.xslt

Ambiguous rule strikes again! Of course we can adjust the new template by increasing its priority:

    [lang=xml]
    <xsl:template match="*[@@ignore = 'yes']" priority="3"/>

## Implicit processing transform

This leads us to the final look of the **implicit** version of transform:

    [lang=xml]
    <xsl:stylesheet version="2.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
        <xsl:output encoding="UTF-8" indent="yes" method="xml" version="1.0"/>
        <xsl:template match="*[@@wrap = 'yes']" priority="2">
            <wrapping>
                <xsl:next-match/>
            </wrapping>
        </xsl:template>
        <xsl:template match="*[@@ignore = 'yes']" priority="3"/>
        <xsl:template match="a">
            <one/>
        </xsl:template>
        <xsl:template match="a[@@lang = 'it']">
            <uno/>
        </xsl:template>
        <xsl:template match="b">
            <two/>
        </xsl:template>
        <xsl:template match="@@*|node()">
            <xsl:copy>
                <xsl:apply-templates select="@@*|node()" />
            </xsl:copy>
        </xsl:template>
    </xsl:stylesheet>

And here comes the question: How **easy** is it to maintain and extend the above transform?
Are the `@@priority` attributes simple to follow?
In above example we have only 3 templates with the potential for ambiguity, but what if this number gets bigger and bigger? 
Those templates might turn out extremely unclear for a new-comer, who tries to figure out the control flow of XSLT transform such as above.

> Note: At first glance we might not spot the problem with this toy example - in practice, XSLT transforms grow in size really quickly and the issue becomes much more visible then.

## Explicit processing transform

To overcome the problem of managing templates with the same priority, I'd like to show an alternative **explicit** version of a XSLT transform which gives the same output:

    [lang=xml]
    <xsl:stylesheet version="2.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
        <xsl:output encoding="UTF-8" indent="yes" method="xml" version="1.0"/>
        <xsl:template match="/root">
            <root>
                <xsl:for-each select="*[not(@@ignore = 'yes')]">
                    <xsl:choose>
                        <xsl:when test="@@wrap = 'yes'">
                            <wrapping>
                                <xsl:apply-templates select="self::*"/>
                            </wrapping>
                        </xsl:when>
                        <xsl:otherwise>
                            <xsl:apply-templates select="self::*"/>
                        </xsl:otherwise>
                    </xsl:choose>
                </xsl:for-each>
            </root>
        </xsl:template>
        <xsl:template match="a">
            <one/>
        </xsl:template>
        <xsl:template match="a[@@lang = 'it']">
            <uno/>
        </xsl:template>
        <xsl:template match="b">
            <two/>
        </xsl:template>
    </xsl:stylesheet>

While the three basic templates for mapping letters to numbers are left untouched (`a -> one`, `b -> two`), the logic for **ignoring** and **wrapping** elements is embodied into the main template which matches `/root`.

Let's quickly go through the steps of the main template:

1. `xsl:for-each` instruction iterates nodes specified by the `@@select` attribute - here we take all children of currently processed element (`*`) and filter out those with `@@ignore = 'yes'` attribute,
2. `xsl:choose` instruction branches on `@@wrap = 'yes'` predicate - if the predicate is satisfied, `wrapping` element is returned first,
3. in both cases of `xsl:choose`, `xsl:apply-templates` is invoked on `self::*` which means the letter element - child element of `root`, selected by the `xsl:for-each` instruction`.

We also got rid of the identity template - we no longer need it as the main template copies `root` element as well.

## Summary

The **explicit** version despite being a bit longer than the **implicit** one, seems to be easier to follow - reader doesn't need to resolve template priorities in her head, but instead is given a clearer picture of the algorithm.

We might think of the above trick as a special kind of [Inversion of Control](https://en.wikipedia.org/wiki/Inversion_of_control) application.
Rather than specifying different templates with priorities, we take **control** over the flow:

* on the `/root` level select child nodes to process,
* filter out unwanted nodes with `[not(@@ignore = 'yes')]` XPath predicate,
* decide how to process an element based on presence of `@@wrap = 'yes'`.

Keep in mind that **explicit** processing might not always be a better choice - e.g. if the output of the transform has a very similar shape to the input, **implicit** processing can turn out to be more concise while remaining equally easy to reason about.

This concludes my thoughts on implicit and explicit ways of processing XML documents with XSLT. In future I plan to bring to the table entry about XPath language to show how one can benefit from it within XSLT. Till next time!