using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // 手札にカードを生成
    [SerializeField] Transform playerHandTransform,playerFieldTransform ,enemyHandTransform,enemyFieldTransform;
    [SerializeField] CardController cardPrefab;

    bool isPlayerTurn;

    // シングルトン化(どこからでもアクセスできるようにする)
    public static GameManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }
    void Start()
    {
        StartGame();
    }

    void StartGame()
    {
        SettingInitHand();
        isPlayerTurn = true;
        TurnCalc();
    }

    void SettingInitHand()
    {
        //　かーどをそれぞれに3枚配る
        for (int i = 0; i < 3; i++)
        {
            CreateCard(playerHandTransform);
            CreateCard(enemyHandTransform);
        }
    }

    void TurnCalc()
    {
        if (isPlayerTurn)
        {
            PlayerTurn();
        }
        else
        {
            EnemyTurn();
        }
    }

    
    public void ChangeTurn()
    {
        isPlayerTurn = !isPlayerTurn;
        if (isPlayerTurn)
        {
            CreateCard(playerHandTransform);
        }
        else
        {
            CreateCard(enemyHandTransform);
        }
        TurnCalc();
    }
    void PlayerTurn()
    {
        Debug.Log("Player Turn");
        // フィールドのカードを攻撃可能にする
        CardController[] playerFieldCardList = playerFieldTransform.GetComponentsInChildren<CardController>();
        foreach (CardController card in playerFieldCardList)
        {
            // cardを攻撃可能にする
            card.SetCanAttack(true);
        }
    }

    void EnemyTurn()
    {
        Debug.Log("Enemy Turn");
        /* 場にカードを出す */
        //手札のカードリストを取得
        CardController[] handCardList = enemyHandTransform.GetComponentsInChildren<CardController>();
        
        //場に出すカードを選択
        CardController enemyCard = handCardList[0];
        
        //カードを移動
        enemyCard.movement.SetCardTransform(enemyFieldTransform);


        /*　攻撃　*/
        //　フィールドのカードリストを取得
        CardController[] fieldCardList = enemyFieldTransform.GetComponentsInChildren<CardController>();

        //　攻撃可能カードを取得
        CardController[] enemyCanAttackCardList = Array.FindAll(fieldCardList, card => card.model.canAttack); //検索： Array.FindAll                                                                                                             //　defenderカードを選択(Playerフィールドから選択)
        CardController[] playerFieldCardList = playerFieldTransform.GetComponentsInChildren<CardController>();

        if (enemyCanAttackCardList.Length > 0 && playerFieldCardList.Length > 0)
        {
            // attackerカードをを選択
            CardController attacker = enemyCanAttackCardList[0];
            
            // defenderカードをを選択
            CardController defender = playerFieldCardList[0];

            //attacker　と defender　カードを戦わせる
            CardsBattle(attacker, defender);
        }
        

        ChangeTurn();
    }

    public void CardsBattle(CardController attacker, CardController defender)
    {
        Debug.Log("CardsBattle");
        Debug.Log("attacker HP:"+ attacker.model.hp);
        Debug.Log("defender HP:"+ defender.model.hp);


        attacker.Attack(defender);
        defender.Attack(attacker);
        Debug.Log("attacker HP:" + attacker.model.hp);
        Debug.Log("defender HP:" + defender.model.hp);
        attacker.CheckAlive();
        defender.CheckAlive();
    }
    void CreateCard(Transform hand)
    {
        //　カードの生成とデータの受け渡し
       CardController card = Instantiate(cardPrefab, hand, false);
        card.Init(1);
    }
}
