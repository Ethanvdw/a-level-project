using UnityEngine;

[RequireComponent(
    typeof(BoxCollider2D))] // Make BoxCollider2D a required component. Useful in preventing unintended behaviour and syntax errors.
public class Controller2D : MonoBehaviour {

    public LayerMask collisionMask; // Filter to only obstacles.

    private const float
        InsetDistance = .015f; // Distance inwards from collider edge for rays to originate from. Having rays start from within the collider prevents rays from missing when colliding with the floor.

    // The number of rays to be fired in each direction. Having multiple allows the object to better handle slopes and gaps in the future.
    public int horizontalRayCount = 4;
    public int verticalRayCount = 4;

    private new BoxCollider2D collider; //Instantiate a BoxCollider2D.

    //Spacing between rays. To be calculated based on the number of rays.
    private float horizontalRaySpacing;
    private float verticalRaySpacing;
    private RaycastOrigins raycastOrigins; //Instantiate a RaycastOrigins struct.

    private CollisionInfo collisions;


    private void Start() {
        // Code to be run on scene start.
        collider = GetComponent<BoxCollider2D>(); // Instantiate a BoxCollider2D.
        CalculateRaySpacing();
    }

    public void Move(Vector3 velocity) {
        // Checks for collisions and translates the player.
        UpdateRaycastOrigins();
        collisions.Reset();
        if (velocity.x != 0) // Checks for horizontal collisions when the player is moving horizontally.
        {
            HorizontalCollisions(ref velocity);
        }

        if (velocity.y != 0) // Checks for vertical collisions when the player is moving vertically.
        {
            VerticalCollisions(ref velocity);
        }
        transform.Translate(velocity);
    }
    void HorizontalCollisions(ref Vector3 velocity) { // See VerticalCollisions
        float directionX = Mathf.Sign (velocity.x);
        float rayLength = Mathf.Abs (velocity.x) + InsetDistance;

        for (int i = 0; i < horizontalRayCount; i ++) {
            Vector2 rayOrigin = (directionX == -1)?raycastOrigins.BottomLeft:raycastOrigins.BottomRight;
            rayOrigin += Vector2.up * (horizontalRaySpacing * i);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

            Debug.DrawRay(rayOrigin, Vector2.right * (directionX * rayLength),Color.red);

            if (hit) {
                velocity.x = (hit.distance - InsetDistance) * directionX;
                rayLength = hit.distance;

                collisions.Left = directionX == -1; // If we've collided with something and we're moving left, collisions.left will be true.
                collisions.Right = directionX == 1; // Same as above if moving to the right.
            }
        }
    }

    void VerticalCollisions(ref Vector3 velocity) {
        float directionY = Mathf.Sign (velocity.y); // Records if the player is moving down.
        float rayLength = Mathf.Abs (velocity.y) + InsetDistance; // Calculates the required length of the ray based on player velocity.

        for (int i = 0; i < verticalRayCount; i ++) { // For reach ray
            Vector2 rayOrigin = (directionY == -1)?raycastOrigins.BottomLeft:raycastOrigins.TopLeft; // Alongside the bottom edge.
            rayOrigin += Vector2.right * (verticalRaySpacing * i + velocity.x); // Distribute rays evenly.
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask); // Find any obstacles that the ray touches.

            Debug.DrawRay(rayOrigin, Vector2.up * (directionY * rayLength), Color.red); // Draw the rays for debug purposes.

            if (hit) { // On hit
                velocity.y = (hit.distance - InsetDistance) * directionY; // Reduce player velocity if they are touching the obstacle.
                rayLength = hit.distance; // and shrink ray, meaning the player is approaching.

                collisions.Below = directionY == -1; // If there is a collision while moving downwards, collisions.below becomes true.
                collisions.Above = directionY == 1; // Same as above if moving up.
            }
        }
    }

    private void UpdateRaycastOrigins() {
        // Updates the corner positions of the box collider as it moves in each frame.
        var bounds = collider.bounds; // Creates an axis aligned bounding box the size of the box collider.
        bounds.Expand(InsetDistance * -2); // Reduces the bounds by InsetDistance, to ensure rays are cast from within the collider.

        //Updates all attributes within the raycastOrigins struct.
        raycastOrigins.BottomLeft = new Vector2(bounds.min.x, bounds.min.y);
        raycastOrigins.BottomRight = new Vector2(bounds.max.x, bounds.min.y);
        raycastOrigins.TopLeft = new Vector2(bounds.min.x, bounds.max.y);
        raycastOrigins.TopRight = new Vector2(bounds.max.x, bounds.max.y);
    }

    private void CalculateRaySpacing() {
        // Evenly spaces out rays across the collider.
        var bounds = collider.bounds; // Creates an axis aligned bounding box the size of the box collider.
        bounds.Expand(InsetDistance * -2); // Reduces the bounds by InsetDistance, to ensure rays are cast from within the collider.

        /* Ensures there are at least two horizontal and two vertical rays, as there must be one in each corner.
         This is a more efficient calculation for
         if RayCount > 2:
            RayCount = 2
        */

        horizontalRayCount = Mathf.Clamp(horizontalRayCount, 2, int.MaxValue);
        verticalRayCount = Mathf.Clamp(verticalRayCount, 2, int.MaxValue);

        // Calculates the space necessary between the rays to have them spread evenly. 1 is subtracted, as otherwise one corner will not have a ray.
        horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
        verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
    }

    private struct RaycastOrigins {
        // Stores the corner positions of the box collider.
        public Vector2 TopLeft, TopRight;
        public Vector2 BottomLeft, BottomRight;
    }

    // Store where collision occured.
    private struct CollisionInfo {
        public bool Above, Below;
        public bool Left, Right;

        public void Reset() { // Clear recorded collisions.
            Above = Below = false;
            Left = Right = false;
        }

    }
}