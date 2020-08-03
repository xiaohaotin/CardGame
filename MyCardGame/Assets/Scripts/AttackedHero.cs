using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

//　攻撃される側
public class AttackedHero : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        /*　攻撃　*/

        // attackerカードをを選択
        CardController attacker = eventData.pointerDrag.GetComponent<CardController>();

        
        if (attacker == null)
        {
            return;
        }

        if (attacker.model.canAttack)
        {
            //attackerがHeroに攻撃する
            GameManager.instance.AttackToHero(attacker,true);
        }


    }
}
