using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TuringController : MonoBehaviour
{
    [Header("Estado de la máquina")]
    [HideInInspector] public List<string> cinta = new List<string>();
    [HideInInspector] public int posicionCabezal = 0;
    [HideInInspector] public string estadoActual = "Q0";

    [Header("Control automático")]
    [SerializeField, Range(0.05f, 2f)] private float autoRunDelay = 0.5f;
    [SerializeField, Range(1, 9)] private int maxOperandValue = 9;

    [Header("Cintas por defecto (unario)")]
    [SerializeField] private string cintaSumaPorDefecto = "111011";  // 3 + 2
    [SerializeField] private string cintaRestaPorDefecto = "1111011"; // 4 - 3
    [SerializeField, Range(1, 64)] private int longitudCintaLimpia = 21;

    private Coroutine autoRunCoroutine;

    private int LongitudObjetivo => Mathf.Max(longitudCintaLimpia, 5);
    private int PrimerEditableIndex => LongitudObjetivo > 2 ? 1 : 0;
    private int UltimoEditableIndex => LongitudObjetivo > 2 ? LongitudObjetivo - 3 : PrimerEditableIndex;

    // Diccionario de reglas
    private readonly Dictionary<(string estado, string simbolo), (string escribir, string mover, string nuevoEstado)> reglas
        = new Dictionary<(string, string), (string, string, string)>();

    public int MaxOperandValue => maxOperandValue;
    public bool EstaCorriendoAutomatico => autoRunCoroutine != null;

    // ---------------------------------------------------------------------
    void Start()
    {
        // Arranca sin cinta cargada, solo las reglas de resta por defecto
        CargarReglasResta();
        estadoActual = "Q0";
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            Step();

        if (Input.GetKeyDown(KeyCode.S))
            CargarProgramaSuma();

        if (Input.GetKeyDown(KeyCode.R))
            CargarProgramaResta();

        if (Input.GetKeyDown(KeyCode.A))
        {
            if (EstaCorriendoAutomatico) StopAutoRun();
            else StartAutoRun();
        }
    }

    // ---------------------------------------------------------------------
    // Configuración general
    // ---------------------------------------------------------------------
    public void SetAutoRunDelay(float segundos)
    {
        autoRunDelay = Mathf.Clamp(segundos, 0.05f, 2f);
    }

    // ---------------------------------------------------------------------
    // PROGRAMAS
    // ---------------------------------------------------------------------
    public void CargarProgramaSuma()
    {
        StopAutoRun();
        CargarReglasSuma();
        estadoActual = "Q0";
        EnsureTapeInitialized();
        Debug.Log("Reglas de SUMA cargadas. Usa los botones de cinta para preparar la entrada.");
    }

    public void CargarProgramaResta()
    {
        StopAutoRun();
        CargarReglasResta();
        estadoActual = "Q0";
        EnsureTapeInitialized();
        Debug.Log("Reglas de RESTA cargadas. Usa los botones de cinta para preparar la entrada.");
    }

    // ---------------------------------------------------------------------
    // EJEMPLOS
    // ---------------------------------------------------------------------
    public void CargarEjemploSuma()
    {
        StopAutoRun();
        CargarReglasSuma();
        CargarCinta(cintaSumaPorDefecto);
        Debug.Log("Ejemplo de SUMA cargado.");
    }

    public void CargarEjemploResta()
    {
        StopAutoRun();
        CargarReglasResta();
        CargarCinta(cintaRestaPorDefecto);
        Debug.Log("Ejemplo de RESTA cargado.");
    }

    // ---------------------------------------------------------------------
    // CARGA DE CINTA Y OPERANDOS
    // ---------------------------------------------------------------------
    public void CargarOperandos(int operandoA, int operandoB)
    {
        int a = Mathf.Clamp(operandoA, 0, maxOperandValue);
        int b = Mathf.Clamp(operandoB, 0, maxOperandValue);

        StopAutoRun();

        string contenido = new string('1', a) + "0" + new string('1', b);
        CargarCinta(contenido);
    }

    public void CargarCinta(string contenido)
    {
        LimpiarCinta();

        int indice = PrimerEditableIndex;
        foreach (char simbolo in contenido)
        {
            if (indice > UltimoEditableIndex)
                break;

            if (simbolo == '1' || simbolo == '0' || simbolo == '_')
            {
                cinta[indice] = simbolo.ToString();
                indice++;
            }
        }

        posicionCabezal = PrimerEditableIndex;
        estadoActual = "Q0";

        Debug.Log($"--- CINTA CARGADA ({contenido}) ---");
        Debug.Log(string.Join(" ", cinta));
    }

    // ---------------------------------------------------------------------
    // AUTO RUN
    // ---------------------------------------------------------------------
    public void StartAutoRun()
    {
        if (EstaCorriendoAutomatico) return;
        if (estadoActual == "QH")
        {
            Debug.LogWarning("Máquina detenida en QH. Carga una cinta nueva.");
            return;
        }
        autoRunCoroutine = StartCoroutine(AutoRunLoop());
    }

    public void StopAutoRun()
    {
        if (autoRunCoroutine == null) return;
        StopCoroutine(autoRunCoroutine);
        autoRunCoroutine = null;
    }

    private IEnumerator AutoRunLoop()
    {
        while (true)
        {
            Step();
            if (estadoActual == "QH")
            {
                autoRunCoroutine = null;
                yield break;
            }
            yield return new WaitForSeconds(autoRunDelay);
        }
    }

    // ---------------------------------------------------------------------
    // PASO A PASO
    // ---------------------------------------------------------------------
    public void Step()
    {
        EnsureTapeInitialized();

        if (estadoActual == "QH") return;

        if (posicionCabezal < 0) posicionCabezal = 0;
        if (posicionCabezal >= cinta.Count) posicionCabezal = cinta.Count - 1;

        string simboloLeido = cinta[posicionCabezal];
        var llave = (estadoActual, simboloLeido);

        if (!reglas.TryGetValue(llave, out var accion))
        {
            Debug.LogError($"ERROR: No existe regla para ({estadoActual}, {simboloLeido})");
            estadoActual = "QH";
            StopAutoRun();
            return;
        }

        cinta[posicionCabezal] = accion.escribir;

        switch (accion.mover)
        {
            case "Derecha":
                posicionCabezal++;
                if (posicionCabezal >= cinta.Count)
                    posicionCabezal = cinta.Count - 1;
                break;

            case "Izquierda":
                posicionCabezal--;
                if (posicionCabezal < 0)
                    posicionCabezal = 0;
                break;
        }

        estadoActual = accion.nuevoEstado;
        Debug.Log($"Paso ejecutado: {estadoActual}");
    }

    public void MoverCabezalIzquierda()
    {
        StopAutoRun();
        EnsureTapeInitialized();

        posicionCabezal = Mathf.Max(PrimerEditableIndex, posicionCabezal - 1);

        Debug.Log($"Cabezal movido manualmente a la izquierda. Posicion: {posicionCabezal}");
    }

    public void MoverCabezalDerecha()
    {
        StopAutoRun();
        EnsureTapeInitialized();

        posicionCabezal = Mathf.Min(UltimoEditableIndex, posicionCabezal + 1);

        Debug.Log($"Cabezal movido manualmente a la derecha. Posicion: {posicionCabezal}");
    }

    public void CiclarSimboloActual()
    {
        StopAutoRun();
        EnsureTapeInitialized();

        if (!EsIndiceEditable(posicionCabezal))
        {
            Debug.LogWarning("No se puede escribir en esta celda. Usa solo las 18 centrales.");
            return;
        }

        string actual = cinta[posicionCabezal];
        string siguiente = actual == "1" ? "0" : actual == "0" ? "_" : "1";
        cinta[posicionCabezal] = siguiente;

        Debug.Log($"Celda {posicionCabezal} ahora contiene '{siguiente}'.");
    }

    public void ReiniciarDesdeInicio()
    {
        StopAutoRun();
        EnsureTapeInitialized();
        posicionCabezal = PrimerEditableIndex;
        estadoActual = "Q0";
        Debug.Log("Cabezal y estado reiniciados al inicio.");
    }

    public void LimpiarCinta()
    {
        StopAutoRun();
        cinta.Clear();

        int longitud = LongitudObjetivo;
        for (int i = 0; i < longitud; i++)
            cinta.Add("_");

        posicionCabezal = PrimerEditableIndex;
        estadoActual = "Q0";
        Debug.Log($"Cinta limpiada con {longitud} celdas en blanco.");
    }

    void EnsureTapeInitialized()
    {
        if (cinta.Count == 0)
        {
            LimpiarCinta();
            return;
        }

        int longitud = LongitudObjetivo;

        while (cinta.Count < longitud)
            cinta.Add("_");

        if (cinta.Count > longitud)
            cinta.RemoveRange(longitud, cinta.Count - longitud);

        if (cinta.Count > 0) cinta[0] = "_";
        if (cinta.Count > 1) cinta[cinta.Count - 1] = "_";
        if (cinta.Count > 2) cinta[cinta.Count - 2] = "_";

        posicionCabezal = Mathf.Clamp(posicionCabezal, 0, cinta.Count - 1);
    }

    bool EsIndiceEditable(int index)
    {
        return index >= PrimerEditableIndex && index <= UltimoEditableIndex;
    }

    // ---------------------------------------------------------------------
    // REGLAS DE SUMA
    // ---------------------------------------------------------------------
    public void CargarReglasSuma()
    {
        reglas.Clear();

        reglas[("Q0", "1")] = ("1", "Derecha", "Q0");
        reglas[("Q0", "0")] = ("1", "Derecha", "Q1");
        reglas[("Q0", "_")] = ("_", "Stay", "QH");

        reglas[("Q1", "1")] = ("1", "Derecha", "Q1");
        reglas[("Q1", "_")] = ("_", "Izquierda", "Q2");

        reglas[("Q2", "1")] = ("_", "Stay", "QH");

        Debug.Log("Reglas de SUMA cargadas.");
    }

    // ---------------------------------------------------------------------
    // REGLAS DE RESTA
    // ---------------------------------------------------------------------
    public void CargarReglasResta()
    {
        reglas.Clear();

        reglas[("Q0", "1")] = ("1", "Derecha", "Q0");
        reglas[("Q0", "0")] = ("0", "Derecha", "Q1_BUSCAR");
        reglas[("Q0", "_")] = ("_", "Stay", "QH");

        reglas[("Q1_BUSCAR", "1")] = ("_", "Izquierda", "Q2");
        reglas[("Q1_BUSCAR", "_")] = ("_", "Derecha", "Q1_BUSCAR");
        reglas[("Q1_BUSCAR", "0")] = ("0", "Izquierda", "Q5_LIMPIAR");

        reglas[("Q2", "1")] = ("1", "Izquierda", "Q2");
        reglas[("Q2", "_")] = ("_", "Izquierda", "Q2");
        reglas[("Q2", "0")] = ("0", "Izquierda", "Q3");

        reglas[("Q3", "1")] = ("_", "Derecha", "Q4");
        reglas[("Q3", "0")] = ("0", "Izquierda", "Q3");
        reglas[("Q3", "_")] = ("_", "Derecha", "Q6");

        reglas[("Q4", "1")] = ("1", "Derecha", "Q4");
        reglas[("Q4", "_")] = ("_", "Derecha", "Q4");
        reglas[("Q4", "0")] = ("0", "Derecha", "Q1_BUSCAR");

        reglas[("Q5_LIMPIAR", "1")] = ("1", "Izquierda", "Q5_LIMPIAR");
        reglas[("Q5_LIMPIAR", "_")] = ("_", "Izquierda", "Q5_LIMPIAR");
        reglas[("Q5_LIMPIAR", "0")] = ("_", "Stay", "QH");

        reglas[("Q6", "1")] = ("_", "Derecha", "Q6");
        reglas[("Q6", "0")] = ("_", "Derecha", "Q6");
        reglas[("Q6", "_")] = ("_", "Stay", "QH");

        Debug.Log("Reglas de RESTA cargadas.");
    }
}
