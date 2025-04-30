using UnityEngine;

namespace Interfaces
{
    public interface IGraphElement : IElement
    {
        string ID { get; set; }
        int Priority { get; }
        void Remove();
    }
}
