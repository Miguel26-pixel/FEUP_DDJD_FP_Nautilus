using PlayerControls;
namespace Items
{
    public class ContextMenuAction
    {
        public delegate void ContextAction(Player player, Item item);

        public ContextMenuAction(string name, ContextAction action)
        {
            Name = name;
            Action = action;
        }

        public string Name { get; }
        public ContextAction Action { get; }
    }
}