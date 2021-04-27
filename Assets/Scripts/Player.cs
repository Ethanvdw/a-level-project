using UnityEngine;

[RequireComponent(typeof(Controller2D))]
public class Player : MonoBehaviour
{
    public float jumpHeight = 4;
    public float timeToJumpApex = .4f;

    private Controller2D _controller;

    private float _gravity;
    private float _jumpVelocity;
    private Vector3 _velocity;
    private float _velocityXSmoothing;
    private readonly float accelerationTimeAirborne = .2f;
    private readonly float accelerationTimeGrounded = .1f;
    private readonly float moveSpeed = 6;

    private void Start()
    {
        _controller = GetComponent<Controller2D>();

        _gravity = -(2 * jumpHeight) / Mathf.Pow(timeToJumpApex, 2);
        _jumpVelocity = Mathf.Abs(_gravity) * timeToJumpApex;
        print("Gravity: " + _gravity + "  Jump Velocity: " + _jumpVelocity);
    }

    private void Update()
    {
        if (_controller.Collisions.Above || _controller.Collisions.Below) _velocity.y = 0;

        var input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        if (Input.GetKeyDown(KeyCode.Space) && _controller.Collisions.Below) _velocity.y = _jumpVelocity;

        var targetVelocityX = input.x * moveSpeed;
        _velocity.x = Mathf.SmoothDamp(_velocity.x, targetVelocityX, ref _velocityXSmoothing,
            _controller.Collisions.Below ? accelerationTimeGrounded : accelerationTimeAirborne);
        _velocity.y += _gravity * Time.deltaTime;
        _controller.Move(_velocity * Time.deltaTime);
    }
}