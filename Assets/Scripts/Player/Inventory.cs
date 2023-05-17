using System.Collections.Generic;
using Items;

namespace Player
{
    public interface IInventory
    {
        public List<Item> GetItems();
    }
}