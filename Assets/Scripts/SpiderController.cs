using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SpiderController : MonoBehaviour {

    private bool _active;

    private Rigidbody rb;
    private BoxCollider boxCollider;
    private NavMeshAgent agent;

    private List<GameObject> legs = new List<GameObject>();

    private void Awake() {
        rb = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
        agent = GetComponent<NavMeshAgent>();

        foreach(Transform child in transform)
            legs.Add(child.gameObject);
    }

    private void Start() {
        Active = legs.Count > 0;
    }

    public bool Active {
        get {
            return _active;
        }
        set {
            _active = value;

            // Set own components
            rb.isKinematic = _active;
            rb.useGravity = !_active;
            boxCollider.enabled = !_active;
            agent.enabled = false; //_active;

            // Set legs
            foreach(GameObject leg in legs) {
                leg.GetComponent<LegController>().Active = _active;
                leg.transform.parent = _active ? transform : null;
            }
        }
    }

}