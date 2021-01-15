using UnityEngine;
using System.Collections.Generic;

namespace CallbackEvents
{
    //test
    public abstract class EventContext { }

    [AddComponentMenu("Callback Event System")]
    public class EventSystem : MonoBehaviour
    {
        //evenets
        protected delegate void EventListener(EventContext e);
        protected Dictionary<System.Type, List<EventListener>> eventListeners;

        //filters
        public delegate T Filter<T>(T e);
        protected delegate EventContext FilterListener(EventContext e);
        protected Dictionary<System.Type, List<FilterListener>> filterListeners;


        // Use this for initialization
        void Awake()
        {
            __Current = this;
        }

        private static EventSystem __Current;
        public static EventSystem Current
        {
            get
            {
                if (__Current == null)
                {
                    __Current = GameObject.FindObjectOfType<EventSystem>();
                }

                return __Current;
            }
        }

        public void RegisterEventListener<T>(System.Action<T> listener) where T : EventContext
        {
            System.Type eventType = typeof(T);
            if (eventListeners == null)
            {
                eventListeners = new Dictionary<System.Type, List<EventListener>>();
            }

            if (eventListeners.ContainsKey(eventType) == false || eventListeners[eventType] == null)
            {
                eventListeners[eventType] = new List<EventListener>();
            }

            // Wrap a type converstion around the event listener
            EventListener wrapper = (ei) => { listener((T)ei); };

            eventListeners[eventType].Add(wrapper);
        }

        public bool UnregisterEventListener<T>(System.Action<T> listener) where T : EventContext
        {
            System.Type eventType = typeof(T);

            if (eventListeners == null || eventListeners.ContainsKey(eventType) == false || eventListeners[eventType] == null)
            {
                return false;
            }

            return eventListeners.Remove(eventType);
        }

        public void FireEvent(EventContext EventContext)
        {
            System.Type trueEventContextClass = EventContext.GetType();

            if (eventListeners == null || !eventListeners.ContainsKey(trueEventContextClass))
            {
                // No one is listening, we are done.
                return;
            }

            foreach (EventListener el in eventListeners[trueEventContextClass])
            {
                el(EventContext);
            }
        }

        public void RegisterFilterListener<T>(Filter<T> listener) where T : EventContext
        {
            System.Type eventType = typeof(T);
            if (filterListeners == null)
            {
                filterListeners = new Dictionary<System.Type, List<FilterListener>>();
            }

            if (filterListeners.ContainsKey(eventType) == false || filterListeners[eventType] == null)
            {
                filterListeners[eventType] = new List<FilterListener>();
            }

            // Wrap a type converstion around the event listener
            FilterListener wrapper = (ei) => { return listener((T)ei); };

            filterListeners[eventType].Add(wrapper);
        }

        public bool UnregisterFilterListener<T>(Filter<T> listener) where T : EventContext
        {
            System.Type eventType = typeof(T);

            if (eventListeners == null || eventListeners.ContainsKey(eventType) == false || eventListeners[eventType] == null)
            {
                return false;
            }

            return eventListeners.Remove(eventType);
        }

        public T FireFilter<T>(EventContext EventContext) where T : EventContext
        {
            System.Type trueEventContextClass = EventContext.GetType();

            if (filterListeners == null || !filterListeners.ContainsKey(trueEventContextClass))
            {
                // No one is listening, so input value is unmodified.
                return (T)EventContext;
            }

            foreach (FilterListener el in filterListeners[trueEventContextClass])
            {
                EventContext = el(EventContext);
            }

            return (T)EventContext;
        }
    }
}
