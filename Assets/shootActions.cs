﻿using UnityEngine;

public class shootActions : MonoBehaviour {
    static public bool allDead = false;
    public AudioClip gunAudiofire;


    void Update()
    {
        if (shootActions.allDead == true)
        {
            shootActions.allDead = false;
            //SampleApp obj = new SampleApp();
            //obj.spawnZombie();

        }
    }
    
            //EnemyHealth enemyHealth = shootHit.collider.GetComponent<EnemyHealth>();

            // If the EnemyHealth component exist...
            //if (enemyHealth != null)
            //{
            // ... the enemy should take damage.
            //enemyHealth.TakeDamage(damagePerShot, shootHit.point);
            //}

            // Set the second position of the line renderer to the point the raycast hit.
            //gunLine.SetPosition(1, shootHit.point);
        }
            //// ... set the second position of the line renderer to the fullest extent of the gun's range.
            //    gunLine.SetPosition(1, shootRay.origin + shootRay.direction * range);
        }