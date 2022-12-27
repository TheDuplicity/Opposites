using System.Collections;
using System.Collections.Generic;
using UnityEngine;





public class Minion : Controllable
{
    [SerializeField] private float speed;
    [SerializeField] private float health;
    private Vector3 whereToMove;
    private Vector3 currentPredictedPos;
    private List<GameManager.minionDefaultMessage> messages;
    private int maxMessagesStored;

    private float timerToInterpolate;
    private float interpolateTimeCutoff;
    // Start is called before the first frame update
    void Start()
    {
        // speed = 1;
        // health = 100;
        messages = new List<GameManager.minionDefaultMessage>();
        maxMessagesStored = 3;
        timerToInterpolate = 0;
        interpolateTimeCutoff = 0.05f;
    }

    // Update is called once per frame
    void Update()
    {
        if (playerControlled)
        {
            handlePlayerControls();

        }
        else
        {
            timerToInterpolate += Time.deltaTime;
            updateFromSimulation();
        }
    }

    public void AddMessage(GameManager.minionDefaultMessage message)
    {
        currentPredictedPos = whereShouldIBe();
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

        //this is where i am right now
        // this is where i think i should end up in by this time with the new simulation
        // 
        Vector3 whereIShouldBe = whereShouldIBe();

        whereToMove = whereIShouldBe - currentPredictedPos;
        timerToInterpolate = 0;
    }

    override public void handlePlayerControls()
    {
        float x = 0, y = 0;
        if (Input.GetKey(KeyCode.W))
        {
            y++;
        }
        if (Input.GetKey(KeyCode.S))
        {
            y--;
        }
        if (Input.GetKey(KeyCode.D))
        {
            x++;
        }
        if (Input.GetKey(KeyCode.A))
        {
            x--;
        }
        Vector2 move = new Vector2(x, y);
        move.Normalize();
        move *= speed * Time.deltaTime;
        transform.position += new Vector3(move.x, move.y, 0);
    }

    private Vector3 whereShouldIBe()
    {
        if (messages.Count < maxMessagesStored)
        {
            return transform.position;
        }

        GameManager.minionDefaultMessage messageMid = messages[1];
        GameManager.minionDefaultMessage messageNew = messages[2];

        float timeSinceNewest = GameManager.Instance.gameTime - messageNew.time;


        Vector2 distanceBetweenMessages = messageNew.position - messageMid.position;
        float timeBetweenMessages = messageNew.time - messageMid.time;

        Vector2 speed = distanceBetweenMessages / timeBetweenMessages;

        Vector2 newPosDisplacement = (speed * timeSinceNewest);

        return new Vector3(newPosDisplacement.x + messageNew.position.x, newPosDisplacement.y + messageNew.position.y, transform.position.z);

    }


    private void updateFromSimulation()
    {
        if (messages.Count < maxMessagesStored)
        {
            return;
        }
        if (timerToInterpolate > interpolateTimeCutoff)
        {
            timerToInterpolate = interpolateTimeCutoff;
        }

        Vector3 interpolateAddition = whereToMove * (timerToInterpolate / interpolateTimeCutoff);
        GameManager.minionDefaultMessage messageOld = messages[0];
        GameManager.minionDefaultMessage messageMid = messages[1];
        GameManager.minionDefaultMessage messageNew = messages[2];
        //transform.position = new Vector3(messages[maxMessagesStored - 1].position.x, messages[maxMessagesStored - 1].position.y, transform.position.z);

        float timeSinceNewest = GameManager.Instance.gameTime - messageNew.time;


        Vector2 distanceBetweenMessages = messageNew.position - messageMid.position;
        float timeBetweenMessages = messageNew.time - messageMid.time;

        Vector2 speed = distanceBetweenMessages / timeBetweenMessages;

        Vector2 newPosDisplacement = (speed * timeSinceNewest);

        Debug.Log($"speed: {speed}, distance covered: {newPosDisplacement}");



        transform.position = new Vector3(newPosDisplacement.x + messageNew.position.x/* + interpolateAddition.x*/, newPosDisplacement.y + messageNew.position.y/* + interpolateAddition.y*/, transform.position.z);
        //currentPredictedPos = transform.position;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {

        GameObject colliderObj = collision.gameObject;
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
            //GameManager.Instance.minionScore ++;
            // die();
        }
    }



    public void takeDamage(float damage)
    {
        health -= damage;
        if (health <= 0)
        {
            // GameManager.Instance.towerScore++;
            // die();
        }
    }
    public void die()
    {
        Destroy(gameObject);
    }

}


