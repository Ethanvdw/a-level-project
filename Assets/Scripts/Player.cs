using UnityEngine;

[RequireComponent(
    typeof(Controller2D))] // Makes Controller2D a required component. Useful in preventing unintended behaviour and syntax errors.
public class Player : MonoBehaviour {
    private readonly float gravity = -20;
    private readonly float moveSpeed = 6; // Set movement speed for the player.
    private Controller2D controller; // Import the Controller2D class.
    private Vector3 velocity; // Store the player velocity as a vector.

    private void Start() {
        // Code to be run the scene starts.
        controller = GetComponent<Controller2D>(); // Instantiate a Controller2D as "controller".
    }

    private void Update() {
        // On every frame.

        var input = new Vector2(Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")); // Store player input as a vector.

        velocity.x = input.x * moveSpeed; // Add player horizontal movement to their velocity.
        //Time.deltaTime is the time since the previous frame. Multiplying by this value allows physics not to be dependant on frame rate.
        velocity.y += gravity * Time.deltaTime; // Drag player downwards by gravity.
        controller.Move(velocity * Time.deltaTime); // Move the player according to their velocity.
    }
}