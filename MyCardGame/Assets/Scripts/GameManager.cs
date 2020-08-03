using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField] GameObject resultPanel;
    [SerializeField] Text resultText;

    // 手札にカードを生成
    [SerializeField] Transform playerHandTransform,playerFieldTransform ,enemyHandTransform,enemyFieldTransform;
    [SerializeField] CardController cardPrefab;

    bool isPlayerTurn;

    List<int> playerDeck = new List<int>() { 3, 1, 2, 2 , 3},
              enemyDeck = new List<int>() { 2, 1, 3, 1 , 3};

    [SerializeField] Text playerHeroHpText;
    [SerializeField] Text enemyHeroHpText;

    int playerHeroHp;
    int enemyHeroHp;

    [SerializeField] Text playerManaCostText;
    [SerializeField] Text enemyManaCostText;
    public int playerManaCost;
    public int enemyManaCost;

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
        resultPanel.SetActive(false);
        playerHeroHp = 1;
        playerManaCost = 1;
        enemyHeroHp = 1;
        enemyManaCost = 1;
        ShowHeroHP();
        ShowManaCost();
        SettingInitHand();
        isPlayerTurn = true;
        TurnCalc();
    }

    void ShowManaCost()
    {
        playerManaCostText.text = playerManaCost.ToString();
        enemyManaCostText.text = enemyManaCost.ToString();
    }

    public void ReduceManaCost(int cost, bool isPlayerCard)
    {
        if (isPlayerCard)
        {
            playerManaCost -= cost;
        }
        else
        {
            enemyManaCost -= cost;
        }
        ShowManaCost();
    }
    public void Restart()
    {
        // handとFieldのカードを削除
        foreach (Transform card in playerHandTransform)
        {
            Destroy(card.gameObject);
        }
        foreach (Transform card in playerFieldTransform)
        {
            Destroy(card.gameObject);
        }
        foreach (Transform card in enemyHandTransform)
        {
            Destroy(card.gameObject);
        }
        foreach (Transform card in enemyFieldTransform)
        {
            Destroy(card.gameObject);
        }
        // デッキを生成
        playerDeck = new List<int>() { 3, 1, 2, 2, 3 };
        enemyDeck = new List<int>() { 2, 1, 3, 1, 3 };
        StartGame();
    }

    void SettingInitHand()
    {
        //　かーどをそれぞれに3枚配る
        for (int i = 0; i < 3; i++)
        {
            GiveCardToHand(playerDeck,playerHandTransform);
            GiveCardToHand(enemyDeck,enemyHandTransform);
        }
    }

    void GiveCardToHand(List<int> deck,Transform hand)
    {
        if (deck.Count == 0)
        {
            return;
        }
        int cardID = deck[0];
        deck.RemoveAt(0);
        CreateCard(cardID, hand);
    }

    void CreateCard(int cardID ,Transform hand)
    {
        //　カードの生成とデータの受け渡し
        CardController card = Instantiate(cardPrefab, hand, false);
        card.Init(cardID);
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
            GiveCardToHand(playerDeck,playerHandTransform);
        }
        else
        {
            GiveCardToHand(enemyDeck,enemyHandTransform);
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
        //　フィールドのカードを攻撃可能にする
        CardController[] enemyFieldCardList = enemyFieldTransform.GetComponentsInChildren<CardController>();
        foreach (CardController card in enemyFieldCardList)
        {
            // cardを攻撃可能にする
            card.SetCanAttack(true);
        }

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

        if (enemyCanAttackCardList.Length > 0 )
        {
            // attackerカードをを選択
            CardController attacker = enemyCanAttackCardList[0];
            if (playerFieldCardList.Length > 0)
            {
                // defenderカードをを選択
                CardController defender = playerFieldCardList[0];

                //attacker　と defender　カードを戦わせる
                CardsBattle(attacker, defender);
            }
            else
            {
                AttackToHero(attacker,false);
            }
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

    void ShowHeroHP()
    {
        playerHeroHpText.text = playerHeroHp.ToString();
        enemyHeroHpText.text = enemyHeroHp.ToString();
    }

    public void AttackToHero(CardController attacker, bool isPlayerCard)
    {
        if (isPlayerCard)
        {
            enemyHeroHp -= attacker.model.at;
        }
        else
        {
            playerHeroHp -= attacker.model.at;
        }
        attacker.SetCanAttack(false);
        ShowHeroHP();
        CheckHeroHp();
    }
    void CheckHeroHp()
    {
        if (playerHeroHp <= 0 || enemyHeroHp <= 0)
        {
            resultPanel.SetActive(true);
            if (playerHeroHp <= 0)
            {
                resultText.text = "LOSE";
            }
            else
            {
                resultText.text = "WIN";
            }
        }
    }

}
