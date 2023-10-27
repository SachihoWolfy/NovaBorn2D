using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private int damage;
    private int attackerId;
    private bool isMine;
    private bool isEnemy;
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
        Vector2 direction = (Vector2)(transform.position - oldPosition);
        RaycastHit2D hit = Physics2D.Raycast(oldPosition, direction, direction.magnitude);

        if (hit.collider != null)
        {
            // Handle the collision
            Debug.Log("Hit: " + hit.collider.name);
            if (hit.collider.gameObject.tag == "Enemy")
            {
                Enemy enemy = hit.collider.GetComponent<Enemy>();
                enemy.photonView.RPC("TakeDamage", Photon.Pun.RpcTarget.MasterClient, damage);
            }
            if (hit.collider.gameObject.tag != "Bullet" && hit.collider.gameObject.tag != "Player" && hit.collider.gameObject.tag != null && hit.collider.tag != "Pickup")
                Destroy(gameObject);
        }
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        
    }
}
