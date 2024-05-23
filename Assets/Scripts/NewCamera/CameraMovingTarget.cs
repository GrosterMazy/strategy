using UnityEngine;

public class CameraMovingTarget : MonoBehaviour
{
    [SerializeField] private float MoveSpeed;

    [SerializeField] private float ZoomSpeed;
    [SerializeField] private float MinZoom;
    [SerializeField] private float MaxZoom;

    [SerializeField] private float RotationSpeed;

    [SerializeField] Transform MapCenter;

    [SerializeField] bool UseRotation=false;

    private float _currentZoom;
    private float _currentRotation;
    private float _currentX;
    private float _currentZ;
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
            if (Input.GetMouseButton(1))
            {
                _currentX = Mathf.Clamp(_currentX + -MoveSpeed * Time.fixedDeltaTime * Input.GetAxis("Mouse X"), -MapCenter.position.x * 2, MapCenter.position.x * 2);
                _currentZ = Mathf.Clamp(_currentZ + -MoveSpeed * Time.fixedDeltaTime * Input.GetAxis("Mouse Y"), -MapCenter.position.z * 2, MapCenter.position.z * 2);
            }
            else if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
            {
                _currentX = Mathf.Clamp(_currentX + MoveSpeed * Time.fixedDeltaTime * Input.GetAxis("Horizontal"), -MapCenter.position.x * 2, MapCenter.position.x * 2);
                _currentZ = Mathf.Clamp(_currentZ + MoveSpeed * Time.fixedDeltaTime * Input.GetAxis("Vertical"), -MapCenter.position.z * 2, MapCenter.position.z * 2);
            }
            // Зум //
            if (Input.GetAxis("Mouse ScrollWheel") != 0) _currentZoom = Mathf.Clamp(_currentZoom - ZoomSpeed * Time.fixedDeltaTime * Input.GetAxis("Mouse ScrollWheel") * 40, MinZoom, MaxZoom);
            if (Input.GetKey(KeyCode.E)) _currentZoom = Mathf.Clamp(_currentZoom + ZoomSpeed * Time.fixedDeltaTime, MinZoom, MaxZoom);
            if (Input.GetKey(KeyCode.Q)) _currentZoom = Mathf.Clamp(_currentZoom - ZoomSpeed * Time.fixedDeltaTime, MinZoom, MaxZoom);
            // Пворот //
            if (UseRotation == true)
            {
                if (Input.GetMouseButton(0)) _currentRotation += RotationSpeed * Time.fixedDeltaTime * Input.GetAxis("Mouse X");
                if (Input.GetKey(KeyCode.E)) _currentRotation += RotationSpeed * Time.fixedDeltaTime;
                if (Input.GetKey(KeyCode.Q)) _currentRotation -= RotationSpeed * Time.fixedDeltaTime;

                transform.rotation = Quaternion.Euler(60, _currentRotation, 0);
            }
            transform.localPosition = new Vector3(_currentX, _currentZoom, _currentZ);
        }
    }
}
