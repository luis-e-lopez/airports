using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System;

[CustomEditor(typeof(BezierSplineMulti))]
public class BezierSplineMultiInspector : Editor
{
    SerializedProperty m_splines;

    private BezierSplineMulti bezierSplineMulti;
    private Quaternion handleRotation;
    private int selectedPointIndex = -1;
    private int selectedSplineIndex = -1;
    private int connectToPointIndex = -1;
    private int connectToSplineIndex = -1;
    private bool canConnect;
    private bool canSetAsNode;
    private bool canShowSetAsNodeToggle;

    private const float handleSize = 0.06f;
    private const float pickSize = 0.08f;
    private const float connectionDistance = 1.0f;
    private const float dotsSize = 2.0f;

    private static Color[] modeColors = {
        Color.white,
        Color.yellow,
        Color.cyan
    };

    private void OnEnable()
    {
        m_splines = serializedObject.FindProperty("splines");
    }

    public override void OnInspectorGUI() 
    {
        bezierSplineMulti = target as BezierSplineMulti;
        EditorGUILayout.PropertyField(m_splines, true);

        if (GUILayout.Button("Clear connections"))
        {
            bezierSplineMulti.ClearConnections();
        }

        if (GUILayout.Button("Print Connections"))
        {
            bezierSplineMulti.PrintDictionary();
        }

        if (GUILayout.Button("Print Nodes"))
        {
            bezierSplineMulti.PrintNodes();
        }

        if (GUILayout.Button("Calculate Lengths"))
        {
            for (int i = 0; i < bezierSplineMulti.splines.Length; i++)  
            {
                Debug.Log("Spline " + i + " length: " + bezierSplineMulti.GetSplineLength(i));
            }
        }

        if (GUILayout.Button("Print Progress on Points"))
        {
            for (int i = 0; i < bezierSplineMulti.splines.Length; i++)
            {
                for (int j = 3; j < bezierSplineMulti.splines[i].ControlPointCount; j += 3) 
                {
                    Debug.Log("Spline " + i + " Point " + j + " Progress " + bezierSplineMulti.GetProgressAtControlPoint(i, j));
                }
                Debug.Log("Spline " + i + " Count " + bezierSplineMulti.splines[i].ControlPointCount);
            }
        }

        if (GUILayout.Button("Print length between two points"))
        {
            int splineIndex = 4;
            int point1Index = 0;
            int point2Index = 3;
            float length = bezierSplineMulti.GetSplineLengthBetweenTwoControlPoints(splineIndex, point1Index, point2Index);
            Debug.Log("Spline " + splineIndex + " Point1 " + point1Index + " Point2 " + point2Index + " Length " + length);
        }

        if (canConnect) 
        {
            int nodeIndex = -1;
            if (bezierSplineMulti.IsPointAlreadyInNode(selectedSplineIndex + "-" + selectedPointIndex, out nodeIndex)) 
            {
                ShowNodeConnections(nodeIndex);
            } 
            else 
            {
                bezierSplineMulti.prepareForPossibleConnection(selectedSplineIndex, selectedPointIndex, connectToSplineIndex, connectToPointIndex);
                ShowConnectionToggle();
            }
        }

        if (canShowSetAsNodeToggle) 
        {
            ShowSetAsNodeToggle();
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void ShowNodeConnections(int nodeIndex) 
    {
        List<string> points = bezierSplineMulti.GetAllPointsInNode(nodeIndex);
        foreach (string key in points) 
        {
            string[] indexes = key.Split('-');
            int splineIndex = Int32.Parse(indexes[0]);
            int pointIndex = Int32.Parse(indexes[1]);

            bezierSplineMulti.prepareForPossibleConnection(splineIndex, pointIndex, -1, -1);
            EditorGUI.BeginChangeCheck();
            bool connect = EditorGUILayout.Toggle("Connected To Node", bezierSplineMulti.Connect);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(bezierSplineMulti, "Toggle Connect " + key);
                EditorUtility.SetDirty(bezierSplineMulti);
                bezierSplineMulti.Connect = connect;
            }

            EditorGUI.BeginChangeCheck();
            Vector3 point = EditorGUILayout.Vector3Field("Point(" + key + ") In Node(" + nodeIndex + ")", bezierSplineMulti.splines[splineIndex].GetControlPoint(pointIndex));
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(bezierSplineMulti, "Point " + pointIndex + ", Spline " + splineIndex);
                EditorUtility.SetDirty(bezierSplineMulti);
                //bezierSplineMulti.splines[selectedSplineIndex].SetControlPoint(selectedPointIndex, fromPoint);
            }
        }

    }

    private void ShowConnectionToggle() //(Vector3 fromPoint, Vector3 toPoint) 
    {
        EditorGUI.BeginChangeCheck();
        bool connect = EditorGUILayout.Toggle("Connect", bezierSplineMulti.Connect);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(bezierSplineMulti, "Toggle Connect");
            EditorUtility.SetDirty(bezierSplineMulti);
            bezierSplineMulti.Connect = connect;
        }

        EditorGUI.BeginChangeCheck();
        Vector3 fromPoint = EditorGUILayout.Vector3Field("From Point", bezierSplineMulti.splines[selectedSplineIndex].GetControlPoint(selectedPointIndex));
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(bezierSplineMulti, "Move From Point");
            EditorUtility.SetDirty(bezierSplineMulti);
            //bezierSplineMulti.splines[selectedSplineIndex].SetControlPoint(selectedPointIndex, fromPoint);
        }

        EditorGUI.BeginChangeCheck();
        Vector3 toPoint = EditorGUILayout.Vector3Field("To Point", bezierSplineMulti.splines[connectToSplineIndex].GetControlPoint(connectToPointIndex));
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(bezierSplineMulti, "Move To Point");
            EditorUtility.SetDirty(bezierSplineMulti);
            //bezierSplineMulti.splines[connectToSplineIndex].SetControlPoint(connectToPointIndex, fromPoint);
        }
    }

    private void ShowSetAsNodeToggle() 
    {
        EditorGUI.BeginChangeCheck();
        bool setAsNode = EditorGUILayout.Toggle("Set As Node", bezierSplineMulti.SetAsNode);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(bezierSplineMulti, "Set As Node");
            EditorUtility.SetDirty(bezierSplineMulti);
            bezierSplineMulti.SetAsNode = setAsNode;
        }
    }

    private void OnSceneGUI()
    {
        bezierSplineMulti = target as BezierSplineMulti;
        List<string> highlightedPath = bezierSplineMulti.GetHighlightedPath();
        for (int i = 0; i <bezierSplineMulti.splines.Length; i++) 
        {
            BezierSpline spline = bezierSplineMulti.splines[i];

            if (spline == null)
                continue;

            if (spline.ControlPointCount == 0)
                continue;

            handleRotation = Tools.pivotRotation == PivotRotation.Local ? spline.transform.rotation : Quaternion.identity;

            Vector3 p0 = ShowPoint(0, i);
            int p0index = 0;
            for (int j = 1; j < spline.ControlPointCount; j += 3)
            {
                Vector3 p1 = ShowPoint(j, i);//spline.transform.TransformPoint(spline.GetControlPoint(j));
                Vector3 p2 = ShowPoint(j + 1, i);//spline.transform.TransformPoint(spline.GetControlPoint(j + 1));
                Vector3 p3 = ShowPoint(j + 2, i);

                Handles.color = Color.white;
                Handles.DrawLine(p0, p1);
                Handles.DrawLine(p2, p3);

                Color bezierColor = Color.green;
                if (highlightedPath != null && highlightedPath.Count > 0) 
                {
                    if (highlightedPath.Contains(i + "-" + p0index + "," + i + "-" + (j + 2)))
                        bezierColor = Color.magenta;
                }
                Handles.DrawBezier(p0, p3, p1, p2, bezierColor, null, 5f);
                p0 = p3;
                p0index = j + 2;
            }
        }

        canConnect = false;
        if (selectedPointIndex != -1 && selectedSplineIndex != -1 && selectedPointIndex % 3 == 0) 
        {
            ShowPossibleConnection(selectedPointIndex, selectedSplineIndex);
        }
        ShowNodesLabels();
        Repaint();
    }

    private void ShowNodesLabels() 
    {
        Vector3[] np = bezierSplineMulti.GetNodesPositions();

        for (int i = 0; i < np.Length; i++) 
        {
            Handles.Label(new Vector3(np[i].x - .5f, np[i].y - .3f, np[i].z), "Node " + i);
        }
    }

    private Vector3 ShowPoint(int pointIndex, int splineIndex)
    {
        BezierSpline spline = bezierSplineMulti.splines[splineIndex];
        Vector3 point = spline.transform.TransformPoint(spline.GetControlPoint(pointIndex));
        float size = HandleUtility.GetHandleSize(point);
        if (pointIndex == 0)
        {
            size *= 2f;
        }
        Handles.color = modeColors[(int)spline.GetControlPointMode(pointIndex)];
        if (Handles.Button(point, handleRotation, size * handleSize, size * pickSize, Handles.DotHandleCap))
        {
            selectedPointIndex = pointIndex;
            selectedSplineIndex = splineIndex;
            canShowSetAsNodeToggle = pointIndex % 3 == 0 && !bezierSplineMulti.IsPointAlreadyInNode(splineIndex + "-" + pointIndex)
                           || bezierSplineMulti.IsSinglePointInNode(splineIndex + "-" + pointIndex);
            if (canShowSetAsNodeToggle)
                bezierSplineMulti.prepareForPossibleConnection(splineIndex, pointIndex, -1, -1);
            //Debug.Log("Selected spline: " + selectedSplineIndex + ", selected point: " + selectedPointIndex + ", mod: " + (selectedPointIndex % 3));

            Repaint();
        }
        if (selectedPointIndex == pointIndex && selectedSplineIndex == splineIndex)
        {
            EditorGUI.BeginChangeCheck();
            point = Handles.DoPositionHandle(point, handleRotation);
            //Handles.Label(new Vector3(point.x - .2f, point.y - .2f, point.z), "Selected spline: " + selectedSplineIndex + ", selected point: " + selectedPointIndex);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(spline, "Move Point");
                EditorUtility.SetDirty(spline);
                Vector3 localSpacePoint = spline.transform.InverseTransformPoint(point);
                spline.SetControlPoint(pointIndex, localSpacePoint);

                // move other connected points
                List<string> connectedPointsKeys = bezierSplineMulti.GetOtherPointsInNode(selectedSplineIndex + "-" + selectedPointIndex);
                //if (bezierSplineMulti.HasConnections(selectedSplineIndex, selectedPointIndex, out connectedPointsKeys)) 
                //Debug.Log("Key " + selectedSplineIndex + "-" + selectedPointIndex + ", other: " + connectedPointsKeys);
                if (connectedPointsKeys.Count > 0)
                {
                    float offset = 0f;
                    foreach (string key in connectedPointsKeys) 
                    {
                        string[] indexes = key.Split('-');
                        int connectedSplineIndex = Int32.Parse(indexes[0]);
                        int connectedPointIndex = Int32.Parse(indexes[1]);

                        Vector3 connectedPoint = new Vector3(localSpacePoint.x + offset, localSpacePoint.y + offset, 0f);
                        bezierSplineMulti.splines[connectedSplineIndex].SetControlPoint(connectedPointIndex, connectedPoint);

                        //offset += 0.5f;
                    }
                }

            }
        }

        return point;
    }

    private void ShowPossibleConnection(int pointIndex, int splineIndex) 
    {
        int indexSplineFrom = splineIndex;
        int indexPointFrom = pointIndex;
        int indexSplineTo = -1;
        int indexPointTo = -1;
        canConnect = false;

        Vector3 pointFrom = bezierSplineMulti.splines[indexSplineFrom].GetControlPoint(indexPointFrom);

        for (int i = 0; i < bezierSplineMulti.splines.Length; i++) 
        {
            if (i == splineIndex) // should not connect to same spline
                continue;
                
            BezierSpline spline = bezierSplineMulti.splines[i];
            if (Vector3.Distance(pointFrom, spline.GetControlPoint(0)) < connectionDistance) 
            {
                if (indexPointTo != -1 && indexSplineTo != -1)
                { 
                    if (Vector3.Distance(pointFrom, spline.GetControlPoint(0)) < Vector3.Distance(pointFrom, bezierSplineMulti.splines[indexSplineTo].GetControlPoint(indexPointTo))) 
                    {
                        indexSplineTo = i;
                        indexPointTo = 0;
                    }
                }
                else 
                {
                    indexSplineTo = i;
                    indexPointTo = 0;
                }
            }
            for (int j = 1; j < spline.ControlPointCount; j += 3) 
            { 
                if (Vector3.Distance(pointFrom, spline.GetControlPoint(j + 2)) < connectionDistance) 
                {
                    if (indexPointTo != -1 && indexSplineTo != -1) 
                    {
                        if (Vector3.Distance(pointFrom, spline.GetControlPoint(j + 2)) < Vector3.Distance(pointFrom, bezierSplineMulti.splines[indexSplineTo].GetControlPoint(indexPointTo)))
                        {
                            indexSplineTo = i;
                            indexPointTo = j + 2;
                        }
                    }
                    else 
                    {
                        indexSplineTo = i;
                        indexPointTo = j + 2;
                    }
                }
            }
        }

        string key = splineIndex + "-" + pointIndex;
        if (indexPointTo != -1 && indexSplineTo != -1) // && !bezierSplineMulti.IsPointAlreadyInNode(key)) 
        {
            Handles.DrawDottedLine(pointFrom, bezierSplineMulti.splines[indexSplineTo].GetControlPoint(indexPointTo), dotsSize);
            connectToPointIndex = indexPointTo;
            connectToSplineIndex = indexSplineTo;
            canConnect = true;
        }
        //Repaint();
    }



}
