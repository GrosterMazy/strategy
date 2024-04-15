using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))] // т.к. скрипт считает, что его позиция - позиция камеры
public class CameraControls : MonoBehaviour {

    [SerializeField] private bool useCurrentPositionAsBasePosition = false;
    [SerializeField] private Vector3 basePosition;

    [SerializeField] private float zoomSpeed;
    [SerializeField] private float minZoom;
    [SerializeField] private float maxZoom;
    
    [SerializeField] private float moveSpeed;
    private  float _currentZoom;
    

    void Start() {
        if (this.useCurrentPositionAsBasePosition) this.basePosition = this.transform.position;
    }

    void Update() {
        // движение камеры
        if (Input.GetMouseButton(2))
            this.basePosition +=
                - (new Vector3(this.transform.right.x, 0, this.transform.right.z)).normalized
                    * this.moveSpeed * Time.deltaTime * Input.GetAxis("Mouse X")
                - Vector3.Cross(
                        (new Vector3(this.transform.right.x, 0, this.transform.right.z)).normalized,
                        new Vector3(0, 1, 0)
                    ) * this.moveSpeed * Time.deltaTime * Input.GetAxis("Mouse Y");
        // зум
        this._currentZoom = Mathf.Clamp(
            this._currentZoom - this.zoomSpeed * Time.deltaTime * Input.GetAxis("Mouse ScrollWheel"),
            this.minZoom, 
            this.maxZoom
        );

        // применение
        this.transform.localPosition = this.basePosition - this.transform.forward * this._currentZoom;
    }
}
