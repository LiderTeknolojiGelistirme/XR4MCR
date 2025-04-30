using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// ScrollRect bile�eninin bir t�revidir.
/// Drag (s�r�kleme) i�lemiyle i�erik hareket ettirilemez.
/// Yaln�zca scrollbar'lar arac�l���yla kayd�rmaya izin verir.
/// </summary>
[AddComponentMenu("UI/No Drag Scroll Rect")]
public class NoDragScrollRect : ScrollRect
{
    /// <summary>
    /// Drag ba�lat�lmak istendi�inde �al���r.
    /// Bu override ile herhangi bir i�lem yap�lmas� engellenir.
    /// ScrollRect'in i�eri�i hareket ettirmesi �nlenmi� olur.
    /// </summary>
    /// <param name="eventData">PointerEventData verisi</param>
    public override void OnInitializePotentialDrag(PointerEventData eventData)
    {
        // ScrollRect'in s�r�kleme haz�rl��� yapmas�n� engelle
        // (base metodu �a�r�lmad��� i�in i�erik kayd�r�lmaz)
    }

    /// <summary>
    /// Kullan�c� drag i�lemine ba�lad���nda �a�r�l�r.
    /// Bu override ile herhangi bir i�lem yap�lmas� engellenir.
    /// </summary>
    /// <param name="eventData">PointerEventData verisi</param>
    public override void OnBeginDrag(PointerEventData eventData)
    {
        // Drag ba�lang�c�nda hi�bir i�lem yap�lmaz
        // ScrollRect i�eri�i s�r�kleyemez
    }

    /// <summary>
    /// Kullan�c� drag yaparken her karede �a�r�l�r.
    /// Bu override ile s�r�kleme esnas�ndaki i�erik hareketi engellenir.
    /// </summary>
    /// <param name="eventData">PointerEventData verisi</param>
    public override void OnDrag(PointerEventData eventData)
    {
        // Drag s�ras�nda i�erik hareket ettirilmez
    }

    /// <summary>
    /// Drag i�lemi sona erdi�inde �a�r�l�r.
    /// Bu override ile s�r�klemenin b�rak�lmas� sonras� da hi�bir i�lem yap�lmaz.
    /// </summary>
    /// <param name="eventData">PointerEventData verisi</param>
    public override void OnEndDrag(PointerEventData eventData)
    {
        // Drag b�rak�ld���nda da i�erik pozisyonu de�i�tirilmez
    }

    /// <summary>
    /// Mouse scroll (tekerlek) hareketiyle scroll yap�lmas�n� da engellemek istersen bu metod da override edilebilir.
    /// Aksi halde scroll wheel aktif kal�r.
    /// </summary>
    /// <param name="eventData">PointerEventData verisi</param>
    public override void OnScroll(PointerEventData eventData)
    {
        // A�a��daki sat�r� aktif hale getirerek scroll wheel hareketini de tamamen kapatabilirsin:
        // return;

        // E�er scroll wheel hareketiyle scroll yap�labilsin istiyorsan base metodunu �a��rabilirsin:
        base.OnScroll(eventData);
    }
}
