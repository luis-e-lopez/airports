using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(LineRenderer))]
public class SplineDecorator2 : MonoBehaviour {

	public BezierSpline spline;

	public float pointSpacing = .01f;

	LineRenderer line;
	List<Vector2> points;

    //void Start() {

    //}

    //private void Awake () {

    //	line = GetComponent<LineRenderer> ();
    //	points = new List<Vector2> ();

    //	for(float ps = 0; ps < 1f; ps = ps + pointSpacing) {
    //		Vector3 position = spline.GetPoint(ps);
    //		points.Add (position);
    //		line.positionCount = points.Count;
    //		line.SetPosition (line.positionCount - 1, position);

    //	} 
    //}

    private void Awake()
    {

        line = GetComponent<LineRenderer>();
        points = new List<Vector2>();

        for (float ps = 0; ps < 1f; ps = ps + pointSpacing)
        {
            Vector3 position = spline.GetPoint(ps);

            points.Add(position);
            line.positionCount = points.Count;
            line.SetPosition(line.positionCount - 1, position);

        }
    }

    void Update()
    {

        if (Application.isPlaying)
        {
            return;
        }

        int lineCount = 0;
        for (float ps = 0; ps < 1f; ps = ps + pointSpacing)
        {
            Vector3 position = spline.GetPoint(ps);
            Vector2 point = points[lineCount];
            if (position.x != point.x || position.y != point.y)
            {
                line.SetPosition(lineCount, position);
                points.RemoveAt(lineCount);
                points.Insert(lineCount, position);
            }
            lineCount++;
        }
    }
}