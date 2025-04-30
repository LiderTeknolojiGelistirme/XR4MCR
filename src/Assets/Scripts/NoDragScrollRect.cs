using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// ScrollRect bileþeninin bir türevidir.
/// Drag (sürükleme) iþlemiyle içerik hareket ettirilemez.
/// Yalnýzca scrollbar'lar aracýlýðýyla kaydýrmaya izin verir.
/// </summary>
[AddComponentMenu("UI/No Drag Scroll Rect")]
public class NoDragScrollRect : ScrollRect
{
    /// <summary>
    /// Drag baþlatýlmak istendiðinde çalýþýr.
    /// Bu override ile herhangi bir iþlem yapýlmasý engellenir.
    /// ScrollRect'in içeriði hareket ettirmesi önlenmiþ olur.
    /// </summary>
    /// <param name="eventData">PointerEventData verisi</param>
    public override void OnInitializePotentialDrag(PointerEventData eventData)
    {
        // ScrollRect'in sürükleme hazýrlýðý yapmasýný engelle
        // (base metodu çaðrýlmadýðý için içerik kaydýrýlmaz)
    }

    /// <summary>
    /// Kullanýcý drag iþlemine baþladýðýnda çaðrýlýr.
    /// Bu override ile herhangi bir iþlem yapýlmasý engellenir.
    /// </summary>
    /// <param name="eventData">PointerEventData verisi</param>
    public override void OnBeginDrag(PointerEventData eventData)
    {
        // Drag baþlangýcýnda hiçbir iþlem yapýlmaz
        // ScrollRect içeriði sürükleyemez
    }

    /// <summary>
    /// Kullanýcý drag yaparken her karede çaðrýlýr.
    /// Bu override ile sürükleme esnasýndaki içerik hareketi engellenir.
    /// </summary>
    /// <param name="eventData">PointerEventData verisi</param>
    public override void OnDrag(PointerEventData eventData)
    {
        // Drag sýrasýnda içerik hareket ettirilmez
    }

    /// <summary>
    /// Drag iþlemi sona erdiðinde çaðrýlýr.
    /// Bu override ile sürüklemenin býrakýlmasý sonrasý da hiçbir iþlem yapýlmaz.
    /// </summary>
    /// <param name="eventData">PointerEventData verisi</param>
    public override void OnEndDrag(PointerEventData eventData)
    {
        // Drag býrakýldýðýnda da içerik pozisyonu deðiþtirilmez
    }

    /// <summary>
    /// Mouse scroll (tekerlek) hareketiyle scroll yapýlmasýný da engellemek istersen bu metod da override edilebilir.
    /// Aksi halde scroll wheel aktif kalýr.
    /// </summary>
    /// <param name="eventData">PointerEventData verisi</param>
    public override void OnScroll(PointerEventData eventData)
    {
        // Aþaðýdaki satýrý aktif hale getirerek scroll wheel hareketini de tamamen kapatabilirsin:
        // return;

        // Eðer scroll wheel hareketiyle scroll yapýlabilsin istiyorsan base metodunu çaðýrabilirsin:
        base.OnScroll(eventData);
    }
}
