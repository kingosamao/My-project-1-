//MainMenuController.cs
using UnityEngine;
using UnityEngine.SceneManagement; // Essencial para mudar de cena

public class MainMenuController : MonoBehaviour
{
    [Header("Painéis do Menu")]
    public GameObject mainMenuPanel;
    public GameObject playPanel;
    public GameObject optionsPanel;

    void Start()
    {
        // Garante que apenas o painel principal esteja visível no início
        ShowMainMenu();
    }

    public void ShowMainMenu()
    {
        mainMenuPanel.SetActive(true);
        playPanel.SetActive(false);
        optionsPanel.SetActive(false);
    }

    // --- Funções dos Botões do Painel Principal ---
    public void OnPlayButtonClicked()
    {
        mainMenuPanel.SetActive(false);
        playPanel.SetActive(true);
    }

    public void OnDeckBuildButtonClicked()
    {
        // Carrega a cena do Deck Builder
        SceneManager.LoadScene("DeckBuilder");
    }

    public void OnOptionsButtonClicked()
    {
        mainMenuPanel.SetActive(false);
        optionsPanel.SetActive(true);
    }

    public void OnQuitButtonClicked()
    {
        Debug.Log("Saindo do jogo...");
        Application.Quit(); // Só funciona no jogo compilado
    }

    // --- Funções do Painel de Jogar ---
    public void OnPlayAIButtonClicked()
    {
        // Carrega a cena principal do jogo
        SceneManager.LoadScene("Game");
    }

    public void OnPlayOnlineButtonClicked()
    {
        Debug.Log("Funcionalidade online a ser implementada!");
        // Aqui viria a lógica para entrar em um lobby online
    }

    // A função de "Voltar" é a mesma para os dois sub-painéis
    public void OnBackButtonClicked()
    {
        ShowMainMenu();
    }
}