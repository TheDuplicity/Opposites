using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Controllable : MonoBehaviour
{
    public bool playerControlled;

    private int id;
    private int type;

    // Start is called before the first frame update
    void Start()
    {
        playerControlled = false;

    }

    private void Update()
    {

    }

    public void setId(int setId)
    {
        id = setId;
    }

    public int getId()
    {
        return id;
    }

    public void setType(int setType)
    {
        type = setType;
    }

    public int getType()
    {
        return type;
    }


    virtual public void handlePlayerControls() { }
}
