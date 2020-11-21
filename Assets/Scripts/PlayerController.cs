using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    [Header("Fireball")]
    [SerializeField] private GameObject fireball;
    [SerializeField] private float fireballSpeed;

    [Header("Other")]
    [SerializeField] private float cooldown;
    private float cooldownTimer;
    

    private void Update() {
        if(cooldownTimer > 0)
            cooldownTimer -= Time.deltaTime;
        else
            cooldownTimer = 0;

        if(Input.GetButtonDown("Fire1") && cooldownTimer == 0) {
            ShootFireball();
            cooldownTimer = cooldown;
        }
    }

    private void ShootFireball() {
        GameObject fireball = Instantiate(this.fireball, transform.position + (transform.forward * 0.5f), Quaternion.identity, null);
        fireball.GetComponent<Rigidbody>().velocity = transform.forward * fireballSpeed;
    }

}