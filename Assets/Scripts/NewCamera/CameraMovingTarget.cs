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

        if (Input.GetMouseButton(1))
        {
            _currentX += -speed * Time.fixedDeltaTime * Input.GetAxis("Mouse X");
            _currentZ += -speed * Time.fixedDeltaTime * Input.GetAxis("Mouse Y");
        }
        else if (Input.GetAxis("Horizontal")!=0 || Input.GetAxis("Vertical") != 0)
        {
            _currentX += speed * Time.fixedDeltaTime * Input.GetAxis("Horizontal") ;
            _currentZ += speed * Time.fixedDeltaTime * Input.GetAxis("Vertical") ;
        }
        if(Input.GetAxis("Mouse ScrollWheel") !=0) _currentZoom = Mathf.Clamp(_currentZoom - zoomSpeed * Time.fixedDeltaTime * Input.GetAxis("Mouse ScrollWheel")*50, minZoom, maxZoom);
        if (Input.GetKey(KeyCode.E)) _currentZoom = Mathf.Clamp(_currentZoom - zoomSpeed * Time.fixedDeltaTime * 1, minZoom, maxZoom);
        if (Input.GetKey(KeyCode.Q)) _currentZoom = Mathf.Clamp(_currentZoom - zoomSpeed * Time.fixedDeltaTime *- 1, minZoom, maxZoom);

        transform.localPosition = new Vector3 (_currentX, _currentZoom, _currentZ);
    }
}
