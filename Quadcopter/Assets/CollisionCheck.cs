using UnityEngine;

public class CollisionCheck : MonoBehaviour {

    void OnTriggerEnter() {
        SendMessageUpwards("Collision");
        GetComponent<Collider>().isTrigger = false;
    }

}