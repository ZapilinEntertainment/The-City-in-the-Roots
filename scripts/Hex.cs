using UnityEngine;
//-------------
public struct HexPos
{
    public byte ring, index;
    public HexPos(byte i_ring, byte i_pos)
    {
        ring = i_ring;
        index = i_pos;
    }
    public static bool operator ==(HexPos A, HexPos B)
    {
        if (ReferenceEquals(A, null))
        {
            return ReferenceEquals(B, null);
        }
        return A.Equals(B);
    }
    public static bool operator !=(HexPos A, HexPos B)
    {
        return !(A == B);
    }
    public override int GetHashCode()
    {
        return 336 + ring + index * 40;
    }
    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType()) return false;
        var info = (HexPos)obj;
        if (ring == info.ring && index == info.index) return true;
        else return false;
    }
}
//---------------
public class Hex : MonoBehaviour
{
    public HexEvent ongoingEvent { get; private set; }
    public Building building { get; private set; }
    public HexPos position { get; private set; }
    public float height { get; private set; }

    public const float R = 1f, r = 0.866025f * R;   
    public static Vector3 GetWorldPosition(int ring, int index)
    {
        switch (ring)
        {
            case 1: return Quaternion.AngleAxis(30f + 60f * index, Vector3.up) * Vector3.forward * 2* r;
            case 2:
                {
                    if (index % 2 == 0 ) return Quaternion.AngleAxis(30f * index, Vector3.up) * Vector3.forward * R * 3f;
                    else return Quaternion.AngleAxis(30f * index, Vector3.up) * Vector3.forward * r * 4f;
                }
            case 3:
                {
                    var x = index;
                    if (x % 3 == 0) return Quaternion.AngleAxis(20f * index, Vector3.up) * (new Vector3(r, 0f, 4.5f * R));
                    else
                    {
                        if ((x -1) % 3 == 0) return Quaternion.AngleAxis(20f * (index-1), Vector3.up) * new Vector3(3 * r, 0, 4.5f * R);
                            //return new Vector3(3 * r, 0, 4.5f * R);
                        else return Quaternion.AngleAxis(20f * (index-2), Vector3.up) * new Vector3(4 * r, 0, 3 * R);
                    }
                }
            case 4:
                {
                    var x = index;
                    if (x % 4 == 0) return Quaternion.AngleAxis(60f * (index / 4), Vector3.up) * Vector3.forward * 6f * R;
                    else
                    {
                        if ((x - 2) % 4 == 0) return Quaternion.AngleAxis(60f * ((index - 2) / 4), Vector3.up) * new Vector3(4f * r, 0f, 6f * R);
                        else
                        {
                            if ((x - 1) % 4 == 0) return Quaternion.AngleAxis(60f * ((index - 1) / 4), Vector3.up) * new Vector3(2f * r, 0f, 6f * R);
                            else return Quaternion.AngleAxis(60f * ((index - 3) / 4), Vector3.up) * new Vector3(5f * r, 0f, 4.5f * R);
                        }
                    }
                }
            case 5:
                {
                    switch (index % 5)
                    {
                        case 1: return Quaternion.AngleAxis(60f * ((index - 1) / 5), Vector3.up) * new Vector3(3f * r, 0f, 7.5f * R);
                        case 2: return Quaternion.AngleAxis(60f * ((index - 2) / 5), Vector3.up) * new Vector3(5f * r, 0f, 7.5f * R);
                        case 3: return Quaternion.AngleAxis(60f * ((index - 3) / 5), Vector3.up) * new Vector3(6f * r, 0f, 6f * R);
                        case 4: return Quaternion.AngleAxis(60f * ((index - 4) / 5), Vector3.up) * new Vector3(7f * r, 0f, 4.5f * R);
                        default: return Quaternion.AngleAxis(60f * ((index) / 5), Vector3.up) * new Vector3(r, 0f, 7.5f * R);
                    }
                }
            default: return Vector3.zero;
        }
    }

    public void Initialize(HexPos hpos, float i_height) {
        position = hpos;
        height = i_height;
        ColonyController.current.AddHex(this);
    }

    public Vector3 GetSurfacePosition()
    {
        return transform.position + Vector3.up * height * 0.5f;
    }

    public void AddBuilding(Building b)
    {
        building = b;
        if (ongoingEvent != null) ApplyOngoingEvent();
        b.InitializeModel(true);
        b.EnlistToColony();
    }
    public void AddEvent(HexEvent ge)
    {
        ongoingEvent = ge;
        ApplyOngoingEvent();
    }
    private void ApplyOngoingEvent()
    {
        if (building != null)
        {
            switch (ongoingEvent.type)
            {
                case EventType.Disease:
                    {
                        switch (ongoingEvent.level)
                        {
                            case 1: ColonyController.current.RegisterBonus(new BuildingBoost(building, building, AffectionType.PeopleSurplusPercentBoost, -0.5f)); break;
                            case 2: ColonyController.current.RegisterBonus(new BuildingBoost(building, building, AffectionType.PeopleIncreaseStopped, 0f)); break;
                            case 3:
                                ColonyController.current.RegisterBonus(new BuildingBoost(building, building, AffectionType.PeopleSurplusPercentBoost, -2f));
                                break;
                        }
                        break;
                    }
                case EventType.Fire:
                    ColonyController.current.RegisterBonus(new BuildingBoost(building, building, AffectionType.OnFire, 0f));
                    break;
            }
        }
    }
    public void EndEvent(HexEvent ge)
    {
        if (ge == ongoingEvent)
        {
            if (ongoingEvent.affectionKey != 0)  ColonyController.current.AnnulateBonus(ge.affectionKey);
            ongoingEvent = null;
        }
    }
    public void ClearBuildingData(Building b)
    {
        if (building == b) building = null;
    }
}
