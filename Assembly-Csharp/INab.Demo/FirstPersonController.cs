using UnityEngine;

namespace INab.Demo;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonController : MonoBehaviour
{
	[Header("Player")]
	[Tooltip("Move speed of the character in m/s")]
	public float MoveSpeed = 4f;

	[Tooltip("Sprint speed of the character in m/s")]
	public float SprintSpeed = 6f;

	[Tooltip("Rotation speed of the character")]
	public float RotationSpeed = 45f;

	[Tooltip("Acceleration and deceleration")]
	public float SpeedChangeRate = 10f;

	[Space(10f)]
	[Tooltip("The height the player can jump")]
	public float JumpHeight = 1.2f;

	[Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
	public float Gravity = -15f;

	[Space(10f)]
	[Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
	public float JumpTimeout = 0.1f;

	[Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
	public float FallTimeout = 0.15f;

	[Header("Player Grounded")]
	[Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
	public bool Grounded = true;

	[Tooltip("Useful for rough ground")]
	public float GroundedOffset = -0.14f;

	[Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
	public float GroundedRadius = 0.5f;

	[Tooltip("What layers the character uses as ground")]
	public LayerMask GroundLayers;

	[Header("Cinemachine")]
	[Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
	public GameObject CinemachineCameraTarget;

	[Tooltip("How far in degrees can you move the camera up")]
	public float TopClamp = 90f;

	[Tooltip("How far in degrees can you move the camera down")]
	public float BottomClamp = -90f;

	private float _cinemachineTargetPitch;

	private float _speed;

	private float _rotationVelocity;

	private float _verticalVelocity;

	private float _terminalVelocity = 53f;

	private float _jumpTimeoutDelta;

	private float _fallTimeoutDelta;

	private CharacterController _controller;

	private GameObject _mainCamera;

	private const float _threshold = 0.01f;

	private void Awake()
	{
		if (_mainCamera == null)
		{
			_mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
		}
	}

	private void Start()
	{
		_controller = GetComponent<CharacterController>();
		_jumpTimeoutDelta = JumpTimeout;
		_fallTimeoutDelta = FallTimeout;
	}

	private void Update()
	{
		JumpAndGravity();
		GroundedCheck();
		Move();
	}

	private void LateUpdate()
	{
		CameraRotation();
	}

	private void GroundedCheck()
	{
		Vector3 vector = new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z);
		Grounded = Physics.CheckSphere(vector, GroundedRadius, (int)GroundLayers, (QueryTriggerInteraction)1);
	}

	private void CameraRotation()
	{
		Vector2 vector = new Vector2(Input.GetAxis("Mouse X"), 0f - Input.GetAxis("Mouse Y"));
		if (vector.sqrMagnitude >= 0.01f)
		{
			float deltaTime = Time.deltaTime;
			_cinemachineTargetPitch += vector.y * RotationSpeed * deltaTime;
			_rotationVelocity = vector.x * RotationSpeed * deltaTime;
			_cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);
			CinemachineCameraTarget.transform.localRotation = Quaternion.Euler(_cinemachineTargetPitch, 0f, 0f);
			transform.Rotate(Vector3.up * _rotationVelocity);
		}
	}

	private void Move()
	{
		//IL_018a: Unknown result type (might be due to invalid IL or missing references)
		bool num = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
		Vector2 vector = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
		float num2 = (num ? SprintSpeed : MoveSpeed);
		if (vector == Vector2.zero)
		{
			num2 = 0f;
		}
		float magnitude = new Vector3(_controller.velocity.x, 0f, _controller.velocity.z).magnitude;
		float num3 = 0.1f;
		float num4 = 1f;
		if (magnitude < num2 - num3 || magnitude > num2 + num3)
		{
			_speed = Mathf.Lerp(magnitude, num2 * num4, Time.deltaTime * SpeedChangeRate);
			_speed = Mathf.Round(_speed * 1000f) / 1000f;
		}
		else
		{
			_speed = num2;
		}
		Vector3 vector2 = new Vector3(vector.x, 0f, vector.y).normalized;
		if (vector != Vector2.zero)
		{
			vector2 = transform.right * vector.x + transform.forward * vector.y;
		}
		_controller.Move(vector2.normalized * (_speed * Time.deltaTime) + new Vector3(0f, _verticalVelocity, 0f) * Time.deltaTime);
	}

	private void JumpAndGravity()
	{
		bool keyDown = Input.GetKeyDown(KeyCode.Space);
		if (Grounded)
		{
			_fallTimeoutDelta = FallTimeout;
			if (_verticalVelocity < 0f)
			{
				_verticalVelocity = -2f;
			}
			if (keyDown && _jumpTimeoutDelta <= 0f)
			{
				_verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);
			}
			if (_jumpTimeoutDelta >= 0f)
			{
				_jumpTimeoutDelta -= Time.deltaTime;
			}
		}
		else
		{
			_jumpTimeoutDelta = JumpTimeout;
			if (_fallTimeoutDelta >= 0f)
			{
				_fallTimeoutDelta -= Time.deltaTime;
			}
		}
		if (_verticalVelocity < _terminalVelocity)
		{
			_verticalVelocity += Gravity * Time.deltaTime;
		}
	}

	private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
	{
		if (lfAngle < -360f)
		{
			lfAngle += 360f;
		}
		if (lfAngle > 360f)
		{
			lfAngle -= 360f;
		}
		return Mathf.Clamp(lfAngle, lfMin, lfMax);
	}

	private void OnDrawGizmosSelected()
	{
		Color color = new Color(0f, 1f, 0f, 0.35f);
		Color color2 = new Color(1f, 0f, 0f, 0.35f);
		if (Grounded)
		{
			Gizmos.color = color;
		}
		else
		{
			Gizmos.color = color2;
		}
		Gizmos.DrawSphere(new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z), GroundedRadius);
	}
}
