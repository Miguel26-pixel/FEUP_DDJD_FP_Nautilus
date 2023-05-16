using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Items
{
    public abstract class ItemEntity<TEntity>
    {
        [JsonProperty("components")] protected List<TEntity> components = new();
        
        public void AddComponent(TEntity component)
        {
            components.Add(component);
        }

        public T GetComponent<T>() where T : TEntity
        {
            return components.OfType<T>().FirstOrDefault();
        }

        public T[] GetComponents<T>() where T : TEntity
        {
            return components.OfType<T>().ToArray();
        }

        public IEnumerable<TEntity> GetComponents()
        {
            return components.ToArray();
        }

        public bool HasComponent<T>() where T : TEntity
        {
            return components.OfType<T>().Any();
        }
    }
}