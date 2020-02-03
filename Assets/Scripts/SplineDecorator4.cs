using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class SplineDecorator4 : MonoBehaviour {

	public BezierSpline spline;
	public SplineWalker3 swalker;
	public GameObject arrow;

	public float pointSpacing = .01f;
	public float frecuency = 2f;
	public int numOfWalkers = 3;

	LineRenderer line;
	List<Vector2> points;
	private SplineWalker3[] walkers;
	private float timeSpent = 0f;
	private int walkersCount = 0;

	private Renderer rend;

	void Start() {
		Debug.Log ("START");
		walkers = new SplineWalker3[numOfWalkers];
		rend = GetComponent<Renderer>();
		/*for (int i = 0; i < numOfWalkers; i++) {
			walkers [i] = Instantiate (swalker);
		}*/
	}

	void Update () {

		timeSpent = timeSpent + Time.fixedDeltaTime;
		if (timeSpent > frecuency && walkersCount < numOfWalkers) {
			Debug.Log ("TIME: " + timeSpent);
			walkers [walkersCount] = Instantiate (swalker);
			//if (walkersCount % 2 == 0) {
			//	walkers [walkersCount].setProgress (1f, false);
			//}
			walkers [walkersCount].spline = this.spline;
			timeSpent = 0;
			walkersCount++;
		}

	}

	private void Awake () {

		line = GetComponent<LineRenderer> ();
		Color color = line.startColor;
		points = new List<Vector2> ();

		for(float ps = 0; ps < 1f; ps = ps + pointSpacing) {
			Vector3 position = spline.GetPoint(ps);
			points.Add (position);
			line.positionCount = points.Count;
			line.SetPosition (line.positionCount - 1, position);
			if (points.Count % 7 == 0) {
				GameObject arrowCopy = Instantiate (arrow);
				SpriteRenderer spriteRenderer = arrowCopy.GetComponent<SpriteRenderer> ();
				spriteRenderer.color = color;
				arrowCopy.transform.position = position;

				Vector3 dir = spline.GetDirection(ps);
				float angle = Vector2.Angle(new Vector2(0, 1), new Vector2(dir.x, dir.y));
				if (dir.x > 0) {
					angle = angle * -1;
				}
				arrowCopy.transform.Rotate(0, 0, angle, Space.Self);
			}

		} 
			
	}
}