open System

module ThirdPartyApi =

  type Document =
    { Type    : string 
      Created : DateTimeOffset
      Content : string }

  let documents =
    Array.init 10000 (sprintf "Document %i")

  type Filters =
    { DocumentType         : string
      CreatedBeforeOrEqual : DateTimeOffset option
      CreatedAfter         : DateTimeOffset option
      (* more filters ... *) }

  type SearchError =
  | SearchResultExceededMaxLimit of int

  let globalMaxLimit = 1000

  let search filters =
    async {
      return Ok []
    }

type DateRange =
| Unbounded
| BeforeOrEqual of DateTimeOffset
| After         of DateTimeOffset
| Between       of DateTimeOffset * DateTimeOffset

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
      let! result = ThirdPartyApi.search filters'
      match result with
      | Ok results ->
        return Ok results
      | Error (ThirdPartyApi.SearchResultExceededMaxLimit _) ->
        match split minDate range with
        | Ok (firstRange, secondRange) ->
          let! results = Async.Parallel [| search firstRange; search secondRange |]
          match results with
          | [| Ok a; Ok b |] -> return Ok (List.append a b)
          | [| Error e; _ |]
          | [| _; Error e |] -> return Error e
          | _                -> return failwith "unexpected, array should have 2 elems"
        | Error e ->
          return Error e
    }

  search (fromFilters filters)