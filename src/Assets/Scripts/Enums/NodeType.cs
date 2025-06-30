namespace Enums
{
    public enum NodeType
    {
        Ghost,
        Start,
        Finish,
        TouchNode,
        GrabNode,
        WaitForNextNode,
        LookNode,
        LogicalOR,
        LogicalAND,
        ToolTouchNode,
        // Action Node Tipleri
        PlaySoundAction,
        ChangeMaterialAction,
        ChangePositionAction,
        ChangeRotationAction,
        ChangeScaleAction,
        ToggleObjectAction,
        PlayAnimationAction,
        DescriptionActionNode,
        RobotAnimationAction,
        WorldDescriptionActionNode,
        VFXActionNode,
        HighlightObjectActionNode
    }
}