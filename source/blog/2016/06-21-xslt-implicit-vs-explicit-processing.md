@{
    Layout = "post";
    Title = "XSLT - Implicit vs Explicit processing";
    Date = "2016-06-21T07:26:11";
    Tags = "XSLT";
    Description = "";
}

Intro goes here

<!--more-->

## Use case

    [lang=xml]
    <people>
        <person firstName="John" lastName="Doe">
            <phone type="mobile">0123456789</phone>
        </person>
        <person firstName="Alice" lastName="Brown" email="alice.brown@example.com">
            <phone type="home">5555555555</phone>
        </person>
    </people>

### Identity transform

    [lang=xml]
    <xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
        <xsl:template match="@@*|node()">
            <xsl:copy>
                <xsl:apply-templates select="@@*|node()" />
            </xsl:copy>
        </xsl:template>
    </xsl:stylesheet>

## XSLT 1.0 - implicit

    [lang=xml]
    <people>
        <person firstName="John" lastName="Doe" phone="0123456789"/>
        <person firstName="Alice" lastName="Brown" email="alice.brown@example.com" phone="5555555555"/>
    </people>

### Implicit processing transform

    [lang=xml]
    <xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
        <xsl:template match="person">
            <xsl:copy>
                <xsl:apply-templates select="@@*" />
                <xsl:attribute name="phone"><xsl:value-of select="phone"/></xsl:attribute>
            </xsl:copy>
        </xsl:template>
        <xsl:template match="@@*|node()">
            <xsl:copy>
                <xsl:apply-templates select="@@*|node()" />
            </xsl:copy>
        </xsl:template>
    </xsl:stylesheet>

## XSLT > 2.0 - explicit

    [lang=xml]
    <contacts>
        <cellPhone guestId="1">0123456789</cellPhone>
        <homePhone guestId="2">5555555555</homePhone>
        <emailAddress guestId="2">alice.brown@example.com</emailAddress>
    </contacts>