using CustomGraphics;
using Managers;
using Presenters;
using UnityEngine;
using System;
using JetBrains.Annotations;

namespace Models
{
    [System.Serializable]
    public class Connection
    {
        public enum CurveStyleType
        {
            Spline,
            Z_Shape,
            Soft_Z_Shape,
            Line
        }
        
        private CurveStyleType curveStyle = CurveStyleType.Soft_Z_Shape;
        public CurveStyleType CurveStyle { get => curveStyle; set => curveStyle = value; }

        public string ID { get; }
        public PortPresenter SourcePort { get; }
        [CanBeNull] public PortPresenter TargetPort { get; private set; }  // nullable

        public GraphManager graphManager;

        public int Priority => 0;

        [SerializeField] bool _enableDrag = true;
        public bool EnableDrag { get => _enableDrag; set => _enableDrag = value; }
        [SerializeField] bool _enableHover = true;
        public bool EnableHover { get => _enableHover; set => _enableHover = value; }
        [SerializeField] bool _enableSelect = true;
        public bool EnableSelect { get => _enableSelect; set => _enableSelect = value; }
        [SerializeField] bool _disableClick = false;
        public bool DisableClick { get => _disableClick; set => _disableClick = value; }

        public ConnectionLabel label;

        public Line line;

        public bool IsPreview => TargetPort == null;

        public Connection(PortPresenter sourcePort, PortPresenter targetPort = null)
        {
            ID = Guid.NewGuid().ToString();
            SourcePort = sourcePort;
            TargetPort = targetPort;
        }

        public void SetTargetPort(PortPresenter target)
        {
            if (IsPreview)
                TargetPort = target;
        }
    }
}