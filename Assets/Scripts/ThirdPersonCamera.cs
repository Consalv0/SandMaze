using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ThirdPersonCamera : MonoBehaviour {
	public Transform target;
	public float zoomSpeed = 10;
	public float maxDistance = 20; // Posible Distances form the target to the camera
	public float minDistance = 5;  
	public float maxDistanceLimit = 100; // Limit of the posible distances
	public float minDistanceLimit = 0.1f;
	public float distance; // The actual distance
	float collisionDistance;	// Distance collide if there's a wall
	Vector3 vectorToCam;

	public float moveSmoothTime = 0.06f; // Move smooth factor
	Vector3 moveSmoothVelocity;	
	Vector3 currentPosition;

	public Vector2 rotationSpeed = new Vector2(10, 5); // Rotation max speed
	public float rotateSmoothTime = 0.12f; // Rotation smooth factor
	Vector3 rotateSmoothVelocity;
	Vector3 currentRotation;
	public float maxPitch = 85; // Range of angles in the X axis
	public float minPitch = -40;
	public float maxPitchLimit = 180; // Limit of the posiible pitch
	public float minPitchLimit = -180;
	float yaw;
	float pitch;

	void Start() {
		distance = maxDistance;
	}

	void FixedUpdate() {
		/* If there's a target, you can move the camera */
		if (target) {
			/* Measure the distance between the target and the camera, then cas a Ray and if there's collision measure 
			 * the distance and modify the current distance acordly to the MaxMin limits, otherwise only take the distance and clamp them*/

			vectorToCam = transform.position - target.position;
			RaycastHit hit;
			Debug.DrawRay(target.position, vectorToCam.normalized * collisionDistance, Color.red);
			if (Physics.Raycast(target.position, vectorToCam.normalized, out hit, distance, 1 << LayerMask.NameToLayer("Terrain"))) {
				collisionDistance = (hit.point - target.position).magnitude - 0.5f;
				collisionDistance = collisionDistance < 0 ? 0 : collisionDistance;
			} else {
				collisionDistance = distance;
			}
			distance += Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
			distance = Mathf.Clamp(distance, minDistance, maxDistance);

			/* Get the inputs and rotate with the given directions, the rotation is clamped and the aplied the rotation with smoothing,
			 * then move the camera in the direction between the camera and the target multipled by the previusly calculated direction */

		#if UNITY_STANDALONE_WIN
			yaw += (Input.GetAxis("Right Horizontal") + Input.GetAxis("Mouse Horizontal")) * rotationSpeed.x;
			pitch -= (Input.GetAxis("Right Vertical") + Input.GetAxis("Mouse Vertical")) * rotationSpeed.y;
		#else
			yaw += (Input.GetAxis("MacRight Horizontal") + Input.GetAxis("Mouse Horizontal")) * rotationSpeed.x;
			pitch -= (Input.GetAxis("MacRight Vertical") + Input.GetAxis("Mouse Vertical")) * rotationSpeed.y;
		#endif
			pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

			currentPosition = Vector3.SmoothDamp(currentPosition, target.position - transform.forward * collisionDistance, ref moveSmoothVelocity, moveSmoothTime);
			transform.position = currentPosition;
			
			currentRotation = Vector3.SmoothDamp(currentRotation, new Vector3(pitch, yaw), ref rotateSmoothVelocity, rotateSmoothTime);
			transform.eulerAngles = currentRotation;
		}
  }
}

/* Make a Custom GUI Editor Layout, the limits are clamped here */
#if UNITY_EDITOR
[CustomEditor(typeof(ThirdPersonCamera))]
public class ThirdPersonCameraEditor : Editor {
	public override void OnInspectorGUI() {
		ThirdPersonCamera script = (ThirdPersonCamera)target;
		script.target = EditorGUILayout.ObjectField("Target", script.target, typeof(Transform), true) as Transform;
		script.moveSmoothTime = EditorGUILayout.Slider("Move Smoothing", script.moveSmoothTime, 0.001f, 0.1f);

		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Zoom", EditorStyles.boldLabel);
		script.zoomSpeed = EditorGUILayout.Slider("Zoom Speed", script.zoomSpeed, 2, 25);
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Camera Dist", GUILayout.MaxWidth(100.0f), GUILayout.MinWidth(12.0f));
		GUILayout.FlexibleSpace();
		EditorGUIUtility.labelWidth = 32;
		script.minDistance = EditorGUILayout.FloatField("Min:", script.minDistance, GUILayout.MinWidth(62));
		GUILayout.FlexibleSpace();
		script.maxDistance = EditorGUILayout.FloatField("Max:", script.maxDistance, GUILayout.MinWidth(62));
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.BeginHorizontal();

		EditorGUIUtility.labelWidth = 1;
		EditorGUILayout.LabelField(script.minDistanceLimit.ToString(), GUILayout.MaxWidth(30));
		EditorGUILayout.MinMaxSlider(ref script.minDistance, ref script.maxDistance, script.minDistanceLimit, script.maxDistanceLimit);
		EditorGUIUtility.labelWidth = 1;
		EditorGUILayout.LabelField(script.maxDistanceLimit.ToString(), GUILayout.MaxWidth(30));
		EditorGUIUtility.labelWidth = 0;
		EditorGUILayout.EndHorizontal();

		EditorGUILayout.Space();
		EditorGUILayout.LabelField("Rotation", EditorStyles.boldLabel);
		script.rotationSpeed = EditorGUILayout.Vector2Field("Rotation Speed", script.rotationSpeed);
		script.rotateSmoothTime = EditorGUILayout.Slider("Rotation Smoothing", script.rotateSmoothTime, 0, 0.8f);
		EditorGUILayout.BeginHorizontal();
		EditorGUILayout.LabelField("Camera Angle", GUILayout.MaxWidth(100.0f), GUILayout.MinWidth(12.0f));
		GUILayout.FlexibleSpace();
		EditorGUIUtility.labelWidth = 32;
		script.minPitch = EditorGUILayout.FloatField("Min:", script.minPitch, GUILayout.MinWidth(62));
		GUILayout.FlexibleSpace();
		script.maxPitch = EditorGUILayout.FloatField("Max:", script.maxPitch, GUILayout.MinWidth(62));
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.BeginHorizontal();

		EditorGUIUtility.labelWidth = 1;
		EditorGUILayout.LabelField(script.minPitchLimit.ToString(), GUILayout.MaxWidth(30));
		EditorGUILayout.MinMaxSlider(ref script.minPitch, ref script.maxPitch, script.minPitchLimit, script.maxPitchLimit);
		EditorGUIUtility.labelWidth = 1;
		EditorGUILayout.LabelField(script.maxPitchLimit.ToString(), GUILayout.MaxWidth(30));
		EditorGUIUtility.labelWidth = 0;
		EditorGUILayout.EndHorizontal();
	}
}
#endif
