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
We have set up a farm of servers to distribute the requests and speed up the regression testing.
The software can queue requests, however there's a **limited number** of requests that can run in parallel on a single server (usually 4).
Because of that, I decided to come up with a solution in F# which tries to distribute the requests as optimal as possible.

## Idea

Initially I tried to split the input set into equal chunks, each for separate server.
This worked quite well, but after a while it turned out that requests which targetted one server finished much earlier than others, and the **fastest server was idle** for the rest of time.
Because of that I started looking for another solution.
Browsing the web, I came across [this](http://tomasp.net/blog/async-sequences.aspx/) article by Tomas Petricek, where the author described how to work with asynchronous sequences in F#.
Provided samples and references led me to think of a different idea.

I wanted to implement an algorithm, where each of the servers would initally take the maximum number of requests (4 in my case).
Then whenever a response was back from one of the servers, this server would acquire next request from the queue.
The pattern would continue iteratively until no more requests were found in the queue.
I pre-ordered the queue descending by input size, so that the potentially longest-running computations could go in first turn.  

## Code

The solution depends on a few libraries, which we can install with [Paket](http://fsprojects.github.io/Paket/):

```
source http://nuget.org/api/v2

nuget FSharp.Control.AsyncSeq
nuget FSharpx.Async
```

And following is a snippet implementing the load balancer:

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

We can imagine `callService` to be a function that makes the actual http call to a web service.
 
The `process` function takes two parametrs: machine (server) name, and input.
It constructs a proper requests and fires it towards destination server.

Most important part comes in `renditions` function:

* the function takes a list of machines and inputs as its parameters
* `workersCount` stands for the maximum amount of computations performed on a single server
* `renditionsCount` is just the number of inputs to process
* `asyncSeq` starts an [Async Sequence](http://fsprojects.github.io/FSharp.Control.AsyncSeq/index.html) computation expression
* inside the computation expression, there are two queues: one for computations and the other for actual results of those computations
    * the queues are of type `BlockingQueueAgent` - this type from [FSharpx.Async](http://fsprojects.github.io/FSharpx.Async/index.html) library implements queue as an agent in blocking fashion 
    * they allow us to keep track of requests yet to be processed, as well as already processed results 
* recursive `worker` function (inspired by [this](https://github.com/tpetricek/FSharp.AsyncExtensions/blob/master/samples/Crawler.fsx#L92-L123) excerpt):
    * is parametrized by `machine` argument
    * extracts a pending request from `queue`
    * processes the result
    * adds the result to `result` queue
    * recursively invokes itself to process another request
    * all of above actions are performed asynchronously 
* the `asyncSeq` computation expression ends with following three iterations:
    * each input is added to the `queue`
    * specified amount of workers are fired (based on `workersCount`) for each machine
    * the results are `yielded` from the `results` queue - we expect precisely `renditionsCount` outputs
* return value of the computation expression is finally "piped" (`|>`) to `Async` combinators, so that the return type of `renditions` function is just an array of results (not wrapped inside `Async`)

Above code managed to meet my expectations - load balance was performed dynamically, based on server capacity at a specific point in time.
Unfortunately I haven't made any benchmarks to measure the speedup, but at least I observed that those faster servers were not idle anymore, and didn't have to wait for others to complete their computations.

## Links

For further reading, I'd like to mention again a couple of resources I used when implementing the solution:

* [FSharp.Control.AsyncSeq](http://fsprojects.github.io/FSharp.Control.AsyncSeq/index.html)
* [FSharpx.Async](http://fsprojects.github.io/FSharpx.Async/index.html)
* [Tomas Petricek's blog post on programming with asynchronous sequences](http://tomasp.net/blog/async-sequences.aspx/)
* [Example of web crawler which also make use of `BlockingQueueAgent`](https://github.com/tpetricek/FSharp.AsyncExtensions/blob/master/samples/Crawler.fsx#L92-L123)