//DeckBuilderController.cs

using System.Collections.Generic;
using System.IO; // Para manipulação de arquivos
using System.Linq; // Para usar funções como Count()
using TMPro; // Para usar TextMeshPro se necessário
using UnityEngine;
using UnityEngine.UI; // Para InputField, etc.
using UnityEngine.SceneManagement; // Para carregar cenas

public class DeckBuilderController : MonoBehaviour
{
    [Header("Fonte de Dados")]
    public CardDatabase cardDatabase; // Arraste seu asset CardDatabase aqui

    [Header("Coleções de Cartas")]
    private List<Card> allNormalCards;
    private List<ActionCard> allActionCards;

    private List<Card> currentDeck;
    private ActionCard currentActionCard; // Para guardar a Carta de Ação 

    [Header("Referências da UI")]
    public Transform collectionContentPanel;
    public Transform deckContentPanel;
    public Transform actionCardSlot;
    public GameObject actionCardUIPrefab;
    public GameObject cardUIPrefab; // O prefab que você usa para mostrar uma carta
    public InputField deckNameInput;
    public TextMeshProUGUI deckInfoText;
    [Header("UI de Carregamento")]
    public GameObject loadDeckPanel;
    public TMP_Dropdown deckSelectionDropdown;

    void Start()
    {
        // --- MUDANÇA 3: PREENCHA AS LISTAS A PARTIR DO BANCO DE DADOS ---
        if (cardDatabase == null)
        {
            Debug.LogError("ERRO CRÍTICO: O 'Card Database' não foi atribuído no Inspector do DeckBuilderController!");
            return;
        }

        // Copiamos as cartas do banco de dados para as nossas listas locais.
        allNormalCards = new List<Card>(cardDatabase.allNormalCards);
        allActionCards = new List<ActionCard>(cardDatabase.allActionCards);
        // Checagem de segurança no início
        if (cardUIPrefab == null)
        {
            Debug.LogError("ERRO CRÍTICO: O 'Card UI Prefab' não foi atribuído no Inspector do DeckBuilderController!");
            return; // Impede que o resto do código quebre
        }

        currentDeck = new List<Card>();
        PopulateCollection();
        UpdateDeckVisuals();
        loadDeckPanel.SetActive(false);
        if (PlayerPrefs.HasKey("LastSavedDeckName"))
        {
            string lastDeckName = PlayerPrefs.GetString("LastSavedDeckName");
            Debug.Log($"Encontrado último deck salvo: '{lastDeckName}'. Carregando...");
            LoadDeck(lastDeckName); // Usa a nossa função de carregar que já existe!
        }
        else
        {
            // Se for a primeira vez que o jogador abre o Deck Builder, começa com um deck novo.
            Debug.Log("Nenhum último deck salvo encontrado. Começando com um deck novo.");
            NewDeck();
        }
        }

    void PopulateCollection()
    {
        // Limpa o painel para evitar duplicatas ao recarregar
        foreach (Transform child in collectionContentPanel) Destroy(child.gameObject);

        // 1. POPULA COM AS CARTAS NORMAIS (ANIMAIS, ESTRATÉGIAS)
        Debug.Log($"Populando coleção com {allNormalCards.Count} cartas normais.");
        foreach (Card cardData in allNormalCards)
        {
            if (cardData == null) continue;

            // Instancia o prefab da CARTA NORMAL
            GameObject cardUI = Instantiate(cardUIPrefab, collectionContentPanel);

            // Pega o componente CardDisplay
            var display = cardUI.GetComponent<CardDisplay>();
            if (display != null)
            {
                // Atribui o ScriptableObject e chama ShowCard()
                display.card = cardData;
                display.ShowCard(); // Esta chamada preenche os textos
            }
            else
            {
                Debug.LogError($"O prefab 'cardUIPrefab' não tem o script CardDisplay!");
            }

            // Adiciona o listener para o clique
            var button = cardUI.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() => AddCardToDeck(cardData));
            }
        }

        // 2. POPULA COM AS CARTAS DE AÇÃO
        Debug.Log($"Populando coleção com {allActionCards.Count} cartas de ação.");
        foreach (ActionCard actionCardData in allActionCards)
        {
            if (actionCardData == null) continue;

            GameObject cardUI = Instantiate(actionCardUIPrefab, collectionContentPanel);
            var display = cardUI.GetComponent<ActionCardDisplay>();
            if (display != null)
            {
                // --- AQUI ESTÁ A CORREÇÃO ---
                // ANTES (com erro):
                // display.Setup(actionCardData); 

                // DEPOIS (corrigido):
                // Passamos o dono da carta como sendo o Jogador.
                display.Setup(actionCardData, DropZone.DonoDaZona.Jogador);
            }
            else
            {
                Debug.LogError($"O prefab 'actionCardUIPrefab' não tem o script ActionCardDisplay!");
            }

            // Adiciona o listener para o clique
            var button = cardUI.GetComponent<Button>();
            if (button != null)
            {
                button.onClick.AddListener(() => SetActionCard(actionCardData));
            }
        }
    }

    // Adiciona uma carta ao deck (chamado pelo clique na coleção)
    public void AddCardToDeck(Card cardToAdd)
    {
        // Validação das regras do deck
        if (currentDeck.Count >= 40)
        {
            Debug.LogWarning("Deck já atingiu o limite máximo de 40 cartas.");
            return;
        }

        int copiesInDeck = currentDeck.Count(c => c.cardName == cardToAdd.cardName);
        if (copiesInDeck >= 3)
        {
            Debug.LogWarning($"Já existem 3 cópias de '{cardToAdd.cardName}' no deck.");
            return;
        }

        // Se passou nas validações, adiciona a carta
        currentDeck.Add(cardToAdd);
        Debug.Log($"Adicionado '{cardToAdd.cardName}' ao deck.");
        UpdateDeckVisuals();
    }

    // Remove uma carta do deck (para ser chamado pelo clique no painel do deck)
    public void RemoveCardFromDeck(Card cardToRemove)
    {
        currentDeck.Remove(cardToRemove);
        Debug.Log($"Removido '{cardToRemove.cardName}' do deck.");
        UpdateDeckVisuals();
    }


    // Atualiza a UI para refletir as mudanças no deck
    void UpdateDeckVisuals()
    {
        // 1. Limpa o painel do deck
        foreach (Transform child in deckContentPanel)
        {
            Destroy(child.gameObject);
        }

        // 2. Repopula o painel do deck
        foreach (Card card in currentDeck)
        {
            GameObject cardUI = Instantiate(cardUIPrefab, deckContentPanel);
            cardUI.GetComponent<CardDisplay>().card = card;
            cardUI.GetComponent<CardDisplay>().ShowCard();

            // Adiciona o listener para REMOVER a carta
            cardUI.GetComponent<Button>().onClick.AddListener(() => RemoveCardFromDeck(card));
        }

        // 3. Atualiza os textos de informação
        deckInfoText.text = $"Cartas: {currentDeck.Count}/40";
        // (Aqui você adicionaria mais lógicas para as outras regras)
    }

    // --- LÓGICA DE SALVAMENTO E CARREGAMENTO ---
    public void SaveDeck()
    {
        if (currentDeck.Count < 30)
        {
            Debug.LogError("O deck precisa de no mínimo 30 cartas para ser salvo!");
            return;
        }
        if (string.IsNullOrEmpty(deckNameInput.text))
        {
            Debug.LogError("Por favor, dê um nome ao seu deck.");
            return;
        }

        if (currentActionCard == null)
        {
            Debug.LogError("Selecione uma Carta de Ação antes de salvar!");
            return;
        }

        DeckData data = new DeckData();
        data.deckName = deckNameInput.text;
        data.mainDeckCardNames = currentDeck.Select(c => c.cardName).ToList();

        // Salva o nome da Carta de Ação escolhida
        data.actionCardName = currentActionCard.cardName;
        // (Aqui você precisaria de uma lógica para selecionar e salvar a Action Card)

        // 2. Converte para JSON
        string json = JsonUtility.ToJson(data, true); // 'true' para formatar o texto

        // 3. Salva o arquivo
        string path = Path.Combine(Application.persistentDataPath, $"{data.deckName}.json");
        File.WriteAllText(path, json);

        PlayerPrefs.SetString("LastSavedDeckName", data.deckName);
        PlayerPrefs.Save(); // Força o salvamento dos dados no disco

        Debug.Log($"Deck salvo em: {path}. Definido como o último deck salvo.");
    }

    public void LoadDeck(string deckNameToLoad)
    {
        string path = Path.Combine(Application.persistentDataPath, $"{deckNameToLoad}.json");
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            DeckData data = JsonUtility.FromJson<DeckData>(json);

            // --- INÍCIO DA CORREÇÃO ---

            // 1. Limpa o estado atual
            currentDeck.Clear();
            currentActionCard = null;

            // 2. Recria o Deck Principal
            // O loop agora procura na nova lista 'allNormalCards'.
            foreach (string cardName in data.mainDeckCardNames)
            {
                Card card = allNormalCards.Find(c => c.cardName == cardName);
                if (card != null)
                {
                    currentDeck.Add(card);
                }
                else
                {
                    Debug.LogWarning($"Não foi possível encontrar a carta normal '{cardName}' na coleção ao carregar o deck.");
                }
            }

            // 3. Recria a Carta de Ação
            // Procura na nova lista 'allActionCards' pelo nome salvo.
            ActionCard actionCard = allActionCards.Find(ac => ac.cardName == data.actionCardName);
            if (actionCard != null)
            {
                // Usa a mesma função que o clique do mouse para definir a carta de ação e atualizar a UI.
                SetActionCard(actionCard);
            }
            else
            {
                Debug.LogWarning($"Não foi possível encontrar a carta de ação '{data.actionCardName}' na coleção ao carregar o deck.");
            }

            // --- FIM DA CORREÇÃO ---

            // Atualiza os elementos visuais
            deckNameInput.text = data.deckName;
            UpdateDeckVisuals(); // Esta função já atualiza o painel do deck principal

            Debug.Log($"Deck '{deckNameToLoad}' carregado com sucesso.");
        }
        else
        {
            Debug.LogError($"Arquivo de deck não encontrado: {path}");
        }
    }
    public void NewDeck()
    {
        // Limpa a lista de cartas do deck principal
        currentDeck.Clear();

        // Limpa a variável da carta de ação
        currentActionCard = null;

        // Limpa o nome do deck no campo de input
        deckNameInput.text = "";

        // Limpa o slot visual da carta de ação
        foreach (Transform child in actionCardSlot)
        {
            Destroy(child.gameObject);
        }

        // Atualiza a UI para refletir o deck vazio
        UpdateDeckVisuals();

        Debug.Log("Novo deck iniciado. Tudo limpo.");
    }
    public void SetActionCard(ActionCard card)
    {
        currentActionCard = card;
        Debug.Log($"Carta de Ação selecionada: {card.cardName}");

        // Atualiza o visual no slot dedicado
        // Limpa o slot antigo
        foreach (Transform child in actionCardSlot)
        {
            Destroy(child.gameObject);
        }
        // Instancia a nova carta no slot
        GameObject cardUI = Instantiate(actionCardUIPrefab, actionCardSlot);
        cardUI.GetComponent<ActionCardDisplay>().Setup(card, DropZone.DonoDaZona.Jogador);

    }
    public void OpenLoadDeckPanel()
    {
        PopulateLoadDeckDropdown();
        loadDeckPanel.SetActive(true);
    }

    public void CloseLoadDeckPanel()
    {
        loadDeckPanel.SetActive(false);
    }

    void PopulateLoadDeckDropdown()
    {
        deckSelectionDropdown.ClearOptions(); // Limpa as opções antigas

        string path = Application.persistentDataPath;
        // Pega todos os arquivos .json do diretório
        var deckFiles = Directory.GetFiles(path, "*.json");

        List<string> deckNames = new List<string>();
        foreach (var file in deckFiles)
        {
            // Pega apenas o nome do arquivo, sem a extensão .json
            deckNames.Add(Path.GetFileNameWithoutExtension(file));
        }

        if (deckNames.Count == 0)
        {
            deckSelectionDropdown.options.Add(new TMP_Dropdown.OptionData("Nenhum deck salvo"));
            deckSelectionDropdown.interactable = false;
        }
        else
        {
            deckSelectionDropdown.AddOptions(deckNames); // Adiciona os nomes ao dropdown
            deckSelectionDropdown.interactable = true;
        }
        deckSelectionDropdown.RefreshShownValue(); // Garante que a primeira opção seja exibida
    }

    // Função para ser chamada pelo botão "Carregar Selecionado"
    public void LoadSelectedDeck()
    {
        // Pega o nome do deck selecionado no dropdown
        string deckToLoad = deckSelectionDropdown.options[deckSelectionDropdown.value].text;

        if (!string.IsNullOrEmpty(deckToLoad) && deckToLoad != "Nenhum deck salvo")
        {
            LoadDeck(deckToLoad); // Chama a função que já temos!
            CloseLoadDeckPanel();
        }
    }
    public void ConfirmDeckAndReturn()
    {
        // Validação final do deck
        if (currentDeck.Count < 30 || currentActionCard == null)
        {
            Debug.LogError("Seu deck não está completo! (Mínimo 30 cartas e 1 Carta de Ação)");
            return;
        }

        // Salva a escolha do jogador no objeto que não é destruído
        if (GameData.instance != null)
        {
            GameData.instance.selectedDeck = new List<Card>(currentDeck);
            // Precisamos encontrar o ScriptableObject da ActionCard para passar adiante
            GameData.instance.selectedActionCard = allActionCards.Find(ac => ac.cardName == currentActionCard.cardName);
        }
        PlayerPrefs.SetString("LastSavedDeckName", deckNameInput.text);
        PlayerPrefs.Save();
        // Volta para o menu principal
        BackToMainMenu();
    }
    public void BackToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}