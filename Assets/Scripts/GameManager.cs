using Photon.Pun;
using Photon.Realtime;
using ExitGames.Client.Photon;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
// This script manages the game phases and action points in a card game.



public class GameManager : MonoBehaviourPunCallbacks
{
    [Header("Bases de Dados e Referências")]
    public CardDatabase cardDatabase;
    public DeckManager deckManager;
    public GameObject actionCardPrefab;
    public GameObject cardPrefab;
    public Transform playerActionCardSlot;
    public Transform opponentActionCardSlot;


    // --- Variáveis de estado de combate ---
    public CardDisplay attacker { get; private set; }
    public CardDisplay defender { get; private set; }
    public enum TurnPhase { Inicial, Principal, Batalha, Final }
    [Header("Pilhas de Descarte")]
    public List<Card> playerGraveyard = new List<Card>();
    public List<Card> opponentGraveyard = new List<Card>();
    [HideInInspector] public bool isTargetingModeActive = false;
    private CardAction actionWaitingForTarget;
    private CardDisplay sourceCardForTargeting;
    private DropZone.DonoDaZona ownerOfTargetingAction;

    [Header("UI de Fim de Jogo")]
    public GameObject endGamePanel;
    public TextMeshProUGUI resultMessageText;
    public Button backToMenuButton; // Referência ao botão

    [Header("UI")]
    public Button nextPhaseButton;
    public TextMeshProUGUI turnCounterText;
    public TextMeshProUGUI phaseText;
    public Slider playerLifeSlider;
    public Slider oponenteLifeSlider;
    public TextMeshProUGUI playerVidaTexto;
    public TextMeshProUGUI vidaDoOponenteTexto;
    public TextMeshProUGUI playerActionPointsText;
    public TextMeshProUGUI opponentActionPointsText;

    // --- Propriedades de estado sincronizadas ---
    public TurnPhase currentPhase { get; private set; }
    public int turnoAtual { get; private set; } = 1;
    public bool isMasterClientTurn { get; private set; } = true;

    // --- Variáveis Locais ---
    public bool isOnlineMatch { get; private set; }
    private int playerVida, oponenteVida;
    private int playerActionPoints, opponentActionPoints;
    private ActionCard myActionCard, opponentActionCard;
    private bool gameHasStarted = false;
    private PhotonView photonView;
    private int nextMatchID = 1;


    void Awake()
    {
        photonView = GetComponent<PhotonView>();
    }
    void Start()
    {
        isOnlineMatch = PhotonNetwork.IsConnected;
       /* if (isOnlineMatch)
        {
            // Pega o deck e a Action Card escolhidos pelo jogador no DeckBuilder
            ActionCard chosenActionCard = GameData.instance?.selectedActionCard ?? cardDatabase.allActionCards[0];
            List<Card> chosenDeck = GameData.instance?.selectedDeck ?? deckManager.defaultPlayerDeck.cards;

            // ANUNCIA o deck completo para os outros jogadores.
            Hashtable playerProps = new Hashtable {
                { "ActionCardName", chosenActionCard.name },
                { "DeckCardNames", chosenDeck.Select(c => c.cardName).ToArray() }
            };
            PhotonNetwork.LocalPlayer.SetCustomProperties(playerProps);
        }*/
        if (endGamePanel != null)
        {
            endGamePanel.SetActive(false);
        }

        // Configura o clique do botão
        if (backToMenuButton != null)
        {
            backToMenuButton.onClick.AddListener(GoBackToMenu);
        }
    }
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        // Se o jogo já começou, ignora todas as outras atualizações de propriedades.
        if (gameHasStarted) return;

        if (changedProps.ContainsKey("ActionCardName") && AllPlayersReady())
        {
            if (PhotonNetwork.IsMasterClient)
            {
                // Liga o "disjuntor" ANTES de enviar o RPC.
                gameHasStarted = true;
                photonView.RPC("SetupGameRPC", RpcTarget.All);
            }
        }
    }
    [PunRPC]
    public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
    {
        if (propertiesThatChanged.TryGetValue("Turno", out object turn))
        {
            turnoAtual = (int)turn;
            isMasterClientTurn = (bool)propertiesThatChanged["IsMasterTurn"];
            currentPhase = (TurnPhase)propertiesThatChanged["Phase"];

            if (currentPhase == TurnPhase.Inicial)
            {
                StartNewTurn();
            }
            UpdateUI();
        }
        else if (propertiesThatChanged.TryGetValue("Phase", out object phase))
        {
            currentPhase = (TurnPhase)phase;
            UpdateUI();
        }
    }
    [PunRPC]
    void SetupGameRPC()
    {
        // --- ADICIONE ESTA LINHA DE SEGURANÇA ---
        if (gameHasStarted && !PhotonNetwork.IsMasterClient) return; // Se eu não sou o master e já recebi este RPC, ignoro.

        // No cliente que recebe, também liga o "disjuntor".
        gameHasStarted = true;

        Debug.Log("RPC: Todos os jogadores estão prontos. Configurando o jogo para todos (CHAMADA ÚNICA).");

        // Define as Action Cards de cada jogador
        string myCardName = (string)PhotonNetwork.LocalPlayer.CustomProperties["ActionCardName"];
        string opponentCardName = (string)PhotonNetwork.PlayerListOthers[0].CustomProperties["ActionCardName"];
        myActionCard = cardDatabase.FindActionCardByName(myCardName);
        if (myActionCard == null)
        {
            Debug.LogError($"NÃO FOI POSSÍVEL ENCONTRAR MINHA ACTION CARD: '{myCardName}'. Usando a primeira da lista como fallback.");
            myActionCard = cardDatabase.allActionCards[0]; // Usa uma carta padrão para não travar
        }
        opponentActionCard = cardDatabase.FindActionCardByName(opponentCardName);
        if (opponentActionCard == null)
        {
            Debug.LogError($"NÃO FOI POSSÍVEL ENCONTRAR A ACTION CARD DO OPONENTE: '{opponentCardName}'. Usando a primeira da lista como fallback.");
            opponentActionCard = cardDatabase.allActionCards[0];
        }

        // Define os Decks de cada jogador
        string[] myDeckNames = (string[])PhotonNetwork.LocalPlayer.CustomProperties["DeckCardNames"];
        string[] opponentDeckNames = (string[])PhotonNetwork.PlayerListOthers[0].CustomProperties["DeckCardNames"];

        // Cria as listas de cartas
        List<Card> myDeck = myDeckNames.Select(name => cardDatabase.FindNormalCardByName(name)).ToList();
        List<Card> opponentDeck = opponentDeckNames.Select(name => cardDatabase.FindNormalCardByName(name)).ToList();

        // MANDA o DeckManager se preparar com AMBOS os decks
        deckManager.SetupDecks(myDeck, opponentDeck);

        // Configura Vida e Displays das Action Cards
        playerVida = myActionCard.startingLife;
        oponenteVida = opponentActionCard.startingLife;
        SetupActionCardDisplays();

        // Compra a mão inicial UMA VEZ AQUI
        deckManager.DrawInitialHand();

        // Master Client define o estado do primeiro turno
        if (PhotonNetwork.IsMasterClient)
        {
            Hashtable roomProps = new Hashtable {
                { "Turno", 1 },
                { "IsMasterTurn", true },
                { "Phase", TurnPhase.Inicial }
            };
            PhotonNetwork.CurrentRoom.SetCustomProperties(roomProps);
        }
    }

    void SetupActionCardDisplays()
    {
        foreach (Transform child in playerActionCardSlot) Destroy(child.gameObject);
        GameObject playerAC_GO = Instantiate(actionCardPrefab, playerActionCardSlot);
        playerAC_GO.GetComponent<ActionCardDisplay>().Setup(myActionCard, DropZone.DonoDaZona.Jogador);

        foreach (Transform child in opponentActionCardSlot) Destroy(child.gameObject);
        GameObject opponentAC_GO = Instantiate(actionCardPrefab, opponentActionCardSlot);
        opponentAC_GO.GetComponent<ActionCardDisplay>().Setup(opponentActionCard, DropZone.DonoDaZona.Oponente);
    }
    public void OnNextPhaseClicked()
    {
        if (isMyTurn())
        {
            // O jogador local envia um pedido para o Master Client
            photonView.RPC("RequestAdvancePhaseRPC", RpcTarget.MasterClient);
        }
    }
    [PunRPC]
    void RequestAdvancePhaseRPC()
    {
        if (!PhotonNetwork.IsMasterClient) return; // Segurança: Apenas o Master Client executa

        TurnPhase nextPhase = currentPhase;
        bool endOfTurn = false;

        switch (currentPhase)
        {
            case TurnPhase.Inicial: nextPhase = TurnPhase.Principal; break;
            case TurnPhase.Principal: nextPhase = (turnoAtual == 1 && isMasterClientTurn) ? TurnPhase.Final : TurnPhase.Batalha; break;
            case TurnPhase.Batalha: nextPhase = TurnPhase.Final; break;
            case TurnPhase.Final: endOfTurn = true; break;
        }

        Hashtable roomProps = new Hashtable();
        if (endOfTurn)
        {
            roomProps.Add("Turno", turnoAtual + 1);
            roomProps.Add("IsMasterTurn", !isMasterClientTurn);
            roomProps.Add("Phase", TurnPhase.Inicial);
        }
        else
        {
            roomProps.Add("Phase", nextPhase);
        }
        PhotonNetwork.CurrentRoom.SetCustomProperties(roomProps);
    }
    void StartNewTurn()
    {
        ResetarStatusDeTurno();

        // Lógica de Compra de Turno (Mão inicial JÁ FOI COMPRADA)
        bool shouldDraw = isMyTurn() ? (turnoAtual > 1 || !isMasterClientTurn) : (turnoAtual > 1 || isMasterClientTurn);
        if (isMyTurn() && shouldDraw)
        {
            // ANTES:
            // if (isMyTurn()) { deckManager.DrawCards(1, true); }

            // DEPOIS (CORRIGIDO):
            // Enviamos um RPC com a informação de quem deve comprar.
            photonView.RPC("RPC_DrawTurnCard", RpcTarget.All, isMasterClientTurn);
        }

        // Lógica de Ganho de PA
        playerActionPoints = isMyTurn() ? myActionCard.paPerTurn : 0;
        opponentActionPoints = isMyTurn() ? 0 : opponentActionCard.paPerTurn;
    }
    [PunRPC]
    void RPC_DrawTurnCard(bool isForMasterClient)
    {
       

        // É meu turno de comprar?
        bool amIDrawing = (isForMasterClient == PhotonNetwork.IsMasterClient);

        // O DeckManager local de CADA jogador executa a compra para o deck apropriado.
        if (amIDrawing)
        {
            deckManager.DrawCards(1, true); // O 'true' significa "para o meu deck local"
        }
        else
        {
            deckManager.DrawCards(1, false); // O 'false' significa "para o deck do oponente local"
        }
    }
    void Update()
    {
        UpdateUI();
    }
    void UpdateUI()
    {
        if (myActionCard == null || opponentActionCard == null) return;
        if (nextPhaseButton != null) nextPhaseButton.interactable = isMyTurn();

        if (!PhotonNetwork.InRoom || myActionCard == null || opponentActionCard == null)
        {
            return;
        }

        // Se passamos na checagem, o resto do código pode ser executado com segurança.
        if (nextPhaseButton != null) nextPhaseButton.interactable = isMyTurn();

        phaseText.text = $"Fase: {currentPhase}";
        if (PhotonNetwork.CurrentRoom.PlayerCount > 1)
        {
            turnCounterText.text = $"Turno {turnoAtual} — {(isMyTurn() ? PhotonNetwork.LocalPlayer.NickName : PhotonNetwork.PlayerListOthers[0].NickName)}";
        }

        playerVidaTexto.text = $"{playerVida}";
        vidaDoOponenteTexto.text = $"{oponenteVida}";
        playerActionPointsText.text = $"Player PA: {playerActionPoints}";
        opponentActionPointsText.text = $"Oponente PA: {opponentActionPoints}";

        if (playerLifeSlider != null) { playerLifeSlider.maxValue = myActionCard.startingLife; playerLifeSlider.value = playerVida; }
        if (oponenteLifeSlider != null) { oponenteLifeSlider.maxValue = opponentActionCard.startingLife; oponenteLifeSlider.value = oponenteVida; }

        CheckForGameOver();
    }
    private void CheckForGameOver()
    {
        // Se o jogo já acabou, não faz nada.
        if (endGamePanel.activeSelf) return;

        bool gameOver = false;
        string message = "";

        // Verifica se a vida de alguém chegou a zero ou menos
        if (playerVida <= 0)
        {
            gameOver = true;
            message = "Você Perdeu!";
        }
        else if (oponenteVida <= 0)
        {
            gameOver = true;
            message = "Você Venceu!";
        }

        // Se o jogo acabou...
        if (gameOver)
        {
            // ...mostra o painel de fim de jogo.
            ShowEndGamePanel(message);
        }
    }
    private void ShowEndGamePanel(string message)
    {
        if (endGamePanel != null)
        {
            // Desativa o botão de passar de fase para impedir mais ações
            if (nextPhaseButton != null)
            {
                nextPhaseButton.interactable = false;
            }

            // Preenche a mensagem e mostra o painel
            resultMessageText.text = message;
            endGamePanel.transform.SetAsLastSibling(); // Garante que fique na frente
            endGamePanel.SetActive(true);
        }
    }

    public void GoBackToMenu()
    {
        // Se estivermos em uma partida online, é importante desconectar da sala.
        if (PhotonNetwork.IsConnected)
        {
            Debug.Log("Saindo da sala do Photon...");
            PhotonNetwork.LeaveRoom();
        }
        else // Se estivermos offline, podemos voltar imediatamente.
        {
            SceneManager.LoadScene("MainMenu");
        }
    }
    public override void OnLeftRoom()
    {
        Debug.Log("Saída da sala confirmada. Voltando para o menu.");
        SceneManager.LoadScene("MainMenu");
    }

    public bool isMyTurn() => isOnlineMatch ? (isMasterClientTurn == PhotonNetwork.IsMasterClient) : true;
    public void AnnounceCardPlay(string cardName, int zoneID, bool cardWasPlayedByMaster)
    {
       
        if (PhotonNetwork.IsMasterClient)
        {
            int newID = nextMatchID;
            nextMatchID++; // Incrementa para o próximo

            // Envia o RPC com o novo ID.
            photonView.RPC("RPC_SyncPlayCard", RpcTarget.All, cardName, zoneID, cardWasPlayedByMaster, newID);
        }
        else // Se eu NÃO sou o Master Client...
        {
            // ...eu envio um RPC APENAS para o Master Client, pedindo para ele fazer a jogada por mim.
            photonView.RPC("RequestPlayCardRPC", RpcTarget.MasterClient, cardName, zoneID, cardWasPlayedByMaster);
        }
    }
    [PunRPC]
    void RequestPlayCardRPC(string cardName, int zoneID, bool cardWasPlayedByMaster)
    {
        // Esta função só é executada no Master Client.
        // Ele simplesmente pega o pedido e o retransmite para todos com um ID oficial.
        Debug.Log($"Master Client recebeu um pedido para jogar '{cardName}'. Retransmitindo para todos.");
        AnnounceCardPlay(cardName, zoneID, cardWasPlayedByMaster);
    }
    [PunRPC]
    void RPC_SyncPlayCard(string cardName, int zoneID, bool cardWasPlayedByMaster, int newMatchID)
    {
        Card cardData = cardDatabase.FindNormalCardByName(cardName);

        // A checagem mais importante: Se a carta for uma Estratégia, NÃO faça nada.
        // Os efeitos dela já foram sincronizados por outros RPCs (compra de carta, dano, etc.)
        bool playedByOpponent = (cardWasPlayedByMaster != PhotonNetwork.IsMasterClient);
        // --- LÓGICA DE TRADUÇÃO DE ID ---
        int targetZoneID = zoneID; // Começa com o ID original
        if (playedByOpponent)
        {
            // ...nós traduzimos os IDs.
            // Se ele jogou na zona 1 dele, para mim é a 4.
            // Se ele jogou na zona 2 dele, para mim é a 5.
            switch (zoneID)
            {
                case 1: // Support do Jogador -> Support do Oponente
                    targetZoneID = 4;
                    break;
                case 2: // Attack do Jogador -> Attack do Oponente
                    targetZoneID = 5;
                    break;
                    // Adicione outros mapeamentos se necessário
            }
        }
        DropZone targetZone = FindObjectsOfType<DropZone>().FirstOrDefault(z => z.zoneID == targetZoneID);
        if (cardData == null || targetZone == null) return;

        bool wasPlayedByMe = (cardWasPlayedByMaster == PhotonNetwork.IsMasterClient);
        if (wasPlayedByMe)
        {
            playerActionPoints -= cardData.cost;
        }

        // --- LÓGICA DE EXECUÇÃO ---
        DropZone.DonoDaZona effectOwner = wasPlayedByMe ? DropZone.DonoDaZona.Jogador : DropZone.DonoDaZona.Oponente;

        // Se for uma Estratégia, ela não é criada no campo.
        if (cardData.type == CardType.Estratégia)
        {
            Debug.Log($"Executando Estratégia Sincronizada: {cardData.cardName}");
            // Criamos uma carta temporária apenas para passar como referência para a habilidade.
            GameObject tempCardGO = new GameObject("TempCardForEffect");
            CardDisplay tempDisplay = tempCardGO.AddComponent<CardDisplay>();
            tempDisplay.card = cardData;

            // Ativa a habilidade e envia ao cemitério.
            cardData.ability?.Activate(this, tempDisplay, effectOwner);
            bool wasOwnedByMaster = (effectOwner == DropZone.DonoDaZona.Jogador);
            SendToGraveyard(tempDisplay, wasOwnedByMaster);
        }
        else // Se for um Animal, ele é criado no campo.
        {
            GameObject cardGO = Instantiate(cardPrefab, targetZone.transform);
            CardDisplay display = cardGO.GetComponent<CardDisplay>();

            display.matchID = newMatchID; // Carimba o ID
            display.card = CopiarCarta(cardData);
            display.currentLocation = CardDisplay.CardLocation.InField;
            display.turnoQueEntrou = this.turnoAtual;
            display.ShowCard();

            if (cardData.battlecryAbility != null)
            {
                cardData.battlecryAbility.Activate(this, display, effectOwner);
            }

            targetZone.GetComponent<PerspectiveZoneLayout>()?.UpdateLayout();
        }
    }
    [PunRPC]
    private void RPC_RemoveCardFromOpponentHand()
    {
        if (PhotonNetwork.IsMasterClient != isMasterClientTurn)
        {
            // Encontra o objeto da mão do oponente
            Transform opponentHand = FindObjectOfType<DeckManager>()?.opponentHandArea;
            if (opponentHand != null && opponentHand.childCount > 0)
            {
                // Destrói a primeira carta que encontrar na mão do oponente.
                // Como não sabemos qual carta foi, remover a primeira (ou a última)
                // é a aproximação visual mais simples.
                Destroy(opponentHand.GetChild(0).gameObject);
            }
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
    [PunRPC]
    private void RPC_SyncHealth(int masterClientLife, int otherClientLife)
    {
        Debug.Log($"RPC Recebido: Sincronizando vida. Master: {masterClientLife}, Cliente: {otherClientLife}");

        // Cada jogador define sua vida e a do oponente com base em quem é o Master Client.
        if (PhotonNetwork.IsMasterClient)
        {
            this.playerVida = masterClientLife;
            this.oponenteVida = otherClientLife;
        }
        else
        {
            this.playerVida = otherClientLife;
            this.oponenteVida = masterClientLife;
        }
    }
    public void ClearAttacker()
    {
        // Limpa a referência do atacante de forma segura.
        this.attacker = null;
    }
    private bool AllPlayersReady()
    {
        if (PhotonNetwork.CurrentRoom.PlayerCount < 2) return false;
        foreach (var player in PhotonNetwork.PlayerList)
        {
            if (!player.CustomProperties.ContainsKey("ActionCardName")) return false;
        }
        return true;
    }
    void ResetarStatusDeTurno()
    {
        foreach (var card in FindObjectsOfType<CardDisplay>())
        {
            card.jaAtacouNesseTurno = false;
            card.jaMoveuNesseTurno = false;
        }
    }
    [PunRPC]
    public void SyncGainActionPoints(int amount, bool forMasterClient)
    {
        if (forMasterClient)
        {
            if (PhotonNetwork.IsMasterClient) playerActionPoints += amount;
            else opponentActionPoints += amount;
        }
        else
        {
            if (!PhotonNetwork.IsMasterClient) playerActionPoints += amount;
            else opponentActionPoints += amount;
        }
        UpdateUI();
    }

    public void SelectAttacker(CardDisplay card)
    {
        if (currentPhase != TurnPhase.Batalha) return;

        if (card.jaAtacouNesseTurno)
        {
            Debug.Log("Este animal já atacou neste turno.");
            return;
        }

        attacker = card;
        Debug.Log($"Atacante selecionado: {attacker.card.cardName}");
    }


    public void SelectDefender(CardDisplay selectedDefender)
    {
        if (currentPhase != TurnPhase.Batalha || attacker == null) return;

        var opponentCreatures = FindObjectsOfType<CardDisplay>()
            .Where(c => c.currentLocation == CardDisplay.CardLocation.InField &&
                        c.GetComponentInParent<DropZone>().dono != attacker.GetComponentInParent<DropZone>().dono)
            .ToList();

        var taunters = opponentCreatures.Where(c => c.card.hasTaunt).ToList();

        if (taunters.Any() && !selectedDefender.card.hasTaunt)
        {
            Debug.Log("Você deve atacar uma criatura com Provocar!");
            return;
        }

        if (attacker.matchID != -1 && selectedDefender.matchID != -1)
        {
            Debug.Log($"Anunciando batalha para a rede: Carta ID {attacker.matchID} ataca Carta ID {selectedDefender.matchID}");
            // Enviamos os nossos IDs manuais no RPC.
            photonView.RPC("RPC_ResolveBattle", RpcTarget.All, attacker.matchID, selectedDefender.matchID);
        }
        else
        {
            Debug.LogError("Atacante ou Defensor não possui um matchID válido!");
        }

        attacker = null;
    }
    [PunRPC]
    private void RPC_ResolveBattle(int attackerMatchID, int defenderMatchID)
    {
        // Cada cliente encontra as cartas em sua própria cena usando os IDs de rede.
        CardDisplay battleAttacker = FindObjectsOfType<CardDisplay>().FirstOrDefault(c => c.matchID == attackerMatchID);
        CardDisplay battleDefender = FindObjectsOfType<CardDisplay>().FirstOrDefault(c => c.matchID == defenderMatchID);

        if (battleAttacker == null || battleDefender == null)
        {
            Debug.LogError($"RPC_ResolveBattle falhou: não foi possível encontrar uma das cartas pelos matchIDs.");
            return;
        }

        Debug.Log($"Sincronizando batalha: {battleAttacker.card.cardName} vs {battleDefender.card.cardName}");
        // Dano do atacante
        int danoDoAtacante = battleAttacker.card.attack;
        battleDefender.card.health -= danoDoAtacante;

        // Dano de retaliação
        DropZone zonaDoDefensor = battleDefender.GetComponentInParent<DropZone>();
        if (zonaDoDefensor != null && zonaDoDefensor.isAttackZone)
        {
            int danoDoDefensor = battleDefender.card.attack;
            battleAttacker.card.health -= danoDoDefensor;
        }

        battleAttacker.jaAtacouNesseTurno = true;

        // Verificação de morte
        if (battleDefender.card.health <= 0)
        {

            SendToGraveyard(battleDefender, !isMasterClientTurn);
        }
        if (battleAttacker.card.health <= 0)
        {
            SendToGraveyard(battleAttacker, isMasterClientTurn);
        }

        // Atualiza a UI
        battleAttacker?.ShowCard();
        battleDefender?.ShowCard();
    }

    public bool SpendActionPoints(int cost, bool isPlayer)
    {
        if (isPlayer)
        {
            if (playerActionPoints >= cost)
            {
                playerActionPoints -= cost;
                return true;
            }
        }
        else // É o oponente
        {
            if (opponentActionPoints >= cost)
            {
                opponentActionPoints -= cost;
                return true;
            }
        }

        Debug.Log("PA insuficiente.");
        return false;
    }
    public bool CanAfford(int cost)
    {
        return playerActionPoints >= cost;
    }
    [PunRPC]
    private void RPC_SendToGraveyard(string cardName, bool wasSentByMaster)
    {
        Card cardData = cardDatabase.FindNormalCardByName(cardName);
        if (cardData == null) return;

        // A LÓGICA DE PONTO DE VISTA CORRIGIDA:
        // A carta era minha? A resposta é SIM se o remetente tem o mesmo "status de master" que eu.
        bool wasMyCard = (wasSentByMaster == PhotonNetwork.IsMasterClient);

        if (wasMyCard)
        {
            Debug.Log($"Sincronizando '{cardName}' para o MEU cemitério.");
            playerGraveyard.Add(cardData);
        }
        else
        {
            Debug.Log($"Sincronizando '{cardName}' para o cemitério do meu OPONENTE.");
            opponentGraveyard.Add(cardData);
        }
    }
    public void SendToGraveyard(CardDisplay cardDisplay, bool wasOwnedByMaster)
    {
        if (cardDisplay == null) return;

        // A lógica de Último Suspiro precisa saber o dono local
        bool amIOwner = (wasOwnedByMaster == PhotonNetwork.IsMasterClient);
        if (amIOwner)
        {
            if (cardDisplay.card.deathrattleAbility != null)
            {
                // O 'owner' aqui é sempre 'Jogador' do ponto de vista local
                cardDisplay.card.deathrattleAbility.Activate(this, cardDisplay, DropZone.DonoDaZona.Jogador);
            }
        }

        // O RPC agora recebe a informação absoluta 'wasOwnedByMaster'.
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("RPC_SendToGraveyard", RpcTarget.All, cardDisplay.card.cardName, wasOwnedByMaster);
        }

        Destroy(cardDisplay.gameObject);
    }
    public void AnnounceAbilityActivation(int sourceCardMatchID, int targetCardMatchID)
    {
        // Envia um RPC para todos com os IDs da carta fonte e da carta alvo.
        // Se não houver alvo, targetCardMatchID será -1.
        photonView.RPC("RPC_ExecuteAbility", RpcTarget.All, sourceCardMatchID, targetCardMatchID);
    }

    // ESTE É O RPC QUE FAZ A MÁGICA
    [PunRPC]
    private void RPC_ExecuteAbility(int sourceCardMatchID, int targetCardMatchID)
    {
        // 1. Encontra a carta fonte em cada máquina.
        CardDisplay sourceCard = FindObjectsOfType<CardDisplay>().FirstOrDefault(c => c.matchID == sourceCardMatchID);
        if (sourceCard == null)
        {
            Debug.LogError($"RPC_ExecuteAbility não encontrou a carta fonte com ID {sourceCardMatchID}");
            return;
        }

        Ability ability = sourceCard.card.ability;
        if (ability == null) return;

        // 2. Determina o dono do efeito com base no ponto de vista local.
        var sourceZone = sourceCard.GetComponentInParent<DropZone>();
        DropZone.DonoDaZona owner = sourceZone.dono;

        Debug.Log($"Sincronizando ativação da habilidade '{ability.name}' da carta '{sourceCard.card.cardName}'.");

        // 3. Executa cada ação da habilidade.
        foreach (var action in ability.actions)
        {
            if (action.requiresTarget)
            {
                // Se a ação precisa de alvo, encontra o alvo pelo ID.
                CardDisplay targetCard = FindObjectsOfType<CardDisplay>().FirstOrDefault(c => c.matchID == targetCardMatchID);
                if (targetCard != null)
                {
                    action.ExecuteAction(this, sourceCard, owner, targetCard);
                }
            }
            else // Ação sem alvo
            {
                action.ExecuteAction(this, sourceCard, owner);
            }
        }
    }
    public bool CheckForValidTargets(CardAction action, DropZone.DonoDaZona owner)
    {
        // Encontra TODAS as cartas com CardDisplay na cena.
        // Nota: Em um jogo muito grande, isso pode ser lento, mas para o nosso escopo é perfeito.
        var allPotentialTargets = FindObjectsOfType<CardDisplay>();

        foreach (var potentialTarget in allPotentialTargets)
        {
            // Usa a mesma lógica de validação que já criamos na CardAction.
            if (action.IsValidTarget(potentialTarget, owner))
            {
                // Se encontrou UM alvo que seja válido, já podemos parar e dizer que a ação é possível.
                return true;
            }
        }

        // Se o loop terminar e não encontrou nenhum alvo, a ação é impossível.
        return false;
    }
    public void EnterTargetingMode(CardAction action, CardDisplay sourceCard, DropZone.DonoDaZona owner)
    {
        Debug.Log("Entrando em modo de mira...");
        isTargetingModeActive = true;
        actionWaitingForTarget = action;
        sourceCardForTargeting = sourceCard;
        ownerOfTargetingAction = owner;
        // (Opcional: Adicionar um feedback visual, como mudar o cursor do mouse)
    }
    public void SelectTarget(CardDisplay target)
    {
        if (!isTargetingModeActive) return;

        if (actionWaitingForTarget.IsValidTarget(target, ownerOfTargetingAction))
        {
            Debug.Log($"Alvo válido selecionado: {target.card.cardName}. Executando ação.");

            // Se for válido, executa a ação e sai do modo de mira.
            AnnounceAbilityActivation(sourceCardForTargeting.matchID, target.matchID);
            isTargetingModeActive = false;
            actionWaitingForTarget = null;
            sourceCardForTargeting = null;
        }
        else
        {
            // Se o alvo NÃO for válido, informa o jogador e continua no modo de mira.
            Debug.LogWarning($"Alvo inválido! {target.card.cardName} não é um alvo válido para esta habilidade.");
            // (Opcional: Adicionar um feedback sonoro ou visual de "erro")
        }
    }
    public void CancelTargetingMode()
    {
        if (!isTargetingModeActive) return;

        Debug.Log("Modo de mira cancelado.");
        isTargetingModeActive = false;
        actionWaitingForTarget = null;
        sourceCardForTargeting = null;
    }
    public void RequestDirectAttack(int attackerMatchID)
    {
        // Se eu sou o Master Client, eu processo o ataque imediatamente.
        if (PhotonNetwork.IsMasterClient)
        {
            ProcessDirectAttack(attackerMatchID);
        }
        else // Se não sou, eu envio um RPC pedindo para o Master Client processar.
        {
            photonView.RPC("RPC_RequestDirectAttack", RpcTarget.MasterClient, attackerMatchID);
        }
    }
    [PunRPC]
    private void RPC_RequestDirectAttack(int attackerMatchID)
    {
        // Esta função só é executada no Master Client.
        ProcessDirectAttack(attackerMatchID);
    }
    private void ProcessDirectAttack(int attackerMatchID)
    {
        if (!PhotonNetwork.IsMasterClient) return; // Segurança

        CardDisplay directAttacker = FindObjectsOfType<CardDisplay>().FirstOrDefault(c => c.matchID == attackerMatchID);
        if (directAttacker == null)
        {
            Debug.LogError($"ProcessDirectAttack falhou: não encontrou a carta com ID {attackerMatchID}");
            return;
        }

        // O Master Client calcula o dano e o novo estado de vida.
        int dano = directAttacker.card.attack;

        // Determina quem está sendo atacado
        int newMasterLife = this.playerVida;
        int newClientLife = this.oponenteVida;

        if (isMasterClientTurn) // Se o Master está atacando
        {
            newClientLife -= dano;
        }
        else // Se o Cliente está atacando
        {
            newMasterLife -= dano;
        }

        // O Master Client anuncia o NOVO ESTADO DE VIDA para todos.
        photonView.RPC("RPC_SyncHealth", RpcTarget.All, newMasterLife, newClientLife);

        // O Master Client também anuncia que a carta atacou.
        photonView.RPC("RPC_MarkCardAsAttacked", RpcTarget.All, attackerMatchID);
    }
    [PunRPC]
    private void RPC_MarkCardAsAttacked(int attackerMatchID)
    {
        CardDisplay card = FindObjectsOfType<CardDisplay>().FirstOrDefault(c => c.matchID == attackerMatchID);
        if (card != null)
        {
            card.jaAtacouNesseTurno = true;
        }

        // O jogador local que fez o ataque limpa sua referência de 'attacker'.
        if (this.attacker != null && this.attacker.matchID == attackerMatchID)
        {
            this.attacker = null;
        }
    }
}
