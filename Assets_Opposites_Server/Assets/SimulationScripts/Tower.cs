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

        numBullets = maxBullets;
        maxMessagesStored = 3;
        messages = new List<GameManager.towerDefaultMessage>();
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

        predictRotation();

    }

    private void predictRotation()
    {
        if (messages.Count == maxMessagesStored)
        {
            Vector3 originalRotation = transform.rotation.eulerAngles;

            transform.rotation = Quaternion.Euler(originalRotation.x, originalRotation.y, messages[maxMessagesStored - 1].zRotation);

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


    private void updateTimer(ref float timer)
    {
        if (timer > 0)
        {
            timer -= Time.deltaTime;
        }
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
        //spawn bullet with initial values
        shootCooldown = 0.35f;
        numBullets--;

        //Bullet createBullet = bulletPref.GetComponent<Bullet>();
        GameObject createBullet = Instantiate(bulletPref);
        createBullet.GetComponent<Bullet>().createBulletData(9, 6, 25,transform.position, transform.rotation);
    }

    public void SpawnWallAhead()
    {
        //spawn wall at mouse position if in range of tower
        wallSpawnCooldown = 3.0f;
        Debug.Log("spawn wall");
    }

    private void Reload()
    {
        reloadTimer = 2.0f;
        numBullets = maxBullets;
        Debug.Log("reloading");
    }

    public void rotate(bool rotateClockwise)
    {
        if (rotateClockwise) {
            transform.rotation *= Quaternion.Euler(0,0,-90 * Time.deltaTime);
            Debug.Log("rotating clockwise");
        }
        else
        {
            transform.rotation *= Quaternion.Euler(0, 0, 90 * Time.deltaTime);
            Debug.Log("rotating anti-clockwise");
        }
    }

    public void die()
    {

        ((PlayerDiedPacket)Server.Instance.FindPacket((int)Packet.PacketID.PlayerDied)).SendPacket(GameManager.Instance.GetInGamePlayerIDs(), GetComponent<Controllable>().getId());
        GameManager.Instance.removeControllableFromGame(GetComponent<Controllable>().type, GetComponent<Controllable>().getId());
        Destroy(gameObject);
    }

}
