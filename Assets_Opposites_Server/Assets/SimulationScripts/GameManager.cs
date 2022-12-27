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
    //list of players that want to join and their tower types
    Dictionary<int, int> clientsWaitingToJoinAsType;

    List<GameObject> minions;
    List<GameObject> towers;
    public GameObject tileSet;
     public GameObject minionPrefab;
     public GameObject towerPrefab;

    public float gameTime { get; private set; }

    public bool gameStarted;

    public List<int> spectatorIDs { get; private set; }

    Vector3 PathStart;

    public int minionScore;
    public int towerScore;

    private float sendPlayerUpdatesTimer;

    public static GameManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null) 
        { 
            Instance = this;
        } else if (Instance != this)
        {
            Destroy(this);
        }

    }

    // Start is called before the first frame update
    void Start()
    {

        gameStarted = false;
        sendPlayerUpdatesTimer = 0;
        gameTime = 0;
        minions = new List<GameObject>();
        towers = new List<GameObject>();

        PathStart = tileSet.GetComponent<CustomTileMap>().startTiles[0].transform.position;

        minionScore = 0;
        towerScore = 0;

        spectatorIDs = new List<int>();

    }

    // Update is called once per frame
    void Update()
    {
        if (!gameStarted)
        {
            //if we have 2 players
            if (minions.Count + towers.Count >= 1) {
                startGame();
            }
            else
            {
                return;
            }
        }
        gameTime += Time.deltaTime;
        sendPlayerUpdatesTimer += Time.deltaTime;
        if (sendPlayerUpdatesTimer > Server.Instance.m_serverSendRate)
        {
            sendPlayerUpdatesTimer = 0;
            sendDefaultUpdatesToEveryone();
        }


    }

    public void PlayerDisconnectedBroadcast(int disconnectClientID)
    {
        int[] inGamePlayerIDs = new int[minions.Count + towers.Count];
        int pos = 0;
        foreach (GameObject minion in minions)
        {
            inGamePlayerIDs[pos] = minion.GetComponent<Controllable>().getId();
            pos++;
        }
        foreach (GameObject tower in towers)
        {
            inGamePlayerIDs[pos] = tower.GetComponent<Controllable>().getId();
            pos++;
        }
        //broadcast 

    }
    //if the player dies, 
    public void PlayerDiedBroadcast(int deadPlayerId)
    {
        //first send message to everyone saying the player died
        ((PlayerDiedPacket)Server.Instance.FindPacket((int)Packet.PacketID.PlayerDied)).SendPacket(GetInGamePlayerAndSpectatorIDs(),deadPlayerId);

        //then kill the player in the server
        for (int i = 0; i < minions.Count; i++)
        {
            if (minions[i].GetComponent<Controllable>().getId() == deadPlayerId)
            {
                Destroy(minions[i]);
                minions.RemoveAt(i);
                return;
            }
            
        }

            for (int i = 0; i < towers.Count; i++)
            {
                if (towers[i].GetComponent<Controllable>().getId() == deadPlayerId)
                {
                    Destroy(towers[i]);
                    towers.RemoveAt(i);
                return;
                }

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

    public void shootBulletFromTower(int towerId)
    {
        returnTowerWithThisClientId(towerId).GetComponent<Tower>().Shoot();
    }


    public void UpdateMinion(int clientId, minionDefaultMessage message)
    {
        GameObject minion = returnMinionWithThisClientId(clientId);
        minion.GetComponent<Minion>().AddMessage(message);
    }

    public void updateTower(int clientId, towerDefaultMessage message)
    {
        GameObject tower = returnTowerWithThisClientId(clientId);
        tower.GetComponent<Tower>().AddMessage(message);
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

    public void sendDefaultUpdatesToEveryone()
    {
        minionDefaultMessage[] minionMessages = fillAllMinionMessages();
        towerDefaultMessage[] towerMessages = fillAllTowerMessages();

        
       ((WorldUpdatePacket)Server.Instance.FindPacket((int)Packet.PacketID.WorldUpdate)).SendPacket(GetInGamePlayerAndSpectatorIDs(), gameTime, minionScore, towerScore, minionMessages, towerMessages);


    }




    private towerDefaultMessage[] fillAllTowerMessages()
    {
        int numTowers = towers.Count;
        towerDefaultMessage[] messages = new towerDefaultMessage[numTowers];
        for (int i = 0; i < numTowers; i++)
        {
            GameObject tower = towers[i];
            messages[i].zRotation = tower.transform.rotation.eulerAngles.z;
            messages[i].clientId = tower.GetComponent<Controllable>().getId();

        }
        return messages;
    }

    private minionDefaultMessage[] fillAllMinionMessages()
    {
        int numMinions = minions.Count;
        minionDefaultMessage[] messages = new minionDefaultMessage[numMinions];
        for (int i = 0; i < numMinions; i++)
        {
            GameObject minion = minions[i];
            messages[i].position = new Vector2(minion.transform.position.x, minion.transform.position.y);
            messages[i].clientId = minion.GetComponent<Controllable>().getId();

        }
        return messages;
    }

    public void tellOtherPlayersIExist(int clientId)
    {
        GameObject newPlayerObject = returnMinionWithThisClientId(clientId);
        if (newPlayerObject == null)
        {
            newPlayerObject = returnTowerWithThisClientId(clientId);
        }

        Vector2 pos = new Vector2(newPlayerObject.transform.position.x, newPlayerObject.transform.position.y);
        int newPlayerType = newPlayerObject.GetComponent<Controllable>().type;
        float newPlayerZRot = newPlayerObject.transform.rotation.eulerAngles.z;
        //would be better to send to all players that can see the game even if they havent got a minion or tower in the game
        int[] otherPlayerIds = new int[minions.Count + towers.Count];
        
        for (int i = 0; i < minions.Count; i++)
        {
            otherPlayerIds[i] += minions[i].GetComponent<Controllable>().getId();


            //send a package to the oother players giving them this new object
            //ServerSend.SendNewConnectedPlayerInit(otherPlayerId, pos, clientId, newPlayerType, newPlayerZRot);
        }
        for (int i = 0; i < towers.Count; i++)
        {
            int playerIDPos = i + minions.Count;
            otherPlayerIds[playerIDPos] = towers[i].GetComponent<Controllable>().getId();

            //ServerSend.SendNewConnectedPlayerInit(otherPlayerId, pos, clientId, newPlayerType, newPlayerZRot);
        }
        //telling other clients we have connected to the server should be separated from telling them information about our spawn but oh well
        ((NewPlayerSpawnPacket)Server.Instance.FindPacket((int)Packet.PacketID.NewPlayerSpawnPacket)).SendPacket(pos, clientId, newPlayerType, newPlayerZRot, otherPlayerIds);
    }

    public void sendWelcomePackage(int sendToId)
    {
        if (gameStarted)
        {
            tellOtherPlayersIExist(sendToId);
        }
        int numPlayers= minions.Count + towers.Count;
        Vector2[] positions = new Vector2[numPlayers];
        int[] ids = new int[numPlayers];
        int[] types = new int[numPlayers];
        float[] zRotations = new float[numPlayers];
        //num players in total but we skip

        for (int i = 0; i < minions.Count; i++)
        {
            
            positions[i] = new Vector2(minions[i].transform.position.x, minions[i].transform.position.y);
            ids[i] = minions[i].GetComponent<Controllable>().getId();
            types[i] = 1;
            zRotations[i] = 0;
            
        }
        for (int i = 0; i < towers.Count; i++)
        {
            int fillValArrayPos = i + minions.Count;
            positions[fillValArrayPos] = new Vector2(towers[i].transform.position.x, towers[i].transform.position.y);
            ids[fillValArrayPos] = towers[i].GetComponent<Controllable>().getId();
            types[fillValArrayPos] = 0;
            zRotations[fillValArrayPos] = 0;
        }

        ((JoinGameDataPacket)Server.Instance.FindPacket((int)Packet.PacketID.JoinGameDataPacket)).SendPacket(sendToId, gameTime, numPlayers, positions, ids, types, zRotations);
    }

    private void startGame()
    {
        for (int i = 0; i < minions.Count; i++)
        {
            sendWelcomePackage(minions[i].GetComponent<Controllable>().getId());
        }
        for (int i = 0; i < towers.Count; i++)
        {
            sendWelcomePackage(towers[i].GetComponent<Controllable>().getId());
        }
        gameStarted = true;
    }

    public bool addMinion(int clientId)
    {
        if (clientAlreadyHasObject(clientId))
        {
            return false;
        }
        int arrPos;
        Server.Instance.FindClient(clientId, out arrPos).m_inGame = true;

        for (int i = 0; i < spectatorIDs.Count; i++)
        {
            if (clientId == spectatorIDs[i])
            {
                spectatorIDs.RemoveAt(i);
                break;
            }
        }

        GameObject newMinion = Instantiate(minionPrefab);
        newMinion.transform.position = PathStart;
        newMinion.GetComponent<Controllable>().setId(clientId);
        newMinion.GetComponent<Controllable>().type = 1;
        minions.Add(newMinion);
        return true;
    }
    public bool addTower(int clientId, Vector3 spawnPos)
    {
        if (clientAlreadyHasObject(clientId))
        {
            return false;
        }
        int arrPos;
        Server.Instance.FindClient(clientId, out arrPos).m_inGame = true; 
        Debug.Log($"spawning tower for client {clientId} at position {spawnPos}");

        for (int i = 0; i < spectatorIDs.Count; i++)
        {
            if (clientId == spectatorIDs[i])
            {
                spectatorIDs.RemoveAt(i);
                break;
            }
        }

        GameObject newTower = Instantiate(towerPrefab);
        newTower.GetComponent<Controllable>().setId(clientId);
        newTower.transform.position = spawnPos;
        newTower.GetComponent<Controllable>().type = 0;
        towers.Add(newTower);
        return true;
    }

    private bool clientAlreadyHasObject(int clientId)
    {

        if (minions.Count > 0)
        {
            foreach (GameObject minion in minions)
            {
                if (minion.GetComponent<Controllable>().getId() == clientId)
                {
                    return true;
                }
            }
        }
        if (towers.Count > 0)
        {
            foreach (GameObject tower in towers)
            {
                if (tower.GetComponent<Controllable>().getId() == clientId)
                {
                    return true;
                }
            }
        }
        return false;
    }

    public bool trySpawnClientAsTower(int clientId, Vector3 mouseCheckPos)
    {

        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseCheckPos);
        RaycastHit2D hit;
        hit = Physics2D.Raycast(mouseWorldPos, new Vector3(0, 0, 1), 100);
        if (hit)
        {
            GameObject hitObj = hit.transform.gameObject;
            if (hitObj.tag == "TowerTile")
            {
                if (!isTowerTileTaken(hitObj.transform.position))
                {
                    if(addTower(clientId, hitObj.transform.position))
                    {
                        return true;
                    }
                }
                else
                {
                    Debug.Log($"couldnt spawn client {clientId}, tile occupied");
                }
                Debug.Log($"couldnt spawn client {clientId}, not on a valid tile");


            }

        }
        return false;
    }
    private bool isTowerTileTaken(Vector3 pos)
    {
        if (towers.Count <= 0)
        {
            return false;
        }
        for (int i = 0; i < towers.Count; i++)
        {
            if (towers[i].gameObject.transform.position == pos)
            {
                return true;
            }
        }
        return false;
    }

    public int[] GetInGamePlayerIDs()
    {
        int[] outIDs = new int[towers.Count + minions.Count];
        int pos = 0;
        foreach (GameObject tower in towers)
        {
            outIDs[pos] = tower.GetComponent<Controllable>().getId();
            pos++;
        }

        foreach (GameObject minion in minions)
        {
            outIDs[pos] = minion.GetComponent<Controllable>().getId();
            pos++;
        }
        return outIDs;
    }

    public int[] GetInGamePlayerAndSpectatorIDs()
    {
        List<int> playerAndSpectatorIDs = new List<int>();
        playerAndSpectatorIDs.AddRange(GetInGamePlayerIDs());
        playerAndSpectatorIDs.AddRange(spectatorIDs);
        return playerAndSpectatorIDs.ToArray();
    }

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

    public void removeControllableFromGame(int controllableType, int controllableID)
    {
        
        switch (controllableType)
        {
            case 0:

                for (int i = 0; i < towers.Count; i++)
                {
                    if (towers[i].GetComponent<Controllable>().getId() == controllableID)
                    {
                        towers.RemoveAt(i);
                    }
                }
                
                break;

            case 1:

                for (int i = 0; i < minions.Count; i++)
                {
                    if (minions[i].GetComponent<Controllable>().getId() == controllableID)
                    {
                        minions.RemoveAt(i);
                    }
                }

                break;

            default:

                break;
        }
        // add this controllable to the spectators if they died and arent spectating

        if (!IsInSpectatorList(controllableID))
        {
            spectatorIDs.Add(controllableID);
        }

    }

    public bool IsInSpectatorList(int clientID)
    {
        foreach (int ID in spectatorIDs)
        {
            if (ID == clientID)
            {
                return true;
            }
        }
        return false;
    }

    public void RemoveDisconnectedPlayer(int disconnectedClientID)
    {
        GameObject disconnectingPlayer = returnObjectWithThisClientId(disconnectedClientID);
        KeyValuePair<int, int> typeAndIndex = findControllableObjectTypeAndIndex(disconnectingPlayer);
        deleteControllable(typeAndIndex.Key, typeAndIndex.Value);

    }

}
