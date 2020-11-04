using System.Collections.Generic;
using UnityEngine;

public enum BuildingType : byte { Housing_1, Housing_2, Trade_1, Trade_2, Industrial_1, Industrial_2, Generator, Park, Farm, Tree}
public class Building
{
    private GameObject model;
    public readonly BuildingType type;
    public readonly Hex myHex;
    private bool enlisted = false;
    private int ID = 0;
    private byte modelRotation = 0;
    private static int lastID = 1;
    public float moneySurplus{get;private set;}
    public float lifepowerSurplus { get; private set; }    
    public float people { get; private set; }
    public float peopleSurplus { get; private set; }
    private float peopleSurplus_basic, lifepowerSurplus_basic, moneySurplus_basic;
    private List<int> buildingBoosts;
    public bool destroyed { get; private set; }
    public sbyte isLifepowerBoosted {
        get {
            if (lifepowerSurplus_basic == lifepowerSurplus) return 0;
            else
            {
                if (lifepowerSurplus_basic > lifepowerSurplus) return -1;
                else return 1;
            }
        }
    }
    public sbyte isMoneyBoosted
    {
        get
        {
            if (moneySurplus_basic == moneySurplus) return 0;
            else
            {
                if (moneySurplus_basic > moneySurplus) return -1;
                else return 1;
            }
        }
    }
    public sbyte isPeopleBoosted
    {
        get
        {
            if (peopleSurplus_basic == peopleSurplus) return 0;
            else
            {
                if (peopleSurplus_basic > peopleSurplus) return -1;
                else return 1;
            }
        }
    }

    public static bool operator ==(Building A, Building B)
    {
        if (ReferenceEquals(A, null))
        {
            return ReferenceEquals(B, null);
        }
        return A.Equals(B);
    }
    public static bool operator !=(Building A, Building B)
    {
        return !(A == B);
    }
    public override int GetHashCode()
    {
        return model.GetHashCode();
    }
    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType()) return false;
        var info = (Building)obj;
        if (ID == info.ID && model == info.model && type == info.type && myHex == info.myHex) return true;
        else return false;
    }

    public Building(BuildingType i_type, Hex h)
    {
        type = i_type;
        myHex = h;
        ID = lastID++;
        destroyed = false;
        if (type == BuildingType.Housing_1 | type == BuildingType.Housing_2) people = 1f;
        RecalculateBasicProperties();
        peopleSurplus = peopleSurplus_basic;
        moneySurplus = moneySurplus_basic;
        lifepowerSurplus = lifepowerSurplus_basic;
    }
    private bool RecalculateBasicProperties()
    {
        bool needRecalculation = false;
        switch (type)
        {
            case BuildingType.Housing_1:
                {
                    peopleSurplus_basic = GameConstants.PEOPLE_SURPLUS;
                    var pr = moneySurplus_basic;
                    moneySurplus_basic = GameConstants.HOUSING_1_MONEY_SURPLUS * ((int)people);
                    if (moneySurplus_basic != pr) needRecalculation = true;
                    pr = lifepowerSurplus_basic;
                    lifepowerSurplus_basic = GameConstants.LIFEPOWER_SURPLUS_HOUSING * ((int)people);
                    if (lifepowerSurplus_basic != pr) needRecalculation = true;
                    break;
                }
            case BuildingType.Housing_2:
                {
                    peopleSurplus_basic = GameConstants.PEOPLE_SURPLUS;
                    var pr = moneySurplus_basic;
                    moneySurplus_basic = GameConstants.HOUSING_2_MONEY_SURPLUS * ((int)people);
                    if (moneySurplus_basic != pr) needRecalculation = true;
                    pr = lifepowerSurplus_basic;
                    lifepowerSurplus_basic = GameConstants.LIFEPOWER_SURPLUS_HOUSING * ((int)people);
                    if (lifepowerSurplus_basic != pr) needRecalculation = true;
                    break;
                }
            case BuildingType.Industrial_1:
                {
                    moneySurplus_basic = GameConstants.INDUSTRIAL_1_MONEY_SURPLUS;
                    break;
                }
            case BuildingType.Industrial_2:
                {
                    moneySurplus_basic = GameConstants.INDUSTRIAL_2_MONEY_SURPLUS;
                    break;
                }
            case BuildingType.Tree:
                {
                    lifepowerSurplus_basic = GameConstants.TREE_LIFEPOWER_SURPLUS;
                    break;
                }
        }
        return needRecalculation;
    }

    public void InitializeModel(bool selectRotation)
    {
        switch (type)
        {
            case BuildingType.Housing_1:
                model = GameObject.Instantiate(Resources.Load<GameObject>("prefs/housing1"));                
                break;
            case BuildingType.Housing_2:
                model = GameObject.Instantiate(Resources.Load<GameObject>("prefs/housing2"));
                break;
            case BuildingType.Trade_1:
                model = GameObject.Instantiate(Resources.Load<GameObject>("prefs/trade1"));
                break;
            case BuildingType.Trade_2:
                model = GameObject.Instantiate(Resources.Load<GameObject>("prefs/trade2"));
                break;
            case BuildingType.Industrial_1:
                model = GameObject.Instantiate(Resources.Load<GameObject>("prefs/industrial1"));
                break;
            case BuildingType.Industrial_2:
                model = GameObject.Instantiate(Resources.Load<GameObject>("prefs/industrial2"));
                break;
            case BuildingType.Generator:
                model = GameObject.Instantiate(Resources.Load<GameObject>("prefs/generator"));
                break;
            case BuildingType.Park:
                model = GameObject.Instantiate(Resources.Load<GameObject>("prefs/park"));
                break;
            case BuildingType.Farm:
                model = GameObject.Instantiate(Resources.Load<GameObject>("prefs/farm"));
                break;
            case BuildingType.Tree:
                model = GameObject.Instantiate(Resources.Load<GameObject>("prefs/tree"));
                break;
        }
        if (model != null)
        {
            model.transform.position = myHex.GetSurfacePosition();
            if (selectRotation) modelRotation = (byte)Random.Range(0, 6);
            model.transform.rotation = Quaternion.Euler(0f, modelRotation * 60f, 0f);
        }
    }
    public void EnlistToColony()
    {
        if (enlisted || myHex == null) return;
        ApplyAffections();
        var colony = ColonyController.current;
        colony.AddBuilding(this);
        if (peopleSurplus != 0f) GameMaster.current.endTurnEvent += this.EndTurn;
        enlisted = true;
    }
    private void EndTurn()
    {
        people += peopleSurplus;
        if (people < 1f) people = 1f;
        if (RecalculateBasicProperties()) RecalculateBoosts();
    }

    private void ApplyAffections()
    {
        Hex h = GameMaster.current.GetNeighbour(myHex, 0);
        Building b = h?.building;
        if (b != null) { ApplyAffection(b);  b.ApplyAffection(this); }
        h = GameMaster.current.GetNeighbour(myHex, 1);
        b = h?.building;
        if (b != null) { ApplyAffection(b); b.ApplyAffection(this); }
        h = GameMaster.current.GetNeighbour(myHex, 2);
        b = h?.building;
        if (b != null) { ApplyAffection(b); b.ApplyAffection(this); }
        h = GameMaster.current.GetNeighbour(myHex, 3);
        b = h?.building;
        if (b != null) { ApplyAffection(b); b.ApplyAffection(this); }
        h = GameMaster.current.GetNeighbour(myHex, 4);
        b = h?.building;
        if (b != null) { ApplyAffection(b); b.ApplyAffection(this); }
        h = GameMaster.current.GetNeighbour(myHex, 5);
        b = h?.building;
        if (b != null) { ApplyAffection(b); b.ApplyAffection(this); }
    }
    private void ApplyAffection(Building target)
    {
        if (target == null) return;
        var colony = ColonyController.current;
        switch (type)
        {
            case BuildingType.Housing_2:
                if (target.type == BuildingType.Tree)
                {
                    colony.RegisterBonus(
                        new BuildingBoost(target, this, AffectionType.LifepowerValueBoost, 1f * people / GameConstants.MAX_HOUSING_2_VOLUME)
                        );
                }
                break;
            case BuildingType.Trade_1:
                colony.RegisterBonus(BuildingBoost.GetTrade1Boost(target, this));
                break;
            case BuildingType.Trade_2:
                colony.RegisterBonus(BuildingBoost.GetTrade2Boost(target, this));
                break;
            case BuildingType.Industrial_1:
                switch (target.type) {
                    case BuildingType.Housing_1:
                        colony.RegisterBonus(new BuildingBoost(target, this, AffectionType.ProfitValueBoost, 2f));
                        colony.RegisterBonus(new BuildingBoost(this, target, AffectionType.LifepowerPercentBoost, -0.5f));
                        break;
                    case BuildingType.Housing_2:
                        colony.RegisterBonus(new BuildingBoost(this, target, AffectionType.ProfitPercentBoost, -0.2f));
                        colony.RegisterBonus(new BuildingBoost(this, target, AffectionType.LifepowerPercentBoost, -0.5f));
                        break;
                    case BuildingType.Tree:
                        colony.RegisterBonus(new BuildingBoost(this, target, AffectionType.LifepowerValueBoost, -0.5f));
                        break;
                    case BuildingType.Park:
                    case BuildingType.Farm:
                        colony.RegisterBonus(new BuildingBoost(this, target, AffectionType.LifepowerPercentBoost, -0.5f));
                        break;
                }
                break;
            case BuildingType.Industrial_2:
                switch (target.type)
                {
                    case BuildingType.Industrial_1:
                        colony.RegisterBonus(new BuildingBoost(target, this, AffectionType.ProfitValueBoost, 2f));                        
                        break;
                    case BuildingType.Housing_1:
                        colony.RegisterBonus(new BuildingBoost(this, target, AffectionType.LifepowerPercentBoost, -1f));
                        break;
                    case BuildingType.Housing_2:
                        colony.RegisterBonus(new BuildingBoost(this, target, AffectionType.ProfitPercentBoost, -0.5f));
                        colony.RegisterBonus(new BuildingBoost(this, target, AffectionType.LifepowerPercentBoost, -1f));
                        break;
                    case BuildingType.Tree:
                        colony.RegisterBonus(new BuildingBoost(this, target, AffectionType.LifepowerValueBoost, -1f));
                        break;
                    case BuildingType.Park:
                    case BuildingType.Farm:
                        colony.RegisterBonus(new BuildingBoost(this, target, AffectionType.LifepowerIncreaseStopped, 0f));
                        break;
                }
                break;
            case BuildingType.Generator:
                {
                    if (target.type == BuildingType.Industrial_1 | target.type == BuildingType.Industrial_2)
                    {
                            colony.RegisterBonus(new BuildingBoost(this, target, AffectionType.ProfitPercentBoost, 0.25f));
                            break;
                    }
                    break;
                }
            case BuildingType.Park:
                {
                    if (target.type == BuildingType.Housing_2)
                        colony.RegisterBonus(new BuildingBoost(this, target, AffectionType.LifepowerPercentBoost, 0.15f));
                    else
                    {
                        if (target.type == BuildingType.Tree)
                        {
                            colony.RegisterBonus(new BuildingBoost(this, target, AffectionType.LifepowerValueBoost, 1f));
                        }
                    }
                    break;
                }
            case BuildingType.Farm:
                {
                    if (target.type == BuildingType.Trade_1)
                    {
                        colony.RegisterBonus(new BuildingBoost(this, target, AffectionType.ProfitPercentBoost, 0.15f));
                    }
                    else
                    {
                        if (target.type == BuildingType.Housing_1)
                        {
                            colony.RegisterBonus(new BuildingBoost(this, target, AffectionType.LifepowerPercentBoost, 0.1f));
                        }
                    }
                    break;
                }
        }
    }

    public void ApplyBoost(int key)
    {
        if (destroyed) return;
        BuildingBoost b;
        if (ColonyController.current.boostsList.TryGetValue(key, out b))
        {
            if (b.bonusGiver == this)
            {
                if (buildingBoosts == null) buildingBoosts = new List<int>();
                buildingBoosts.Add(key);
                return;
            }
            else
            {
                if (b.bonusApplier == this)
                {                    
                    if (buildingBoosts == null) buildingBoosts = new List<int>();
                    buildingBoosts.Add(key);
                    RecalculateBoosts();
                }
            }
        }
    }
    public void AnnulateBoost(int key)
    {
        if (destroyed) return;
        buildingBoosts?.Remove(key);
        RecalculateBoosts();
    }
    private void RecalculateBoosts()
    {
        lifepowerSurplus = lifepowerSurplus_basic;
        moneySurplus = moneySurplus_basic;
        peopleSurplus = peopleSurplus_basic;
        if (buildingBoosts!= null && buildingBoosts.Count > 0)
        {
            float money_pc = 0f, lifepower_pc = 0f;
            BuildingBoost b;
            var colony = ColonyController.current;
            var blist = colony.boostsList;
            foreach (var bk in buildingBoosts)
            {
                if (blist.TryGetValue(bk, out b))
                {
                    switch (b.type)
                    {
                        case AffectionType.LifepowerPercentBoost:
                            lifepower_pc += b.value;
                            break;
                        case AffectionType.LifepowerValueBoost:
                            lifepowerSurplus += b.value;
                            break;
                        case AffectionType.ProfitPercentBoost:
                            money_pc += b.value;
                            break;
                        case AffectionType.ProfitValueBoost:
                            moneySurplus += b.value;
                            break;
                    }
                }
                if (lifepower_pc != 0f) lifepowerSurplus *= (1f + lifepower_pc);
                if (money_pc != 0f) moneySurplus *= (1f + money_pc);
                if (lifepowerSurplus < 0f) lifepowerSurplus = 0f;
                if (moneySurplus < 0f) moneySurplus = 0f;
            }
            colony.needRecalculation = true;
        }        
    }

    public void SYSTEM_SetPeopleValue(float f)
    {
        if (people != f)
        {
            people = f;
            if (RecalculateBasicProperties()) RecalculateBoosts();
        }
    }

    public void Demolish()
    {
        if (destroyed) return;
        myHex.ClearBuildingData(this);
        if (model != null) Object.Destroy(model);
        destroyed = true;
        if (enlisted)
        {
            GameMaster.current.endTurnEvent -= EndTurn;
        }
        if (buildingBoosts != null)
        {
            var colony = ColonyController.current;
            foreach (var b in buildingBoosts)
            {
                colony.AnnulateBonus(b);
            }
        }
    }
}
