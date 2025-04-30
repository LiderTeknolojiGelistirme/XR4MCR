using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI; // GraphicRaycaster için bu gerekli
using System.Collections.Generic;
using Presenters;
using Zenject;
using IClickable = Interfaces.IClickable;
using IElement = Interfaces.IElement;
using Models;
using System.Linq;


// todo: ilk burasi bitecek raycast su anki sisteme uygun hale getir static yapidan cikar ve injection yap' sonra input manager
namespace Managers
{
    public class Raycaster
    {
        public PointerEventData PointerEventData;

        private SystemManager _systemManager;
        private GraphManager _graphManager;

        [Inject]
        public void Construct(SystemManager systemManager, GraphManager graphManager)
        {
            Debug.Log("ENTER: Raycaster Construct");
            _systemManager = systemManager;
            _graphManager = graphManager;
        }


        public List<RaycastResult> RaycastUIAll(Vector3 position)
        {
            if (PointerEventData == null)
                PointerEventData = new PointerEventData(EventSystem.current);

            PointerEventData.position = position;
            List<RaycastResult> resultsLocal = new List<RaycastResult>();
            List<RaycastResult> results = new List<RaycastResult>();

            List<GraphicRaycaster> _raycasterList = new List<GraphicRaycaster>();

            if (_systemManager.CacheRaycasters)
            {
                _raycasterList = _systemManager.raycasterList;
            }
            else
            {
                _raycasterList.Clear();
                _raycasterList.AddRange(GameObject.FindObjectsOfType<GraphicRaycaster>());
            }

            foreach (GraphicRaycaster gr in _raycasterList)
            {
                gr.Raycast(PointerEventData, resultsLocal);
                results.AddRange(resultsLocal);
            }

            return results;
        }

        public List<IElement> OrderedElementsAtPosition(GraphManager graphManager, Vector3 screenPosition, Vector3 canvasPosition)
        {
            // Debug.Log("OrderedElementsAtPosition");
            IElement element = null;
            List<IElement> orderedElements = new List<IElement>();

            List<RaycastResult> results = RaycastUIAll(screenPosition);
            foreach (RaycastResult result in results)
            {
                // Debug.Log("Raycast hit: " + result.gameObject.name);
                element = result.gameObject.GetComponentInParent<IElement>();

                if (element != null)
                {
                    if (!(element is IClickable) || !(element as IClickable).DisableClick)
                        orderedElements.Add(element);
                }
            }

            Vector3 convertedPosition = LTGUtility.ConvertPointsToRenderMode(graphManager, canvasPosition);

            element = FindClosestConnectionToPosition(convertedPosition, graphManager.ConnectionDetectionDistance);

            if (element != null)
                if (!(element as IClickable).DisableClick)
                    orderedElements.Add(element);

            orderedElements.Sort(LTGUtility.SortByPriority);

            return orderedElements;
        }

        public ConnectionPresenter FindClosestConnectionToPosition(Vector3 position, float maxDistance)
        {
            float minDist = Mathf.Infinity;
            ConnectionPresenter closestConnectionPresenter = null;

            foreach (var item in _graphManager.ConnectionPresenters)
            {
                float distance = LTGUtility.DistanceToConnection(item.Model, position, maxDistance);
                if (distance < minDist)
                {
                    closestConnectionPresenter = item;
                    minDist = distance;
                }
            }

            return closestConnectionPresenter;
        }

    }
}
