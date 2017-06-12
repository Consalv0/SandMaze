using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Detector : MonoBehaviour {
	public MazeManager manager;
	public int[] pos;
	public bool isActive;

	public Vector3 onPosition;
	public Vector3 offPosition;
	Vector3 currentPosition;
	[Range(0, 1)]
	public float smoothVelPosition;
	Transform parent;

	void OnTriggerEnter(Collider other) {
		if (other.GetComponentInChildren<Indicator>() != null)
			manager.initialPos = pos;
	}

	void Start() {
		parent = transform.parent.gameObject.transform;
		onPosition = parent.position + new Vector3(0, 1, 0);
		offPosition = parent.position;
	}

	void FixedUpdate() {
		if (isActive) {
			parent.position = Vector3.SmoothDamp(parent.position, onPosition, ref currentPosition, smoothVelPosition, 5);
		} else {
			parent.position = Vector3.SmoothDamp(parent.position, offPosition, ref currentPosition, smoothVelPosition, 5);
		}
	}	
}
