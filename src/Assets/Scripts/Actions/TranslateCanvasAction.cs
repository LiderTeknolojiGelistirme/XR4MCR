using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Viroo.Interactions;

public class TranslateCanvasAction : BroadcastObjectAction
{
    [SerializeField] CanvasControllerXR canvasControllerXR;

    protected override void LocalExecuteImplementation(string data)
    {
        
        canvasControllerXR.HoldCanvas();
        
    }
}
