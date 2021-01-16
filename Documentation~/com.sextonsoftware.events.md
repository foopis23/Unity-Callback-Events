# Callback Event System
This System was designed to handle all Game Events genericly so that the programmer can write less boilerplate code. This is done by combining event data and event type definations into one class.

This system was created using https://github.com/quill18/UnityCallbackAndEventTutorial as starting point.

## Usage
To use the event system, you must have an object with the Callback Event System Component Attached.

### Defining Event Type
To define a new event type, a new class needs to be defined that extends `CallbackEvents.EventContext`. Any members or methods can be added to this class as it will also be the data context container you pass when firing the event.

#### Example
```cs
using CallbackEvents;

//defines a Power Activated Event
public class PowerActivatedContext : EventContext
{
    public Player Player;

    public PlayerMovementInfo(Player player)
    {
        this.Player = player;
    }
}
```

### Register an Event Listener
To register an event listener, we need to access the Event System's current instance. The current instance can be accessed through `CallbackEvents.EventSystem.Current`. Once we have access to the current instance we just need to register or event listener with the `RegisterEventListener<T>(System.Action<T> callback)` method. A callback method for an event listener is defined as a function that return void and takes one argument of type T where T : EventContext.

#### Example
```cs
...
using CallbackEvents;

public class LightsController : MonoBehaviour {
    public GameObject[] lights; //set in the unity editor

    void Start() {
        //register event listener
        EventSystem.Current.RegisterEventListener<PowerActivatedContext>(OnPlayerActivated); 
    }

    //Event Callback Function for Power Activated Event
    public void OnPlayerActivated(PowerActivatedContext context) {
        Debug.log($"{context.Player.name} has activated the power");

        foreach(GameObject light in lights) {
            light.setActive(true);
        }
    }
}
```

### Unregister an Event Listener
Unregistering an event listener is very similiar to registering an event listener. Events should be unregister if the script that as listening to the events is about to be destroyed, but not because a new scene is loading (All events listeners are cleared between scene loads if the Event System GameObject is allowed to be destroyed which is the default behavior). To uneregister an event listener we need the Event System's current instance. The current instance can be accessed through `CallbackEvents.EventSystem.Current`. After we have access to the instance, we can use `UnregisterEventListener<T>(System.Action<T> callback)`. The callback parameter is the same value as registering the event.

#### Example
```cs
...
using CallbackEvents;

public class LightsController : MonoBehaviour {
    public GameObject[] lights; //set in the unity editor

    void Start() {
        //register event listener
        EventSystem.Current.RegisterEventListener<PowerActivatedContext>(OnPlayerActivated); 
    }

    void OnDestroy() {
        EventSystem es = GameEventSystem.Current;

        if  (es == null) return; //on scene destruction, the game event system may have been destroyed already. This catches that

        //unregister event listener
        es.UnregisterEventListener<PowerActivatedContext>(OnPlayerActivated);
    }

    //Event Callback Function for Power Activated Event
    public void OnPlayerActivated(PowerActivatedContext context) {
        Debug.log($"{context.Player.name} has activated the power");

        foreach(GameObject light in lights) {
            light.setActive(true);
        }
    }
}
```

### Fire an Event
To fire an event, we again need access to the Event System's current intance. The current instance can be accessed through `CallbackEvents.EventSystem.Current`. Once we have this reference, we can use the `FireEvent(EventContext e)` method to fire an event. The event passed through in the argument is used to figure out what event type we are firing.  

#### Example
```cs
...
using CallbackEvents;

public class PowerSwitchController : MonoBehaviour
{
    //this method is called from some other class. Player Class, Interaction Manager, etc..
    public void ActivatePower(Player player) {
        //Fires Power Activated Event
        EventSystem.Current.FireEvent(new PowerActivatedContext(player));
    }
}
```

### Define Filter Type
Defining a filter type is the exact same as defining an event type. Event types defined for events can also be used in filters.

#### Example
```cs
using CallbackEvents;

public class PlayerSpeedContext : EventContext {
    public Player player;
    public float baseSpeed;
    public float filteredSpeed;

    public PlayerSpeedContext(Player player, float baseSpeed) {
        this.player = player;
        this.baseSpeed = baseSpeed;
        this.filteredSpeed = baseSpeed;
    }
}
```

### Register Filter Listener
Registering a filter listener is very similiar to registering an event listener. To register we need access to the current instance of the Event System, which can be access through `CallbackEvents.EventSystem.Current`. After that we can use the `RegisterFilterListener<T>(CallbackEvents.EventSystem.Filter<T> callback)` method. The callback parameter is now a method that returns type T and takes type T as a parameter where T : EventContext. 

#### Example
```cs
...
using CallbackEvents;

public class AreaOfEffectSpeedBoost : MonoBehaviour {
    public List<Player> playersEffected;

    void Start() {
        playersEffected = new List<Player>();

        //register filter listener
        EventSystem.RegisterFilterListener<PlayerSpeedContext>(OnPlayerFilterSpeed);
    }

    //Filter Callback function
    PlayerSpeedContext OnPlayerFilterSpeed(PlayerSpeedContext context) {
        if (playersEffected.contains(context.player)) {
            context.filteredSpeed += baseSpeed * 0.1;
        }

        //return modified value so it can be passed to the next filter
        return context;
    }

    void OnTriggerEnter(Collider collider) {
        //Adds Player to Effected Player
        ...
    }

    void OnTriggerExit(Collider collider) {
        //Removes Player from Effected Players
        ...
    }
}
```

### Unregister Filter Listener
This process is exactly the same as unregistering an event, but now with a filter callback instead of an event callback. We use the `UnregisterFilterListener<T>(CallbackEvents.EventSystem.Filter<T> callback)` to do so.

#### Example
```cs
...
using CallbackEvents;

public class AreaOfEffectSpeedBoost : MonoBehaviour {
    public List<Player> playersEffected;

    void Start() {
        playersEffected = new List<Player>();

        //register filter listener
        EventSystem.RegisterFilterListener<PlayerSpeedContext>(OnPlayerFilterSpeed);
    }

    void OnDestroy() {
        EventSystem es = GameEventSystem.Current;

        if  (es == null) return; //on scene destruction, the game event system may have been destroyed already. This catches that

        //unregister filter listener
        EventSystem.UnregisterFilterListener<PlayerSpeedContext>(OnPlayerFilterSpeed);
    }

    //Filter Callback function
    PlayerSpeedContext OnPlayerFilterSpeed(PlayerSpeedContext context) {
        if (playersEffected.contains(context.player)) {
            context.filteredSpeed += baseSpeed * 0.1;
        }

        //return modified value so it can be passed to the next filter
        return context;
    }

    void OnTriggerEnter(Collider collider) {
        //Adds Player to Effected Player
        ...
    }

    void OnTriggerExit(Collider collider) {
        //Removes Player from Effected Players
        ...
    }
}
```

### Fire Filter
Firing a filter is very similiar to firing an event. We need access to the Event System's current instance. To get that, we can access `CallbackEvents.EventSystem.Current`. To fire the event, use the `FireFilter<T>(EventContext e)` method. When firing a filter, the type returned is set to type T where T : EventContext. The returned value is the result of all of the filters' modifications. If there are no filters listening, the orginal object is returned.   

#### Example
```cs
...
using CallbackEvents;

public class Player : MonoBehaviour {

    public float movementSpeed = 10.0f;

    void Update() {
        Vector2 input = new Vector2();
        //collect input
        ...

        if (input.magnitude > 0.001) { //checks if the player is attempting to move
            
            //Fires Filter and returns result of all filters applied
            PlayerSpeedContext res = EventSystem.Current.FireFilter<PlayerSpeedContext>(new PlayerSpeedContext(this, movementSpeed));

            //get the result of the filter out of the container object
            float filteredMovementSpeed = res.filteredSpeed;

            //apply movement with new filteredSpeed
            ApplyMovement(input, filteredMovementSpeed);
        }
    }

    void ApplyMovement(Vector2 input, float speed) {
        //Applys player input and speed to make player move
        ...
    }
}
```