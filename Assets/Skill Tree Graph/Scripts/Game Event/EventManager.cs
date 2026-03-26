using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameEvents
{
    public static class EventManager
    {
        private static readonly Dictionary<Type, IEventListenerBase> _typeToRegistry = new Dictionary<Type, IEventListenerBase>();

        public static IDisposable AddListener<T>(Action<T> action, int priority = 0) where T : struct
        {
            return new EventListenerWrapper<T>(action, priority);
        }

        public static void AddListener<T>(IEventListener<T> listener, int priority = 0) where T : struct
        {
            GetRegistry<T>().Add(listener, priority);
        }

        public static void RemoveListener<T>(IEventListener<T> listener) where T : struct
        {
            GetRegistry<T>().Remove(listener);
        }

        public static void TriggerEvent<T>(T eventData) where T : struct
        {
            GetRegistry<T>().Trigger(eventData);
        }

        private static EventRegistry<T> GetRegistry<T>() where T : struct
        {
            Type type = typeof(T);
            if (!_typeToRegistry.TryGetValue(type, out var registry))
            {
                registry = new EventRegistry<T>();
                _typeToRegistry[type] = registry;
            }
            return (EventRegistry<T>)registry;
        }

        private interface IEventListenerBase { }

        private class EventRegistry<T> : IEventListenerBase where T : struct
        {
            private struct PrioritizedListener
            {
                public IEventListener<T> Listener;
                public int Priority;
            }

            private readonly List<PrioritizedListener> _listeners = new List<PrioritizedListener>();
            private readonly List<PrioritizedListener> _toAdd = new List<PrioritizedListener>();
            private readonly List<IEventListener<T>> _toRemove = new List<IEventListener<T>>();
            private bool _isTriggering;
            private bool _needsSorting;

            public void Add(IEventListener<T> listener, int priority)
            {
                var entry = new PrioritizedListener { Listener = listener, Priority = priority };

                if (_isTriggering)
                {
                    _toAdd.Add(entry);
                }
                else
                {
                    if (!Contains(listener))
                    {
                        _listeners.Add(entry);
                        _needsSorting = true;
                    }
                }
            }

            public void Remove(IEventListener<T> listener)
            {
                if (_isTriggering)
                {
                    _toRemove.Add(listener);
                }
                else
                {
                    _listeners.RemoveAll(l => l.Listener == listener);
                }
            }

            public void Trigger(T eventData)
            {
                _isTriggering = true;

                if (_needsSorting) Sort();

                for (int i = 0; i < _listeners.Count; i++)
                {
                    try
                    {
                        _listeners[i].Listener?.OnEvent(eventData);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogException(ex);
                    }
                }

                _isTriggering = false;
                ApplyPendingChanges();
            }

            private void Sort()
            {
                _listeners.Sort((a, b) => b.Priority.CompareTo(a.Priority));
                _needsSorting = false;
            }

            private bool Contains(IEventListener<T> listener)
            {
                for (int i = 0; i < _listeners.Count; i++)
                {
                    if (_listeners[i].Listener == listener) return true;
                }
                return false;
            }

            private void ApplyPendingChanges()
            {
                if (_toRemove.Count > 0)
                {
                    foreach (var l in _toRemove) _listeners.RemoveAll(x => x.Listener == l);
                    _toRemove.Clear();
                }

                if (_toAdd.Count > 0)
                {
                    foreach (var l in _toAdd)
                    {
                        if (!Contains(l.Listener)) _listeners.Add(l);
                    }
                    _toAdd.Clear();
                    _needsSorting = true;
                }
            }
        }
    }
}