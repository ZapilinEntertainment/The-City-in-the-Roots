using UnityEngine;
public enum EventType : byte { Disease, Fire} 
//dependency: colony.StartGameEvent, Hex.AddEvent
public class HexEvent 
{
    public readonly Hex myHex;
    public readonly EventType type;
    public byte level { get; private set; }
    public byte turnsLeft { get; private set; }
    private bool subscribedToUpdate = false;
    public int affectionKey = 0;
    private GameObject effectMarker;
    public static bool operator ==(HexEvent A, HexEvent B)
    {
        if (ReferenceEquals(A, null))
        {
            return ReferenceEquals(B, null);
        }
        return A.Equals(B);
    }
    public static bool operator !=(HexEvent A, HexEvent B)
    {
        return !(A == B);
    }
    public override int GetHashCode()
    {
        return level + turnsLeft + (int)type + (subscribedToUpdate ? 1000 : 1);
    }
    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType()) return false;
        var other = (HexEvent)obj;
        if (type == other.type && myHex == other.myHex && level == other.level && turnsLeft == other.turnsLeft ) return true;
        else return false;
    }

    public HexEvent(Hex i_hex, EventType i_type, byte i_lvl)
    {
        myHex = i_hex;
        type = i_type;
        level = i_lvl;
        switch (type)
        {
            case EventType.Fire: turnsLeft = GameConstants.FIRE_EVENT_LENGTH; break;
            case EventType.Disease: turnsLeft = GameConstants.ILLNESS_EVENT_LENGTH; break;
        }
        SubscribeToUpdate();
        PrepareEffectMarker(myHex.GetSurfacePosition());
    }

    public void SubscribeToUpdate()
    {
        if (!subscribedToUpdate)
        {
            GameMaster.current.endTurnEvent += this.EndTurn;
            subscribedToUpdate = true;
        }
    }

    private void EndTurn()
    {
        if (turnsLeft > 0) turnsLeft--;
        if (turnsLeft == 0)
        {
            myHex.EndEvent(this);
            GameMaster.current.endTurnEvent -= this.EndTurn;
            subscribedToUpdate = false;
        }
    }

    private void PrepareEffectMarker(Vector3 pos)
    {
        if (effectMarker == null)
        {
            switch (type)
            {
                case EventType.Fire:
                    effectMarker = GameObject.Instantiate(Resources.Load<GameObject>("prefs/fire"), pos, Quaternion.identity);
                    break;
                case EventType.Disease:
                    effectMarker = GameObject.Instantiate(Resources.Load<GameObject>("prefs/disease"), pos, Quaternion.identity);
                    break;
            }
        }
    }
}
