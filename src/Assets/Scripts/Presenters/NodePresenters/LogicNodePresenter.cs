using System.Collections.Generic;
using UnityEngine;
using Models;
using Presenters;
using Interfaces;
using UnityEngine.UI;
using System.Linq;
using NodeSystem;
using Enums;
using Unity.VisualScripting;

namespace Presenters.NodePresenters
{
    public class LogicNodePresenter : BaseNodePresenter
    {
        public LogicNodeType logicNodeType;

        

        public override void ActivateNode()
        {
            base.ActivateNode();
        }

        public override void StartNode()
        {
            Debug.Log("Start LogicNodePresenter");
            base.StartNode();
        }

        public override void CompleteNode()
        {
            Debug.Log("Complete LogicNodePresenter");
            base.CompleteNode();
        }
        
        public override void Play()
        {
            if (CheckLogic(logicNodeType))
            {
                CompleteNode();
            }
        }

        bool CheckLogic(LogicNodeType logic)
        {
            var inputPorts = Ports.Where(p => p.Polarity == NodeSystem.PolarityType.Input).ToList();
            switch (logic)
            {
                case LogicNodeType.OR:
                    foreach (PortPresenter portPresenter in inputPorts)
                    {
                        foreach (ConnectionPresenter connectionPresenter in portPresenter.ConnectionPresenters)
                        {
                            if (connectionPresenter.Model.SourcePort != null && connectionPresenter.Model.SourcePort
                                    .Model.baseNode.Model.IsCompleted)
                            {
                                return true;
                            }
                        }
                    }

                    break;
                case LogicNodeType.AND:
                    foreach (PortPresenter portPresenter in inputPorts)
                    {
                        foreach (ConnectionPresenter connectionPresenter in portPresenter.ConnectionPresenters)
                        {
                            if (connectionPresenter.Model.SourcePort != null && !connectionPresenter.Model.SourcePort
                                    .Model.baseNode.Model.IsCompleted)
                            {
                                return false;
                            }
                        }

                        return true;
                    }

                    break;
            }

            return false;
        }
    }
}