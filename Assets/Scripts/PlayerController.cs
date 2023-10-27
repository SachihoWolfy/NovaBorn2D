using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerController : MonoBehaviourPun
{
    [HideInInspector]
    public int id;
    [Header("Info")]
    public float moveSpeed;
    public int gold;
    public int curHp;
    public int maxHp;
    public float iTime;
    public bool dead;
    public int curShield;
    public int maxShield;
    public float shieldRate;
    public int kills;
    private float lastShieldTime;
    private float lastDamageTime;
    [Header("Attack")]
    public int damage;
    public float attackRange;
    public float attackRate;
    private float lastAttackTime;
    [Header("Components")]
    public Rigidbody2D rig;
    public Player photonPlayer;
    public SpriteRenderer sr;
    public Animator weaponAnim;
    public GameObject moveables;
    public GameObject gun;
    public PlayerWeapon weapon;
    public HeaderInfo headerInfo;

    [Header("Sounds")]
    public AudioSource AS;
    public AudioClip hurt;
    public AudioClip jump;
    public AudioClip heal;
    public AudioClip largeJump;
    public AudioClip shieldGet;
    public AudioClip shieldHurt;
    public AudioClip shieldBreak;

    // local player
    public static PlayerController me;
    // Start is called before the first frame update
    void Start()
    {
        lastShieldTime = Time.time;
        SetName();
    }

    public void SetName()
    {
        headerInfo.Initialize(photonView.Owner.NickName, maxHp);
    }
    void Update()
    {
        if (!photonView.IsMine)
            return;
        Move();
        //Aim at mouse, movables before the gun, for aiming accuracy.
        AimAtMouse(moveables);
        AimAtMouse(gun);
        //Temporary Attack
        if (Input.GetMouseButtonDown(0) && Time.time - lastAttackTime > attackRate)
            Attack();
        //NovaBorn 2 Attack
        if (Input.GetMouseButtonDown(0))
            weapon.TryShoot();
        if (Input.GetMouseButton(0))
        {
            weapon.TryRapidShoot();
            weapon.setIsFiring(true);
        }
        else
        {
            weapon.setIsFiring(false);
            weapon.stopFiring();
        }
        if (curShield > 1 && Time.time - lastShieldTime > shieldRate && Time.time - lastDamageTime > shieldRate)
        {
            Debug.Log("Trying to regen shield.");
            lastShieldTime = Time.time;
            curShield = Mathf.Clamp(curShield + 20, 0, maxShield);
            GameUI.instance.UpdateShieldBar();
            headerInfo.photonView.RPC("UpdateShieldBar", RpcTarget.All, curShield);
        }
        else
        {
            Debug.Log("Shield regen unable to at the moment. Shield time status: " + (Time.time - lastShieldTime < shieldRate));
        }
    }

    // Should Aim Given Game Object on player to the mouse. 
    // I Use it twice, once to make the whole player look (Which is an illusion, it's just the "Movables"
    // and another time to make the gun aim at the mouse, for accuracy.
    // "hands" is just a placeholder name... and the only name I could think of.
    void AimAtMouse(GameObject hands)
    {
        Vector3 dir = Input.mousePosition - Camera.main.WorldToScreenPoint(hands.transform.position);
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        hands.transform.rotation = Quaternion.AngleAxis(angle + 270, Vector3.forward);
    }
    void Move()
    {
        // get the horizontal and vertical inputs
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");
        // apply that to our velocity
        rig.velocity = new Vector2(x, y) * moveSpeed;
    }
    // melee attacks towards the mouse (!BACKUP INCASE THINGS DON'T WORK!)
    // !!! TURN OFF WHEN SHOOT ATTACK IS FULLY IMPLEMENTED !!! //
    void Attack()
    {
        lastAttackTime = Time.time;
        // calculate the direction
        /*Vector3 dir = (Input.mousePosition - Camera.main.WorldToScreenPoint(transform.position)).normalized;
        // shoot a raycast in the direction
        RaycastHit2D hit = Physics2D.Raycast(transform.position + dir, dir, attackRange);
        // did we hit an enemy?
        if (hit.collider != null && hit.collider.gameObject.CompareTag("Enemy"))
        {
            // get the enemy and damage them
            Enemy enemy = hit.collider.GetComponent<Enemy>();
            enemy.photonView.RPC("TakeDamage", RpcTarget.MasterClient, damage);
        }
        */
        // play attack animation
        weaponAnim.SetTrigger("Attack");
    }
    [PunRPC]
    public void TakeDamage(int damage)
    {
        if (dead)
        {
            return;
        }
        if (Time.time - lastDamageTime < iTime)
        {
            Debug.Log("invicibility frames");
            return;
        }
        lastDamageTime = Time.time;
        Debug.Log("Trying To Take Damage");
        if (curShield > 0)
        {
            SoundController.instance.PlaySound(AS, shieldHurt);
            curShield = Mathf.Clamp(curShield - damage, 0, curShield);
        }
        else
        {
            SoundController.instance.PlaySound(AS, hurt);
            curHp -= damage;
        }
        // flash the player red
        photonView.RPC("DamageFlash", RpcTarget.Others);
        // update the health bar UI
        GameUI.instance.UpdateHealthBar();
        GameUI.instance.UpdateShieldBar();
        // update the health bar
        headerInfo.photonView.RPC("UpdateHealthBar", RpcTarget.All, curHp);
        headerInfo.photonView.RPC("UpdateShieldBar", RpcTarget.All, curShield);
        // die if no health left
        if (curHp <= 0)
            photonView.RPC("Die", RpcTarget.All);
    }
    [PunRPC]
    void Die()
    {
        dead = true;
        rig.isKinematic = true;
        transform.position = new Vector3(0, 99, 0);
        Vector3 spawnPos = GameManager.instance.spawnPoints[Random.Range(0, GameManager.instance.spawnPoints.Length)].position;
        StartCoroutine(Spawn(spawnPos, GameManager.instance.respawnTime));
    }
    IEnumerator Spawn(Vector3 spawnPos, float timeToSpawn)
    {
        yield return new WaitForSeconds(timeToSpawn);
        dead = false;
        transform.position = spawnPos;
        curHp = maxHp;
        rig.isKinematic = false;
        // update the health bar
        headerInfo.photonView.RPC("UpdateHealthBar", RpcTarget.All, curHp);
        headerInfo.photonView.RPC("UpdateShieldBar", RpcTarget.All, curShield);
    }
    [PunRPC]
    public void Initialize(Player player)
    {
        id = player.ActorNumber;
        photonPlayer = player;
        GameManager.instance.players[id - 1] = this;
        // initialize the health bar
        headerInfo.Initialize(player.NickName, maxHp);

        if (player.IsLocal)
        {
            me = this;
            GameUI.instance.Initialize(this);
        }
        else
            rig.isKinematic = true;
    }
    [PunRPC]
    void GiveGold(int goldToGive)
    {
        gold += goldToGive;
        // update the ui
        GameUI.instance.UpdatePlayerInfoText();
    }
    public void AddKill()
    {
        kills++;
        GameUI.instance.UpdatePlayerInfoText();
    }
    [PunRPC]
    public void Heal(int amountToHeal)
    {
        curHp = Mathf.Clamp(curHp + amountToHeal, 0, maxHp);
        SoundController.instance.PlaySound(AS, heal);
        // update the health bar UI
        GameUI.instance.UpdateHealthBar();
        headerInfo.photonView.RPC("UpdateHealthBar", RpcTarget.All, curHp);
        headerInfo.photonView.RPC("UpdateShieldBar", RpcTarget.All, curShield);
    }
    [PunRPC]
    public void Shield(int amountToShield)
    {
        curShield = Mathf.Clamp(curShield + amountToShield, 0, maxShield);
        lastShieldTime = Time.time;
        SoundController.instance.PlaySound(AS, shieldGet);
        GameUI.instance.UpdateShieldBar();
        headerInfo.photonView.RPC("UpdateHealthBar", RpcTarget.All, curHp);
        headerInfo.photonView.RPC("UpdateShieldBar", RpcTarget.All, curShield);
    }
}
