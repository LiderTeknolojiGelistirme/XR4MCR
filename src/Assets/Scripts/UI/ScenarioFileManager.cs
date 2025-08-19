using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using UnityEngine.XR.Interaction.Toolkit.Samples.SpatialKeyboard;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit;
using Zenject;
using Managers;

namespace UI
{
    /// <summary>
    /// XR ortamında çalışan senaryo dosya kaydetme/yükleme popup yöneticisi
    /// 
    /// ÖNEMLİ DÜZELTİLMİŞ SORUNLAR:
    /// - fileListParent artık doğru ScrollView Content objesine atanıyor
    /// - GridHolder yaklaşımı kaldırıldı, item'lar direkt Content'e ekleniyor
    /// - Transform null check'leri eklendi, MissingReference hataları çözüldü
    /// - Layout sistem optimize edildi: VerticalLayoutGroup + ContentSizeFitter
    /// </summary>
    public class ScenarioFileManager : MonoBehaviour
    {
        [Header("Main Containers")]
        [SerializeField] private GameObject scenarioPopups;
        [SerializeField] private GameObject panel;

        [Header("Save Popup UI")]
        [SerializeField] private GameObject savePopup;
        [SerializeField] private TMP_InputField scenarioNameInput;
        [SerializeField] private Button saveOkButton;
        [SerializeField] private Button saveCancelButton;
        [SerializeField] private TextMeshProUGUI saveStatusText;

        [Header("Load Popup UI")]
        [SerializeField] private GameObject loadPopup;
        [SerializeField] private Transform fileListParent;
        [SerializeField] private Button loadOkButton;
        [SerializeField] private Button loadCancelButton;
        [SerializeField] private GameObject fileItemPrefab;
        
        [SerializeField] private Sprite fileHeaderSprite; // Icon_ItemShow sprite'ı
        
        [SerializeField] private Sprite fileItemIconSprite; // Icon_ItemShow sprite'ı

        [Header("Overwrite Confirmation")]
        [SerializeField] private GameObject overwriteConfirmPopup;
        [SerializeField] private Button overwriteYesButton;
        [SerializeField] private Button overwriteNoButton;
        [SerializeField] private TextMeshProUGUI overwriteMessageText;

        [Header("Settings")]
        [SerializeField] private string scenariosFolder = "scenarios";
        [SerializeField] private string fileExtension = "mcrsf";
        [SerializeField] private XRKeyboardDisplay keyboardDisplay;
        
        // XR Keyboard (Inject edilecek)
        private XRKeyboard xrKeyboard;
        
        // GraphManager (Inject edilecek)
        private GraphManager graphManager;

        // Events
        public static event System.Action<string> OnSaveRequested;
        public static event System.Func<string, Task> OnLoadRequested;

        // Private members
        private string selectedFileName;
        private List<FileInfo> availableFiles = new List<FileInfo>();
        private List<GameObject> fileListItems = new List<GameObject>();
        private string pendingSaveFileName;
        
        // Confirmation popup mode
        private enum ConfirmationMode
        {
            Overwrite,
            NewScenario
        }
        private ConfirmationMode currentConfirmationMode;
        private string lastFilePath;


        [Inject]
        public void Construct(XRKeyboard keyboard, GraphManager graphManagerInstance)
        {
            xrKeyboard = keyboard;
            graphManager = graphManagerInstance;
        }

        private void Awake()
        {
            InitializeButtons();
            EnsureScenariosFolderExists();
            SetupCanvasCamera();
            SetupXRKeyboard();
        }
        
        private void SetupXRKeyboard()
        {
            // XRKeyboardDisplay'i ayarla
            if (keyboardDisplay != null)
            {
                keyboardDisplay.updateOnKeyPress = true;
                keyboardDisplay.onTextSubmitted.AddListener(UpdateScenarioName);
                
                // Inject edilmiş keyboard'u ata
                if (xrKeyboard != null)
                {
                    keyboardDisplay.keyboard = xrKeyboard;
                }
                else
                {
                    Debug.LogWarning("XRKeyboard inject edilmedi! DI container ayarlarını kontrol edin.");
                }
            }
        }

        private void UpdateScenarioName(string arg0)
        {
            scenarioNameInput.text = arg0;
        }
        
        private void SetupCanvasCamera()
        {
            // Ana container'ın Canvas'ını bul ve kamerayı ata
            if (scenarioPopups != null)
            {
                var canvas = scenarioPopups.GetComponent<Canvas>();
                if (canvas != null && canvas.renderMode == RenderMode.WorldSpace)
                {
                    // XR kamerasını bul ve ata
                    Camera xrCamera = Camera.main;
                    if (xrCamera == null)
                    {
                        // XR Origin Camera'sını ara
                        xrCamera = FindObjectOfType<Camera>();
                    }
                    
                    if (xrCamera != null)
                    {
                        canvas.worldCamera = xrCamera;
                    }
                    
                    // XR için TrackedDeviceGraphicRaycaster gerekli
                    SetupXRGraphicRaycaster(canvas);
                }
            }
        }
        
        private void SetupXRGraphicRaycaster(Canvas canvas)
        {
            // Normal GraphicRaycaster'ı kaldır (varsa)
            var normalRaycaster = canvas.GetComponent<GraphicRaycaster>();
            if (normalRaycaster != null && !(normalRaycaster is UnityEngine.XR.Interaction.Toolkit.UI.TrackedDeviceGraphicRaycaster))
            {
                DestroyImmediate(normalRaycaster);
            }
            
            // TrackedDeviceGraphicRaycaster ekle (yoksa)
            var trackedRaycaster = canvas.GetComponent<UnityEngine.XR.Interaction.Toolkit.UI.TrackedDeviceGraphicRaycaster>();
            if (trackedRaycaster == null)
            {
                trackedRaycaster = canvas.gameObject.AddComponent<UnityEngine.XR.Interaction.Toolkit.UI.TrackedDeviceGraphicRaycaster>();
                Debug.Log("TrackedDeviceGraphicRaycaster eklendi - XR UI etkileşimi aktif");
                }
            }
            


        private void InitializeButtons()
        {
            // Save popup butonları
            if (saveOkButton != null)
                saveOkButton.onClick.AddListener(OnSaveOkClicked);
            if (saveCancelButton != null)
                saveCancelButton.onClick.AddListener(OnSaveCancelClicked);

            // Load popup butonları
            if (loadOkButton != null)
                loadOkButton.onClick.AddListener(OnLoadOkClicked);
            if (loadCancelButton != null)
                loadCancelButton.onClick.AddListener(OnLoadCancelClicked);

            // Overwrite confirmation butonları
            if (overwriteYesButton != null)
                overwriteYesButton.onClick.AddListener(OnOverwriteYesClicked);
            if (overwriteNoButton != null)
                overwriteNoButton.onClick.AddListener(OnOverwriteNoClicked);

            // Popup'ları başlangıçta kapat
            CloseAllPopups();
        }

        private void EnsureScenariosFolderExists()
        {
            string fullPath = Path.Combine(Application.persistentDataPath, scenariosFolder);
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);

            }
        }

        #region Public Methods

        /// <summary>
        /// Save popup'ını açar
        /// </summary>
        public void ShowSavePopup()
        {
            CloseAllPopups();
            
            // Ana container'ları aktif et
            if (scenarioPopups != null)
                scenarioPopups.SetActive(true);
            if (panel != null)
                panel.SetActive(true);
            
            if (savePopup != null)
            {
                savePopup.SetActive(true);
                
                // Input field'ı temizle ve focus ver
                if (scenarioNameInput != null)
                {
                    scenarioNameInput.text = "";
                    scenarioNameInput.Select();
                }

                // Status text'i temizle
                if (saveStatusText != null)
                {
                    saveStatusText.text = "";
                }


            }
        }

        /// <summary>
        /// Load popup'ını açar ve dosya listesini günceller
        /// </summary>
        public void ShowLoadPopup()
        {
            CloseAllPopups();
            
            // Ana container'ları aktif et
            if (scenarioPopups != null)
                scenarioPopups.SetActive(true);
            if (panel != null)
                panel.SetActive(true);

            

            
            if (loadPopup != null)
            {
                loadPopup.SetActive(true);
                RefreshFileList();

            }
        }

        /// <summary>
        /// New Scenario confirmation popup'ını açar
        /// </summary>
        public void ShowNewScenarioConfirmation()
        {
            CloseAllPopups();
            
            // Confirmation mode'u ayarla
            currentConfirmationMode = ConfirmationMode.NewScenario;
            
            // Ana container'ları aktif et
            if (scenarioPopups != null)
                scenarioPopups.SetActive(true);
            if (panel != null)
                panel.SetActive(true);
            
            if (overwriteConfirmPopup != null)
            {
                overwriteConfirmPopup.SetActive(true);
                
                if (overwriteMessageText != null)
                {
                    overwriteMessageText.text = "Are you sure you want to create a new scenario?\n\nYou might lose your current data. Please save your data if you haven't already.";
                }
            }
        }

        /// <summary>
        /// Tüm popup'ları kapatır
        /// </summary>
        public void CloseAllPopups()
        {
            if (savePopup != null) savePopup.SetActive(false);
            if (loadPopup != null) loadPopup.SetActive(false);
            if (overwriteConfirmPopup != null) overwriteConfirmPopup.SetActive(false);
            
            // Ana container'ları da kapat
            if (scenarioPopups != null) scenarioPopups.SetActive(false);
            if (panel != null) panel.SetActive(false);
        }

        #endregion

        #region Private Methods

        private void RefreshFileList()
        {
            // Önceki liste elemanlarını temizle
            ClearFileList();

            // ÖNCE: fileListParent'ın doğru Content olduğundan emin ol
            EnsureCorrectFileListParent();

            // LoadPopup içindeki elemanları analiz et - butonları engelleyen nedir?
            AnalyzeLoadPopupChildren();

            // ScrollView yapısını debug et
            DebugScrollViewStructure();

            // ScrollView Content'ine Layout Group ekle
            SetupScrollViewLayout();

            // Scenarios klasöründeki MCRSF dosyalarını tara
            string scenariosPath = Path.Combine(Application.persistentDataPath, scenariosFolder);
            
            if (Directory.Exists(scenariosPath))
            {
                var files = Directory.GetFiles(scenariosPath, $"*.{fileExtension}")
                                   .Select(f => new FileInfo(f))
                                   .OrderByDescending(f => f.LastWriteTime)
                                   .ToList();

                availableFiles = files;

                // Her dosya için UI elemanı oluştur
                foreach (var file in files)
                {
                    CreateFileListItem(file);
                }

                Debug.Log($"Toplam {files.Count} dosya eklendi. Content child count: {fileListParent.childCount}");
            }
            else
            {
                Debug.LogWarning($"Scenarios klasörü bulunamadı: {scenariosPath}");
            }

            // Eğer hiç dosya yoksa load butonunu deaktif et
            if (loadOkButton != null)
            {
                loadOkButton.interactable = availableFiles.Count > 0 && !string.IsNullOrEmpty(selectedFileName);
            }
        }

        /// <summary>
        /// fileListParent'ın doğru Content objesine atandığından emin olur
        /// </summary>
        private void EnsureCorrectFileListParent()
        {
            if (fileListParent == null)
            {
                Debug.LogError("fileListParent NULL! Inspector'da atanmalı.");
                return;
            }

            // Eğer fileListParent 'Scroll View' ise, Content'i bul
            if (fileListParent.name == "Scroll View")
            {
                Debug.LogWarning("fileListParent yanlış! Content'i bulup atanıyor...");
                
                var viewportChild = fileListParent.Find("Viewport");
                if (viewportChild != null)
                {
                    var content = viewportChild.Find("Content");
                    if (content != null)
                    {
                        fileListParent = content;
                        Debug.Log($"✅ fileListParent düzeltildi: {fileListParent.name}");
                    }
                    else
                    {
                        Debug.LogError("❌ Content bulunamadı! Manuel olarak Content'i assign edin.");
                    }
                }
                else
                {
                    Debug.LogError("❌ Viewport bulunamadı!");
                }
            }
        }

        private void DebugScrollViewStructure()
        {
            Debug.Log("=== SCROLLVIEW HIERARCHY DEBUG ===");
            
            if (fileListParent == null)
            {
                Debug.LogError("fileListParent NULL!");
                return;
            }

            Debug.Log($"fileListParent: {fileListParent.name}");
            Debug.Log($"  - Transform Parent: {(fileListParent.parent != null ? fileListParent.parent.name : "NULL")}");
            Debug.Log($"  - Transform Grand Parent: {(fileListParent.parent?.parent != null ? fileListParent.parent.parent.name : "NULL")}");
            Debug.Log($"  - Transform Great Grand Parent: {(fileListParent.parent?.parent?.parent != null ? fileListParent.parent.parent.parent.name : "NULL")}");
            
            // Component'leri kontrol et
            var scrollRect = fileListParent.GetComponentInParent<ScrollRect>();
            var viewport = fileListParent.parent?.GetComponent<RectTransform>();
            var mask = fileListParent.parent?.GetComponent<Mask>();
            
            Debug.Log($"  - ScrollRect parent: {(scrollRect != null ? scrollRect.name : "YOK")}");
            Debug.Log($"  - Viewport (parent): {(viewport != null ? "VAR" : "YOK")}");
            Debug.Log($"  - Mask: {(mask != null ? "VAR" : "YOK")}");
            
            // RectTransform bilgileri
            var rect = fileListParent.GetComponent<RectTransform>();
            if (rect != null)
            {
                Debug.Log($"  - RectTransform: Size={rect.sizeDelta}, Anchors={rect.anchorMin}-{rect.anchorMax}");
            }
            
            // Content doğrulaması
            if (fileListParent.name != "Content")
            {
                Debug.LogError("❌ HATA: fileListParent 'Content' değil! Doğru objeyi assign edin.");
            }
            else
            {
                Debug.Log("✅ fileListParent doğru - Content objesi");
            }
            
            Debug.Log("=== DEBUG BİTTİ ===");
        }

        private void SetupScrollViewLayout()
        {
            if (fileListParent == null)
            {
                Debug.LogError("fileListParent NULL! Layout ayarlanamıyor.");
                return;
            }

            // Null check ekle
            if (fileListParent.gameObject == null)
            {
                Debug.LogError("fileListParent GameObject destroyed!");
                return;
            }

            Debug.Log($"Layout ayarlanıyor: {fileListParent.name}");

            // Content'te basit VerticalLayoutGroup
            var layoutGroup = fileListParent.GetComponent<VerticalLayoutGroup>();
            if (layoutGroup == null)
            {
                layoutGroup = fileListParent.gameObject.AddComponent<VerticalLayoutGroup>();
                Debug.Log("VerticalLayoutGroup eklendi");
            }
            
            // Basit layout ayarları - Manuel buton uyumlu
            layoutGroup.spacing = 2f;                          // Aralar 2px
            layoutGroup.padding = new RectOffset(10, 10, 10, 10); // Kenar boşlukları
            layoutGroup.childAlignment = TextAnchor.UpperLeft;  // Sol üst hizalama
            
            // Child control - Basit ayarlar
            layoutGroup.childControlWidth = false;   // Width kontrolü yok - item'lar kendi boyutunu belirler
            layoutGroup.childControlHeight = false;  // Height kontrolü yok - item'lar kendi boyutunu belirler
            layoutGroup.childForceExpandWidth = false;  // Width genişletme yok
            layoutGroup.childForceExpandHeight = false; // Height genişletme yok
            
            // ContentSizeFitter - Content boyutu
            var sizeFitter = fileListParent.GetComponent<ContentSizeFitter>();
            if (sizeFitter == null)
            {
                sizeFitter = fileListParent.gameObject.AddComponent<ContentSizeFitter>();
                Debug.Log("ContentSizeFitter eklendi");
            }
            
            // Content boyut ayarları - Basit
            sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;   // Height auto
            sizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained; // Width parent'a uyum
            
            // Sütun başlıklarını oluştur
            CreateColumnHeaders();
            
            Debug.Log("✅ Sütunlu Content Layout ayarlandı");
            Debug.Log($"  - childControlWidth: {layoutGroup.childControlWidth}");
            Debug.Log($"  - childControlHeight: {layoutGroup.childControlHeight}");
            Debug.Log($"  - spacing: {layoutGroup.spacing}");
        }

        /// <summary>
        /// Sütun başlıklarını oluşturur
        /// </summary>
        private void CreateColumnHeaders()
        {
            // Önceki header varsa temizle
            var existingHeader = fileListParent.Find("ColumnHeader");
            if (existingHeader != null)
            {
                Destroy(existingHeader.gameObject);
            }

            // Header container oluştur
            GameObject headerObj = new GameObject("ColumnHeader");
            headerObj.transform.SetParent(fileListParent, false);
            headerObj.layer = LayerMask.NameToLayer("UI");
            
            // Header'ı en üste taşı
            headerObj.transform.SetAsFirstSibling();
            
            // RectTransform ayarları
            var headerRect = headerObj.AddComponent<RectTransform>();
            headerRect.anchorMin = new Vector2(0, 1);
            headerRect.anchorMax = new Vector2(0, 1);
            headerRect.pivot = new Vector2(0.5f, 0.5f);
            headerRect.sizeDelta = new Vector2(735, 50);
            
            // LayoutElement
            var headerLayoutElement = headerObj.AddComponent<LayoutElement>();
            headerLayoutElement.minHeight = 50f;
            headerLayoutElement.preferredHeight = 50f;
            headerLayoutElement.layoutPriority = 2; // Header öncelikli
            
            // Header background
            var headerImage = headerObj.AddComponent<Image>();
            headerImage.color = new Color(0.85f, 0.85f, 0.85f, 1f); // Açık gri
            headerImage.raycastTarget = false;
            if (fileHeaderSprite != null)
            {
                headerImage.sprite = fileHeaderSprite; // Source Image: Icon_ItemShow (Inspector'dan)
                Debug.Log($"✅ Icon sprite assign edildi: header");
            }
            else
            {
                headerImage.sprite = null;               // Fallback: No sprite
                Debug.LogWarning($"⚠️ fileHeaderSprite null! Inspector'da assign edin.");
            }
            
            // HorizontalLayoutGroup - Sütunlar yan yana
            var headerHorizontal = headerObj.AddComponent<HorizontalLayoutGroup>();
            headerHorizontal.spacing = 10f;
            headerHorizontal.padding = new RectOffset(15, 15, 8, 8);
            headerHorizontal.childAlignment = TextAnchor.MiddleLeft;
            headerHorizontal.childControlWidth = false;
            headerHorizontal.childControlHeight = true;
            headerHorizontal.childForceExpandWidth = false;
            headerHorizontal.childForceExpandHeight = false;
            
            // Scenario Name başlığı
            CreateHeaderColumn(headerObj, "Senaryo Adı", 500f);
            
            // Date başlığı  
            CreateHeaderColumn(headerObj, "Tarih", 230f);
            
            Debug.Log("✅ Sütun başlıkları oluşturuldu");
        }

        /// <summary>
        /// Tek bir header sütunu oluşturur
        /// </summary>
        private void CreateHeaderColumn(GameObject parent, string title, float width)
        {
            GameObject columnObj = new GameObject($"Header_{title}");
            columnObj.transform.SetParent(parent.transform, false);
            columnObj.layer = LayerMask.NameToLayer("UI");
            
            // RectTransform
            var columnRect = columnObj.AddComponent<RectTransform>();
            columnRect.anchorMin = Vector2.zero;
            columnRect.anchorMax = Vector2.one;
            
            // LayoutElement - Sütun genişliği
            var columnLayout = columnObj.AddComponent<LayoutElement>();
            columnLayout.preferredWidth = width;
            columnLayout.flexibleWidth = 0;
            
            // Text
            var headerText = columnObj.AddComponent<TextMeshProUGUI>();
            headerText.text = title;
            headerText.fontSize = 18;
            headerText.fontStyle = FontStyles.Bold;
            headerText.color = Color.white;
            headerText.alignment = TextAlignmentOptions.Left;
            headerText.raycastTarget = false;
        }

        private void CreateFileListItem(FileInfo fileInfo)
        {
            if (fileListParent == null) return;

            // Dosya adını (uzantısız) al
            string displayName = Path.GetFileNameWithoutExtension(fileInfo.Name);
            string dateStr = fileInfo.LastWriteTime.ToString("dd.MM.yyyy HH:mm");

            // Basit UI elemanı oluştur (kod ile)
            GameObject item = CreateSimpleFileListItem(displayName, dateStr);
            fileListItems.Add(item);
        }

        private GameObject CreateSimpleFileListItem(string fileName, string fileDate)
        {
            // Ana container - Sütunlu file item
            GameObject item = new GameObject($"FileItem_{fileName}");
            item.transform.SetParent(fileListParent, false);
            
            // LAYER AYARI - UI layer'a ata
            item.layer = LayerMask.NameToLayer("UI");
            
            // RectTransform - Inspector'daki ayarları kopyala
            var rect = item.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);    // Min: X=0, Y=1
            rect.anchorMax = new Vector2(0, 1);    // Max: X=0, Y=1  
            rect.pivot = new Vector2(0.5f, 0.5f);  // Pivot: X=0.5, Y=0.5
            
            // Size - Inspector'dan
            rect.sizeDelta = new Vector2(735, 64f); // Width=735, Height=64
            
            // LayoutElement - Inspector ayarları
            var layoutElement = item.AddComponent<LayoutElement>();
            layoutElement.minHeight = 64f;
            layoutElement.preferredHeight = -1;    // Unchecked
            layoutElement.flexibleHeight = -1;     // Unchecked
            layoutElement.minWidth = -1;           // Unchecked
            layoutElement.preferredWidth = -1;     // Unchecked
            layoutElement.flexibleWidth = -1;      // Unchecked
            layoutElement.ignoreLayout = false;    // Layout'a dahil
            layoutElement.layoutPriority = 1;      // Priority = 1
            
            // Image - Inspector ayarları
            var image = item.AddComponent<Image>();
            if (fileItemIconSprite != null)
            {
                image.sprite = fileItemIconSprite; // Source Image: Icon_ItemShow (Inspector'dan)
                Debug.Log($"✅ Icon sprite assign edildi: {fileName}");
            }
            else
            {
                image.sprite = null;               // Fallback: No sprite
                Debug.LogWarning($"⚠️ fileItemIconSprite null! Inspector'da Icon_ItemShow sprite'ını assign edin.");
            }
            image.color = Color.white;             // Color: White
            image.material = null;                 // Material: None
            image.raycastTarget = true;            // Raycast Target: ✓
            image.type = Image.Type.Sliced;        // Image Type: Sliced
            image.fillCenter = true;               // Fill Center: ✓
            image.pixelsPerUnitMultiplier = 1;     // Pixels Per Unit Multiplier: 1
            
            // Button - Inspector ayarları
            var button = item.AddComponent<Button>();
            button.interactable = true;            // Interactable: ✓
            button.transition = Selectable.Transition.ColorTint; // Transition: Color Tint
            button.targetGraphic = image;          // Target Graphic: Load (Image)
            
            // Button Colors - Inspector'dan
            var colors = button.colors;
            colors.normalColor = Color.white;      // Normal Color: White
            colors.highlightedColor = Color.white; // Highlighted Color: White  
            colors.pressedColor = Color.white;     // Pressed Color: White
            colors.selectedColor = Color.white;    // Selected Color: White
            colors.disabledColor = Color.white;    // Disabled Color: White
            colors.colorMultiplier = 1f;           // Color Multiplier: 1
            colors.fadeDuration = 0.1f;            // Fade Duration: 0.1
            button.colors = colors;
            
            // Navigation - Inspector'dan
            var nav = button.navigation;
            nav.mode = Navigation.Mode.Automatic;  // Navigation: Automatic
            button.navigation = nav;
            
            // Button Click Event
            button.onClick.AddListener(() => {
                Debug.Log($"Sütunlu Button Click: {fileName}");
                OnSimpleFileItemClicked(fileName, item);
            });
            
            // HorizontalLayoutGroup - Sütunlar yan yana
            var horizontalLayout = item.AddComponent<HorizontalLayoutGroup>();
            horizontalLayout.spacing = 10f;
            horizontalLayout.padding = new RectOffset(15, 15, 5, 5);
            horizontalLayout.childAlignment = TextAnchor.MiddleLeft;
            horizontalLayout.childControlWidth = false;
            horizontalLayout.childControlHeight = true;
            horizontalLayout.childForceExpandWidth = false;
            horizontalLayout.childForceExpandHeight = false;
            
            // Scenario Name sütunu
            CreateFileItemColumn(item, fileName, 500f, TextAlignmentOptions.Left);
            
            // Date sütunu
            CreateFileItemColumn(item, fileDate, 230f, TextAlignmentOptions.Left);
            
            // Z-ORDER KONTROLÜ VE DÜZELTMESİ
            FixButtonZOrder(item);
            
            Debug.Log($"✅ Sütunlu file item oluşturuldu: {fileName}");
            Debug.Log($"  - Layer: {LayerMask.LayerToName(item.layer)}");
            Debug.Log($"  - Sibling Index: {item.transform.GetSiblingIndex()}");
            Debug.Log($"  - Parent: {item.transform.parent.name}");
            
            return item;
        }

        /// <summary>
        /// File item için tek bir sütun oluşturur
        /// </summary>
        private void CreateFileItemColumn(GameObject parent, string text, float width, TextAlignmentOptions alignment)
        {
            GameObject columnObj = new GameObject($"Column_{text.Substring(0, Math.Min(10, text.Length))}");
            columnObj.transform.SetParent(parent.transform, false);
            columnObj.layer = LayerMask.NameToLayer("UI");
            
            // RectTransform
            var columnRect = columnObj.AddComponent<RectTransform>();
            columnRect.anchorMin = Vector2.zero;
            columnRect.anchorMax = Vector2.one;
            
            // LayoutElement - Sütun genişliği
            var columnLayout = columnObj.AddComponent<LayoutElement>();
            columnLayout.preferredWidth = width;
            columnLayout.flexibleWidth = 0;
            
            // Text
            var columnText = columnObj.AddComponent<TextMeshProUGUI>();
            columnText.text = text;
            columnText.fontSize = 16;
            columnText.color = Color.white;
            columnText.alignment = alignment;
            columnText.raycastTarget = false; // Text raycast kapalı
            columnText.overflowMode = TextOverflowModes.Ellipsis; // Uzun metinlerde ...
        }
        
        /// <summary>
        /// Button'ın Z-order'ını düzeltir - arkada kalma sorununu çözer
        /// </summary>
        private void FixButtonZOrder(GameObject buttonItem)
        {
            // Sibling index'i en sona taşı (en üstte render edilsin)
            buttonItem.transform.SetAsLastSibling();
            
            // Parent hiyerarşisini kontrol et
            Transform current = buttonItem.transform;
            int level = 0;
            while (current != null && level < 5)
            {
                Debug.Log($"Level {level}: {current.name} - SiblingIndex: {current.GetSiblingIndex()}, ChildCount: {current.parent?.childCount}");
                
                // Canvas kontrolü
                var canvas = current.GetComponent<Canvas>();
                if (canvas != null)
                {
                    Debug.Log($"  Canvas found: SortingOrder={canvas.sortingOrder}, SortingLayerName={canvas.sortingLayerName}");
                }
                
                current = current.parent;
                level++;
            }
            
            // LoadPopup içindeki diğer elemanları kontrol et
            AnalyzeLoadPopupChildren();
        }
        
        /// <summary>
        /// LoadPopup içindeki elemanları analiz eder - hangisi butonları engelliyor?
        /// </summary>
        private void AnalyzeLoadPopupChildren()
        {
            if (loadPopup == null) return;
            
            Debug.Log("🔍 LOADPOPUP CHILDREN ANALİZİ:");
            
            for (int i = 0; i < loadPopup.transform.childCount; i++)
            {
                var child = loadPopup.transform.GetChild(i);
                var image = child.GetComponent<Image>();
                var canvas = child.GetComponent<Canvas>();
                var canvasGroup = child.GetComponent<CanvasGroup>();
                var rect = child.GetComponent<RectTransform>();
                
                Debug.Log($"  Child {i}: {child.name}");
                Debug.Log($"    - SiblingIndex: {i}");
                Debug.Log($"    - Layer: {LayerMask.LayerToName(child.gameObject.layer)}");
                Debug.Log($"    - Image: {(image != null ? $"RaycastTarget={image.raycastTarget}" : "YOK")}");
                Debug.Log($"    - Canvas: {(canvas != null ? $"SortingOrder={canvas.sortingOrder}" : "YOK")}");
                Debug.Log($"    - CanvasGroup: {(canvasGroup != null ? $"BlocksRaycasts={canvasGroup.blocksRaycasts}" : "YOK")}");
                
                if (rect != null)
                {
                    Debug.Log($"    - Size: {rect.rect.size}");
                    Debug.Log($"    - Position: {rect.anchoredPosition}");
                }
                
                // Büyük Image + raycastTarget = Şüpheli!
                if (image != null && image.raycastTarget && rect.rect.size.magnitude > 100)
                {
                    Debug.LogWarning($"⚠️ ŞÜPHELI: {child.name} büyük bir Image ile raycast target!");
                }
            }
        }

        private void OnSimpleFileItemClicked(string fileName, GameObject itemObj)
        {
            Debug.Log($"=== FILE ITEM CLICKED ===");
            Debug.Log($"Seçilen dosya: {fileName}");
            Debug.Log($"Item object: {itemObj.name}");
            
            selectedFileName = fileName;
            
            // Önceki seçimi temizle
            foreach (var item in fileListItems)
            {
                var img = item.GetComponent<Image>();
                if (img != null)
            {
                    img.color = new Color(0.95f, 0.95f, 0.95f, 1f); // Normal renk
                }
            }

            // Seçili olanı vurgula
            var selectedImg = itemObj.GetComponent<Image>();
            if (selectedImg != null)
            {
                selectedImg.color = new Color(0.6f, 0.8f, 1f, 1f); // Açık mavi
                Debug.Log("Seçili item rengini değiştirdim");
            }

            // Load butonunu aktif et
            if (loadOkButton != null)
            {
                loadOkButton.interactable = true;
                Debug.Log("Load butonu aktif edildi");
            }
            
            Debug.Log($"Seçilen dosya adı: {selectedFileName}");
            Debug.Log("=== CLICK İŞLEMİ BİTTİ ===");
        }

        private void CreateFileListItemFallback(GameObject item, string displayName, System.DateTime fileDate)
        {
            // Dosya adını ve tarihi göster
            string dateStr = fileDate.ToString("dd.MM.yyyy HH:mm");

            // UI elemanlarını ayarla
            var nameText = item.GetComponentInChildren<TextMeshProUGUI>();
            if (nameText != null)
            {
                nameText.text = $"{displayName}\n<size=12><color=#888888>{dateStr}</color></size>";
            }

            // Butona click event ekle
            var button = item.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() => OnFileItemClickedFallback(displayName, item));
            }
        }

        private void OnFileItemSelected(FileListItem selectedItem)
        {
            selectedFileName = selectedItem.FileName;
            
            // Önceki seçimi temizle
            foreach (var itemObj in fileListItems)
            {
                var fileItem = itemObj.GetComponent<FileListItem>();
                if (fileItem != null)
                {
                    fileItem.SetSelected(false);
                }
            }

            // Seçili olanı işaretle
            selectedItem.SetSelected(true);

            // Load butonunu aktif et
            if (loadOkButton != null)
            {
                loadOkButton.interactable = true;
            }


        }

        private void OnFileItemClickedFallback(string fileName, GameObject itemObj)
        {
            selectedFileName = fileName;
            
            // Önceki seçimi temizle
            foreach (var item in fileListItems)
            {
                var img = item.GetComponent<Image>();
                if (img != null)
                {
                    img.color = Color.white; // Normal renk
                }
            }

            // Seçili olanı vurgula
            var selectedImg = itemObj.GetComponent<Image>();
            if (selectedImg != null)
            {
                selectedImg.color = new Color(0.8f, 0.9f, 1f); // Açık mavi
            }

            // Load butonunu aktif et
            if (loadOkButton != null)
            {
                loadOkButton.interactable = true;
            }


        }

        private void ClearFileList()
        {
            // Güvenli item temizliği
            for (int i = fileListItems.Count - 1; i >= 0; i--)
            {
                var item = fileListItems[i];
                if (item != null && item.gameObject != null)
                {
                    Destroy(item);
                }
                else
                {
                    Debug.LogWarning($"File list item {i} zaten null veya destroyed");
                }
            }
            
            fileListItems.Clear();
            selectedFileName = null;
            
            // NOT: Header temizliği CreateColumnHeaders() metodunda yapılıyor
            
            Debug.Log($"✅ File list temizlendi. Toplam {fileListItems.Count} item kaldı.");
        }

        private string GetFullFilePath(string fileName)
        {
            return Path.Combine(Application.persistentDataPath, scenariosFolder, $"{fileName}.{fileExtension}");
        }

        #endregion

        #region Button Event Handlers

        private void OnSaveOkClicked()
        {
            if (scenarioNameInput == null) return;

            string scenarioName = scenarioNameInput.text.Trim();
            
            if (string.IsNullOrEmpty(scenarioName))
            {
                if (saveStatusText != null)
                {
                    saveStatusText.text = "<color=red>Senaryo adı boş olamaz!</color>";
                }
                return;
            }

            // Geçersiz karakterleri kontrol et
            if (scenarioName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            {
                if (saveStatusText != null)
                {
                    saveStatusText.text = "<color=red>Geçersiz karakterler içeriyor!</color>";
                }
                return;
            }

            string filePath = GetFullFilePath(scenarioName);
            
            // Dosya varsa overwrite onayı iste
            if (File.Exists(filePath))
            {
                pendingSaveFileName = scenarioName;
                ShowOverwriteConfirmation(scenarioName);
            }
            else
            {
                // Direkt kaydet
                PerformSave(scenarioName);
            }
        }

        private void OnSaveCancelClicked()
        {
            CloseAllPopups();
        }

        private async void OnLoadOkClicked()
        {
            if (string.IsNullOrEmpty(selectedFileName))
            {
                Debug.LogWarning("Hiç dosya seçilmedi!");
                return;
            }

            string filePath = GetFullFilePath(selectedFileName);

            lastFilePath = filePath;
            
            if (File.Exists(filePath))
            {
                if (OnLoadRequested != null)
                {
                    await OnLoadRequested.Invoke(filePath);
                }
                CloseAllPopups();

            }
            else
            {
                Debug.LogError($"Dosya bulunamadı: {filePath}");
            }
        }

        private void OnLoadCancelClicked()
        {
            CloseAllPopups();
        }

        private void OnOverwriteYesClicked()
        {
            switch (currentConfirmationMode)
            {
                case ConfirmationMode.Overwrite:
                    if (!string.IsNullOrEmpty(pendingSaveFileName))
                    {
                        PerformSave(pendingSaveFileName);
                        pendingSaveFileName = null;
                    }
                    break;
                    
                case ConfirmationMode.NewScenario:
                    // GraphManager üzerinden yeni senaryo oluştur
                    if (graphManager != null)
                    {
                        PerformNewScenario();
                    }
                    else
                    {
                        Debug.LogError("GraphManager null! Inject edilmediğinden emin olun.");
                    }
                    break;
            }
            
            CloseAllPopups();
        }

        private void OnOverwriteNoClicked()
        {
            pendingSaveFileName = null;
            CloseAllPopups();
            // Save popup'ına geri dön
            ShowSavePopup();
        }

        #endregion

        #region Overwrite Confirmation

        private void ShowOverwriteConfirmation(string fileName)
        {
            if (overwriteConfirmPopup == null) return;

            // Confirmation mode'u ayarla
            currentConfirmationMode = ConfirmationMode.Overwrite;

            // Save popup'ını kapat, overwrite popup'ını aç
            if (savePopup != null) savePopup.SetActive(false);
            
            // Ana container'lar açık kalmalı
            if (scenarioPopups != null) scenarioPopups.SetActive(true);
            if (panel != null) panel.SetActive(true);
            
            overwriteConfirmPopup.SetActive(true);

            if (overwriteMessageText != null)
            {
                overwriteMessageText.text = $"The scenario '{fileName}' already exists.\n\nAre you sure you want to overwrite it?";
            }


        }

        private void PerformSave(string fileName)
        {
            string filePath = GetFullFilePath(fileName);
            OnSaveRequested?.Invoke(filePath);
            CloseAllPopups();
            
            if (saveStatusText != null)
            {
                saveStatusText.text = $"<color=green>'{fileName}' kaydedildi!</color>";
            }


        }

        private void PerformNewScenario()
        {
            try
            {
                // GraphManager üzerinden new scenario işlemini çağır
                if (graphManager != null)
                {
                    // GraphManager'daki CreateNewScenario metodunu reflection ile çağır
                    var createNewScenarioMethod = graphManager.GetType().GetMethod("CreateNewScenario", 
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    
                    if (createNewScenarioMethod != null)
                    {
                        createNewScenarioMethod.Invoke(graphManager, null);
                        Debug.Log("✅ New scenario created successfully via ScenarioFileManager");
                    }
                    else
                    {
                        Debug.LogError("CreateNewScenario metodu GraphManager'da bulunamadı!");
                    }
                }
                else
                {
                    Debug.LogError("GraphManager null! DI container ayarlarını kontrol edin.");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"New scenario creation error: {ex.Message}");
            }
        }

        #endregion

        #region XR Keyboard Methods

        /// <summary>
        /// Input field'a tıklandığında XR klavyeyi açar
        /// </summary>
        public void OnClickInputField()
        {
            if (xrKeyboard != null)
            {
                xrKeyboard.Open();
            }
            else
            {
                Debug.LogWarning("XRKeyboard referansı yok! SetupXRKeyboard() çağrıldığından emin olun.");
            }
        }

        #endregion

        #region XR UI Debug Methods

        [ContextMenu("Debug XR UI Chain")]
        public void DebugXRUIChain()
        {
            Debug.Log("=== XR UI ETKILEŞIM ZİNCİRİ DEBUG ===");
            
            // 1. XR Ray Interactor Kontrol
            var rayInteractors = FindObjectsOfType<UnityEngine.XR.Interaction.Toolkit.Interactors.XRRayInteractor>();
            Debug.Log($"XR Ray Interactor sayısı: {rayInteractors.Length}");
            foreach (var interactor in rayInteractors)
            {
                Debug.Log($"  - {interactor.name}: UI Enabled={interactor.enableUIInteraction}, Mask={interactor.raycastMask}");
            }
            
            // 2. EventSystem Kontrol
            var eventSystem = FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
            if (eventSystem != null)
            {
                var xrInput = eventSystem.GetComponent<UnityEngine.XR.Interaction.Toolkit.UI.XRUIInputModule>();
                var standaloneInput = eventSystem.GetComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                Debug.Log($"EventSystem: XRUIInputModule={xrInput != null}, StandaloneInputModule={standaloneInput != null && standaloneInput.enabled}");
            }
            else
            {
                Debug.LogError("EventSystem YOK!");
            }
            
            // 3. Canvas Kontrol
            if (scenarioPopups != null)
            {
                var canvas = scenarioPopups.GetComponent<Canvas>();
                var raycaster = scenarioPopups.GetComponent<UnityEngine.XR.Interaction.Toolkit.UI.TrackedDeviceGraphicRaycaster>();
                Debug.Log($"Canvas: RenderMode={canvas.renderMode}, Camera={canvas.worldCamera?.name}, TrackedRaycaster={raycaster != null}");
                Debug.Log($"Canvas Layer: {LayerMask.LayerToName(scenarioPopups.layer)}");
            }
            
            // 4. FileList Parent Kontrol  
            if (fileListParent != null)
            {
                Debug.Log($"FileListParent: {fileListParent.name}, Child Count: {fileListParent.childCount}");
                
                // İlk file item'ı detaylı kontrol et
                if (fileListParent.childCount > 0)
                {
                    var firstChild = fileListParent.GetChild(0);
                    var button = firstChild.GetComponent<Button>();
                    var xrInteractable = firstChild.GetComponent<UnityEngine.XR.Interaction.Toolkit.Interactables.XRSimpleInteractable>();
                    var image = firstChild.GetComponent<Image>();
                    var collider = firstChild.GetComponent<Collider>();
                    var rect = firstChild.GetComponent<RectTransform>();
                    var layoutElement = firstChild.GetComponent<LayoutElement>();
                    
                    Debug.Log($"=== İLK FILE ITEM DETAY ===");
                    Debug.Log($"Name: {firstChild.name}");
                    Debug.Log($"Button: {button != null}, XRInteractable: {xrInteractable != null}, Image: {image != null}, Collider: {collider != null}");
                    
                    // RectTransform detayları
                    Debug.Log($"RectTransform:");
                    Debug.Log($"  - Size: {rect.sizeDelta}");
                    Debug.Log($"  - Anchors: Min={rect.anchorMin}, Max={rect.anchorMax}");
                    Debug.Log($"  - Offsets: Min={rect.offsetMin}, Max={rect.offsetMax}");
                    Debug.Log($"  - Actual Rect: {rect.rect}");
                    Debug.Log($"  - World Corners: {string.Join(", ", GetWorldCorners(rect))}");
                    
                    // LayoutElement detayları
                    if (layoutElement != null)
                    {
                        Debug.Log($"LayoutElement:");
                        Debug.Log($"  - minWidth: {layoutElement.minWidth}, preferredWidth: {layoutElement.preferredWidth}");
                        Debug.Log($"  - flexibleWidth: {layoutElement.flexibleWidth}");
                        Debug.Log($"  - ignoreLayout: {layoutElement.ignoreLayout}");
                    }
                    
                    // Image raycast kontrol
                    if (image != null)
                    {
                        Debug.Log($"Image: RaycastTarget={image.raycastTarget}, Color={image.color}");
                    }
                    
                    // Collider kontrol
                    if (collider != null)
                    {
                        Debug.Log($"Collider: Size={collider.bounds.size}, Center={collider.bounds.center}");
                    }
                    
                    // Text child kontrol
                    var textChild = firstChild.Find("Text");
                    if (textChild != null)
                    {
                        var textComponent = textChild.GetComponent<TextMeshProUGUI>();
                        Debug.Log($"Text: RaycastTarget={textComponent.raycastTarget}");
                    }
                }
            }
            
            Debug.Log("=== DEBUG BİTTİ ===");
        }
        
        /// <summary>
        /// RectTransform'un dünya koordinatlarındaki köşelerini alır
        /// </summary>
        private Vector3[] GetWorldCorners(RectTransform rectTransform)
        {
            Vector3[] corners = new Vector3[4];
            rectTransform.GetWorldCorners(corners);
            return corners;
        }

        #endregion

        #region Unity Lifecycle

        private void OnDestroy()
        {
            // Event temizliği
            if (saveOkButton != null) saveOkButton.onClick.RemoveAllListeners();
            if (saveCancelButton != null) saveCancelButton.onClick.RemoveAllListeners();
            if (loadOkButton != null) loadOkButton.onClick.RemoveAllListeners();
            if (loadCancelButton != null) loadCancelButton.onClick.RemoveAllListeners();
            if (overwriteYesButton != null) overwriteYesButton.onClick.RemoveAllListeners();
            if (overwriteNoButton != null) overwriteNoButton.onClick.RemoveAllListeners();
            
            ClearFileList();
        }

        internal async void ReloadScenario()
        {
            if (File.Exists(lastFilePath))
            {
                if (OnLoadRequested != null)
                {
                    await OnLoadRequested.Invoke(lastFilePath);
                }
            }
            else
            {
                LogManager.LogWarning("ScenarioFileManager: Last file path not found");
            }
        }


        #endregion
    }
} 