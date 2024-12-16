using System.Collections.Generic;
using UnityEngine.Events;

namespace model
{
    public abstract class ApplicationModel
    {
        private readonly List<UnityEvent<ApplicationModel>> _listeners = new();
        
        public void AddListener(UnityEvent<ApplicationModel> listener)
        {
            _listeners.Add(listener);
        }
        
        public void RemoveListener(UnityEvent<ApplicationModel> listener)
        {
            _listeners.Remove(listener);
        }

        protected void NotifyListeners()
        {
            foreach (var listener in _listeners)
            {
                listener.Invoke(this);
            }
        }
    }
}