using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum Icon : byte { MoneyGift, Demolition, MoneyLoss, Farm, Fire, Disease, Rain,
    Industrial_1, Industrial_2, Park, Generator, Housing_1, Housing_2, Trade_1, Trade_2, Lifepower, People, Unknown}

public enum RollResult : byte
{
    Housing_1, Housing_2, Trade_1, Trade_2, Industrial_1, Industrial_2, Gift_1, Gift_2, Gift_3,
    Rain, Fire_1, Fire_2, Fire_3, Disease_1, Disease_2, Disease_3, Loss_1, Loss_2, Loss_3, DemolitionToken, Park, Generator, Farm
}
//dependecies: GetCardRarieties, GetIconType, ColonyController.ApplyRoll

public class CardMaster : MonoBehaviour
{  
    [SerializeField] private Transform card0, card1, card2, xcard;
    [SerializeField] private GameObject cardboard, endButton;
    [SerializeField] private Texture cards_normal, cards_unknown;
    private sbyte cardRotationStatus = 0;
    private byte  rotatingCardStep = 0;
    private bool cardDrawn = false, cardSelected = false;
    private RollResult card0Result, card1Result,card2Result, resultingRoll;
    private const float D_CARD_CHANCE = 0.5f, C_CARD_CHANCE = 0.3f, B_CARD_CHANCE = 0.15f, A_CARD_CHANCE = 0.5f,
        BUILDING_CHANCE = 0.5f, EVENT_CHANCE = 0.2f, TREASURE_CHANCE = 0.2f, LOSS_CHANCE = 0.1f, DEMOLITION_CHANCE = 0.15f;
    private const float CARD_ROTATION_SPEED = 200f;
    private readonly Rect cardFullRect = new Rect(0.1f, 0f, 0.8f, 1f);

    public static CardMaster current;   

    private void Awake()
    {
        current = this;
    }

    public void PrepareForChoice()
    {
        cardboard.SetActive(true);
        card0Result = Roll();
        card1Result = Roll();
        card2Result = Roll();
        DrawCard(card0, card0Result);
        DrawCard(card1, card1Result);
        DrawCard(card2, card2Result);
        cardSelected = false;
    }

    // сделать бонус за комбинации - три одинаковые карточки или определенные наборы
    private RollResult Roll()
    {
        var s = BUILDING_CHANCE + TREASURE_CHANCE;
        float f = s * Random.value;
        if ( f > BUILDING_CHANCE)
        {
            f = Random.value;
            bool improved = Random.value > 0.8f;
            if (f > 0.5f)
            {
                if (f > 0.7f) return improved ? RollResult.Housing_2 : RollResult.Housing_1;
                else return improved ? RollResult.Trade_2 : RollResult.Trade_1;
            }
            else return improved ? RollResult.Industrial_2 : RollResult.Industrial_1;
        }
        else
        {
            s = Random.value * (D_CARD_CHANCE + C_CARD_CHANCE + B_CARD_CHANCE);
            if (s < D_CARD_CHANCE) return RollResult.Gift_1;
            else return s > (D_CARD_CHANCE + C_CARD_CHANCE) ? RollResult.Gift_3 : RollResult.Gift_2;
        }
    }
    private RollResult XRoll()
    {
        var f = Random.value;
        float s = D_CARD_CHANCE + C_CARD_CHANCE;
        if (f >= s)
        {
            f -= s;
            if ( f > B_CARD_CHANCE)
            { // A-CARD
                s = BUILDING_CHANCE + EVENT_CHANCE + LOSS_CHANCE;
                f = Random.value * s;
                if (f > BUILDING_CHANCE)
                {
                    f -= BUILDING_CHANCE;
                    if (f > EVENT_CHANCE)
                    {
                        ColonyController.current.SpendMoney(GameConstants.LOSS_3_COUNT);
                        return RollResult.Loss_3;
                    }
                    else
                    {
                        return Random.value > 0.5f ? RollResult.Fire_3 : RollResult.Disease_3;
                    }
                }
                else
                {
                    f /= BUILDING_CHANCE;
                    if (f <= 0.4f) return RollResult.Park;
                    else
                    {
                        if (f > 0.7f) return RollResult.Farm;
                        else return RollResult.Generator;
                    }
                }
            }
            else
            { // B - CARD
                s = EVENT_CHANCE + TREASURE_CHANCE + DEMOLITION_CHANCE + LOSS_CHANCE;
                f = Random.value * s;
                if (f > EVENT_CHANCE + TREASURE_CHANCE)
                {
                    f -= (EVENT_CHANCE + TREASURE_CHANCE);
                    if (f > DEMOLITION_CHANCE) return RollResult.Loss_2;
                    else return RollResult.DemolitionToken;
                }
                else
                {
                    if (f > EVENT_CHANCE) return RollResult.Gift_3;
                    else
                    {
                        f = Random.value;
                        if (f < 0.4f) return RollResult.Rain;
                        else return f > 0.7f ? RollResult.Fire_2 : RollResult.Disease_2;
                    }
                }
            }
        }
        else
        {
            if (f > D_CARD_CHANCE)
            { // C - CARD                
                s = BUILDING_CHANCE + EVENT_CHANCE;
                f = Random.value * s;
                if (f > s)
                {
                    f -= s;
                    if (f <= TREASURE_CHANCE) return RollResult.Gift_2;
                    else return RollResult.Loss_1;
                }
                else
                {
                    if (f > BUILDING_CHANCE) return Random.value > 0.5f ? RollResult.Fire_1 : RollResult.Disease_1;
                    else
                    {
                        f /= BUILDING_CHANCE;
                        if (f <= 0.4f) return RollResult.Housing_2;
                        else
                        {
                            if (f > 0.7f) return RollResult.Industrial_2;
                            else return RollResult.Trade_2;
                        }
                    }
                }
            }
            else
            {// D - Card
                s = (BUILDING_CHANCE + TREASURE_CHANCE) * Random.value;
                if (s <= BUILDING_CHANCE)
                {
                    f /= BUILDING_CHANCE;
                    if (f <= 0.4f) return RollResult.Housing_1;
                    else
                    {
                        if (f > 0.7f) return RollResult.Industrial_1;
                        else return RollResult.Trade_1;
                    }
                }
                else return RollResult.Gift_1;
            }
        }
    }

    private void XUpdate()
    {
        if (cardRotationStatus == 1)
        {
            Quaternion r0, r1 = Quaternion.LookRotation(cardDrawn ? Vector3.forward : Vector3.right, Vector3.up);          
            switch (rotatingCardStep)
            {
                case 0:
                    {
                        r0 = card0.rotation;
                        r1 = Quaternion.RotateTowards(r0, r1, CARD_ROTATION_SPEED * Time.deltaTime);
                        if (Quaternion.Equals(r0, r1))
                        {
                            if (cardDrawn)
                            {
                                rotatingCardStep++;
                                cardDrawn = false;
                            }
                            else
                            {
                                DrawCard(card0, card0Result);
                                cardDrawn = true;
                            }
                        }
                        card0.rotation = r1;
                        break;
                    }
                case 1:
                    {
                        r0 = card1.rotation;
                        r1 = Quaternion.RotateTowards(r0, r1, CARD_ROTATION_SPEED * Time.deltaTime);
                        if (Quaternion.Equals(r0, r1))
                        {
                            if (cardDrawn)
                            {
                                rotatingCardStep++;
                                cardDrawn = false;
                            }
                            else
                            {
                                DrawCard(card1, card1Result);
                                cardDrawn = true;
                            }
                        }
                        card1.rotation = r1;
                        break;
                    }
                case 2:
                    {
                        r0 = card2.rotation;
                        r1 = Quaternion.RotateTowards(r0, r1, CARD_ROTATION_SPEED * Time.deltaTime);
                        if (Quaternion.Equals(r0, r1))
                        {
                            if (cardDrawn)
                            {
                                rotatingCardStep++;
                                cardDrawn = false;
                            }
                            else
                            {
                                DrawCard(card2, card2Result);
                                cardDrawn = true;
                            }
                        }
                        card2.rotation = r1;
                        break;
                    }
                case 3:
                    {
                        rotatingCardStep = 0;
                        cardRotationStatus = 2;
                        break;
                    }
            }
        }
        else
        {
            if (cardRotationStatus == -1)
            {
                Quaternion r0, r1 = Quaternion.LookRotation(cardDrawn ? Vector3.right : Vector3.forward, Vector3.up);
                switch (rotatingCardStep)
                {
                    case 0:
                        {
                            r0 = card0.rotation;
                            r1 = Quaternion.RotateTowards(r0, r1, CARD_ROTATION_SPEED * Time.deltaTime);
                            if (Quaternion.Equals(r0, r1))
                            {
                                if (cardDrawn)
                                {
                                    rotatingCardStep++;
                                    cardDrawn = false;
                                }
                                else
                                {
                                    DrawCard(card0, card0Result);
                                    cardDrawn = true;
                                }
                            }
                            card0.rotation = r1;
                            break;
                        }
                    case 1:
                        {
                            r0 = card1.rotation;
                            r1 = Quaternion.RotateTowards(r0, r1, CARD_ROTATION_SPEED * Time.deltaTime);
                            if (Quaternion.Equals(r0, r1))
                            {
                                if (cardDrawn)
                                {
                                    rotatingCardStep++;
                                    cardDrawn = false;
                                }
                                else
                                {
                                    DrawCard(card1, card1Result);
                                    cardDrawn = true;
                                }
                            }
                            card1.rotation = r1;
                            break;
                        }
                    case 2:
                        {
                            r0 = card2.rotation;
                            r1 = Quaternion.RotateTowards(r0, r1, CARD_ROTATION_SPEED * Time.deltaTime);
                            if (Quaternion.Equals(r0, r1))
                            {
                                if (!cardDrawn)
                                {
                                    rotatingCardStep--;
                                }
                                else
                                {
                                    UndrawCard(card2);
                                    cardDrawn = false;
                                }
                            }
                            card2.rotation = r1;
                            break;
                        }
                }
                r0 = xcard.rotation;
                r1 = Quaternion.RotateTowards(r0, r1, CARD_ROTATION_SPEED * Time.deltaTime);
                if (Quaternion.Equals(r0, r1))
                {
                    if (!cardDrawn)
                    {
                        rotatingCardStep--;
                    }
                    else
                    {
                        UndrawCard(card2);
                        cardDrawn = false;
                    }
                }
                card2.rotation = r1;
            }
        }
    }

    public void CardClick(int i)
    {
        if (cardSelected) return;
        if (i != 0) UndrawCard(card0); else resultingRoll = card0Result;
        if (i != 1) UndrawCard(card1); else resultingRoll = card1Result;
        if (i != 2) UndrawCard(card2); else resultingRoll = card2Result;
        ColonyController.current.ApplyRoll(resultingRoll);
        endButton.SetActive(true);
        cardSelected = true;
    }
    public void XCardClick()
    {
        if (cardSelected) return;
        if (!ColonyController.current.TrySpendMoney(GameConstants.XCARD_ROLL_COST)) return;
        UndrawCard(card0);
        UndrawCard(card1);
        UndrawCard(card2);
        resultingRoll = XRoll();
        DrawCard(xcard, resultingRoll);
        ColonyController.current.ApplyRoll(resultingRoll);
        endButton.SetActive(true);
        cardSelected = true;
    }
    public void EndButton()
    {
        cardboard.SetActive(false);
        endButton.SetActive(false);
        UndrawCard(card0);
        UndrawCard(card1);
        UndrawCard(card2);
        UndrawCard(xcard);
        UIMaster.current.ReturnControl();        
    }

    private void DrawCard(Transform t, RollResult rr)
    {
        var ri = t.GetComponent<RawImage>();
        ri.texture = cards_normal;
        ri.uvRect = GetCardRarityTextureUV(rr);
        var t2 = t.GetChild(0);
        t2.GetComponent<RawImage>().uvRect = PoolMaster.GetIconUV(GameMaster.GetIconType(rr));
        t2.gameObject.SetActive(true);
    }
    private void UndrawCard(Transform t)
    {
        var ri = t.GetComponent<RawImage>();
        ri.texture = cards_unknown;
        ri.uvRect = cardFullRect;
        t.GetChild(0).gameObject.SetActive(false);
    }
   
    private static Rect GetCardRarityTextureUV(RollResult rr)
    {
        switch(rr)
        {
            case RollResult.Housing_2:
            case RollResult.Trade_2:
            case RollResult.Industrial_2:
            case RollResult.Gift_2:
            case RollResult.Fire_1:
            case RollResult.Disease_1:
            case RollResult.Loss_1:
                return new Rect(0.55f, 0.5f, 0.4f, 0.5f);            
            case RollResult.DemolitionToken:
            case RollResult.Fire_2:
            case RollResult.Disease_2:
            case RollResult.Rain:
            case RollResult.Gift_3:
            case RollResult.Loss_2:
                return new Rect(0.05f, 0f, 0.4f, 0.5f);
            case RollResult.Fire_3:
            case RollResult.Disease_3:
            case RollResult.Generator:
            case RollResult.Farm:
            case RollResult.Park:
            case RollResult.Loss_3:
                return new Rect(0.55f, 0f, 0.4f, 0.5f);       
            default: return new Rect(0.05f, 0.5f, 0.4f, 0.5f);
        }
    }    
}

