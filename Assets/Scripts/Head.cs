using UnityEngine;

public class Head : MonoBehaviour {

	public float speed = 3f;
	public float rotationSpeed = 200f;

	float horizontal = 0f;

	// Use this for initialization
	void Update () {
		horizontal = Input.GetAxisRaw ("Horizontal");
	}
	
	// Update is called once per frame
	void FixedUpdate () {

		transform.Translate (Vector2.up * speed * Time.fixedDeltaTime, Space.Self);
		transform.Rotate (Vector3.forward * -horizontal * rotationSpeed * Time.fixedDeltaTime);
	}

	void OnTriggerEnter2D(Collider2D col) {
		Debug.Log (col.name);
	}
}
