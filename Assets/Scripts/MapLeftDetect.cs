using SKCell;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapLeftDetect : MonoBehaviour
{
    //The array that keeps records of how many blocks are inside the map
    GameObject[] blocks;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CheckBlocksLeft()
    {
        blocks = GameObject.FindGameObjectsWithTag("Block");
        foreach (GameObject block in blocks)
        {
            Block _Block = block.GetComponent<Block>();
            if (_Block.isLeft == true)
            {
                if(_Block.isRed == false)
                {

                }
            }

        }
    }

}
