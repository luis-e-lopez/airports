using System;
using System.Collections.Generic;
using UnityEngine;

public class DijkstrasAlgo : MonoBehaviour
{
    private BezierSplineMulti bsMulti;

    private Dictionary<int, Distance> distances;
    private List<int> visitedNodes;
    private List<int> unvisitedNodes;

    [SerializeField]
    private bool highlight;

    public string[] nodes;
    public int initialNodeIdx;
    public int finalNodeIdx;

    private void Awake()
    {
        bsMulti = transform.GetComponent<BezierSplineMulti>();
    }

    public bool HighlightShortestPath 
    { 
        get 
        {
            return highlight;
        }
        set 
        {
            highlight = value;

            if (highlight) 
            {
                Debug.Log("Initial node: " + initialNodeIdx + ", Final node: " + finalNodeIdx);
                bsMulti.SetHighlightedPath(GetShortestPath(initialNodeIdx, finalNodeIdx));
            }
            else
            {
                bsMulti.SetHighlightedPath(new List<string>());
            }
        }
    }

    public int StartNode 
    { 
        get 
        {
            return initialNodeIdx;
        }
        set 
        {
            initialNodeIdx = value;
            CalculateShortestPath(initialNodeIdx);
        }
    }

    public int EndNode
    {
        get
        {
            return finalNodeIdx;
        }
        set
        {
            finalNodeIdx = value;
        }
    }

    public void SetNodes() 
    {
        if (bsMulti == null) 
        {
            nodes = new string[0];
            return;
        }


        nodes = new string[bsMulti.GetNodesCount()];

        for (int i = 0; i < bsMulti.GetNodesCount(); i++) 
        {
            nodes[i] = i + "";
        }
    }

    public int GetNodesCount() 
    {
        if (bsMulti == null)
            return 0;

        return bsMulti.GetNodesCount();
    }

    public List<BezierSplineMulti.NodeByPoint> GetNeighbourNodes(int nodeIndex) 
    {
        return bsMulti.GetNeighbourNodes(nodeIndex);
    }

    public Dictionary<int, Distance> GetDistances() 
    {
        return distances;
    }

    public void CalculateShortestPath(int initNode) 
    {
        InitDistances(initNode);
        InitUnvisited();
        CalculateDistances();
    }

    public void CalculateShortestPath() 
    {
        CalculateShortestPath(0);
    }

    private void InitDistances(int initNode) 
    {
        distances = new Dictionary<int, Distance>();

        for (int i = 0;  i < bsMulti.GetNodesCount(); i++) 
        {
            if (i == initNode)
                distances.Add(i, new Distance(-1, 0, -1, -1, -1));
            else
                distances.Add(i, new Distance(-1, float.MaxValue, -1, -1, -1));
        }
    }

    private void InitUnvisited() 
    {
        visitedNodes = new List<int>();
        unvisitedNodes = new List<int>();

        for (int i = 0; i < bsMulti.GetNodesCount(); i++) 
        {
            unvisitedNodes.Add(i);
        }
    }

    private List<string> GetShortestPath(int startNodeIndex, int finalNodeIndex) 
    {
        List<string> points = new List<string>();

        while (finalNodeIndex != startNodeIndex) 
        {
            Distance d;
            if (distances.TryGetValue(finalNodeIndex, out d)) 
            {
                int fromPoint = d.prevPoint;
                int toPoint = d.point;

                if (toPoint < fromPoint) 
                {
                    fromPoint = d.point;
                    toPoint = d.prevPoint;
                }

                while (toPoint != fromPoint) 
                {
                    points.Add(d.spline + "-" + fromPoint + "," + d.spline + "-" + (fromPoint + 3));
                    fromPoint += 3;
                }

                finalNodeIndex = d.prevNode;
            }
        }
        foreach (string pair in points)
            Debug.Log(pair);

        return points;
    }

    private void CalculateDistances() 
    {
        while (unvisitedNodes.Count > 0)
        {
            // visit the unvisited node with the smallest known distance from the starting node
            int currentNode = GetNodeWithShortestKnownDistance();

            // get all neighbour nodes from current node
            List<BezierSplineMulti.NodeByPoint> nodes = bsMulti.GetNeighbourNodes(currentNode);
            List<BezierSplineMulti.NodeByPoint> temp = new List<BezierSplineMulti.NodeByPoint>();
            // check which of these nodes are already visited and discard them
            foreach (BezierSplineMulti.NodeByPoint node in nodes)
            {
                if (unvisitedNodes.Contains(node.GetNodeIndex()))
                    temp.Add(node);
            }
            nodes = temp;
            // get distance accumulated until current node
            Distance distance;
            float d = 0f;
            if (distances.TryGetValue(currentNode, out distance))
            {
                d += distance.distance;
            }
            // calculate distance of each neighbour from start node
            foreach (BezierSplineMulti.NodeByPoint node in nodes)
            {
                float dis = bsMulti.GetSplineLengthFromNodeToControlPoint(currentNode, node.GetSplineIndex(), node.GetPointIndex());
                dis += d;
                Distance nDistance;
                distances.TryGetValue(node.GetNodeIndex(), out nDistance);
                // if calculated distance is less than the previously known distance, then update
                if (dis < nDistance.distance)
                {
                    distances.Remove(node.GetNodeIndex());
                    distances.Add(node.GetNodeIndex(), new Distance(currentNode, dis, node.GetSplineIndex(), node.GetPointIndex(), node.GetFromPointIndex()));
                }
            }
            // add the current node to the list of visited nodes and remove it from unvisited
            unvisitedNodes.Remove(currentNode);
            visitedNodes.Add(currentNode);
        }
    }

    private int GetNodeWithShortestKnownDistance() 
    {
        float shortestKnownDistance = float.MaxValue;
        int currentNode = -1;
        foreach (int node in unvisitedNodes) 
        {
            Distance distance;
            distances.TryGetValue(node, out distance);
            if (distance.distance < shortestKnownDistance) 
            {
                shortestKnownDistance = distance.distance;
                currentNode = node;
            }
        }
        return currentNode;
    }

    public void Reset()
    {
        bsMulti = transform.GetComponent<BezierSplineMulti>();
    }

    public class Distance 
    {
        public int prevNode;
        public float distance;
        public int spline;
        public int point;
        public int prevPoint;

        public Distance(int prevNode, float distance, int spline, int point, int prevPoint) 
        {
            this.prevNode = prevNode;
            this.distance = distance;
            this.spline = spline;
            this.point = point;
            this.prevPoint = prevPoint;
        }
    }
}
