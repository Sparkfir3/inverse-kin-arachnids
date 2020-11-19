using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SpiderController : MonoBehaviour {

    public float casterOffset = 1.0f;
    public float bodyHeight = 1.0f;

#pragma warning disable 0649 // Disable "Field is never assigned" warning for SerializeField

    private bool _active;

    private Rigidbody rb;
    private BoxCollider boxCollider;
    private NavMeshAgent agent;

    [Header("IK Control")]
    private List<Vector3> baseLegPositions = new List<Vector3>();
    [SerializeField] List<Transform> legTargets = new List<Transform>();
    //private List<Transform> legTargetCasters = new List<Transform>();
    [SerializeField] List<GameObject> legs = new List<GameObject>();

    [Header("AI")]
    [SerializeField] private float moveSpeed;
    private Coroutine move;

    [Header("Re-assembly")]
    [SerializeField] private float rebuildHeightOffset;

    // -------------------------------------------------------------------------------------------------------------

    #region Initalization

    private void Awake() {
        rb = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
        agent = GetComponent<NavMeshAgent>();
        transform.localPosition = new Vector3(transform.localPosition.x, bodyHeight, transform.localPosition.z);

        agent.speed = moveSpeed;

        // Set legs, if not already set
        if(legs.Count == 0)
            foreach(Transform child in transform)
                legs.Add(child.gameObject);

        // Set list
        //legTargets.Clear();
        //foreach (GameObject leg in legs)
            //legTargets.Add(leg.transform.Find("Desired End Target"));
        
        // Set target casters
        for (int i = 0; i < legs.Count; i++)
        {
            legs[i].GetComponent<LegController>().legNum = i;
            Transform t = legs[i].GetComponent<LegController>().targetCaster;

            if (legs.Count == 2)
            {
               switch (i)
                {
                    case 0:
                        //t.position.Set(t.position.x - 1, t.position.y, t.position.z);
                        legs[0].GetComponent<LegController>().footStopPos = new Vector3(0, 0, transform.TransformPoint(t.transform.position.z - (legs[0].GetComponent<LegController>().distThreshold / 2), 0, 0).z);
                        t.localPosition = new Vector3(t.localPosition.x - (casterOffset / 2), t.localPosition.y, t.localPosition.z);
                        //legTargetCasters.Add(t);
                        break;
                    case 1:
                        //t.position.Set(t.position.x + 1, t.position.y, t.position.z);
                        legs[1].GetComponent<LegController>().footStopPos = new Vector3(transform.TransformPoint(t.transform.position).x, 0, 0);
                        t.localPosition = new Vector3(t.localPosition.x + (casterOffset / 2), t.localPosition.y, t.localPosition.z);
                        //legTargetCasters.Add(t);
                        break;
                }
            }
            else
            {
                switch (i)
                {
                    case 0:
                        //t.position.Set(t.position.x - 1, t.position.y, t.position.z);
                        legs[0].GetComponent<LegController>().footStopPos = new Vector3(0, 0, transform.TransformPoint(t.transform.position.z - (legs[0].GetComponent<LegController>().distThreshold / 2), 0, 0).z);
                        t.localPosition = new Vector3(t.localPosition.x - casterOffset, t.localPosition.y, t.localPosition.z);
                        //legTargetCasters.Add(t);
                        break;
                    case 1:
                        //t.position.Set(t.position.x + 1, t.position.y, t.position.z);
                        legs[1].GetComponent<LegController>().footStopPos = new Vector3(transform.TransformPoint(t.transform.position).x, 0, 0);
                        t.localPosition = new Vector3(t.localPosition.x + casterOffset, t.localPosition.y, t.localPosition.z);
                        //legTargetCasters.Add(t);
                        break;
                    case 2:
                        //t.position.Set(t.position.x + 1, t.position.y, t.position.z);
                        legs[2].GetComponent<LegController>().footStopPos = new Vector3(transform.TransformPoint(t.transform.position).x, 0, 0);
                        t.localPosition = new Vector3(t.localPosition.x + casterOffset, t.localPosition.y, t.localPosition.z);
                        //legTargetCasters.Add(t);
                        break;
                    case 3:
                        //t.position.Set(t.position.x - 1, t.position.y, t.position.z);
                        legs[3].GetComponent<LegController>().footStopPos = new Vector3(0, 0, transform.TransformPoint(t.transform.position.z - (legs[3].GetComponent<LegController>().distThreshold / 2), 0, 0).z);
                        t.localPosition = new Vector3(t.localPosition.x - casterOffset, t.localPosition.y, t.localPosition.z);
                        //t.localPosition = new Vector3(t.localPosition.x - casterOffset - (legs[i].GetComponent<LegController>().distThreshold / 2), t.localPosition.y, t.localPosition.z);
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
            }
        }

        legs[1].GetComponent<LegController>().canStep = true;

        if (legs.Count > 2)
        {
            legs[2].GetComponent<LegController>().canStep = true;
        }
    }

    private void Start() {
        Active = legs.Count >= 2;

        /*// Set target casters
        for (int i = 0; i < legs.Count; i++)
        {
            legs[i].GetComponent<LegController>().legNum = i;
            Transform t = legs[i].GetComponent<LegController>().targetCaster;

            switch (i)
            {
                case 0:
                    t.localPosition = new Vector3(t.localPosition.x + (legs[i].GetComponent<LegController>().distThreshold / 2), t.localPosition.y, t.localPosition.z);
                    break;
                case 3:
                    t.localPosition = new Vector3(t.localPosition.x + (legs[i].GetComponent<LegController>().distThreshold / 2), t.localPosition.y, t.localPosition.z);
                    break;
                default:
                    break;
            }
        }*/

        //InvokeRepeating("AttemptRebuild", 2f, 2f); // Repeat every 2 seconds
    }

    #endregion

    #region Repeating

    private void Update()
    {
       /* float avgLegTargetHeight = 0;
        
        if(legTargets.Count > 0 && legTargets.Count == legs.Count)
        {
            //int numLegsCounted = 0;
            for (int i = 0; i < legTargets.Count; i++)
            {
                if (legs[i].GetComponent<LegController>().isGrounded)
                {
                    avgLegTargetHeight += legTargets[i].position.y;
                    //numLegsCounted++;
                }
                else
                {
                    avgLegTargetHeight += legs[i].GetComponent<LegController>().footStopPos.y;
                }
            }
            avgLegTargetHeight /= legTargets.Count;
            //avgLegTargetHeight /= numLegsCounted;
        }*/

        //float bodyOffset = transform.position.y + bodyHeight - avgLegTargetHeight;

        //transform.position = new Vector3(transform.position.x, bodyHeight + avgLegTargetHeight, transform.position.z);
        //Debug.Log("avg leg: " + avgLegTargetHeight + "   +offset: " + (bodyHeight + avgLegTargetHeight));
        //Debug.Log("actual bod pos: " + transform.position.y);
        //transform.localPosition = new Vector3(transform.localPosition.x, bodyHeight + localLegAvg.y, transform.localPosition.z);

        for (int i = 0; i < legs.Count; i++)
        {
            if(legs.Count == 2)
            {
                if (i == 0)
                {
                    if (legs[1].GetComponent<LegController>().isGrounded &&
                        (legs[1].GetComponent<LegController>().travelDistance >= legs[1].GetComponent<LegController>().distThreshold / 4) && (legs[1].GetComponent<LegController>().travelDistance <= 3 * legs[1].GetComponent<LegController>().distThreshold / 4))
                    {
                        legs[0].GetComponent<LegController>().canStep = true;
                    }
                    else
                    {
                        legs[0].GetComponent<LegController>().canStep = false;
                    }
                }

                if (i == 1)
                {
                    if (legs[0].GetComponent<LegController>().isGrounded &&
                        (legs[0].GetComponent<LegController>().travelDistance >= legs[0].GetComponent<LegController>().distThreshold / 4) && (legs[0].GetComponent<LegController>().travelDistance <= 3 * legs[0].GetComponent<LegController>().distThreshold / 4))
                    {
                        legs[1].GetComponent<LegController>().canStep = true;
                    }
                    else
                    {
                        legs[1].GetComponent<LegController>().canStep = false;
                    }
                }
            }

            if(legs.Count == 4)
            {
                if (i == 0 || i == 3)
                {
                    if (legs[1].GetComponent<LegController>().isGrounded && legs[2].GetComponent<LegController>().isGrounded &&
                        (legs[1].GetComponent<LegController>().travelDistance >= legs[1].GetComponent<LegController>().distThreshold / 4) && (legs[1].GetComponent<LegController>().travelDistance <= 3 * legs[1].GetComponent<LegController>().distThreshold / 4))
                    {
                        legs[0].GetComponent<LegController>().canStep = true;
                        legs[3].GetComponent<LegController>().canStep = true;
                    }
                    else
                    {
                        legs[0].GetComponent<LegController>().canStep = false;
                        legs[3].GetComponent<LegController>().canStep = false;
                    }
                }

                if (i == 1 || i == 2)
                {
                    if (legs[0].GetComponent<LegController>().isGrounded && legs[3].GetComponent<LegController>().isGrounded &&
                        (legs[0].GetComponent<LegController>().travelDistance >= legs[0].GetComponent<LegController>().distThreshold / 4) && (legs[0].GetComponent<LegController>().travelDistance <= 3 * legs[0].GetComponent<LegController>().distThreshold / 4))
                    {
                        legs[1].GetComponent<LegController>().canStep = true;
                        legs[2].GetComponent<LegController>().canStep = true;
                    }
                    else
                    {
                        legs[1].GetComponent<LegController>().canStep = false;
                        legs[2].GetComponent<LegController>().canStep = false;
                    }
                }
            }
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
            agent.enabled = _active;

            // Disable legs
            if(!_active) {
                foreach(GameObject leg in legs) {
                    leg.GetComponent<LegController>().Active = false;
                    leg.transform.parent = null;
                }
                legs.Clear();
            }

            // Movement
            //if(_active)
              //  move = StartCoroutine(Navigate());
            //else if(move != null)
              //  StopCoroutine(move);
        }
    }

    // -------------------------------------------------------------------------------------------------------------

    #region Behavior

    private IEnumerator Navigate() {
        Vector3 target;
        /*Vector3 lookAt;
        Quaternion lookRotation;*/

        while(agent) {
            // Stop in place
            agent.destination = transform.position;
            yield return new WaitForSeconds(0.5f);

            // Set destination
            target = transform.position + new Vector3(Random.Range(-5f, 5f), 0f, Random.Range(-5f, 5f));
            /*lookAt = target - transform.position;
            lookAt.y = 0;
            lookRotation = Quaternion.LookRotation(target, Vector3.up);
            lookRotation = Quaternion.Euler(0f, lookRotation.eulerAngles.y, 0f);*/

            // Turn towards target
            /*for(float i = 0; i < 1f; i += Time.deltaTime) {
                transform.rotation = Quaternion.RotateTowards(transform.rotation, lookRotation, 360f * Time.deltaTime);
                Debug.Log("rotating");
                yield return null;
            }*/

            yield return new WaitForSeconds(0.5f);

            // Move
            agent.destination = target;
            yield return new WaitForSeconds(3f);
        }
    }

    #endregion

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
            if(hitLeg && !legs.Contains(hitLeg.gameObject) && legs.Count < 4) {
                legs.Add(hitLeg.gameObject);
            }
        }

        Debug.Log(gameObject.name + ", " + legs.Count);
        return legs;
    }

    private IEnumerator Rebuild(List<GameObject> newLegs) {
        // Make number of legs even
        if(newLegs.Count % 2 == 1)
            newLegs.RemoveAt(newLegs.Count - 1);

        // Freeze rigidbody
        rb.constraints = RigidbodyConstraints.FreezeAll;
        yield return new WaitForSeconds(1f);

        // Move body and legs
        yield return ResetBody();
        yield return new WaitForSeconds(0.5f);
        yield return SetNewLegs(newLegs);

        // Set list
        legs = newLegs;
        legTargets.Clear();
        foreach (GameObject leg in legs)
            legTargets.Add(leg.transform.Find("Desired End Target"));
        //legTargets.Add(leg.transform);

        // Unfreeze rigidbody
        rb.constraints = RigidbodyConstraints.None;

        // Re-enable
        yield return null;
        for(int j = 0; j < newLegs.Count; j++) { // Set parent and enable ik
            newLegs[j].GetComponent<LegController>().SetIk(true);
            newLegs[j].transform.parent = transform;
        }
        Active = true;
    }

    private IEnumerator ResetBody() {
        Quaternion desiredRotation, oldRotation = transform.rotation;
        Vector3 desiredPosition = transform.position, oldPosition = transform.position;

        // Get desired transform
        desiredRotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y, 0f);
        if(Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 5f, LayerMask.GetMask("Terrain")))
            desiredPosition = hit.point + new Vector3(0, rebuildHeightOffset, 0);

        // Lerp over time
        for(float i = 0; i < 1; i += Time.deltaTime) {
            transform.rotation = Quaternion.Slerp(oldRotation, desiredRotation, i / 1f);
            transform.position = Vector3.Lerp(oldPosition, desiredPosition, i / 1f);
            yield return null;
        }

        // Force end
        transform.rotation = desiredRotation;
        transform.position = desiredPosition;
        yield return null;
    }

    private IEnumerator SetNewLegs(List<GameObject> newLegs) {// Set legs active
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
        yield return null;
    }

    private void SetLegBasePositions(int count) {
        baseLegPositions.Clear();
        switch(count) {
            case 2:
                baseLegPositions.Add(new Vector3(0f, 0.745f, 0.2f));
                baseLegPositions.Add(new Vector3(0f, 0.745f, 0.2f));
                break;
            case 4:
                baseLegPositions.Add(new Vector3(0f, 0.745f, 0.2f));
                baseLegPositions.Add(new Vector3(0f, 0.745f, 0.2f));
                baseLegPositions.Add(new Vector3(0f, 0.745f, 0.09f));
                baseLegPositions.Add(new Vector3(0f, 0.745f, 0.09f));
                break;
            default:
                break;
        }
    }

    #endregion

}