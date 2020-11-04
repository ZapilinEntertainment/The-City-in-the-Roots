using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ColonyToken : byte { Housing_1 = 0, Housing_2, Trade_1, Trade_2, Industrial_1, Industrial_2, Farm, Generator, Park, Demolition, Total }
//dependency: GameMaster - GetIconUV

public class ColonyController : MonoBehaviour
{   
    private Dictionary<HexPos, Hex> hexlist;
    private List<Building> buildingsList;
    private List<DelayedEvent> delayedEvents; 
    public Dictionary<int, BuildingBoost> boostsList { get; private set; }
    private int lastBoostID = 1;
    public int[] tokens { get; private set; }
    public float money { get; private set; }
    public bool needRecalculation = false;
    private byte fire_timer = 0; private byte fire_level = 0;
    private float timer = RECALCULATION_TIME;
    private const float RECALCULATION_TIME = 1f;

    public float lifepower { get; private set; }
    public float lifepowerSurplus { get; private set; }
    public float moneySurplus { get; private set; }

    public float people { get; private set; }
    private byte currentRing = 1;
    public float nextLifepowerValue = 12f;

    public static ColonyController current;

    private void Awake()
    {
        current = this;
        tokens = new int[(int)ColonyToken.Total];
        buildingsList = new List<Building>();
        boostsList = new Dictionary<int, BuildingBoost>();
        delayedEvents = new List<DelayedEvent>();
        GameMaster.current.endTurnEvent += this.EndTurn;
    }

    public void AddHex(Hex h)
    {
        if (hexlist == null) hexlist = new Dictionary<HexPos, Hex>();
        hexlist.Add(h.position, h);
    }
    public Hex GetHex(int ring, int index)
    {
        if (ring > 255 || index > 255 || ring < 0 || index < 0) return null;
        var hpos = new HexPos((byte)ring, (byte)index);
        if (hexlist != null && hexlist.ContainsKey(hpos)) return hexlist[hpos];
        else return null;
    }

    public void Start()
    {
        UIMaster.current.TakeControl();
        CardMaster.current.PrepareForChoice();
    }
    private void Update()
    {
        timer -= Time.deltaTime;
        if (timer <= 0f)
        {
            if (needRecalculation) Recalculate();
            timer = RECALCULATION_TIME;
        }
    }
    private void Recalculate()
    {
        moneySurplus = 0f;
        lifepowerSurplus = 0f;
        people = 0f;
        if (buildingsList.Count > 0)
        {
            foreach (var b in buildingsList)
            {
                moneySurplus += b.moneySurplus;
                people += b.people;
                lifepowerSurplus += b.lifepowerSurplus;
            }
        }
        if (fire_timer > 0)
        {
            switch(fire_level)
            {
                case 1:
                    if (lifepowerSurplus > 0f) lifepowerSurplus *= 0.5f;
                    break;
                case 2: lifepowerSurplus = 0f; break;
                case 3: lifepowerSurplus = -1f; break;
            }
        }
        UIMaster.current.RefreshInfopanel();
        needRecalculation = false;
    }

    public void EndTurn()
    {
        money += moneySurplus;
        if (money < 0f) money = 0f;
        lifepower += lifepowerSurplus; 
        if (lifepower >= nextLifepowerValue)
        {
            currentRing++;
            if (currentRing < 6) Constructor.CreateRing(currentRing);
            switch (currentRing)
            {
                case 2: nextLifepowerValue = GameConstants.LIFEPOWER_LIMIT_1; break;
                case 3: nextLifepowerValue = GameConstants.LIFEPOWER_LIMIT_2; break;
                case 4: nextLifepowerValue = GameConstants.LIFEPOWER_LIMIT_3; break;
                case 5: nextLifepowerValue = GameConstants.LIFEPOWER_LIMIT_4; break;
            }
        }
        else
        {
            if (lifepower < 0f) lifepower = 0f;
        }
        if (delayedEvents.Count > 0)
        {
            int i = 0;
            DelayedEvent de;
            while (i < delayedEvents.Count)
            {
                de = delayedEvents[i];
                if (de.turnsLeft == 0)
                {
                    switch (de.type)
                    {
                        case DelayedEventType.ChangeFireLevel:
                            if (fire_timer > 0) fire_level = de.value;
                            break;
                    }
                    delayedEvents.RemoveAt(i);
                    continue;
                }
                else
                {
                    de.EndTurn();
                    i++;
                }
            }
        }
        Recalculate();
    }

    public void ApplyRoll(RollResult rr)
    {
        bool newTokens = false;
        switch(rr)
        {
            case RollResult.Housing_1:
                tokens[(int)ColonyToken.Housing_1]++;
                newTokens = true;
                break;
            case RollResult.Housing_2: tokens[(int)ColonyToken.Housing_2]++; newTokens = true; break;
            case RollResult.Trade_1: tokens[(int)ColonyToken.Trade_1]++; newTokens = true; break;
            case RollResult.Trade_2: tokens[(int)ColonyToken.Trade_2]++; newTokens = true; break;
            case RollResult.Industrial_1:
                tokens[(int)ColonyToken.Industrial_1]++; newTokens = true; break;
            case RollResult.Industrial_2:
                tokens[(int)ColonyToken.Industrial_2]++; newTokens = true; break;
            case RollResult.DemolitionToken:
                tokens[(int)ColonyToken.Demolition]++; newTokens = true; break;
            case RollResult.Park:
                tokens[(int)ColonyToken.Park]++; newTokens = true; break;
            case RollResult.Generator:
                tokens[(int)ColonyToken.Generator]++; newTokens = true; break;
            case RollResult.Farm:
                tokens[(int)ColonyToken.Farm]++; newTokens = true; break;
            case RollResult.Gift_1:
                AddMoney(GameConstants.GIFT_1_COUNT);
                break;
            case RollResult.Gift_2:
                AddMoney(GameConstants.GIFT_2_COUNT);
                break;
            case RollResult.Gift_3:
                AddMoney(GameConstants.GIFT_3_COUNT);
                break;
            case RollResult.Loss_1: SpendMoney(GameConstants.LOSS_1_COUNT); break;
            case RollResult.Loss_2: SpendMoney(GameConstants.LOSS_2_COUNT); break;
            case RollResult.Loss_3: SpendMoney(GameConstants.LOSS_3_COUNT); break;
            case RollResult.Fire_1: StartGameEvent(EventType.Fire, 1); break;
            case RollResult.Fire_2: StartGameEvent(EventType.Fire, 2); break;
            case RollResult.Fire_3: StartGameEvent(EventType.Fire, 3); break;
            case RollResult.Disease_1: StartGameEvent(EventType.Disease, 1); break;
            case RollResult.Disease_2: StartGameEvent(EventType.Disease, 2); break;
            case RollResult.Disease_3: StartGameEvent(EventType.Disease, 3); break;
        }
        if (newTokens) UIMaster.current.PrepareTokensPanel();
    }

    public void SpendMoney(float f)
    {
        if (money > f) money -= f; else money = 0f;
        UIMaster.current.RefreshInfopanel();
    }
    public bool TrySpendMoney(float f)
    {
        if (money >= f)
        {
            money -= f;
            return true;
        }
        else return false;
    }
    public void AddMoney( float f)
    {
        money += f;
        UIMaster.current.RefreshInfopanel();
    }
    public bool TrySpentToken( ColonyToken ct)
    {
        if (ct == ColonyToken.Total) return false;
        if (tokens[(int)ct] > 0)
        {
            tokens[(int)ct]--;
            return true;
        }
        else return false;
    }


    public void AddBuilding(Building b)
    {
        if (!buildingsList.Contains(b))
        {
            buildingsList.Add(b);
            needRecalculation = true;
        }
    }

    public void RegisterBonus(BuildingBoost boost)
    {
        if (boost.bonusGiver.destroyed || boost.bonusApplier.destroyed) return;
        int key = lastBoostID++;
        boostsList.Add(key, boost);
        boost.bonusGiver.ApplyBoost(key);
        boost.bonusApplier.ApplyBoost(key);
    }
    public void AnnulateBonus(int key)
    {
        BuildingBoost b;
        if (boostsList.TryGetValue(key, out b))
        {
            b.bonusApplier.AnnulateBoost(key);
            b.bonusGiver.AnnulateBoost(key);
            boostsList.Remove(key);
        }
    }

    public void StartGameEvent(EventType i_type, byte i_lvl)
    {
        List<Hex> suitableList = new List<Hex>();
        Hex h;
        foreach (var fh in hexlist)
        {
            h = fh.Value;
            if (h.ongoingEvent == null) suitableList.Add(h);
        }
        if (suitableList.Count == 0) return;
        switch (i_type)
        {            
            case EventType.Fire:
                switch (i_lvl)
                {
                    case 1:
                        {
                            h = suitableList[Random.Range(0, suitableList.Count)];
                            h.AddEvent(new HexEvent(h, EventType.Fire, 1));
                            if (fire_timer > 0)
                            {
                                if (fire_level != 1) delayedEvents.Add(new DelayedEvent(DelayedEventType.ChangeFireLevel, fire_timer, 1));
                            }
                            else fire_level = 1;
                            fire_timer = GameConstants.FIRE_EVENT_LENGTH;                            
                            break;
                        }
                    case 2:
                        {
                            int i = Random.Range(0, suitableList.Count);
                            h = suitableList[i]; suitableList.RemoveAt(i);
                            h.AddEvent(new HexEvent(h, EventType.Fire, 2));
                            if (suitableList.Count > 0)
                            {
                                i = Random.Range(0, suitableList.Count);
                                h = suitableList[i];
                                h.AddEvent(new HexEvent(h, EventType.Fire, 2));
                                suitableList.RemoveAt(i);
                                if (suitableList.Count > 0)
                                {
                                    i = Random.Range(0, suitableList.Count);
                                    h = suitableList[i];
                                    h.AddEvent(new HexEvent(h, EventType.Fire, 2));
                                    suitableList.RemoveAt(i);
                                }
                            }
                            if (fire_timer > 0)
                            {
                                if (fire_level != 2) delayedEvents.Add(new DelayedEvent(DelayedEventType.ChangeFireLevel, fire_timer, 2));
                            }
                            else fire_level = 2;
                            fire_timer = GameConstants.FIRE_EVENT_LENGTH;
                            break;
                        }
                    case 3:
                        {
                            int i = Random.Range(0, suitableList.Count);
                            h = suitableList[i]; suitableList.RemoveAt(i);
                            h.AddEvent(new HexEvent(h, EventType.Fire, 3));
                            if (suitableList.Count > 0)
                            {
                                i = Random.Range(0, suitableList.Count);
                                h = suitableList[i];
                                h.AddEvent(new HexEvent(h, EventType.Fire, 3));
                                suitableList.RemoveAt(i);
                                if (suitableList.Count > 0)
                                {
                                    i = Random.Range(0, suitableList.Count);
                                    h = suitableList[i];
                                    h.AddEvent(new HexEvent(h, EventType.Fire, 3));
                                    suitableList.RemoveAt(i);
                                    if (suitableList.Count > 0)
                                    {
                                        i = Random.Range(0, suitableList.Count);
                                        h = suitableList[i];
                                        h.AddEvent(new HexEvent(h, EventType.Fire, 3));
                                        suitableList.RemoveAt(i);
                                        if (suitableList.Count > 0)
                                        {
                                            i = Random.Range(0, suitableList.Count);
                                            h = suitableList[i];
                                            h.AddEvent(new HexEvent(h, EventType.Fire, 3));
                                            suitableList.RemoveAt(i);
                                        }
                                    }
                                }
                            }
                            if (fire_timer > 0)
                            {
                                if (fire_level != 3) delayedEvents.Add(new DelayedEvent(DelayedEventType.ChangeFireLevel, fire_timer, 3));
                            }
                            else fire_level = 3;
                            fire_timer = GameConstants.FIRE_EVENT_LENGTH;
                            break;
                        }
                }
                break;
            case EventType.Disease:
                switch (i_lvl)
                {
                    case 1:
                        {
                            int i = Random.Range(0, suitableList.Count);
                            h = suitableList[i];
                            h.AddEvent(new HexEvent(h, EventType.Disease, 1));                          
                            break;
                        }
                    case 2:
                        {
                            int i = Random.Range(0, suitableList.Count);
                            h = suitableList[i];
                            h.AddEvent(new HexEvent(h, EventType.Disease, 2));
                            suitableList.RemoveAt(i);
                            if (suitableList.Count > 0)
                            {
                                i = Random.Range(0, suitableList.Count);
                                h = suitableList[i];
                                h.AddEvent(new HexEvent(h, EventType.Disease, 2));
                                suitableList.RemoveAt(i);
                                if (suitableList.Count > 0)
                                {
                                    i = Random.Range(0, suitableList.Count);
                                    h = suitableList[i];
                                    h.AddEvent(new HexEvent(h, EventType.Disease, 2));
                                    suitableList.RemoveAt(i);
                                }
                            }                            
                            break;
                        }
                    case 3:
                        {
                            int i = Random.Range(0, suitableList.Count);
                            h = suitableList[i];
                            h.AddEvent(new HexEvent(h, EventType.Disease, 3));
                            suitableList.RemoveAt(i);
                            if (suitableList.Count > 0)
                            {
                                i = Random.Range(0, suitableList.Count);
                                h = suitableList[i];
                                h.AddEvent(new HexEvent(h, EventType.Disease, 3));
                                suitableList.RemoveAt(i);
                                if (suitableList.Count > 0)
                                {
                                    i = Random.Range(0, suitableList.Count);
                                    h = suitableList[i];
                                    h.AddEvent(new HexEvent(h, EventType.Disease, 3));
                                    suitableList.RemoveAt(i);
                                    if (suitableList.Count > 0)
                                    {
                                        i = Random.Range(0, suitableList.Count);
                                        h = suitableList[i];
                                        h.AddEvent(new HexEvent(h, EventType.Disease, 3));
                                        suitableList.RemoveAt(i);
                                        if (suitableList.Count > 0)
                                        {
                                            i = Random.Range(0, suitableList.Count);
                                            h = suitableList[i];
                                            h.AddEvent(new HexEvent(h, EventType.Disease, 3));
                                            suitableList.RemoveAt(i);
                                        }
                                    }
                                }
                            }
                            break;
                        }
                }
                break;
        }
       
    }
}
