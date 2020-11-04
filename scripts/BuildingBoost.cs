public enum AffectionType : byte {
    ProfitValueBoost, ProfitPercentBoost, LifepowerValueBoost, LifepowerPercentBoost,
    LifepowerIncreaseStopped, PeopleSurplusPercentBoost, PeopleIncreaseStopped, OnFire
}
//dependency: colonyController - recalculation
public class BuildingBoost
{
    public readonly AffectionType type;
    public float value;
    public readonly Building bonusGiver, bonusApplier;

    public BuildingBoost(Building i_giver, Building i_applier, AffectionType i_type, float i_val)
    {
        type = i_type;
        value = i_val;
        bonusGiver = i_giver;
        bonusApplier = i_applier;
    }

    public static BuildingBoost GetTrade1Boost(Building giver, Building applier)
    {
        float val = 0f;
        switch (giver.type)
        {
            case BuildingType.Housing_1: val = 2f; break;
            case BuildingType.Industrial_1: val = 5f; break;
            case BuildingType.Trade_2: val = 1f; break;
        }
        return new BuildingBoost(giver, applier, AffectionType.ProfitValueBoost, val);
    }
    public static BuildingBoost GetTrade2Boost(Building giver, Building applier)
    {
        float val = 0f;
        switch (giver.type)
        {
            case BuildingType.Housing_2: val = 5f; break;
            case BuildingType.Industrial_2: val = 7f; break;
            case BuildingType.Trade_1: val = 1f; break;
        }
        return new BuildingBoost(giver, applier, AffectionType.ProfitValueBoost, val);
    }
}
