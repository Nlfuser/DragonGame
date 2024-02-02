using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    [SerializeField] private int _height;
    [SerializeField] private int _width;
    [SerializeField] private float TurnLimit;
    private float _turnTimer;
    [SerializeField] private Transform grid;
    [SerializeField] private Node nodePrefab;
    [SerializeField] private Exit exitPrefab;

    private Exit exit;
    [SerializeField] private Player playerPrefab;
    private Player player;

    private List<Node> _nodes;
    // private List<Block> _blocks;
    private GameState _state;
    // private int _round;
    // Start is called before the first frame update

    void Start()
    {
        var center = new Vector2((float)_width / 2 - 0.5f, (float)_height / 2 - 0.5f);
        Camera.main.transform.position = new Vector3(center.x, center.y, -10);
        ChangeState(GameState.GenerateLevel);
    }

    private void ChangeState(GameState newState)
    {
        _state = newState;

        switch (newState)
        {
            case GameState.GenerateLevel:
                GenerateGrid();
                ChangeState(GameState.SpawningBlocks);
                break;
            case GameState.SpawningBlocks:
                // SpawnBlocks(_round++ == 0 ? 2 : 1);
                ChangeState(GameState.WaitingInput);
                break;
            case GameState.WaitingInput:
                //Begin clock
                break;
            //case GameState.Moving:
            //    break;
            // case GameState.Win:
            //     _winScreen.SetActive(true);
            //     Invoke(nameof(DelayedWinScreenText),1.5f);
            //     break;
            // case GameState.Lose:
            //     _loseScreen.SetActive(true);
            // break;
            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
        }
    }
    void Update()
    {
        if (_state == GameState.WaitingInput)
        {
            Vector2 direction = Vector2.zero;
            if (Input.GetKeyDown(KeyCode.LeftArrow)) direction = Vector2.left;
            if (Input.GetKeyDown(KeyCode.RightArrow)) direction = Vector2.right;
            if (Input.GetKeyDown(KeyCode.UpArrow)) direction = Vector2.up;
            if (Input.GetKeyDown(KeyCode.DownArrow)) direction = Vector2.down;
            if (direction != Vector2.zero)
            {                
                Shift(direction);
                Debug.Log("EnemyBehaviour after move");
                _turnTimer = 0;
            }
            _turnTimer += Time.deltaTime;
            if (_turnTimer >= TurnLimit)
            {
                _turnTimer = 0;
                Debug.Log("EnemyBehaviour after inaction"); //EnemyBehaviour
            }
        }
        // if(Input.GetKeyDown(KeyCode.Space)) ExitLevel();        
        if (exit.transform.position == player.transform.position)
        {
            ExitLevel();
        }
        
    }
    void TickTack()
    {

    }
    void GenerateGrid()
    {
        // _round = 0;
        print("grid making");
        _nodes = new List<Node>();
        // _blocks = new List<Block>();
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                var node = Instantiate(nodePrefab, new Vector2(x, y), Quaternion.identity, grid);
                _nodes.Add(node);
            }
        }
        exit = Instantiate(exitPrefab, new Vector2(_width - 1, Random.Range(0, _height)), Quaternion.identity, grid);
        player = Instantiate(playerPrefab, new Vector2(0, _height % 2 == 0 ? _height / 2 : (float)_height / 2 - 0.5f), Quaternion.identity);
        // var board = Instantiate(_boardPrefab, center, Quaternion.identity);
        // board.size = new Vector2(_width,_height);
    }

    void Shift(Vector2 dir)
    {
        Vector2 possibleLocation = (Vector2)player.transform.position + dir;
        var possibleNode = GetNodeAtPosition(possibleLocation);
        if (possibleNode != null)
        {
            player.transform.position = possibleLocation;
        }
    }

    void ExitLevel()
    {
        foreach (Transform child in grid)
        {
            Destroy(child.gameObject);
        }
        Destroy(player.gameObject);
        Destroy(exit.gameObject);
        ChangeState(GameState.GenerateLevel);

    }

    Node GetNodeAtPosition(Vector2 pos)
    {
        return _nodes.FirstOrDefault(n => n.Pos == pos);
    }
}

public enum GameState
{
    GenerateLevel,
    SpawningBlocks,
    WaitingInput,
    Moving,
    Win,
    Lose
}
