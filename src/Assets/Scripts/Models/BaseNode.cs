using System;
using System.Collections.Generic;
using Presenters;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

namespace Models
{
    public abstract class BaseNode
    {
        public string ID { get; private set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public Color Color { get; set; }
        public Color HeaderColor { get; set; }
        public Color DefaultColor { get; set; }
        public bool EnableSelect { get; set; }
        
        public bool EnableSelfConnection = true;
        public List<PortPresenter> PortPresenters { get; private set; } = new List<PortPresenter>();

        #region Scenario Members
        
        public bool IsActive, IsStarted, IsCompleted;
        
        
        #endregion
        
        

        public BaseNode(string id, string title, Color color, bool enableSelect)
        {
            ID = id;
            Title = title;
            Color = color;
            DefaultColor = color;
            HeaderColor = new Color(0.2f, 0.2f, 0.2f); // Koyu gri header
            EnableSelect = enableSelect;
        }

        public void AddPort(PortPresenter port)
        {
            if (!PortPresenters.Contains(port))
            {
                PortPresenters.Add(port);
            }
        }

        public void RemovePort(PortPresenter port)
        {
            if (PortPresenters.Contains(port))
            {
                PortPresenters.Remove(port);
            }
        }
    }
}