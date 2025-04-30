using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class NodeGraphData
{
    public List<NodeData> Nodes;
    public List<ConnectionData> Connections;
}

[Serializable]
public class NodeData
{
    public string ID;
    public string Type;
    public Vector2 Position;
    public List<PortData> Ports;
}

[Serializable]
public class PortData
{
    public string ID;
    public string Type;
}

[Serializable]
public class ConnectionData
{
    public string SourcePortID;
    public string TargetPortID;
}
