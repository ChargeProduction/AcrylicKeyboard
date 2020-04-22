# AcrylicKeyboard (work in progress)
A highly customizable alternative to windows touch keyboard using WPF.

## Warning: This is a very early release which may contain errors or slow code.


If you run the application you will see the following result:

![Functional layout][base_layout_example]

[base_layout_example]: https://github.com/ChargeProduction/AcrylicKeyboard/blob/master/Images/base_layout_example.jpg "Example Screenshot"


## Work in progress
* Cleanup and documentation (see cleanup branch)
* Conversion into service like application
* Api to attach to keyboard events such as visibility, window docking, key presses etc.
* Implementing Tab-Key
* Implementing Undo, Redo Keys
* Uncoupling keypress actions from keyboard into command pattern
* Adding different window modes for "picture-in-picture" and windowed keyboards

## Planned
* Adding example themes with custom key renderers and behaviour
* Extracting a renderer interface to support more platforms (e.g. UWP, Android, IOS...)
