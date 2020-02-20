using UnityEngine;

public class SplineWalker2 : MonoBehaviour {

	public BezierSpline spline;
	public float speed = 3f;
	public bool lookForward;
	public float rotationSpeed = 100f;
	public SplineWalkerMode mode;
	public float stopTime = 1f;

	private float progress;
	private bool goingForward = true;
	private Vector3 position;
	private float constantSpeed;
	private float accumulatedStopTime;
	private int collidersCount = 0;
	private Collider col;
	private float lastDistance = 0f;

	void Start () {
		position = spline.GetPoint2(progress);
		transform.localPosition = position;
		constantSpeed = speed;
	}
	// Update is called once per frame
	void FixedUpdate () {

		if (constantSpeed == 0) {
			accumulatedStopTime += Time.fixedDeltaTime;
			if (accumulatedStopTime < stopTime) {
				return;
			}
		}

        if (progress >= 0.95f) 
        {
            Debug.Log("Progress: " + progress);
        }

        if (goingForward) {
			progress += Time.fixedDeltaTime * constantSpeed;
			if (progress > 1f) {
				if (mode == SplineWalkerMode.Once) {
					progress = 1f;
				}
				else if (mode == SplineWalkerMode.Loop) {
					progress -= 1f;
				}
				else {
					progress = 2f - progress;
					goingForward = false;
				}
			}
		}
		else {
			progress -= Time.fixedDeltaTime * constantSpeed;
			if (progress < 0f) {
				progress = -progress;
				goingForward = true;
			}
		}

		position = spline.GetPoint2(progress);
		position.Set (position.x, position.y, position.z - .05f);
		Vector3 velocity = spline.GetVelocity (progress);

		if (collidersCount == 1 || collidersCount == 3) {
			//Debug.Log ("Distance: " + Vector3.Distance (col.transform.position, transform.position));
			constantSpeed = speed * (Vector3.Distance (col.transform.position, position) / velocity.magnitude);
		} else if (collidersCount == 2) {
			constantSpeed = speed * (.1f / velocity.magnitude);

			float newDistance = Vector3.Distance (col.transform.position, position);
			//Debug.Log ("Distance: " + newDistance);
			if (lastDistance < newDistance) {
				constantSpeed = 0;
				accumulatedStopTime = 0;
				collidersCount++;
				//Debug.Log ("STOPS");
			}
			lastDistance = newDistance;
		} else {
			constantSpeed = speed * (1f / velocity.magnitude);
		}

		transform.localPosition = position;
		if (lookForward) {
            //Debug.Log("Forward");

            transform.LookAt (position + spline.GetDirection(progress), Vector3.forward);
            //transform.RotateAround(transform.position, transform.up, Time.deltaTime * 90f);
            //Vector3 dir = spline.GetDirection(progress);
            //Debug.Log("X:" + dir.x + " Y:" + dir.y + " Z:" + dir.z);
            //float angle = Vector3.Angle(position, position + spline.GetDirection(progress));
            //Debug.Log("Angle:" + angle);
            transform.Rotate(90f, -90f, 0);
        }
		/*
		Debug.Log ("Dir: " + position.magnitude);
		transform.Translate (Vector2.up * speed * Time.fixedDeltaTime, Space.World);
		transform.Rotate (Vector3.forward * spline.GetDirection2(progress).magnitude * Time.fixedDeltaTime);
		*/
	}

	void OnTriggerEnter(Collider col) {

		if (col.tag == "Local Station" || col.tag == "Local and Express Station") {
			
			if (collidersCount == 0) {
				// start decreasing speed
				//Debug.Log ("ENTERS");
				//Debug.Log ("Starts decreasing speed. Position is " + transform.position + ", Col Position is " + col.transform.position);
				//Debug.Log ("Distance: " + Vector3.Distance (col.transform.position, transform.position));
				this.col = col;
				collidersCount++;
			} else if (collidersCount == 1) {
				//Debug.Log ("ENTERS STATION");
				lastDistance = Vector3.Distance (col.transform.position, transform.position);
				//constantSpeed = 0;
				//accumulatedStopTime = 0;
				collidersCount++;
			} else {
				// do nothing
				collidersCount = 0;
			}

		} else {
			constantSpeed = 0;
			accumulatedStopTime = 0;
		}
	}
}
