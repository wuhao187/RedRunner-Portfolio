using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

using UnityStandardAssets.CrossPlatformInput;

using RedRunner.Utilities;

namespace RedRunner.Characters
{

	public class RedCharacter : Character
	{
		#region Fields

		[Header ( "Character Details" )]
		[Space]
		[SerializeField]
		protected float m_MaxRunSpeed = 8f;
		[SerializeField]
		protected float m_RunSmoothTime = 5f;
		[SerializeField]
		protected float m_RunSpeed = 5f;
		[SerializeField]
		protected float m_WalkSpeed = 1.75f;
		[SerializeField]
		protected float m_JumpStrength = 10f;
		[SerializeField]
		protected int m_MaxAirJumps = 1;
		[SerializeField]
		protected float m_AirJumpStrength = 9f;
		[SerializeField]
		protected float m_DashSpeed = 14f;
		[SerializeField]
		protected float m_DashDuration = 0.15f;
		[SerializeField]
		protected float m_DashCooldown = 1f;
		[SerializeField]
		protected float m_DashStretchX = 1.25f;
		[SerializeField]
		protected float m_DashStretchY = 0.85f;
		[SerializeField]
		protected bool m_DifficultyScalingEnabled = true;
		[SerializeField]
		protected float m_DifficultyFullMeters = 120f;
		[SerializeField]
		protected float m_DifficultyRunSpeedBoost = 2f;
		[SerializeField]
		protected float m_DifficultyMaxRunSpeedBoost = 3.5f;
		[SerializeField]
		protected string[] m_Actions = new string[0];
		[SerializeField]
		protected int m_CurrentActionIndex = 0;

		[Header ( "Character Reference" )]
		[Space]
		[SerializeField]
		protected Rigidbody2D m_Rigidbody2D;
		[SerializeField]
		protected Collider2D m_Collider2D;
		[SerializeField]
		protected Animator m_Animator;
		[SerializeField]
		protected GroundCheck m_GroundCheck;
		[SerializeField]
		protected ParticleSystem m_RunParticleSystem;
		[SerializeField]
		protected ParticleSystem m_JumpParticleSystem;
		[SerializeField]
		protected ParticleSystem m_DashParticleSystem;
		[SerializeField]
		protected ParticleSystem m_WaterParticleSystem;
		[SerializeField]
		protected ParticleSystem m_BloodParticleSystem;
		[SerializeField]
		protected Skeleton m_Skeleton;
		[SerializeField]
		protected float m_RollForce = 10f;

		[Header ( "Character Audio" )]
		[Space]
		[SerializeField]
		protected AudioSource m_MainAudioSource;
		[SerializeField]
		protected AudioSource m_FootstepAudioSource;
		[SerializeField]
		protected AudioSource m_JumpAndGroundedAudioSource;

		#endregion

		#region Private Variables

		protected bool m_ClosingEye = false;
		protected bool m_Guard = false;
		protected bool m_Block = false;
		protected Vector2 m_Speed = Vector2.zero;
		protected float m_CurrentRunSpeed = 0f;
		protected float m_CurrentSmoothVelocity = 0f;
		protected int m_CurrentFootstepSoundIndex = 0;
		protected Vector3 m_InitialScale;
		protected Vector3 m_InitialPosition;
		protected bool m_IsDashing = false;
		protected float m_LastDashTime = -999f;
		protected int m_RemainingAirJumps = 0;
		protected float m_BaseRunSpeed = 0f;
		protected float m_BaseMaxRunSpeed = 0f;

		#endregion

		#region Properties

		public override float MaxRunSpeed
		{
			get
			{
				return m_MaxRunSpeed;
			}
		}

		public override float RunSmoothTime
		{
			get
			{
				return m_RunSmoothTime;
			}
		}

		public override float RunSpeed
		{
			get
			{
				return m_RunSpeed;
			}
		}

		public override float WalkSpeed
		{
			get
			{
				return m_WalkSpeed;
			}
		}

		public override float JumpStrength
		{
			get
			{
				return m_JumpStrength;
			}
		}

		public int RemainingAirJumps
		{
			get
			{
				return m_RemainingAirJumps;
			}
		}

		public int MaxAirJumps
		{
			get
			{
				return m_MaxAirJumps;
			}
		}

		public float DashCooldown
		{
			get
			{
				return m_DashCooldown;
			}
		}

		public float DashCooldownRemaining
		{
			get
			{
				return Mathf.Max ( 0f, ( m_LastDashTime + m_DashCooldown ) - Time.time );
			}
		}

		public float DashCooldownNormalized
		{
			get
			{
				if ( m_DashCooldown <= 0f )
				{
					return 0f;
				}

				return DashCooldownRemaining / m_DashCooldown;
			}
		}

		public bool CanDash
		{
			get
			{
				return !IsDead.Value && !m_IsDashing && DashCooldownRemaining <= 0f;
			}
		}
		public override Vector2 Speed
		{
			get
			{
				return m_Speed;
			}
		}

		public override string[] Actions
		{
			get
			{
				return m_Actions;
			}
		}

		public override string CurrentAction
		{
			get
			{
				return m_Actions [ m_CurrentActionIndex ];
			}
		}

		public override int CurrentActionIndex
		{
			get
			{
				return m_CurrentActionIndex;
			}
		}

		public override GroundCheck GroundCheck
		{
			get
			{
				return m_GroundCheck;
			}
		}

		public override Rigidbody2D Rigidbody2D
		{
			get
			{
				return m_Rigidbody2D;
			}
		}

		public override Collider2D Collider2D
		{
			get
			{
				return m_Collider2D;
			}
		}

		public override Animator Animator
		{
			get
			{
				return m_Animator;
			}
		}

		public override ParticleSystem RunParticleSystem
		{
			get
			{
				return m_RunParticleSystem;
			}
		}

		public override ParticleSystem JumpParticleSystem
		{
			get
			{
				return m_JumpParticleSystem;
			}
		}

		public override ParticleSystem WaterParticleSystem
		{
			get
			{
				return m_WaterParticleSystem;
			}
		}

		public override ParticleSystem BloodParticleSystem
		{
			get
			{
				return m_BloodParticleSystem;
			}
		}

		public override Skeleton Skeleton
		{
			get
			{
				return m_Skeleton;
			}
		}

        public override bool ClosingEye
		{
			get
			{
				return m_ClosingEye;
			}
		}

		public override bool Guard
		{
			get
			{
				return m_Guard;
			}
		}

		public override bool Block
		{
			get
			{
				return m_Block;
			}
		}

		public override AudioSource Audio
		{
			get
			{
				return m_MainAudioSource;
			}
		}

		#endregion

		#region MonoBehaviour Messages

		void Awake ()
		{
			m_InitialPosition = transform.position;
			m_InitialScale = transform.localScale;
			m_GroundCheck.OnGrounded += GroundCheck_OnGrounded;
			m_Skeleton.OnActiveChanged += Skeleton_OnActiveChanged;
            IsDead = new Property<bool>(false);
			m_ClosingEye = false;
			m_Guard = false;
			m_Block = false;
			m_IsDashing = false;
			m_CurrentFootstepSoundIndex = 0;
			m_BaseRunSpeed = m_RunSpeed;
			m_BaseMaxRunSpeed = m_MaxRunSpeed;
			GameManager.OnReset += GameManager_OnReset;
			GameManager.OnScoreChanged += GameManager_OnScoreChanged;
		}

		void OnDestroy ()
		{
			GameManager.OnReset -= GameManager_OnReset;
			GameManager.OnScoreChanged -= GameManager_OnScoreChanged;
		}

		void Update ()
		{
			GameManager gameManager = GameManager.Singleton;
			if ( gameManager == null || !gameManager.gameStarted || !gameManager.gameRunning )
			{
				return;
			}

			RefreshAirJumpCount ();

			if ( transform.position.y < 0f )
			{
				Die ();
			}

			// Speed
			m_Speed = new Vector2 ( Mathf.Abs ( m_Rigidbody2D.linearVelocity.x ), Mathf.Abs ( m_Rigidbody2D.linearVelocity.y ) );

			// Speed Calculations
			m_CurrentRunSpeed = m_RunSpeed;
			if ( m_Speed.x >= m_RunSpeed )
			{
				m_CurrentRunSpeed = Mathf.SmoothDamp ( m_Speed.x, m_MaxRunSpeed, ref m_CurrentSmoothVelocity, m_RunSmoothTime );
			}

			// Input Processing
			Move ( CrossPlatformInputManager.GetAxis ( "Horizontal" ) );
			if ( CrossPlatformInputManager.GetButtonDown ( "Jump" ) )
			{
				Jump ();
			}
			if ( Input.GetKeyDown ( KeyCode.LeftShift ) )
			{
				Dash ();
			}
			if ( IsDead.Value && !m_ClosingEye )
			{
				StartCoroutine ( CloseEye () );
			}
			if ( CrossPlatformInputManager.GetButtonDown ( "Guard" ) )
			{
				m_Guard = !m_Guard;
			}
			if ( m_Guard )
			{
				if ( CrossPlatformInputManager.GetButtonDown ( "Fire" ) )
				{
					m_Animator.SetTrigger ( m_Actions [ m_CurrentActionIndex ] );
					if ( m_CurrentActionIndex < m_Actions.Length - 1 )
					{
						m_CurrentActionIndex++;
					}
					else
					{
						m_CurrentActionIndex = 0;
					}
				}
			}

			if ( Input.GetButtonDown ( "Roll" ) )
			{
				Vector2 force = new Vector2 ( 0f, 0f );
				if ( transform.localScale.z > 0f )
				{
					force.x = m_RollForce;
				}
				else if ( transform.localScale.z < 0f )
				{
					force.x = -m_RollForce;
				}
				m_Rigidbody2D.AddForce ( force );
			}
		}

		void LateUpdate ()
		{
			if ( m_Animator == null || m_Rigidbody2D == null || m_GroundCheck == null || IsDead == null )
			{
				return;
			}

			m_Animator.SetFloat ( "Speed", m_Speed.x );
			m_Animator.SetFloat ( "VelocityX", Mathf.Abs ( m_Rigidbody2D.linearVelocity.x ) );
			m_Animator.SetFloat ( "VelocityY", m_Rigidbody2D.linearVelocity.y );
			m_Animator.SetBool ( "IsGrounded", m_GroundCheck.IsGrounded );
			m_Animator.SetBool ( "IsDead", IsDead.Value );
			m_Animator.SetBool ( "Block", m_Block );
			m_Animator.SetBool ( "Guard", m_Guard );
			if ( Input.GetButtonDown ( "Roll" ) )
			{
				m_Animator.SetTrigger ( "Roll" );
			}
		}

		//		void OnCollisionEnter2D ( Collision2D collision2D )
		//		{
		//			bool isGround = collision2D.collider.CompareTag ( GroundCheck.GROUND_TAG );
		//			if ( isGround && !m_IsDead )
		//			{
		//				bool isBottom = false;
		//				for ( int i = 0; i < collision2D.contacts.Length; i++ )
		//				{
		//					if ( !isBottom )
		//					{
		//						isBottom = collision2D.contacts [ i ].normal.y == 1;
		//					}
		//					else
		//					{
		//						break;
		//					}
		//				}
		//				if ( isBottom )
		//				{
		//					m_JumpParticleSystem.Play ();
		//				}
		//			}
		//		}

		#endregion

		#region Private Methods

		IEnumerator CloseEye ()
		{
			m_ClosingEye = true;
			yield return new WaitForSeconds ( 0.6f );
			while ( m_Skeleton.RightEye.localScale.y > 0f )
			{
				if ( m_Skeleton.RightEye.localScale.y > 0f )
				{
					Vector3 scale = m_Skeleton.RightEye.localScale;
					scale.y -= 0.1f;
					m_Skeleton.RightEye.localScale = scale;
				}
				if ( m_Skeleton.LeftEye.localScale.y > 0f )
				{
					Vector3 scale = m_Skeleton.LeftEye.localScale;
					scale.y -= 0.1f;
					m_Skeleton.LeftEye.localScale = scale;
				}
				yield return new WaitForSeconds ( 0.05f );
			}
		}


		IEnumerator DashRoutine ( float direction )
		{
			m_IsDashing = true;
			m_LastDashTime = Time.time;

			Vector3 originalScale = transform.localScale;
			Vector3 dashScale = originalScale;
			dashScale.x = Mathf.Sign ( direction ) * Mathf.Abs ( originalScale.x ) * m_DashStretchX;
			dashScale.y = originalScale.y * m_DashStretchY;
			transform.localScale = dashScale;

			PlayDashParticleSystem ();
			PlayDashSound ();

			Vector2 velocity = m_Rigidbody2D.linearVelocity;
			velocity.x = m_DashSpeed * direction;
			m_Rigidbody2D.linearVelocity = velocity;

			yield return new WaitForSeconds ( m_DashDuration );

			transform.localScale = originalScale;
			m_IsDashing = false;
		}

		void RefreshAirJumpCount ()
		{
			if ( m_GroundCheck != null && m_GroundCheck.IsGrounded )
			{
				m_RemainingAirJumps = m_MaxAirJumps;
			}
		}

		void PerformJump ( float jumpStrength )
		{
			Vector2 velocity = m_Rigidbody2D.linearVelocity;
			velocity.y = jumpStrength;
			m_Rigidbody2D.linearVelocity = velocity;
			m_Animator.ResetTrigger ( "Jump" );
			m_Animator.SetTrigger ( "Jump" );
			m_JumpParticleSystem.Play ();
			AudioManager.Singleton.PlayJumpSound ( m_JumpAndGroundedAudioSource );
		}

		void ApplyDifficultyScaling ( float score )
		{
			if ( !m_DifficultyScalingEnabled || m_DifficultyFullMeters <= 0f )
			{
				m_RunSpeed = m_BaseRunSpeed;
				m_MaxRunSpeed = m_BaseMaxRunSpeed;
				return;
			}

			float scoreMeters = score * Extensions.modifier;
			float progress = Mathf.Clamp01 ( scoreMeters / m_DifficultyFullMeters );
			m_RunSpeed = m_BaseRunSpeed + ( m_DifficultyRunSpeedBoost * progress );
			m_MaxRunSpeed = m_BaseMaxRunSpeed + ( m_DifficultyMaxRunSpeedBoost * progress );
		}

		void PlayDashParticleSystem ()
		{
			if ( m_DashParticleSystem == null )
			{
				return;
			}

			Vector3 particlePosition = transform.position;
			particlePosition.x -= Mathf.Sign ( transform.localScale.x ) * 0.5f;
			particlePosition.y += 0.1f;
			m_DashParticleSystem.transform.position = particlePosition;

			m_DashParticleSystem.Stop ( true, ParticleSystemStopBehavior.StopEmittingAndClear );
			m_DashParticleSystem.Emit ( 30 );
		}

		void PlayDashSound ()
		{
			if ( AudioManager.Singleton == null )
			{
				return;
			}

			AudioManager.Singleton.PlayDashSound ( m_MainAudioSource );
		}

		#endregion

		#region Public Methods

		public virtual void PlayFootstepSound ()
		{
			if ( m_GroundCheck.IsGrounded )
			{
				AudioManager.Singleton.PlayFootstepSound ( m_FootstepAudioSource, ref m_CurrentFootstepSoundIndex );
			}
		}

		public override void Move ( float horizontalAxis )
		{
			if ( !IsDead.Value )
			{
				if ( m_IsDashing )
				{
					return;
				}

				float speed = m_CurrentRunSpeed;
//				if ( CrossPlatformInputManager.GetButton ( "Walk" ) )
//				{
//					speed = m_WalkSpeed;
				//				}
				Vector2 velocity = m_Rigidbody2D.linearVelocity;
				velocity.x = speed * horizontalAxis;
				m_Rigidbody2D.linearVelocity = velocity;
				if ( horizontalAxis > 0f )
				{
					Vector3 scale = transform.localScale;
					scale.x = Mathf.Sign ( horizontalAxis );
					transform.localScale = scale;
				}
				else if ( horizontalAxis < 0f )
				{
					Vector3 scale = transform.localScale;
					scale.x = Mathf.Sign ( horizontalAxis );
					transform.localScale = scale;
				}
			}
		}

		public virtual void Dash ()
		{
			if ( IsDead.Value || m_IsDashing )
			{
				return;
			}

			if ( Time.time < m_LastDashTime + m_DashCooldown )
			{
				return;
			}

			float direction = transform.localScale.x >= 0f ? 1f : -1f;
			StartCoroutine ( DashRoutine ( direction ) );
		}

		public override void Jump ()
		{
			if ( IsDead.Value )
			{
				return;
			}

			if ( m_GroundCheck.IsGrounded )
			{
				PerformJump ( m_JumpStrength );
				return;
			}

			if ( m_RemainingAirJumps <= 0 )
			{
				return;
			}

			m_RemainingAirJumps--;
			PerformJump ( m_AirJumpStrength );
		}

		public override void Die ()
		{
			Die ( false );
		}

		public override void Die ( bool blood )
		{
			if ( !IsDead.Value )
			{
                IsDead.Value = true;
				m_Skeleton.SetActive ( true, m_Rigidbody2D.linearVelocity );
				if ( blood )
				{
					ParticleSystem particle = Instantiate<ParticleSystem> (
						                          m_BloodParticleSystem,
						                          transform.position,
						                          Quaternion.identity );
					Destroy ( particle.gameObject, particle.main.duration );
				}
				CameraController.Singleton.fastMove = true;
			}
		}

		public override void EmitRunParticle ()
		{
			if ( !IsDead.Value )
			{
				m_RunParticleSystem.Emit ( 1 );
			}
		}

		public override void Reset ()
		{
            IsDead.Value = false;
			m_ClosingEye = false;
			m_Guard = false;
			m_Block = false;
			m_IsDashing = false;
			m_CurrentFootstepSoundIndex = 0;
			m_RemainingAirJumps = m_MaxAirJumps;
			transform.localScale = m_InitialScale;
			m_Rigidbody2D.linearVelocity = Vector2.zero;
			m_Skeleton.SetActive ( false, m_Rigidbody2D.linearVelocity );
		}

		#endregion

		#region Events

		void GameManager_OnReset ()
		{
			ApplyDifficultyScaling ( 0f );
			transform.position = m_InitialPosition;
			Reset ();
		}

		void GameManager_OnScoreChanged ( float newScore, float highScore, float lastScore )
		{
			ApplyDifficultyScaling ( newScore );
		}

		void Skeleton_OnActiveChanged ( bool active )
		{
			m_Animator.enabled = !active;
			m_Collider2D.enabled = !active;
			m_Rigidbody2D.simulated = !active;
		}

		void GroundCheck_OnGrounded ()
		{
			m_RemainingAirJumps = m_MaxAirJumps;

			if ( !IsDead.Value )
			{
				m_JumpParticleSystem.Play ();
				AudioManager.Singleton.PlayGroundedSound ( m_JumpAndGroundedAudioSource );
			}
		}

		#endregion

		[System.Serializable]
		public class CharacterDeadEvent : UnityEvent
		{

		}

	}

}









