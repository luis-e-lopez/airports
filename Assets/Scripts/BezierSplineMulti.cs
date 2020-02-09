using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

public class BezierSplineMulti : MonoBehaviour, ISerializationCallbackReceiver {

    [SerializeField]
    public BezierSpline[] splines;

    [SerializeField]
    private bool connect;

    private int selectedPathIndex = -1;
    private int selectedPointIndex = -1;
    private int connectToPathIndex = -1;
    private int connectToPointIndex = -1;

    private Dictionary<string, List<string>> connections;

    private List<string>[] nodes;

    [SerializeField]
    private List<string> _keys = new List<string>();

    [SerializeField]
    private List<string> _values = new List<string>();

    [SerializeField]
    private string[] _nodes;

    public bool Connect
    {
        get
        {
            if (connectToPathIndex != -1 && selectedPointIndex != -1)
                return ConnectionAlreadyExists();
            return IsPointAlreadyInNode(selectedPathIndex + "-" + selectedPointIndex);
        }
        set
        {
            if (connections == null)
                connections = new Dictionary<string, List<string>>();

            string key = selectedPathIndex + "-" + selectedPointIndex;
            string otherPointKey = connectToPathIndex + "-" + connectToPointIndex;

            connect = value;
            if (value) // wants to connect points
            {
                //AddPointToConnections(key, otherPointKey);
                int nodeIndex = -1;
                if (IsPointAlreadyInNode(otherPointKey, out nodeIndex)) 
                {
                    AddPointsToNode(new List<string>() { key }, nodeIndex);
                } 
                else
                {
                    AddPointsToNode(new List<string>() { key, otherPointKey }, nodeIndex);
                }

                // move the point to the same place as the other point
                Vector3 cPoint = splines[connectToPathIndex].GetControlPoint(connectToPointIndex);
                splines[selectedPathIndex].SetControlPoint(selectedPointIndex, new Vector3(cPoint.x, cPoint.y, cPoint.z));
            }
            else // wants to disconnect points
            {
                //RemovePointFromConnections(key, otherPointKey);
                RemovePointFromNode(key);   

                // move the point down
                Vector3 cPoint = splines[selectedPathIndex].GetControlPoint(selectedPointIndex);
                splines[selectedPathIndex].SetControlPoint(selectedPointIndex, new Vector3(cPoint.x, cPoint.y - 0.5f, cPoint.z));
            }
        }
    }

    private bool ConnectionAlreadyExists()
    {
        if (nodes.Length == 0)
            return false;

        string key = selectedPathIndex + "-" + selectedPointIndex;
        string otherPointKey = connectToPathIndex + "-" + connectToPointIndex;
        int nodeIndex = -1;
        if (IsPointAlreadyInNode(key, out nodeIndex)) 
        {
            int otherNodeIndex = -1;
            if (IsPointAlreadyInNode(otherPointKey, out otherNodeIndex)) 
            {
                if (nodeIndex == otherNodeIndex)
                    return true;
            }
        }
        return false;
    }

    public bool IsPointAlreadyInNode(string pointKey, out int nodeIndex) 
    {
        nodeIndex = -1;

        if (nodes.Length == 0) 
            return false;

        for (int i = 0; i < nodes.Length; i++) 
        { 
            foreach (string key in nodes[i]) 
            {
                if (key.Equals(pointKey)) 
                {
                    nodeIndex = i;
                    return true;
                }
            }
        }
        return false;
    }

    public bool IsPointAlreadyInNode(string pointKey)
    {
        if (nodes.Length == 0)
            return false;

        for (int i = 0; i < nodes.Length; i++)
        {
            foreach (string key in nodes[i])
            {
                if (key.Equals(pointKey))
                {
                    return true;
                }
            }
        }
        return false;
    }

    public List<string> GetOtherPointsInNode(string pointKey)
    {

        List<string> otherPoints = new List<string>();
        foreach (List<string> keys in nodes) 
        { 
            if (keys.Contains(pointKey)) 
            { 
                foreach (string key in keys) 
                { 
                    if (!key.Equals(pointKey)) 
                    {
                        otherPoints.Add(key);
                    }
                }
                return otherPoints;
            }
        }
        return otherPoints;
    }

    public List<string> GetAllPointsInNode(int nodeIndex)
    {

        List<string> points = new List<string>();
        List<string> keys = nodes[nodeIndex];
        foreach (string key in keys)
        {
            points.Add(key);
        }
        return points;
    }

    public void AddPointsToNode(List<string> pointKeys, int nodeIndex) 
    {
        if (nodeIndex >= nodes.Length)
            return;

        if (nodeIndex == -1) 
        {
            Array.Resize(ref nodes, nodes.Length + 1);
            nodes[nodes.Length - 1] = pointKeys;
            return;
        }

        List<string> keys = nodes[nodeIndex];
        foreach (string pointKey in pointKeys) 
        { 
            if (!keys.Contains(pointKey)) 
            {
                keys.Add(pointKey);
            }
        }
    }

    public void RemovePointFromNode(string pointKey) 
    { 
        for (int i = 0; i < nodes.Length; i++) 
        {
            List<string> keys = nodes[i];
            if (keys.Contains(pointKey)) 
            {
                keys.Remove(pointKey);
                if (keys.Count <= 1)
                {
                    var temp = new List<List<string>>(nodes);
                    temp.RemoveAt(i);
                    nodes = temp.ToArray();
                }
                return;
            }
        }
    }

    private void AddPointToConnections(string key, string otherPointKey) 
    {
        List<string> connectedPoints = new List<string>();
        if (connections.TryGetValue(key, out connectedPoints))
        {
            foreach (string point in connectedPoints)
            {
                if (point.Equals(otherPointKey))
                {
                    return;
                }
            }
            //List<string> cPoints = connectedPoints.Select(p => p).ToList();
            connectedPoints.Add(otherPointKey);
            if (connections.TryGetValue(otherPointKey, out connectedPoints))
            {
                connectedPoints.Add(key);
                return;
            }
            connections.Add(otherPointKey, new List<string>() { key });
            return;
        }

        connections.Add(key, new List<string>() { otherPointKey });

        if (connections.TryGetValue(otherPointKey, out connectedPoints)) 
        {
            foreach (string point in connectedPoints)
            {
                if (point.Equals(key))
                {
                    return;
                }
            }
            connectedPoints.Add(key);
            return;
        }
        connections.Add(otherPointKey, new List<string>() { key });
    }

    private void RemovePointFromConnections(string key, string otherPointKey) 
    {
        List<string> connectedPoints = new List<string>();
        if (connections.TryGetValue(key, out connectedPoints))
        {
            foreach (string point in connectedPoints)
            {
                if (point.Equals(otherPointKey))
                {
                    connectedPoints.Remove(point);
                    if (connectedPoints.Count == 0) // Removes from Dictionary if List is empty. Not sure about this yet.
                        connections.Remove(key);
                    break;
                }
            }
        }
        if (connections.TryGetValue(otherPointKey, out connectedPoints))
        {
            foreach (string point in connectedPoints)
            {
                if (point.Equals(key))
                {
                    connectedPoints.Remove(point);
                    if (connectedPoints.Count == 0) // Removes from Dictionary if List is empty. Not sure about this yet.
                        connections.Remove(otherPointKey);
                    break;
                }
            }
        }
    }

    public void prepareForPossibleConnection(int pathIndex, int pointIndex, int toPathIndex, int toPointIndex) 
    {
        selectedPathIndex = pathIndex;
        selectedPointIndex = pointIndex;
        connectToPathIndex = toPathIndex;
        connectToPointIndex = toPointIndex;
    }

    //private bool ConnectionAlreadyExists() 
    //{
    //    if (connections.Count == 0)
    //        return false;

    //    string key = selectedPathIndex + "-" + selectedPointIndex;
    //    string otherPointKey = connectToPathIndex + "-" + connectToPointIndex;
    //    List<string> connectedPoints = new List<string>();
    //    //Debug.Log("Connected Points Length: " + connections.Count);
    //    if (connections.TryGetValue(key, out connectedPoints)) 
    //    {
    //        foreach (string point in connectedPoints)
    //        {
    //            if (point.Equals(otherPointKey))
    //            {
    //                return true;
    //            }
    //        }
    //    }
    //    return false;
    //}

    public bool HasConnections(int fromPath, int fromPoint, out List<string> connectedPoints) 
    {
        //if (connections.Count == 0)
            //return false;

        string key = fromPath + "-" + fromPoint;
        connectedPoints = new List<string>();
        if (connections.TryGetValue(key, out connectedPoints)) 
        {
            return true;
        }
        return false;
    }

    public Vector3 GetPointFromKey(string key) 
    {
        string[] indexes = key.Split('-');
        if (indexes.Length != 2)
            return new Vector3();

        try
        {
            int pathIndex = Int32.Parse(indexes[0]);
            int pointIndex = Int32.Parse(indexes[1]);

            return splines[pathIndex].GetControlPoint(pointIndex);
        }
        catch(FormatException e) 
        {
            Debug.Log("Exception: " + e.Message);
        }
        return new Vector3();
    }

    public float GetSplineLength(int index) 
    {
        BezierSpline spline = splines[index];
        float step = 0.01f;
        Vector3 prevPoint = spline.GetPoint(0f);
        float length = 0f;

        float curveLength = 0f;
        int nextControlPointIndex = 3;
        Vector3 nextControlPoint = spline.GetControlPoint(nextControlPointIndex);
        for (float i = 0f; i <= 1f; i += step) 
        {
            Vector3 nextPoint = spline.GetPoint(i);
            float sLength = Vector3.Distance(prevPoint, nextPoint);
            length += sLength;
            prevPoint = nextPoint;

            curveLength += sLength;
            float d = Vector3.Distance(nextPoint, nextControlPoint);
            if (d < 0.02f) 
            {
                Debug.Log("Distance from point " + (nextControlPointIndex - 3) + " to point " + nextControlPointIndex + " is " + curveLength);
                if (nextControlPointIndex + 3 < spline.ControlPointCount) 
                {
                    nextControlPointIndex += 3;
                    nextControlPoint = spline.GetControlPoint(nextControlPointIndex);
                    curveLength = 0f;
                }
            }
        }

        return length;
    }

    public float GetSplineLengthBetweenTwoControlPoints(int splineIndex, int point1Index, int point2Index) 
    {
        if (point1Index % 3 != 0 || point2Index % 3 != 0)
            return -1f;

        float length = 0f;
        float step = 0.01f;
        float progress = GetProgressAtControlPoint(splineIndex, point1Index);
        BezierSpline spline = splines[splineIndex];
        Vector3 prevPoint = spline.GetPoint(progress);
        Vector3 controlPoint = spline.GetControlPoint(point2Index);
        for (float i = progress; i <= 1f; i += step)
        {
            Vector3 nextPoint = spline.GetPoint(i);
            float sLength = Vector3.Distance(prevPoint, nextPoint);
            length += sLength;
            prevPoint = nextPoint;

            float d = Vector3.Distance(nextPoint, controlPoint);
            if (d < 0.02f)
            {
                return length;
            }
        }
        return length;
    }

    public float GetProgressAtControlPoint(int splineIndex, int pointIndex) 
    {
        if (pointIndex % 3 != 0)
            return -1f;

        int cPointCount = splines[splineIndex].ControlPointCount;
        return (float)pointIndex / ((float)cPointCount - 1);
    }

    public void PrintDictionary() 
    {
        if (connections.Count == 0)
            Debug.Log("No connections");

        foreach(string key in connections.Keys) 
        {
            Debug.Log("Dictionary entry key " + key + " has the following points connected:");
            List<string> connectedPoints = new List<string>();
            if (connections.TryGetValue(key, out connectedPoints)) 
            { 
                foreach (string point in connectedPoints) 
                {
                    Debug.Log("   -> " + point);
                }
            }
        }
    }

    public void PrintNodes() 
    {
        if (nodes.Length == 0)
            Debug.Log("No nodes");

        for (int i = 0; i < nodes.Length; i++) 
        {
            Debug.Log("Node " + i + ": " + String.Join(",", nodes[i]));
        }
    }

    public void ClearConnections() 
    {
        connections.Clear();
    }

    public void ClearNodes()
    {
        Array.Resize(ref nodes, 0);
    }

    public void Reset() 
    {
        splines = new BezierSpline[0];
        connections = new Dictionary<string, List<string>>();
        nodes = new List<string>[0];
    }

    public void OnBeforeSerialize()
    {
        _keys.Clear();
        _values.Clear();

        foreach (var kvp in connections)
        {
            _keys.Add(kvp.Key);
            _values.Add(String.Join(",", kvp.Value));
        }

        _nodes = new string[nodes.Length];

        for (int i = 0; i < nodes.Length; i++) 
        {
            _nodes[i] = String.Join(",", nodes[i]);
        }
    }

    public void OnAfterDeserialize()
    {
        connections = new Dictionary<string, List<string>>();
        for (int i = 0; i != Math.Min(_keys.Count, _values.Count); i++) 
        {
            List<string> vals = _values[i].Split(',').OfType<string>().ToList();
            connections.Add(_keys[i], vals);
        }

        nodes = new List<string>[_nodes.Length];

        for (int i = 0; i < _nodes.Length; i++) 
        { 
            nodes[i] = _nodes[i].Split(',').OfType<string>().ToList();
        }
    }
}