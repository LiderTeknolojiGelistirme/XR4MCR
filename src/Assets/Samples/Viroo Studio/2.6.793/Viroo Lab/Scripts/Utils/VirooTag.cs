using UnityEngine;

namespace VirooLab
{
    public class VirooTag : MonoBehaviour
    {
        [SerializeField]
        private new string tag = string.Empty;

        public string Tag => tag;
    }
}
