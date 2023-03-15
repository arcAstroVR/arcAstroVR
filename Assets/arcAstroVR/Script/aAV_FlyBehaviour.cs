using UnityEngine;
using UnityEngine.InputSystem;

// FlyBehaviour inherits from GenericBehaviour. This class corresponds to the flying behaviour.
public class aAV_FlyBehaviour : aAV_GenericBehaviour
{
	public string flyButton = "Fly";              // Default fly button.
	public float flySpeed = 20f;                 // Default flying speed.
	public float sprintFactor = 10f;             // How much sprinting affects fly speed.
	public float flyMaxVerticalAngle = 90f;       // Angle to clamp camera vertical movement when flying.

	private int flyBool;                          // Animator variable related to flying.
	private bool fly = false;                     // Boolean to determine whether or not the player activated fly mode.
	private CapsuleCollider col;                  // Reference to the player capsulle collider.

	// Start is always called after any Awake functions.
	void Start()
	{
		// Set up the references.
		flyBool = Animator.StringToHash("Fly");
		col = this.GetComponent<CapsuleCollider>();
		// Subscribe this behaviour on the manager.
		behaviourManager.SubscribeBehaviour(this);
	}

	public void ChangeFly()
	{
		if (!behaviourManager.IsOverriding() && !behaviourManager.GetTempLockStatus(behaviourManager.GetDefaultBehaviour))
		{
			fly = !fly;

			// Force end jump transition.
			behaviourManager.UnlockTempBehaviour(behaviourManager.GetDefaultBehaviour);

			// Obey gravity. It's the law!
			behaviourManager.GetRigidBody.useGravity = !fly;

			// Player is flying.
			if (fly)
			{
				// Register this behaviour.
				behaviourManager.RegisterBehaviour(this.behaviourCode);
			}
			else
			{
				// Set collider direction to vertical.
				col.direction = 1;

				// Unregister this behaviour and set current behaviour to the default one.
				behaviourManager.UnregisterBehaviour(this.behaviourCode);
			}
		}

		// Assert this is the active behaviour
		fly = fly && behaviourManager.IsCurrentBehaviour(this.behaviourCode);

		// Set fly related variables on the Animator Controller.
		behaviourManager.GetAnim.SetBool(flyBool, fly);
	}

	// This function is called when another behaviour overrides the current one.
	public override void OnOverride()
	{
		// Ensure the collider will return to vertical position when behaviour is overriden.
		col.direction = 1;
	}

	// LocalFixedUpdate overrides the virtual function of the base class.
	public override void LocalFixedUpdate()
	{
		// Set camera limit angle related to fly mode.
		behaviourManager.GetCamScript.SetMaxVerticalAngle(flyMaxVerticalAngle);

		// Call the fly manager.
		FlyManagement(behaviourManager.GetH, behaviourManager.GetV);

	}
	
	
	// Deal with the player movement when flying.
	void FlyManagement(float horizontal, float vertical)
	{
		// Add a force player's rigidbody according to the fly direction.
		Vector3 direction = Rotating(horizontal, vertical);

		//飛行高度に応じた飛行速度補正
		var rigid = behaviourManager.GetRigidBody;
		Ray ray = new Ray(rigid.position, Vector3.down);
		RaycastHit hit;
		float aglSpeed; 
		if ( Physics.Raycast(ray, out hit) ) {
			aglSpeed = hit.distance/20 +1;
		} else {
			aglSpeed = 1;
		}
		
		//InputSytem&GamePad対応、岩城追加
		//飛行時上昇処理
		if (aAV_Event.rise){
			direction += new Vector3(0, 0.1f, 0);
		}
		//飛行時スプリント処理
		behaviourManager.GetRigidBody.AddForce((direction * flySpeed * aglSpeed * 100 * (aAV_Event.sprint ? sprintFactor : 1)), ForceMode.Acceleration);
	}

	// Rotate the player to match correct orientation, according to camera and key pressed.
	Vector3 Rotating(float horizontal, float vertical)
	{
		Vector3 forward = behaviourManager.playerCamera.TransformDirection(Vector3.forward);
		// Camera forward Y component is relevant when flying.
		forward = forward.normalized;

		Vector3 right = new Vector3(forward.z, 0, -forward.x);

		// Calculate target direction based on camera forward and direction key.
		Vector3 targetDirection = forward * vertical + right * horizontal;

		// Rotate the player to the correct fly position.
		if ((behaviourManager.IsMoving() && targetDirection != Vector3.zero))
		{
			Quaternion targetRotation = Quaternion.LookRotation(targetDirection);

			Quaternion newRotation = Quaternion.Slerp(behaviourManager.GetRigidBody.rotation, targetRotation, behaviourManager.turnSmoothing);

			behaviourManager.GetRigidBody.MoveRotation(newRotation);
			behaviourManager.SetLastDirection(targetDirection);
		}

		// Player is flying and idle?
		if (!(Mathf.Abs(horizontal) > 0.2 || Mathf.Abs(vertical) > 0.2))
		{
			// Rotate the player to stand position.
			behaviourManager.Repositioning();
			// Set collider direction to vertical.
			col.direction = 1;
		}
		else
		{
			// Set collider direction to horizontal.
			col.direction = 2;
		}

		// Return the current fly direction.
		return targetDirection;
	}
}
