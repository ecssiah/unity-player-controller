using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace C0
{
	public class Player : MonoBehaviour
	{
		public float Facing => transform.localScale.x;
		public Vector2 Position => transform.position;
		public Vector2 Velocity => rigidBody2D.velocity;

		public PlayerState CurrentState { get; private set; }
		public PlayerStateType PlayerState;

		private Dictionary<PlayerStateType, PlayerState> playerStates;

		public InputInfo InputInfo;
		public TriggerInfo TriggerInfo;

		private GameSettings settings;

		private Animator animator;
		private Collider2D bodyCollider;
		private Rigidbody2D rigidBody2D;

		private GameObject cameraTarget;

		private LayerMask surfaceLayerMask;
		private LayerMask climbableLayerMask;

		public void AwakeManaged()
		{
			settings = Resources.Load<GameSettings>("Settings/GameSettings");

			animator = GetComponent<Animator>();
			bodyCollider = GetComponent<Collider2D>();
			rigidBody2D = GetComponent<Rigidbody2D>();

			cameraTarget = transform.Find("Target").gameObject;

			surfaceLayerMask = LayerMask.GetMask("Surface");
			climbableLayerMask = LayerMask.GetMask("Climbable");
		}

		public void StartManaged()
		{
			playerStates = new Dictionary<PlayerStateType, PlayerState>
			{
				[PlayerStateType.Move] = new MoveState(this, settings),
				[PlayerStateType.Duck] = new DuckState(this, settings),
				[PlayerStateType.Hang] = new HangState(this, settings),
				[PlayerStateType.Climb] = new ClimbState(this, settings),
				[PlayerStateType.ClimbLedge] = new ClimbLedgeState(this, settings),
				[PlayerStateType.WallSlide] = new WallSlideState(this, settings),
			};

			SetState(PlayerStateType.Move);
			SetPosition(settings.StartPosition);
		}

		public void UpdateManaged()
		{
			CurrentState.Update();

			PlayerState = CurrentState.Type;
		}

		public void FixedUpdateManaged()
		{
			CurrentState.FixedUpdate();
		}

		public void SetState(PlayerStateType stateType)
		{
			CurrentState = playerStates[stateType];

			CurrentState.Init();
		}

		public void SetFacing(float facing)
		{
			transform.localScale = new Vector3(facing, transform.localScale.y, transform.localScale.z);
		}

		public void SetPosition(float x, float y)
		{
			transform.position = new Vector2(x, y);
		}

		public void SetPosition(Vector2 position)
		{
			SetPosition(position.x, position.y);
		}

		public void SetVelocity(float x, float y)
		{
			if (x == 0)
			{
				CurrentState.ResetDamping();
			}
			else if (Mathf.Abs(x) < settings.MinMoveSpeed)
			{
				x = 0;
				CurrentState.ResetDamping();
			}

			rigidBody2D.velocity = new Vector2(x, Mathf.Max(y, settings.MaxFallSpeed));
		}

		public void SetVelocity(Vector2 velocity)
		{
			SetVelocity(velocity.x, velocity.y);
		}

		public void SetGravityScale(float gravityScale)
		{
			rigidBody2D.gravityScale = gravityScale;
		}

		public void RunClimbLedgeAction()
		{
			StartCoroutine(ClimbLedgeCoroutine());
		}

		private IEnumerator ClimbLedgeCoroutine()
		{
			yield return null;

			while (animator.GetCurrentAnimatorStateInfo(0).normalizedTime <= 1)
			{
				cameraTarget.transform.localPosition = Vector2.Lerp(
					Vector2.zero,
					settings.ClimbLedgeOffset,
					animator.GetCurrentAnimatorStateInfo(0).normalizedTime
				);

				yield return null;
			}

			cameraTarget.transform.localPosition = Vector2.zero;

			SetPosition(Position + transform.localScale * settings.ClimbLedgeOffset);
			SetVelocity(Vector2.zero);
			SetState(PlayerStateType.Move);
		}

		public void UpdateTriggers()
		{
			TriggerInfo.Reset();

			UpdateGroundTrigger();
			UpdateClimbTrigger();
			UpdateWallTriggers();
		}

		private void UpdateGroundTrigger()
		{
			TriggerInfo.GroundBounds = new Bounds(
				transform.position + 0.025f * Vector3.down,
				new Vector2(bodyCollider.bounds.size.x - 0.02f, 0.05f)
			);

			TriggerInfo.Ground = Physics2D.OverlapBox
			(
				TriggerInfo.GroundBounds.center, TriggerInfo.GroundBounds.size, 0f, surfaceLayerMask
			);
		}

		private void UpdateClimbTrigger()
		{
			TriggerInfo.ClimbBounds = new Bounds(
				transform.position + 0.6f * Vector3.up,
				new Vector2(bodyCollider.bounds.size.x - 0.02f, 0.4f)
			);

			TriggerInfo.Climb = Physics2D.OverlapBox
			(
				TriggerInfo.ClimbBounds.center, TriggerInfo.ClimbBounds.size, 0f, climbableLayerMask
			);
		}

		private void UpdateWallTriggers()
		{
			float horizontalOffset = Facing * (bodyCollider.bounds.extents.x + 0.05f);

			TriggerInfo.WallTopBounds = new Bounds(
				transform.position + new Vector3(horizontalOffset, 1.1f * bodyCollider.bounds.size.y),
				settings.WallTriggerSize
			);

			TriggerInfo.WallTop = Physics2D.OverlapBox
			(
				TriggerInfo.WallTopBounds.center, TriggerInfo.WallTopBounds.size, 0f, surfaceLayerMask
			);

			TriggerInfo.WallMidBounds = new Bounds(
				transform.position + new Vector3(horizontalOffset, 0.8f * bodyCollider.bounds.size.y),
				settings.WallTriggerSize
			);

			TriggerInfo.WallMid = Physics2D.OverlapBox
			(
				TriggerInfo.WallMidBounds.center, TriggerInfo.WallMidBounds.size, 0f, surfaceLayerMask
			);

			TriggerInfo.WallLowBounds = new Bounds(
				transform.position + new Vector3(horizontalOffset, 0.1f * bodyCollider.bounds.size.y),
				settings.WallTriggerSize
			);

			TriggerInfo.WallLow = Physics2D.OverlapBox
			(
				TriggerInfo.WallLowBounds.center, TriggerInfo.WallLowBounds.size, 0f, surfaceLayerMask
			);
		}

		public void SetAnimation(string stateName)
		{
			SetAnimationSpeed(1);

			animator.Play($"Base Layer.Player-{stateName}");
		}

		public void SetAnimationSpeed(float speed)
		{
			animator.speed = speed;
		}

		public void UpdateAnimation()
		{
			if (rigidBody2D.velocity.y > settings.MinJumpSpeed)
			{
				SetAnimation("Jump");
			}
			else if (rigidBody2D.velocity.y < -settings.MinFallSpeed)
			{
				SetAnimation("Fall");
			}
			else if (InputInfo.Direction.x != 0 && Mathf.Abs(rigidBody2D.velocity.x) > settings.MinRunSpeed)
			{
				SetAnimation("Run");
			}
			else
			{
				SetAnimation("Idle");
			}
		}

		public void UpdateFacing()
		{
			if (Facing != 1 && rigidBody2D.velocity.x > settings.MinRunSpeed)
			{
				SetFacing(1);
			}
			else if (Facing != -1 && rigidBody2D.velocity.x < -settings.MinRunSpeed)
			{
				SetFacing(-1);
			}
		}

		void OnDrawGizmos()
		{
			if (Application.isPlaying)
			{
				Gizmos.color = new Color(1, 0, 1, 0.4f);

				Gizmos.DrawWireCube(TriggerInfo.GroundBounds.center, TriggerInfo.GroundBounds.size);
				Gizmos.DrawWireCube(TriggerInfo.ClimbBounds.center, TriggerInfo.ClimbBounds.size);

				Gizmos.DrawWireCube(TriggerInfo.WallTopBounds.center, TriggerInfo.WallTopBounds.size);
				Gizmos.DrawWireCube(TriggerInfo.WallMidBounds.center, TriggerInfo.WallMidBounds.size);
				Gizmos.DrawWireCube(TriggerInfo.WallLowBounds.center, TriggerInfo.WallLowBounds.size);
			}
		}
	}
}