using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager _Instance;
    [SerializeField] private int _height;
    [SerializeField] private int _width;
    [SerializeField] private float turnLimit;
    private float _turnTimer;
    [SerializeField] private int LavaLimit;
    private int _lavaTimer;
    [SerializeField] private int GoldLimit;
    private int _goldTimer;
    private Vector2 _lastDir = Vector2.right;
    [SerializeField] private Transform grid;
    [SerializeField] private Transform others;
    [SerializeField] private Node nodePrefab;
    [SerializeField] private Exit exitPrefab;
    [SerializeField] private Lava lavaPrefab;
    [SerializeField] public GameObject ArrowPrefab;
    [SerializeField] private int lavaDamage;
    [SerializeField] private GoldBag GoldBagPrefab;

    private Exit exit;
    [SerializeField] private Player playerPrefab;
    public Player player;
    [SerializeField] private Enemy enemyMeleePrefab;
    [SerializeField] private Enemy enemyRangedPrefab;

    public List<Enemy> _enemies;
    public Vector2[] cardinals;
    public float enemyrange = 3f;
    // private List<Block> _blocks;

    private List<Node> _nodes;
    
    private List<Lava> _lavas;
    public List<GoldBag> _goldBags;
    
    private GameState _state;

    public TMP_Text AttackText;
    public TMP_Text TurnText;
    public TMP_Text TurnTimerText;
    public TMP_Text HealthText;
    public TMP_Text CoinText;



    private Camera myCamera;
    public int xcamera;
    public int ycamera;
    private void Awake()
    {
        if (_Instance != null) Destroy(gameObject);
        else
        {
            _Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
    void Start()
    {
        ChangeState(GameState.GenerateLevel);
        myCamera = Camera.main;
        cardinals = new Vector2[4];
        cardinals[0] = Vector2.right;
        cardinals[1] = Vector2.up;
        cardinals[2] = Vector2.left;
        cardinals[3] = Vector2.down;


        TurnText.SetText("Your turn");
        CoinText.SetText("0");

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
                TurnText.SetText("Your turn");
                TurnTimerText.SetText(string.Format("{0:N2}", turnLimit));
                break;
            case GameState.Moving:
                // HeuristicCamera();
                break;
            case GameState.EnemiesMoving:
                TurnText.SetText("Enemies turn");
                MoveEnemies();
                break;
            case GameState.LavaMoving:
                _lavaTimer++;
                if (_lavaTimer % LavaLimit == 0)
                {
                    MoveLava();
                }
                if (GetLavaAtPosition(player.transform.position) != null)
                {
                    if (player._health - lavaDamage <= 0)
                    {
                        GameOver();
                    }
                    player.Takedmg(lavaDamage);
                }
                foreach (Enemy e in _enemies.ToList())
                {
                    if (GetLavaAtPosition(e.transform.position) != null)
                    {
                        if (e.Takedmg(lavaDamage)) _enemies.Remove(e);
                    }
                }
                ChangeState(GameState.GoldManagement);
                break;
            case GameState.GoldManagement:
                PickupGoldCheck();
                 _goldTimer++;
                if (_goldTimer % GoldLimit == 0)
                {
                    SpawnGold();
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
        TurnTimerText.SetText(string.Format("{0:N2}", _turnTimer));
        if (_turnTimer >= turnLimit)
        {
            _turnTimer = 0;
            ChangeState(GameState.EnemiesMoving);
        }
        try { enemybehaviourtest(); } catch { }
        // if(Input.GetKeyDown(KeyCode.Space)) 
    }
    void enemybehaviourtest()
    {
        foreach (Enemy e in _enemies.ToList())
        {
            Vector2 possibleLocation = (Vector2)e.transform.position - Simplepursuit(player.transform.position, e.transform.position);
            Debug.DrawLine((Vector2)e.transform.position, possibleLocation);
        }
    }
    private void HeuristicCamera() //Deprecated
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
        _goldBags = new List<GoldBag>();

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
        AttackText.SetText(string.Format("{0:N2}", player._attack));
        HealthText.SetText(string.Format("{0:N2}", player._health));

        // var board = Instantiate(_boardPrefab, center, Quaternion.identity);
        // board.size = new Vector2(_width,_height);

        // Camera.main.transform.position = new Vector3(player.transform.position.x, player.transform.position.y, -10);
        Camera.main.transform.position = new Vector3(center.x,center.y,-10);

    }
    void InitEnemies()
    {
        var _enemy = Instantiate(enemyMeleePrefab, new Vector2(_width - 2, Random.Range(0, _height)), Quaternion.identity, others);
        var _enemy1 = Instantiate(enemyMeleePrefab, new Vector2(_width - 3, Random.Range(0, _height)), Quaternion.identity, others);
        var _enemy2 = Instantiate(enemyRangedPrefab, new Vector2(_width - 4, Random.Range(0, _height)), Quaternion.identity, others);
        _enemies.Add(_enemy);
        _enemies.Add(_enemy1);
        _enemies.Add(_enemy2);
    }
    void InitLavaRow(int x)
    {
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
            e.Behave(player); //Mandatory coupling from GameManager singleton instance; 
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
    public void Fight(Enemy fightingEnemy)
    {
        PlayerHurt(fightingEnemy._attack);

        if (fightingEnemy._health - player._attack <= 0)
        {
            _enemies.Remove(fightingEnemy);
        }
        if (fightingEnemy.coinCount > 0)
        {
            var _gold = Instantiate(GoldBagPrefab, fightingEnemy.Pos, Quaternion.identity, others);
            _gold.value = fightingEnemy.coinCount;
            _goldBags.Add(_gold);
        }        
        fightingEnemy.Takedmg(player._attack);
    }
    public void PlayerHurt(int dmg)
    {
        if (player._health - dmg <= 0)
        {
            GameOver();
        }
        player.Takedmg(dmg);
        HealthText.SetText(string.Format("{0:N2}", player._health));
    }
    void MoveLava()
    {
        InitLavaRow(_lavaTimer / LavaLimit);
    }

    void PickupGoldCheck(){
        foreach (GoldBag g in _goldBags.ToList())
        {
            if((Vector2)player.transform.position == g.Pos){// if player at gold position, remove gold and give gold to player
                player.GainGold(g.value);
                CoinText.SetText(string.Format("{0:N2}", player.coinCount));
                _goldBags.Remove(g);
                Destroy(g.gameObject);
                print("Picked up gold");
            }

            var e = GetEnemyAtPosition(g.Pos);
            if(e != null){// if enemy at gold position, remove gold and give gold to enemy
                e.GainGold(g.value);
                _goldBags.Remove(g);
                Destroy(g.gameObject);
                print("Picked up gold");
            }
        }
    }

    void SpawnGold(){

        var possibleLocations = new List<Vector2>();
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                var location = new Vector2(x, y);
                if (//continue if any of condition x is true, continue if there is an object at the position. if getXatLoc() != null
                    (GetEnemyAtPosition(location) == null) ||
                    (GetLavaAtPosition(location) == null) ||
                    (GetGoldAtPosition(location) == null) ||
                    location != (Vector2)player.transform.position    ||
                    location != (Vector2)exit.transform.position
                    ){
                       possibleLocations.Add(location); 
                    }
            }
        }
        if(possibleLocations.Count != 0){
            var _gold = Instantiate(GoldBagPrefab, possibleLocations[Random.Range(0, possibleLocations.Count)], Quaternion.identity, others);
            _goldBags.Add(_gold);
        }
        else print("No spots");
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
        _lavaTimer = 0;
        _goldTimer = 0;

    }
    void GameOver()
    {
        print("Game Over");
        ExitLevel();
    }
    public Node GetNodeAtPosition(Vector2 pos)
    {
        return _nodes.FirstOrDefault(n => n.Pos == pos);
    }
    public Enemy GetEnemyAtPosition(Vector2 pos)
    {
        return _enemies.FirstOrDefault(n => n.Pos == pos);
    }
    public Lava GetLavaAtPosition(Vector2 pos)
    {
        return _lavas.FirstOrDefault(n => n.Pos == pos);
    }
        
    public GoldBag GetGoldAtPosition(Vector2 pos)
    {
        return _goldBags.FirstOrDefault(n => n.Pos == pos);
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
    GoldManagement,
    Win,
    Lose
}
