using UnityEngine;

public class NewCameraControl : MonoBehaviour
{
    public Transform objectToFollow;
    [SerializeField] private float MovementDeceleration;
    [SerializeField] private float RotationDeceleration;

    private Transform _transform;

    private void Awake()
    {
        _transform = transform;
        _transform.position = objectToFollow.position;
    }

    private void FixedUpdate()
    {
        _transform.position = Vector3.Lerp(_transform.position, objectToFollow.position, Time.fixedDeltaTime* MovementDeceleration) + Vector3.up / 4;
        _transform.rotation = Quaternion.Lerp(_transform.rotation, objectToFollow.rotation, Time.fixedDeltaTime * RotationDeceleration);
    }
}
