using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class NavBallDragController : MonoBehaviour, IBeginDragHandler, IDragHandler
{

    [SerializeField] private Transform _baseBallTransform;
    [SerializeField] private float _dragMod;
    
    private Vector3 _dragStart;
    private Quaternion _baseRotation;

    public void OnBeginDrag(PointerEventData eventData)
    {
        _dragStart = Input.mousePosition;
        _baseRotation = _baseBallTransform.rotation;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector3 direction = Input.mousePosition - _dragStart;
        Vector3 rotationAxis = Vector3.Cross(direction, Vector3.forward);
        
        _baseBallTransform.rotation = _baseRotation;
        _baseBallTransform.Rotate(rotationAxis, direction.magnitude * _dragMod, Space.World);
    }

}