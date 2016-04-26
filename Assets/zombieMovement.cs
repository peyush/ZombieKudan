﻿using UnityEngine;
    // Use this for initialization
    bool moveTrue;
        moveTrue = true;
    }
        Quaternion newRotation = Quaternion.LookRotation(rotationToCamera);
        transform.rotation = Quaternion.Slerp(transform.rotation, newRotation, Time.deltaTime * speed);
        {
            float step = speed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, Camera.main.gameObject.transform.position, step);
        }
        if(col.gameObject.name == "Kudan Camera")
        {
            moveTrue = false;
        }
    }

    void OnCollisionExit(Collision collisionInfo) {
        moveTrue = true;
    }
}