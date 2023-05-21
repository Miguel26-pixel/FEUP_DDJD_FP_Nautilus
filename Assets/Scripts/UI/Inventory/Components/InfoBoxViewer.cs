using System.Collections.Generic;
using Items;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.Inventory.Components
{
    public class InfoBoxViewer
    {
        private readonly Item _item;
        private readonly VisualElement _root;
        private readonly VisualElement _itemInfo;
        private readonly VisualElement _itemInfoStats;
        private readonly VisualElement _itemInfoDescriptors;
        
        private readonly VisualTreeAsset _itemDescriptorTemplate;
        
        private readonly Label _itemInfoName;
        private readonly Label _itemInfoDescription;

        public bool IsOpen { get; private set; }


        public InfoBoxViewer(Item item, VisualElement root)
        {
            _itemInfo = root.Q<VisualElement>("ItemInfo");
            _itemInfo.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);

            _itemInfoName = _itemInfo.Q<Label>("InfoTitle");
            _itemInfoDescription = _itemInfo.Q<Label>("InfoDescription");
            _itemInfoStats = _itemInfo.Q<VisualElement>("ItemInfoStats");
            _itemInfoDescriptors = _itemInfo.Q<VisualElement>("Descriptors");
            
            _itemDescriptorTemplate = Resources.Load<VisualTreeAsset>("UI/Descriptor");

            _root = root;
            _item = item;
        }
        
        public void Open()
        {
            IsOpen = true;
            _itemInfoName.text = _item.Name;
            _itemInfoDescription.text = _item.Description;

            List<KeyValuePair<string, string>> descriptors = _item.GetDescriptors();

            if (descriptors.Count == 0)
            {
                _itemInfoStats.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
            }
            else
            {
                _itemInfoStats.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
                _itemInfoDescriptors.Clear();
                foreach (KeyValuePair<string, string> descriptor in descriptors)
                {
                    VisualElement descriptorElement = _itemDescriptorTemplate.Instantiate();
                    descriptorElement.Q<Label>("DescriptorKey").text = descriptor.Key;
                    descriptorElement.Q<Label>("DescriptorValue").text = descriptor.Value;
                    descriptorElement.pickingMode = PickingMode.Ignore;

                    _itemInfoDescriptors.Add(descriptorElement);
                }
            }

            _itemInfo.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.Flex);
            _itemInfo.style.visibility = new StyleEnum<Visibility>(Visibility.Hidden);
        }
        
        public void MakeVisible()
        {
            _itemInfo.style.visibility = new StyleEnum<Visibility>(Visibility.Visible);
        }
        
        public bool IsStyleResolved()
        {
            return _itemInfo.resolvedStyle.width != 0 && _itemInfo.resolvedStyle.height != 0;
        }
        
        public void Close()
        {
            IsOpen = false;
            _itemInfo.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
        }
    }
}