using System;
using System.Collections;
using TMPro;
using UnityEngine;

namespace VirooLab
{
    public class UITouchPad : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI inputText = default;

        [SerializeField]
        private int maxLength = 4;

        [SerializeField]
        private string correctCode = "1234";

        private bool resettingText;

        public void AddNumber(string number)
        {
            if (resettingText)
            {
                return;
            }

            inputText.text += number;

            if (CheckLength())
            {
                inputText.color = CheckText() ? Color.green : Color.red;

                StartCoroutine(ResetText());
            }
        }

        IEnumerator ResetText()
        {
            resettingText = true;

            yield return new WaitForSeconds(1);

            inputText.text = string.Empty;
            inputText.color = Color.white;

            resettingText = false;
        }

        public bool CheckLength() => inputText.text.Length == maxLength;

        public bool CheckText() => inputText.text.Equals(correctCode, StringComparison.Ordinal);
    }
}
