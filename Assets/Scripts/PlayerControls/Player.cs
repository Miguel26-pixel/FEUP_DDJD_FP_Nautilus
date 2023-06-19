using Inventory;

namespace PlayerControls
{
    public class Player : AbstractPlayer
    {
        public int health = 1000;
        public int maxHealth = 1000;
        
        private PlayerInventory _playerInventory = new("Inventory", new[,]
        {
            { false, false, false, false, false, false },
            { false, false, false, false, false, false },
            { false, true, true, true, true, false },
            { true, true, true, true, true, true },
            { true, true, true, true, true, true },
            { true, true, true, true, true, true },
            { true, true, true, true, true, true },
            { false, true, true, true, true, false },
            { false, false, false, false, false, false }
        });
        
        public bool IsDead => health == 0;
        public float HealthPercentage => health / (float) maxHealth;
        
        public void RemoveHealth(int amount)
        {
            health -= amount;
            if (health < 0)
            {
                health = 0;
            }
        }

        public override PlayerInventory GetInventory()
        {
            return _playerInventory;
        }

        public override void SetInventory(PlayerInventory inventory)
        {
            _playerInventory = inventory;
        }

        public override IInventoryNotifier GetInventoryNotifier()
        { 
            return _playerInventory;
        }
    }
}