using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public struct minionDefaultMessage
    {
        public int clientId;
        public Vector2 position;
        public float time;
    }
    public struct towerDefaultMessage
    {
        public int clientId;
        public float zRotation;
        public float time;
    }

    List<GameObject> minions;
    List<GameObject> towers;
    private GameObject tileSet;
    public GameObject minionPrefab;
    public GameObject towerPrefab;

    GameObject player;
    Vector3 PathStart;

    public int minionScore;
    public int towerScore;
    public float offsetTime;

    public float sendUpdateTimer;

    private int pingTimeCount;
    private float pingTimeTimer;
    public bool updateTimerWithOffsetTime;
    public bool updateTimerInGameLoop;

    public float latestServerTime;

    public float gameTime { get; private set; }
    public static GameManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(this);
        }

    }

    // Start is called before the first frame update
    void Start()
    {
        pingTimeCount = 3;
        updateTimerWithOffsetTime = false;
        updateTimerInGameLoop = false;
        player = null;

        sendUpdateTimer = 0;

        minions = new List<GameObject>();
        towers = new List<GameObject>();
        tileSet = GameObject.Find("Tiles");

        PathStart = tileSet.GetComponent<CustomTileMap>().startTiles[0].transform.position;

        minionScore = 0;

        DataFromMenuToLevel instantiateLevelData = FindObjectOfType<DataFromMenuToLevel>();

        //offsetTime = NetworkManager.instance.calculatenetWorkAverageHalfTripTime();
        //gameTime = instantiateLevelData.serverGameTime + NetworkManager.instance.calculatenetWorkAverageHalfTripTime();
        gameTime = instantiateLevelData.serverGameTime; // replace this 



        for (int i = 0; i < instantiateLevelData.numPlayers; i++)
        {
            GameObject newObject = CreateNewObject(instantiateLevelData.ids[i], instantiateLevelData.types[i], instantiateLevelData.positions[i], instantiateLevelData.zRotations[i]);


        }

    }

    public void setGameTime(float newTime)
    {
        gameTime = newTime;
    }

    // Update is called once per frame
    void Update()
    {
        getBetterLatencyVal();
        sendUpdateTimer += Time.deltaTime;
        //send update every 50 ms
        if (sendUpdateTimer > Client.Instance.m_networkSendRate)
        {
            sendUpdateTimer = 0;
            if (player != null)
            {
                sendPlayerUpdate();
            }
        }




        gameTime += Time.deltaTime;


        // Debug.Log($"server time: {latestServerTime}, game time {gameTime}, offset {offsetTime}, difference in time: {gameTime - latestServerTime}");

        UIManager.instance.minionScoreText.text = minionScore.ToString();
        UIManager.instance.TowerScoreText.text = towerScore.ToString();
        UIManager.instance.gameTimeText.text = gameTime.ToString(".0#");
        if (Camera.main != null && player != null)
        {
            Camera.main.transform.position = new Vector3(player.transform.position.x, player.transform.position.y, Camera.main.transform.position.z);
        }
    }

    private void getBetterLatencyVal()
    {
        if (pingTimeCount > 0)
        {
            pingTimeTimer += Time.deltaTime;
            if (pingTimeTimer > 2)
            {
                pingTimeCount--;
                //NetworkManager.instance.pingServerForRoundTripTime(3);
                if (pingTimeCount == 0)
                {
                    // offsetTime = NetworkManager.instance.calculatenetWorkAverageHalfTripTime();
                    offsetTime = 0; //remove this
                    updateTimerWithOffsetTime = true;
                }
            }
        }
    }
    public void KillObject(int objectId)
    {
        GameObject killObject = returnObjectWithThisClientId(objectId);
        int objId = killObject.GetComponent<Controllable>().getId();
        if (player != null)
        {
            if (objId == player.GetComponent<Controllable>().getId())
            {
                player = null;

                UIManager.instance.respawnButton.gameObject.SetActive(true);
            }
        }
        int objType = killObject.GetComponent<Controllable>().getType();

        if (objType == 0)
        {
            for (int i = 0; i < towers.Count; i++)
            {
                if (towers[i].GetComponent<Controllable>().getId() == objId)
                {
                    killObject.GetComponent<Tower>().die();
                    towers.RemoveAt(i);
                    //kick from game if tower dies
                    //
                    //
                    break;
                }

            }
        }
        else if (objType == 1)
        {
            for (int i = 0; i < minions.Count; i++)
            {
                if (minions[i].GetComponent<Controllable>().getId() == objId)
                {
                    killObject.GetComponent<Minion>().die();
                    minions.RemoveAt(i);
                    break;
                }

            }
        }



    }

    public void OtherTowerShot(int towerId)
    {
        returnTowerWithThisClientId(towerId).GetComponent<Tower>().Shoot();
    }

    public GameObject CreateNewObject(int id, int type, Vector2 position, float zRot)
    {

        GameObject newObject;

        if (type == 0)
        {
            newObject = addTower();

        }
        else if (type == 1)
        {
            newObject = addMinion();

        }
        else
        {
            newObject = new GameObject();
        }

        newObject.GetComponent<Controllable>().setType(type);
        newObject.transform.position = new Vector3(position.x, position.y, newObject.transform.position.z);
        newObject.transform.rotation = Quaternion.Euler(0, 0, zRot);
        newObject.GetComponent<Controllable>().setId(id);

        //if the id of this player in the game is our id the this is us
        if (id == Client.Instance.m_myID)
        {
            setPlayer(newObject);
        }

        return newObject;

    }

    private void sendPlayerUpdate()
    {
        if (player.GetComponent<Controllable>().getType() == 0)
        {
            towerDefaultMessage message;
            message.time = gameTime;
            message.clientId = player.GetComponent<Controllable>().getId();
            message.zRotation = player.transform.rotation.eulerAngles.z;
            //ClientSend.SendTowerUpdate(message);
            ((TowerUpdatePacket)Client.Instance.FindPacket((int)Packet.PacketID.TowerUpdate)).SendPacket(message);
        }
        else
        {
            minionDefaultMessage message;
            message.time = gameTime;
            message.clientId = player.GetComponent<Controllable>().getId();
            message.position = new Vector2(player.transform.position.x, player.transform.position.y);
            // ClientSend.SendMinionUpdate(message);
            ((MinionUpdatePacket)Client.Instance.FindPacket((int)Packet.PacketID.MinionUpdate)).SendPacket(message);
        }
    }

    public void sendWorldUpdateToObjects(int setMinionScore, int setTowerScore, minionDefaultMessage[] minionMessages, towerDefaultMessage[] towerMessages)
    {

        minionScore = setMinionScore;
        towerScore = setTowerScore;
        for (int i = 0; i < minionMessages.Length; i++)
        {
            int id = minionMessages[i].clientId;
            GameObject selectedMinion = returnMinionWithThisClientId(id);
            if (selectedMinion == null)
            {
                continue;
            }

            selectedMinion.GetComponent<Minion>().AddMessage(minionMessages[i]);
        }
        for (int i = 0; i < towerMessages.Length; i++)
        {
            int id = towerMessages[i].clientId;
            GameObject selectedTower = returnTowerWithThisClientId(id);
            if (selectedTower == null)
            {
                continue;
            }
            selectedTower.GetComponent<Tower>().AddMessage(towerMessages[i]);
        }
    }

    private GameObject returnObjectWithThisClientId(int clientId)
    {
        GameObject returnObj = returnMinionWithThisClientId(clientId);
        if (returnObj == null)
        {
            returnObj = returnTowerWithThisClientId(clientId);
        }
        return returnObj;
    }

    private GameObject returnMinionWithThisClientId(int clientId)
    {
        foreach (GameObject minion in minions)
        {
            if (minion.GetComponent<Controllable>().getId() == clientId)
            {
                return minion;
            }
        }
        return null;
    }
    private GameObject returnTowerWithThisClientId(int clientId)
    {
        foreach (GameObject tower in towers)
        {
            if (tower.GetComponent<Controllable>().getId() == clientId)
            {
                return tower;
            }
        }
        return null;
    }

    //returns object type(0 for tower, 1 for minion) and their index in their respective array
    public KeyValuePair<int, int> findControllableObjectTypeAndIndex(GameObject controllableObj)
    {
        for (int i = 0; i < minions.Count; i++)
        {
            if (minions[i].gameObject == controllableObj)
            {
                return new KeyValuePair<int, int>(1, i);
            }
        }
        for (int i = 0; i < towers.Count; i++)
        {
            if (towers[i].gameObject == controllableObj)
            {
                return new KeyValuePair<int, int>(0, i);
            }
        }
        return new KeyValuePair<int, int>(-1, -1);

    }

    public void deleteControllable(int controllableType, int index)
    {
        if (controllableType == 0)
        {
            Destroy(towers[index].gameObject);
            towers.RemoveAt(index);
        }
        else if (controllableType == 1)
        {
            Destroy(minions[index].gameObject);
            minions.RemoveAt(index);
        }
    }
    private void setPlayer(GameObject newPlayer)
    {

        player = newPlayer;
        player.GetComponent<Controllable>().playerControlled = true;
    }
    public GameObject addMinion()
    {
        GameObject newMinion = Instantiate(minionPrefab);
        minions.Add(newMinion);
        return newMinion;
    }
    public GameObject addTower()
    {
        GameObject newTower = Instantiate(towerPrefab);
        towers.Add(newTower);
        return newTower;
    }

    public void RemoveDisconnectedPlayer(int disconnectedClientID)
    {
        GameObject disconnectingPlayer = returnObjectWithThisClientId(disconnectedClientID);
        KeyValuePair<int,int> typeAndIndex = findControllableObjectTypeAndIndex(disconnectingPlayer);
        deleteControllable(typeAndIndex.Key, typeAndIndex.Value);

    }
    public void spawnAsMinion()
    {
        ((SpawnMinionPacket)Client.Instance.FindPacket((int)Packet.PacketID.SpawnMinion)).SendPacket(true);
        //ClientSend.AttemptMinionCreation(true);
    }
}
