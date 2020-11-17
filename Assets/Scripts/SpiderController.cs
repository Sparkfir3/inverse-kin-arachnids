using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SpiderController : MonoBehaviour {

    private bool _active;

    private Rigidbody rb;
    private BoxCollider boxCollider;
    private NavMeshAgent agent;

    private List<Vector3> baseLegPositions = new List<Vector3>();
    [SerializeField] List<Transform> legTargets = new List<Transform>();
    //private List<Transform> legTargetCasters = new List<Transform>();
    [SerializeField] List<GameObject> legs = new List<GameObject>();

    // -------------------------------------------------------------------------------------------------------------

    #region Initalization

    private void Awake() {
        rb = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
        agent = GetComponent<NavMeshAgent>();

        //foreach(Transform child in transform)
            //legs.Add(child.gameObject);

        // Set list
        //legTargets.Clear();
        //foreach (GameObject leg in legs)
            //legTargets.Add(leg.transform.Find("Desired End Target"));

        // Set target casters
        for (int i = 0; i < legs.Count; i++)
        {
            legs[i].GetComponent<LegController>().legNum = i;
            Transform t = legs[i].GetComponent<LegController>().targetCaster;

            switch(i)
            {
                case 0:
                    //t.position.Set(t.position.x - 1, t.position.y, t.position.z);
                    t.localPosition = new Vector3(t.localPosition.x - 1, t.localPosition.y, t.localPosition.z);
                    //legTargetCasters.Add(t);
                    break;
                case 1:
                    //t.position.Set(t.position.x + 1, t.position.y, t.position.z);
                    t.localPosition = new Vector3(t.localPosition.x + 1, t.localPosition.y, t.localPosition.z);
                    //legTargetCasters.Add(t);
                    break;
                case 2:
                    //t.position.Set(t.position.x + 1, t.position.y, t.position.z);
                    t.localPosition = new Vector3(t.localPosition.x + 1, t.localPosition.y, t.localPosition.z);
                    //legTargetCasters.Add(t);
                    break;
                case 3:
                    //t.position.Set(t.position.x - 1, t.position.y, t.position.z);
                    t.localPosition = new Vector3(t.localPosition.x - 1, t.localPosition.y, t.localPosition.z);
                    //legTargetCasters.Add(t);
                    break;
                case 4:
                    break;
                case 5:
                    break;
                case 6:
                    break;
                case 7:
                    break;
                default:
                    break;
            }

            //legs[i].
        }
    }

    private void Start() {
        Active = legs.Count >= 2;

        //InvokeRepeating("AttemptRebuild", 2f, 2f); // Repeat every 2 seconds
    }

    #endregion

    #region Repeating

    private void Update()
    {
        foreach (Transform legTarget in legTargets)
        {

        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.black;
        foreach (Transform legTarget in legTargets)
            Gizmos.DrawSphere(legTarget.position, 0.15f);
    }

    #endregion

    // -------------------------------------------------------------------------------------------------------------

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

            // Disable legs
            if(!_active) {
                foreach(GameObject leg in legs) {
                    leg.GetComponent<LegController>().Active = false;
                    leg.transform.parent = null;
                }
                legs.Clear();
            }
        }
    }

    // -------------------------------------------------------------------------------------------------------------

    #region Rebuilding Spider

    public void AttemptRebuild() {
        if(!_active) {
            List<GameObject> newLegs = ScanForLegs();
            if(newLegs.Count >= 2) {
                StartCoroutine(Rebuild(newLegs));
            }
        }
    }

    private List<GameObject> ScanForLegs() {
        List<GameObject> legs = new List<GameObject>(); // TODO
        RaycastHit[] hits = Physics.SphereCastAll(transform.position, 5f, transform.forward, 0f);
        foreach(RaycastHit hit in hits) {
            //Debug.Log(hit.collider.gameObject.name);
            LegController hitLeg = hit.collider.GetComponentInParent<LegController>();
            if(hitLeg && !legs.Contains(hitLeg.gameObject)) {
                legs.Add(hitLeg.gameObject);
            }
        }

        return legs;
    }

    private IEnumerator Rebuild(List<GameObject> newLegs) {
        // Make number of legs even
        if(newLegs.Count % 2 == 1)
            newLegs.RemoveAt(newLegs.Count - 1);

        #region Reposition Legs
        // Set legs active
        SetLegBasePositions(newLegs.Count);
        for(int j = 0; j < newLegs.Count; j++) {
            newLegs[j].GetComponent<LegController>().Active = true;
        }

        // Determine end points in world space
        List<Quaternion> endRotations = new List<Quaternion>();
        List<Vector3> endPositions = new List<Vector3>();
        for(int i = 0; i < newLegs.Count; i++) {
            endRotations.Add(transform.rotation * Quaternion.Euler(0f, 90 * (i % 2 == 0 ? 1f : -1f), 0f));
            endPositions.Add(transform.TransformPoint(baseLegPositions[i]));
        }

        // Lerp over time
        for(float i = 0; i < 1f; i += Time.deltaTime) {
            for(int j = 0; j < newLegs.Count; j++) {
                newLegs[j].transform.rotation = Quaternion.Slerp(newLegs[j].transform.rotation, endRotations[j], i / 1f);
                newLegs[j].transform.position = Vector3.Lerp(newLegs[j].transform.position, endPositions[j], i / 1f);
            }
            yield return null;
        }

        // Force end position
        for(int j = 0; j < newLegs.Count; j++) {
            newLegs[j].transform.rotation = endRotations[j];
            newLegs[j].transform.position = endPositions[j];
        }
        #endregion

        // Set list
        legs = newLegs;
        legTargets.Clear();
        foreach (GameObject leg in legs)
            legTargets.Add(leg.transform.Find("Desired End Target"));
            //legTargets.Add(leg.transform);

        // Re-enable
        yield return new WaitForSeconds(0.1f);
        for(int j = 0; j < newLegs.Count; j++) { // Set parent and enable ik
            newLegs[j].GetComponent<LegController>().SetIk(true);
            newLegs[j].transform.parent = transform;
        }
        Active = true;
    }

    private void SetLegBasePositions(int count) {
        baseLegPositions.Clear();
        switch(count) {
            case 2:
                baseLegPositions.Add(new Vector3(0.5f, 0f, 0f));
                baseLegPositions.Add(new Vector3(-0.5f, 0f, 0f));
                break;
            case 4:
                baseLegPositions.Add(new Vector3(0.5f, 0f, 0.2f));
                baseLegPositions.Add(new Vector3(-0.5f, 0f, 0.2f));
                baseLegPositions.Add(new Vector3(0.5f, 0f, -0.2f));
                baseLegPositions.Add(new Vector3(-0.5f, 0f, -0.2f));
                break;
            default:
                break;
        }
    }

    #endregion

}