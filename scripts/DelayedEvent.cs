
public enum DelayedEventType : byte { ChangeFireLevel }
//dependency: colonyCOntroller.EndTurn
public struct DelayedEvent
{
    public readonly DelayedEventType type;
    public byte turnsLeft;
    public readonly byte value;
    public static bool operator ==(DelayedEvent A, DelayedEvent B)
    {
        if (ReferenceEquals(A, null))
        {
            return ReferenceEquals(B, null);
        }
        return A.Equals(B);
    }
    public static bool operator !=(DelayedEvent A, DelayedEvent B)
    {
        return !(A == B);
    }
    public override int GetHashCode()
    {
        return -366827 + turnsLeft * 10 + value * 100;
    }
    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType()) return false;
        var info = (DelayedEvent)obj;
        if (type == info.type && turnsLeft == info.turnsLeft && value == info.value) return true;
        else return false;
    }

    public DelayedEvent(DelayedEventType i_type, byte i_length, byte i_val)
    {
        type = i_type;
        turnsLeft = i_length;
        value = i_val;
    } 

    public void EndTurn()
    {
        if (turnsLeft > 0) turnsLeft--;
    }
}
