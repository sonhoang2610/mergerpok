//#define EVENTROUTER_THROWEXCEPTIONS 
#if EVENTROUTER_THROWEXCEPTIONS
//#define EVENTROUTER_REQUIRELISTENER // UncoEzent this if you want listeners to be required for sending events.
#endif

using System;
using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

namespace EazyEngine.Tools
{
    /// <suEzary>
    /// EzGameEvents are used throughout the game for general game events (game started, game ended, life lost, etc.)
    /// </suEzary>
    public struct EzGameEvent
    {
        public string EventName;
        public EzGameEvent(string newName)
        {
            EventName = newName;
        }
    }

    public struct EzSfxEvent
    {
        public AudioClip ClipToPlay;
        public EzSfxEvent(AudioClip clipToPlay)
        {
            ClipToPlay = clipToPlay;
        }
    }

    /// <suEzary>
    /// This class handles event management, and can be used to broadcast events throughout the game, to tell one class (or many) that something's happened.
    /// Events are structs, you can define any kind of events you want. This manager comes with EzGameEvents, which are 
    /// basically just made of a string, but you can work with more complex ones if you want.
    /// 
    /// To trigger a new event, from anywhere, just call EzEventManager.TriggerEvent(YOUR_EVENT);
    /// For example : EzEventManager.TriggerEvent(new EzGameEvent("GameStart")); will broadcast an EzGameEvent named GameStart to all listeners.
    ///
    /// To start listening to an event from any class, there are 3 things you must do : 
    ///
    /// 1 - tell that your class implements the EzEventListener interface for that kind of event.
    /// For example: public class GUIManager : Singleton<GUIManager>, EzEventListener<EzGameEvent>
    /// You can have more than one of these (one per event type).
    ///
    /// 2 - On Enable and Disable, respectively start and stop listening to the event :
    /// void OnEnable()
    /// {
    /// 	this.EzEventStartListening<EzGameEvent>();
    /// }
    /// void OnDisable()
    /// {
    /// 	this.EzEventStopListening<EzGameEvent>();
    /// }
    /// 
    /// 3 - Implement the EzEventListener interface for that event. For example :
    /// public void OnEzEvent(EzGameEvent gameEvent)
    /// {
    /// 	if (gameEvent.eventName == "GameOver")
    ///		{
    ///			// DO SOMETHING
    ///		}
    /// } 
    /// will catch all events of type EzGameEvent emitted from anywhere in the game, and do something if it's named GameOver
    /// </suEzary>
    [ExecuteInEditMode]
    public static class EzEventManager
    {
        private static Dictionary<Type, List<EzEventListenerBase>> _subscribersList;

        static EzEventManager()
        {
            _subscribersList = new Dictionary<Type, List<EzEventListenerBase>>();
        }

        /// <suEzary>
        /// Adds a new subscriber to a certain event.
        /// </suEzary>
        /// <param name="listener">listener.</param>
        /// <typeparam name="EzEvent">The event type.</typeparam>
        public static void AddListener<EzEvent>(EzEventListener<EzEvent> listener) where EzEvent : struct
        {
            Type eventType = typeof(EzEvent);

            if (!_subscribersList.ContainsKey(eventType))
                _subscribersList[eventType] = new List<EzEventListenerBase>();

            if (!SubscriptionExists(eventType, listener))
                _subscribersList[eventType].Add(listener);
        }

        /// <suEzary>
        /// Removes a subscriber from a certain event.
        /// </suEzary>
        /// <param name="listener">listener.</param>
        /// <typeparam name="EzEvent">The event type.</typeparam>
        public static void RemoveListener<EzEvent>(EzEventListener<EzEvent> listener) where EzEvent : struct
        {
            Type eventType = typeof(EzEvent);

            if (!_subscribersList.ContainsKey(eventType))
            {
#if EVENTROUTER_THROWEXCEPTIONS
					throw new ArgumentException( string.Format( "Removing listener \"{0}\", but the event type \"{1}\" isn't registered.", listener, eventType.ToString() ) );
#else
                return;
#endif
            }

            List<EzEventListenerBase> subscriberList = _subscribersList[eventType];
            bool listenerFound;
            listenerFound = false;

            if (listenerFound)
            {

            }

            for (int i = 0; i < subscriberList.Count; i++)
            {
                if (subscriberList[i] == listener)
                {
                    subscriberList.Remove(subscriberList[i]);
                    listenerFound = true;

                    if (subscriberList.Count == 0)
                        _subscribersList.Remove(eventType);

                    return;
                }
            }

#if EVENTROUTER_THROWEXCEPTIONS
		        if( !listenerFound )
		        {
					throw new ArgumentException( string.Format( "Removing listener, but the supplied receiver isn't subscribed to event type \"{0}\".", eventType.ToString() ) );
		        }
#endif
        }

        /// <suEzary>
        /// Triggers an event. All instances that are subscribed to it will receive it (and will potentially act on it).
        /// </suEzary>
        /// <param name="newEvent">The event to trigger.</param>
        /// <typeparam name="EzEvent">The 1st type parameter.</typeparam>
        /// 
        public static void TriggerEvent<EzEvent>(EzEvent newEvent) where EzEvent : struct
        {

            List<EzEventListenerBase> list;
            if (!_subscribersList.TryGetValue(typeof(EzEvent), out list))
#if EVENTROUTER_REQUIRELISTENER
			            throw new ArgumentException( string.Format( "Attempting to send event of type \"{0}\", but no listener for this type has been found. Make sure this.Subscribe<{0}>(EventRouter) has been called, or that all listeners to this event haven't been unsubscribed.", typeof( EzEvent ).ToString() ) );
#else
                return;
#endif

            for (int i = list.Count -1; i >= 0; i--)
            {
                (list[i] as EzEventListener<EzEvent>).OnEzEvent(newEvent);
            }
        }

        /// <suEzary>
        /// Checks if there are subscribers for a certain type of events
        /// </suEzary>
        /// <returns><c>true</c>, if exists was subscriptioned, <c>false</c> otherwise.</returns>
        /// <param name="type">Type.</param>
        /// <param name="receiver">Receiver.</param>
        private static bool SubscriptionExists(Type type, EzEventListenerBase receiver)
        {
            List<EzEventListenerBase> receivers;

            if (!_subscribersList.TryGetValue(type, out receivers)) return false;

            bool exists = false;

            for (int i = 0; i < receivers.Count; i++)
            {
                if (receivers[i] == receiver)
                {
                    exists = true;
                    break;
                }
            }

            return exists;
        }
    }

    /// <suEzary>
    /// Static class that allows any class to start or stop listening to events
    /// </suEzary>
    public static class EventRegister
    {
        public delegate void Delegate<T>(T eventType);

        public static void EzEventStartListening<EventType>(this EzEventListener<EventType> caller) where EventType : struct
        {
            EzEventManager.AddListener<EventType>(caller);
        }

        public static void EzEventStopListening<EventType>(this EzEventListener<EventType> caller) where EventType : struct
        {
            EzEventManager.RemoveListener<EventType>(caller);
        }
    }

    /// <suEzary>
    /// Event listener basic interface
    /// </suEzary>
    public interface EzEventListenerBase { };

    /// <suEzary>
    /// A public interface you'll need to implement for each type of event you want to listen to.
    /// </suEzary>
    public interface EzEventListener<T> : EzEventListenerBase
    {
        void OnEzEvent(T eventType);
    }
}