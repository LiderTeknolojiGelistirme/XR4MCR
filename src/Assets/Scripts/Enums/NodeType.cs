namespace Enums
{
    public enum NodeType
    {
        Start,
        Finish,
        TouchNode,
        GrabNode,
        WaitForNextNode,
        LookNode,
        LogicalOR,
        LogicalAND,
        // Action Node Tipleri
        PlaySoundAction,
        ChangeMaterialAction,
        ChangePositionAction,
        ChangeRotationAction,
        ChangeScaleAction,
        ToggleObjectAction,
        PlayAnimationAction,
        DescriptionActionNode,
        RobotAnimationAction
    }
}