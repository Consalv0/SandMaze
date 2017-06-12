using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ControllerMovement : MonoBehaviour {
	Rigidbody rigBody;
	public Transform camTransform; // The camera transform if not assigned it will be the main camera

	[Range(0.05f, 3.5f)]
	public float speedSmoothing = 1.5f;	// Speed smoothing factor
	public float maxSpeed = 10f; // Max speed
	[SerializeField]
	float curretVelocity;
	float targetVelocity;
	float speedSmoothVelocity;

	[Range(0.05f, 1)]
	public float stabilizeSmoothing = 0.3f; // Stabilize smoothing factor
	public Vector3 rotationSmoothing = new Vector3(0.5f, 0.3f, -20f); // Vector3 smoothing factor
	Vector3 rotationSmoothVelocity;

	Vector2 input;
	Vector2 inputDir;
	public float inputSpeed;
	Vector3 targetRotation;

#if UNITY_STANDALONE_OSX
	bool rightTriggerReady;
#endif

	void Start() {
		Cursor.lockState = CursorLockMode.Locked;

		rigBody = GetComponent<Rigidbody>();
		if (camTransform == null)
			camTransform = Camera.main.transform;
	}
	void Update() {
		/* Get the inputs, and make a direction with them, then while there's a direction or the gameObject is moving
		 * add torque with the respective rotation, I added a Relative Rotation because of the interaction of the torque in the X axis,
		 * then added the stabilize function because of the wanted tend of the gameObject to be stand up, at the end we only multiply
		 * the speed and add the velocity clamped*/

		inputSpeed = Input.GetButton("Right Trigger") ? 1 : 0;
	#if UNITY_STANDALONE_WIN
		inputSpeed += Input.GetAxisRaw("Right Trigger");
	#endif
	#if UNITY_STANDALONE_OSX
		inputSpeed += MacTrigger("Right", ref rightTriggerReady);
	#endif
		input = new Vector2(Input.GetAxisRaw("Left Horizontal"), Input.GetAxisRaw("Left Vertical"));
		inputDir = input.normalized;
		if (inputSpeed > 0) {
			targetVelocity = maxSpeed * inputSpeed;
			curretVelocity = Mathf.SmoothDamp(curretVelocity, targetVelocity, ref speedSmoothVelocity, speedSmoothing);
			rigBody.velocity = camTransform.forward * curretVelocity;
		} else {
			curretVelocity = rigBody.velocity.magnitude;
		}

		//// Kinematic Movement ////
		/* curretSpeed = Mathf.SmoothDamp(curretSpeed, targetSpeed, ref speedSmoothVelocity, speedSmoothing);
		transform.Translate(transform.forward * curretSpeed * Time.deltaTime, Space.World); */
	}

	public float MacTrigger(string side, ref bool triggerReady) {
		float adjustedAxis = 0f;
		float timeAxis = Input.GetAxisRaw("Mac" + side + " Trigger");

		if((timeAxis > -0.9f && timeAxis < -0.0001f) && triggerReady == false)
			triggerReady = true;
		if (triggerReady)
			adjustedAxis = (timeAxis + 1) * 0.5f;

		return adjustedAxis;
	}
}
