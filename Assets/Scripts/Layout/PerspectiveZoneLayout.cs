//PerspectiveZoneLayout.cs

using UnityEngine;
using System.Collections.Generic;

[ExecuteInEditMode] // Permite que o layout se atualize no Editor
public class PerspectiveZoneLayout : MonoBehaviour
{
    [Tooltip("A distância fixa entre o centro de cada carta.")]
    public float cardSpacing = 220f; // Novo controle principal

    [Tooltip("A 'profundidade' da perspectiva. Quanto maior, mais as cartas do centro ficam para frente.")]
    public float perspectiveAmount = 100f;

    [Tooltip("A curvatura do layout. Quanto maior, mais as cartas nas pontas se curvam.")]
    public float curveAmount = 50f;

    [Header("Configuração das Cartas")]
    public float baseCardScale = 1f;
    public float scaleFalloff = 0.2f;

    // --- MUDANÇA 1: RENAMEIE A VARIÁVEL PARA MAIOR CLAREZA ---
    [Tooltip("Rotação em torno do eixo Z (de lado).")]
    public float maxCardRotationZ = 15f;

    // --- MUDANÇA 2: ADICIONE A NOVA VARIÁVEL PARA O OUTRO EIXO ---
    [Tooltip("Inclinação em torno do eixo X (para frente/trás).")]
    public float tiltAmountX = 0f;

    [Header("Controle de Atualização")]
    // Adicionamos um "botão" para o Inspector
    [ContextMenuItem("Update Layout Now", "UpdateLayout")]
    public bool continuousUpdate = false;

    [Header("Ajuste de Posição")]
    public float verticalOffset = 0f;
    public float horizontalOffset = 0f;

    // Esta função é chamada automaticamente sempre que um filho é adicionado ou removido.
    //private void OnTransformChildrenChanged()
    //    UpdateLayout();
    //}

    // A função principal que organiza todas as cartas
    private void OnValidate()
    {
        // Se estivermos em modo Play, a DropZone vai cuidar da atualização.
        // Só atualiza no editor para podermos ver o resultado.
        if (!Application.isPlaying)
        {
            UpdateLayout();
        }
    }
    private void Update()
    {
        if (continuousUpdate && Application.isPlaying)
        {
            UpdateLayout();
        }
    }
    public void UpdateLayout()
    {
        List<RectTransform> children = new List<RectTransform>();
        for (int i = 0; i < transform.childCount; i++)
        {
            var child = transform.GetChild(i) as RectTransform;
            if (child != null) children.Add(child);
        }

        if (children.Count == 0) return;

        float totalWidth = (children.Count - 1) * cardSpacing;

        for (int i = 0; i < children.Count; i++)
        {
            RectTransform child = children[i];

            // A posição X agora é calculada a partir do espaçamento.
            // O cálculo começa em -totalWidth / 2 para centralizar o grupo de cartas.
            float xPos = (-totalWidth / 2f) + (i * cardSpacing);

            // A posição normalizada (de -0.5 a 0.5) ainda é útil para os outros efeitos.
            // Ela agora é baseada na posição X da carta em relação à largura total.
            // Adicionamos uma pequena checagem para evitar divisão por zero se houver apenas uma carta.
            float normalizedPosition = (totalWidth > 0) ? (xPos / totalWidth) : 0;

            // O resto dos cálculos (yPos, escala, rotação) pode continuar usando a normalizedPosition,
            // pois ela ainda representa a posição relativa da carta no arco.
            float yPos = (1 - Mathf.Cos(normalizedPosition * Mathf.PI)) * -curveAmount;
            yPos += (1 - Mathf.Cos(normalizedPosition * Mathf.PI * 2)) * -perspectiveAmount;

            child.anchoredPosition = new Vector2(xPos + horizontalOffset, yPos + verticalOffset);

            child.anchoredPosition = new Vector2(xPos, yPos + verticalOffset);
            child.anchoredPosition = new Vector2(xPos + horizontalOffset, yPos + verticalOffset);

            // --- ESCALA ---
            float scale = baseCardScale - (Mathf.Abs(normalizedPosition) * 2 * scaleFalloff);
            child.localScale = new Vector3(scale, scale, 1f);
            float zRotation = normalizedPosition * -maxCardRotationZ * 2;
            float xRotation = (1 - Mathf.Cos(normalizedPosition * Mathf.PI * 2)) * tiltAmountX;

            // Aplicamos as duas rotações. A ordem (Z, X, Y) é importante.
            child.localRotation = Quaternion.Euler(xRotation, 0, zRotation);

            // --- ORDEM DE RENDERIZAÇÃO ---
            // Cartas mais "para frente" (no centro) devem ser renderizadas por último
            // para ficarem na frente das outras.
            child.SetSiblingIndex(Mathf.RoundToInt(scale * 100));
        }
    }
}