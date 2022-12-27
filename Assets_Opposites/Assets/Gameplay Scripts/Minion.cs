using System.Collections;
using System.Collections.Generic;
using UnityEngine;





public class Minion : Controllable
{
    [SerializeField] private float speed;
    [SerializeField] private float health;
    private Vector2 distToAccuratePredictedPos;
    private Vector2 velocityTowardsPredictedPos;
    private float timeToInterpolateToAccuratePredictedPos = 0.125f;
    private Vector3 positionAtLatestMessageReceive;
    private float timeAtLatestMessageReceive;
    private List<GameManager.minionDefaultMessage> messages;
    private int maxMessagesStored;

    private float timerToInterpolate;

    // Start is called before the first frame update
    void Start()
    {
        // speed = 1;
        // health = 100;
        messages = new List<GameManager.minionDefaultMessage>();
        maxMessagesStored = 3;
        timerToInterpolate = 0;
        //interpolateTimeCutoff = 0.05f;
        distToAccuratePredictedPos = new Vector2(0, 0);
        velocityTowardsPredictedPos = new Vector2(0, 0);
        positionAtLatestMessageReceive = new Vector3(0, 0, 0);
        timeAtLatestMessageReceive = 0;
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
            ////if we've been interpolating the over the time needed to move the full distance, set the time to the max value, otherwise add delta time and check again. if its equal, do nothing to the timer
            if (timerToInterpolate > timeToInterpolateToAccuratePredictedPos)
            {
                timerToInterpolate = timeToInterpolateToAccuratePredictedPos;
            }
            else if (timerToInterpolate < timeToInterpolateToAccuratePredictedPos)
            {
                timerToInterpolate += Time.deltaTime;
                if (timerToInterpolate > timeToInterpolateToAccuratePredictedPos)
                {
                    timerToInterpolate = timeToInterpolateToAccuratePredictedPos;
                }
            }

            updateFromSimulation();
        }
    }

    private void AddMessage(GameManager.minionDefaultMessage message)
    {
        messages.Add(message);
        if (messages.Count > 1)
        {
            messages.Sort((mes1, mes2) => mes1.time.CompareTo(mes2.time));
        }
    }

    public void HandleNewMessage(GameManager.minionDefaultMessage message)
    {
        // if we dont hav enough messages in the buffer to do interpolation or prediction, just add the message and do not calculate anything   
        if (messages.Count < maxMessagesStored)
        {
            AddMessage(message);
            return;
        }

        positionAtLatestMessageReceive = transform.position;
        timeAtLatestMessageReceive = GameManager.Instance.gameTime;

        
        int oldestTimeId = 0;

        while (messages.Count >= maxMessagesStored)
        {
            for (int i = 0; i < messages.Count; i++)
            {
                if (messages[i].time < messages[oldestTimeId].time)
                {
                    oldestTimeId = i;
                }
            }
            //handle the case where we have the oldest message id and the number of messages is exactly full outside the loop
            if (messages.Count == maxMessagesStored)
            {
                break;
            }
        }

        // compare the oldest packet in the full list with the new packet, if the oldest packet is newer, disregard the new packet
        if (messages.Count >= maxMessagesStored)
        {
            if (message.time > messages[oldestTimeId].time)
            {
                messages.RemoveAt(oldestTimeId);
            }
            else
            {
                // if the new message is older then dont do anything extra with prediction/interpolation
                return;
            }
           
        }

        AddMessage(message);

        // this is where i should be right now with the new simulation from the new message
        Vector3 WhereIShouldBe = PredictedPosFromMessages();

        // get the distance from where i am now to where i should be and calculate a velocity based on how quickly i want to return to my original spot
        distToAccuratePredictedPos = new Vector2(WhereIShouldBe.x - positionAtLatestMessageReceive.x, WhereIShouldBe.y - positionAtLatestMessageReceive.y);
        //velocity towards our best prediction from current
        velocityTowardsPredictedPos = distToAccuratePredictedPos / timeToInterpolateToAccuratePredictedPos;
        // timer to calculate interpolation distance based on time elapsed and to cap out distance
        timerToInterpolate = 0;


    }

    public void justinterpolationtest()
    {

        GameManager.minionDefaultMessage messageNew = messages[2];
        // this is where i should be right now with the new simulation from the new message
        Vector2 WhereIShouldBe = messageNew.position;

        // find the distance to the position i should be at right now from where i am and use it to calculate a velocity towards that location and how long i will need to move for
        distToAccuratePredictedPos = new Vector2(WhereIShouldBe.x - transform.position.x, WhereIShouldBe.y - transform.position.y);
        //velocity towards our best prediction from current
        velocityTowardsPredictedPos = distToAccuratePredictedPos / timeToInterpolateToAccuratePredictedPos;
        // timer to calculate interpolation distance based on time elapsed and to cap out distance
        timerToInterpolate = 0;
    }

    public void interptestupdate()
    {
        GameManager.minionDefaultMessage messageNew = messages[1];

        Vector2 interpolationMoveDist = new Vector2(0, 0);
        //only calculate interpolation if we arent where we should be
        if (distToAccuratePredictedPos.x != 0.0f || distToAccuratePredictedPos.y != 0.0f)
        {
            //calculate distance we will move this timepoint as a result of interpolation
            interpolationMoveDist = velocityTowardsPredictedPos * Time.deltaTime;
            if (interpolationMoveDist.magnitude > distToAccuratePredictedPos.magnitude)
            {
                interpolationMoveDist = distToAccuratePredictedPos;

            }
            distToAccuratePredictedPos -= interpolationMoveDist;

        }

        transform.position = new Vector3(interpolationMoveDist.x + transform.position.x, interpolationMoveDist.y + transform.position.y, transform.position.y);

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


    private Vector3 PredictedPosFromMessagesPlusInterpolation()
    {
        Vector2 currentTargetVelocity = getVelocityFromLatestMessages();

        Vector2 interpolationMoveDist = new Vector2(0, 0);
        //only calculate interpolation if we arent where we should be
        if (distToAccuratePredictedPos.x != 0.0f || distToAccuratePredictedPos.y != 0.0f)
        {
            //calculate distance we will move this timepoint as a result of interpolation
            interpolationMoveDist = velocityTowardsPredictedPos * timerToInterpolate;
            if (interpolationMoveDist.magnitude > distToAccuratePredictedPos.magnitude) 
            {
                interpolationMoveDist = distToAccuratePredictedPos;
            }

        }

        float timeSinceLastMessage = GameManager.Instance.gameTime - timeAtLatestMessageReceive;

        //take the position at the time of the packet, the distance we would move from that position using the velocity from the latest packets and add the interpolation distance to move towards the correct position
        //essentially keep moving from the location you were at at the latest packet but now move in the direction you should be and use interpolation to return to the correct line of action
        Vector2 newPosition = new Vector2(positionAtLatestMessageReceive.x + interpolationMoveDist.x + currentTargetVelocity.x * timeSinceLastMessage, positionAtLatestMessageReceive.y + interpolationMoveDist.y + currentTargetVelocity.y * timeSinceLastMessage);

        return newPosition;

    }

    private Vector2 getVelocityFromLatestMessages()
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

        return speed;
    }

    private Vector3 PredictedPosFromMessages()
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
        //Vector2 interpolationMoveDist = new Vector2(0,0);
        //only calculate interpolation if we arent where we should be
        //if (distToAccuratePredictedPos.x != 0.0f || distToAccuratePredictedPos.y != 0.0f) {
        //    calculate distance we will move this frame as a result of interpolation
        //    interpolationMoveDist = velocityTowardsPredictedPos * timerToInterpolate;
        //    if (interpolationMoveDist.magnitude > distToAccuratePredictedPos.magnitude) ;
        //    {
        //        interpolationMoveDist = distToAccuratePredictedPos;
        //    }

        //    distToAccuratePredictedPos -= interpolationMoveDist;
        //}


        ////Vector3 interpolateAddition = whereToMove * (timerToInterpolate / interpolateTimeCutoff);
        //GameManager.minionDefaultMessage messageOld = messages[0];
        //GameManager.minionDefaultMessage messageMid = messages[1];
        //GameManager.minionDefaultMessage messageNew = messages[2];
        ////transform.position = new Vector3(messages[maxMessagesStored - 1].position.x, messages[maxMessagesStored - 1].position.y, transform.position.z);

        //float timeSinceNewest = GameManager.Instance.gameTime - messageNew.time;


        //Vector2 distanceBetweenMessages = messageNew.position - messageMid.position;
        //float timeBetweenMessages = messageNew.time - messageMid.time;

        //Vector2 speed = distanceBetweenMessages / timeBetweenMessages;

        //Vector2 newPosDisplacement = (speed * timeSinceNewest);

        //Debug.Log($"speed: {speed}, distance covered: {newPosDisplacement}");

        //Vector3 newPosDisplacement = PredictedPosFromMessagesPlusInterpolation();
        //newPosDisplacement.x += interpolationMoveDist.x;
        //newPosDisplacement.y += interpolationMoveDist.y;

        transform.position = PredictedPosFromMessagesPlusInterpolation();
        //transform.position = new Vector3(newPosDisplacement.x + messageNew.position.x/* + interpolateAddition.x*/, newPosDisplacement.y + messageNew.position.y/* + interpolateAddition.y*/, transform.position.z);
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


