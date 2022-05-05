# Callback Event System
This system was designed so that the programmer can write less boilerplate code 
for event systems. This is done by combining event data and event type 
definitions into one class. This system is not supposed to replace unity events,
but rather fill the role of a global event bus. This is great for games that want
to implement the systemic design.

This system was created using 
https://github.com/quill18/UnityCallbackAndEventTutorial as starting point.

## Getting Started
- [API Reference](https://github.com/foopis23/Unity-Callback-Events/wiki/API-Reference)
## Concepts
### Event
Is a trigger that does not receive feedback from the callbacks that are 
listening to it. Think of it as someone basically shouting into a microphone 
what is currently happening to them at any given moment. Imagine you wanted to 
implement a heat mechanic. You can have a fire that is throwing events saying 
"It's HOT RIGHT HERE" and heat-sensitive listeners listening for that.

### Filter
Is a trigger that does receive feedback from the listeners. This is a way to 
override specific data coming through a filter pipeline. Say, for example, you
want to modify the damage amount for a sword when a certain ability is active. You
could achieve this with a filter that emits whenever sword damage is used and a 
filter listener that increases the damage with the ability to be active.

### EventContext
The type of Event or Filter is denoted by the EventContext Type. When registering 
an event listener, a generic is passed which represents not only the first
 parameter of the callback functions but also the type of event the listener is
 listening to. For example, if you have two events, like heat and rain, you
 would make two classes that extend EventContext that contain all of the data
 you would want to pass to the event listeners. No EventContext should use
 EventContext directly. EventContext is just an empty class to be extended.

