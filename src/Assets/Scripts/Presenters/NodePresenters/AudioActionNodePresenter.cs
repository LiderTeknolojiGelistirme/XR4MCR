using System;
using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Models.Nodes;
using Managers;
using System.Collections.Generic;
using UnityEditor;

namespace Presenters.NodePresenters
{
    public class AudioActionNodePresenter : ActionNodePresenter, IDisposable
    {
        [SerializeField] private AudioSource au;
        [SerializeField] private Button selectObjectButton;
        [SerializeField] private TMP_Dropdown audioDropdown;
        [SerializeField] private TMP_InputField selectedObjectInputField;

        [SerializeField] private Button previewPlayButton;
        [SerializeField] private Button stopButton;
        [SerializeField] private Toggle loopToggle;
        [SerializeField] private Slider volumeSlider;


        private AudioSource _audioSource;

        private Camera cam;
        private GameObject centerEyeAnchor;
        private GameObject audioSourcesParent;

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

                    // CenterEyeAnchor altında AudioSources GameObject'i kontrol et veya oluştur
                    CheckOrCreateAudioSources();

                    // Yeni bir AudioSource ekle
                    _audioSource = AddNewAudioSource();

                    //Instantiate(_audioSources, centerEyeAnchor.transform);

                    //_audioSource = cam.gameObject.AddComponent<AudioSource>();


                    _audioSource.clip = FindClipInProject(audioDropdown.options[0].text);
                   
                 
                    _audioSource.loop = false;
                    _audioSource.volume = 1f;

                    break;
                }
            }
        }


        protected override void Awake()
        {
            base.Awake();

            SetActionType(ActionNode.ActionType.PlayAudio);

            if (selectObjectButton != null)
            {
                //selectObjectButton.onClick.AddListener(OnSelectObject);
            }

            // Audio dropdown
            if (audioDropdown != null)
            {
                audioDropdown.ClearOptions();

                // Proje içindeki tüm audio kaynaklarını bul
                List<string> audioOptions = FindAllClipsInProject();

                audioDropdown.AddOptions(audioOptions);
                audioDropdown.onValueChanged.AddListener(OnAudioSelected);
            }

            // Log: AudioNodePresenter oluşturuldu
            LogManager.LogSuccess("AudioNodePresenter başlatıldı: " + gameObject.name);

            if (previewPlayButton != null)
            {
                previewPlayButton.onClick.AddListener(OnPreviewPlayClicked);
            }

            if (stopButton != null)
            {
                stopButton.onClick.AddListener(OnStopClicked);
            }

            if (loopToggle != null)
            {
                loopToggle.onValueChanged.AddListener(OnLoopToggleChanged);
            }

            if (volumeSlider != null)
            {
                volumeSlider.minValue = 0f;
                volumeSlider.maxValue = 1f;
                volumeSlider.value = 1f; // Varsayılan %100 ses
                volumeSlider.onValueChanged.AddListener(OnVolumeChanged);
            }


        }

        protected override void OnDisable()
        {
            base.OnDisable();

            Dispose();

            // Buton listener'ını temizle
            if (selectObjectButton != null)
            {
                selectObjectButton.onClick.RemoveAllListeners();
            }

            // Dropdown listener'ını temizle
            if (audioDropdown != null)
            {
                audioDropdown.onValueChanged.RemoveAllListeners();
            }

            if (previewPlayButton != null)
            {
                previewPlayButton.onClick.RemoveAllListeners();
            }

            if (stopButton != null)
            {
                stopButton.onClick.RemoveAllListeners();
            }


            if (loopToggle != null)
            {
                loopToggle.onValueChanged.RemoveAllListeners();
            }

            if (volumeSlider != null)
            {
                volumeSlider.onValueChanged.RemoveAllListeners();
            }

        }

        

        private void OnAudioSelected(int index)
        {
            string audioName = audioDropdown.options[index].text;

            // Model'e kaydet
            SetParameterName("audio");
            SetParameterValue(audioName);

            // AudioSource'u prefab olarak yüklemeye çalışıyoruz
            Debug.Log($"Audio kaynak yüklemeye çalışılıyor: '{audioName}'");

            // AssetDatabase kullanarak "Assets/Audio" klasöründen ses kaynağını bul
            AudioClip selectedClip = FindClipInProject(audioName);

            if (selectedClip != null)
            {
                _audioSource.clip = selectedClip;
                LogManager.LogSuccess($"Audio kaynak seçildi: {audioName}");
            }
            else
            {
                // Audio bulunamadıysa, varsayılan değer kullanılabilir
                _audioSource.clip = null;
                LogManager.LogWarning($"Audio kaynak bulunamadı: {audioName}, varsayılan kullanılacak");
            }
        }



        protected override void PerformAction()
        {
            base.PerformAction();
            
           _audioSource.Play();

        }


        
        private List<string> FindAllClipsInProject()
        {
            List<string> clipNames = new List<string>();

            AudioClip[] clips = Resources.LoadAll<AudioClip>("AudioClips");
            foreach (AudioClip ac in clips)
            {
                if (ac != null)
                {
                    clipNames.Add(ac.name);
                    Debug.Log($"Resources/AudioClips'da bulunan materyal: {ac.name}");
                }
            }

            if (clipNames.Count == 0)
            {
                Debug.LogWarning("Resources/AudioClips klasöründe hiç clip bulunamadı. Varsayılan değerler kullanılıyor.");
                clipNames.Add("Varsayılan");
            }

            SetDropdownItems(clipNames);

            return clipNames;
        }


        
        private AudioClip FindClipInProject(string clipName)
        {
            AudioClip ac = Resources.Load<AudioClip>("AudioClips/" + clipName);

            if (ac != null)
            {
                Debug.Log($"Clip doğrudan bulundu: AudioClips/{clipName}");
                return ac;
            }

            AudioClip[] allClips = Resources.LoadAll<AudioClip>("Clips");
            foreach (AudioClip clip in allClips)
            {
                if (clip != null && clip.name == clipName)
                {
                    Debug.Log($"Clip resources taramasından bulundu: {clipName}");
                    return clip;
                }
            }

            Debug.LogWarning($"Clip bulunamadı: {clipName}");
            return null;
        }
    

        private void OnPreviewPlayClicked()
        {
            if (_audioSource != null)
            {
                _audioSource.Play();

                
                LogManager.LogSuccess("Önizleme sesi oynatıldı.");
            }
            else
            {
                LogManager.LogWarning("Önizleme için ses kaynağı seçilmedi.");
            }
        }

        private void OnStopClicked()
        {
            if (_audioSource != null)
            {
                _audioSource.Stop();


                LogManager.LogSuccess("Önizleme sesi durduruldu.");
            }
            else
            {
                LogManager.LogWarning("Önizleme için ses kaynağı seçilmedi.");
            }
        }

        private void OnLoopToggleChanged(bool isLooping)
        {
            if (isLooping)
                _audioSource.loop = true;
            else
                _audioSource.loop = false;

            SetParameterName("loop");
            SetParameterValue(isLooping.ToString());
        }

        private void OnVolumeChanged(float value)
        {

            _audioSource.volume = value;

            SetParameterName("volume");
            SetParameterValue(value.ToString("F2"));
        }

        void FindCenterEyeAnchor()
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                centerEyeAnchor = mainCamera.gameObject;
                Debug.Log("CenterEyeAnchor bulundu: " + centerEyeAnchor.name);
            }
            else
            {
                Debug.LogError("Ana kamera bulunamadı!");
            }
        }

        void CheckOrCreateAudioSources()
        {
            if (centerEyeAnchor == null)
            {
                Debug.LogError("CenterEyeAnchor bulunamadı, AudioSources oluşturulamıyor!");
                return;
            }

            Transform audioSourcesTransform = centerEyeAnchor.transform.Find("AudioSources");

            if (audioSourcesTransform != null)
            {
                // Varsa, referansı al
                audioSourcesParent = audioSourcesTransform.gameObject;
                Debug.Log("Mevcut AudioSources bulundu");
            }
            else
            {
                audioSourcesParent = new GameObject("AudioSources");
                audioSourcesParent.transform.SetParent(centerEyeAnchor.transform);
                audioSourcesParent.transform.localPosition = Vector3.zero;
                audioSourcesParent.transform.localRotation = Quaternion.identity;
                Debug.Log("Yeni AudioSources oluşturuldu");
            }
        }

        AudioSource AddNewAudioSource()
        {
            if (audioSourcesParent == null)
            {
                Debug.LogError("AudioSources GameObject bulunamadı!");
                return null;
            }

            GameObject newAudioSourceObj = new GameObject("AudioSource_" + System.DateTime.Now.Ticks);
            newAudioSourceObj.transform.SetParent(audioSourcesParent.transform);
            newAudioSourceObj.transform.localPosition = Vector3.zero;
            newAudioSourceObj.transform.localRotation = Quaternion.identity;
            AudioSource audioSource = newAudioSourceObj.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.spatialBlend = 1.0f;
            audioSource.volume = 1.0f;

            Debug.Log("Yeni AudioSource oluşturuldu: " + newAudioSourceObj.name);

            return audioSource;
        }

        protected List<string> SetDropdownItems(List<string> t)
        {
            List<string> clipNames = t;


            return clipNames;
        }

        public void Dispose()
        {
            Destroy(_audioSource.gameObject);
        }

        public void PerformStop()
        {
            _audioSource.Stop();
        }
    }
}
