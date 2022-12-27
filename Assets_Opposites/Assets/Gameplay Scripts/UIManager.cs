using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private bool escMenuOpen;
    private bool selectingTower;
    public static UIManager instance;

    public GameObject tileSetPrefab;
    private GameObject tileSetInstance;

    public GameObject startMenu;
    public GameObject afterServerResponse;
    public GameObject escMenu;
    public InputField usernameField;
    public Button respawnButton;
    public Text serverMessageText;

    public Text TowerScoreText;
    public Text minionScoreText;
    public Text gameTimeText;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.Log("instance already exists, destroying object.");
            Destroy(this);
        }
        selectingTower = false;
        escMenuOpen = false;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            escMenuOpen = !escMenuOpen;
            escMenu.SetActive(escMenuOpen);
        }

        if (selectingTower)
        {
            if (Input.GetMouseButtonDown(0))
            {
                //send mouse pos and tower type to server to try and spawn
              ((SpawnTowerPacket)Client.Instance.FindPacket((int)Packet.PacketID.SpawnTower)).SendPacket(Input.mousePosition, false);
            }
        }
    }

    public void ConnectToServer()
    {

        startMenu.SetActive(false);

        usernameField.interactable = false;

        ((ConnectPacket)Client.Instance.FindPacket((int)Packet.PacketID.Connect)).SendPacket();
        
    }

    public void afterResp()
    {

        serverMessageText.text = "select which side you want to play";
        afterServerResponse.SetActive(true);

    }

    public void PingClosedStatus()
    {

        ((DisconnectPacket)Client.Instance.FindPacket((int)Packet.PacketID.Disconnect)).SendPacket();
    }

    public void CloseStartMenu()
    {
        startMenu.SetActive(false);
    }

    public void deactivateRespawnButton()
    {
        respawnButton.gameObject.SetActive(false);
    }
    public void towerSelected()
    {


        afterServerResponse.SetActive(false);
        tileSetInstance = Instantiate(tileSetPrefab);
        selectingTower = true;
    }
    public void minionSelected()
    {

        ((SpawnMinionPacket)Client.Instance.FindPacket((int)Packet.PacketID.SpawnMinion)).SendPacket(false);
        afterServerResponse.SetActive(false);
    }
}
