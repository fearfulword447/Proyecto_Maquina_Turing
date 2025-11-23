using System.Collections.Generic;
using UnityEngine;

public class StateLEDController : MonoBehaviour
{
    [System.Serializable]
    public class StateLED
    {
        public string stateName;         // Nombre del estado, ej: "Q0", "Q1", "QF"
        public Renderer ledRenderer;     // Renderer de la esfera del LED
        public Color onColor = Color.green;
        public Color offColor = Color.red;
    }

    [Header("Referencias")]
    public TuringController controller;  // Máquina de Turing lógica

    [Header("LEDs por estado")]
    public List<StateLED> leds = new List<StateLED>();

    void Update()
    {
        if (controller == null) return;

        foreach (var led in leds)
        {
            if (led.ledRenderer == null)
                continue;

            var mat = led.ledRenderer.material;

            if (controller.estadoActual == led.stateName)
            {
                // LED encendido
                mat.color = led.onColor;
                mat.EnableKeyword("_EMISSION");
                mat.SetColor("_EmissionColor", led.onColor * 0.5f);
            }
            else
            {
                // LED apagado
                mat.color = led.offColor;
                mat.DisableKeyword("_EMISSION");
            }
        }
    }
}
