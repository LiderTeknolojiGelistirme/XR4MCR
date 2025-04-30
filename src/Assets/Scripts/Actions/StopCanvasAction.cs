using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Viroo.Interactions;
using Virtualware.Localization;

public class StopCanvasAction : BroadcastObjectAction
{
    [SerializeField] CanvasControllerXR canvasControllerXR;

    protected override void LocalExecuteImplementation(string data)
    {

        canvasControllerXR.ReleaseCanvas();

    }
}
