using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SpiderController : MonoBehaviour {

    public float rotSpeed = 10.0f;
    public float casterOffset = 1.0f;
    //public float bodyHeight = 1.0f;

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
    [SerializeField] private bool runAI = true;
    private Coroutine move;

    [Header("Re-assembly")]
    [SerializeField] private float rebuildHeightOffset;

    // -------------------------------------------------------------------------------------------------------------

    #region Initalization

    private void Awake() {
        rb = GetComponent<Rigidbody>();
        boxCollider = GetComponent<BoxCollider>();
        agent = GetComponent<NavMeshAgent>();

        InitializeLegs();
        reset();
    }

    private void Start() {
        Active = legs.Count >= 2;
        foreach(GameObject leg in legs)
            leg.GetComponent<LegController>().Active = true;
    }

    private void InitializeLegs() {
        //Set target caster positions for each leg
        for(int i = 0; i < legs.Count; i++) {
            Transform t = legs[i].GetComponent<LegController>().targetCaster;

            switch(i) {
                case 0:
                    t.localPosition = new Vector3(t.localPosition.x - casterOffset, t.localPosition.y, t.localPosition.z);
                    break;
                case 1:
                    t.localPosition = new Vector3(t.localPosition.x + casterOffset, t.localPosition.y, t.localPosition.z);
                    break;
                case 2:
                    t.localPosition = new Vector3(t.localPosition.x + casterOffset, t.localPosition.y, t.localPosition.z);
                    break;
                case 3:
                    t.localPosition = new Vector3(t.localPosition.x - casterOffset, t.localPosition.y, t.localPosition.z);
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

    public void reset()
    {
        agent.speed = moveSpeed;

        // Set legs, if not already set
        if(legs.Count == 0) {
            foreach(LegController leg in GetComponentsInChildren<LegController>())
                legs.Add(leg.gameObject);
        }

        // Set target casters
        for (int i = 0; i < legs.Count; i++)
        {
            legs[i].GetComponent<LegController>().legNum = i;
            Transform t = legs[i].GetComponent<LegController>().targetCaster;
            RaycastHit hit;

            //Send out a raycast so legs can be placed at default positions
            if (Physics.Raycast(t.transform.position, Vector3.down, out hit, Mathf.Infinity, LayerMask.GetMask("Terrain")))
            {
                if (legs.Count == 2)
                {
                    switch (i)
                    {
                        case 0:
                            //Place leg 0 half a step forward
                            legs[0].GetComponent<LegController>().footStopPos = new Vector3(hit.point.x, hit.point.y, hit.point.z + (legs[0].GetComponent<LegController>().distThreshold / 2));
                            break;
                        case 1:
                            legs[1].GetComponent<LegController>().footStopPos = hit.point;
                            break;
                    }
                }
                else
                {
                    switch (i)
                    {
                        case 0:
                            //Place leg 0 half a step forward
                            legs[0].GetComponent<LegController>().footStopPos = new Vector3(hit.point.x, hit.point.y, hit.point.z + (legs[0].GetComponent<LegController>().distThreshold / 2));
                            break;
                        case 1:
                            legs[1].GetComponent<LegController>().footStopPos = hit.point;
                            break;
                        case 2:
                            legs[2].GetComponent<LegController>().footStopPos = hit.point;
                            break;
                        case 3:
                            //Place leg 3 half a step forward
                            legs[3].GetComponent<LegController>().footStopPos = new Vector3(hit.point.x, hit.point.y, hit.point.z + (legs[3].GetComponent<LegController>().distThreshold / 2));
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
    }

    #endregion

    // -------------------------------------------------------------------------------------------------------------

    #region Repeating

    private void Update()
    {
        if(!Active)
            return;

        bool stepAnyway = true;

        //Cycle through the legs, allowing them to take a step if appropriate
        for (int i = 0; i < legs.Count; i++)
        {
            if (legs.Count == 2)
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

                //If all legs are grounded and cannot step, prepare to force them to step anyway
                if (stepAnyway && legs[i].GetComponent<LegController>().canStep == false &&
                    legs[0].GetComponent<LegController>().isGrounded && legs[1].GetComponent<LegController>().isGrounded) { }
                else
                {
                    stepAnyway = false;
                }
            }

            if (legs.Count == 4)
            {
                if (i == 0 || i == 3)
                {
                    //If legs 1&2 are grounded and leg 1's travel distance is between 1/4 and 3/4
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

                //If all legs are grounded and cannot step, prepare to force them to step anyway
                if (stepAnyway && legs[i].GetComponent<LegController>().canStep == false &&
                    legs[0].GetComponent<LegController>().isGrounded && legs[1].GetComponent<LegController>().isGrounded && legs[2].GetComponent<LegController>().isGrounded && legs[3].GetComponent<LegController>().isGrounded)
                { }
                else
                {
                    stepAnyway = false;
                }
            }
        }

        //If the legs are lagging behind and need to be forced to step
        if (legs.Count == 2 && stepAnyway)
        {
            //Check which of the front legs is lagging behind the most, then make that leg step forward
            if (transform.InverseTransformPoint(legs[0].GetComponent<LegController>().footStopPos).z <= transform.InverseTransformPoint(legs[1].GetComponent<LegController>().footStopPos).z)
            {
                legs[0].GetComponent<LegController>().canStep = true;
            }
            else
            {
                legs[1].GetComponent<LegController>().canStep = true;
            }
        }
        else if (legs.Count == 4 && stepAnyway)
        { 
            //Check which of the front legs is lagging behind the most, then make those legs step forward
            if(transform.InverseTransformPoint(legs[0].GetComponent<LegController>().footStopPos).z <= transform.InverseTransformPoint(legs[1].GetComponent<LegController>().footStopPos).z)
            {
                legs[0].GetComponent<LegController>().canStep = true;
                legs[3].GetComponent<LegController>().canStep = true;
            }
            else
            {
                legs[1].GetComponent<LegController>().canStep = true;
                legs[2].GetComponent<LegController>().canStep = true;
            }
        }


        if(legs.Count == 2)
        {
            //Take the average height of the right and left legs

        }
        if(legs.Count == 4 && legs[0].GetComponent<LegController>().isGrounded && legs[1].GetComponent<LegController>().isGrounded && legs[2].GetComponent<LegController>().isGrounded && legs[3].GetComponent<LegController>().isGrounded)
        {
            float avgFX = (legs[1].GetComponent<LegController>().footStopPos.x + legs[3].GetComponent<LegController>().footStopPos.x) / 2;
            float avgBX = (legs[0].GetComponent<LegController>().footStopPos.x + legs[2].GetComponent<LegController>().footStopPos.x) / 2;
            float avgFY = (legs[1].GetComponent<LegController>().footStopPos.y + legs[3].GetComponent<LegController>().footStopPos.y) / 2;
            float avgBY = (legs[0].GetComponent<LegController>().footStopPos.y + legs[2].GetComponent<LegController>().footStopPos.y) / 2;
            float slopeFB = (avgFY - avgBY) / (avgFX - avgBX);
            float angleFB = Mathf.Rad2Deg * (Mathf.Atan(slopeFB));


            float LX = legs[1].GetComponent<LegController>().footStopPos.x;
            float RX = legs[0].GetComponent<LegController>().footStopPos.x;
            float avgLY = (legs[1].GetComponent<LegController>().footStopPos.y + legs[3].GetComponent<LegController>().footStopPos.y) / 2;
            float avgRY = (legs[0].GetComponent<LegController>().footStopPos.y + legs[2].GetComponent<LegController>().footStopPos.y) / 2;
            float slopeLR = (avgLY - avgRY) / (LX - RX);
            float angleLR = Mathf.Rad2Deg * (Mathf.Atan(slopeLR));

            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(-angleFB, transform.rotation.eulerAngles.y, angleLR), rotSpeed * Time.deltaTime);
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
            //boxCollider.enabled = !_active;
            agent.enabled = _active;

            // Disable legs
            if(!_active) {
                foreach(GameObject leg in legs) {
                    leg.GetComponent<LegController>().Active = false;
                    leg.GetComponent<LegController>().owned = false;
                    leg.transform.parent = null;
                }
                legs.Clear();
                legTargets.Clear();
            }

            // Movement
            if(runAI)
                if(_active)
                    move = StartCoroutine(Navigate());
                else if(move != null)
                    StopCoroutine(move);

            // Start rebuild try routine
            if(!_active)
                StartCoroutine(TryRebuild());
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
            while(agent.remainingDistance > 2f)
                yield return null;
            yield return new WaitForSeconds(Random.Range(1f, 2.5f));
        }
    }

    private IEnumerator TryRebuild() {
        while(!_active) {
            yield return new WaitForSeconds(5f);
            AttemptRebuild();
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
            if(hitLeg && !hitLeg.owned && !legs.Contains(hitLeg.gameObject) && legs.Count < 4) {
                hitLeg.owned = true;
                legs.Add(hitLeg.gameObject);
            }
        }

        // 1/10 chance to only have 2 legs
        if(legs.Count == 4 && Random.Range(0, 9) == 0) {
            legs.RemoveAt(3);
            legs.RemoveAt(2);
        }

        Debug.Log(gameObject.name + " attempting to rebuild, found " + legs.Count + " legs");
        return legs;
    }

    private IEnumerator Rebuild(List<GameObject> newLegs) {
        // Make number of legs even
        if(newLegs.Count % 2 == 1)
            newLegs.RemoveAt(newLegs.Count - 1);

        // Freeze rigidbody
        rb.constraints = RigidbodyConstraints.FreezeAll;
        boxCollider.enabled = false;
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

        // Set legs
        InitializeLegs();
        reset();

        Active = true;
        boxCollider.enabled = true;
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

    private IEnumerator SetNewLegs(List<GameObject> newLegs) { // Set legs active
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
                baseLegPositions.Add(new Vector3(0f, 0.123f, 0.465f));
                baseLegPositions.Add(new Vector3(0f, 0.123f, 0.465f));
                break;
            case 4:
                baseLegPositions.Add(new Vector3(0f, 0.123f, 0.465f));
                baseLegPositions.Add(new Vector3(0f, 0.123f, 0.465f));
                baseLegPositions.Add(new Vector3(0f, 0.123f, 0.278f));
                baseLegPositions.Add(new Vector3(0f, 0.123f, 0.278f));
                break;
            default:
                break;
        }
    }

    #endregion

}