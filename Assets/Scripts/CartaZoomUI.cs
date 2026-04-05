using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CartaZoomUI : MonoBehaviour
{
    public TextMeshProUGUI nomeTexto;
    public TextMeshProUGUI descricaoTexto;
    public TextMeshProUGUI statusTexto;
    public TextMeshProUGUI costTexto;
    public TextMeshProUGUI classificationTextZoom;
    public Image fundoImagem;

    public static CartaZoomUI instancia;

    void Awake()
    {
        instancia = this;
        gameObject.SetActive(false);
    }

    public void MostrarCarta(Card carta)
    {
        gameObject.SetActive(true);
        nomeTexto.text = carta.cardName;
        descricaoTexto.text = carta.description;
        costTexto.text = $"{carta.cost}";


        if (carta.type == CardType.Animal)
        {
            // Se for um Animal, mostra os stats de ataque e vida
            statusTexto.gameObject.SetActive(true);
        }
        else
        {
            // Se não for um Animal, esconde os stats de ataque e vida
            statusTexto.gameObject.SetActive(false);
        }
        statusTexto.text = $"ATK: {carta.attack}  HP: {carta.health}";
        fundoImagem.sprite = carta.cardArt; // se tiver imagem
        if (carta.type == CardType.Animal)
        {
            if (classificationTextZoom != null)
            {
                classificationTextZoom.gameObject.SetActive(true);
                classificationTextZoom.text = $"Classe:{carta.classe} " +
                    $"Ordem:{carta.ordem}";
            }
        }
        else
        {
            if (classificationTextZoom != null)
            {
                classificationTextZoom.gameObject.SetActive(false);
            }
        }
    }

    public void Esconder()
    {
        gameObject.SetActive(false);
    }
}
