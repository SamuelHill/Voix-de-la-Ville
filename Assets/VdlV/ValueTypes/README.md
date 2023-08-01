# ValueTypes

`TED` tables are basically just `Arrays` of `Tuples`. As such, we are going to be storing a lot of value types - namely enums and structs. 

## Enums

Enums are named constants (the integral numeric component is less important for our usage), acting like strings in loading from a CSV or printing out to the screen.

Enums in this folder:

* `ActionType`
* `BusinessStatus`
* `DailyOperation`
* `DayOfWeek`
* `Facet`
* `InteractionType`
* `LocationCategory`
* `LocationType`
* `Month`
* `ScheduleName`
* `Sex`
* `SexualityName`
* `TimeOfDay`
* `VitalStatus`
* `Vocation`

## Structs

Structs "encapsulate data and related functionality" and should be used instead of classes for most (non-enum) types in a `TED` simulation. Any time that a value will be shared amongst tables and rows without necessarily referencing back to the same instance of a thing you should use structs. I.e. structs are a value type while classes are a reference type - use accordingly.

Structs in this folder:

* `Date`
* `Schedule`
* `Sexuality`
* `TimePoint`
* `UnorderedPair`

## Classes

There are a few times that it will be necessary to use classes - if every usage of a value in the table in referring back to the same instance. I.e. reference types.

Classes in this folder:

* `Location`
* `OrderedPair`
* `People`
