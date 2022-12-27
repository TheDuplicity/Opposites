using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataFromMenuToLevel : MonoBehaviour
{
    public int numPlayers;
    public Vector2[] positions;
    public int[] ids;
    public int[] types;
    public float[] zRotations;
    public float serverGameTime;

    public static DataFromMenuToLevel instance;
    // Start is called before the first frame update
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
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    public void instantiateArrays(int size)
    {
        numPlayers = size;
        positions = new Vector2[size];
        ids = new int[size];
        types = new int[size];
        zRotations = new float[size];
    }

}
