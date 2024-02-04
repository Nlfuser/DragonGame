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
    public List<Level> gameLevels;
    public int currentLevelIndex;
    [SerializeField] private int _height;
    [SerializeField] private int _width;
    [SerializeField] private int GridOffset;
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
    [SerializeField] private Lava lavaPoolPrefab;
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
    //public float enemyrange = 3f;
    int enemylev = 0;
    // private List<Block> _blocks;

    public List<Node> _nodes;

    private List<Lava> _lavas;
    private List<Lava> _lavaspool;
    public List<GoldBag> _goldBags;

    private GameState _state;

    public TMP_Text AttackText;
    public TMP_Text TurnText;
    public TMP_Text TurnTimerText;
    public TMP_Text HealthText;
    public TMP_Text CoinText;
    [SerializeField] private GameObject GameOverMenuUI;


    public int lavaholefloor;
    public int rockceiling;

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
        ChangeState(GameState.Stop);
    }

    public void InitGameManager()
    {
        myCamera = Camera.main;
        cardinals = new Vector2[4];
        cardinals[0] = Vector2.right;
        cardinals[1] = Vector2.up;
        cardinals[2] = Vector2.left;
        cardinals[3] = Vector2.down;
        currentLevelIndex = 0;
        _nodes = new List<Node>();
        _enemies = new List<Enemy>();
        _lavas = new List<Lava>();
        _goldBags = new List<GoldBag>();
        _lavaspool = new List<Lava>();
        TurnText.SetText("Your turn");
        CoinText.SetText("0");
        ChangeState(GameState.GenerateLevel);
    }

    private void ChangeState(GameState newState)
    {
        _state = newState;

        switch (newState)
        {
            case GameState.GenerateLevel:
                print("generating level");
                GenerateGrid();
                AttackText.SetText(string.Format("{0}", player._attack));
                HealthText.SetText(string.Format("{0}", player._health));
                ChangeState(GameState.WaitingInput);
                break;
            case GameState.SpawningBlocks:
                // SpawnBlocks(_round++ == 0 ? 2 : 1);
                ChangeState(GameState.WaitingInput);
                break;
            case GameState.WaitingInput:
                print("waiting input");
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
                    HealthText.SetText(string.Format("{0}", player._health));

                }
                foreach (Enemy e in _enemies.ToList())
                {
                    if (GetLavaAtPosition(e.transform.position) != null)
                    {
                        if (e.Takedmg(lavaDamage)) _enemies.RemoveAll(i => i.Equals(e));
                    }
                }
                foreach (GoldBag G in _goldBags.ToList())
                {
                    if (GetLavaAtPosition(G.transform.position) != null)
                    {
                        Destroy(G.gameObject);
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
            case GameState.Lose:
                GameOverMenuUI.SetActive(true);
                //_loseScreen.SetActive(true);
                break;
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
        //try { enemybehaviourtest(); } catch { }
        // if(Input.GetKeyDown(KeyCode.Space)) 
    }
    void enemybehaviourtest() //Deprecated
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
        Level currentLevel = gameLevels.ElementAt(currentLevelIndex);
        ++currentLevelIndex;
        _nodes = new List<Node>();
        _enemies = new List<Enemy>();
        _lavas = new List<Lava>();
        _goldBags = new List<GoldBag>();
        _lavaspool = new List<Lava>();
        _width = currentLevel.xtiles;
        _height = currentLevel.ytiles;
        List<Vector2> pathNodes = new List<Vector2>();
        GeneratePath(ref pathNodes);
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                if (pathNodes.Exists(v => v.x == x && v.y == y))
                {
                    RockTile(x, y);
                }
                else
                {
                    int randtile = Random.Range(0, 100);
                    if (randtile > rockceiling)
                    {
                        RockTile(x, y);
                    }
                    else if (randtile < lavaholefloor)
                    {
                        LavaPoolTile(x, y);
                    }
                }
            }
        }
        InitLavaRow(0);
        var center = new Vector2((float)_width / 2 - 0.5f, (float)_height / 2 - 0.5f);
        exit = Instantiate(exitPrefab, pathNodes.Last() + new Vector2(0, GridOffset), Quaternion.identity, others);
        player = Instantiate(playerPrefab, pathNodes.ElementAt(1) + new Vector2(0, GridOffset), Quaternion.identity, others);
        InitEnemies(currentLevel.enemiesMelee, currentLevel.enemiesRanged);
        // var board = Instantiate(_boardPrefab, center, Quaternion.identity);
        // board.size = new Vector2(_width,_height);
        // Camera.main.transform.position = new Vector3(player.transform.position.x, player.transform.position.y, -10);
        Camera.main.transform.position = new Vector3(center.x, center.y, -10);

    }
    void RockTile(int x, int y)
    {
        var node = Instantiate(nodePrefab, new Vector2(x, y + GridOffset), Quaternion.identity, grid);
        _nodes.Add(node);
    }
    void LavaPoolTile(int x, int y)
    {
        var lava = Instantiate(lavaPoolPrefab, new Vector2(x, y + GridOffset), Quaternion.identity, grid);
        _lavaspool.Add(lava);
    }
    private void GeneratePath(ref List<Vector2> nodeDenyList)
    {
        bool endReach = false;
        int levdimens = _width;
        int initpivot = UnityEngine.Random.Range(0, _height);
        while (!endReach)
        {
            Vector3[] Trace = TraceMaker(ref levdimens, initpivot);
            levdimens -= Trace.Length;
            if (!(levdimens > 0)) endReach = true;
            foreach (Vector3 t in Trace)
            {
                nodeDenyList.Add(t);
            }

            if (!endReach)
            {
                int levpivot = initpivot;
                initpivot = UnityEngine.Random.Range(0, _height);

                Vector3[] Patch = TracePatcher(levpivot, initpivot, levdimens);
                foreach (Vector3 t in Patch)                                            //Debug here
                {
                    nodeDenyList.Add(t);
                }
            }
        }
    }
    private Vector3[] TracePatcher(int start, int end, int pendinglength)
    {
        int difference = end - start;
        int direction = difference > 0 ? 1 : -1;
        Vector3[] tracebuilder = new Vector3[Mathf.Abs(difference)];
        int levx = start + direction;
        for (int i = 0; i < tracebuilder.Length; ++i)
        {
            tracebuilder[i] = new Vector3(_width - pendinglength - 1, levx);
            levx += direction;
        }
        return tracebuilder;
    }
    private Vector3[] TraceMaker(ref int pendinglength, int tracepivot)
    {
        int tracelength = pendinglength == 1 ? 1 : UnityEngine.Random.Range(2, pendinglength);
        Vector3[] tracebuilder = new Vector3[tracelength];
        int levy = _width - pendinglength;
        for (int i = 0; i < tracelength; ++i)
        {
            tracebuilder[i] = new Vector3(levy, tracepivot);
            ++levy;
        }
        return tracebuilder;
    }
    void InitEnemies(int enemiesMelee, int enemiesRanged)
    {
        for (int i = 0; i < enemiesMelee; ++i)
        {
            Vector2 randpos;
            do
            {
                randpos = new Vector2(_width - 2, Random.Range(0, _height) + GridOffset);

            } while (GetNodeAtPosition(randpos) == null);
            var _enemy = Instantiate(enemyMeleePrefab, randpos, Quaternion.identity, others);
            _enemy.name = "enemy" + enemylev;
            _enemies.Add(_enemy);
            ++enemylev;
        }
        for (int i = 0; i < enemiesRanged; ++i)
        {
            Vector2 randpos;
            do
            {
                randpos = new Vector2(_width - 2, Random.Range(0, _height) + GridOffset);

            } while (GetNodeAtPosition(randpos) == null);
            var _enemy = Instantiate(enemyRangedPrefab, randpos, Quaternion.identity, others);
            _enemy.name = "enemy" + enemylev;
            _enemies.Add(_enemy);
            ++enemylev;
        }
    }
    void InitLavaRow(int x)
    {
        for (int y = 0; y < _height; y++)
        {
            var lava = Instantiate(lavaPrefab, new Vector2(x, y + GridOffset), Quaternion.identity, grid);
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
                ChangeState(GameState.GenerateLevel);
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
        HealthText.SetText(string.Format("{0}", player._health));
    }
    void MoveLava()
    {
        InitLavaRow(_lavaTimer / LavaLimit);
    }

    void PickupGoldCheck()
    {
        foreach (GoldBag g in _goldBags.ToList())
        {
            if ((Vector2)player.transform.position == g.Pos)
            {// if player at gold position, remove gold and give gold to player
                player.GainGold(g.value);
                CoinText.SetText(string.Format("{0}", player.coinCount));
                _goldBags.Remove(g);
                Destroy(g.gameObject);
            }

            var e = GetEnemyAtPosition(g.Pos);
            if (e != null)
            {// if enemy at gold position, remove gold and give gold to enemy
                e.GainGold(g.value);
                _goldBags.Remove(g);
                Destroy(g.gameObject);
            }
        }
    }

    void SpawnGold()
    {
        var possibleLocations = new List<Vector2>();
        for (int x = 0; x < _width; x++)
        {
            for (int y = 0; y < _height; y++)
            {
                var location = new Vector2(x, y + GridOffset);
                if (//continue if any of condition x is true, continue if there is an object at the position. if getXatLoc() != null
                    (GetEnemyAtPosition(location) == null) ||
                    (GetLavaAtPosition(location) == null) ||
                    (GetGoldAtPosition(location) == null) ||
                    location != (Vector2)player.transform.position ||
                    location != (Vector2)exit.transform.position
                    )
                {
                    possibleLocations.Add(location);
                }
            }
        }
        if (possibleLocations.Count != 0)
        {
            var _gold = Instantiate(GoldBagPrefab, possibleLocations[Random.Range(0, possibleLocations.Count)], Quaternion.identity, others);
            _goldBags.Add(_gold);
        }
        else print("No spots");
        foreach (GoldBag G in _goldBags.ToList())
        {
            if (GetLavaAtPosition(G.transform.position) != null)
            {
                Destroy(G.gameObject);
            }
            if (GetNodeAtPosition(G.transform.position) == null)
            {
                Destroy(G.gameObject);
            }
        }
    }

    public void ExitLevel()
    {
        print("exiting level");
        foreach (Transform child in grid)
        {
            _nodes.Remove(child.GetComponent<Node>());
            Destroy(child.gameObject);
        }
        foreach (Enemy child in _enemies.ToList())
        {
            _enemies.Remove(child);
            Destroy(child.gameObject);
        }
        foreach (GoldBag child in _goldBags.ToList())
        {
            _goldBags.Remove(child);
            Destroy(child.gameObject);
        }
        Destroy(player.gameObject);
        Destroy(exit.gameObject);
        _lavaTimer = 0;
        _goldTimer = 0;
    }


    public void DestroyLevel()
    {
        print("Destroy level");
        foreach (Transform child in grid)
        {
            _nodes.Remove(child.GetComponent<Node>());
            Destroy(child.gameObject);
        }
        foreach (Enemy child in _enemies.ToList())
        {
            _enemies.Remove(child);
            Destroy(child.gameObject);
        }
        foreach (GoldBag child in _goldBags.ToList())
        {
            _goldBags.Remove(child);
            Destroy(child.gameObject);
        }
        Destroy(player.gameObject);
        Destroy(exit.gameObject);
        _lavaTimer = 0;
        _goldTimer = 0;
    }




    void GameOver()
    {
        ChangeState(GameState.Lose);

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
    Stop,
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
