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
    private Vector2 _lastDir = Vector2.right;
    [SerializeField] private Transform grid;
    [SerializeField] private Transform others;
    [SerializeField] private Node nodePrefab;
    [SerializeField] private Exit exitPrefab;

    private Exit exit;
    [SerializeField] private Player playerPrefab;
    private Player player;
    [SerializeField] private Enemy enemyPrefab;


    private List<Enemy> _enemies;
    // private List<Block> _blocks;

    private List<Node> _nodes;
    // private List<Block> _blocks;
    private GameState _state;
    // private int _round;
    // Start is called before the first frame update

    void Start() {
       ChangeState(GameState.GenerateLevel);
    }

    private void ChangeState(GameState newState) {
        _state = newState;

        switch (newState) {
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
                break;
            case GameState.EnemiesMoving:
                MoveEnemies();
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
    void Update() {
        if(_state != GameState.WaitingInput) return;

        if(Input.GetKeyDown(KeyCode.LeftArrow)) MovePlayer(Vector2.left);
        if(Input.GetKeyDown(KeyCode.RightArrow)) MovePlayer(Vector2.right);
        if(Input.GetKeyDown(KeyCode.UpArrow)) MovePlayer(Vector2.up);
        if(Input.GetKeyDown(KeyCode.DownArrow)) MovePlayer(Vector2.down);

        _turnTimer += Time.deltaTime;
        if(_turnTimer >= TurnLimit){
            ChangeState(GameState.EnemiesMoving);
            _turnTimer = 0;
        }

        // if(Input.GetKeyDown(KeyCode.Space)) 

    }
    void GenerateGrid() {
        // _round = 0;
        // print("grid making");
        _nodes = new List<Node>();
        _enemies = new List<Enemy>();
        // _blocks = new List<Block>();
        for (int x = 0; x < _width; x++) {
            for (int y = 0; y < _height; y++) {
                var node = Instantiate(nodePrefab, new Vector2(x, y), Quaternion.identity, grid);
                _nodes.Add(node);
            }
        }

        var center = new Vector2((float) _width /2 - 0.5f,(float) _height / 2 -0.5f);

        exit = Instantiate(exitPrefab, new Vector2(_width-1, Random.Range(0, _height)), Quaternion.identity, others);

        InitEnemies();
        
        player = Instantiate(playerPrefab, new Vector2(0, _height%2 == 0 ? _height / 2 : (float) _height / 2 -0.5f), Quaternion.identity, others);

        // var board = Instantiate(_boardPrefab, center, Quaternion.identity);
        // board.size = new Vector2(_width,_height);

        Camera.main.transform.position = new Vector3(center.x,center.y,-10);
    }

    void InitEnemies(){
        var _enemy = Instantiate(enemyPrefab, new Vector2(_width-2, Random.Range(0, _height)), Quaternion.identity, others);
        var _enemy1 = Instantiate(enemyPrefab, new Vector2(_width-3, Random.Range(0, _height)), Quaternion.identity, others);
        _enemies.Add(_enemy);
        _enemies.Add(_enemy1);
    }

    void MovePlayer(Vector2 dir) {

        Vector2 possibleLocation = (Vector2)player.transform.position + dir;


        var possibleNode = GetNodeAtPosition(possibleLocation);
            if (possibleNode != null) {//if grid exists
                _turnTimer = 0;
                var Enemy = GetEnemyAtPosition(possibleLocation);
                if(Enemy == null){//if there are no enemies at the location
                    player.transform.position = possibleLocation;
                }
                else{
                    print("enemies here");
                    Fight(Enemy);
                    Enemy = GetEnemyAtPosition(possibleLocation);
                    if(Enemy == null){//if there are no enemies at the location
                        player.transform.position = possibleLocation;
                    }
                }

                if(exit.transform.position == player.transform.position){
                    ExitLevel();
            }
        }


        else ChangeState(GameState.EnemiesMoving);
    }

    void MoveEnemies() {
        // print("Enemies move now");

        ChangeState(GameState.WaitingInput);
    }

    void Fight(Enemy fightingEnemy){
        if(player._health - fightingEnemy._attack <= 0) {
            GameOver();
        }

        player.Takedmg(fightingEnemy._attack);

        if(fightingEnemy._health - player._attack <= 0) {
            _enemies.Remove(fightingEnemy);
        }

        fightingEnemy.Takedmg(player._attack);  
    }



    void ExitLevel(){
        foreach(Transform child in grid)
        {
            Destroy(child.gameObject);
        }
        Destroy(player.gameObject);
        Destroy(exit.gameObject);
        ChangeState(GameState.GenerateLevel);

    }

    void GameOver(){
        print("Game Over");
        ExitLevel();
    }

    Node GetNodeAtPosition(Vector2 pos) {
        return _nodes.FirstOrDefault(n => n.Pos == pos);
    }

    Enemy GetEnemyAtPosition(Vector2 pos) {
        return _enemies.FirstOrDefault(n => n.Pos == pos);
    }
}

public enum GameState {
    GenerateLevel,
    SpawningBlocks,
    WaitingInput,
    Moving,
    EnemiesMoving,
    Win,
    Lose
}
