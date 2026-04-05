using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class VisualDeck : MonoBehaviour
{
    [Header("Referências")]
    public TextMeshProUGUI counterText;
    public Transform cardStackContainer;
    public GameObject cardBackPrefab; // Um prefab simples, apenas uma imagem do verso da carta

    [Header("Configuração da Pilha")]
    public int maxVisibleCards = 5;
    public Vector2 stackOffset = new Vector2(0, -2f); // Deslocamento para cada carta na pilha

    private List<GameObject> stackedCards = new List<GameObject>();

    // Função pública que será chamada pelo DeckManager
    public void UpdateVisuals(int cardCount)
    {
        // Atualiza o contador de texto
        if (counterText != null)
        {
            counterText.text = cardCount.ToString();
        }

        // Limpa a pilha visual antiga
        foreach (var card in stackedCards)
        {
            Destroy(card);
        }
        stackedCards.Clear();

        // Cria a nova pilha visual
        int cardsToShow = Mathf.Min(cardCount, maxVisibleCards);
        for (int i = 0; i < cardsToShow; i++)
        {
            GameObject cardBack = Instantiate(cardBackPrefab, cardStackContainer);
            // Aplica o deslocamento para criar o efeito de pilha
            cardBack.GetComponent<RectTransform>().anchoredPosition = i * stackOffset;
            stackedCards.Add(cardBack);
        }
    }
}