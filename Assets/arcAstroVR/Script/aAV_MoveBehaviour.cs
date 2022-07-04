using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.XR.Management;
using System.Collections;

// MoveBehaviour inherits from GenericBehaviour. This class corresponds to basic walk and run behaviour, it is the default behaviour.
public class aAV_MoveBehaviour : aAV_GenericBehaviour
{
	public float walkSpeed = 0.15f;                 // Default walk speed.
	public float runSpeed = 1.0f;                   // Default run speed.
	public float sprintFactor = 10f;                // Default sprint speed.
	public float speedDampTime = 0f;              // Default damp time to change the animations based on current speed.
	public float jumpHeight = 1.5f;                 // Default jump height.
	public float jumpIntertialForce = 10f;          // Default horizontal inertial force when jumping.

	private float speed;						 // Moving speed.
	private int jumpBool;                           // Animator variable related to jumping.
	private int groundedBool;                       // Animator variable related to whether or not the player is on ground.
	private bool jump;                              // Boolean to determine whether or not the player started a jump.
	private bool isColliding;                       // Boolean to determine if the player has collided with an obstacle.

	//視点切り替え用追加設定　岩城追加
	private int aimBool;                                                  // Animator variable related to aiming.
	private bool aim;                                                     // Boolean to determine whether or not the player is aiming.
	public Vector3 aimPivotOffset = new Vector3(0f, 1.6f,  0f);         // Offset to repoint the camera when aiming.
	public Vector3 aimCamOffset   = new Vector3(0f, 0f, 0f);         // Offset to relocate the camera when aiming.

	//階段用設定　岩城追加
	private Vector3 stepOffset =  new Vector3(0f, 0.5f, 0f);	//階段検知レイの発射高さ
	private Vector3 RayAngle = new Vector3(0f, Mathf.Tan(50 * Mathf.Deg2Rad), 0f);
	private float RayDistance = 0.4f;		//階段の存在を確認するレイの距離
	
	// Start is always called after any Awake functions.
	void Start()
	{
		// Set up the references.
		jumpBool = Animator.StringToHash("Jump");
		groundedBool = Animator.StringToHash("Grounded");
		behaviourManager.GetAnim.SetBool(groundedBool, true);

		// Subscribe and register this behaviour as the default behaviour.
		behaviourManager.SubscribeBehaviour(this);
		behaviourManager.RegisterDefaultBehaviour(this.behaviourCode);
		
		// Set up the references.
		aimBool = Animator.StringToHash("Aim");
	}
	
	// ジャンプ設定　InputSystem対応　岩城追加
	public void DoJump(){
		if (!jump && behaviourManager.IsCurrentBehaviour(this.behaviourCode) && !behaviourManager.IsOverriding()) 
		{
			if(!behaviourManager.IsMoving()){		//岩城追加
				jumpIntertialForce = 0f;
			}else{
				jumpIntertialForce = 2+speed;
			}
			jump = true;
		}
	}

	// 視点設定　InputSystem対応　岩城追加
	public void ChangeAim(){
			if (!aim || (aAV_Public.displayMode ==1))
			{
				Debug.Log("1st Camera");
				aim = true;
				int signal = 1;
				aimCamOffset.x = Mathf.Abs(aimCamOffset.x) * signal;
				aimPivotOffset.x = Mathf.Abs(aimPivotOffset.x) * signal;
				behaviourManager.GetCamScript.SetTargetOffsets (aimPivotOffset, aimCamOffset);
				this.gameObject.transform.GetChild(0).gameObject.SetActive(false);
			}
			else
			{
				Debug.Log("3rd Camera");
				aim = false;
				behaviourManager.GetCamScript.ResetTargetOffsets();
				behaviourManager.GetCamScript.ResetMaxVerticalAngle();
				this.gameObject.transform.GetChild(0).gameObject.SetActive(true);
			}
	}
	
	// LocalFixedUpdate overrides the virtual function of the base class.
	public override void LocalFixedUpdate()
	{
		// Call the basic movement manager.
		MovementManagement(behaviourManager.GetH, behaviourManager.GetV);

		// Call the jump manager.
		JumpManagement();
		
	}

	// Execute the idle and walk/run jump movements.
	void JumpManagement()
	{
		// Start a new jump.
		if (jump && !behaviourManager.GetAnim.GetBool(jumpBool) && behaviourManager.IsGrounded())
		{
			// Set jump related parameters.
			behaviourManager.LockTempBehaviour(this.behaviourCode);
			behaviourManager.GetAnim.SetBool(jumpBool, true);

			// Temporarily change player friction to pass through obstacles.
			GetComponent<CapsuleCollider>().material.dynamicFriction = 0f;
			GetComponent<CapsuleCollider>().material.staticFriction = 0f;
			// Remove vertical velocity to avoid "super jumps" on slope ends.
			RemoveVerticalVelocity();
			// Set jump vertical impulse velocity.
			float velocity = 2f * Mathf.Abs(Physics.gravity.y) * jumpHeight;
			velocity = Mathf.Sqrt(velocity);
			behaviourManager.GetRigidBody.AddForce(Vector3.up * velocity, ForceMode.VelocityChange);
		}
		// Is already jumping?
		else if (behaviourManager.GetAnim.GetBool(jumpBool))
		{
			// Keep forward movement while in the air.
			if (!behaviourManager.IsGrounded() && !isColliding && behaviourManager.GetTempLockStatus())
			{
				behaviourManager.GetRigidBody.AddForce(transform.forward * jumpIntertialForce * Physics.gravity.magnitude * sprintFactor, ForceMode.Acceleration);
			}
			// Has landed?
			if ((behaviourManager.GetRigidBody.velocity.y < 0) && behaviourManager.IsGrounded())
			{
				behaviourManager.GetAnim.SetBool(groundedBool, true);
				// Change back player friction to default.
				GetComponent<CapsuleCollider>().material.dynamicFriction = 1f;
				GetComponent<CapsuleCollider>().material.staticFriction = 1f;
				// Set jump related parameters.
				jump = false;
				behaviourManager.GetAnim.SetBool(jumpBool, false);
				behaviourManager.UnlockTempBehaviour(this.behaviourCode);
			}
		}
	}

	// Deal with the basic player movement
	void MovementManagement(float horizontal, float vertical)
	{

		// On ground, obey gravity.
		if (behaviourManager.IsGrounded()){
			behaviourManager.GetRigidBody.useGravity = true;
		// Avoid takeoff when reached a slope end.
		}else if (!behaviourManager.GetAnim.GetBool(jumpBool) && behaviourManager.GetRigidBody.velocity.y > 0 && !isColliding){
			RemoveVerticalVelocity();
		}

		// Call function that deals with player orientation.
		Rotating(horizontal, vertical);

		// Set proper speed.
		Vector2 dir = new Vector2(horizontal, vertical);
		speed = Vector2.ClampMagnitude(dir, 1f).magnitude;
		behaviourManager.GetAnim.SetFloat(speedFloat, speed*(aAV_Event.sprint ? sprintFactor : 1f), speedDampTime, Time.deltaTime);
	}

	// Remove vertical rigidbody velocity.
	private void RemoveVerticalVelocity()
	{
		Vector3 horizontalVelocity = behaviourManager.GetRigidBody.velocity;
		horizontalVelocity.y = 0;
		behaviourManager.GetRigidBody.velocity = horizontalVelocity;
	}

	// Rotate the player to match correct orientation, according to camera and key pressed.
	Vector3 Rotating(float horizontal, float vertical)
	{
		// Get camera forward direction, without vertical component.
		Vector3 forward = behaviourManager.playerCamera.TransformDirection(Vector3.forward);

		// Player is moving on ground, Y component of camera facing is not relevant.
		forward.y = 0.0f;
		forward = forward.normalized;

		// Calculate target direction based on camera forward and direction key.
		Vector3 right = new Vector3(forward.z, 0, -forward.x);
		Vector3 targetDirection;
		targetDirection = forward * vertical + right * horizontal;

		// Lerp current direction to calculated target direction.
		if ((behaviourManager.IsMoving() && targetDirection != Vector3.zero))
		{
			Quaternion targetRotation = Quaternion.LookRotation(targetDirection);

			Quaternion newRotation = Quaternion.Slerp(behaviourManager.GetRigidBody.rotation, targetRotation, behaviourManager.turnSmoothing);
			behaviourManager.GetRigidBody.MoveRotation(newRotation);
			behaviourManager.SetLastDirection(targetDirection);
		}
		// If idle, Ignore current camera facing and consider last moving direction.
		if (!(Mathf.Abs(horizontal) > 0.9 || Mathf.Abs(vertical) > 0.9))
		{
			behaviourManager.Repositioning();
		}

		return targetDirection;
	}

	// Collision detection. コライダー接触判定（地面に立っているときも呼ばれる）
	private void OnCollisionStay(Collision collision)
	{
		isColliding = true;

		//階段処理
		var rigid = behaviourManager.GetRigidBody;
		var stepRayStart = rigid.position+ stepOffset ;
		var stepRayEnd1 = stepRayStart + (rigid.transform.forward - RayAngle)* RayDistance;
		var stepRayEnd2 = stepRayStart + (rigid.transform.forward + RayAngle)* RayDistance;
		var upCast1 = Physics.Linecast(stepRayStart, stepRayEnd1);
		var upCast2 = Physics.Linecast(stepRayStart, stepRayEnd2);
//		Debug.DrawLine(stepRayStart, stepRayEnd1, Color.red);
//		Debug.DrawLine(stepRayStart, stepRayEnd2, Color.red);
//		Debug.Log("階段判定"+(upCast1 && !upCast2);
		//collision.GetContact(0).normalは、接触相手の法線。normal.yが1の時は床接触、0の時は壁接触
		//collision.GetContact(0).pointは、接点の座標。
		if (behaviourManager.IsMoving() && upCast1 && !upCast2)
		{
			GetComponent<CapsuleCollider>().material.dynamicFriction = 0f;
			GetComponent<CapsuleCollider>().material.staticFriction = 0f;
			Vector3 horizontalVelocity = rigid.velocity;
			horizontalVelocity.y += 0.3f;
			rigid.velocity = horizontalVelocity;
		}else{
			GetComponent<CapsuleCollider>().material.dynamicFriction = 1f;
			GetComponent<CapsuleCollider>().material.staticFriction = 1f;
		}
	}
	private void OnCollisionExit(Collision collision)
	{
		isColliding = false;
	}
}
