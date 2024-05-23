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
        if (Input.GetMouseButton(1))
            this.basePosition +=
                -(new Vector3(this.transform.right.x, 0, this.transform.right.z)).normalized * this.moveSpeed * Time.deltaTime * Input.GetAxis("Mouse X")
                - (new Vector3(this.transform.forward.x, 0, this.transform.forward.z)).normalized * this.moveSpeed * Time.deltaTime * Input.GetAxis("Mouse X");
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
