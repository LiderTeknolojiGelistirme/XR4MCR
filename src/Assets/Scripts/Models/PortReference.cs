using System;
using UnityEditor;

namespace Models
{
    // Port referansı için immutable struct
    public readonly struct PortReference : IEquatable<PortReference>
    {
        public readonly string NodeId { get; }
        public readonly string PortId { get; }

        public PortReference(string nodeId, string portId)
        {
            NodeId = nodeId;
            PortId = portId;
        }

        // Eşitlik karşılaştırması için gerekli metodlar
        public bool Equals(PortReference other)
        {
            return NodeId == other.NodeId && PortId == other.PortId;
        }

        public override bool Equals(object obj)
        {
            return obj is PortReference other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(NodeId, PortId);
        }

        public static bool operator ==(PortReference left, PortReference right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(PortReference left, PortReference right)
        {
            return !left.Equals(right);
        }

        // Factory method ekle
        public static PortReference Create(Port port)
        {
            if (port?.ParentBaseNode == null)
                throw new InvalidOperationException("Invalid port or missing parent node");
            
            return new PortReference(port.ParentBaseNode.ID, port.ID);
        }

        // ToString override ekle
        public override string ToString() => $"Node:{NodeId}/Port:{PortId}";
    }
} 