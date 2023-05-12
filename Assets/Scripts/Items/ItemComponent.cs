using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Items
{
    [Serializable]
    public abstract class ItemComponent
    {
        [NonSerialized] [JsonIgnore] protected List<ContextMenuAction> actions = new();

        public List<ContextMenuAction> GetActions()
        {
            return actions;
        }
    }
}