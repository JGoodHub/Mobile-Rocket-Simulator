using System;
using System.Collections;
using System.Collections.Generic;
using GoodHub.Core.Runtime;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraController : SceneSingleton<CameraController>
{

    [SerializeField] private Transform _trackingTarget;
    [SerializeField] private float _horizontalDragModifier = 1f;
    [SerializeField] private float _verticalDragModifier = 1f;
    [SerializeField] private Transform _yawPivot;
    [SerializeField] private Transform _pitchPivot;

    [SerializeField] private Transform _planetTransform;

    private Vector3 _dragStart;
    private Quaternion _yawBaseRotation;
    private Quaternion _pitchBaseRotation;

    private void Start()
    {
        TouchInput.OnTouchDragEnter += OnDragEnter;
        TouchInput.OnTouchDragStay += OnDragStay;
    }

    public void Update()
    {
        Vector3 planetSurfaceUp = (_planetTransform.position - transform.position).normalized;
        Plane horizonPlane = new Plane(planetSurfaceUp, 0);
        Vector3 planeProjectedNorth = horizonPlane.ClosestPointOnPlane(_planetTransform.up).normalized;
        
        transform.position = _trackingTarget.position;
        transform.rotation = Quaternion.LookRotation(planeProjectedNorth, -planetSurfaceUp);
    }

    public void OnDragEnter(TouchInput.TouchData touchData)
    {
        if (touchData.DownOverUI)
            return;

        _dragStart = Input.mousePosition;
        _yawBaseRotation = _yawPivot.localRotation;
        _pitchBaseRotation = _pitchPivot.localRotation;
    }

    public void OnDragStay(TouchInput.TouchData touchData)
    {
        if (touchData.DownOverUI)
            return;

        Vector3 direction = Input.mousePosition - _dragStart;

        _yawPivot.localRotation = _yawBaseRotation;
        _yawPivot.Rotate(_yawPivot.up, direction.x * _horizontalDragModifier, Space.World);

        _pitchPivot.localRotation = _pitchBaseRotation;
        _pitchPivot.Rotate(_pitchPivot.right, -direction.y * _verticalDragModifier, Space.World);
    }

}