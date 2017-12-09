@{
    Layout = "post";
    Title = "Splitting Date Ranges";
    Date = "2017-12-17T04:03:37";
    Tags = "F#";
    Description = "";
}

Intro here - mention that it's not very fancy, but the algo is expressed so nicely in F# i wanted to share it

<!--more-->

<div class="message">

This post is part of the [F# Advent Calendar 2017](https://sergeytihon.com/2017/10/22/f-advent-calendar-in-english-2017/) initiative - make sure to go check out rest of posts as well.

</div>

## Background

Lately, I've faced following issue at my work:

There was a third-party API over a documents database.
This API exposed a standard endpoint to search for documents which met specified criteria.
The problem was that at the database level, there was a global max limit of documents that could be returned, and the API didn't offer **any way of paging results**.

So whenever you'd look for documents, and there were more documents meeting the search criteria than the global limit, you're basically screwed.

To demonstrate the situation, let's consider following F# code:

> Note: code presented in this post has no dependencies, so you should be able copy & run in F# 4.1 interactive no problem.

```fsharp
module ThirdPartyApi =
  open System

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
module ThirdPartyApi =
  open System

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

**Again**: there's no way to page the search results.

Leaving the API design without comments, let's see how we could workaround such limitation in F#.

## Solution

As you may have noticed, each document has its own `created` timestamp.
In addition to that search filters allow to specify a range when a document was created.

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

* date after is always exclusive,
* date before is always inclusive (hence the `OrEqual` suffix in case name).

> Note: One may wonder why [DateTimeOffset](https://msdn.microsoft.com/pl-pl/library/system.datetimeoffset(v=vs.110).aspx) is used instead of [DateTime](https://msdn.microsoft.com/pl-pl/library/system.datetime(v=vs.110).aspx). [MSDN](https://docs.microsoft.com/en-us/dotnet/standard/datetime/choosing-between-datetime) suggests that in most scenarios you should default to the first one, and that's what I usually tend to do as this structure is more explicit.

Using `DateRange` we can declare `split` function:

```fsharp
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

`NoLimitSearchError` is a type declares possible failures:

* `MinDateAfterNow` - happens whenever given minimum date is in the future
* `MinTimeSpanExceeded` - happens if by any chance there are more documents than global max limit created within minimum time span (one second in our case). The minium time span says that we cannot split a date range smaller than that.

`midBetween` function computes a date between two other dates.

Inside `split` function, `dateA` and `dateB` stand for dates for which we'll compute a date in between.

Nested `dates` function takes a mid date and splits given range into two sub-ranges.