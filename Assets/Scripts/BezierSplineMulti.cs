using UnityEngine;
using System.Collections.Generic;
using System;

public class BezierSplineMulti : MonoBehaviour {

    [SerializeField]
    public BezierSpline[] splines;

    [SerializeField]
    private bool connect;

    private int selectedPathIndex = -1;
    private int selectedPointIndex = -1;
    private int connectToPathIndex = -1;
    private int connectToPointIndex = -1;

    private Dictionary<string, List<string>> connections = new Dictionary<string, List<string>>();

    public bool Connect
    {
        get
        {
            return ConnectionAlreadyExists();
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
                AddPointToConnections(key, otherPointKey);

                // move the point to the same place as the other point
                Vector3 cPoint = splines[connectToPathIndex].GetControlPoint(connectToPointIndex);
                splines[selectedPathIndex].SetControlPoint(selectedPointIndex, new Vector3(cPoint.x, cPoint.y, cPoint.z));
            }
            else // wants to disconnect points
            {
                RemovePointFromConnections(key, otherPointKey);

                // move the point down
                Vector3 cPoint = splines[selectedPathIndex].GetControlPoint(selectedPointIndex);
                splines[selectedPathIndex].SetControlPoint(selectedPointIndex, new Vector3(cPoint.x, cPoint.y - 0.5f, cPoint.z));
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

    private bool ConnectionAlreadyExists() 
    {
        string key = selectedPathIndex + "-" + selectedPointIndex;
        string otherPointKey = connectToPathIndex + "-" + connectToPointIndex;
        List<string> connectedPoints = new List<string>();
        if (connections.TryGetValue(key, out connectedPoints)) 
        {
            foreach (string point in connectedPoints)
            {
                if (point.Equals(otherPointKey))
                {
                    return true;
                }
            }
        }
        return false;
    }

    public bool HasConnections(int fromPath, int fromPoint, out List<string> connectedPoints) 
    {
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
            return new Vector3(0f, 0f, 0f);

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
        return new Vector3(0f, 0f, 0f);
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

        //float progress = 0f;
        //Vector3 progressPoint = spline.GetPoint(0f);
        //for (int i = 0; i <= spline.ControlPointCount; i += 3)
        //{
        //    Vector3 nextPoint = spline.GetPoint(i);
        //    float curveLength = 0f;
        //    float d = Vector3.Distance(progressPoint, nextPoint);
        //    while(d > 0.02) 
        //    {
        //        curveLength += d;
        //        progress += step;
        //        progressPoint = spline.GetPoint(progress);
        //        d = Vector3.Distance(progressPoint, nextPoint);
        //    }

        //}

        return length;
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

    public void ClearConnections() 
    {
        connections.Clear();
    }
}