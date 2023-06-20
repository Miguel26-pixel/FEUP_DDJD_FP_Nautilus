using UnityEngine;

namespace UI.Inventory
{
    public class IconRepository : MonoBehaviour
    {
        public enum IconType
        {
            Error,
            Checkmark
        }
        
        [SerializeField] private Sprite errorIcon;
        [SerializeField] private Sprite checkmarkIcon;
        
        public Sprite GetIcon(IconType type)
        {
            return type switch
            {
                IconType.Error => errorIcon,
                IconType.Checkmark => checkmarkIcon,
                _ => null
            };
        }
    }
}