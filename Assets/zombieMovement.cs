using UnityEngine;using System.Collections;public class zombieMovement : MonoBehaviour {    public float speed;
    // Use this for initialization
    bool moveTrue;	void Start () {
        moveTrue = true;
    }		// Update is called once per frame	void Update () {        Vector3 rotationToCamera = Camera.main.transform.position - transform.position;
        Quaternion newRotation = Quaternion.LookRotation(rotationToCamera);
        transform.rotation = Quaternion.Slerp(transform.rotation, newRotation, Time.deltaTime * speed);        if (moveTrue == true)
        {
            float step = speed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, Camera.main.gameObject.transform.position, step);
        }    }    void OnCollisionEnter(Collision col) {
        if(col.gameObject.name == "Kudan Camera")
        {
            moveTrue = false;
        }
    }

    void OnCollisionExit(Collision collisionInfo) {
        moveTrue = true;
    }
}