# Unity

## GUI

Using the legacy GUI system in OnGUI.

* GUIManager contains the layout logic for OnGUI - table and string display management as well as control interfaces
* GUIString allows for display of static or updatable (via some function) strings
* GUITable allows for display of TablePredicates

## Tiles

* TileManager - handles display of locations in a TileMap (position and color) as well as the ability to click on a tile

## Simulation

* Simulation Controller - Unity.MonoBehaviour handling running the Simulation and interfacing with the GUI and Tile Managers
* Simulation Info - Info strings from TalkOfTheTown & table interfacing for TileManager
