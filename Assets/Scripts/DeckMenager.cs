using UnityEngine;
using System.Collections.Generic;
using Photon.Pun;

public class DeckManager : MonoBehaviour
{
    // 1. ADICIONE A REFERÊNCIA AO CARDDATABASE
    [Header("Bases de Dados")]
    public CardDatabase cardDatabase; // Arraste seu asset CardDatabase aqui

    [Header("Configuração de Deck Padrão")]
    public DeckList defaultPlayerDeck;
    public Transform playerHandArea;
    public Transform opponentHandArea;
    public GameObject cardPrefab;
    private List<Card> cardsToUseInGame;
    // DOIS DECKS SEPARADOS
    private Stack<Card> playerDeck = new Stack<Card>();
    private Stack<Card> opponentDeck = new Stack<Card>();
    [Header("Artes Padrão")]
    public Sprite cardBackArt;
    [Header("Referências Visuais")]
    public VisualDeck playerVisualDeck;
    public VisualDeck opponentVisualDeck;


    private PhotonView photonView;
    void Start() { }
    void Awake()
    {
        photonView = GetComponent<PhotonView>();
        // Adicione esta linha de segurança
        if (photonView == null)
        {
            photonView = gameObject.AddComponent<PhotonView>();
        }
    }

    public void SyncDeck(List<string> deckCardNames)
    {
        photonView.RPC("ReceiveDeck", RpcTarget.All, deckCardNames.ToArray());
    }
    [PunRPC]
    public void ReceiveDeck(string[] deckCardNames)
    {
        cardsToUseInGame = new List<Card>();
        foreach (var cardName in deckCardNames)
        {
            var card = FindCardByName(cardName);
            if (card != null) cardsToUseInGame.Add(card);
        }
        CreateDeck();
        playerVisualDeck?.UpdateVisuals(playerDeck.Count);
        opponentVisualDeck?.UpdateVisuals(opponentDeck.Count);
    }
    public Card FindCardByName(string name)
    {
        if (cardDatabase == null)
        {
            Debug.LogError("CardDatabase não está atribuído no DeckManager! Não é possível encontrar a carta.");
            return null;
        }
        return cardDatabase.FindNormalCardByName(name);
    }
    [PunRPC]
    public void SetupDecks(List<Card> myCards, List<Card> opponentCards)
    {
        // Limpa decks antigos
        playerDeck.Clear();
        opponentDeck.Clear();

        // Cria deck do jogador local
        Shuffle(myCards);
        foreach (Card c in myCards) playerDeck.Push(c);

        // Cria deck do oponente
        Shuffle(opponentCards);
        foreach (Card c in opponentCards) opponentDeck.Push(c);

        // Atualiza a UI visual
        playerVisualDeck?.UpdateVisuals(playerDeck.Count);
        opponentVisualDeck?.UpdateVisuals(opponentDeck.Count);
    }
    [PunRPC]
    void CreateDeck()
    {
        if (cardsToUseInGame == null || cardsToUseInGame.Count == 0)
        {
            Debug.LogError("Nenhuma carta para criar o deck! Usando fallback...");
            cardsToUseInGame = defaultPlayerDeck.cards;
        }
        var tempList = new List<Card>(cardsToUseInGame);
        Shuffle(tempList);
        foreach (Card c in tempList)
            playerDeck.Push(c);
    }

    void Shuffle(List<Card> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            Card temp = list[i];
            int randIndex = Random.Range(i, list.Count);
            list[i] = list[randIndex];
            list[randIndex] = temp;
        }
    }
    private Card CopiarCarta(Card original)
    {
        Card nova = ScriptableObject.CreateInstance<Card>();

        nova.cardName = original.cardName;
        nova.description = original.description;
        nova.attack = original.attack;
        nova.health = original.health;
        nova.cost = original.cost;
        nova.type = original.type;
        nova.classe = original.classe;
        nova.ordem = original.ordem;
        nova.cardArt = original.cardArt;
        nova.ability = original.ability;
        nova.activateInHand = original.activateInHand;
        nova.activateInField = original.activateInField;
        nova.hasTaunt = original.hasTaunt;
        nova.battlecryAbility = original.battlecryAbility;
        nova.deathrattleAbility = original.deathrattleAbility;
        return nova;
    }
    public void DrawInitialHand()
    {
        DrawCards(5, true);  // 5 para o jogador local
        DrawCards(5, false); // 5 para o oponente
    }
    public void DrawCards(int amount, bool forPlayer)
    {
        // A lógica de qual deck/mão usar está correta
        Stack<Card> targetDeck = forPlayer ? playerDeck : opponentDeck;
        Transform targetHand = forPlayer ? playerHandArea : opponentHandArea;

        if (targetDeck.Count == 0) return;

        for (int i = 0; i < amount; i++)
        {
            // ... (seu código de Instantiate, CopiarCarta, etc., que já funcionava) ...
            GameObject cardGO = Instantiate(cardPrefab, targetHand);
            CardDisplay display = cardGO.GetComponent<CardDisplay>();
            display.card = CopiarCarta(targetDeck.Pop());
            display.currentLocation = CardDisplay.CardLocation.InHand;

            // A lógica de virar a carta é a chave
            // Se a compra é para o oponente (do meu ponto de vista), vira.
            if (!forPlayer)
            {
                display.Flip(false, cardBackArt);
            }
            else // Se é para mim, mostra a frente.
            {
                display.ShowCard();
            }
        }

        // Atualiza a UI visual (isso já estava correto)
        playerVisualDeck?.UpdateVisuals(playerDeck.Count);
        opponentVisualDeck?.UpdateVisuals(opponentDeck.Count);
    }
}
