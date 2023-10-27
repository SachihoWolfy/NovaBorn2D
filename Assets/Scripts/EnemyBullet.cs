using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBullet : MonoBehaviour
{
    private int damage;
    public Rigidbody2D rig;
    private Vector3 oldPosition;

    public void Initialize(int damage)
    {
        this.damage = damage;
        Destroy(gameObject, 5.0f);
    }

    private void Start()
    {
        oldPosition = transform.position;
    }
    private void Update()
    {
        // Shoot a ray between the old and new position to detect collisions
        Vector2 direction = (Vector2)(transform.position - oldPosition);
        RaycastHit2D hit = Physics2D.Raycast(oldPosition, direction, direction.magnitude);

        if (hit.collider != null)
        {
            // Handle the collision
            Debug.Log("Hit: " + hit.collider.name);
            if (hit.collider.gameObject.tag == "Player")
            {
                PlayerController player = hit.collider.GetComponent<PlayerController>();
                player.photonView.RPC("TakeDamage", player.photonPlayer, damage);
            }
            if (hit.collider.gameObject.tag != "Bullet" && hit.collider.gameObject.tag != "Enemy" && hit.collider.gameObject.tag != null && hit.collider.tag != "Pickup")
                Destroy(gameObject);
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        
    }
}
