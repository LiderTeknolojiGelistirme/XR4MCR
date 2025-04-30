using UnityEngine;
using UnityEngine.UI;

namespace VirooLab
{
    [RequireComponent(typeof(Button))]
    public class DynamicObjectActionButton : MonoBehaviour
    {
        [SerializeField]
        private ChangeSpriteColorAction changeSpriteColorAction = default;

        protected void Awake()
        {
            Button button = GetComponent<Button>();
            button.onClick.AddListener(() => changeSpriteColorAction.Execute());
        }

        public void SetActionId(string id)
        {
            changeSpriteColorAction.SetId(id);
        }
    }
}
