using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Items
{
    public interface IInstantiable<out T>
    {
        public T CreateInstance();
    }


    [Serializable]
    public abstract class ItemComponentData : IInstantiable<ItemComponent>
    {
        [NonSerialized] [JsonIgnore] protected List<ContextMenuAction> actions = new();

        public abstract ItemComponent CreateInstance();

        public List<ContextMenuAction> GetActions()
        {
            return actions;
        }
    }

    [Serializable]
    public abstract class ItemComponent
    {
        [NonSerialized] [JsonIgnore] public readonly ItemComponentData itemComponentData;

        protected ItemComponent(ItemComponentData itemComponentData)
        {
            this.itemComponentData = itemComponentData;
        }

        public List<ContextMenuAction> GetActions()
        {
            return itemComponentData.GetActions();
        }
    }
}