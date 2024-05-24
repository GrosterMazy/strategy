using UnityEngine;

public class CameraMovingTarget : MonoBehaviour
{
    [SerializeField] private float MoveSpeed;

    [SerializeField] private float ZoomSpeed;
    [SerializeField] private float MinZoom;
    [SerializeField] private float MaxZoom;

    [SerializeField] private float RotationSpeed;

    [SerializeField] Transform MapCenter;

    private float _currentZoom;
    private float _currentRotation;
    private float _currentX;
    private float _currentZ;
    private Vector3 _currentPosition;
    private Transform _transform;
    private SingleResourcesTransferWindow SRTW;

    private void Awake()
    {
        SRTW = FindObjectOfType<SingleResourcesTransferWindow>();
        _transform = transform;
    }
    private void FixedUpdate()
    {
        if(SRTW.gameObject.activeSelf==false)
            {
            // Передвижение //
            if (!Input.GetKey(KeyCode.LeftControl) && Input.GetMouseButton(2) || Input.GetKey(KeyCode.F1))
            {
                _currentPosition -= (new Vector3(transform.right.x, 0, transform.right.z)).normalized * MoveSpeed * Time.deltaTime * Input.GetAxis("Mouse X");
                _currentPosition -= (new Vector3(transform.forward.x, 0, transform.forward.z)).normalized * MoveSpeed * Time.deltaTime * Input.GetAxis("Mouse Y");
                _currentPosition.x = Mathf.Clamp(_currentPosition.x, -MapCenter.position.x * 2, MapCenter.position.x * 2);
                _currentPosition.z = Mathf.Clamp(_currentPosition.z, -MapCenter.position.z * 2, MapCenter.position.z * 2);
            }
            else if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
            {
                _currentPosition += (new Vector3(transform.right.x, 0, transform.right.z)).normalized * MoveSpeed * Time.deltaTime * Input.GetAxis("Horizontal");
                _currentPosition += (new Vector3(transform.forward.x, 0, transform.forward.z)).normalized * MoveSpeed * Time.deltaTime * Input.GetAxis("Vertical");
                _currentPosition.x = Mathf.Clamp(_currentPosition.x, -MapCenter.position.x * 2, MapCenter.position.x * 2);
                _currentPosition.z = Mathf.Clamp(_currentPosition.z, -MapCenter.position.z * 2, MapCenter.position.z * 2);
            }

            // Зум //
            if (Input.GetAxis("Mouse ScrollWheel") != 0) _currentZoom = Mathf.Clamp(_currentZoom - ZoomSpeed * Time.fixedDeltaTime * Input.GetAxis("Mouse ScrollWheel") * 40, MinZoom, MaxZoom);
            if (Input.GetKey(KeyCode.E)) _currentZoom = Mathf.Clamp(_currentZoom + ZoomSpeed * Time.fixedDeltaTime, MinZoom, MaxZoom);
            if (Input.GetKey(KeyCode.Q)) _currentZoom = Mathf.Clamp(_currentZoom - ZoomSpeed * Time.fixedDeltaTime, MinZoom, MaxZoom);
            // Пворот //

            if (Input.GetKey(KeyCode.LeftControl) && Input.GetMouseButton(2)) _currentRotation += RotationSpeed * Time.fixedDeltaTime * Input.GetAxis("Mouse X");

                transform.rotation = Quaternion.Euler(60, _currentRotation, 0);
            transform.localPosition = new Vector3(_currentPosition.x, _currentZoom, _currentPosition.z);
        }
    }
}
