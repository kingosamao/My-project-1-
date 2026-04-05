using UnityEngine;
using System.Collections.Generic;

public class GameData : MonoBehaviour
{
    public static GameData instance;

    public List<Card> selectedDeck;

    // --- AQUI ESTÁ A CORREÇÃO ---
    // ANTES (com erro):
    // public Card selectedActionCard;

    // DEPOIS (corrigido):
    // Agora a variável espera o tipo correto, ActionCard.
    public ActionCard selectedActionCard;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}