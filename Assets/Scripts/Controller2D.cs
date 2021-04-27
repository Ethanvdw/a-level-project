using UnityEngine;

[RequireComponent (typeof (BoxCollider2D))]
public class Controller2D : MonoBehaviour {

	public LayerMask collisionMask;

	const float SkinWidth = .015f;
	public int horizontalRayCount = 4;
	public int verticalRayCount = 4;

	float maxClimbAngle = 80;
	float maxDescendAngle = 80;

	float _horizontalRaySpacing;
	float _verticalRaySpacing;

	BoxCollider2D _collider;
	RaycastOrigins _raycastOrigins;
	public CollisionInfo Collisions;

	void Start() {
		_collider = GetComponent<BoxCollider2D> ();
		CalculateRaySpacing ();
	}

	public void Move(Vector3 velocity) {
		UpdateRaycastOrigins ();
		Collisions.Reset ();
		Collisions.VelocityOld = velocity;

		if (velocity.y < 0) {
			DescendSlope(ref velocity);
		}
		if (velocity.x != 0) {
			HorizontalCollisions (ref velocity);
		}
		if (velocity.y != 0) {
			VerticalCollisions (ref velocity);
		}

		transform.Translate (velocity);
	}

	void HorizontalCollisions(ref Vector3 velocity) {
		float directionX = Mathf.Sign (velocity.x);
		float rayLength = Mathf.Abs (velocity.x) + SkinWidth;
		
		for (int i = 0; i < horizontalRayCount; i ++) {
			Vector2 rayOrigin = (directionX == -1)?_raycastOrigins.BottomLeft:_raycastOrigins.BottomRight;
			rayOrigin += Vector2.up * (_horizontalRaySpacing * i);
			RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

			Debug.DrawRay(rayOrigin, Vector2.right * (directionX * rayLength),Color.red);

			if (hit) {

				float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);

				if (i == 0 && slopeAngle <= maxClimbAngle) {
					if (Collisions.DescendingSlope) {
						Collisions.DescendingSlope = false;
						velocity = Collisions.VelocityOld;
					}
					float distanceToSlopeStart = 0;
					if (slopeAngle != Collisions.SlopeAngleOld) {
						distanceToSlopeStart = hit.distance-SkinWidth;
						velocity.x -= distanceToSlopeStart * directionX;
					}
					ClimbSlope(ref velocity, slopeAngle);
					velocity.x += distanceToSlopeStart * directionX;
				}

				if (!Collisions.ClimbingSlope || slopeAngle > maxClimbAngle) {
					velocity.x = (hit.distance - SkinWidth) * directionX;
					rayLength = hit.distance;

					if (Collisions.ClimbingSlope) {
						velocity.y = Mathf.Tan(Collisions.SlopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x);
					}

					Collisions.Left = directionX == -1;
					Collisions.Right = directionX == 1;
				}
			}
		}
	}
	
	void VerticalCollisions(ref Vector3 velocity) {
		float directionY = Mathf.Sign (velocity.y);
		float rayLength = Mathf.Abs (velocity.y) + SkinWidth;

		for (int i = 0; i < verticalRayCount; i ++) {
			Vector2 rayOrigin = (directionY == -1)?_raycastOrigins.BottomLeft:_raycastOrigins.TopLeft;
			rayOrigin += Vector2.right * (_verticalRaySpacing * i + velocity.x);
			RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);

			Debug.DrawRay(rayOrigin, Vector2.up * (directionY * rayLength),Color.red);

			if (hit) {
				velocity.y = (hit.distance - SkinWidth) * directionY;
				rayLength = hit.distance;

				if (Collisions.ClimbingSlope) {
					velocity.x = velocity.y / Mathf.Tan(Collisions.SlopeAngle * Mathf.Deg2Rad) * Mathf.Sign(velocity.x);
				}

				Collisions.Below = directionY == -1;
				Collisions.Above = directionY == 1;
			}
		}

		if (Collisions.ClimbingSlope) {
			float directionX = Mathf.Sign(velocity.x);
			rayLength = Mathf.Abs(velocity.x) + SkinWidth;
			Vector2 rayOrigin = ((directionX == -1)?_raycastOrigins.BottomLeft:_raycastOrigins.BottomRight) + Vector2.up * velocity.y;
			RaycastHit2D hit = Physics2D.Raycast(rayOrigin,Vector2.right * directionX,rayLength,collisionMask);

			if (hit) {
				float slopeAngle = Vector2.Angle(hit.normal,Vector2.up);
				if (slopeAngle != Collisions.SlopeAngle) {
					velocity.x = (hit.distance - SkinWidth) * directionX;
					Collisions.SlopeAngle = slopeAngle;
				}
			}
		}
	}

	void ClimbSlope(ref Vector3 velocity, float slopeAngle) {
		float moveDistance = Mathf.Abs (velocity.x);
		float climbVelocityY = Mathf.Sin (slopeAngle * Mathf.Deg2Rad) * moveDistance;

		if (velocity.y <= climbVelocityY) {
			velocity.y = climbVelocityY;
			velocity.x = Mathf.Cos (slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign (velocity.x);
			Collisions.Below = true;
			Collisions.ClimbingSlope = true;
			Collisions.SlopeAngle = slopeAngle;
		}
	}

	void DescendSlope(ref Vector3 velocity) {
		float directionX = Mathf.Sign (velocity.x);
		Vector2 rayOrigin = (directionX == -1) ? _raycastOrigins.BottomRight : _raycastOrigins.BottomLeft;
		RaycastHit2D hit = Physics2D.Raycast (rayOrigin, -Vector2.up, Mathf.Infinity, collisionMask);

		if (hit) {
			float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
			if (slopeAngle != 0 && slopeAngle <= maxDescendAngle) {
				if (Mathf.Sign(hit.normal.x) == directionX) {
					if (hit.distance - SkinWidth <= Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x)) {
						float moveDistance = Mathf.Abs(velocity.x);
						float descendVelocityY = Mathf.Sin (slopeAngle * Mathf.Deg2Rad) * moveDistance;
						velocity.x = Mathf.Cos (slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign (velocity.x);
						velocity.y -= descendVelocityY;

						Collisions.SlopeAngle = slopeAngle;
						Collisions.DescendingSlope = true;
						Collisions.Below = true;
					}
				}
			}
		}
	}

	void UpdateRaycastOrigins() {
		Bounds bounds = _collider.bounds;
		bounds.Expand (SkinWidth * -2);

		_raycastOrigins.BottomLeft = new Vector2 (bounds.min.x, bounds.min.y);
		_raycastOrigins.BottomRight = new Vector2 (bounds.max.x, bounds.min.y);
		_raycastOrigins.TopLeft = new Vector2 (bounds.min.x, bounds.max.y);
		_raycastOrigins.TopRight = new Vector2 (bounds.max.x, bounds.max.y);
	}

	void CalculateRaySpacing() {
		Bounds bounds = _collider.bounds;
		bounds.Expand (SkinWidth * -2);

		horizontalRayCount = Mathf.Clamp (horizontalRayCount, 2, int.MaxValue);
		verticalRayCount = Mathf.Clamp (verticalRayCount, 2, int.MaxValue);

		_horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
		_verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
	}

	struct RaycastOrigins {
		public Vector2 TopLeft, TopRight;
		public Vector2 BottomLeft, BottomRight;
	}

	public struct CollisionInfo {
		public bool Above, Below;
		public bool Left, Right;

		public bool ClimbingSlope;
		public bool DescendingSlope;
		public float SlopeAngle, SlopeAngleOld;
		public Vector3 VelocityOld;

		public void Reset() {
			Above = Below = false;
			Left = Right = false;
			ClimbingSlope = false;
			DescendingSlope = false;

			SlopeAngleOld = SlopeAngle;
			SlopeAngle = 0;
		}
	}
}
