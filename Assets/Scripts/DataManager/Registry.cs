using UnityEngine;

namespace DataManager
{
    public abstract class Registry<T> : MonoBehaviour
    {
        public bool Initialized { get; private set; }

        public abstract T[] GetAll();
        public abstract void Add(T item);

        public void SetInitialized()
        {
            Initialized = true;
        }
    }
}