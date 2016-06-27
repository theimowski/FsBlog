@{
    Layout = "post";
    Title = "XSLT - Implicit vs Explicit processing";
    Date = "2016-06-21T07:26:11";
    Tags = "XSLT";
    Description = "";
}

In this first of a series of posts about XSLT (hopefully to come), I'd like to share my thoughts on 2 different approaches to processing XML documents. From a shorter and **implicit** version I will transit to a bit longer and **explicit** transform, with the latter being easier to reason about. Fair compromise I think.

<!--more-->

## Input

Let's imagine we are given a XML document with a pretty straight-forward structure like the following:

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
In addition to that we'd like to add support for multi-culture, and e.g. `lang="it"` attribute should output the number in italian (`uno`).

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

The above XSLT adds three more templates which translate a letter to a corresponding literal word.
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

We can achieve this result by adding a new specific template to our XSLT, which makes use of the [xsl:next-match](http://www.saxonica.com/html/documentation/xsl-elements/next-match.html) instruction:

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

This is because we have now two templates which match a `<a wrap="yes"/>` element - and both have the same priority, because they are equally specific. 

By default, XSLT processing engines will issue a warning but ignore this error (because it's "recoverable"), and the behavior is that the template defined later wins. This means that if we added the new template at the beginning of or XSLT file, we'll get incorrect output.

To fix this, we can increase the priority of the new template:

    [lang=xml]
    <xsl:template match="*[@@wrap = 'yes']" priority="2">
        <wrapping>
            <xsl:next-match/>
        </wrapping>
    </xsl:template>

What value should be used for the `@@priority` attribute?
It's not that easy to answer this question, as knowledge of how **default priorities** work is necessary.
In our case, we can safely assume that `2` is ok and will make the template more important than the other.

> Note: if you want to learn more about **default priorities** for XSLT templates, refer to [this section of XSLT docs](https://www.w3.org/TR/xslt#conflict).

---

    [lang=xml]
    <root>
        <a/>
        <b/>
        <a wrap="yes"/>
        <b wrap="yes"/>
        <a wrap="yes" lang="it"/>
        <a wrap="yes" lang="it" ignore="yes"/>
    </root>

---

    [lang=xml]
    <?xml version="1.0" encoding="UTF-8"?>
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

## Implicit processing transform

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

## Explicit processing transform

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

## Summary