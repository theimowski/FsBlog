@{
    Layout = "post";
    Title = "Paket workflow for testing new NuGet package before release";
    Date = "2016-05-21T08:45:31";
    Tags = "Paket,Git,FAKE,ProjectScaffold,F#,C#,NuGet,Continuous delivery";
    Description = "";
}

Modularity is becoming more and more popular nowadays. 
Managing separately maintained components in .NET ecosystem is usually achieved by using **NuGet** packages.
Unfortunately with current NuGet approach, there's no easy way of testing new changes in referenced projects.
Today I want to show you how we can make our lifes easier with brand new [Paket](http://fsprojects.github.io/Paket/) feature: [paket.local](http://fsprojects.github.io/Paket/local-file.html).

<!--more-->
    
The story
---------

Let's imagine that there's an awesome library available on NuGet.
It's called `ToyPaketCalculator` ([link](https://www.nuget.org/packages/ToyPaketCalculator/0.0.1-beta)) and it allows to add or multiply two integer numbers.
The library got very popular, and everyone is now using it to perform complex calculations.

We decide to use it for our new project called `CalculatorClient` as well.
It works quite nicely and after a while we come up with idea for a new feature in `ToyPaketCalculator`.
The new feature would allow one to substract two integers.
To make the dreams come true, we want to contribute to `ToyPaketCalculator`, but ideally we'd like to test the new feature against our `Calculatorclient` application before submitting a Pull Request. 
    
Calculator Client project
--------------------------------

Our new `CalculatorClient` project will be a simple console app written in C# - create one with your favourite editor / IDE.
To pull the `ToyPaketCalculator` package we'll use [Paket](http://fsprojects.github.io/Paket/).

First, we need to [download paket.bootstrapper](https://github.com/fsprojects/Paket/releases/latest) and save it in `.paket` directory in the root of our codebase.  

Next, we invoke a couple of paket commands from the command line (see also a [list of editor plugins](http://fsprojects.github.io/Paket/editor-support.html) if not comfortable with CLI):

### Download Paket


    [lang=bash]
    $ .paket\paket.bootstrapper.exe prerelease
    
Above command will download a **prerelease** (as of the time of writing) version of Paket, which is 3.0.0-beta and which contains the new feature described in this post.

### Init Paket


    [lang=bash]    
    $ .paket\paket.exe init

Init command will initialize our codebase for Paket usage, i.e. it will create a basic `paket.dependencies` file.

### Install ToyPaketCalculator package


    [lang=bash]
    $ .paket\paket.exe add nuget ToyPaketCalculator -i
    
This command will add `ToyPaketCalculator` package to our `paket.dependencies` and in interactive mode (`-i` flag) will ask whether we want to add the package to available projects within our codebase. Prompt to install the package to console project:

    [lang=bash]
    Install to c:\github\ToyPaketCalculatorClient\ToyPaketCalculatorClient\ToyPaketCalculatorClient.csproj into group Main?
        [Y]es/[N]o => Y
   
Now we can use the package in our console app: 
    
    [lang=csharp]
    using System;

    namespace ToyPaketCalculatorClient
    {
        class Program
        {
            static void Main(string[] args)
            {
                Console.WriteLine(ToyPaketCalculator.Calculator.add(3, 5));
            }
        }
    }

Running this program should result in printing "8" to the console:

![8](8.png)

If we however switch `add` to `substract`, we won't be able to compile - the function is not yet implemented:

![no substract](no_substract.png)

Clone ToyPaketCalculator Project
----------------------

Now, in order to add the new feature to `ToyPaketCalculator`, we first need to pull down its sources:

    [lang=bash]
    $ git clone https://github.com/theimowski/ToyPaketCalculator.git
    
Solution file for this project resides in the root of the repository.

Add new feature to ToyPaketCalculator
----------------------------
    
It's usually a good practice to create separate branch per each feature we're working on.
For that reason let's open a new branch in `ToyCalculatorProject`:

    [lang=bash]
    $ git checkout -b new_feature

Now we're ready to implement our beloved `substract` function. 
Turns out that the project is written in F#, but hey even if you don't know the language don't give up - it should be rather easy: 

    [lang=fsharp]
    module ToyPaketCalculator.Calculator
  
    let add x y = x + y

    let mult x y = x * y

    let substract x y = x - y

(`src\ToyPaketCalculator\Library.fs`)

We can commit our changes:

    [lang=bash]
    $ git commit -am "implement substract function"

Fine, but how can we test `substract` function from our `CalculatorClient` now? 
We surely don't want end up creating a pull request only to later find out that our implementation was buggy.
 
> Note: Normally in this case we would add appropriate automated tests to the `ToyPaketCalculator` project to verify the behavior, however keep in mind above example is only for demonstration purposes and in practical use cases stuff might not be that easy to test in isolation, and/or we would like to test it from the referencing project anyway.
 
paket.local to the rescue!
-------------------------    

Let's navigate back to our `CalculatorClient` project and create `paket.local` file with a single line:

    [lang=paket]
    nuget ToyPaketCalculator -> git file:///c:\github\ToyPaketCalculator new_feature build:"build.cmd NuGet", Packages: /bin/

Above line stands for source override.
Splliting it to parts:

* `nuget ToyPaketCalulator` token identifies NuGet package that the override corresponds to,
* `->` splits the package id (left side) and actual override (right side),
* `git` relates to [git dependency](http://fsprojects.github.io/Paket/git-dependencies.html#Using-Git-repositories-as-NuGet-source) feature,
* `file:///c:\github\ToyPaketCalculator` points to a local directory - place where we cloned the project,
* `new_feature` is the name of the branch to be used from `ToyPaketCalculator` project,
* `build:"build.cmd NuGet"` denotes a single-line build command, which produces a NuGet package (`.nupkg`),
* `Packages: /bin/` points to a directory within the referenced project where the `.nupkg` file should be located after `build.cmd NuGet` succeeds.

> Note: don't forget to add `paket.local` to `.gitignore`- we don't want to keep track and share with other team members our local file paths!

Now when we invoke `paket restore`, we get the following:

    [lang=bash]
    $ .paket\paket.exe restore
    Paket version 3.0.0.0
    paket.local override: nuget ToyPaketCalculator -> file:///c:\github\ToyPaketCalculator new_feature build:"build.cmd NuGet", Packages: /bin/
    Cloning file:///c:\github\ToyPaketCalculator to C:\Users\tomasz.heimowski\.paket\git\db\ToyPaketCalculator
    Setting c:\github\ToyPaketCalculatorClient\paket-files\localfilesystem\ToyPaketCalculator to e16e10eca854a61f586945181aa45e65e0059aa1
    Running "c:\github\ToyPaketCalculatorClient\paket-files\localfilesystem\ToyPaketCalculator\build.cmd NuGet"
    50 seconds - ready.

In third line we can see that Paket warns us about the override.
This is to remind us that the package used during restore has been taken from a source other than the one pinned in `paket.lock`.  

Test new feature
----------------

After successful restore with overriden `ToyPaketCalculator` package, we can test the new NuGet package even though it has not been released anywhere yet.
Paket handles `.nupkg` archive extraction for us, so all we have to do is move to the `Program.cs` file and invoke `substract` function - code should now compile just fine:

    [lang=csharp]
    using System;

    namespace ToyPaketCalculatorClient
    {
        class Program
        {
            static void Main(string[] args)
            {
                Console.WriteLine(ToyPaketCalculator.Calculator.substract(3, 5));
            }
        }
    }

Running modified program should print expected result to the output:

![minus 2](minus_2.png)

Create pull request / release
-------------------

Now that we are confident with the change, we can create a pull request to the referenced project.
In case we own the referenced project, then we can simply release new version 

> Note: if you have just rolled out a new project you might want to check out how releases can be a piece of cake with the cool [ProjectScaffold](http://fsprojects.github.io/ProjectScaffold/release-process.html)

Hopefully the workflow I've described in this post can help .NET devs maintain their components and save plenty of time, which otherwise would be wasted on manual copying of binaries or watching failing builds on CI servers.

If you find any issue / bug in `paket.local` feature, please create a corresponding issue in [Paket](https://github.com/fsprojects/Paket/issues).

Below are links to github repos for both `ToyPaketCalculator` as well as the client project:

* https://github.com/theimowski/ToyPaketCalculator
* https://github.com/theimowski/ToyPaketCalculatorClient

Till next time!