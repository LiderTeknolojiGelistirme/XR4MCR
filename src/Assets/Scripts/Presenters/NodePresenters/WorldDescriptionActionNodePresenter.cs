using System.Collections;
using Models.Nodes;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Samples.SpatialKeyboard;
using Zenject;
using Managers;
using Enums;

namespace Presenters.NodePresenters
{
    public class WorldDescriptionActionNodePresenter : ActionNodePresenter
    {
        [Inject] XRKeyboard _keyboard;
        [Inject] NodeConfig nodeConfig;
        [SerializeField] private TMP_InputField narrativeText;

        [SerializeField] private FlexibleColorPicker fcp_text;
        [SerializeField] private FlexibleColorPicker fcp_bg;

        [SerializeField] private Button btn_txt;
        [SerializeField] private Button btn_bg;
        [SerializeField] private XRKeyboardDisplay XRKeyboardDisplay;
        [SerializeField] private Button locateCanvasButton;
        [SerializeField] private Button durationIncreaseButton;
        [SerializeField] private Button durationDecreaseButton;
        [SerializeField] private TMP_InputField durationInputField;

        public WorldNotifierCanvas nc;
        private Camera cam;
        private GameObject centerEyeAnchor;
        private GameObject _instantiatedCanvasGameObject;
        private bool _holdingTarget;

        // Model'e kolay erişim için cast property
        private WorldDescriptionActionNode WorldDescriptionModel => Model as WorldDescriptionActionNode;

        IEnumerator Start()
        {
            // Keep looking for the camera until it's found
            while (cam == null)
            {
                cam = Camera.main;
                if (cam == null)
                {
                    yield return new WaitForSeconds(0.1f);
                }
                else
                {
                    centerEyeAnchor = cam.gameObject;

                    break;
                }
            }
            
            // Default canvas'ı oluştur (kullanıcı görebilsin ve isterse yerini değiştirebilsin)
            CreateDefaultCanvas();
        }
        
        protected override void Update()
        {
            base.Update();
            
            if (_holdingTarget)
            {
                if (XRInputManager.GetRawTriggerState())
                {
                    var parent = GameObject.Find("Root").transform;
                    Debug.Log(parent.name);
                    _instantiatedCanvasGameObject.transform.parent = parent;
                    _holdingTarget = false;
                    
                    // Canvas yerleştirildiğinde transform bilgilerini modele kaydet
                    SaveCanvasTransformToModel();
                }
            }
            
            // MVP: Color picker değişikliklerini sürekli kontrol et ve modele yansıt
            if (WorldDescriptionModel != null)
            {
                bool colorChanged = false;
                
                if (fcp_text != null && WorldDescriptionModel.TextColor != fcp_text.color)
                {
                    WorldDescriptionModel.TextColor = fcp_text.color;
                    LogManager.LogInteraction($"World text color updated: {fcp_text.color}");
                    colorChanged = true;
                }

                if (fcp_bg != null && WorldDescriptionModel.BackgroundColor != fcp_bg.color)
                {
                    WorldDescriptionModel.BackgroundColor = fcp_bg.color;
                    LogManager.LogInteraction($"World background color updated: {fcp_bg.color}");
                    colorChanged = true;
                }
                
                // Renk değişti ise preview'ı güncelle
                if (colorChanged)
                {
                    UpdateCanvasPreview();
                }
            }
        }

        protected override void Awake()
        {
            base.Awake();
            
            XRKeyboardDisplay.keyboard = _keyboard;
            SetActionType(NodeType.WorldDescriptionActionNode);

            if (btn_txt != null)
            {
                btn_txt.onClick.AddListener(OnButtonClick_Text);
            }

            if (btn_bg != null)
            {
                btn_bg.onClick.AddListener(OnButtonClick_BackGround);
            }

            if (locateCanvasButton != null)
            {
                locateCanvasButton.onClick.AddListener(OnLocateCanvas);
            }
            durationIncreaseButton.onClick.AddListener(OnIncreaseDuration);
            durationDecreaseButton.onClick.AddListener(OnDecreaseDuration);

            // Default color'ları initialize et
            InitializeDefaultColors();
            
            // MVP: UI değişikliklerini modele anında yansıt
            SetupUIToModelBinding();
        }

        /// <summary>
        /// Default color'ları initialize eder (hiçbir renk seçilmediyse)
        /// </summary>
        private void InitializeDefaultColors()
        {
            // Color picker'ları default değerlerle initialize et
            if (fcp_text != null)
            {
                fcp_text.color = new Color(235f,236f,236f,255f); // Default text color: beyaz
            }
            
            if (fcp_bg != null)
            {
                fcp_bg.color = new Color(0f,49f,65f,255f); // Default background color: siyah/şeffaf
            }
            
            LogManager.LogInteraction("Default colors initialized: Text=White, Background=Black");
        }

        private void SetupUIToModelBinding()
        {
            // Text değişikliklerini modele yansıt
            if (narrativeText != null)
            {
                narrativeText.onValueChanged.AddListener(OnNarrativeTextChanged);
            }

            // Duration input field değişikliklerini modele yansıt
            if (durationInputField != null)
            {
                durationInputField.onValueChanged.AddListener(OnDurationInputChanged);
            }

            // Color picker değişiklikleri Update() metodunda kontrol ediliyor
        }

        private void OnNarrativeTextChanged(string newText)
        {
            if (WorldDescriptionModel != null)
            {
                WorldDescriptionModel.WorldMessageText = newText;
                LogManager.LogInteraction($"World narrative text updated: {newText}");
                
                // Real-time preview güncelle
                UpdateCanvasPreview();
            }
        }

        private void OnDurationInputChanged(string newDurationStr)
        {
            if (WorldDescriptionModel != null && int.TryParse(newDurationStr, out int newDuration))
            {
                if (newDuration >= 0)
                {
                    WorldDescriptionModel.DisplayDuration = newDuration;
                    LogManager.LogInteraction($"World duration updated via input: {newDuration}");
                }
            }
        }

        /// <summary>
        /// Canvas transform bilgilerini modele kaydeder
        /// </summary>
        private void SaveCanvasTransformToModel()
        {
            if (WorldDescriptionModel != null && _instantiatedCanvasGameObject != null)
            {
                var canvasTransform = _instantiatedCanvasGameObject.transform;
                
                // Transform bilgilerini modele kaydet (world coordinates)
                WorldDescriptionModel.CanvasPosition = canvasTransform.position;
                WorldDescriptionModel.CanvasRotation = canvasTransform.rotation;
                WorldDescriptionModel.CanvasScale = canvasTransform.lossyScale; // World scale
                
                // Parent bilgisini kaydet
                if (canvasTransform.parent != null)
                {
                    WorldDescriptionModel.CanvasParentName = canvasTransform.parent.name;
                }
                else
                {
                    WorldDescriptionModel.CanvasParentName = "";
                }
                
                WorldDescriptionModel.IsCanvasPlaced = true;
                
                LogManager.LogSuccess($"World canvas transform saved to model: Position={WorldDescriptionModel.CanvasPosition}, Rotation={WorldDescriptionModel.CanvasRotation}, Parent={WorldDescriptionModel.CanvasParentName}");
            }
        }

        /// <summary>
        /// Model'den canvas transform bilgilerini restore eder
        /// </summary>
        private void RestoreCanvasFromModel()
        {
            if (WorldDescriptionModel != null && WorldDescriptionModel.IsCanvasPlaced)
            {
                // Mevcut canvas'ı kontrol et ve varsa temizle (leakage önleme)
                CleanupExistingCanvas();
                
                // Canvas'ı oluştur
                _instantiatedCanvasGameObject = Instantiate(nodeConfig.worldNotificationCanvas);
                nc = _instantiatedCanvasGameObject.GetComponent<WorldNotifierCanvas>();
                
                var canvasTransform = _instantiatedCanvasGameObject.transform;
                
                // Önce world transform'ları set et
                canvasTransform.position = WorldDescriptionModel.CanvasPosition;
                canvasTransform.rotation = WorldDescriptionModel.CanvasRotation;
                canvasTransform.localScale = WorldDescriptionModel.CanvasScale;
                
                // Sonra parent'ı ayarla (transform'lar korunur)
                if (!string.IsNullOrEmpty(WorldDescriptionModel.CanvasParentName))
                {
                    var parentObject = GameObject.Find(WorldDescriptionModel.CanvasParentName);
                    if (parentObject != null)
                    {
                        canvasTransform.SetParent(parentObject.transform, true); // worldPositionStays = true
                    }
                    else
                    {
                        LogManager.LogWarning($"Canvas parent not found: {WorldDescriptionModel.CanvasParentName}");
                        // Fallback olarak Root altına yerleştir
                        var rootParent = GameObject.Find("Root")?.transform;
                        if (rootParent != null)
                        {
                            canvasTransform.SetParent(rootParent, true);
                        }
                    }
                }
                
                // Canvas'ı göster ve içeriği yükle
                nc.descriptionPanel.SetActive(true);
                UpdateCanvasPreview();
                
                LogManager.LogSuccess($"World canvas restored from model: Position={WorldDescriptionModel.CanvasPosition}, Rotation={WorldDescriptionModel.CanvasRotation}, Parent={WorldDescriptionModel.CanvasParentName}");
            }
        }

        /// <summary>
        /// Bu presenter'a ait mevcut canvas'ı temizler
        /// </summary>
        private void CleanupExistingCanvas()
        {
            if (_instantiatedCanvasGameObject != null)
            {
                LogManager.LogInteraction($"Cleaning up existing canvas: {_instantiatedCanvasGameObject.name}");
                DestroyImmediate(_instantiatedCanvasGameObject);
                _instantiatedCanvasGameObject = null;
                nc = null;
            }
        }

        /// <summary>
        /// Component destroy edildiğinde canvas'ı temizle
        /// </summary>
        private void OnDestroy()
        {
            CleanupExistingCanvas();
        }

        /// <summary>
        /// Model verilerini UI'ya senkronize eder (Load işlemi sonrası)
        /// </summary>
        public override void SyncModelToUI()
        {
            base.SyncModelToUI(); // Base sınıfın sync metodunu çağır
            
            if (WorldDescriptionModel == null)
            {
                LogManager.LogWarning("WorldDescriptionActionNode model is null, cannot sync to UI");
                return;
            }

            // Text içeriğini sync et
            if (narrativeText != null)
            {
                narrativeText.text = WorldDescriptionModel.WorldMessageText ?? "";
            }

            // Duration'ı sync et
            if (durationInputField != null)
            {
                durationInputField.text = WorldDescriptionModel.DisplayDuration.ToString();
            }

            // Text color'ı sync et
            if (fcp_text != null)
            {
                fcp_text.color = WorldDescriptionModel.TextColor;
            }

            // Background color'ı sync et
            if (fcp_bg != null)
            {
                fcp_bg.color = WorldDescriptionModel.BackgroundColor;
            }

            // Canvas'ı restore et (eğer önceden yerleştirilmişse)
            RestoreCanvasFromModel();
            
            // Eğer canvas yok ise default canvas oluştur
            if (_instantiatedCanvasGameObject == null)
            {
                CreateDefaultCanvas();
            }

            LogManager.LogSuccess($"WorldDescriptionActionNode UI synced from model: Text='{WorldDescriptionModel.WorldMessageText}', Duration={WorldDescriptionModel.DisplayDuration}, CanvasPlaced={WorldDescriptionModel.IsCanvasPlaced}");
        }

        public new void OnClickInputField()
        {
            _keyboard.Open();
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            if (btn_txt != null)
            {
                btn_txt.onClick.RemoveAllListeners();
            }

            if (btn_bg != null)
            {
                btn_bg.onClick.RemoveAllListeners();
            }

            durationIncreaseButton.onClick.RemoveAllListeners();
            durationDecreaseButton.onClick.RemoveAllListeners();
            
            // UI event'lerini temizle
            if (narrativeText != null)
            {
                narrativeText.onValueChanged.RemoveAllListeners();
            }
            
            if (durationInputField != null)
            {
                durationInputField.onValueChanged.RemoveAllListeners();
            }
        }

        protected override void PerformAction()
        {
            base.PerformAction();

            // Model'den değerleri al ve kullan
            if (WorldDescriptionModel != null)
            {
                nc.descriptionPanel.GetComponentInChildren<TMP_Text>().text = WorldDescriptionModel.WorldMessageText;
                nc.descriptionPanel.GetComponentInChildren<TMP_Text>().color = WorldDescriptionModel.TextColor;
                nc.descriptionPanel.GetComponent<Image>().color = WorldDescriptionModel.BackgroundColor;

                nc.ShowDescriptionPanel();
            }
        }

        /// <summary>
        /// Stop action için world description panelini hemen gizler
        /// </summary>
        public override void StopAction()
        {
            base.StopAction();
            
            if (nc != null)
            {
                nc.HideDescriptionPanel(); // Hemen gizle
                LogManager.LogInteraction("World description panel stopped via StopAction");
            }
        }

        /// <summary>
        /// Canvas'ı senaryo başında gizlemek için kullanılır
        /// </summary>
        public void HideCanvasOnScenarioStart()
        {
            if (nc != null)
            {
                nc.HideDescriptionPanel();
                LogManager.LogInteraction($"World description canvas hidden on scenario start: {Model.Title}");
            }
        }

        private void OnLocateCanvas()
        {
            LogManager.LogInteraction("Locate world canvas button clicked");
            
            // Mevcut canvas'ı temizle (eğer varsa)
            CleanupExistingCanvas();
            
            // Yeni canvas oluştur
            _instantiatedCanvasGameObject =
                Instantiate(nodeConfig.worldNotificationCanvas,
                    XRInputManager.xrRayInteractor.transform);
            nc = _instantiatedCanvasGameObject.GetComponent<WorldNotifierCanvas>();

            _holdingTarget = true;
            
            // Canvas'ı göster ve preview'ı güncelle
            nc.descriptionPanel.SetActive(true);
            UpdateCanvasPreview();
            
            LogManager.LogSuccess("World canvas creation started");
        }

        private void OnButtonClick_Text()
        {
            LogManager.LogInteraction("World text color picker button clicked");
            
            if (fcp_text.gameObject.activeSelf == true)
            {
                fcp_text.gameObject.SetActive(false);
                LogManager.LogSuccess("World text color picker closed");
            }
            else
            {
                fcp_text.gameObject.SetActive(true);
                LogManager.LogSuccess("World text color picker opened");
            }
        }

        private void OnButtonClick_BackGround()
        {
            LogManager.LogInteraction("World background color picker button clicked");
            
            if (fcp_bg.gameObject.activeSelf == true)
            {
                fcp_bg.gameObject.SetActive(false);
                LogManager.LogSuccess("World background color picker closed");
            }
            else
            {
                fcp_bg.gameObject.SetActive(true);
                LogManager.LogSuccess("World background color picker opened");
            }
        }
        
        private void OnIncreaseDuration()
        {
            LogManager.LogInteraction("Increase world description duration button clicked");
            
            if (WorldDescriptionModel != null)
            {
                WorldDescriptionModel.DisplayDuration++;
                
                // UI'yı güncelle
                if (durationInputField != null)
                {
                    durationInputField.text = WorldDescriptionModel.DisplayDuration.ToString();
                }
                
                LogManager.LogSuccess($"World description duration increased: {WorldDescriptionModel.DisplayDuration}");
            }
        }

        private void OnDecreaseDuration()
        {
            LogManager.LogInteraction("Decrease world description duration button clicked");
            
            if (WorldDescriptionModel != null)
            {
                if (WorldDescriptionModel.DisplayDuration > 0)
                {
                    WorldDescriptionModel.DisplayDuration--;
                    
                    // UI'yı güncelle
                    if (durationInputField != null)
                    {
                        durationInputField.text = WorldDescriptionModel.DisplayDuration.ToString();
                    }
                    
                    LogManager.LogSuccess($"World description duration decreased: {WorldDescriptionModel.DisplayDuration}");
                }
                else
                {
                    LogManager.LogWarning("World description duration cannot be less than 0");
                }
            }
        }

        /// <summary>
        /// Default olarak canvas'ı sahneye ekler (preview için)
        /// </summary>
        private void CreateDefaultCanvas()
        {
            if (_instantiatedCanvasGameObject == null && WorldDescriptionModel != null)
            {
                // Default pozisyonda canvas oluştur
                _instantiatedCanvasGameObject = Instantiate(nodeConfig.worldNotificationCanvas);
                nc = _instantiatedCanvasGameObject.GetComponent<WorldNotifierCanvas>();
                
                // Default parent olarak Root'u kullan
                var rootParent = GameObject.Find("Root")?.transform;
                if (rootParent != null)
                {
                    _instantiatedCanvasGameObject.transform.SetParent(rootParent);
                }
                
                // Default pozisyon - ScenarioArea'nın ortası
                var scenarioArea = GameObject.Find("ScenarioArea");
                if (scenarioArea != null)
                {
                    // ScenarioArea'nın merkezi pozisyonu
                    var bounds = scenarioArea.GetComponent<Renderer>();
                    if (bounds != null)
                    {
                        _instantiatedCanvasGameObject.transform.position = bounds.bounds.center + Vector3.up * 1.5f; // Biraz yukarıda
                    }
                    else
                    {
                        // Renderer yoksa transform pozisyonunu kullan
                        _instantiatedCanvasGameObject.transform.position = scenarioArea.transform.position + Vector3.up * 1.5f;
                    }
                    
                    // Kameraya bakacak şekilde rotate et
                    if (cam != null)
                    {
                        _instantiatedCanvasGameObject.transform.LookAt(cam.transform);
                    }
                }
                else
                {
                    // Fallback: Kamera önünde 2 metre
                    if (cam != null)
                    {
                        _instantiatedCanvasGameObject.transform.position = cam.transform.position + cam.transform.forward * 2f;
                        _instantiatedCanvasGameObject.transform.LookAt(cam.transform);
                    }
                }
                
                // Canvas'ı başlangıçta göster ama içeriği boş olabilir
                nc.descriptionPanel.SetActive(true);
                
                // İlk içeriği yükle
                UpdateCanvasPreview();
                
                LogManager.LogSuccess("Default world canvas created for preview at ScenarioArea center");
            }
        }

        /// <summary>
        /// Canvas preview'ını güncellem (real-time)
        /// </summary>
        private void UpdateCanvasPreview()
        {
            if (nc != null && WorldDescriptionModel != null)
            {
                // Text içeriğini güncelle
                var textComponent = nc.descriptionPanel.GetComponentInChildren<TMP_Text>();
                if (textComponent != null)
                {
                    textComponent.text = string.IsNullOrEmpty(WorldDescriptionModel.WorldMessageText) 
                        ? "Preview text..." 
                        : WorldDescriptionModel.WorldMessageText;
                }
                
                // Text rengini güncelle
                if (textComponent != null)
                {
                    textComponent.color = WorldDescriptionModel.TextColor;
                }
                
                // Background rengini güncelle
                var imageComponent = nc.descriptionPanel.GetComponent<Image>();
                if (imageComponent != null)
                {
                    imageComponent.color = WorldDescriptionModel.BackgroundColor;
                }
                
                LogManager.LogInteraction("Canvas preview updated");
            }
        }

        #region Edit Mode Functions

        /// <summary>
        /// World description action node için düzenleme modunu açar.
        /// Canvas location button, color picker'lar, duration butonları gösterilir.
        /// </summary>
        public override void EditModeOn()
        {
            base.EditModeOn(); // Base class'ın keyboardDisplay'ini göster

             if (nc != null)
            {
                nc.ShowDescriptionPanel();
                LogManager.LogSuccess("World description preview canvas shown");
            }

        }

        /// <summary>
        /// World description action node için düzenleme modunu kapatır.
        /// Canvas location button, color picker'lar, duration butonları gizlenir.
        /// </summary>
        public override void EditModeOff()
        {
            base.EditModeOff(); // Base class'ın keyboardDisplay'ini gizle
            if (nc != null)
            {
                nc.HideDescriptionPanel(WorldDescriptionModel.DisplayDuration);
                LogManager.LogSuccess("World description preview canvas hidden");
            }

           
        }

        #endregion
    }
}