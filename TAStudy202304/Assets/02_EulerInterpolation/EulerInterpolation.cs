using UnityEngine;

public class EulerInterpolation : MonoBehaviour
{
    [SerializeField] private Transform _target;
    [SerializeField] private Transform _transform;
    [SerializeField] private float _speed = 1000f;
    [SerializeField, Range(0.9f, 1.0f)] private float _damping = 0.99f;

    private Vector3 _velocity;

    private void Update()
    {
        Vector3 acceleration = Vector3.Normalize(_target.position - _transform.position) * _speed;
        _velocity += acceleration * Time.deltaTime;
        _transform.position += _velocity * Time.deltaTime;
        _velocity *= _damping;
    }
}
