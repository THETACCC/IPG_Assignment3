using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{

    //The array that keeps records of how many blocks are inside the map
    GameObject[] blocks;


    public GameObject WinUI;

    public GameState state;
    // Start is called before the first frame update
    void Start()
    {
        state = GameState.Start;
    }

    // Update is called once per frame
    void Update()
    {
        switch (state)
        {
            default:
            case GameState.Start:
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    state = GameState.Prepare;
                }

                break;
            case GameState.Prepare:
                LevelLoader.instance.isWin = false;
                LevelLoader.instance.SpawnBlocks();
                state = GameState.Aligning;



                break;

            case GameState.Aligning:

                if (LevelLoader.instance.isAligned == true)
                {
                    state = GameState.Placing;

                }



                break;


            case GameState.Placing:
                if (LevelLoader.instance.isWin)
                {
                    state = GameState.Win;
                }


                break;

            case GameState.Win:


                //Starting Aligning Blocks
                blocks = GameObject.FindGameObjectsWithTag("Block");
                foreach (GameObject block in blocks)
                {
                    Destroy(block);
                }
                LevelLoader.instance.clearMap();
                LevelLoader.instance.isAligned = false;
                state = GameState.Restart;

                break;

            case GameState.Restart:

                    WinUI.SetActive(true);
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    WinUI.SetActive(false);
                    state = GameState.Prepare;
                }



                break;
        }


    }
   


      
    
}

public enum GameState
{
    Start,
    Placing,
    Win,
    Prepare,
    Restart,
    Aligning
}
