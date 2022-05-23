using UnityEngine;

namespace C0
{
	public class TriggerInfo : MonoBehaviour
	{
		private GameSettings settings;
		private Player player;

		public Collider2D Ground;
		public Collider2D Climb;
		public Collider2D WallTop;
		public Collider2D WallMid;
		public Collider2D WallLow;

		private Bounds groundBounds;
		public Bounds GroundBounds
		{
			get
			{
				Vector3 playerPosition = player.Position;
				groundBounds.center = playerPosition + 0.025f * Vector3.down;

				return groundBounds;
			}
		}

		private Bounds climbBounds;
		public Bounds ClimbBounds
		{
			get
			{
				Vector3 playerPosition = player.Position;
				climbBounds.center = playerPosition + 0.6f * Vector3.up;

				return climbBounds;
			}
		}

		private Bounds wallTopBounds;
		public Bounds WallTopBounds
		{
			get
			{
				Vector3 localScale = new Vector3(player.Facing, 1, 1);
				Vector3 playerPosition = player.Position;

				wallTopBounds.center = playerPosition + Vector3.Scale(localScale, TopOffset);

				return wallTopBounds;
			}
		}

		private Bounds wallMidBounds;
		public Bounds WallMidBounds
		{
			get
			{
				Vector3 localScale = new Vector3(player.Facing, 1, 1);
				Vector3 playerPosition = player.Position;

				wallMidBounds.center = playerPosition + Vector3.Scale(localScale, MidOffset);

				return wallMidBounds;
			}
		}

		private Bounds wallLowBounds;
		public Bounds WallLowBounds
		{
			get
			{
				Vector3 localScale = new Vector3(player.Facing, 1, 1);
				Vector3 playerPosition = player.Position;

				wallLowBounds.center = playerPosition + Vector3.Scale(localScale, LowOffset);

				return wallLowBounds;
			}
		}

		public Vector3 TopOffset { get; set; }
		public Vector3 MidOffset { get; set; }
		public Vector3 LowOffset { get; set; }

		public bool Ledge => !WallTop && WallMid;
		public bool WallSlide => !Ground && WallTop && WallMid && WallLow;

		void Awake()
		{
			settings = Resources.Load<GameSettings>("Settings/GameSettings");
			player = GetComponentInParent<Player>();
		}

		void Start()
		{
			groundBounds = new Bounds(Vector3.zero, new Vector2(player.Bounds.size.x - 0.02f, 0.05f));
			climbBounds = new Bounds(Vector3.zero, new Vector2(player.Bounds.size.x - 0.02f, 0.4f));
			wallTopBounds = new Bounds(Vector3.zero, settings.WallTriggerSize);
			wallMidBounds = new Bounds(Vector3.zero, settings.WallTriggerSize);
			wallLowBounds = new Bounds(Vector3.zero, settings.WallTriggerSize);

			float horizontalOffset = player.Bounds.extents.x + 0.05f;

			TopOffset = new Vector3(horizontalOffset, 1.1f * player.Bounds.size.y);
			MidOffset = new Vector3(horizontalOffset, 0.8f * player.Bounds.size.y);
			LowOffset = new Vector3(horizontalOffset, 0.1f * player.Bounds.size.y);
		}

		public void ResetTriggers()
		{
			Ground = null;
			Climb = null;
			WallTop = null;
			WallMid = null;
			WallLow = null;
		}
	}
}
