using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] float moveSpeed = 10f;
    private Vector3 moveDirection = Vector3.zero;

    Vector3 Angles;
    [SerializeField] float sensitivityVertical = 2.0f;
    [SerializeField] float sensitivityHorizontal = 2.0f;

    void Update() {
        if(!GameManager.manager.Paused) {
            float rotationY = Input.GetAxis("Mouse Y") * sensitivityVertical;
            float rotationX = Input.GetAxis("Mouse X") * sensitivityHorizontal;
            if(rotationY > 0)
                Angles = new Vector3(Mathf.MoveTowards(Angles.x, -80, rotationY), Angles.y + rotationX, 0);
            else
                Angles = new Vector3(Mathf.MoveTowards(Angles.x, 80, -rotationY), Angles.y + rotationX, 0);
            transform.localEulerAngles = Angles;
        }
    }

    void FixedUpdate()
    {
        if(!GameManager.manager.Paused) {
            moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
            moveDirection = transform.TransformDirection(moveDirection);
            moveDirection += new Vector3(0, Input.GetAxis("Jump"), 0);
            moveDirection *= moveSpeed * Time.deltaTime;
            transform.position += moveDirection;
        }
    }
}
