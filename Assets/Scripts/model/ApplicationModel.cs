using System.Collections.Generic;
using UnityEngine.Events;

namespace model
{
    public abstract class ApplicationModel
    {
        private readonly Dictionary<string, List<UnityAction<ApplicationModel>>> _listeners = new();
        private const string DefaultKey = "default";
        
        public void AddListener(UnityAction<ApplicationModel> listener, string key = DefaultKey)
        {
            if (!_listeners.ContainsKey(key)) 
                _listeners.Add(key, new List<UnityAction<ApplicationModel>>());
            _listeners[key].Add(listener);
        }
        
        public void RemoveListener(UnityAction<ApplicationModel> listener, string key = DefaultKey)
        {
            if (!_listeners.TryGetValue(key, value: out var listener1))
                return;
            listener1.Remove(listener);
        }

        protected void NotifyListeners(string key = DefaultKey)
        {
            if (_listeners.TryGetValue(key, out var listeners))
            {
                foreach (var listener in listeners)
                {
                    listener.Invoke(this);
                }
            }
        }
    }
}