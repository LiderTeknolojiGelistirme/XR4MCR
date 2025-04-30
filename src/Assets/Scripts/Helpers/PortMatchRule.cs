using Models;
using UnityEngine;

namespace Helpers
{
    public abstract class PortMatchRule : MonoBehaviour
    {
        [SerializeField] static PortMatchRule _instance;

        public static PortMatchRule Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<PortMatchRule>();
                }

                return _instance;
            }
            set => _instance = value;
        }

        public virtual bool ExecuteRule(Port draggedPort, Port foundPort)
        {
            return true;
        }
    }
}