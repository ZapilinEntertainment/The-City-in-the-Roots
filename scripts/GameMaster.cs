using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameMaster : MonoBehaviour
{
    public static bool startNewGame = true;
    public static readonly Vector3 sceneCenter = Vector3.zero;
    public System.Action endTurnEvent;
    public static GameMaster current;
    public int turnNumber { get; private set; }

    private void Awake()
    {
        current = this;
    }

    void Start()
    {
        if (PoolMaster.current == null)   gameObject.AddComponent<PoolMaster>().Prepare();
        if (startNewGame)
        {
            UIMaster.current.Initialize();
            Constructor.CreateRing(1);
        }
    }

    public void EndTurn()
    {
        turnNumber++;
        endTurnEvent();
        UIMaster.current.EndTurn();
    }

    public static Icon GetIconType(RollResult rr)
    {
        switch (rr)
        {
            case RollResult.Housing_1: return Icon.Housing_1;
            case RollResult.Housing_2: return Icon.Housing_2;
            case RollResult.Trade_1: return Icon.Trade_1;
            case RollResult.Trade_2: return Icon.Trade_2;
            case RollResult.Industrial_1: return Icon.Industrial_1;
            case RollResult.Industrial_2: return Icon.Industrial_2;
            case RollResult.Gift_1:
            case RollResult.Gift_2:
            case RollResult.Gift_3: return Icon.MoneyGift;
            case RollResult.Rain: return Icon.Rain;
            case RollResult.Fire_1:
            case RollResult.Fire_2:
            case RollResult.Fire_3:
                return Icon.Fire;
            case RollResult.Disease_1:
            case RollResult.Disease_2:
            case RollResult.Disease_3:
                return Icon.Disease;
            case RollResult.Loss_1:
            case RollResult.Loss_2:
            case RollResult.Loss_3:
                return Icon.MoneyLoss;
            case RollResult.DemolitionToken: return Icon.Demolition;
            case RollResult.Park: return Icon.Park;
            case RollResult.Generator: return Icon.Generator;
            case RollResult.Farm: return Icon.Farm;
            default: return Icon.Unknown;
        }
    }
    public static Icon GetIconType(ColonyToken ct)
    {
        switch (ct)
        {
            case ColonyToken.Housing_1: return Icon.Housing_1;
            case ColonyToken.Housing_2: return Icon.Housing_2;
            case ColonyToken.Trade_1: return Icon.Trade_1;
            case ColonyToken.Trade_2: return Icon.Trade_2;
            case ColonyToken.Industrial_1: return Icon.Industrial_1;
            case ColonyToken.Industrial_2: return Icon.Industrial_2;
            case ColonyToken.Farm: return Icon.Farm;
            case ColonyToken.Generator: return Icon.Generator;
            case ColonyToken.Park: return Icon.Park;
            case ColonyToken.Demolition: return Icon.Demolition;
            default: return Icon.Unknown;
        }
    }

    public Hex GetNeighbour(Hex h, byte side)
    {
        var pos = h.GetSurfacePosition();
        Vector3 direction = Quaternion.AngleAxis(30f + 60f * side, Vector3.up) * Vector3.forward * Hex.r * 2f;
        RaycastHit rh;
        if (Physics.Raycast(h.transform.position + Vector3.down * 100 + direction, Vector3.up, out rh))
        {
            if (rh.collider.tag == GameConstants.HEX_COLLIDER_TAG) return rh.collider.transform.parent.GetComponent<Hex>();
            else return null;
        }
        else return null;
    }
}
