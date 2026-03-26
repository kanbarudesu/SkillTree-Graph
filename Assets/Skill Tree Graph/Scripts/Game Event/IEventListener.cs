namespace GameEvents
{
    public interface IEventListener<T> where T : struct
    {
        void OnEvent(T eventData);
    }
}