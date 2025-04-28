using UnityEngine;
using UnityEngine.UI;

namespace CustomGraphics
{
    public class RoundedRectangle : Image
    {
        [SerializeField] private float _radius = 10f;
        
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            
            var rect = rectTransform.rect;
            var width = rect.width;
            var height = rect.height;
            
            // Köşe yarıçapını sınırla
            _radius = Mathf.Min(_radius, width/2, height/2);
            
            // Köşeleri yuvarla
            vh.AddVert(new Vector3(-width/2 + _radius, -height/2, 0), color, Vector2.zero);
            vh.AddVert(new Vector3(width/2 - _radius, -height/2, 0), color, Vector2.one);
            vh.AddVert(new Vector3(-width/2 + _radius, height/2, 0), color, Vector2.up);
            vh.AddVert(new Vector3(width/2 - _radius, height/2, 0), color, Vector2.right);
            
            // Üçgenleri oluştur
            vh.AddTriangle(0, 1, 2);
            vh.AddTriangle(1, 3, 2);
        }
    }
} 