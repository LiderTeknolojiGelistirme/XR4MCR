using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Models.Nodes;
using Managers;
using System.Threading.Tasks;

namespace Presenters.NodePresenters
{
    public class VFXActionNodePresenter : ActionNodePresenter
    {
        protected VFXActionNode VFXModel => Model as VFXActionNode;

        [SerializeField] private TMP_Dropdown effectDropdown;
        [SerializeField] private TMP_InputField durationInputField;
        [SerializeField] private Button increaseButton;
        [SerializeField] private Button decreaseButton;
        [SerializeField] private Button selectTargetButton;
        [SerializeField] private Toggle toggleDuration;

        private GameObject _instantiatedVFX;
        private List<GameObject> _vfxPrefabs = new List<GameObject>();
        private float _duration = 2.0f;
        private GameObject _targetSphere;
        private bool _holdingTarget = false;

        protected override void Awake()
        {
            base.Awake();
            LogManager.LogSuccess("VFXActionNodePresenter started: " + gameObject.name);
        }

        private void Start()
        {
            LoadVFXPrefabs();
            
            // Sadece UI listener'ları setup et, dropdown setup'ını SyncModelToUI'a bırak
            SetupUIListeners();
        }
        
        private void SetupUIListeners()
        {
            // Sadece listener'ları setup et, dropdown populate etme
            if (durationInputField != null)
            {
                durationInputField.text = _duration.ToString();
                durationInputField.onEndEdit.AddListener(OnDurationChanged);
            }
            
            if (increaseButton != null)
            {
                increaseButton.onClick.AddListener(OnIncreaseTime);
            }
            
            if (decreaseButton != null)
            {
                decreaseButton.onClick.AddListener(OnDecreaseTime);
            }
            
            if (selectTargetButton != null)
            {
                selectTargetButton.onClick.AddListener(OnSelectTarget);
            }
            
            if (toggleDuration != null)
            {
                toggleDuration.onValueChanged.AddListener(OnToggleValueChanged);
            }
            
            LogManager.LogSuccess("VFX UI listeners setup completed");
        }
        
        private void Update()
        {
            if (_holdingTarget && _targetSphere != null)
            {
                if (XRInputManager.GetRawTriggerState())
                {
                    var parent = GameObject.Find("Root").transform;
                    _targetSphere.transform.SetParent(parent);
                    _holdingTarget = false;
                    
                    // Target position'ı model'e kaydet
                    if (VFXModel != null)
                    {
                        VFXModel.TargetPosition = _targetSphere.transform.position;
                        LogManager.LogSuccess($"VFX target position saved to model: {VFXModel.TargetPosition}");
                    }
                    
                    LogManager.LogSuccess($"VFX target position set: {_targetSphere.transform.position}");
                }
            }
        }

        private void LoadVFXPrefabs()
        {
            _vfxPrefabs.Clear();
            
            GameObject[] prefabs = Resources.LoadAll<GameObject>("VFX");
            
            if (prefabs != null && prefabs.Length > 0)
            {
                _vfxPrefabs.AddRange(prefabs);
                LogManager.LogSuccess($"VFX prefabs loaded: {prefabs.Length} items");
            }
            else
            {
                LogManager.LogError("No VFX prefabs found in Resources/VFX folder");
            }
        }
        
        private void OnEffectSelected(int index)
        {
            LogManager.LogInteraction($"VFX effect dropdown selection changed: index {index}");
            
            if (VFXModel != null && index >= 0 && index < _vfxPrefabs.Count)
            {
                VFXModel.SelectedEffect = _vfxPrefabs[index].name;
                VFXModel.SelectedVFXIndex = index;
                
                LogManager.LogSuccess($"VFX effect selected: {VFXModel.SelectedEffect} (Index: {index})");
            }
            else
            {
                LogManager.LogWarning("Invalid VFX effect selection");
            }
        }
        
        private void OnDurationChanged(string value)
        {
            if (float.TryParse(value, out float duration))
            {
                _duration = duration;
                if (VFXModel != null)
                {
                    VFXModel.Duration = duration;
                    LogManager.LogSuccess($"VFX duration set to: {duration} seconds");
                }
            }
            else
            {
                LogManager.LogWarning($"Invalid VFX duration value: {value}");
            }
        }
        
        private void OnIncreaseTime()
        {
            LogManager.LogInteraction("Increase VFX duration button clicked");
            
            _duration += 1f;
            durationInputField.text = Mathf.RoundToInt(_duration).ToString();
            if (VFXModel != null)
            {
                VFXModel.Duration = _duration;
            }
            
            LogManager.LogSuccess($"VFX duration increased: {_duration}");
        }
        
        private void OnDecreaseTime()
        {
            LogManager.LogInteraction("Decrease VFX duration button clicked");
            
            _duration -= 1f;
            if (_duration < 1f)
            {
                _duration = 1f;
            }
            durationInputField.text = Mathf.RoundToInt(_duration).ToString();
            if (VFXModel != null)
            {
                VFXModel.Duration = _duration;
            }
            
            LogManager.LogSuccess($"VFX duration decreased: {_duration}");
        }
        
        private void OnToggleValueChanged(bool isOn)
        {
            LogManager.LogInteraction($"VFX duration toggle changed: {isOn}");
            
            if (VFXModel != null)
            {
                VFXModel.UseDuration = isOn;
                
                // UI kontrol durumlarını güncelle
                if (durationInputField != null)
                    durationInputField.interactable = isOn;
                if (increaseButton != null)
                    increaseButton.interactable = isOn;
                if (decreaseButton != null)
                    decreaseButton.interactable = isOn;
                
                LogManager.LogSuccess($"VFX duration settings {(isOn ? "enabled" : "disabled")}");
            }
        }
        
        private void OnSelectTarget()
        {
            LogManager.LogInteraction("Select VFX target position button clicked");
            
            try
            {
                if (_targetSphere == null)
                {
                    _targetSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    _targetSphere.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
            
                    Material mat = new Material(Shader.Find("Standard"));
                    mat.color = new Color(0, 1, 0, 0.5f);
                    _targetSphere.GetComponent<Renderer>().material = mat;
            
                    Destroy(_targetSphere.GetComponent<Collider>());
                }
        
                _targetSphere.transform.SetParent(XRInputManager.xrRayInteractor.transform);
                _targetSphere.transform.localPosition = Vector3.zero;
                _holdingTarget = true;
        
                LogManager.LogSuccess("VFX target position selection started. Use trigger to set position.");
            }
            catch (Exception e)
            {
                LogManager.LogError("VFX target selection error: " + e.Message);
            }
        }
        
        private void PlayVFXPreview()
        {
            StopVFXPreview();
    
            int selectedIndex = effectDropdown != null ? effectDropdown.value : 0;
    
            if (selectedIndex >= 0 && selectedIndex < _vfxPrefabs.Count)
            {
                Vector3 position = Vector3.zero;
        
                if (_targetSphere != null)
                {
                    position = _targetSphere.transform.position;
                }
                else if (VFXModel != null && VFXModel.HasTargetPosition)
                {
                    position = VFXModel.TargetPosition;
                }
        
                _instantiatedVFX = Instantiate(_vfxPrefabs[selectedIndex], position, Quaternion.identity);
                
                if (toggleDuration != null && toggleDuration.isOn)
                {
                    Destroy(_instantiatedVFX, _duration);
                }
            }
        }

        public override void StopAction()
        {
            base.StopAction();
            StopVFXPreview();
        }

        public void StopVFXPreview()
        {
            if (_instantiatedVFX != null)
            {
                Destroy(_instantiatedVFX);
                _instantiatedVFX = null;
            }
        }
        
        protected override async Task PerformActionAsync()
        {
            int selectedIndex = effectDropdown != null ? effectDropdown.value : 0;
    
            if (selectedIndex >= 0 && selectedIndex < _vfxPrefabs.Count)
            {
                Vector3 position = Vector3.zero;
        
                if (_targetSphere != null)
                {
                    position = _targetSphere.transform.position;
                }
                else if (VFXModel != null && VFXModel.HasTargetPosition)
                {
                    position = VFXModel.TargetPosition;
                }
        
                _instantiatedVFX = Instantiate(_vfxPrefabs[selectedIndex], position, Quaternion.identity);
                
                bool useDuration = toggleDuration != null ? toggleDuration.isOn : VFXModel?.UseDuration ?? false;
                
                if (useDuration)
                {
                    await Task.Delay(Mathf.RoundToInt(_duration * 1000));
                    
                    if (_instantiatedVFX != null)
                    {
                        Destroy(_instantiatedVFX);
                        _instantiatedVFX = null;
                    }
                }
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
    
            if (effectDropdown != null)
                effectDropdown.onValueChanged.RemoveAllListeners();
        
            if (durationInputField != null)
                durationInputField.onEndEdit.RemoveAllListeners();
        
            if (increaseButton != null)
                increaseButton.onClick.RemoveAllListeners();
        
            if (decreaseButton != null)
                decreaseButton.onClick.RemoveAllListeners();
        
            if (selectTargetButton != null)
                selectTargetButton.onClick.RemoveAllListeners();
            
            if (toggleDuration != null)
                toggleDuration.onValueChanged.RemoveAllListeners();
        
            StopVFXPreview();
    
            if (_targetSphere != null)
            {
                Destroy(_targetSphere);
                _targetSphere = null;
            }
        }

        /// <summary>
        /// Model'deki değerleri UI'ya aktarır (yükleme sonrası)
        /// </summary>
        public override void SyncModelToUI()
        {
            // Önce base sınıfın ortak özelliklerini sync et
            base.SyncModelToUI();
            
            if (VFXModel == null) return;

            // VFX prefabs'ı yükle (eğer boşsa)
            if (_vfxPrefabs.Count == 0)
            {
                LoadVFXPrefabs();
            }

            // Dropdown'u her durumda setup et
            if (effectDropdown != null)
            {
                effectDropdown.ClearOptions();
                List<string> options = new List<string>();
                
                foreach (var prefab in _vfxPrefabs)
                {
                    options.Add(prefab.name);
                }
                
                effectDropdown.AddOptions(options);
                
                // Listener'ı ekle (eğer daha önce eklenmemişse)
                effectDropdown.onValueChanged.RemoveAllListeners();
                effectDropdown.onValueChanged.AddListener(OnEffectSelected);
                
                LogManager.LogSuccess($"VFXNode: Dropdown setup completed with {options.Count} items");
            }

            // Seçili VFX index'ini restore et - En kritik kısım!
            if (effectDropdown != null && VFXModel.SelectedVFXIndex >= 0 && 
                VFXModel.SelectedVFXIndex < effectDropdown.options.Count)
            {
                // Dropdown listener'ını geçici olarak kaldır (OnEffectSelected tetiklenmesini engelle)
                effectDropdown.onValueChanged.RemoveListener(OnEffectSelected);
                
                // Dropdown value'yu set et
                effectDropdown.value = VFXModel.SelectedVFXIndex;
                
                // Manual olarak dropdown refresh
                effectDropdown.RefreshShownValue();
                
                // Listener'ı geri ekle
                effectDropdown.onValueChanged.AddListener(OnEffectSelected);
                
                LogManager.LogSuccess($"VFXNode: Selected effect index restored: {VFXModel.SelectedVFXIndex} ({VFXModel.SelectedEffect})");
            }
            else if (effectDropdown != null && !string.IsNullOrEmpty(VFXModel.SelectedEffect))
            {
                // Index çalışmıyorsa, effect name ile arama yap
                LogManager.LogWarning($"VFXNode: Index mismatch, searching by name: {VFXModel.SelectedEffect}");
                
                effectDropdown.onValueChanged.RemoveListener(OnEffectSelected);
                
                for (int i = 0; i < effectDropdown.options.Count; i++)
                {
                    if (effectDropdown.options[i].text == VFXModel.SelectedEffect)
                    {
                        effectDropdown.value = i;
                        effectDropdown.RefreshShownValue();
                        VFXModel.SelectedVFXIndex = i; // Index'i düzelt
                        LogManager.LogSuccess($"VFXNode: Found effect by name at index {i}: {VFXModel.SelectedEffect}");
                        break;
                    }
                }
                
                effectDropdown.onValueChanged.AddListener(OnEffectSelected);
            }
            else if (effectDropdown != null && _vfxPrefabs.Count > 0)
            {
                // Hiçbir seçili effect yoksa, ilk option'ı seç (sadece UI'da, model'e yazmadan)
                effectDropdown.onValueChanged.RemoveListener(OnEffectSelected);
                effectDropdown.value = 0;
                effectDropdown.RefreshShownValue();
                effectDropdown.onValueChanged.AddListener(OnEffectSelected);
                
                LogManager.LogSuccess("VFXNode: Default first option selected for UI");
            }
            
            // Duration'ı UI'ya aktar
            _duration = VFXModel.Duration;
            if (durationInputField != null)
            {
                durationInputField.text = _duration.ToString();
            }

            // Toggle durumunu UI'ya aktar
            if (toggleDuration != null)
            {
                // Listener'ı geçici olarak kaldır
                toggleDuration.onValueChanged.RemoveListener(OnToggleValueChanged);
                
                // Toggle value'yu set et
                toggleDuration.isOn = VFXModel.UseDuration;
                
                // UI kontrol durumlarını güncelle
                if (durationInputField != null)
                    durationInputField.interactable = VFXModel.UseDuration;
                if (increaseButton != null)
                    increaseButton.interactable = VFXModel.UseDuration;
                if (decreaseButton != null)
                    decreaseButton.interactable = VFXModel.UseDuration;
                
                // Listener'ı geri ekle
                toggleDuration.onValueChanged.AddListener(OnToggleValueChanged);
            }

            // Target position'ı restore et
            if (VFXModel.HasTargetPosition)
            {
                if (_targetSphere == null)
                {
                    _targetSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    _targetSphere.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
            
                    Material mat = new Material(Shader.Find("Standard"));
                    mat.color = new Color(0, 1, 0, 0.5f);
                    _targetSphere.GetComponent<Renderer>().material = mat;
            
                    Destroy(_targetSphere.GetComponent<Collider>());
                }
                
                var parent = GameObject.Find("Root").transform;
                _targetSphere.transform.SetParent(parent);
                _targetSphere.transform.position = VFXModel.TargetPosition;
                
                LogManager.LogSuccess($"VFXNode: Target position restored: {VFXModel.TargetPosition}");
            }

            LogManager.LogSuccess($"VFXNode UI synced - Effect: {VFXModel.SelectedEffect}, Index: {VFXModel.SelectedVFXIndex}, Duration: {VFXModel.Duration}, UseDuration: {VFXModel.UseDuration}");
        }
    }
}