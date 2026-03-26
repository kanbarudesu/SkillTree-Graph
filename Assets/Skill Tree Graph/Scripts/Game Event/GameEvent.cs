namespace GameEvents
{
    public struct GameEvent
    {
        public string EventName;

        public GameEvent(string name)
        {
            EventName = name;
        }

        public static void Trigger(string name)
        {
            EventManager.TriggerEvent(new GameEvent(name));
        }
    }
}