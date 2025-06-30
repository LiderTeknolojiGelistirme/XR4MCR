using System;
using Enums;
using Unity.Mathematics;
using UnityEngine;

namespace Models
{
    [Serializable]
    public class ObjectModel
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public ObjectType ObjectType { get; set; }

        // Pozisyon
        public float PosX { get; set; }
        public float PosY { get; set; }
        public float PosZ { get; set; }

        // Dönüş (Euler açıları)
        public float RotX { get; set; }
        public float RotY { get; set; }
        public float RotZ { get; set; }

        // Ölçek
        public float ScaleX { get; set; }
        public float ScaleY { get; set; }
        public float ScaleZ { get; set; }

        public ObjectModel() { }

        public ObjectModel(string id, string name, ObjectType objectType,
            float posX, float posY, float posZ,
            float rotX, float rotY, float rotZ,
            float scaleX, float scaleY, float scaleZ)
        {
            ID = id;
            Name = name;
            ObjectType = objectType;
            PosX = posX;
            PosY = posY;
            PosZ = posZ;
            RotX = rotX;
            RotY = rotY;
            RotZ = rotZ;
            ScaleX = scaleX;
            ScaleY = scaleY;
            ScaleZ = scaleZ;
        }
    }

}
