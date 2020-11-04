using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIMaster : MonoBehaviour
{
    [SerializeField] private GameObject tokenPanelsHost, endTurnButton, buildingPanel, buildingUpgradeButton;
    [SerializeField] private GameObject[] tokenPanels, buildingInfoLines;
    [SerializeField] private Text infopanel;
    public static UIMaster current;
    private bool userControlEnabled = true;
    private Hex selectedHex;
    private ColonyToken[] tokensArray;

    private void Awake()
    {
        current = this;
        tokensArray = new ColonyToken[10];
        for (int i = 0; i < 10; i++) tokensArray[i] = ColonyToken.Total;
    }

    public void Initialize()
    {
        
    }

    public void Raycasting()
    {
        if (!userControlEnabled) return;
        Vector2 mpos = Input.mousePosition;
        RaycastHit rh;
        if (Physics.Raycast(CameraController.cam.ScreenPointToRay(Input.mousePosition), out rh))
        {
            GameObject collided = rh.collider.gameObject;
            if (collided.tag == GameConstants.HEX_COLLIDER_TAG) SelectHex(collided.transform.parent?.GetComponent<Hex>());
        }
    }
    
    public void EndTurn()
    {
        TakeControl();
        CardMaster.current.PrepareForChoice();
        RefreshInfopanel();
        if (selectedHex != null) RefreshHexInfo();
    }

    public void SelectHex(Hex h)
    {
        selectedHex = h;
        if (selectedHex == null) return;
        CameraController.main.SetLookPoint(h.GetSurfacePosition());
        RefreshHexInfo();
    }
    private void RefreshHexInfo()
    {
        PrepareTokensPanel();
        if (selectedHex.building != null) SelectBuilding(selectedHex.building);
        else
        {
            if (buildingPanel.activeSelf) buildingPanel.SetActive(false);
        }
    }
    public void SelectBuilding(Building b)
    {
        buildingPanel.SetActive(false);
        buildingUpgradeButton.SetActive(false);
        sbyte result = b.isLifepowerBoosted;
        int i = 0;
        Transform t;
        Text tx;
        float f = b.lifepowerSurplus;
        bool drawLine = result != 0 | b.lifepowerSurplus != 0;
        if (drawLine)
        {
            t = buildingInfoLines[i].transform;
            t.GetChild(0).GetComponent<RawImage>().uvRect = PoolMaster.GetIconUV(Icon.Lifepower);
            tx = t.GetChild(1).GetComponent<Text>();
            if (f !=  0)
            {
                if (f > 0) tx.text = '+' + string.Format("{0:0.##}", f);
                else tx.text = '-' + string.Format("{0:0.##}", f);
            }
            else tx.text = string.Format("{0:0.##}", f);
            switch (result)
            {
                case 1:                    
                    tx.color = Color.green;
                    break;
                case -1:
                    tx.color = Color.red;
                    break;
                default:
                    tx.color = Color.white;
                    break;
            }
        }
        //
        result = b.isMoneyBoosted;
        drawLine = result != 0 | b.moneySurplus != 0;
        if (drawLine)
        {
            t = buildingInfoLines[i].transform;
            t.GetChild(0).GetComponent<RawImage>().uvRect = PoolMaster.GetIconUV(Icon.MoneyGift);
            tx = t.GetChild(1).GetComponent<Text>();
            f = b.moneySurplus;
            if (f != 0)
            {
                if (f > 0) tx.text = '+' + string.Format("{0:0.##}", f);
                else tx.text = '-' + string.Format("{0:0.##}", f);
            }
            else tx.text = string.Format("{0:0.##}", f);
            switch (result)
            {
                case 1:
                    tx.color = Color.green;
                    break;
                case -1:
                    tx.color = Color.red;
                    break;
                default:
                    tx.color = Color.white;
                    break;
            }
            i++;
        }
        //
        result = b.isPeopleBoosted;
        drawLine = result != 0 | b.peopleSurplus != 0;
        if (drawLine)
        {
            t = buildingInfoLines[i].transform;
            t.GetChild(0).GetComponent<RawImage>().uvRect = PoolMaster.GetIconUV(Icon.People);
            tx = t.GetChild(1).GetComponent<Text>();
            f = b.peopleSurplus;
            tx.text = ((int)b.people).ToString();
            if (f != 0)
            {
                if (f > 0) tx.text += " (+" + string.Format("{0:0.##}", f) + ')';
                else tx.text += " (-" + string.Format("{0:0.##}", f) + ')';
            }
            else tx.text += " (" + string.Format("{0:0.##}", f) + ')';
            switch (result)
            {
                case 1:
                    tx.color = Color.green;
                    break;
                case -1:
                    tx.color = Color.red;
                    break;
                default:
                    tx.color = Color.white;
                    break;
            }
            i++;
        }
        //
        var bt = b.type;
        bool canBeUpgraded = bt == BuildingType.Housing_1 | bt == BuildingType.Trade_1 | bt == BuildingType.Industrial_1 ;
        if (i != 0 | canBeUpgraded)
        {
            switch (i)
            {
                case 0:
                    buildingInfoLines[0].SetActive(false);
                    buildingInfoLines[1].SetActive(false);
                    buildingInfoLines[2].SetActive(false);
                    break;
                case 1:
                    buildingInfoLines[0].SetActive(true);
                    buildingInfoLines[1].SetActive(false);
                    buildingInfoLines[2].SetActive(false);
                    break;
                case 2:
                    buildingInfoLines[0].SetActive(true);
                    buildingInfoLines[1].SetActive(true);
                    buildingInfoLines[2].SetActive(false);
                    break;
                case 3:
                    buildingInfoLines[0].SetActive(true);
                    buildingInfoLines[1].SetActive(true);
                    buildingInfoLines[2].SetActive(true);
                    break;
            }
            buildingUpgradeButton.SetActive(canBeUpgraded);
            buildingPanel.SetActive(true);
        }
    }

    public void TakeControl()
    {
        userControlEnabled = false;
        /*
        if (selectedHex != null)
        {
            selectedHex = null;
            if (buildingPanel.activeSelf) buildingPanel.SetActive(false);
        }
        */
        tokenPanelsHost.SetActive(false);
        endTurnButton.SetActive(false);
    }
    public void ReturnControl()
    {
        userControlEnabled = true;
        PrepareTokensPanel();
        endTurnButton.SetActive(true);
    }

    public void PrepareTokensPanel()
    {
        if (!userControlEnabled) return;
        var tokens = ColonyController.current.tokens;
        var tlist = new List<ColonyToken>();
        if (tokens[(int)ColonyToken.Housing_1] > 0) tlist.Add(ColonyToken.Housing_1);
        if (tokens[(int)ColonyToken.Housing_2] > 0) tlist.Add(ColonyToken.Housing_2);
        if (tokens[(int)ColonyToken.Trade_1] > 0) tlist.Add(ColonyToken.Trade_1);
        if (tokens[(int)ColonyToken.Trade_2] > 0) tlist.Add(ColonyToken.Trade_2);
        if (tokens[(int)ColonyToken.Industrial_1] > 0) tlist.Add(ColonyToken.Industrial_1);
        if (tokens[(int)ColonyToken.Industrial_2] > 0) tlist.Add(ColonyToken.Industrial_2);
        if (tokens[(int)ColonyToken.Park] > 0) tlist.Add(ColonyToken.Park);
        if (tokens[(int)ColonyToken.Farm] > 0) tlist.Add(ColonyToken.Farm);
        if (tokens[(int)ColonyToken.Generator] > 0) tlist.Add(ColonyToken.Generator);
        if (tokens[(int)ColonyToken.Demolition] > 0) tlist.Add(ColonyToken.Demolition);
        int count = tlist.Count;
        if (count == 0) tokenPanelsHost.SetActive(false);
        else
        {
            int i = 0;
            if (count > 0)
            {
                Transform tp;
                bool buildingMode = false;
                if (selectedHex != null && selectedHex.building == null) buildingMode = true;
                ColonyToken ct;
                while (i < count)
                {
                    tp = tokenPanels[i].transform;
                    ct = tlist[i];
                    tp.GetChild(0).gameObject.SetActive( ct == ColonyToken.Demolition ? !buildingMode : buildingMode);
                    tp.GetChild(1).GetComponent<RawImage>().uvRect = PoolMaster.GetIconUV(GameMaster.GetIconType(ct));
                    tp.GetChild(2).GetComponent<Text>().text = tokens[(int)tlist[i]].ToString();
                    if (!tokenPanels[i].activeSelf) tokenPanels[i].SetActive(true);
                    tokensArray[i] = ct;
                    i++;
                }
            }
            if (i < 10)
            {
                while (i < 10)
                {
                    if (tokenPanels[i].activeSelf) tokenPanels[i].SetActive(false);
                    i++;
                }
            }
            if (!tokenPanelsHost.activeSelf) tokenPanelsHost.SetActive(true);
        }        
    }
    public void RefreshInfopanel()
    {
        var colony = ColonyController.current;
        var s = Localization.GetLabel(Localization.Label.Lifepower) + ": " + string.Format("{0:0.##}", colony.lifepower) +
            " / " + colony.nextLifepowerValue.ToString();
        float f = colony.lifepowerSurplus;
        if ( f != 0)
        {
            if (f > 0) s += " (+";
            else s += " (-";
            s += string.Format("{0:0.##}", colony.lifepowerSurplus) + ')';
        }
        s += "\n";
        s += Localization.GetLabel(Localization.Label.Money) + ": " + string.Format("{0:0.##}", colony.money);
        f = colony.moneySurplus;
        if (f != 0)
        {
            if (f > 0) s += " (+";
            else s += " (-";
            s += string.Format("{0:0.##}", colony.moneySurplus) + ')';
        }
        s += "\n";
        s += Localization.GetLabel(Localization.Label.People) + ": " + string.Format("{0:0.##}", colony.people) + " \n";
        s += "\n Turn: " + GameMaster.current.turnNumber.ToString();
        infopanel.text = s;
    }

    public void EndTurnButton()
    {
        GameMaster.current.EndTurn();
    }
    public void BuildButton(int i)
    {
        if (selectedHex == null) return;
        var ct = tokensArray[i];
        if (ct != ColonyToken.Demolition)
        {
            if (selectedHex.building != null)
            {
                PrepareTokensPanel();
                return;
            }
        }
        else
        {
            if (selectedHex.building == null)
            {
                PrepareTokensPanel();
                return;
            }
        }
        if (tokensArray[i] != ColonyToken.Total && ColonyController.current.TrySpentToken(ct))
        {
            BuildingType bt = BuildingType.Housing_1;
            switch (tokensArray[i])
            {
                case ColonyToken.Housing_2: bt = BuildingType.Housing_2; break;
                case ColonyToken.Trade_1: bt = BuildingType.Trade_1; break;
                case ColonyToken.Trade_2: bt = BuildingType.Trade_2; break;
                case ColonyToken.Industrial_1: bt = BuildingType.Industrial_1; break;
                case ColonyToken.Industrial_2: bt = BuildingType.Industrial_2; break;
                case ColonyToken.Generator: bt = BuildingType.Generator; break;
                case ColonyToken.Park: bt = BuildingType.Park; break;
                case ColonyToken.Farm: bt = BuildingType.Farm; break;
            }
            selectedHex.AddBuilding(new Building(bt, selectedHex));
            PrepareTokensPanel();
            EndTurnButton();
        }
    }
    public void UpgradeButton()
    {
        if (selectedHex != null && selectedHex.building != null)
        {
            var b = selectedHex.building;           
            switch (b.type)
            {
                case BuildingType.Housing_1:
                    if (ColonyController.current.TrySpendMoney(100f))
                    {
                        var savedPeopleVal = b.people;
                        b.Demolish();
                        var nb = new Building(BuildingType.Housing_2, selectedHex);
                        selectedHex.AddBuilding(nb);
                        nb.SYSTEM_SetPeopleValue(savedPeopleVal);
                        SelectBuilding(selectedHex.building);
                    }
                    break;
                case BuildingType.Trade_1:
                    if (ColonyController.current.TrySpendMoney(150f))
                    {
                        b.Demolish();
                        selectedHex.AddBuilding(new Building(BuildingType.Trade_2, selectedHex));
                        SelectBuilding(selectedHex.building);
                    }
                    break;
                case BuildingType.Industrial_1:
                    if (ColonyController.current.TrySpendMoney(200f))
                    {
                        b.Demolish();
                        selectedHex.AddBuilding(new Building(BuildingType.Industrial_2, selectedHex));
                        SelectBuilding(selectedHex.building);
                    }
                    break;
            }
        }
        else
        {
            buildingUpgradeButton.SetActive(false);
        }
    }
}
