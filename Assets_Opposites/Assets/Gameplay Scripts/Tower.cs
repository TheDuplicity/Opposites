using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Tower : Controllable
{
    public GameObject bulletPref;
    [SerializeField] private int maxBullets;
    [SerializeField] private float reloadSpeed;
    private float shootCooldown;
    [SerializeField] private float rotateSpeed;
    [SerializeField] private float wallSpawnRange;
    private float wallSpawnCooldown;
    private int numBullets;
    private float reloadTimer;
    private int maxMessagesStored;
    private List<GameManager.towerDefaultMessage> messages;
    // Start is called before the first frame update
    void Start()
    {
        messages = new List<GameManager.towerDefaultMessage>();
        numBullets = maxBullets;
        maxMessagesStored = 3;
    }

    // Update is called once per frame
    void Update()
    {
        updateTimer(ref reloadTimer);
        updateTimer(ref wallSpawnCooldown);
        updateTimer(ref shootCooldown);
        if (numBullets <= 0)
        {
            if (reloadTimer <= 0)
            {
                Reload();
            }
        }

        if (playerControlled)
        {
            handlePlayerControls();
        }
        else
        {
            updateFromSimulation();
        }
    }

    public void AddMessage(GameManager.towerDefaultMessage message)
    {
        while (messages.Count >= maxMessagesStored)
        {
            int oldestTimeId = 0;
            for (int i = 0; i < messages.Count; i++)
            {
                if (messages[i].time < messages[oldestTimeId].time)
                {
                    oldestTimeId = i;
                }
            }
            messages.RemoveAt(oldestTimeId);
        }
        messages.Add(message);
        messages.Sort((mes1, mes2) => mes1.time.CompareTo(mes2.time));
    }

    override public void handlePlayerControls()
    {

        if (Input.GetKey(KeyCode.Q))
        {
            rotate(false);
        }
        if (Input.GetKey(KeyCode.E))
        {
            rotate(true);
        }
        if (Input.GetMouseButtonDown(0))
        {
            if (numBullets > 0)
            {
                if (reloadTimer <= 0)
                {
                    if (shootCooldown <= 0)
                    {
                        Shoot();
                    }
                }
            }

        }
        else if (Input.GetMouseButtonDown(1))
        {
            if (wallSpawnCooldown <= 0)
            {
                SpawnWallAhead();
            }
        }
    }

    private void updateTimer(ref float timer)
    {
        if (timer > 0)
        {
            timer -= Time.deltaTime;
        }
    }

    private void updateFromSimulation()
    {
        if (messages.Count == maxMessagesStored)
        {
            Vector3 originalRotation = transform.rotation.eulerAngles;

            transform.rotation = Quaternion.Euler(originalRotation.x, originalRotation.y, messages[maxMessagesStored - 1].zRotation);

        }

    }
    public void die()
    {
        Destroy(gameObject);
    }
    public void Shoot()
    {
        if (reloadTimer > 0)
        {
            return;
        }
        if (numBullets <= 0)
        {
            return;
        }
        if (shootCooldown > 0)
        {
            return;
        }
        ((TowerShotPacket)Client.Instance.FindPacket((int)Packet.PacketID.TowerShot)).SendPacket();
        //spawn bullet with initial values
        shootCooldown = 0.35f;
        numBullets--;
        Debug.Log("shoot");

        //Bullet createBullet = bulletPref.GetComponent<Bullet>();
        GameObject createBullet = Instantiate(bulletPref);
        createBullet.GetComponent<Bullet>().createBulletData(9, 6, 25, transform.position, transform.rotation);
    }

    public void SpawnWallAhead()
    {
        //spawn wall at mouse position if in range of tower
        wallSpawnCooldown = 3.0f;
        Debug.Log("spawn wall");
    }

    public void Reload()
    {
        reloadTimer = 2.0f;
        numBullets = maxBullets;
        Debug.Log("reloading");
    }

    public void rotate(bool rotateClockwise)
    {
        if (rotateClockwise)
        {
            transform.rotation *= Quaternion.Euler(0, 0, -90 * Time.deltaTime);
            Debug.Log("rotating clockwise");
        }
        else
        {
            transform.rotation *= Quaternion.Euler(0, 0, 90 * Time.deltaTime);
            Debug.Log("rotating anti-clockwise");
        }
    }

}
