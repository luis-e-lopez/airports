using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(EdgeCollider2D))]
public class Tail : MonoBehaviour {

	public float pointSpacing = .1f;
	public Transform head;

	List<Vector2> points;
	LineRenderer line;
	EdgeCollider2D col;

	// Use this for initialization
	void Start () {
		line = GetComponent<LineRenderer> ();
		col = GetComponent<EdgeCollider2D> ();

		points = new List<Vector2> ();
		SetPoint ();
	}
	
	// Update is called once per frame
	void Update () {

		if (Vector3.Distance (points.Last (), head.position) > pointSpacing) {
			SetPoint ();
		}
	}

	void SetPoint() {

		if (points.Count > 1) {
			col.points = points.ToArray<Vector2> ();
		}
		points.Add (head.position);

		line.positionCount = points.Count;
		line.SetPosition (line.positionCount - 1, head.position);
	}
}
