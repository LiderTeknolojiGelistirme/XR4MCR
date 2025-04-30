using UnityEngine;

namespace Interfaces
{
    public interface IDraggable
    {
        bool EnableDrag { get; set; }
        void OnDrag(Vector2 position);
        void OnBeginDrag();
        void OnEndDrag();
    }
}