using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Constructor : MonoBehaviour
{
    private static GameObject hexPref;

    public const int RINGS_COUNT_1 = 6, RINGS_COUNT_2 = 12, RINGS_COUNT_3 = 18, RINGS_COUNT_4 = 24, RINGS_COUNT_5 = 30;

    public static void CreateRing(byte index)
    {       
        if (hexPref == null) hexPref = Resources.Load<GameObject>("prefs/hex");
        if (index > 0 & index < 6)
        {
            byte i = 0;
            Transform t;
            GameObject g;
            HexLift hl;
            Vector3 pos;
            switch (index)
            {
                case 1:
                    {
                        pos = Hex.GetWorldPosition(0, 0);
                        g = Instantiate(hexPref, Vector3.zero, Quaternion.identity);
                        t = g.transform;
                        var c = t.GetChild(0).GetComponent<Collider>();
                        if (c != null)
                        {
                            c.enabled = true;
                            c.tag = GameConstants.HEX_COLLIDER_TAG;
                        }
                        else Debug.Log("error - start hex collider uninitialized");
                        g.name = "start hex";
                        t.localScale = new Vector3(1f, 5f, 1f);
                        var h = g.AddComponent<Hex>();
                        h.Initialize(new HexPos(0, 0), 5f);
                        h.AddBuilding(new Building(BuildingType.Tree, h));
                        //
                        for (; i < RINGS_COUNT_1; i++)
                        {
                            pos = Hex.GetWorldPosition(index, i);
                            g =Instantiate(hexPref, new Vector3(pos.x, -5f + Random.value * 0.5f, pos.z) , Quaternion.identity);
                            t = g.transform;
                            g.name = index.ToString() + ' ' + i.ToString();
                            t.localScale = new Vector3(1f, 4f, 1f);
                            hl = g.GetComponent<HexLift>();
                            hl.height = pos.y + Random.value * 0.4f;
                            hl.position = new HexPos(index, i);
                        }
                        break;
                    }
                case 2:
                    {
                        for (; i < RINGS_COUNT_2; i++)
                        {
                            pos = Hex.GetWorldPosition(index, i);
                            g = Instantiate(hexPref, new Vector3(pos.x, -5f + Random.value * 0.5f, pos.z), Quaternion.identity);
                            t = g.transform;
                            g.name = index.ToString() + ' ' + i.ToString();
                            t.localScale = new Vector3(1f, 3.2f, 1f);
                            hl = g.GetComponent<HexLift>();
                            hl.height = pos.y + Random.value * 0.4f;
                            hl.position = new HexPos(index, i);
                        }
                        break;
                    }
                case 3:
                    {
                        for (; i < RINGS_COUNT_3; i++)
                        {
                            pos = Hex.GetWorldPosition(index, i);
                            g = Instantiate(hexPref, new Vector3(pos.x, -5f + Random.value * 0.5f, pos.z), Quaternion.identity);
                            t = g.transform;
                            g.name = index.ToString() + ' ' + i.ToString();
                            t.localScale = new Vector3(1f, 2f, 1f);
                            hl = g.GetComponent<HexLift>();
                            hl.height = pos.y + Random.value * 0.4f;
                            hl.position = new HexPos(index, i);
                        }
                        break;
                    }
                case 4:
                    {
                        for (; i < RINGS_COUNT_4; i++)
                        {
                            pos = Hex.GetWorldPosition(index, i);
                            g = Instantiate(hexPref, new Vector3(pos.x, -5f + Random.value * 0.5f, pos.z), Quaternion.identity);
                            t = g.transform;
                            g.name = index.ToString() + ' ' + i.ToString();
                            t.localScale = new Vector3(1f, 1.4f, 1f);
                            hl = g.GetComponent<HexLift>();
                            hl.height = pos.y + Random.value * 0.4f;
                            hl.position = new HexPos(index, i);
                        }
                        break;
                    }
                case 5:
                    {
                        for (; i < RINGS_COUNT_5; i++)
                        {
                            pos = Hex.GetWorldPosition(index, i);
                            g = Instantiate(hexPref, new Vector3(pos.x, -5f + Random.value * 0.5f, pos.z), Quaternion.identity);
                            t = g.transform;
                            g.name = index.ToString() + ' ' + i.ToString();
                            t.localScale = new Vector3(1f, 1f, 1f);
                            hl = g.GetComponent<HexLift>();
                            hl.height = pos.y + Random.value * 0.4f;
                            hl.position = new HexPos(index, i);
                        }
                        break;
                    }
            }
        }
    }
}
