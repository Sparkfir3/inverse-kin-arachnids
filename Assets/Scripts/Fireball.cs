using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fireball : MonoBehaviour {

#pragma warning disable 0649 // Disable "Field is never assigned" warning for SerializeField

    [SerializeField] private float blastRadius;

    private MeshRenderer mesh;
    private Rigidbody rb;
    private List<SpiderController> hitSpiders = new List<SpiderController>();

    private void Awake() {
        mesh = GetComponent<MeshRenderer>();
        rb = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision collision) {
        mesh.enabled = false;
        rb.velocity = Vector3.zero;

        RaycastHit[] hits = Physics.SphereCastAll(transform.position, blastRadius, transform.forward, 0f, LayerMask.GetMask("Spider", "Legs"));
        foreach(RaycastHit hit in hits) {
            SpiderController hitSpider = hit.collider.GetComponentInParent<SpiderController>();
            if(hitSpider && hitSpider.Active && !hitSpiders.Contains(hitSpider))
                hitSpiders.Add(hitSpider);
        }
        Debug.Log(gameObject.name + " hit " + hits.Length + " objects, total " + hitSpiders.Count + " spiders");

        foreach(SpiderController spider in hitSpiders) {
            try {
                spider.Active = false;
            } catch { }
        }

        Destroy(gameObject);
    }

}