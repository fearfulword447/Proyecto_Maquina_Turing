using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TuringVisualizer3D : MonoBehaviour
{
    [Header("Referencias")]
    public TuringController controller;   // Tu script lógico
    public GameObject cellPrefab;         // Prefab del cubo de la cinta
    public Transform headObject;          // Esfera que muestra el cabezal

    [Header("Cinta visible")]
    public int cellsToShow = 16;          // Cuántas celdas se ven
    public float cellSpacing = 1.1f;      // Distancia entre cubos

    private List<GameObject> cellInstances = new List<GameObject>();
    private int offset = 0;               // Índice en la cinta para la celda más a la izquierda

    void Start()
    {
        // Si no arrastras el controller a mano, intenta buscarlo en el mismo objeto
        if (controller == null)
        {
            controller = GetComponent<TuringController>();
        }

        // Creamos los cubos una vez
        CreateCells();
        UpdateCells();
    }

    void Update()
    {
        if (controller == null || controller.cinta == null || controller.cinta.Count == 0)
            return;

        // Centramos la vista alrededor del cabezal
        offset = controller.posicionCabezal - cellsToShow / 2;

        UpdateCells();
    }

    void CreateCells()
    {
        // Limpia si ya había algo
        foreach (var go in cellInstances)
        {
            if (go != null) Destroy(go);
        }
        cellInstances.Clear();

        for (int i = 0; i < cellsToShow; i++)
        {
            Vector3 pos = new Vector3(i * cellSpacing, 0, 0);
            GameObject cell = Instantiate(cellPrefab, pos, Quaternion.identity, transform);
            cell.name = "Cell_" + i;
            cellInstances.Add(cell);
        }
    }

    void UpdateCells()
    {
        for (int i = 0; i < cellsToShow; i++)
        {
            int tapeIndex = offset + i;
            string symbol = "_";

            if (tapeIndex >= 0 && tapeIndex < controller.cinta.Count)
            {
                symbol = controller.cinta[tapeIndex];
            }

            GameObject cell = cellInstances[i];

            // Cambiar el texto
            TextMeshPro tmp = cell.GetComponentInChildren<TextMeshPro>();
            if (tmp != null)
            {
                tmp.text = symbol;
            }

            // Resaltar la celda donde está el cabezal
            Renderer r = cell.GetComponent<Renderer>();
            if (r != null)
            {
                if (tapeIndex == controller.posicionCabezal)
                    r.material.color = Color.yellow;
                else
                    r.material.color = Color.white;
            }
        }

        // Mover el cabezal encima de la celda actual
        if (headObject != null)
        {
            int headLocalIndex = controller.posicionCabezal - offset;
            if (headLocalIndex >= 0 && headLocalIndex < cellsToShow)
            {
                Vector3 cellPos = cellInstances[headLocalIndex].transform.position;
                headObject.position = cellPos + new Vector3(0, 1f, 0);
            }
        }
    }
}
