using UnityEngine;
using UnityEngine.Events;

namespace GameEvents
{
    public class GameEventListener : MonoBehaviour, IEventListener<GameEvent>
    {
        [SerializeField] private string eventName = null;
        [SerializeField] private UnityEvent onEvent = null;

        public void OnEvent(GameEvent eventData)
        {
            if (eventName == eventData.EventName)
            {
                onEvent.Invoke();
            }
        }

        private void OnEnable() => this.StartListening();

        private void OnDisable() => this.StopListening();
    }
}