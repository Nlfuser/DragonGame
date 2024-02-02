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
    [SerializeField] private int LavaLimit;
    private int _lavaTimer;
    private Vector2 _lastDir = Vector2.right;
    [SerializeField] private Transform grid;
    [SerializeField] private Transform others;
    [SerializeField] private Node nodePrefab;
    [SerializeField] private Exit exitPrefab;
    [SerializeField] private Lava lavaPrefab;
    [SerializeField] private int lavaDamage;

    private Exit exit;
    [SerializeField] private Player playerPrefab;
    private Player player;
    [SerializeField] private Enemy enemyPrefab;

    public List<Enemy> _enemies;
    public float enemyrange = 3f;
    // private List<Block> _blocks;

    private List<Node> _nodes;
    private List<Lava> _lavas;
    private GameState _state;
    // private int _round;
    // Start is called before the first frame update
    private Camera myCamera;
    public int xcamera;
    public int ycamera;
    void Start()
    {
        ChangeState(GameState.GenerateLevel);
        myCamera = Camera.main;
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
                break;
            case GameState.Moving:
                HeuristicCamera();
                break;
            case GameState.EnemiesMoving:
                MoveEnemies();
                break;
            case GameState.LavaMoving:
                _lavaTimer ++;
                if(_lavaTimer % LavaLimit == 0){
                    MoveLava();
                }
                if(GetLavaAtPosition(player.transform.position) != null){
                    if (player._health - lavaDamage <= 0)
                    {
                    GameOver();
                    }
                    player.Takedmg(lavaDamage);
                }
                ChangeState(GameState.WaitingInput);
                break;
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
        if (_state != GameState.WaitingInput) return;

        if (Input.GetKeyDown(KeyCode.LeftArrow)) MovePlayer(Vector2.left);
        if (Input.GetKeyDown(KeyCode.RightArrow)) MovePlayer(Vector2.right);
        if (Input.GetKeyDown(KeyCode.UpArrow)) MovePlayer(Vector2.up);
        if (Input.GetKeyDown(KeyCode.DownArrow)) MovePlayer(Vector2.down);

        _turnTimer += Time.deltaTime;
        if (_turnTimer >= TurnLimit)
        {
            _turnTimer = 0;
            ChangeState(GameState.EnemiesMoving);
        }
        
        // if(Input.GetKeyDown(KeyCode.Space)) 
    }

    private void HeuristicCamera()
    {
        Vector3 playertp = player.transform.position;
        Vector3 cameratp = myCamera.transform.position;
        if (playertp.x < cameratp.x - xcamera)
        {
            myCamera.transform.position = cameratp + Vector3.left + Vector3.back;
        }
        else if (playertp.x > cameratp.x + xcamera)
        {
            myCamera.transform.position = cameratp + Vector3.right + Vector3.back;
        } 
    }

    void GenerateGrid()
    {
        // _round = 0;
        // print("grid making");
        _nodes = new List<Node>();
        _enemies = new List<Enemy>();
        _lavas = new List<Lava>();
        // _blocks = new List<Block>();
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                var node = Instantiate(nodePrefab, new Vector2(x, y), Quaternion.identity, grid);
                _nodes.Add(node);
                // if(x==0) {
                //     var lava = Instantiate(lavaPrefab, new Vector2(x, y), Quaternion.identity, grid);
                //     _lavas.Add(lava);
                // }
            }
        }
        InitLavaRow(0);

        var center = new Vector2((float)_width / 2 - 0.5f, (float)_height / 2 - 0.5f);

        exit = Instantiate(exitPrefab, new Vector2(_width - 1, Random.Range(0, _height)), Quaternion.identity, others);

        InitEnemies();

        player = Instantiate(playerPrefab, new Vector2(1, _height % 2 == 0 ? _height / 2 : (float)_height / 2 - 0.5f), Quaternion.identity, others);


        // var board = Instantiate(_boardPrefab, center, Quaternion.identity);
        // board.size = new Vector2(_width,_height);

        Camera.main.transform.position = new Vector3(player.transform.position.x, player.transform.position.y, -10);
    }
    void InitEnemies()
    {
        var _enemy = Instantiate(enemyPrefab, new Vector2(_width - 2, Random.Range(0, _height)), Quaternion.identity, others);
        var _enemy1 = Instantiate(enemyPrefab, new Vector2(_width - 3, Random.Range(0, _height)), Quaternion.identity, others);
        _enemies.Add(_enemy);
        _enemies.Add(_enemy1);
    }

    void InitLavaRow(int x){
        print(x);
        for (int y = 0; y < _height; y++)
        {
            var lava = Instantiate(lavaPrefab, new Vector2(x, y), Quaternion.identity, grid);
            _lavas.Add(lava);       
        }
    }

    void MovePlayer(Vector2 dir)
    {
        Vector2 possibleLocation = (Vector2)player.transform.position + dir;
        var possibleNode = GetNodeAtPosition(possibleLocation);
        if (possibleNode != null)
        {//if grid exists
            _turnTimer = 0;
            ChangeState(GameState.Moving);
            var Enemy = GetEnemyAtPosition(possibleLocation);
            if (Enemy == null)
            {//if there are no enemies at the location
                player.transform.position = possibleLocation;
                ChangeState(GameState.EnemiesMoving);
            }
            else
            {
                print("enemies here");
                Fight(Enemy);
                Enemy = GetEnemyAtPosition(possibleLocation);
                if (Enemy == null)
                {//if there are no enemies at the location
                    player.transform.position = possibleLocation;
                }
                ChangeState(GameState.EnemiesMoving);
            }
            if (exit.transform.position == player.transform.position)
            {
                ExitLevel();
            }
        }
        else ChangeState(GameState.WaitingInput);
    }
    void MoveEnemies()
    {
        foreach (Enemy e in _enemies.ToList())
        {
            if (Vector2.Distance(player.transform.position, e.transform.position) < enemyrange)
            {
                Vector2 possibleLocation = (Vector2)e.transform.position - Simplepursuit(player.transform.position, e.transform.position);
                var possibleNode = GetNodeAtPosition(possibleLocation);
                if (possibleNode != null)
                {
                    if (possibleLocation == (Vector2)player.transform.position)
                    {
                        Fight(e);
                    }
                    else
                    {
                        e.transform.position = possibleLocation;
                    }
                }
            }
        }
        ChangeState(GameState.LavaMoving);
    }
    Vector2 Simplepursuit(Vector2 player, Vector2 currentEnemy)
    {
        Vector2 pursuitShort = currentEnemy - player;
        Vector2 xpursuit = new Vector2(pursuitShort.x, 0);
        Vector2 ypursuit = new Vector2(0, pursuitShort.y);
        Vector2 route = Vector2.zero;
        if (Math.Abs(pursuitShort.x) == Math.Abs(pursuitShort.y))
        {
            if (Random.Range(0, 2) == 0) route = xpursuit;
            else { route = ypursuit; }
        }
        else
        {
            route = Math.Abs(pursuitShort.x) > Math.Abs(pursuitShort.y) ? xpursuit : ypursuit;
        }
        return route.normalized;
    }
    void Fight(Enemy fightingEnemy)
    {
        if (player._health - fightingEnemy._attack <= 0)
        {
            GameOver();
        }
        player.Takedmg(fightingEnemy._attack);
        if (fightingEnemy._health - player._attack <= 0)
        {
            _enemies.Remove(fightingEnemy);
        }
        fightingEnemy.Takedmg(player._attack);
    }

    void MoveLava(){
        InitLavaRow(_lavaTimer / LavaLimit);
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
        _lavaTimer=0;

    }
    void GameOver()
    {
        print("Game Over");
        ExitLevel();
    }
    Node GetNodeAtPosition(Vector2 pos)
    {
        return _nodes.FirstOrDefault(n => n.Pos == pos);
    }

    Enemy GetEnemyAtPosition(Vector2 pos)
    {
        return _enemies.FirstOrDefault(n => n.Pos == pos);
    }

    Lava GetLavaAtPosition(Vector2 pos)
    {
        return _lavas.FirstOrDefault(n => n.Pos == pos);
    }
}
public enum GameState
{
    GenerateLevel,
    SpawningBlocks,
    WaitingInput,
    Moving,
    EnemiesMoving,
    LavaMoving,
    Win,
    Lose
}
