using UnityEngine;using System.Collections;using UnityEngine.UI;using Kudan.AR.Samples;

public class shootActions : MonoBehaviour {    public int damagePerShot = 20;                  // The damage inflicted by each bullet.    public float range = 10000f;                     // The distance the gun can fire.
    static public bool allDead = false;    Ray shootRay;                                   // A ray from the gun end forwards.    RaycastHit shootHit;                            // A raycast hit to get information about what was hit.    int shootableMask;                              // A layer mask so the raycast only hits things on the shootable layer.    public AudioClip gunAudioMisfire;               // Reference to the audio source.
    public AudioClip gunAudiofire;    Light gunLight;                                 // Reference to the light component.    float effectsDisplayTime = 0.2f;                // The proportion of the timeBetweenBullets that the effects will display for.    public Text textBox;    public Text ScoreTextbox;    public static int zombieKilled;    void Awake()    {        // Create a layer mask for the Shootable layer.        shootableMask = LayerMask.GetMask("Shootable");        //allDead = false;    }


    void Update()
    {
        if (shootActions.allDead == true)
        {
            shootActions.allDead = false;
            //SampleApp obj = new SampleApp();
            //obj.spawnZombie();

        }
    }
        public void Shoot()    {                // Set the shootRay so that it starts at the end of the gun and points forward from the barrel.        shootRay.origin = Camera.main.gameObject.transform.position;        shootRay.direction = Camera.main.gameObject.transform.forward;        Debug.DrawRay(shootRay.origin, shootRay.direction);        // Perform the raycast against gameobjects on the shootable layer and if it hits something...        if (Physics.Raycast(shootRay, out shootHit, Mathf.Infinity, shootableMask))        {            textBox.text = "Zombie got hahaha";            AudioSource.PlayClipAtPoint(gunAudiofire, Vector3.zero);            //Destroy(shootHit.collider.gameObject);            allDead = true;            zombieKilled++;            ScoreTextbox.text = "Score: " + zombieKilled;            // Try and find an EnemyHealth script on the gameobject hit.
            //EnemyHealth enemyHealth = shootHit.collider.GetComponent<EnemyHealth>();

            // If the EnemyHealth component exist...
            //if (enemyHealth != null)
            //{
            // ... the enemy should take damage.
            //enemyHealth.TakeDamage(damagePerShot, shootHit.point);
            //}

            // Set the second position of the line renderer to the point the raycast hit.
            //gunLine.SetPosition(1, shootHit.point);
        }        // If the raycast didn't hit anything on the shootable layer...        else        {            //textBox.text = "Zombie not";            AudioSource.PlayClipAtPoint(gunAudioMisfire, Vector3.zero);
            //// ... set the second position of the line renderer to the fullest extent of the gun's range.
            //    gunLine.SetPosition(1, shootRay.origin + shootRay.direction * range);
        }    }}