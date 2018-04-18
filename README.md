# StateController
A C# PlugAndPlay StateController to apply state pattern to any class.

## Why
I love state pattern, it's really useful especially in Game Development. But I hate to redo a class or inherit a class to something for state pattern. I want something easy and simple that I can attach to any class I want.

## How
This library is full Generic, it can be adapted by any type of class.
Simply initialize a StateController<T> any where for your T class, assign an initial state and kick it start.
StateController update the active state on your demand, for example, in Unity, you can call the StateController.Update() in Unity's Update so it updates as fast as framerate.

A StateController<T> does all the transition between state, whenever it's requested to update, it calls the Update() of the current state.

A State<T> does all the works for its Target(The T), it contains OnEntered(), Update(), OnLeaving():
  - OnEntered() gets called when StateController has entered this state.
  - Update() gets called when StateController is requested to Update.
  - OnLeaving() gets called when StateController is leaving this state.

## Usage
A Unity example:
