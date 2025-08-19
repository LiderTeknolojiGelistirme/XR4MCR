using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Managers;

namespace Helpers
{
    /// <summary>
    /// Akıllı buton EventTrigger - Scroll view içinde scroll'a izin verir
    /// Sadece editör alanına sürüklenince ghost oluşturur
    /// </summary>
    public class SmartButtonEventTrigger : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        [Header("References")]
        private Pointer _pointer;
        private AddNodeButtonHelper _addNodeButtonHelper;
        
        [Header("Settings")]
        [SerializeField] private float dragThreshold = 20f; // Drag başlamak için minimum mesafe
        
        private Vector2 _pointerDownPosition;
        private bool _isDragging = false;
        private bool _ghostCreated = false;
        private bool _isPointerDown = false;
        
        // Scroll view referansları
        private ScrollRect _parentScrollRect;
        
        private void Awake()
        {
            // Pointer'ı bul
            _pointer = FindObjectOfType<Pointer>();
            
            // AddNodeButtonHelper'ı al
            _addNodeButtonHelper = GetComponent<AddNodeButtonHelper>();
            
            // Parent ScrollRect'i bul
            _parentScrollRect = GetComponentInParent<ScrollRect>();
            
            if (_pointer == null)
            {
                LogManager.LogError($"[SmartButtonEventTrigger] Pointer bulunamadı! ({gameObject.name})");
            }
            
            if (_addNodeButtonHelper == null)
            {
                LogManager.LogError($"[SmartButtonEventTrigger] AddNodeButtonHelper bulunamadı! ({gameObject.name})");
            }
        }
        
        public void OnPointerDown(PointerEventData eventData)
        {
            _pointerDownPosition = eventData.position;
            _isPointerDown = true;
            _isDragging = false;
            _ghostCreated = false;
            
            // XR RAY KONTROLÜ: Scroll view içinde miyiz?
            bool isInScrollView = IsPositionInScrollView();
            
            if (!isInScrollView)
            {
                // Scroll view dışındayız - normal click, return et
                if (_addNodeButtonHelper != null)
                {
                    _addNodeButtonHelper.AddNode();
                    LogManager.LogSuccess($"Node oluşturuldu: {gameObject.name}");
                }
                
                // State'leri sıfırla ve çık
                _isPointerDown = false;
                return;
            }
            
            // ScrollRect'in aktif olmasına izin ver
            if (_parentScrollRect != null)
            {
                _parentScrollRect.enabled = true;
                // ScrollRect'e OnBeginDrag event'ini forward et
                _parentScrollRect.OnBeginDrag(eventData);
            }
        }
        
        public void OnDrag(PointerEventData eventData)
        {
            // OnPointerDown'da return edildiyse drag'e izin verme
            if (!_isPointerDown) return;
            
            float dragDistance = Vector2.Distance(_pointerDownPosition, eventData.position);
            
            // Drag başladı mı?
            if (!_isDragging && dragDistance > dragThreshold)
            {
                _isDragging = true;
            }
            
            if (!_isDragging) return;
            
            // ALAN KONTROLÜ: Hala scroll view içinde miyiz? (XR ray ile kontrol)
            bool stillInScrollView = IsPositionInScrollView();
            
            if (stillInScrollView && !_ghostCreated)
            {
                // Hala scroll view içindeyiz - scroll'a izin ver, ghost oluşturma
                // ScrollRect'e OnDrag event'ini forward et
                if (_parentScrollRect != null)
                {
                    _parentScrollRect.enabled = true;
                    _parentScrollRect.OnDrag(eventData);
                }
            }
            else if (!stillInScrollView && !_ghostCreated)
            {
                // Scroll view'dan çıktık - Ghost oluştur
                _ghostCreated = true;
                
                // ScrollRect'i durdur ve OnEndDrag çağır
                if (_parentScrollRect != null)
                {
                    _parentScrollRect.OnEndDrag(eventData);
                    _parentScrollRect.enabled = false;
                }
                
                // Ghost oluştur
                if (_pointer != null)
                {
                    _pointer.CreateGhostNode();
                }
            }
            else if (stillInScrollView && _ghostCreated)
            {
                // Tekrar scroll view'a döndük - Ghost'u kaldır
                _ghostCreated = false;
                
                // Ghost'u kaldır
                if (_pointer != null)
                {
                    _pointer.DestroyGhostNode();
                }
                
                // ScrollRect'i yeniden etkinleştir, OnBeginDrag ve OnDrag çağır
                if (_parentScrollRect != null)
                {
                    _parentScrollRect.enabled = true;
                    _parentScrollRect.OnBeginDrag(eventData);
                    _parentScrollRect.OnDrag(eventData);
                }
            }
        }
        
        public void OnPointerUp(PointerEventData eventData)
        {
            // Eğer OnPointerDown'da return ettiyse buraya gelmeyecek ama yine de kontrol edelim
            if (!_isPointerDown) return;
            
            // EditorMask kontrolü - editör alanında mıyız?
            bool isInEditorArea = IsPositionInEditorArea();
            
            if (_ghostCreated)
            {
                // Ghost varsa - sadece editör alanındaysa AddNode işlemi yap
                if (isInEditorArea)
                {
                    if (_addNodeButtonHelper != null)
                    {
                        _addNodeButtonHelper.AddNode();
                        LogManager.LogSuccess($"Node oluşturuldu (ghost): {gameObject.name}");
                    }
                }
                
                // Ghost'u her durumda temizle
                if (_pointer != null)
                {
                    _pointer.DestroyGhostNode();
                }
                
                // Ghost oluştuktan sonra OnEndDrag çağır
                if (_parentScrollRect != null)
                {
                    _parentScrollRect.OnEndDrag(eventData);
                }
            }
            else if (_isDragging)
            {
                // ScrollRect'e OnEndDrag event'ini forward et
                if (_parentScrollRect != null)
                {
                    _parentScrollRect.OnEndDrag(eventData);
                }
            }
            else
            {
                // Sadece click yapıldı - normal buton davranışı
                if (_addNodeButtonHelper != null)
                {
                    _addNodeButtonHelper.AddNode();
                    LogManager.LogSuccess($"Node oluşturuldu (click): {gameObject.name}");
                }
                
                // Eğer OnBeginDrag çağrıldıysa OnEndDrag de çağır
                if (_parentScrollRect != null)
                {
                    _parentScrollRect.OnEndDrag(eventData);
                }
            }
            
            // ScrollRect'i yeniden etkinleştir
            if (_parentScrollRect != null)
            {
                _parentScrollRect.enabled = true;
            }
            
            // State'leri sıfırla
            _isPointerDown = false;
            _isDragging = false;
            _ghostCreated = false;
        }
        
        /// <summary>
        /// XR ray ile hit edilen pozisyonun scroll view içinde olup olmadığını kontrol eder
        /// </summary>
        private bool IsPositionInScrollView()
        {
            // XRInputManager'dan world hit point'i al
            XRInputManager xrInputManager = FindObjectOfType<XRInputManager>();
            if (xrInputManager == null)
            {
                LogManager.LogError("[SmartButtonEventTrigger] XRInputManager bulunamadı!");
                return false;
            }
            
            // XR ray ile hit point al
            if (!xrInputManager.TryGetPrecisionRaycastHit(out RaycastHit hit))
            {
                return false;
            }
            
            // Hit edilen GameObject'i kontrol et
            GameObject hitObject = hit.collider.gameObject;
            return IsHitInScrollView(hitObject);
        }
        
        /// <summary>
        /// Hit edilen objenin Context Menu Scroll View veya NodeListMask içinde olup olmadığını kontrol eder
        /// </summary>
        private bool IsHitInScrollView(GameObject hitObject)
        {
            // Direkt NodeListMask'a mı hit edildi?
            if (hitObject.name == "NodeListMask")
            {
                return true;
            }
            
            Transform current = hitObject.transform;
            
            // Parent hierarchy'de Context Menu Scroll View veya NodeListMask ara
            while (current != null)
            {
                if (current.name == "Context Menu Scroll View" || current.name == "NodeListMask")
                {
                    return true;
                }
                current = current.parent;
            }
            
            return false;
        }
        
        /// <summary>
        /// XR ray ile hit edilen pozisyonun EditorMask içinde olup olmadığını kontrol eder
        /// </summary>
        private bool IsPositionInEditorArea()
        {
            // XRInputManager'dan world hit point'i al
            XRInputManager xrInputManager = FindObjectOfType<XRInputManager>();
            if (xrInputManager == null)
            {
                LogManager.LogError("[SmartButtonEventTrigger] XRInputManager bulunamadı! (EditorMask kontrolü)");
                return false;
            }
            
            // XR ray ile hit point al
            if (!xrInputManager.TryGetPrecisionRaycastHit(out RaycastHit hit))
            {
                return false;
            }
            
            // Hit edilen GameObject'i kontrol et
            GameObject hitObject = hit.collider.gameObject;
            return IsHitInEditorLimitor(hitObject);
        }
        
        /// <summary>
        /// Hit edilen objenin EditorMask içinde olup olmadığını kontrol eder
        /// </summary>
        private bool IsHitInEditorLimitor(GameObject hitObject)
        {
            // Direkt EditorMask'a mı hit edildi?
            if (hitObject.name == "EditorMask")
            {
                return true;
            }
            
            Transform current = hitObject.transform;
            
            // Parent hierarchy'de EditorMask ara
            while (current != null)
            {
                if (current.name == "EditorMask")
                {
                    return true;
                }
                current = current.parent;
            }
            
            return false;
        }
        
        /// <summary>
        /// Drag threshold'u runtime'da ayarlamak için
        /// </summary>
        public void SetDragThreshold(float threshold)
        {
            dragThreshold = threshold;
        }
    }
} 