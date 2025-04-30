using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

namespace VirooLab
{
    public class DynamicObjectActionButtonGenerator : MonoBehaviour
    {
        [SerializeField]
        private DynamicObjectActionButton prefab = default;

        [SerializeField]
        private VerticalLayoutGroup verticalLayout = default;

        [SerializeField]
        private int instantiateButtonsCount = 3;

        protected void Start()
        {
            InstantiateButtons();
        }

        private void InstantiateButtons()
        {
            for (int i = 0; i < instantiateButtonsCount; i++)
            {
                string actionId = string.Format(CultureInfo.InvariantCulture, "button_{0}", i);

                DynamicObjectActionButton instantiated = Instantiate(prefab).GetComponent<DynamicObjectActionButton>();
                instantiated.transform.SetParent(verticalLayout.transform, worldPositionStays: false);
                instantiated.SetActionId(actionId);

                instantiated.gameObject.SetActive(value: true);
            }
        }
    }
}
