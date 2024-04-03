using UnityEngine;
using System.Collections;

// 3rd-person movement that picks direction relative to target (usually the camera)
// commented lines demonstrate snap to direction and without ground raycast
//
// To setup animated character create an animation controller with states for idle, running, jumping
// transition between idle and running based on added Speed float, set those not atomic so that they can be overridden by...
// transition both idle and running to jump based on added Jumping boolean, transition back to idle

[RequireComponent(typeof(CharacterController))]
public class RelativeMovementEthan : MonoBehaviour {
	[SerializeField] private Transform target;
	public float rotSpeed = 15.0f;
	public float moveSpeed = 6.0f;

	public float jumpSpeed = 15.0f;
	public float gravity = -9.8f;
	public float terminalVelocity = -10.0f;
	public float minFall = -1.5f;

	private CharacterController charController;

	private Animator animator;
	

	private float vertSpeed;
	private ControllerColliderHit contact;

	void Start() {
		charController = GetComponent<CharacterController>();
		vertSpeed = minFall;
		animator = GetComponent<Animator>();
	}
	
	// Update is called once per frame
	void Update() {
		
		

		// start with zero and add movement components progressively
		Vector3 movement = Vector3.zero;
		

		// x z movement transformed relative to target
		float horInput = Input.GetAxis("Horizontal");
		float vertInput = Input.GetAxis("Vertical");
		if (horInput != 0 || vertInput != 0) {
			movement.x = horInput * moveSpeed;
			movement.z = vertInput * moveSpeed;
			movement = Vector3.ClampMagnitude(movement, moveSpeed);

			Quaternion tmp = target.rotation;
			target.eulerAngles = new Vector3(0, target.eulerAngles.y, 0);
			movement = target.TransformDirection(movement);
			target.rotation = tmp;

			// face movement direction
			//transform.rotation = Quaternion.LookRotation(movement);
			Quaternion direction = Quaternion.LookRotation(movement);
			transform.rotation = Quaternion.Lerp(transform.rotation,
			                                     direction, rotSpeed * Time.deltaTime);
		}
		
		Debug.Log(movement.sqrMagnitude);
		animator.SetFloat("Speed", movement.sqrMagnitude);

		bool hitGround = false;
		RaycastHit hit;
		if (vertSpeed < 0 && Physics.Raycast(transform.position, Vector3.down, out hit)) {
			float check = (charController.height + charController.radius) / 1.9f;
			hitGround = hit.distance <= check;
		}

		if (hitGround) {
			if (Input.GetButtonDown("Jump")) {
				vertSpeed = jumpSpeed;
			} else {
				vertSpeed = minFall;
				animator.SetBool("Jumping", false);
			}
		} else {
			vertSpeed += gravity * 5 * Time.deltaTime;
			if (vertSpeed < terminalVelocity) {
				vertSpeed = terminalVelocity;
			}
			if (contact != null) {
				animator.SetBool("Jumping", true);
			}
			if (charController.isGrounded) {
				if (Vector3.Dot(movement, contact.normal) < 0) {
					movement = contact.normal * moveSpeed;
				} else {
					movement += contact.normal * moveSpeed;
				}
			}
		}


		
		movement.y = vertSpeed;

		movement *= Time.deltaTime;
		charController.Move(movement);
	}

	void OnControllerColliderHit(ControllerColliderHit hit) {
		contact = hit;
	}
}
