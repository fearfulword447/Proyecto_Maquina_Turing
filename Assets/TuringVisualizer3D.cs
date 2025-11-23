using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TuringVisualizer3D : MonoBehaviour
{
    [Header("Referencias")]
    public TuringController controller;   // Script lógico
    public GameObject cellPrefab;         // Prefab del "LED"
    public Transform headObject;          // Esfera/cabezal

    [Header("Anclajes de la cinta")]
    public Transform inicioCinta;         // Punto izquierdo del riel
    public Transform finCinta;            // Punto derecho del riel

    [Header("Cinta visible (21 LEDs fijos)")]
    [SerializeField] private int cellsToShow = 21; // No tocar en el inspector

    [Header("Celdas existentes (opcional)")]
    [SerializeField] private bool usarCeldasPrecolocadas = false;
    [SerializeField] private Transform contenedorCeldas;

    [Header("Animación del cabezal")]
    public float headHeight = 0.6f;
    public Vector3 headOffset = Vector3.zero;
    public bool snapHeadToCell = true;
    public bool usarNormalDeCelda = true;
    public float headMoveSpeed = 5f;

    [Header("Colores de las celdas")]
    public Color blankColor = Color.white;
    public Color oneColor   = new Color(1f, 1f, 0.2f);    // 1 = amarillo
    public Color zeroColor  = new Color(0.8f, 0.8f, 0.8f); // 0 = gris

    [Header("Opciones visuales")]
    public bool mostrarSimboloEnTexto = false;
    public bool resaltarCeldaActual   = false;
    [Range(0f, 2f)]
    public float emisionCeldaActual   = 0.8f;

    private readonly List<GameObject> cellInstances = new List<GameObject>();
    private bool celdasInstanciadas = false;
    private Vector3 headTargetPos;

    // -------------------------------------------------------------
    void Awake()
    {
        if (controller == null)
            controller = GetComponent<TuringController>();
    }

    void Start()
    {
        // Nos aseguramos que siempre haya 21
        cellsToShow = 21;

        CreateCells();
        ForceHeadPosition();
    }

    void Update()
    {
        if (controller == null)
            return;

        UpdateCells();
        UpdateHead();
    }

    // -------------------------------------------------------------
    void CreateCells()
    {
        LimpiarCeldasInstanciadas();
        cellInstances.Clear();

        if (usarCeldasPrecolocadas && contenedorCeldas != null)
        {
            foreach (Transform child in contenedorCeldas)
            {
                if (child == contenedorCeldas) continue;
                cellInstances.Add(child.gameObject);
            }

            if (cellInstances.Count == 0)
            {
                Debug.LogWarning("TuringVisualizer3D: el contenedor de celdas no tiene hijos. Se usara el modo instanciado.");
            }
            else
            {
                cellsToShow = cellInstances.Count;
                celdasInstanciadas = false;
                return;
            }
        }

        if (inicioCinta == null || finCinta == null)
        {
            Debug.LogError("TuringVisualizer3D: falta asignar 'inicioCinta' o 'finCinta' en el inspector.");
            return;
        }

        // Crear 21 celdas perfectamente repartidas entre inicioCinta y finCinta
        for (int i = 0; i < cellsToShow; i++)
        {
            float t = (cellsToShow == 1) ? 0f : (float)i / (cellsToShow - 1);
            Vector3 pos = Vector3.Lerp(inicioCinta.position, finCinta.position, t);

            GameObject cell = Instantiate(cellPrefab, pos, Quaternion.identity, transform);
            cell.name = "Cell_" + i;
            cellInstances.Add(cell);
        }

        celdasInstanciadas = true;
    }

    void LimpiarCeldasInstanciadas()
    {
        if (!celdasInstanciadas)
            return;

        foreach (var go in cellInstances)
        {
            if (go != null)
                Destroy(go);
        }

        celdasInstanciadas = false;
    }

    // -------------------------------------------------------------
    void UpdateCells()
    {
        if (controller.cinta == null || cellInstances.Count == 0)
            return;

        for (int localIndex = 0; localIndex < cellInstances.Count; localIndex++)
        {
            GameObject go = cellInstances[localIndex];
            if (go == null) continue;

            int tapeIndex = localIndex; // 0..20 => 21 celdas físicas

            string symbol = "_";
            if (tapeIndex >= 0 && tapeIndex < controller.cinta.Count)
                symbol = controller.cinta[tapeIndex];

            // Texto opcional dentro del LED
            var tmp = go.GetComponentInChildren<TextMeshPro>();
            if (tmp != null)
                tmp.text = mostrarSimboloEnTexto ? symbol : "";

            // Color según símbolo
            Color baseColor = blankColor;
            if (symbol == "1")      baseColor = oneColor;
            else if (symbol == "0") baseColor = zeroColor;

            var rend = go.GetComponent<Renderer>();
            if (rend != null)
            {
                rend.material.color = baseColor;

                if (resaltarCeldaActual && tapeIndex == controller.posicionCabezal)
                {
                    rend.material.EnableKeyword("_EMISSION");
                    rend.material.SetColor("_EmissionColor", baseColor * emisionCeldaActual);
                }
                else
                {
                    rend.material.DisableKeyword("_EMISSION");
                }
            }
        }
    }

    // -------------------------------------------------------------
    void ForceHeadPosition()
    {
        if (headObject == null || cellInstances.Count == 0 || controller == null)
            return;

        int localIndex = Mathf.Clamp(controller.posicionCabezal, 0, cellInstances.Count - 1);
        Vector3 cellPos = cellInstances[localIndex].transform.position;
        Vector3 offset = CalcularOffsetCabezal(localIndex);
        headObject.position = cellPos + offset;
        headTargetPos = headObject.position;
    }

    void UpdateHead()
    {
        if (headObject == null || cellInstances.Count == 0)
            return;

        int localIndex = Mathf.Clamp(controller.posicionCabezal, 0, cellInstances.Count - 1);
        Vector3 cellPos = cellInstances[localIndex].transform.position;
        Vector3 offset = CalcularOffsetCabezal(localIndex);
        headTargetPos = cellPos + offset;

        if (snapHeadToCell)
        {
            headObject.position = headTargetPos;
        }
        else
        {
            headObject.position = Vector3.Lerp(
                headObject.position,
                headTargetPos,
                Time.deltaTime * headMoveSpeed
            );
        }
    }

    Vector3 CalcularOffsetCabezal(int cellIndex)
    {
        Vector3 upDir = Vector3.up;
        if (usarNormalDeCelda && cellIndex >= 0 && cellIndex < cellInstances.Count)
        {
            var cell = cellInstances[cellIndex];
            if (cell != null)
                upDir = cell.transform.up;
        }

        Vector3 baseOffset = upDir.normalized * headHeight;
        if (headOffset != Vector3.zero)
            baseOffset += headOffset;

        return baseOffset;
    }
}
