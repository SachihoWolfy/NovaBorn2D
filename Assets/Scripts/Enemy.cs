using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
public class Enemy : MonoBehaviourPun
{
    [Header("Info")]
    public string enemyName;
    public float moveSpeed;
    public int curHp;
    public int maxHp;
    public float chaseRange;
    public float attackRange;
    public float bulletSpeed;
    private PlayerController targetPlayer;
    public float playerDetectRate = 0.2f;
    private float lastPlayerDetectTime;
    public string objectToSpawnOnDeath;
    [Header("Attack")]
    public int damage;
    public float attackRate;
    private float lastAttackTime;
    [Header("Components")]
    public HeaderInfo healthBar;
    public SpriteRenderer sr;
    public Rigidbody2D rig;
    public GameObject moveables;
    public GameObject gun;
    public GameObject bulletPrefab;

    // Start is called before the first frame update
    void Start()
    {
        healthBar.Initialize(enemyName, maxHp);
    }


    // Update is called once per frame
    void Update()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;
        if (targetPlayer != null)
        {
            // calculate the distance
            float dist = Vector3.Distance(transform.position, targetPlayer.transform.position);
            // look at player
            AimAtPlayer(moveables);
            // Aim at player (With gun)
            AimAtPlayer(gun);
            // if we're able to attack, do so
            if (dist < attackRange && Time.time - lastAttackTime >= attackRange)
                Attack();
            // otherwise, do we move after the player?
            else if (dist > attackRange)
            {
                Vector3 dir = targetPlayer.transform.position - transform.position;
                rig.velocity = dir.normalized * moveSpeed;
            }
            else
            {
                rig.velocity = Vector2.zero;
            }
        }
        DetectPlayer();
    }

    // Aims at target player
    public void AimAtPlayer(GameObject hands)
    {
        Vector3 dir = targetPlayer.transform.position - hands.transform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        hands.transform.rotation = Quaternion.AngleAxis(angle + 270, Vector3.forward);
    }
    // attacks the targeted player
    void Attack()
    {
        lastAttackTime = Time.time;
        targetPlayer.photonView.RPC("TakeDamage", targetPlayer.photonPlayer, damage);
    }
    [PunRPC]
    void SpawnEnemyBullet(Vector3 pos, Vector3 dir)
    {
        // spawn and orientate it
        GameObject bulletObj = Instantiate(bulletPrefab, pos, Quaternion.identity);
        bulletObj.transform.forward = dir;
        // get bullet script
        EnemyBullet bulletScript = bulletObj.GetComponent<EnemyBullet>();
        // initialize it and set the velocity
        bulletScript.Initialize(damage);
        bulletScript.rig.velocity = dir * bulletSpeed;
    }
    // updates the targeted player
    void DetectPlayer()
    {
        if (Time.time - lastPlayerDetectTime > playerDetectRate)
        {
            lastPlayerDetectTime = Time.time;
            // loop through all the players
            foreach (PlayerController player in GameManager.instance.players)
            {
                // calculate distance between us and the player
                float dist = Vector2.Distance(transform.position, player.transform.position);
                if (player == targetPlayer)
                {
                    if (dist > chaseRange)
                        targetPlayer = null;
                }
                else if (dist < chaseRange)
                {
                    if (targetPlayer == null)
                        targetPlayer = player;
                }
            }
        }
    }
    [PunRPC]
    public void TakeDamage(int damage)
    {
        curHp -= damage;
        // update the health bar
        healthBar.photonView.RPC("UpdateHealthBar", RpcTarget.All, curHp);
        if (curHp <= 0)
            Die();
        else
        {
            photonView.RPC("FlashDamage", RpcTarget.All);
        }
    }
    [PunRPC]
    void FlashDamage()
    {
        StartCoroutine(DamageFlash());
        IEnumerator DamageFlash()
        {
            sr.color = Color.red;
            yield return new WaitForSeconds(0.05f);
            sr.color = Color.white;
        }
    }
    void Die()
    {
        if (objectToSpawnOnDeath != string.Empty)
            PhotonNetwork.Instantiate(objectToSpawnOnDeath, transform.position, Quaternion.identity);
        // destroy the object across the network
        PhotonNetwork.Destroy(gameObject);
    }

}
