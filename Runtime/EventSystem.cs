using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CallbackEvents
{
    public abstract class EventContext { }

    [AddComponentMenu("Callback Event System")]
    public class EventSystem : MonoBehaviour
    {
        //evenets
        public delegate Task AsyncEvent<in T>(T e);
        protected delegate void EventListener(EventContext e);
        protected delegate Task AsyncEventListener(EventContext e);
        protected Dictionary<Type, Dictionary<int, EventListener>> EventListeners;
        protected Dictionary<Type, Dictionary<int, AsyncEventListener>> AsyncEventListeners;

        //filters
        public delegate T Filter<T>(T e);
        protected delegate EventContext FilterListener(EventContext e);
        protected Dictionary<Type, Dictionary<int, FilterListener>> FilterListeners;
        
        //Debounce
        protected Dictionary<int, bool> DebounceCallbacks;
        protected Dictionary<Type, bool> DebounceFireAfter;


        // Use this for initialization
        private void Awake()
        {
            _current = this;
        }

        private static EventSystem _current;
        public static EventSystem Current
        {
            get
            {
                if (_current == null)
                {
                    _current = FindObjectOfType<EventSystem>();
                }

                return _current;
            }
        }

        public void RegisterEventListener<T>(Action<T> listener) where T : EventContext
        {
            var eventType = typeof(T);
            
            if (EventListeners == null)
            {
                EventListeners = new Dictionary<Type, Dictionary<int, EventListener>>();
            }

            if (EventListeners.ContainsKey(eventType) == false || EventListeners[eventType] == null)
            {
                EventListeners[eventType] = new Dictionary<int, EventListener>();
            }

            // Wrap a type converstion around the event listener
            void Wrapper(EventContext ei)
            {
                listener((T) ei);
            }
            EventListeners[eventType].Add(listener.GetHashCode(), Wrapper);
        }

        public bool UnregisterEventListener<T>(Action<T> listener) where T : EventContext
        {
            var eventType = typeof(T);

            if (EventListeners == null || EventListeners.ContainsKey(eventType) == false || EventListeners[eventType] == null)
            {
                return false;
            }

            return EventListeners[eventType].Remove(listener.GetHashCode());
        }

        public void RegisterAsyncEventListener<T>(AsyncEvent<T> listener) where T : EventContext
        {
            var eventType = typeof(T);
            if (EventListeners == null)
            {
                AsyncEventListeners = new Dictionary<Type, Dictionary<int, AsyncEventListener>>();
            }

            if (AsyncEventListeners.ContainsKey(eventType) == false || AsyncEventListeners[eventType] == null)
            {
                AsyncEventListeners[eventType] = new Dictionary<int, AsyncEventListener>();
            }

            // Wrap a type converstion around the event listener
            Task Wrapper(EventContext ei)
            {
                return listener((T) ei);
            }

            AsyncEventListeners[eventType].Add(listener.GetHashCode(), Wrapper);
        }

        public bool UnregisterAsyncEventListener<T>(AsyncEvent<T> listener) where T : EventContext
        {
            var eventType = typeof(T);

            if (AsyncEventListeners == null || AsyncEventListeners.ContainsKey(eventType) == false || AsyncEventListeners[eventType] == null)
            {
                return false;
            }

            return AsyncEventListeners[eventType].Remove(listener.GetHashCode());
        }


        public void FireEvent(EventContext eventContext)
        {
            var trueEventContextClass = eventContext.GetType();

            if (EventListeners == null || !EventListeners.ContainsKey(trueEventContextClass))
            {
                // No one is listening, we are done.
                return;
            }

            var keys = EventListeners[trueEventContextClass].Keys.ToArray();
            foreach (var key in keys)
            {
                EventListeners[trueEventContextClass][key](eventContext);
            }
        }

        public void FireEventAfter(EventContext eventContext, int ms, bool debounce = false)
        {
            if (debounce)
            {
                if (DebounceFireAfter == null) DebounceFireAfter = new Dictionary<Type, bool>();

                var trueEventContextClass = eventContext.GetType();
                if (DebounceFireAfter.ContainsKey(trueEventContextClass))
                {
                    if (DebounceFireAfter[trueEventContextClass]) return;

                    DebounceFireAfter[trueEventContextClass] = true;
                }
                else
                {
                    DebounceFireAfter.Add(trueEventContextClass, true);
                }
            }
            
            var coroutine = WaitFireEvent(eventContext, ms, debounce);
            StartCoroutine(coroutine);
        }

        private IEnumerator WaitFireEvent(EventContext eventContext, float ms, bool debounce)
        {
            yield return new WaitForSecondsRealtime(ms / 1000.0f);
            var trueEventContextClass = eventContext.GetType();

            if (EventListeners == null || !EventListeners.ContainsKey(trueEventContextClass)) yield break;
            
            var keys = EventListeners[trueEventContextClass].Keys.ToArray();
            foreach (var key in keys)
            {
                EventListeners[trueEventContextClass][key](eventContext);
            }

            if (debounce)
            {
                DebounceFireAfter[trueEventContextClass] = false;
            }
        }

        public void CallbackAfter(Action callback, int ms, bool debounce = false)
        {
            if (debounce)
            {
                if (DebounceCallbacks == null) DebounceCallbacks = new Dictionary<int, bool>();
                
                if (DebounceCallbacks.ContainsKey(callback.GetHashCode()))
                {
                    if (DebounceCallbacks[callback.GetHashCode()]) return;
                    
                    DebounceCallbacks[callback.GetHashCode()] = true;
                }
                else
                {
                    DebounceCallbacks.Add(callback.GetHashCode(), true);
                }
            }
            
            StartCoroutine(WaitForCallback(callback, ms, debounce));
        }

        private IEnumerator WaitForCallback(Action callback, float ms, bool debounce)
        {
            yield return new WaitForSecondsRealtime(ms / 1000.0f);

            callback();
            
            if (debounce)
            {
                DebounceCallbacks[callback.GetHashCode()] = false;
            }
        }

        //this allows all events to run at the same time
        public async Task AsyncFireEvent(EventContext eventContext)
        {
            var trueEventContextClass = eventContext.GetType();

            if (AsyncEventListeners == null || !AsyncEventListeners.ContainsKey(trueEventContextClass))
            {
                // No one is listening, we are done.
                return;
            }

            var eventTasks = new List<Task>();
            
            var keys = AsyncEventListeners[trueEventContextClass].Keys.ToArray();
            
            foreach (var key in keys)
            {
                eventTasks.Add(AsyncEventListeners[trueEventContextClass][key](eventContext));
            }
            
            await Task.WhenAll(eventTasks);
        }

        public void RegisterFilterListener<T>(Filter<T> listener) where T : EventContext
        {
            var eventType = typeof(T);
            if (FilterListeners == null)
            {
                FilterListeners = new Dictionary<Type, Dictionary<int, FilterListener>>();
            }

            if (FilterListeners.ContainsKey(eventType) == false || FilterListeners[eventType] == null)
            {
                FilterListeners[eventType] = new Dictionary<int, FilterListener>();
            }

            // Wrap a type converstion around the event listener
            EventContext Wrapper(EventContext ei)
            {
                return listener((T) ei);
            }

            FilterListeners[eventType].Add(listener.GetHashCode(), Wrapper);
        }

        public bool UnregisterFilterListener<T>(Filter<T> listener) where T : EventContext
        {
            var eventType = typeof(T);

            if (FilterListeners == null || FilterListeners.ContainsKey(eventType) == false || FilterListeners[eventType] == null)
            {
                return false;
            }

            return FilterListeners[eventType].Remove(listener.GetHashCode());
        }

        public T FireFilter<T>(EventContext eventContext) where T : EventContext
        {
            var trueEventContextClass = eventContext.GetType();

            if (FilterListeners == null || !FilterListeners.ContainsKey(trueEventContextClass))
            {
                // No one is listening, so input value is unmodified.
                return (T)eventContext;
            }

            var keys = FilterListeners[trueEventContextClass].Keys.ToArray();
            
            foreach (var key in keys)
            {
                eventContext = FilterListeners[trueEventContextClass][key](eventContext);
            }

            return (T)eventContext;
        }
    }
}
