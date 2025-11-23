using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

public class TuringUIController : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] TuringController controller;

    [Header("Botones")]
    [SerializeField] Button btnCargarSuma;
    [SerializeField] Button btnCargarResta;
    [SerializeField] Button btnPaso;
    [SerializeField] Button btnPlayPause;
    [SerializeField] Button btnMoverIzquierda;
    [SerializeField] Button btnMoverDerecha;
    [SerializeField] Button btnCambiarSimbolo;
    [SerializeField] Button btnLimpiarCinta;

    [Header("Indicadores (opcional)")]
    [SerializeField] TMP_Text estadoLabel;
    [SerializeField] TMP_Text mensajeLabel;

    readonly List<ButtonBinding> buttonBindings = new List<ButtonBinding>();

    struct ButtonBinding
    {
        public Button button;
        public UnityAction action;
    }

    void Awake()
    {
        HookButton(btnCargarSuma, OnCargarSuma);
        HookButton(btnCargarResta, OnCargarResta);
        HookButton(btnPaso, OnPaso);
        HookButton(btnPlayPause, OnPlayPause);
        HookButton(btnMoverIzquierda, OnMoverIzquierda);
        HookButton(btnMoverDerecha, OnMoverDerecha);
        HookButton(btnCambiarSimbolo, OnCambiarSimbolo);
        HookButton(btnLimpiarCinta, OnLimpiarCinta);
    }

    void OnDestroy()
    {
        foreach (var binding in buttonBindings)
        {
            if (binding.button != null)
                binding.button.onClick.RemoveListener(binding.action);
        }
    }

    void Update()
    {
        if (estadoLabel != null && controller != null)
            estadoLabel.text = "Estado: " + controller.estadoActual;
    }

    void HookButton(Button button, UnityAction action)
    {
        if (button == null || action == null)
            return;

        button.onClick.AddListener(action);
        buttonBindings.Add(new ButtonBinding { button = button, action = action });
    }

    void OnCargarSuma()
    {
        if (controller == null) return;
        controller.StopAutoRun();
        controller.CargarProgramaSuma();
        SetMensaje("Programa de suma cargado.");
    }

    void OnCargarResta()
    {
        if (controller == null) return;
        controller.StopAutoRun();
        controller.CargarProgramaResta();
        SetMensaje("Programa de resta cargado.");
    }

    void OnPaso()
    {
        if (controller == null) return;
        controller.StopAutoRun();
        controller.Step();
        SetMensaje("Paso ejecutado.");
    }

    void OnPlayPause()
    {
        if (controller == null) return;

        if (controller.EstaCorriendoAutomatico)
        {
            controller.StopAutoRun();
            SetMensaje("Auto-run detenido.");
        }
        else
        {
            controller.ReiniciarDesdeInicio();
            controller.StartAutoRun();
            SetMensaje("Auto-run iniciado.");
        }
    }

    void OnMoverIzquierda()
    {
        if (controller == null) return;
        controller.MoverCabezalIzquierda();
        SetMensaje("Cabezal movido a la izquierda.");
    }

    void OnMoverDerecha()
    {
        if (controller == null) return;
        controller.MoverCabezalDerecha();
        SetMensaje("Cabezal movido a la derecha.");
    }

    void OnCambiarSimbolo()
    {
        if (controller == null) return;
        controller.CiclarSimboloActual();
        SetMensaje("Celda actual alternada (1/0/_).");
    }

    void OnLimpiarCinta()
    {
        if (controller == null) return;
        controller.LimpiarCinta();
        SetMensaje("Cinta limpiada.");
    }

    void SetMensaje(string texto)
    {
        if (mensajeLabel != null)
            mensajeLabel.text = texto;
        else
            Debug.Log(texto);
    }
}
