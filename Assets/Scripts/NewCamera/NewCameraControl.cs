﻿using UnityEngine;

public class NewCameraControl : MonoBehaviour
{
    public Transform objectToFollow;
    [SerializeField] private float Deceleration;

    private Transform _transform;

    private void Awake()
    {
        _transform = transform;
        _transform.position = objectToFollow.position;
    }

    private void FixedUpdate()
    {
        _transform.position = Vector3.Lerp(_transform.position, objectToFollow.position, Time.fixedDeltaTime* Deceleration) + Vector3.up / 4;
    }
}
