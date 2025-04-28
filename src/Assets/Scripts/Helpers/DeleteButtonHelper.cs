using Interfaces;
using UnityEngine;
using IContextItem = Interfaces.IContextItem;

namespace Helpers
{
    public class DeleteButtonHelper : MonoBehaviour, IContextItem, IElement
    {
        public int Priority { get; }

        public void Remove()
        {
            throw new System.NotImplementedException();
        }

        // Start is called before the first frame update
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {
        }
        public void OnChangeSelection()
        {
            throw new System.NotImplementedException();
        }
        
    }
}