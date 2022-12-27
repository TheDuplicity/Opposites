using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Minion : Controllable
{
    [SerializeField] private float speed;
    [SerializeField] private float health;
    private List<GameManager.minionDefaultMessage> messages;
    private int maxMessagesStored;
    // Start is called before the first frame update
    void Start()
    {
        // speed = 1;
        // health = 100;
        maxMessagesStored = 3;
        messages = new List<GameManager.minionDefaultMessage>();
    }

    // Update is called once per frame
    void Update()
    {
        predictPosition();
            
        
    }

    private void predictPosition()
    {
        if (messages.Count == maxMessagesStored)
        {
            transform.position = new Vector3(messages[maxMessagesStored - 1].position.x, messages[maxMessagesStored - 1].position.y, transform.position.z);
        }
    }

    public void AddMessage(GameManager.minionDefaultMessage message)
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

    private void OnTriggerEnter2D(Collider2D collision)
    {

        GameObject colliderObj = collision.gameObject;
        Debug.Log("collided");
        if (colliderObj.tag == "Bullet")
        {
            Debug.Log(" collided with bullet");
            Bullet bullet = colliderObj.GetComponent<Bullet>();
            takeDamage(bullet.dealDamage());
            Destroy(colliderObj);
        }

    }

    private void OnTriggerStay2D(Collider2D collision)
    {

        GameObject colliderObj = collision.gameObject;
        if (colliderObj.tag == "TowerTile")
        {
            Debug.Log("inside tile");
            Vector3 colliderTowardMe = transform.position - colliderObj.transform.position;
            colliderTowardMe.Normalize();
            transform.position += colliderTowardMe * 0.03f * speed;
        }
        if (colliderObj.tag == "FinishTile")
        {
            //increment score
            GameManager.Instance.minionScore ++;
            die();
        }
    }

   

    public void takeDamage(float damage)
    {
        health -= damage;
        if (health <= 0)
        {
            GameManager.Instance.towerScore++;
            die();
        }
    }
    public void die()
    {
        Debug.Log("death");
        //GameManager.Instance.KillPlayerAndUpdateClients(GetComponent<Controllable>().getId());
    }

}
