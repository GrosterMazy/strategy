using UnityEngine;

    public class MenuCamera : MonoBehaviour
    {
        public Transform objectToFollow;
        public float speed;

        private Transform _transform;

        private bool _isValid;
        
        private void Awake()
        {
            _isValid = objectToFollow;
            if (!_isValid)
            {
                Debug.LogError("There is no Object To Follow in CameraRig. Please set it.");
                return;
            }
            
            _transform = transform;
            _transform.position = objectToFollow.position;
        }

        private void FixedUpdate()
        {
            if(!_isValid) return;
            _transform.position = Vector3.Lerp(_transform.position, objectToFollow.position, Time.fixedDeltaTime * speed) + Vector3.up/4;

            if(Input.GetMouseButton(1))
            {
            transform.position += -new Vector3(10 / speed * Time.fixedDeltaTime * Input.GetAxis("Mouse X"),0,0);
            transform.position += -new Vector3(0, 0, 10 / speed * Time.fixedDeltaTime * Input.GetAxis("Mouse Y"));
            }
        }
        
        private void Update()
        {

          /*  float tiltAngle = Input.GetAxis("Horizontal") * 15f;
            Quaternion targetRotation = Quaternion.Euler(0, tiltAngle, 0);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, speed * Time.deltaTime);*/

        }
    }
