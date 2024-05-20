using UnityEngine;

public class CameraMovingTarget : MonoBehaviour
{
    [SerializeField] private float speed;

    [SerializeField] private float zoomSpeed;
    [SerializeField] private float minZoom;
    [SerializeField] private float maxZoom;

    [SerializeField] Transform MapCenter;

    private float _currentZoom;
    private float _currentX;
    private float _currentZ;
    private Transform _transform;

    private void Awake()
    {
        _transform = transform;
    }
    private void FixedUpdate()
    {
       // _transform.position = Vector3.Lerp(_transform.position, MapCenter.position, Time.fixedDeltaTime * speed) + Vector3.up / 4;
       // Передвижение //
        if (Input.GetMouseButton(1))
        {
           _currentX = Mathf.Clamp(_currentX + -speed * Time.fixedDeltaTime * Input.GetAxis("Mouse X"),-MapCenter.position.x, MapCenter.position.x * 2);
           _currentZ = Mathf.Clamp(_currentZ + - speed * Time.fixedDeltaTime * Input.GetAxis("Mouse Y"), -MapCenter.position.z, MapCenter.position.z * 2);
        }
        else if (Input.GetAxis("Horizontal")!=0 || Input.GetAxis("Vertical") != 0)
        {
            _currentX = Mathf.Clamp(_currentX + speed * Time.fixedDeltaTime * Input.GetAxis("Horizontal"), -MapCenter.position.x, MapCenter.position.x * 2);
            _currentZ = Mathf.Clamp(_currentZ + speed * Time.fixedDeltaTime * Input.GetAxis("Vertical"), -MapCenter.position.z, MapCenter.position.z * 2);
        }
        // Зум //
        if(Input.GetAxis("Mouse ScrollWheel") !=0) _currentZoom = Mathf.Clamp(_currentZoom - zoomSpeed * Time.fixedDeltaTime * Input.GetAxis("Mouse ScrollWheel")*50, minZoom, maxZoom);
        if (Input.GetKey(KeyCode.E)) _currentZoom = Mathf.Clamp(_currentZoom - zoomSpeed * Time.fixedDeltaTime * 1, minZoom, maxZoom);
        if (Input.GetKey(KeyCode.Q)) _currentZoom = Mathf.Clamp(_currentZoom - zoomSpeed * Time.fixedDeltaTime *- 1, minZoom, maxZoom);

        transform.localPosition = new Vector3 (_currentX, _currentZoom, _currentZ);
    }
}
