using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace UI
{
    /// <summary>
    /// Dosya listesinde kullanılan tek bir dosya elemanı
    /// </summary>
    public class FileListItem : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI fileNameText;
        [SerializeField] private TextMeshProUGUI fileDateText;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Button selectButton;

        [Header("Visual Settings")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color selectedColor = new Color(0.8f, 0.9f, 1f);
        [SerializeField] private Color hoverColor = new Color(0.95f, 0.95f, 0.95f);

        public string FileName { get; private set; }
        public System.DateTime FileDate { get; private set; }
        public bool IsSelected { get; private set; }

        public System.Action<FileListItem> OnItemSelected;

        private void Awake()
        {
            if (selectButton != null)
            {
                selectButton.onClick.AddListener(OnButtonClicked);
            }
        }

        public void Initialize(string fileName, System.DateTime fileDate)
        {
            FileName = fileName;
            FileDate = fileDate;

            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            if (fileNameText != null)
            {
                fileNameText.text = FileName;
            }

            if (fileDateText != null)
            {
                fileDateText.text = FileDate.ToString("dd.MM.yyyy HH:mm");
            }

            UpdateVisualState();
        }

        public void SetSelected(bool selected)
        {
            IsSelected = selected;
            UpdateVisualState();
        }

        private void UpdateVisualState()
        {
            if (backgroundImage != null)
            {
                backgroundImage.color = IsSelected ? selectedColor : normalColor;
            }
        }

        private void OnButtonClicked()
        {
            OnItemSelected?.Invoke(this);
        }

        private void OnDestroy()
        {
            if (selectButton != null)
            {
                selectButton.onClick.RemoveAllListeners();
            }
        }

        #region Hover Effects (XR uyumlu)

        public void OnPointerEnter()
        {
            if (!IsSelected && backgroundImage != null)
            {
                backgroundImage.color = hoverColor;
            }
        }

        public void OnPointerExit()
        {
            if (!IsSelected && backgroundImage != null)
            {
                backgroundImage.color = normalColor;
            }
        }

        #endregion
    }
} 