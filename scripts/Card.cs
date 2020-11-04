using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum CardStatus : byte { CardBack, RotatingToOpen, Open, RotatingToClose }
public class Card : MonoBehaviour
{
    [SerializeField] private Transform cardBase;
    [SerializeField] private RawImage icon;
    private RollResult result;
    public CardStatus status { get; private set; }
    private const float CARD_ROTATION_SPEED = 200f;
    private readonly Rect cardFullRect = new Rect(0.1f, 0f, 0.8f, 1f);

    public void Open(RollResult r)
    {
        
    }
}
