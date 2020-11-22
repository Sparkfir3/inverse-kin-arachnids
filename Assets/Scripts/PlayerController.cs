using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

#pragma warning disable 0649 // Disable "Field is never assigned" warning for SerializeField

    [Header("Fireball")]
    [SerializeField] private GameObject fireball;
    [SerializeField] private float fireballSpeed;

    [Header("Other")]
    [SerializeField] private float cooldown;
    private float cooldownTimer;

    [SerializeField] private GameObject spiderBody, spiderLeg;


    private void Update() {
        if(!GameManager.manager.Paused) {
            if(cooldownTimer > 0)
                cooldownTimer -= Time.deltaTime;
            else
                cooldownTimer = 0;

            // Fireball
            if(Input.GetButtonDown("Fire1") && cooldownTimer == 0) {
                ShootFireball();
                cooldownTimer = cooldown;
            }

            // Body
            /*if(Input.GetButtonDown("Fire2")) {
                SpawnBody();
            }*/

            // Leg
            if(Input.GetButtonDown("Fire3")) {
                SpawnLeg();
            }
        }
    }

    private void ShootFireball() {
        GameObject fireball = Instantiate(this.fireball, transform.position + (transform.forward * 0.5f), Quaternion.identity, null);
        fireball.GetComponent<Rigidbody>().velocity = transform.forward * fireballSpeed;
    }

    private void SpawnBody() {
        Instantiate(spiderBody, transform.position + (transform.forward * 0.5f), Quaternion.identity, null);
    }

    private void SpawnLeg() {
        GameObject leg = Instantiate(spiderLeg, transform.position + (transform.forward * 2f), transform.rotation, null);
        leg.GetComponent<LegController>().Active = false;
    }

}