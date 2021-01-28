using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CallbackEvents
{
    //test
    public abstract class EventContext { }

    [AddComponentMenu("Callback Event System")]
    public class EventSystem : MonoBehaviour
    {
        //evenets
        public delegate Task AsyncEvent<T>(T e);
        protected delegate void EventListener(EventContext e);
        protected delegate Task AsyncEventListener(EventContext e);
        protected Dictionary<System.Type, List<EventListener>> eventListeners;
        protected Dictionary<System.Type, List<AsyncEventListener>> asyncEventListeners;

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

        public void RegisterAsyncEventListener<T>(AsyncEvent<T> listener) where T : EventContext
        {
            System.Type eventType = typeof(T);
            if (eventListeners == null)
            {
                asyncEventListeners = new Dictionary<System.Type, List<AsyncEventListener>>();
            }

            if (asyncEventListeners.ContainsKey(eventType) == false || asyncEventListeners[eventType] == null)
            {
                asyncEventListeners[eventType] = new List<AsyncEventListener>();
            }

            // Wrap a type converstion around the event listener
            AsyncEventListener wrapper = (ei) => { return listener((T)ei); };

            asyncEventListeners[eventType].Add(wrapper);
        }

        public bool UnregisterAsyncEventListener<T>(AsyncEvent<T> listener) where T : EventContext
        {
            System.Type eventType = typeof(T);

            if (asyncEventListeners == null || asyncEventListeners.ContainsKey(eventType) == false || asyncEventListeners[eventType] == null)
            {
                return false;
            }

            return asyncEventListeners.Remove(eventType);
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

        public void FireEventAfter(EventContext EventContext, int ms)
        {
            IEnumerator coroutine = WaitFireEvent(EventContext, ms);
            StartCoroutine(coroutine);
        }

        private IEnumerator WaitFireEvent(EventContext EventContext, float ms)
        {
            yield return new WaitForSeconds(ms / 1000.0f);
            System.Type trueEventContextClass = EventContext.GetType();

            if (eventListeners != null && eventListeners.ContainsKey(trueEventContextClass))
            {
                foreach (EventListener el in eventListeners[trueEventContextClass])
                {
                    el(EventContext);
                }
            }
        }

        public void CallbackAfter(System.Action callback, int ms)
        {
            StartCoroutine(WaitForCallback(callback, ms));
        }

        private IEnumerator WaitForCallback(System.Action callback, float ms)
        {
            yield return new WaitForSeconds(ms / 1000.0f);
            callback();
        }

        //this allows all events to run at the same time
        public async Task AsyncFireEvent(EventContext EventContext)
        {
            System.Type trueEventContextClass = EventContext.GetType();

            if (asyncEventListeners == null || !asyncEventListeners.ContainsKey(trueEventContextClass))
            {
                // No one is listening, we are done.
                return;
            }

            List<Task> eventTasks = new List<Task>();

            foreach (AsyncEventListener el in asyncEventListeners[trueEventContextClass])
            {
                eventTasks.Add(el(EventContext));
            }

            await Task.WhenAll(eventTasks);
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
