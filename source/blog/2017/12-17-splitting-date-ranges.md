@{
    Layout = "post";
    Title = "Splitting Date Ranges";
    Date = "2017-12-17T04:03:37";
    Tags = "F#";
    Description = "";
}

The power and beauty of F# pattern matching are often underestimated.
To shine more light on them I decided to share code fragments which exercise pattern matching to deal with date ranges.
This will be a simple divide and conquer implementation for a workaround over poorly designed API.

<!--more-->

<div class="message">

This post is part of the [F# Advent Calendar 2017](https://sergeytihon.com/2017/10/22/f-advent-calendar-in-english-2017/) initiative - make sure to go check out rest of posts as well.

</div>

## Background

Lately, I've faced following issue at my work:

There was a third-party API over a documents database.
This API exposed a standard endpoint to search for documents which met specified criteria.
The problem was that at the database level, there was a global max limit of documents that could be returned, and the API didn't offer **any way of paging results**.

So whenever you'd look for documents, and there were more documents meeting the search criteria than the global limit, you were basically **screwed**.

To demonstrate the situation, let's consider following F# code:

> Note: code presented in this post has no dependencies, so you should be able copy & run in FSI (F# Interactive) 4.1 no problem.

```fsharp
open System

module ThirdPartyApi =

  type DocType =
  | DocTypeA
  | DocTypeB
  | DocTypeC

  type Document =
    { DocId   : int
      Type    : DocType
      Created : DateTimeOffset
      Content : string
      (* more properties ... *) }

  // 2000-01-01 00:00:00 UTC
  let baseDate = DateTimeOffset(2000, 1, 1, 0, 0, 0, 0, TimeSpan.Zero)

  let documents : Document [] =
    Array.init 100000 (fun i ->
      let index = i + 1
      let typ =
        match i % 3 with
        | 0 -> DocTypeA
        | 1 -> DocTypeB
        | 2 -> DocTypeC
        | _ -> failwith "should not happen"

      { DocId   = index
        Type    = typ
        Created = baseDate + TimeSpan.FromHours (float index)
        Content = sprintf "Document no. %d" index })
```

This is a fake in-memory model of the documents database.
Each document has its id, type, content, and date of creation.
In reality, the documents have a lot of other properties.
We populate the array with 100K sample documents, each of which is created within an hour timespan.

We can now simulate the behavior of the mentioned search API with following:

```fsharp
  type Filters =
    { Type                 : DocType option
      CreatedBeforeOrEqual : DateTimeOffset option
      CreatedAfter         : DateTimeOffset option
      (* more filters ... *) }

  let globalMaxLimit = 1000

  let search filters =
    async {
      let results =
        documents
        |> Array.filter
          (fun doc -> 
            match filters.Type with
            | Some t -> doc.Type = t 
            | None   -> true)
        |> Array.filter
          (fun doc ->
            match filters.CreatedAfter with
            | Some a -> doc.Created > a
            | None   -> true)
        |> Array.filter
          (fun doc ->
            match filters.CreatedBeforeOrEqual with
            | Some b -> doc.Created <= b
            | None   -> true)

      if results.Length > globalMaxLimit then
        return Array.take globalMaxLimit results
      else
        return results
    }
```

The relevant part of the snippet is the `globalMaxLimit` value (1000), and `search` result.
Note that no matter what filters you apply to the search, results get trimmed whenever they exceed the limit.

Let's see what happens if we try to find all `DocTypeA` documents:

```fsharp
let onlyDocA : ThirdPartyApi.Filters =
  { Type                 = Some ThirdPartyApi.DocTypeA
    CreatedBeforeOrEqual = None
    CreatedAfter         = None  }

ThirdPartyApi.search onlyDocA
|> Async.RunSynchronously
|> fun results -> printfn "Third Party Api results length: %d" results.Length

// Third Party Api results length: 1000
```

We hit the max limit, and **again**: there's no way to page the results.

Leaving the API design without comments, let's see how we could workaround such limitation in F#.

## Solution

As you may have noticed, each document has its own `created` timestamp.
Search filters allow to specify a date range when a document was created.

This means that we could take the original search definition filters and **split** a date range into two sub-ranges whenever max limit is exceeded.
Results for those two sub-ranges could be merged afterwards.

This idea is just another application of a known [Divide and conquer algorithm](https://en.wikipedia.org/wiki/Divide_and_conquer_algorithm).

Let's start with a type definition for a date range:

```fsharp
type DateRange =
| Unbounded
| BeforeOrEqual of DateTimeOffset
| After         of DateTimeOffset
| Between       of DateTimeOffset * DateTimeOffset
```

The type is pretty self-explanatory.
What's also worth mentioning is that for the ranges we'll assume that:

* date after is always exclusive (after but not equal),
* date before is always inclusive (before or equal) - hence the `OrEqual` suffix in case name.

> Note: One may wonder why [DateTimeOffset](https://msdn.microsoft.com/pl-pl/library/system.datetimeoffset(v=vs.110).aspx) is used instead of [DateTime](https://msdn.microsoft.com/pl-pl/library/system.datetime(v=vs.110).aspx). [MSDN](https://docs.microsoft.com/en-us/dotnet/standard/datetime/choosing-between-datetime) suggests that in most scenarios you should default to the first one, and that's what I usually tend to do as this structure is more explicit.

Using `DateRange` we can declare `split` function:

```fsharp
type Result<'a,'b> =
| Ok    of 'a
| Error of 'b

type NoLimitSearchError =
| MinDateAfterNow     of minDate : DateTimeOffset * now : DateTimeOffset
| MinTimeSpanExceeded of TimeSpan

let minTimeSpan = TimeSpan.FromSeconds 1.0

let midBetween (dateA : DateTimeOffset) (dateB : DateTimeOffset) =
  let diff = dateB - dateA
  let halfDiff = TimeSpan(diff.Ticks / 2L)
  if halfDiff < minTimeSpan then
    Error (MinTimeSpanExceeded minTimeSpan)
  else
    Ok (dateA + halfDiff)

let getNowDate () =
  DateTimeOffset.UtcNow

let split minDate range =
  let now = getNowDate()
  if minDate > now then
    Error (MinDateAfterNow (minDate, now))
  else
    let dateA, dateB =
      match range with
      | Unbounded       -> minDate, now
      | BeforeOrEqual b -> minDate, b
      | After a         -> a, now
      | Between (b, a)  -> b, a

    let dates mid =
      match range with
      | Unbounded       -> BeforeOrEqual mid, After mid
      | BeforeOrEqual b -> BeforeOrEqual mid, Between (mid, b)
      | After a         -> Between (a, mid), After mid
      | Between (a, b)  -> Between (a, mid), Between (mid, b)

    midBetween dateA dateB
    |> Result.map dates
```

The `split` function takes a minimum date as first argument.
This helps restricting the date range, and is usually an easy to determine value.

`NoLimitSearchError` is a type that declares possible failures:

* `MinDateAfterNow` - happens whenever the minimum date is in the future
* `MinTimeSpanExceeded` - happens if by any chance there are more documents than global max limit created within minimum time span (one second in our case). The minium time span says that we cannot split any further a date range.

`midBetween` function computes a date between two other dates.

Inside `split` function, `dateA` and `dateB` stand for dates for which we'll compute a date in between.

Nested `dates` function takes a mid date and splits given range into two sub-ranges.

> Note: We're using [Result type](https://docs.microsoft.com/en-us/dotnet/fsharp/language-reference/results) for denoting that function may return error. If you want to learn more about the approach, have a look into [ROP](https://fsharpforfunandprofit.com/rop/) introduction. I added the definition of this type, so that tooltip engine can infer types correctly (Result type was introduced in F# 4.1)

I'd like to pay your attention for a second to the **beauty of pattern matching** in above snippet. This was de-facto the reason why I decided to share this blog post in first place.

Note, how thanks to the `DateRange` DU we're easily able to specify relevant `split` logic.
For me personally, this is a **big F# win**.

## Wrapping it up

Let's see how we can use the split function to define a workaround for the inital issue:

```fsharp
let fromFilters (filters : ThirdPartyApi.Filters) =
  let before = filters.CreatedBeforeOrEqual
  let after  = filters.CreatedAfter

  match before, after with
  | Some before, Some after -> Between (before, after)
  | Some before, None       -> BeforeOrEqual before
  | None, Some after        -> After after
  | None, None              -> Unbounded

let apply range (filters : ThirdPartyApi.Filters) =
  let filters =
    { filters with 
        CreatedBeforeOrEqual = None
        CreatedAfter         = None }

  match range with
  | Unbounded ->
    filters
  | BeforeOrEqual b ->
    { filters with CreatedBeforeOrEqual = Some b }
  | After a ->
    { filters with CreatedAfter = Some a }
  | Between (a, b) ->
    { filters with
        CreatedAfter         = Some a
        CreatedBeforeOrEqual = Some b }

let searchNoLimit minDate filters =
  let rec search range =
    async {
      let filters' = apply range filters
      let! results = ThirdPartyApi.search filters'
      if results.Length < ThirdPartyApi.globalMaxLimit then
        return Ok results
      else
        match split minDate range with
        | Ok (firstRange, secondRange) ->
          let! results = Async.Parallel [| search firstRange; search secondRange |]
          match results with
          | [| Ok a; Ok b |] -> return Ok (Array.append a b)
          | [| Error e; _ |]
          | [| _; Error e |] -> return Error e
          | _                -> return failwith "unexpected, array should have 2 elems"
        | Error e ->
          return Error e
    }

  search (fromFilters filters)
```

The `fromFilters` and `apply` functions serve as simple helpers to go from `ThirdPartyApi.Filters` to `DateRange` type and back. Once again, pattern matching proves helpful in both cases.

Most interesting part is the `searchNoLimit` function:

* nested `search` function takes a `DateRange` and invokes the original api,
* if results are within the global limit we're good,
* otherwise split the `DateRange` and:
  * invoke `search` recursively in parallel for both sub-ranges, `firstRange` and `secondRange`,
  * append results of sub-ranges into single array.

Let's see if we can find now all `DocTypeA` documents:

```fsharp
let minDate = ThirdPartyApi.baseDate

match searchNoLimit minDate onlyDocA |> Async.RunSynchronously with
| Ok results ->
  printfn "No Limit results length: %d" results.Length
| Error e ->
  printfn "No Limit failed with: %A" e

// No Limit results length: 33334
```

As expected, we get back now 1/3 of all documents from the database.

## Be aware

The `searchNoLimit` function will invoke recursively until it's able to find all relevant documents.
This means that it might not be always appropriate to invoke - e.g. when your search filters are too liberal, you might attempt to download **all** documents from database.

Because of that, please be aware that this workaround might not work for all cases.
Next step could be to take the function and extend it to actually page the results.

## Recap

In this entry we saw how we can workaround a third-part API limitation by writing a divide and conquer algorithm in F#. We also saw how pattern matching help us declare relevant logic in a nice and concise way.

Till next time, Merry Christmas!