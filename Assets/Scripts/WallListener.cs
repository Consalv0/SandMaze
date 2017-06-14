using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct MazeWall {
	public int Index;
	public GameObject Prefab;
	public int[] MazePosition;
	public Vector3 WorldPosition;
}

public class WallListener : MonoBehaviour {
	public MazeController controller;
	public MazeWall index;
	public bool isActive = true;
	public bool isHidden;

	Renderer render;
	Collider coll;
	Vector3 currentPosition;
	bool hidden;
	Vector3 targetPosition;

	void Start() {
		render = GetComponent<Renderer>();
		coll = GetComponent<Collider>();

		if (controller != null)
		if (isActive) {
			targetPosition = index.WorldPosition + Vector3.up * controller.wallSize.y * 0.4f;
			transform.position = targetPosition;
		} else {
			targetPosition = index.WorldPosition + Vector3.down * controller.wallSize.y * 0.4f;
			transform.position = targetPosition;
		}
	}

	void Update() {
		if (controller != null) {
			if (hidden != isHidden) {
				hidden = isHidden;
				render.enabled = !hidden;
				coll.enabled = !hidden;
			}

			var distance = index.WorldPosition - targetPosition;
			if (!hidden) {
				if (isActive) {
					targetPosition = index.WorldPosition + Vector3.up * controller.wallSize.y * 0.4f;
					if (distance.magnitude > 0.2f)
					transform.position = Vector3.SmoothDamp(transform.position, targetPosition,
																									ref currentPosition, controller.smoothVelocity, 5);
				} else {
					targetPosition = index.WorldPosition + Vector3.down * controller.wallSize.y * 0.4f;
					if (distance.magnitude > 0.2f)
						transform.position = Vector3.SmoothDamp(transform.position, targetPosition,
																										ref currentPosition, controller.smoothVelocity, 5);
				}
			}
		}
	}
}
