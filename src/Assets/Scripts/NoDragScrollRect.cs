using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// ScrollRect bileşeninin aynısı gibi çalışır.
/// Normal drag, scroll ve tıklama işlemlerinin hepsine izin verir.
/// </summary>
[AddComponentMenu("UI/Custom Scroll Rect")]
public class NoDragScrollRect : ScrollRect
{
    // ScrollRect'in tüm özelliklerini aynen kullan
    // Hiçbir kısıtlama yok, normal ScrollRect davranışı
}
