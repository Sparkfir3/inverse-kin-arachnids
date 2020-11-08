using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using DitzelGames.FastIK;

public class LegController : MonoBehaviour {

    [Header("IK Control")]
    public Transform baseTarget;
    public Transform endTarget;

    private FastIKFabric ik;

    private void Awake() {
        ik = GetComponentInChildren<FastIKFabric>();
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