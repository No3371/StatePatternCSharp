# StatePatternC#

A simple framework does nothing on its own, made to help you keep your code structure simple.

### Usage
The framework is composed of 2 parts: StateMachine<T> and State<T>.

Let's take **Character Movement** for example.

Imagine we have a movement system that a character can double jump, we usually have to keep track of last jump time and do some if check somewhere, lying around a script, which is messy, that's where we should apply state pattern.

``