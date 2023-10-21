using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private int damage;
    private int attackerId;
    private bool isMine;
    public Rigidbody2D rig;
    private Vector3 oldPosition;

    public void Initialize(int damage, int attackerId, bool isMine)
    {
        this.damage = damage;
        this.attackerId = attackerId;
        this.isMine = isMine;
        Destroy(gameObject, 5.0f);
    }

    private void Start()
    {
        oldPosition = transform.position;
    }
    private void Update()
    {
        // Shoot a ray between the old and new position to detect collisions
        RaycastHit hit;
        if (Physics.Raycast(oldPosition, transform.position - oldPosition, out hit, (transform.position - oldPosition).magnitude))
        {
            // Handle the collision
            //Debug.Log("Hit: " + hit.collider.name);
            if (hit.collider.gameObject.tag == "Enemy" && isMine)
            {
                //Implement New Hit Function To deal with enemys once they are made.

                /*PlayerController player = GameManager.instance.GetPlayer(hit.collider.gameObject);
                if (player.id != attackerId)
                {
                    player.photonView.RPC("TakeDamage", player.photonPlayer, attackerId, damage);
                }*/
            }
            if (hit.collider.gameObject.tag != "Bullet")
                Destroy(gameObject);
        }
    }
}
