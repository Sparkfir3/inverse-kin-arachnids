using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using DitzelGames.FastIK;

public class LegController : MonoBehaviour {

    [Header("IK Control")]
    public Transform baseTarget;
    public Transform endTarget;

    private bool _active;
    private FastIKFabric ik;
    private Rigidbody rb;
    private List<BoxCollider> boxColliders = new List<BoxCollider>();

    private void Awake() {
        ik = GetComponentInChildren<FastIKFabric>();
        rb = GetComponent<Rigidbody>();
        foreach(BoxCollider collider in GetComponentsInChildren<BoxCollider>())
            boxColliders.Add(collider);
    }

    public bool Active {
        get {
            return _active;
        }
        set {
            _active = value;

            ik.enabled = _active;
            rb.isKinematic = _active;
            rb.useGravity = !_active;
            foreach(BoxCollider boxCollider in boxColliders)
                boxCollider.enabled = !_active;
        }
    }

    private void LateUpdate() {
        if(baseTarget)
            transform.position = baseTarget.position;
        ik.Target = endTarget;
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(endTarget.position, 0.05f);
    }

}