using System;

namespace GameEvents
{
    public class EventListenerWrapper<T> : IEventListener<T>, IDisposable where T : struct
    {
        private Action<T> _onEventAction;
        private bool _isSubscribed;

        public EventListenerWrapper(Action<T> onEventAction, int priority = 0)
        {
            _onEventAction = onEventAction;
            EventManager.AddListener(this, priority);
            _isSubscribed = true;
        }

        public void OnEvent(T eventData)
        {
            _onEventAction?.Invoke(eventData);
        }

        public void Dispose()
        {
            if (_isSubscribed)
            {
                EventManager.RemoveListener(this);
                _onEventAction = null;
                _isSubscribed = false;
            }
        }
    }
}