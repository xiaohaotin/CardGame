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

    public bool isPlayerTurn;

    List<int> playerDeck = new List<int>() { 3, 1, 2, 2 , 3},
              enemyDeck = new List<int>() { 2, 1, 3, 1 , 3};

    [SerializeField] Text playerHeroHpText;
    [SerializeField] Text enemyHeroHpText;

    int playerHeroHp;
    int enemyHeroHp;

    [SerializeField] Transform playerHero;

    [SerializeField] Text playerManaCostText;
    [SerializeField] Text enemyManaCostText;
    public int playerManaCost;
    public int enemyManaCost;
    public int playerDefaultManaCost;
    public int enemyDefaultManaCost;

    //時間管理
    [SerializeField] Text timeCountText;
    public int timeCount;
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
        playerManaCost = playerDefaultManaCost = 10;        
        enemyHeroHp = 1;
        enemyManaCost = enemyDefaultManaCost = 10;        
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
        if (hand.name == "PlayerHand")
        {
            card.Init(cardID, true);
        }
        else 
        {
            card.Init(cardID,false);
        }
    }

    void TurnCalc()
    {
        StopAllCoroutines();
        StartCoroutine(CountDown());
        if (isPlayerTurn)
        {
            PlayerTurn();
        }
        else
        {
            StartCoroutine(EnemyTurn());
        }
    }


    IEnumerator CountDown()
    {
        timeCount = 30;
        timeCountText.text = timeCount.ToString();

        while (timeCount > 0)
        {
            yield return new WaitForSeconds(1);  //1秒待機
            timeCount--;
            timeCountText.text = timeCount.ToString();
        }
        ChangeTurn();
    }

    public void OnClickTurnEndButton()
    {
        if (isPlayerTurn)
        {
            ChangeTurn();
        }
    }

    public void ChangeTurn()
    {
        isPlayerTurn = !isPlayerTurn;


        CardController[] playerFieldCardList = playerFieldTransform.GetComponentsInChildren<CardController>();
        SettingCanAttackView(playerFieldCardList, false);

        CardController[] enemyFieldCardList = enemyFieldTransform.GetComponentsInChildren<CardController>();
        SettingCanAttackView(enemyFieldCardList, false);


        if (isPlayerTurn)
        {
            playerDefaultManaCost++;
            playerManaCost = playerDefaultManaCost;
            GiveCardToHand(playerDeck,playerHandTransform);
        }
        else
        {
            enemyDefaultManaCost++;
            enemyManaCost = enemyDefaultManaCost;
            GiveCardToHand(enemyDeck,enemyHandTransform);
        }
        ShowManaCost();
        TurnCalc();
    }

    void SettingCanAttackView(CardController[] fieldCardList, bool canAttack)
    {
        foreach (CardController card in fieldCardList)
        {
            card.SetCanAttack(canAttack);
        }
    }

    void PlayerTurn()
    {
        Debug.Log("Player Turn");
        // フィールドのカードを攻撃可能にする
        CardController[] playerFieldCardList = playerFieldTransform.GetComponentsInChildren<CardController>();
        SettingCanAttackView(playerFieldCardList, true);
    }

    IEnumerator EnemyTurn()
    {
        Debug.Log("Enemy Turn");
        //　フィールドのカードを攻撃可能にする
        CardController[] enemyFieldCardList = enemyFieldTransform.GetComponentsInChildren<CardController>();
        SettingCanAttackView(enemyFieldCardList,true);

        yield return new WaitForSeconds(1);

        /* 場にカードを出す */
        //手札のカードリストを取得
        CardController[] handCardList = enemyHandTransform.GetComponentsInChildren<CardController>();

        //コスト以下のカードがあれば、カードをフィールドに出し続ける
        while (Array.Exists(handCardList, card => card.model.cost <= enemyManaCost))
        {
            //コスト以下のカードリストを取得
            CardController[] selectableHandCardList = Array.FindAll(handCardList, card => card.model.cost <= enemyManaCost);
            
            //場に出すカードを選択
            CardController enemyCard = selectableHandCardList[0];
            //カードを移動
            StartCoroutine(enemyCard.movement.MoveToField(enemyFieldTransform));
            ReduceManaCost(enemyCard.model.cost, false);
            enemyCard.model.isFieldCard = true;
            handCardList = enemyHandTransform.GetComponentsInChildren<CardController>();
            yield return new WaitForSeconds(1);
        }



       

        yield return new WaitForSeconds(1);



        /*　攻撃　*/
        //　フィールドのカードリストを取得
        CardController[] fieldCardList = enemyFieldTransform.GetComponentsInChildren<CardController>();

        //攻撃可能カードがあれば攻撃を繰り返す
        while (Array.Exists(fieldCardList, card => card.model.canAttack))
        {
            //　攻撃可能カードを取得
            CardController[] enemyCanAttackCardList = Array.FindAll(fieldCardList, card => card.model.canAttack); //検索： Array.FindAll                                                                                                             //　defenderカードを選択(Playerフィールドから選択)
            CardController[] playerFieldCardList = playerFieldTransform.GetComponentsInChildren<CardController>();           
            // attackerカードをを選択
            CardController attacker = enemyCanAttackCardList[0];
            if (playerFieldCardList.Length > 0)
            {
                // defenderカードをを選択
                CardController defender = playerFieldCardList[0];

                //attacker　と defender　カードを戦わせる
                StartCoroutine(attacker.movement.MoveToTarget(defender.transform));
                yield return new WaitForSeconds(0.25f);
                CardsBattle(attacker, defender);
            }
            else
            {
                StartCoroutine(attacker.movement.MoveToTarget(playerHero));
                yield return new WaitForSeconds(0.25f);
                AttackToHero(attacker, false);
                yield return new WaitForSeconds(0.25f);
                CheckHeroHp();
            }
            fieldCardList = enemyFieldTransform.GetComponentsInChildren<CardController>();
            yield return new WaitForSeconds(1);
        }
        yield return new WaitForSeconds(1);
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
    }
    void CheckHeroHp()
    {
        if (playerHeroHp <= 0 || enemyHeroHp <= 0)
        {
            ShowResuluPanel(playerHeroHp);            
        }
    }
    void ShowResuluPanel(int heroHp)
    {
        StopAllCoroutines();
        resultPanel.SetActive(true);
        if (heroHp <= 0)
        {
            resultText.text = "LOSE";
        }
        else
        {
            resultText.text = "WIN";
        }
    }
}
