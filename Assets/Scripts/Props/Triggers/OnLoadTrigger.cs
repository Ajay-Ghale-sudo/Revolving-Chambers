using UnityEngine;

namespace Props.Triggers
{
    /// <summary>
    /// Trigger that activates when the object is loaded.
    /// </summary>
    public class OnLoadTrigger : TriggerBase
    {
        [SerializeField] private float delay = 0.0f;
        private void Start()
        {
            Invoke(nameof(Trigger), delay);
        }
    }
}