namespace GameEvents
{
    public static class EventExtensions
    {
        public static void StartListening<T>(this IEventListener<T> caller, int priority = 0) where T : struct
        {
            EventManager.AddListener(caller, priority);
        }

        public static void StopListening<T>(this IEventListener<T> caller) where T : struct
        {
            EventManager.RemoveListener(caller);
        }
    }
}