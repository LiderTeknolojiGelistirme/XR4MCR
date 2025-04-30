using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Helpers
{
    public class CustomScrollRect : ScrollRect
    {
        // Eger scroll yapabilmek istiyorsak
        public bool canDrag = true;

        // Eger ray hedefi button ise, drag islemi engellenecek
        public void SetCanDrag(bool value)
        {
            canDrag = value;
        }

        public override void OnBeginDrag(PointerEventData eventData)
        {
            if (canDrag)
                base.OnBeginDrag(eventData);
        }

        public override void OnDrag(PointerEventData eventData)
        {
            if (canDrag)
                base.OnDrag(eventData);
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            if (canDrag)
                base.OnEndDrag(eventData);
        }
    }
}

