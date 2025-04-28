using System;
using System.Collections.Generic;
using Interfaces;
using Models;
using Presenters;
using UnityEngine;

namespace Managers
{
    public static class LTGUtility
    {
        public static float FindDistanceToSegment(Vector2 pt, Vector2 p1, Vector2 p2)
        {
            float dx = p2.x - p1.x;
            float dy = p2.y - p1.y;
            if (dx == 0f && dy == 0f)
            {
                dx = pt.x - p1.x;
                dy = pt.y - p1.y;
                return Mathf.Sqrt(dx * dx + dy * dy);
            }
            float t = ((pt.x - p1.x) * dx + (pt.y - p1.y) * dy) / (dx * dx + dy * dy);
            if (t < 0f)
            {
                dx = pt.x - p1.x;
                dy = pt.y - p1.y;
            }
            else if (t > 1f)
            {
                dx = pt.x - p2.x;
                dy = pt.y - p2.y;
            }
            else
            {
                float projX = p1.x + t * dx;
                float projY = p1.y + t * dy;
                dx = pt.x - projX;
                dy = pt.y - projY;
            }
            return Mathf.Sqrt(dx * dx + dy * dy);
        }

        public static float DistanceToConnection(Connection conn, Vector3 point, float maxDistance)
        {
            if(conn == null)
                Debug.Log("conn null");
            List<Vector2> linePoints = conn.line.points;
            int pointsCount = linePoints.Count;
            float minDist = Mathf.Infinity;
            for (int i = 1; i < pointsCount; i++)
            {
                float distance = FindDistanceToSegment(point, linePoints[i - 1], linePoints[i]);
                if (distance < minDist && distance <= maxDistance)
                {
                    minDist = distance;
                }
            }
            return minDist;
        }

        static bool PointIsOnSegment(Vector2 p, Vector2 q, Vector2 r)
        {
            return (q.x <= Mathf.Max(p.x, r.x) && q.x >= Mathf.Min(p.x, r.x) &&
                    q.y <= Mathf.Max(p.y, r.y) && q.y >= Mathf.Min(p.y, r.y));
        }

        static int LineOrientation(Vector2 p, Vector2 q, Vector2 r)
        {
            float val = (q.y - p.y) * (r.x - q.x) - (q.x - p.x) * (r.y - q.y);
            if (val == 0f) return 0;
            return (val > 0f) ? 1 : 2;
        }

        public static bool DoLinesIntersect(Vector2 p1, Vector2 q1, Vector2 p2, Vector2 q2)
        {
            int o1 = LineOrientation(p1, q1, p2);
            int o2 = LineOrientation(p1, q1, q2);
            int o3 = LineOrientation(p2, q2, p1);
            int o4 = LineOrientation(p2, q2, q1);
            if (o1 != o2 && o3 != o4)
                return true;
            if (o1 == 0 && PointIsOnSegment(p1, p2, q1)) return true;
            if (o2 == 0 && PointIsOnSegment(p1, q2, q1)) return true;
            if (o3 == 0 && PointIsOnSegment(p2, p1, q2)) return true;
            if (o4 == 0 && PointIsOnSegment(p2, q1, q2)) return true;
            return false;
        }

        public static bool DoConnectionsIntersect(ConnectionPresenter conn1, ConnectionPresenter conn2)
        {
            if (conn1 == conn2) return false;
            List<Vector2> conn1Points = conn1.Model.line.points;
            List<Vector2> conn2Points = conn2.Model.line.points;
            int conn1Count = conn1Points.Count;
            int conn2Count = conn2Points.Count;
            for (int i = 1; i < conn1Count; i++)
            {
                for (int j = 1; j < conn2Count; j++)
                {
                    if (DoLinesIntersect(conn1Points[i - 1], conn1Points[i], conn2Points[j - 1], conn2Points[j]))
                        return true;
                }
            }
            return false;
        }

        public static bool DoConnectionIntersectRect(ConnectionPresenter conn, RectTransform rt)
        {
            List<Vector2> connPoints = conn.Model.line.points;
            int connCount = connPoints.Count;
            for (int i = 1; i < connCount; i++)
            {
                Vector2 p1 = connPoints[i - 1];
                Vector2 q1 = connPoints[i];
                Vector3[] corners = new Vector3[4];
                rt.GetWorldCorners(corners);
                int intersectCount = 0;
                for (int j = 0; j < 4; j++)
                {
                    Vector2 p2 = corners[j];
                    Vector2 q2 = corners[(j + 1) % 4];
                    if (DoLinesIntersect(p1, q1, p2, q2))
                        intersectCount++;
                    if (intersectCount >= 2)
                        return true;
                }
            }
            return false;
        }

        public static Vector3 WorldToScreenPointInCanvas(Vector3 point, GraphManager graphManager)
        {
            Camera mainCamera = graphManager.MainCamera;
            RectTransform canvasRect = graphManager.CanvasRectTransform;
            Vector3 gmOffset = mainCamera.WorldToScreenPoint(graphManager.transform.position);
            Vector3 lrOffset = mainCamera.WorldToScreenPoint(graphManager.LineRenderer.transform.position);
            Vector3 screenPos = mainCamera.WorldToScreenPoint(point) - lrOffset + gmOffset;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, new Vector2(screenPos.x, screenPos.y), mainCamera, out Vector2 anchoredPos);
            return anchoredPos;
        }

        public static Vector3 WorldToScreenPoint(Vector3 point, GraphManager graphManager)
        {
            Camera mainCamera = graphManager.MainCamera;
            Vector3 lrOffset = mainCamera.WorldToScreenPoint(graphManager.LineRenderer.transform.position);
            return mainCamera.WorldToScreenPoint(point) - lrOffset;
        }

        public static Vector3[] WorldToScreenPointsForRenderMode(GraphManager graphManager, Vector3[] points)
        {
            if (graphManager.CanvasRenderMode == RenderMode.ScreenSpaceOverlay)
                return points;
            Vector3[] newPoints = new Vector3[points.Length];
            if (graphManager.CanvasRenderMode == RenderMode.ScreenSpaceCamera)
            {
                for (int i = 0; i < points.Length; i++)
                    newPoints[i] = WorldToScreenPoint(points[i], graphManager);
            }
            else if (graphManager.CanvasRenderMode == RenderMode.WorldSpace)
            {
                for (int i = 0; i < points.Length; i++)
                    newPoints[i] = WorldToScreenPointInCanvas(points[i], graphManager);
            }
            return newPoints;
        }

        public static Vector3 ConvertPointsToRenderMode(GraphManager graphManager, Vector3 point)
        {
            if (graphManager.CanvasRenderMode == RenderMode.ScreenSpaceOverlay)
                return point;
            if (graphManager.CanvasRenderMode == RenderMode.ScreenSpaceCamera)
                return WorldToScreenPoint(point, graphManager);
            if (graphManager.CanvasRenderMode == RenderMode.WorldSpace)
                return WorldToScreenPointInCanvas(point, graphManager);
            return point;
        }

        public static Vector3 ScreenToWorldPoint(Vector3 point, GraphManager graphManager)
        {
            Camera mainCamera = graphManager.MainCamera;
            Vector3 lrOffset = mainCamera.WorldToScreenPoint(graphManager.LineRenderer.transform.position);
            return mainCamera.ScreenToWorldPoint(point + lrOffset);
        }

        public static Vector3 ScreenToWorldPointScale(Vector3 point, GraphManager graphManager)
        {
            Camera mainCamera = graphManager.MainCamera;
            RectTransform canvasRect = graphManager.CanvasRectTransform;
            Vector3 gmOffset = graphManager.LineRenderer.transform.position;
            Vector3 rotated = RotatePointAroundPoint(point, graphManager.transform.position, graphManager.transform.eulerAngles.z);
            return rotated * canvasRect.localScale.x + gmOffset;
        }

        static Vector3 RotatePointAroundPoint(Vector3 point1, Vector3 point2, float angle)
        {
            angle *= Mathf.Deg2Rad;
            float cos = Mathf.Cos(angle);
            float sin = Mathf.Sin(angle);
            float x = cos * (point1.x - point2.x) - sin * (point1.y - point2.y) + point2.x;
            float y = sin * (point1.x - point2.x) + cos * (point1.y - point2.y) + point2.y;
            return new Vector3(x, y);
        }

        public static Vector3[] ScreenToWorldPointsForRenderMode(GraphManager graphManager, Vector3[] points)
        {
            if (graphManager.CanvasRenderMode == RenderMode.ScreenSpaceOverlay)
                return points;
            Vector3[] newPoints = new Vector3[points.Length];
            if (graphManager.CanvasRenderMode == RenderMode.ScreenSpaceCamera)
            {
                for (int i = 0; i < points.Length; i++)
                    newPoints[i] = ScreenToWorldPoint(points[i], graphManager);
            }
            else if (graphManager.CanvasRenderMode == RenderMode.WorldSpace)
            {
                for (int i = 0; i < points.Length; i++)
                    newPoints[i] = ScreenToWorldPointScale(points[i], graphManager);
            }
            return newPoints;
        }

        public static Vector3 ScreenToWorldPointsForRenderMode(GraphManager graphManager, Vector3 point)
        {
            if (graphManager.CanvasRenderMode == RenderMode.ScreenSpaceOverlay)
                return point;
            if (graphManager.CanvasRenderMode == RenderMode.ScreenSpaceCamera)
                return ScreenToWorldPoint(point, graphManager);
            if (graphManager.CanvasRenderMode == RenderMode.WorldSpace)
                return ScreenToWorldPointScale(point, graphManager);
            return point;
        }

        public static int SortByPriority(IElement o1, IElement o2)
        {
            return o2.Priority.CompareTo(o1.Priority);
        }

        public static T Clone<T>(this T source)
        {
            if (ReferenceEquals(source, null))
                return default;
            return JsonUtility.FromJson<T>(JsonUtility.ToJson(source));
        }

        public static float ConvertScale(float OldValue, float OldMin, float OldMax, float NewMin, float NewMax)
        {
            return ((OldValue - OldMin) * (NewMax - NewMin)) / (OldMax - OldMin) + NewMin;
        }

        public static string GenerateSID()
        {
            return Guid.NewGuid().ToString();
        }

        public static U TryGetValue<T, U>(this Dictionary<T, U> dictionary, T key)
        {
            dictionary.TryGetValue(key, out U result);
            return result;
        }
    }
}
