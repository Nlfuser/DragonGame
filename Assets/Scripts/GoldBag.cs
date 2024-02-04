using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoldBag : MonoBehaviour
{
    public Vector2 Pos => transform.position;
    public int value;
    private void OnDestroy()
    {
        GameManager._Instance._goldBags.Remove(this);
    }
}
