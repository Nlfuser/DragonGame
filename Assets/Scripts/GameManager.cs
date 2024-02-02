using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private int _width = 4;
    [SerializeField] private int _height = 4;
    [SerializeField] private Transform grid;
    [SerializeField] private Node _nodePrefab;
    [SerializeField] private Player playerPrefab;
    private Player _player;


    private List<Node> _nodes;
    // private List<Block> _blocks;
    private GameState _state;
    private int _round;
    // Start is called before the first frame update

    void Start() {
       ChangeState(GameState.GenerateLevel);
    }

    private void ChangeState(GameState newState) {
        _state = newState;

        switch (newState) {
            case GameState.GenerateLevel:
                GenerateGrid();
                break;
            case GameState.SpawningBlocks:
                // SpawnBlocks(_round++ == 0 ? 2 : 1);
                ChangeState(GameState.WaitingInput);
                break;
            case GameState.WaitingInput:
                break;
            case GameState.Moving:
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

        // if(Input.GetKeyDown(KeyCode.LeftArrow)) Shift(Vector2.left);
        // if(Input.GetKeyDown(KeyCode.RightArrow)) Shift(Vector2.right);
        // if(Input.GetKeyDown(KeyCode.UpArrow)) Shift(Vector2.up);
        // if(Input.GetKeyDown(KeyCode.DownArrow)) Shift(Vector2.down);
    }

    void GenerateGrid() {
        _round = 0;
        _nodes = new List<Node>();
        // _blocks = new List<Block>();
        for (int x = 0; x < _width; x++) {
            for (int y = 0; y < _height; y++) {
                var node = Instantiate(_nodePrefab, new Vector2(x, y), Quaternion.identity, grid);
                _nodes.Add(node);
            }
        }

        var center = new Vector2((float) _width /2 - 0.5f,(float) _height / 2 -0.5f);
        print(_height%2 == 0 ? _height / 2 : _height / 2 -0.5f);
        _player = Instantiate(playerPrefab, new Vector2(0, _height%2 == 0 ? _height / 2 : _height / 2 -0.5f), Quaternion.identity);

        // var board = Instantiate(_boardPrefab, center, Quaternion.identity);
        // board.size = new Vector2(_width,_height);

        Camera.main.transform.position = new Vector3(center.x,center.y,-10);

        ChangeState(GameState.SpawningBlocks);
    }

    // void Shift(Vector2 dir) {
    //     ChangeState(GameState.Moving);

    //     var orderedBlocks = _blocks.OrderBy(b => b.Pos.x).ThenBy(b => b.Pos.y).ToList();
    //     if (dir == Vector2.right || dir == Vector2.up) orderedBlocks.Reverse();

    //     foreach (var block in orderedBlocks) {
    //         var next = block.Node;
    //         do {
    //             block.SetBlock(next);

    //             var possibleNode = GetNodeAtPosition(next.Pos + dir);
    //             if (possibleNode != null) {
    //                 // We know a node is present
    //                 // If it's possible to merge, set merge
    //                 if (possibleNode.OccupiedBlock != null && possibleNode.OccupiedBlock.CanMerge(block.Value)) {
    //                     block.MergeBlock(possibleNode.OccupiedBlock);
    //                 }
    //                 // Otherwise, can we move to this spot?
    //                 else if (possibleNode.OccupiedBlock == null) next = possibleNode;

    //                 // None hit? End do while loop
    //             }
                

    //         } while (next != block.Node);
    //     }

    // }
}

public enum GameState {
    GenerateLevel,
    SpawningBlocks,
    WaitingInput,
    Moving,
    Win,
    Lose
}
