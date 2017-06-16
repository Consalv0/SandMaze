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
	public bool isActive = true;
	public bool isHidden;
	public MazeWall index;

	Renderer render;
	Collider[] colls;
	Vector3 currentPosition;
	bool hidden;
	Vector3 targetPosition;
	bool controllerIsActive;

	void OnTriggerEnter() {
		if (controller.mazeSize[0] % 2 == index.MazePosition[0] % 2)
		if (controller.mazeSize[1] % 2 == index.MazePosition[1] % 2)
			controller.startPos = index.MazePosition;
	}

	void Start() {
		render = GetComponent<Renderer>();
		colls = GetComponents<Collider>();

		if (controller != null)
		if (isActive) {
			targetPosition = index.WorldPosition + Vector3.up * controller.wallSize.y * -0.5f;
			transform.position = targetPosition;
		} else {
			targetPosition = index.WorldPosition + Vector3.down * controller.wallSize.y * 0.5f;
			transform.position = targetPosition;
		}
		InvokeRepeating("UnActive", 0, Random.value * 10);
	}

	void Update() {
		if (controller != null) {
			var distance = index.WorldPosition - targetPosition;
			if (isActive) {
				targetPosition = index.WorldPosition + Vector3.down * controller.wallSize.y * -0.5f;
				if (distance.magnitude > 0.2f)
					transform.position = Vector3.SmoothDamp(transform.position, targetPosition,
																									ref currentPosition, controller.smoothVelocity, 5);
			} else {
				targetPosition = index.WorldPosition + Vector3.down * controller.wallSize.y * 0.5f;
				if (distance.magnitude > 0.2f)
					transform.position = Vector3.SmoothDamp(transform.position, targetPosition,
																									ref currentPosition, controller.smoothVelocity, 5);
			}
		}
	}

	void UnActive() {
		isHidden = Mathf.Abs(index.MazePosition[0] - controller.startPos[0]) +
							 Mathf.Abs(index.MazePosition[1] - controller.startPos[1]) > 16;
		if (hidden != isHidden) {
			hidden = isHidden;
			foreach(Collider coll in colls) {
				coll.enabled = !hidden;
			}
			render.enabled = !hidden;
			this.enabled = !hidden;
		}
	}
}
