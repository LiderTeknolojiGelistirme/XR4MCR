using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Models.Nodes;
using Managers;
using System.Threading.Tasks;
using System;
using Presenters;
using Unity.VisualScripting;
using Zenject;

namespace Presenters.NodePresenters

{
    public class GhostNodePresenter: BaseNodePresenter
    {
        [Inject] GraphManager graphManager;
        [Inject] Pointer pointer;
        [Inject] XRInputManager inputManager;

        protected override void Update()
        {
            OnDrag(inputManager.GetCanvasPointerPosition(graphManager));
        }

        
    }
}
