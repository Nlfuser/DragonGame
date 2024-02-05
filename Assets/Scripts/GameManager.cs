using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;
using TMPro;
using System.Xml.Linq;

public class GameManager : MonoBehaviour
{
    [SerializeField] private int AttackCost;
    [SerializeField] private int HealthCost;
    [SerializeField] private int RefillCost;
    [SerializeField] private int FlyCost;


    public static GameManager _Instance;
    public List<GameObject> Objects;
    GameObject querylev = null;
    public List<Level> gameLevels;
    private Level currentLevel;
    public int currentLevelIndex;
    private int _height;
    private int _width;
    public int GridOffset;
    [SerializeField] private float turnLimit;
    private float _turnTimer;
    [SerializeField] private int LavaLimit;
    private int _lavaTimer;
    [SerializeField] private int GoldLimit;
    private int _goldTimer;
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
    public Player playerLevelSnapshot;
    [SerializeField] private Enemy enemyMeleePrefab;
    [SerializeField] private Enemy enemyRangedPrefab;

    public List<Enemy> _enemies;
    public Vector2[] cardinals;
    //public float enemyrange = 3f;
    int enemylev = 0;
    // private List<Block> _blocks;

    public List<Node> _nodes;
    private List<Vector2> pathNodes;

    public List<Lava> _lavas;
    public List<Lava> _lavaspool;
    public List<GoldBag> _goldBags;

    public GameState _state;

    public TMP_Text AttackText;
    public TMP_Text TurnText;
    public TMP_Text TurnTimerText;
    public TMP_Text HealthText;
    public TMP_Text CoinText;

    private GameObject WinMenuUI;
    private GameObject GameOverMenuUI;
    private GameObject MainMenuUICanvas;
    private GameObject ShopMenuUI;
    private GameObject MainSceneUI;
    [SerializeField] private AudioManager AM;

    float coinCountMemory = 0;
    int maxhealthMemory = 10;
    int healthMemory = 10;
    bool canFlyMemory = false;
    int attackMemory = 1;

    public int lavaholefloor;
    public int rockceiling;

    private Camera myCamera;
    public int xcamera;
    public int ycamera;

    bool canmovelock;
    private void Awake()
    {
        if (_Instance != null) Destroy(gameObject);
        else
        {
            _Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        //RecursiveChildrenWrapper(Objects, transform);
        querylev = null;
    }
    private void RecursiveChildrenWrapper(List<GameObject> objects, Transform node)
    {
        foreach (Transform child in node)
        {
            objects.Add(child.gameObject);
            if (child.transform.childCount > 0)
            {
                RecursiveChildrenWrapper(objects, child);
            }
        }
    }
    void Start()
    {
        AM = transform.gameObject.GetComponent<AudioManager>();
        AM.Play("SearingEscape");
        ChangeState(GameState.Stop);
    }

    public void InitGameManager()
    {
        _lavaTimer = 0;
        _goldTimer = 0;
        canmovelock = false;
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
        WinMenuUI = GameObject.Find("WinMenuUI");
        GameOverMenuUI = GameObject.Find("GameOverMenuUI");
        MainMenuUICanvas = GameObject.Find("MainMenuUICanvas");
        ShopMenuUI = GameObject.Find("ShopMenuUI");
        MainSceneUI = GameObject.Find("MainSceneUI");
    }

    public void ChangeState(GameState newState)
    {
        _state = newState;

        switch (newState)
        {
            case GameState.Stop:
                InitGameManager();
                break;
            case GameState.GenerateLevel:
                GenerateGrid();
                grid.gameObject.SetActive(true);
                AttackText.SetText(string.Format("{0}", player._attack));
                UIHealthUpdate();
                rememberPlayer();
                ChangeState(GameState.WaitingInput);
                break;
            case GameState.WaitingInput:
                MainSceneUI.SetActive(true);
                canmovelock = true;
                TurnTimerText.SetText(string.Format("{0:N2}", turnLimit));
                break;
            case GameState.Moving:
                // HeuristicCamera();
                canmovelock = false;
                break;
            case GameState.EnemiesMoving:
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
                    PlayerHurt(lavaDamage);
                }
                if (GetLavaPoolAtPosition(player.transform.position) != null && !player.canFly)
                {
                    PlayerHurt(lavaDamage);
                }
                foreach (Enemy e in _enemies.ToList())
                {
                    if (GetLavaAtPosition(e.transform.position) != null)
                    {
                        if (e.Takedmg(lavaDamage)) _enemies.RemoveAll(i => i.Equals(e));
                    }
                    if (GetLavaPoolAtPosition(e.transform.position) != null)
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
                    if (GetLavaPoolAtPosition(G.transform.position) != null)
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
            case GameState.Shop:
                ShopMenuUI.SetActive(true);
                break;
            case GameState.Lose:
                MainSceneUI.SetActive(false);
                GameOverMenuUI.SetActive(true);
                break;
            case GameState.Win:
                MainSceneUI.SetActive(false);
                ShopMenuUI.SetActive(false);
                WinMenuUI.SetActive(true);
                    AM.Stop("SearingEscape");


                //     _winScreen.SetActive(true);
                //     Invoke(nameof(DelayedWinScreenText),1.5f);
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(newState), newState, null);
        }
    }
    void Update()
    {
        if(_state != GameState.Stop && _state != GameState.Win && _state != GameState.Lose && _state != GameState.Shop)
        {
            _turnTimer += Time.deltaTime;
            TurnTimerText.SetText(string.Format("{0:N2}", _turnTimer));
            if (_turnTimer >= turnLimit)
            {
                _turnTimer = 0;
                ChangeState(GameState.EnemiesMoving);
            }
        }
        try
        {
            if (canmovelock && player != null)
            {
                if (Input.GetKeyDown(KeyCode.LeftArrow)) MovePlayer(Vector2.left);
                if (Input.GetKeyDown(KeyCode.RightArrow)) MovePlayer(Vector2.right);
                if (Input.GetKeyDown(KeyCode.UpArrow)) MovePlayer(Vector2.up);
                if (Input.GetKeyDown(KeyCode.DownArrow)) MovePlayer(Vector2.down);
            }
        }
        catch(Exception e)
        {
            Debug.LogException(e);
        }
        //try { enemybehaviourtest(); } catch { }
        // if(Input.GetKeyDown(KeyCode.Space)) 
    }
    void GenerateGrid()
    {
        currentLevel = gameLevels.ElementAt(currentLevelIndex);
        ++currentLevelIndex;
        _nodes = new List<Node>();
        _enemies = new List<Enemy>();
        _lavas = new List<Lava>();
        _goldBags = new List<GoldBag>();
        _lavaspool = new List<Lava>();
        _width = currentLevel.xtiles;
        _height = currentLevel.ytiles;
        pathNodes = new List<Vector2>();
        GeneratePath(ref pathNodes);
        rockceiling = 100 - currentLevel.rockpercent;
        lavaholefloor = currentLevel.lavapoolpercent;
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
        exit = Instantiate(exitPrefab, pathNodes.Last() + new Vector2(0, GridOffset), Quaternion.identity, grid);
        InitEnemies(currentLevel.enemiesMelee, currentLevel.enemiesRanged);
        player = Instantiate(playerPrefab, pathNodes.ElementAt(1) + new Vector2(0, GridOffset), Quaternion.identity, others);
        recallPlayer();
        player.rogerUIUpdate();
        // var board = Instantiate(_boardPrefab, center, Quaternion.identity);
        // board.size = new Vector2(_width,_height);
        // Camera.main.transform.position = new Vector3(player.transform.position.x, player.transform.position.y, -10);
        Camera.main.transform.position = new Vector3(center.x, center.y, -10);

    }
    public void UIRegenerateCall()
    {
        GameOverMenuUI.SetActive(false);
        RegenerateLevel();
        ChangeState(GameState.WaitingInput);
    }
    void rememberPlayer()
    {
        coinCountMemory = player.coinCount;
        maxhealthMemory = player._maxhealth;
        healthMemory = player._health;
        canFlyMemory = player.canFly;
        attackMemory = player._attack;
    }

    void recallPlayer()
    {
        player._attack = attackMemory;
        player._health = healthMemory;
        player._maxhealth = maxhealthMemory;
        player.canFly = canFlyMemory;
        player.coinCount = coinCountMemory;
    }
    public void RegenerateLevel() //call from button
    {
        grid.gameObject.SetActive(true);
        InitLavaRow(0);
        player = Instantiate(playerPrefab, pathNodes.ElementAt(1) + new Vector2(0, GridOffset), Quaternion.identity, others);
        recallPlayer();
        player.rogerUIUpdate();
        _lavaTimer = 0;
        _goldTimer = 0;
        InitEnemies(currentLevel.enemiesMelee, currentLevel.enemiesRanged);
    }    

    void RockTile(int x, int y)
    {
        var node = Instantiate(nodePrefab, new Vector2(x, y + GridOffset), Quaternion.identity, grid);
        _nodes.Add(node);
    }
    void LavaPoolTile(int x, int y)
    {
        RockTile(x, y);
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
                foreach (Vector3 t in Patch)
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

            } while (GetNodeAtPosition(randpos) == null && GetLavaPoolAtPosition(randpos) == null && GetEnemyAtPosition(randpos) == null);
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

            } while (GetNodeAtPosition(randpos) == null && GetLavaPoolAtPosition(randpos) == null && GetEnemyAtPosition(randpos) == null);
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
        foreach (Enemy e in _enemies)
        {
            if (player.transform.position == e.transform.position) Destroy(e);
        }
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
                AM.Play("MoveFromTile");
                ChangeState(GameState.EnemiesMoving);
            }
            else
            {
                Fight(Enemy);
                Enemy = GetEnemyAtPosition(possibleLocation);
                if (Enemy == null)
                {//if there are no enemies at the location
                    player.transform.position = possibleLocation;
                    AM.Play("MoveFromTile");
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
        UIHealthUpdate();
    }
    void MoveLava()
    {
        InitLavaRow(_lavaTimer / LavaLimit);
        AM.Play("LavaMoving");
    }
    void PickupGoldCheck()
    {
        foreach (GoldBag g in _goldBags.ToList())
        {
            if ((Vector2)player.transform.position == g.Pos)
            {// if player at gold position, remove gold and give gold to player
                PlayerCoinGain(g.value);
                UICoinUpdate();
                _goldBags.Remove(g);
                Destroy(g.gameObject);
                AM.Play("CollectGem");
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
    public void PlayerCoinGain(int amount)
    {
        player.GainGold(amount);
        UICoinUpdate();
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
        AM.Play("ReachNextLevel");
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
        rememberPlayer();
        Destroy(player.gameObject);
        Destroy(exit.gameObject);
        _lavaTimer = 0;
        _goldTimer = 0;
        if (currentLevelIndex == gameLevels.Count)
        {
            ChangeState(GameState.Win);
        }
        else
        {
            ChangeState(GameState.Shop);
        }
    }


    public void DestroyLevel()
    {
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
        _lavaTimer = 0;
        _goldTimer = 0;
        if(exit != null) Destroy(exit.gameObject);
        if (player != null) Destroy(player.gameObject);
    }
    void GameOver()
    {
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
        foreach (Lava child in _lavas.ToList())
        {
            _lavas.Remove(child);
            Destroy(child.gameObject);
        }
        grid.gameObject.SetActive(false);
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
    public Lava GetLavaPoolAtPosition(Vector2 pos)
    {
        return _lavaspool.FirstOrDefault(n => n.Pos == pos);
    }
    public GoldBag GetGoldAtPosition(Vector2 pos)
    {
        return _goldBags.FirstOrDefault(n => n.Pos == pos);
    }
    public void UIAttackUpdate()
    {
        AttackText.SetText(string.Format("{0}", player._attack));
    }
    public void UICoinUpdate()
    {
        CoinText.SetText(string.Format("{0}", player.coinCount));
    }
    public void UIHealthUpdate()
    {
        HealthText.SetText(string.Format("{0}", player._health) + "/" + string.Format("{0}", player._maxhealth));
    }
    //shopui
    public void BoughtAttack()
    {
        if (player.coinCount >= AttackCost)
        {
            print("BoughtAttack");
            PlayerCoinGain(-AttackCost);
            player._attack += 1;
            attackMemory += 1;
            AttackText.SetText(string.Format("{0}", player._attack));
            CloseShop();
        }

    }
    public void BoughtMaxHealth()
    {
        if (player.coinCount >= HealthCost)
        {
            print("BoughtMaxHealth");
            PlayerCoinGain(-HealthCost);
            player._maxhealth += 5;
            maxhealthMemory += 5;
            player._health += 5;
            healthMemory += 5;
            UIHealthUpdate();
            CloseShop();
        }
    }

    public void BoughtHealthRefill()
    {
        if (player.coinCount >= RefillCost)
        {
            print("BoughtRefill");
            PlayerCoinGain(-RefillCost);
            player._health = player._maxhealth;
            healthMemory = maxhealthMemory;
            UIHealthUpdate();
            CloseShop();
        }
    }

    public void BoughtFlight()
    {
        if (player.coinCount >= FlyCost)
        {
            print("BoughtFlight");
            PlayerCoinGain(-FlyCost);
            player.canFly = true;
            canFlyMemory = true;
            CloseShop();
        }
    }
    public void CloseShop()
    {
        ShopMenuUI.SetActive(false);
        player.rogerUIUpdate();
        ChangeState(GameState.GenerateLevel);
    }
    //mainmenui
    public void PlayGame()
    {
        MainMenuUICanvas.SetActive(false);
        MainSceneUI.SetActive(true);
        ChangeState(GameState.GenerateLevel);
    }
    public void BackToTheMenu()
    {
        MainSceneUI.SetActive(false);
        GameOverMenuUI.SetActive(false);
        WinMenuUI.SetActive(false);
        MainMenuUICanvas.SetActive(true);
        DestroyLevel();
        ChangeState(GameState.Stop);
    }
    public void arrowcall(float arrowDuration, Vector2 playerPos, int _attack, Transform transform)
    {
        StartCoroutine(Arrow(arrowDuration, playerPos, _attack, transform));
    }
    public IEnumerator Arrow(float duration, Vector2 targetPos, int attack, Transform enemy)
    {
        float t = 0;
        GameObject arrowClone = Instantiate(ArrowPrefab, enemy.position, enemy.rotation);
        while (t < 1f && _state != GameState.Win && _state != GameState.Lose && _state != GameState.Shop && _state != GameState.Stop)
        {
            t += Time.deltaTime / duration;
            arrowClone.transform.position = Vector3.Lerp(enemy.position, targetPos, t);
            yield return null;
        }
        if (arrowClone.transform.position == player.transform.position)
        {
            PlayerHurt(attack);
        }
        Destroy(arrowClone);
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
    Shop,
    Win,
    Lose
}
