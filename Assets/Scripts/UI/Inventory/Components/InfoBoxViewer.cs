using System.Collections.Generic;
using Items;
using UnityEngine;
using UnityEngine.UIElements;

namespace UI.Inventory.Components
{
    public class InfoBoxViewer
    {
        private readonly VisualTreeAsset _itemDescriptorTemplate;
        private readonly VisualElement _itemInfo;
        private readonly Label _itemInfoDescription;
        private readonly VisualElement _itemInfoDescriptors;

        private readonly Label _itemInfoName;
        private readonly VisualElement _itemInfoStats;
        private readonly VisualElement _root;

        public InfoBoxViewer(VisualElement root)
        {
            VisualElement itemInfo = root.Q<VisualElement>("ItemInfo");
            if (itemInfo == null)
            {
                Resources.Load<VisualTreeAsset>("UI/ItemInfo").CloneTree(root);
                itemInfo = root.Q<VisualElement>("ItemInfo");
            }
            
            _itemInfo = itemInfo;
            _itemInfo.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);

            _itemInfoName = _itemInfo.Q<Label>("InfoTitle");
            _itemInfoDescription = _itemInfo.Q<Label>("InfoDescription");
            _itemInfoStats = _itemInfo.Q<VisualElement>("ItemInfoStats");
            _itemInfoDescriptors = _itemInfo.Q<VisualElement>("Descriptors");

            _itemDescriptorTemplate = Resources.Load<VisualTreeAsset>("UI/Descriptor");

            _root = root;
        }

        private bool IsOpen { get; set; }

        public void Open(Item item)
        {
            IsOpen = true;
            _itemInfoName.text = item.Name;
            _itemInfoDescription.text = item.Description;

            List<KeyValuePair<string, string>> descriptors = item.GetDescriptors();

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

        public void Update(Vector2 mousePos)
        {
            if (!IsOpen)
            {
                return;
            }

            if (UiUtils.IsNotStyleResolved(_itemInfo))
            {
                return;
            }

            UiUtils.SetTopLeft(mousePos, _itemInfo, _root);

            if (_itemInfo.style.visibility.value == Visibility.Hidden)
            {
                UiUtils.MakeVisible(_itemInfo);
            }
        }

        public void Close()
        {
            IsOpen = false;
            _itemInfo.style.display = new StyleEnum<DisplayStyle>(DisplayStyle.None);
        }
    }
}