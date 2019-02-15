# HISTORY

### 1.1.4
* 2019-02-15 RIL
* New: Commands to toggle-control visibility of the Menu ("m") and Header ("h") in Console Window mode.
* New: Colors moved into a "Theme" struct.
* New: Added test project XUnit (more tests to be added later)
* New: Rewrite of the main app into a class ConsoleApp.
* New: Added version info to the menu header.
* Fix: Some minor bugs fixed in file listings and color-coding of the output.
* Some code refactoring.


### 1.0.3
* Added options to hide menu ("m" toggle)and menu-header ("h" toggle). 
* Separated drawing of menu to Draw methods

### 1.0.2
* Fixed erroneous log message in command for removing temp folders (_gzip was reported as _ghx)