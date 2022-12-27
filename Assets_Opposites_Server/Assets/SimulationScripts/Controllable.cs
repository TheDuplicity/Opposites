using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controllable : MonoBehaviour
{
    public bool readyToGetWelcomePackage;
    private float maxMessages;
    private int id;
    public int type;


    // Start is called before the first frame update
    void Start()
    {


    }

    private void Update()
    {
        /*
        if (readyToGetWelcomePackage)
        {
            if (GameManager.Instance.gameStarted)
            {
                GameManager.Instance.sendWelcomePackage(id);
            }

        }
        */

    }

    public void setId(int setId)
    {
        id = setId;
    }

    public int getId()
    {
        return id;
    }




}
