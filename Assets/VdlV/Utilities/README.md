# Utilities

Collection of helper classes and functions. Two major categories of utility:

## C\# Utilities

Extension and Comparer classes, string and csv processing, and randomization.

* `ByteExtensions` adds Suffixes ("st", "rd", "th")
* `IntExtensions` adds Numerals ("One", "Two", ... "Thirteen Thousand Twenty Six", ...)
* `GridComparer` allows for Vector2Int comparisons (with a hashing function that doesn't _always_ collide like Unity's default)
* `CsvParsing` handles declaring `TED.CsvReader` parsers for all relevant ValueTypes
* `StringProcessing` handles `CultureInfo` stuff like TitleCase as well as all display functions for Joining lists and shortening table names
* `Randomize` maps over RNG.Next for basic types and provides pseudo-normal bell curve random scoring and various random element/shuffle functions
* `SimpleCFG` super simple CFG, similar to the more feature packed TextGenerator (used for generating names)
 
## Simulation Utilities

`Sims`, `Town`, and `Calendar` - static classes full of functions relevant to the respective underlying type (people, locations, and time).

* `Calendar` contains almost all maths for calculating any component of time (from tick to `ValueTypes` and visa versa, time since a point, or probability of occurrence in various time frames)
  * Relates all subtypes of time - `Month`, `Day`, `DayOfWeek`, `TimeOfDay`, etc - to the same standard Calendar (conceptually)
* `Sims` contains some functions for creating new people (wrap Person constructor, random adult age and sex) and the math behind fertility rates
* `Town` contains a location creation wrapper, distance calculations, and a random lot/position function
  * Random lot takes the number of lots currently active in town and expands the "borders of the town" when location density is to high
