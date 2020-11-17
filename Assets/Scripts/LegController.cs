using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using DitzelGames.FastIK;

public class LegController : MonoBehaviour {

    [Header("IK Control")]
    public Transform endTarget;             //Position where the foot will try to place itself
    public Transform targetCaster;          //Floating transform where the raycast will originate from
    public float distThreshold = 1.0f;      //Max distance between raycast hit and current foot position before spider takes a step
    public float stepTime = 0.5f;           //Time required for the leg to take a step forward/backward

    private bool _active;
    private FastIKFabric ik;
    private Rigidbody rb;
    private List<BoxCollider> boxColliders = new List<BoxCollider>();
    private RaycastHit hit;
    private bool drawRayGizmo = false;
    private Vector3 footStopPos;

    // -------------------------------------------------------------------------------------------------------------

    private void Awake() {
        ik = GetComponentInChildren<FastIKFabric>();
        rb = GetComponent<Rigidbody>();
        foreach(BoxCollider collider in GetComponentsInChildren<BoxCollider>())
            boxColliders.Add(collider);
        footStopPos = new Vector3(-2, 0, 0);
    }

    // -------------------------------------------------------------------------------------------------------------

    public bool Active {
        get {
            return _active;
        }
        set {
            _active = value;

            rb.isKinematic = _active;
            rb.useGravity = !_active;
            foreach(BoxCollider boxCollider in boxColliders)
                boxCollider.enabled = !_active;

            // Disable IK
            if(!_active)
                SetIk(false);
        }
    }

    public void SetIk(bool value) {
        ik.enabled = value;
    }

    public void SetTarget(Vector3 position) {
        endTarget.position = position;
    }

    private void Update()
    {
        SetTarget(footStopPos);

        // Does the ray intersect any objects excluding the player layer
        if (Physics.Raycast(targetCaster.transform.position, transform.TransformDirection(Vector3.down), out hit, Mathf.Infinity, LayerMask.GetMask("Default")))
        {
            float travelDistance = Vector3.Distance(hit.point, endTarget.transform.position);
            Debug.DrawRay(targetCaster.transform.position, transform.TransformDirection(Vector3.down) * hit.distance, Color.magenta);
            Debug.DrawRay(hit.point, transform.TransformDirection(Vector3.left) * travelDistance, Color.cyan);
            Debug.Log("Did Hit");
            drawRayGizmo = true;

            if(travelDistance > distThreshold)
            {
                StartCoroutine(StepLerp(stepTime));
            }
        }
        else
        {
            Debug.DrawRay(targetCaster.transform.position, transform.TransformDirection(Vector3.down) * 1000, Color.red);
            Debug.Log("Did not Hit");
            drawRayGizmo = false;
        }
    }

    private void LateUpdate() {
        if(ik.enabled)
            ik.Target = endTarget;
    }

    private void OnDrawGizmos() {
        //End Target Gizmo (RED)
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(endTarget.position, 0.05f);

        //Target Caster Gizmo (BLUE)
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(targetCaster.position, 0.1f);

        if(drawRayGizmo)
        {
            //Raycast Hit Gizmo (GREEN)
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(hit.point, 0.1f);
        }
    }

    IEnumerator StepLerp(float stepTime)
    {
        Vector3 oldFootPos = footStopPos;
        Vector3 newFootPos = hit.point;

        for (float l = 0; l < 1; l += (stepTime * Time.deltaTime))
        {
            footStopPos = Vector3.Lerp(oldFootPos, newFootPos, l);
            yield return null;
        }
    }
}