using UnityEngine;
using UnityEngine.UIElements;

namespace UI
{
    public static class UiUtils
    {
        public static void SetTopLeft(Vector2 mousePos, VisualElement element, VisualElement root)
        {
            if (mousePos.y - element.resolvedStyle.height < 0)
            {
                element.style.bottom = mousePos.y - 3;
                element.style.top = new StyleLength(StyleKeyword.Auto);
            }
            else
            {
                element.style.top = root.resolvedStyle.height - mousePos.y + 3;
                element.style.bottom = new StyleLength(StyleKeyword.Auto);
            }

            if (mousePos.x + element.resolvedStyle.width > root.resolvedStyle.width)
            {
                element.style.right = root.resolvedStyle.width - mousePos.x - 3;
                element.style.left = new StyleLength(StyleKeyword.Auto);
            }
            else
            {
                element.style.left = mousePos.x + 3;
                element.style.right = new StyleLength(StyleKeyword.Auto);
            }
        }
    }
}