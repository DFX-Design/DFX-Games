using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform targetObject;

    public Vector3 cameraOffset;
    void Start()
    {
        cameraOffset = transform.position - targetObject.transform.position;
    }

    //LateUpdate calculates after physics engine completes its cycle. Physics engine is void update()
    void LateUpdate()
    {
        Vector3 newPosition = targetObject.transform.position + cameraOffset;
        transform.position = newPosition;
    }
}
