﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InnRestButton : MonoBehaviour
{
    public void RestParty()
    {
        if (PlayerParty.gold < 5)
        {
            NotificationPopupSystem.PopupText("Not enough gold");
            return;
        }
        PlayerParty.gold -= 5;
        foreach (CharacterSheet c in PlayerParty.partyMembers)
        {
            c.Heal(10);
        }
    }
}
