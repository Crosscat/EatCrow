using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(RaycastLauncher))]
public class Physics : MonoBehaviour
{
    public static bool Stopped = false;

    public static float JumpLeniency = .05f;

    private float _gravity = .4f;
    private const float _maxFall = 10;
    private const float _margin = .025f;
    private float _angleLeeway = 30;

    public Vector2 Velocity;
    public bool HasGravity;
    public bool DisableCollisions;

    public bool Grounded { get; private set; } = false;
    public bool Falling { get; private set; } = true;

    private Vector2 _inputDirection;
    private float _targetSpeedX;
    private float _targetSpeedY;
    private float _acceleration;
    private bool _onASlope = false;
    public RaycastLauncher RaycastLauncher { get; set; }
    private float _fallTime;

    public Collider2D Collider { get; set; }
    public Collider2D WallExitCollider { get; set; }

    private void Awake()
    {
        RaycastLauncher = GetComponent<RaycastLauncher>();
        Collider = RaycastLauncher.Collider;
    }

    public bool CanJump()
    {
        return (Grounded || _fallTime < JumpLeniency) && !_onASlope;
    }

    public void Jump(float power)
    {
        if (CanJump())
        {
            Velocity.y = power;
            Grounded = false;
        }
    }

    public void AddJump(float power)
    {
        if (Velocity.y > 0)
        {
            Velocity.y += power;
        }
    }

    public void ForceJump(float power)
    {
        Velocity.y = power;
        Grounded = false;
        Falling = false;
    }

    public void Move(Vector2 direction, float speed, float acceleration)
    {
        direction = direction.normalized;
        _inputDirection = direction;
        _acceleration = acceleration;
        _targetSpeedX = Mathf.Abs(direction.x * speed);
        _targetSpeedY = Mathf.Abs(direction.y * speed);
    }

    public void FixedUpdate()
    {
        if (Stopped) return;

        HorizontalMovement();
        VerticalMovement();
        transform.Translate(Velocity * Time.fixedDeltaTime);
    }

    private void Update()
    {
        _fallTime += Time.deltaTime;
    }

    private void VerticalMovement()
    {
        // apply gravity if airborne
        if (!Grounded && HasGravity)
        {
            Velocity.y = Mathf.Max(Velocity.y - _gravity, -_maxFall);
        }

        if (Velocity.y < 0)
        {
            Falling = true;
        }

        if (DisableCollisions) return;

        float rayDistance = Collider.bounds.extents.y + (Grounded ? .1f : Mathf.Abs(Velocity.y * Time.deltaTime));

        if (Grounded || Falling)
        {
            // check for collision with ground
            float hitDistance = Colliding(Vector2.down, rayDistance);
            if (hitDistance > 0)
            {
                Grounded = true;
                Falling = false;
                Velocity.y = 0;

                var (hit, _) = CollidingWithSlope(Vector2.down, .4f);
                _onASlope = hit > 0;
                if (_onASlope)
                {
                    // if on a steep slope, instead of aligning with ground, get pushed downwards
                    transform.Translate(Vector2.down * .05f);

                    // if inside slope after being pushed down, get pushed outwards
                    CheckHorizontalSlopes(Vector2.left);
                    CheckHorizontalSlopes(Vector2.right);
                }
                else
                {
                    // if not on steep slope, align with ground
                    transform.Translate(Vector2.down * (hitDistance - Collider.bounds.extents.y));
                }
            }
            else
            {
                if (Grounded)
                {
                    _fallTime = 0;
                }
                Grounded = false;
            }
        }
        else
        {
            // check for collision with ceiling
            float hitDistance = Colliding(Vector2.up, rayDistance);
            if (hitDistance > 0)
            {
                transform.Translate(Vector2.up * (hitDistance - Collider.bounds.extents.y));
                Velocity.y = 0;
            }
        }
    }

    private void HorizontalMovement()
    {
        var accelerationVector = new Vector2(_inputDirection.x, _inputDirection.y).normalized * _acceleration;
        var decelerationVector = new Vector2(Velocity.x, Velocity.y).normalized * -_acceleration;

        // apply acceleration/deceleration to the horizontal velocity based on whether input is being pressed
        if (_inputDirection.x != 0)
        {
            Velocity.x += accelerationVector.x;
            Velocity.x = Mathf.Clamp(Velocity.x, -_targetSpeedX, _targetSpeedX);
        }
        else if (Velocity.x != 0)
        {
            Velocity.x += decelerationVector.x;
            if ((Velocity.x <= 0 && decelerationVector.x < 0) ||
                (Velocity.x >= 0 && decelerationVector.x > 0))
            {
                Velocity.x = 0;
            }
        }

        if (!DisableCollisions)
        {
            // prevent walking through walls or up steep slopes
            CheckHorizontalSlopes(Velocity);
        }
    }

    private float Colliding(Vector2 direction, float rayDistance)
    {
        var hits = RaycastLauncher.GetHitData(direction, rayDistance, _margin).Where(x => x.distance > 0).ToList();

        if (hits.Count == 0) return 0;
        return hits.Min(x => x.distance);
    }

    private (float, float) CollidingWithSlope(Vector2 direction, float rayDistance)
    {
        var hits = RaycastLauncher.GetHitData(direction, rayDistance, _margin).Where(x => x.distance > 0).ToList();
        if (hits.Count == 0) return (0, 0);

        var minDist = hits.Min(x => x.distance);
        var minHit = hits.First(x => x.distance == minDist);

        var angle = Vector2.Angle(minHit.normal, Vector2.up);
        if (Mathf.Abs(angle) < _angleLeeway || (angle == 90 && OnlyOneHit(hits)))
        {
            return (0, angle);
        }
        else
        {
            return (minDist, angle);
        }
    }

    private bool OnlyOneHit(List<RaycastHit2D> hits)
    {
        if (hits.Count > 1)
        {
            return false;
        }

        return true;
    }

    private void CheckHorizontalSlopes(Vector2 velocity)
    {
        if (velocity.x != 0)
        {
            var tooSlow = Mathf.Abs(velocity.x) < 1f;
            var rayDistanceX = tooSlow ? .1f : Mathf.Abs(velocity.x * Time.deltaTime);
            rayDistanceX += Collider.bounds.extents.x;
            var (hitDistanceX, angle) = CollidingWithSlope(new Vector2(velocity.x, 0), rayDistanceX);
            if (hitDistanceX > 0)
            {
                if (!tooSlow && angle != 90)
                {
                    float moveDistance = Mathf.Sign(velocity.x) * (hitDistanceX - Collider.bounds.extents.x);
                    transform.Translate(moveDistance, 0, 0);
                }
                Velocity.x = 0;
            }
        }

        PushOutOfWalls();
    }

    public void PushOutOfWalls()
    {
        if (WallExitCollider == null) WallExitCollider = Collider;

        var (hitDistanceX1, angleX1) = CollidingWithSlope(Vector2.right, WallExitCollider.bounds.extents.x + .01f);

        if (hitDistanceX1 != 0 && angleX1 == 90)
        {
            float moveDistance = hitDistanceX1 - WallExitCollider.bounds.extents.x;
            transform.Translate(moveDistance - .02f, 0, 0);
        }

        var (hitDistanceX2, angleX2) = CollidingWithSlope(Vector2.left, WallExitCollider.bounds.extents.x + .01f);

        if (hitDistanceX2 != 0 && angleX2 == 90)
        {
            float moveDistance = -hitDistanceX2 + WallExitCollider.bounds.extents.x;
            transform.Translate(moveDistance + .02f, 0, 0);
        }
    }
}
