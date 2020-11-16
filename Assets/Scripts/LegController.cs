using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using DitzelGames.FastIK;

public class LegController : MonoBehaviour {

    [Header("IK Control")]
    public Transform endTarget;

    private bool _active;
    private FastIKFabric ik;
    private Rigidbody rb;
    private List<BoxCollider> boxColliders = new List<BoxCollider>();

    // -------------------------------------------------------------------------------------------------------------

    private void Awake() {
        ik = GetComponentInChildren<FastIKFabric>();
        rb = GetComponent<Rigidbody>();
        foreach(BoxCollider collider in GetComponentsInChildren<BoxCollider>())
            boxColliders.Add(collider);
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

    // -------------------------------------------------------------------------------------------------------------

    private void LateUpdate() {
        if(ik.enabled)
            ik.Target = endTarget;
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(endTarget.position, 0.05f);
    }

}