@{
    Layout = "post";
    Title = "Dynamic load balancing in F#";
    Date = "2016-04-27T04:13:26";
    Tags = "F#,Async,Paket";
    Description = "";
}

In this short entry I'll present how one can create a small load balancer in F# to optimize distribution of a set of computations between multiple nodes.
The example will make use of powerful F# libraries: 
[FSharp.Control.AsyncSeq](http://fsprojects.github.io/FSharp.Control.AsyncSeq/index.html)
as well as 
[FSharpx.Async](http://fsprojects.github.io/FSharpx.Async/index.html).

<!--more-->

## Use case

Today I faced an interesting problem at work - for the purpose of regression testing we run several dozens of web service calls, each of which takes a noticable amount of time (even up to 120 seconds).
These calls target a third-party software which renders a document in PDF format.
We have set up a farm of boxes to distribute the requests and speed up the regression testing.
The software can queue requests, however there's a limited number of requests that can run in parallel on a single server.
Because of that, I decided to come up with a solution in F# which tries to distribute the requests as optimal as possible.

## Dependencies

The solution depends on a few libraries, which we can install with [Paket](http://fsprojects.github.io/Paket/):

```
source http://nuget.org/api/v2

nuget FSharp.Control.AsyncSeq
nuget FSharpx.Async
```

## Code

```
open System

#r @@"packages/FSharp.Control.AsyncSeq/lib/net45/FSharp.Control.AsyncSeq.dll"
open FSharp.Control

#r @@"packages/FSharpx.Async/lib/net40/FSharpx.Async.dll"
open FSharpx.Control

let process machine input =
    let url = sprintf "http://%s/webservice/endpoint" machine
    
    async {
        // call a web service with a given input (file)
        return! callService url input
    }

let renditions (machines: string list) (inputs: string list) =
    
    let workersCount = 4
    let renditionsCount = inputs.Length

    asyncSeq {
        let queue = BlockingQueueAgent<_>(renditionsCount)
        let results = BlockingQueueAgent<_>(renditionsCount)

        let rec worker machine  = async {
            let! input = queue.AsyncGet()
            let! result = process machine input 
                
            do! results.AsyncAdd (result)
            do! worker machine
        }

        for input in inputs do
            do! queue.AsyncAdd(input)

        for machine in machines do
            for i in 0 .. workersCount - 1 do
                worker machine |> Async.Start
        
        for i in 0 .. renditionsCount - 1 do 
            let! result = results.AsyncGet()
            yield result
    }
    |> AsyncSeq.toArrayAsync
    |> Async.RunSynchronously
```


## Links

For further reading, here are a couple of resources I used when implementing the solution:

* [FSharp.Control.AsyncSeq](http://fsprojects.github.io/FSharp.Control.AsyncSeq/index.html)
* [FSharpx.Async](http://fsprojects.github.io/FSharpx.Async/index.html)
* [Tomas Petricek's blog post on programming with asynchronous sequences](http://tomasp.net/blog/async-sequences.aspx/)
* [Example of web crawler which also make use of `BlockingQueueAgent`](https://github.com/tpetricek/FSharp.AsyncExtensions/blob/master/samples/Crawler.fsx#L92-L123)