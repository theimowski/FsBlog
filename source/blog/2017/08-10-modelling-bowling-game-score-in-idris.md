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

* [Robert C. Martin - The Bowling Game: An example of test-first pair programming](https://sites.google.com/site/unclebobconsultingllc/home/articles/the-bowling-game-an-example-of-test-first-pair-programming)
* [Robert C. Martin - Agile Software Development, Principles, Patterns, and Practices](http://a.co/g4lENrM)
* [Wiki - Scoring for Ten-pin bowling](https://en.wikipedia.org/wiki/Ten-pin_bowling#Scoring)
* [Coding Dojo - Bowling](http://codingdojo.org/kata/Bowling/)
* [Codewars - Bowling score calculator](https://www.codewars.com/kata/bowling-score-calculator)
* [Tomasz Heimowski - F# Workshop, Bowling score](http://theimowski.com/fsharp-workshops-data/#/3/2)
* [Idris website](https://www.idris-lang.org/)
* [Edwin Brady - Type Driven Development in Idris](https://www.manning.com/books/type-driven-development-with-idris)