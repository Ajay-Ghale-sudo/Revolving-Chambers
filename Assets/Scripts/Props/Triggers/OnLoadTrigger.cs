using UnityEngine;

namespace Props.Triggers
{
    /// <summary>
    /// Trigger that activates when the object is loaded.
    /// </summary>
    public class OnLoadTrigger : TriggerBase 
    {
        private void Start()
        {
            Trigger();
        }
    }
}