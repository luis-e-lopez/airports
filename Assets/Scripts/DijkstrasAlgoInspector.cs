using UnityEditor;
using System.Collections.Generic;
using UnityEngine;
using System;

[CustomEditor(typeof(DijkstrasAlgo))]
public class DijkstrasAlgoInspector : Editor
{
    private DijkstrasAlgo dijkstrasAlgo;

    public override void OnInspectorGUI() 
    {
        dijkstrasAlgo = target as DijkstrasAlgo;

        EditorGUI.BeginChangeCheck();
        bool highlight = EditorGUILayout.Toggle("Highlight Shortest Path", dijkstrasAlgo.HighlightShortestPath);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(dijkstrasAlgo, "Shortest Path");
            EditorUtility.SetDirty(dijkstrasAlgo);
            dijkstrasAlgo.HighlightShortestPath = highlight;
        }

        dijkstrasAlgo.SetNodes();
        EditorGUI.BeginChangeCheck();
        int startNode = EditorGUILayout.Popup(new GUIContent("Start Node"), dijkstrasAlgo.StartNode, dijkstrasAlgo.nodes);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(dijkstrasAlgo, "Start Node");
            EditorUtility.SetDirty(dijkstrasAlgo);
            dijkstrasAlgo.StartNode = startNode;
        }

        EditorGUI.BeginChangeCheck();
        int endNode = EditorGUILayout.Popup(new GUIContent("End Node"), dijkstrasAlgo.EndNode, dijkstrasAlgo.nodes);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(dijkstrasAlgo, "End Node");
            EditorUtility.SetDirty(dijkstrasAlgo);
            dijkstrasAlgo.EndNode = endNode;
        }

        //GUILayout.Label("Nodes count: " + dijkstrasAlgo.GetNodesCount());

        for (int i = 0; i < dijkstrasAlgo.GetNodesCount(); i++) 
        {
            List<BezierSplineMulti.NodeByPoint> neighbourNodes = dijkstrasAlgo.GetNeighbourNodes(i);
            GUILayout.Label("Node " + i + " with neighbour nodes: ");
            foreach (BezierSplineMulti.NodeByPoint nbp in neighbourNodes) 
            {
                GUILayout.Label(" ==> Node " + nbp.GetNodeIndex() + " in spline " + nbp.GetSplineIndex() + " and point " + nbp.GetPointIndex() + ", point in node " + i + " is " + nbp.GetFromPointIndex());
            }

        }

        //dijkstrasAlgo.CalculateShortestPath();
        Dictionary<int, DijkstrasAlgo.Distance> distances = dijkstrasAlgo.GetDistances();

        if (distances == null)
            return;

        GUILayout.Label(" |\tNode\t|\tDistance\t|\tPrevious\t|\tSpline\t|\tPoint\t|\tPrevPoint\t|");
        for (int i = 0; i < dijkstrasAlgo.GetNodesCount(); i++) 
        {
            DijkstrasAlgo.Distance distance;
            if (distances.TryGetValue(i, out distance)) 
            {
                GUILayout.Label(" |\t" + i + "\t|\t" + distance.distance + "\t|\t" + distance.prevNode + "\t|\t" + distance.spline + "\t|\t" + distance.point + "\t|\t" + distance.prevPoint + "\t|");
            }
        }
    }
}
