using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] float moveSpeed = 1.5f;
    private Vector3 moveDirection = Vector3.zero;

    Vector3 Angles;
    [SerializeField] float sensitivityVertical = 2.0f;
    [SerializeField] float sensitivityHorizontal = 2.0f;
    void Update()
    {
        float rotationY = Input.GetAxis("Mouse Y") * sensitivityVertical;
        float rotationX = Input.GetAxis("Mouse X") * sensitivityHorizontal;
        if (rotationY > 0)
            Angles = new Vector3(Mathf.MoveTowards(Angles.x, -80, rotationY), Angles.y + rotationX, 0);
        else
            Angles = new Vector3(Mathf.MoveTowards(Angles.x, 80, -rotationY), Angles.y + rotationX, 0);
        transform.localEulerAngles = Angles;
    }

    void FixedUpdate()
    {
        moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        moveDirection = transform.TransformDirection(moveDirection);
        moveDirection *= moveSpeed;
        transform.position += moveDirection;
    }
}
