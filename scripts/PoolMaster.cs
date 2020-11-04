using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolMaster : MonoBehaviour
{
    public Material baseMaterial { get; private set; }
    public static PoolMaster current;
    // Start is called before the first frame update
    public void Prepare()
    {
        if (current != null) return;
        current = this;
        baseMaterial = Resources.Load<Material>("Materials/Base");
    }

    public static Rect GetIconUV(Icon icon)
    {
        const float p = 0.125f;
        Vector2 v = Vector2.zero;
        switch (icon)
        {
            case Icon.Demolition: v = Vector2.right * p; break;
            case Icon.MoneyLoss: v = Vector2.right * p; break;
            case Icon.Farm: v = Vector2.up * p; break;
            case Icon.Fire: v = Vector2.one * p; break;
            case Icon.Disease: v = new Vector2(2f * p, p); break;
            case Icon.Rain: v = new Vector2(3f * p, p); break;
            case Icon.Industrial_1: v = Vector2.up * 2f * p; break;
            case Icon.Industrial_2: v = new Vector2(p, 2f * p); break;
            case Icon.Park: v = Vector2.one * 2f * p; break;
            case Icon.Generator: v = new Vector2(3f * p, 2f * p); break;
            case Icon.Housing_1: v = new Vector2(0f, 3f * p); break;
            case Icon.Housing_2: v = new Vector2(p, 3f * p); break;
            case Icon.Trade_1: v = new Vector2(2f * p, 3f * p); break;
            case Icon.Trade_2: v = Vector2.one * 3f * p; break;
            case Icon.Lifepower: v = Vector2.right * 4f * p; break;
            case Icon.People: v = Vector2.right * 5f * p;break;
            case Icon.MoneyGift: v = Vector2.right * 3f * p; break;
        }
        return new Rect(v.x, v.y, p, p);
    }
}
