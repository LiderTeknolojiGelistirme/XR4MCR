using System;
using System.Collections.Generic;
using Enums;

namespace Models
{
    [Serializable]
    public class SaveFile
    {
        public List<BaseNode> Nodes { get; set; } = new List<BaseNode>();
        public List<ConnectionInfo> Connections { get; set; } = new List<ConnectionInfo>();
        public List<SceneObjectInfo> SceneObjects { get; set; } = new List<SceneObjectInfo>();
    }
    
    [Serializable]
    public class ConnectionInfo
    {
        public string ID { get; set; }
        public string SourcePortID { get; set; }
        public string TargetPortID { get; set; }
        
        public ConnectionInfo() { } // XML serializasyon için boş constructor
        
        public ConnectionInfo(string id, string sourcePortID, string targetPortID)
        {
            ID = id;
            SourcePortID = sourcePortID;
            TargetPortID = targetPortID;
        }
    }
    
    [Serializable]
    public class SceneObjectInfo
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public ObjectType ObjectType { get; set; }
        
        // Transform bilgileri
        public float PosX { get; set; }
        public float PosY { get; set; }
        public float PosZ { get; set; }
        
        public float RotX { get; set; }
        public float RotY { get; set; }
        public float RotZ { get; set; }
        
        public float ScaleX { get; set; }
        public float ScaleY { get; set; }
        public float ScaleZ { get; set; }
    }
}





