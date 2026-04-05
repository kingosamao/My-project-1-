using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GraveyardViewer : MonoBehaviour
{
    public static GraveyardViewer instance;

    [Header("Referências da UI")]
    public GameObject graveyardPanel; // Arraste o GraveyardPanel aqui
    public Transform contentParent;   // Arraste o objeto "Content" da Scroll View aqui
    public Button closeButton;        // Arraste o CloseButton aqui

    [Header("Prefab da Carta")]
    public GameObject cardPrefab; // Arraste seu prefab de carta que tem o CardDisplay

    private GameManager gm;

    void Awake()
    {
        // Padrão Singleton para fácil acesso
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        gm = FindObjectOfType<GameManager>();

        // Esconde o painel no início
        graveyardPanel.SetActive(false);

        // Configura o botão de fechar para chamar a função HideGraveyard
        closeButton.onClick.AddListener(HideGraveyard);
    }

    /// <summary>
    /// Abre o painel e exibe as cartas do cemitério do dono especificado.
    /// </summary>
    /// <param name="owner">O dono do cemitério a ser visualizado (Jogador ou Oponente).</param>
    public void ShowGraveyard(DropZone.DonoDaZona owner)
    {
        // 1. Limpa as cartas da visualização anterior
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        // 2. Determina qual lista de cemitério usar
        List<Card> graveyardToShow = (owner == DropZone.DonoDaZona.Jogador)
            ? gm.playerGraveyard
            : gm.opponentGraveyard;

        // 3. Popula a grade com as cartas do cemitério
        if (graveyardToShow.Count == 0)
        {
            Debug.Log($"O cemitério de {owner} está vazio.");
        }

        foreach (Card cardData in graveyardToShow)
        {
            GameObject cardObj = Instantiate(cardPrefab, contentParent);
            CardDisplay display = cardObj.GetComponent<CardDisplay>();
            if (display != null)
            {
                display.card = cardData;
                display.ShowCard();
            }
            if (cardObj.GetComponent<CardDragHandler>() != null)
                cardObj.GetComponent<CardDragHandler>().enabled = false;
        }
        graveyardPanel.transform.SetAsLastSibling();
        // 4. Mostra o painel
        graveyardPanel.SetActive(true);
    }

    // --- ADICIONE ESTAS DUAS NOVAS FUNÇÕES ABAIXO ---

    /// <summary>
    /// Função pública para o botão do jogador. Não tem parâmetros, então a Unity vai encontrá-la.
    /// </summary>
    public void ShowPlayerGraveyard()
    {
        ShowGraveyard(DropZone.DonoDaZona.Jogador);
    }

    /// <summary>
    /// Função pública para o botão do oponente.
    /// </summary>
    public void ShowOpponentGraveyard()
    {
        ShowGraveyard(DropZone.DonoDaZona.Oponente);
    }

    // --- FIM DA ADIÇÃO ---


    /// <summary>
    /// Esconde o painel do cemitério.
    /// </summary>
    public void HideGraveyard()
    {
        graveyardPanel.SetActive(false);
    }
}