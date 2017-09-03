@{
    Layout = "post";
    Title = "Modelling bowling game score in Idris";
    Date = "2017-08-10T06:20:59";
    Tags = "Idris";
    Description = "";
}

TODO: Add abstract

<!--more-->

Bowling game score
------------------

Have you ever been in a position (just like me) where you enjoyed playing Bowling with friends, while asking yourself "*how the hell do you count points in this game?*"
It was not until I read one of my first programming books that I found out how number of knocked pins in each frame impact your final score.
The book was [Robert C. Martin - Agile Software Development, Principles, Patterns, and Practices](http://a.co/g4lENrM) and it demonstrated how to apply Test-Driven Development approach to the ten-pin bowling score problem.
Part of that chapter has also been published as a [blog post](https://sites.google.com/site/unclebobconsultingllc/home/articles/the-bowling-game-an-example-of-test-first-pair-programming).

> Note: I will not explain rules of Bowling score in this entry, since there's already a number of resources that do so. If you're not familiar with the rules, please take a look at this [wikipedia article](https://en.wikipedia.org/wiki/Ten-pin_bowling#Scoring) (traditional scoring).

This problem turns out nontrivial enough to be the topic of programming challenges such as [Coding Dojo](http://codingdojo.org/kata/Bowling/) or [Codewars](https://www.codewars.com/kata/bowling-score-calculator).

```haskell
import Data.Fin
import Data.Vect

%default total

data Frame : Type where
     Strike : Frame
     Spare : (first : Fin 10) -> Frame
     Pins : (first : Nat) -> 
            (second : Nat) ->
            {auto prf : LT (first + second) 10} -> 
            Frame

bonus : Frame -> Type
bonus Strike = (Fin 11, Fin 11)
bonus (Spare _) = (Fin 11)
bonus (Pins _ _) = ()

FrameAndBonus : Type
FrameAndBonus = (frame : Frame ** bonus frame)

data GameScore : Type where 
  MkGameScore : Vect 9 Frame -> FrameAndBonus -> GameScore

sampleGame : GameScore
sampleGame = MkGameScore 
          ([Strike, Pins 2 7, Spare 2, Strike, Strike,
            Strike, Spare 9,  Strike,  Strike])
          (Strike ** (9, 8))
```


Links:

* [Tomasz Heimowski - F# Workshop, Bowling score](http://theimowski.com/fsharp-workshops-data/#/3/2)
* [Idris website](https://www.idris-lang.org/)
* [Edwin Brady - Type Driven Development in Idris](https://www.manning.com/books/type-driven-development-with-idris)

Old Trials
* https://github.com/ToJans/idris101
* http://tojans.me/blog/2014/11/27/about-dependent-typing-idris-and-the-road-to-valhalla/
* http://ascjones.com/post/idris-bowling/

New trials
* https://deque.blog/2017/07/01/idris-bowling-kata/
* https://www.reddit.com/r/haskell/comments/6kmelb/a_type_safety_challenge_in_idris_using_dependent/
* https://github.com/QuentinDuval/IdrisBowlingKata/blob/master/Bowling.idr

<link rel="stylesheet" href="//cdnjs.cloudflare.com/ajax/libs/highlight.js/9.12.0/styles/default.min.css">
<script src="//cdnjs.cloudflare.com/ajax/libs/highlight.js/9.12.0/highlight.min.js"></script>
// https://github.com/fsprojects/FSharp.Formatting/pull/319/files