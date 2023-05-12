using System;

namespace Items
{
    public class ContextMenuAction
    {
        public string Name { get; }
        public delegate void ContextAction();
        public ContextAction Action { get; }
        
        public ContextMenuAction(string name, ContextAction action)
        {
            Name = name;
            Action = action;
        }
    }
}