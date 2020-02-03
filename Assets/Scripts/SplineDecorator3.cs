using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class SplineDecorator3 : MonoBehaviour {

	public BezierSpline spline;
	//public SplineWalker3 swalker;

	public float pointSpacing = .01f;
	public float frecuency = 2f;
	public int numOfWalkers = 3;

	LineRenderer line;
	List<Vector2> points;
	private SplineWalker3[] walkers;
	private float timeSpent = 0f;
	private int walkersCount = 0;

	/*void Start() {
		walkers = new SplineWalker3[numOfWalkers];
	}

	void Update () {

		timeSpent = timeSpent + Time.fixedDeltaTime;
		if (timeSpent > frecuency && walkersCount < numOfWalkers) {

			walkers [walkersCount] = Instantiate (swalker);
			if (walkersCount % 2 == 0) {
				walkers [walkersCount].setProgress (1f, false);
			}
			walkers [walkersCount].spline = this.spline;
			timeSpent = 0;
			walkersCount++;
		}

	}*/

	private void Awake () {

		line = GetComponent<LineRenderer> ();
		points = new List<Vector2> ();

		for(float ps = 0; ps < 1f; ps = ps + pointSpacing) {
			Vector3 position = spline.GetPoint(ps);
			points.Add (position);
			line.positionCount = points.Count;
			line.SetPosition (line.positionCount - 1, position);

		} 
			
	}
}