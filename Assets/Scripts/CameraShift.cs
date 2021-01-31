using UnityEngine;

public class CameraShift : MonoBehaviour
{
    private const float SHIFT_TIME = 10f;
    private const float SHIFT_RADIUS = 1;

    private Vector3 _defaultPos;
    private float _shiftTimer;
    private Vector3 _targetPos;
    private Vector3 _prevPos;

    private void Awake()
    {
        _defaultPos = transform.position;
        SetTargetPos();
    }

    private void Update()
    {
        if (_shiftTimer < SHIFT_TIME)
        {
            _shiftTimer += Time.deltaTime;
            if (_shiftTimer > SHIFT_TIME)
            {
                SetTargetPos();
            }
        }

        transform.position = Vector3.Lerp(_prevPos, _targetPos, _shiftTimer / SHIFT_TIME);
    }

    private void SetTargetPos()
    {
        _prevPos = transform.position;
        _targetPos = _defaultPos + (Vector3)Random.insideUnitCircle * SHIFT_RADIUS;
        _shiftTimer = 0;
    }
}
