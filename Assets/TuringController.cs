using System.Collections;
using System.Collections.Generic; // ¡Importante! Esta línea es necesaria
using UnityEngine;

public class TuringController : MonoBehaviour
{
    // ----- Estas son las variables de nuestro "cerebro" -----

    // 1. LA CINTA:
    List<string> cinta = new List<string>();

    // 2. EL CABEZAL:
    int posicionCabezal = 0;

    // 3. EL ESTADO:
    string estadoActual = "Q0";

    // 4. LA TABLA DE TRANSICIÓN (El Núcleo):
    Dictionary<(string, string), (string, string, string)> reglas = new Dictionary<(string, string), (string, string, string)>();

    // ----- Fin de las variables -----


    // Start() se ejecuta UNA VEZ cuando le das Play.
    // Lo usaremos para configurar la máquina.
    void Start()
    {
        // Al empezar, no cargamos ninguna regla.
        // Carga una cinta de ejemplo para 3 - 2 (111011)
        CargarCinta("111011");
    }

    void Update()
    {
        // Presiona Espacio para ejecutar un paso
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Step();
        }

        // Presiona "S" para cargar la SUMA
        if (Input.GetKeyDown(KeyCode.S))
        {
            CargarReglasSuma();
            CargarCinta("1101"); // Carga 2+1
        }

        // Presiona "R" para cargar la RESTA
        if (Input.GetKeyDown(KeyCode.R))
        {
            CargarReglasResta();
            CargarCinta("111011"); // Carga 3-2
        }
    }

    // ... aquí va tu función Step() ...

    // ... aquí van tus CargarReglasSuma() y CargarReglasResta() ...

    // ----- AÑADE ESTA NUEVA FUNCIÓN -----
    // La usaremos para reiniciar la cinta fácilmente

    void CargarCinta(string contenido)
    {
        // Limpiamos la cinta anterior
        cinta.Clear();

        // Añadimos blancos al inicio
        cinta.Add("_");
        cinta.Add("_");

        // Cargamos el contenido (ej: "111011")
        foreach (char simbolo in contenido)
        {
            cinta.Add(simbolo.ToString());
        }

        // Añadimos blancos al final
        cinta.Add("_");
        cinta.Add("_");

        // Reiniciamos el cabezal y el estado
        posicionCabezal = 2; // Inicia en el primer '1'
        estadoActual = "Q0";

        // Imprimir estado inicial
        Debug.Log("--- CINTA CARGADA (" + contenido + ") ---");
        Debug.Log(string.Join(" ", cinta));
        Debug.Log("Cabezal en: " + posicionCabezal + " (leyendo '" + cinta[posicionCabezal] + "')");
        Debug.Log("Estado actual: " + estadoActual);
    }


    // ----- ESTA ES LA NUEVA FUNCIÓN (EL MOTOR) -----

    void Step()
    {
        // 1. NO HACER NADA SI YA TERMINAMOS (Estado QH)
        if (estadoActual == "QH")
        {
            Debug.Log("--- MÁQUINA DETENIDA (Estado QH) ---");
            return; // Salir de la función
        }

        // 2. LEER EL SÍMBOLO ACTUAL
        string simboloLeido = cinta[posicionCabezal];

        // 3. BUSCAR LA REGLA EN NUESTRO DICCIONARIO
        // Creamos la "llave" para buscar: (Estado Actual, Símbolo Leído)
        (string, string) llaveRegla = (estadoActual, simboloLeido);

        // Verificamos si existe una regla para esta combinación
        if (reglas.ContainsKey(llaveRegla))
        {
            // 4. OBTENER Y APLICAR LA REGLA
            (string escribir, string mover, string nuevoEstado) accion = reglas[llaveRegla];

            // 4a. Escribir el nuevo símbolo en la cinta
            cinta[posicionCabezal] = accion.escribir;

            // 4b. Mover el cabezal
            if (accion.mover == "Derecha")
            {
                posicionCabezal++;
                // --- Seguridad de la cinta ---
                // Si nos salimos por la derecha, añadimos un nuevo "blanco"
                if (posicionCabezal >= cinta.Count)
                {
                    cinta.Add("_");
                }
            }
            else if (accion.mover == "Izquierda")
            {
                posicionCabezal--;
                // --- Seguridad de la cinta ---
                // Si nos salimos por la izquierda, añadimos un "blanco" al inicio
                if (posicionCabezal < 0)
                {
                    cinta.Insert(0, "_");
                    posicionCabezal = 0; // Reajustamos la posición a 0
                }
            }
            // (Si es "Stay", no hacemos nada)

            // 4c. Actualizar al nuevo estado
            estadoActual = accion.nuevoEstado;

            // 5. IMPRIMIR EL NUEVO ESTADO (para depurar)
            Debug.Log("---------------------------------");
            Debug.Log("Paso ejecutado. Nuevo estado:");
            Debug.Log(string.Join(" ", cinta));
            Debug.Log("Cabezal en: " + posicionCabezal + " (leyendo '" + cinta[posicionCabezal] + "')");
            Debug.Log("Estado actual: " + estadoActual);
        }
        else
        {
            // No se encontró una regla para (estado, símbolo)
            // Esto es un error o el fin de la computación
            Debug.LogError("ERROR: No existe regla para (" + estadoActual + ", " + simboloLeido + ")");
            estadoActual = "QH"; // Forzar detención
        }
    }
    // ... aquí termina tu función Step() ...

    void CargarReglasSuma()
    {
        // Limpiamos las reglas anteriores
        reglas.Clear();

        // --- REGLAS DE SUMA (Tu tabla ) ---
        reglas[("Q0", "1")] = ("1", "Derecha", "Q0");
        reglas[("Q0", "0")] = ("1", "Derecha", "Q1");
        reglas[("Q0", "_")] = ("_", "Stay", "QH");
        reglas[("Q1", "1")] = ("1", "Derecha", "Q1");
        reglas[("Q1", "_")] = ("_", "Izquierda", "Q2");
        reglas[("Q2", "1")] = ("_", "Stay", "QH");

        Debug.Log("--- REGLAS DE SUMA CARGADAS ---");
    }

    void CargarReglasResta()
    {
        reglas.Clear();

        // --- REGLAS DE RESTA (Basadas en tu nueva tabla de la imagen) ---
        // Mapeo: '-' en tu tabla es '0' aquí. 'h' es 'QH'.

        // Estado 0: Escanear el primer número hacia la derecha
        reglas[("Q0", "1")] = ("1", "Derecha", "Q0");
        reglas[("Q0", "0")] = ("0", "Derecha", "Q1"); // Encontró el separador '-'

        // Estado 1: Buscar un '1' en el segundo número para marcarlo
        reglas[("Q1", "1")] = ("x", "Izquierda", "Q2"); // Marca el 1 como x
        reglas[("Q1", "_")] = ("_", "Izquierda", "Q5"); // Fin del segundo número, ir a limpiar
        reglas[("Q1", "x")] = ("x", "Derecha", "Q1");   // Saltar las x

        // Estado 2: Volver a la izquierda hacia el separador
        reglas[("Q2", "0")] = ("0", "Izquierda", "Q3"); // Cruzó el separador hacia A
        reglas[("Q2", "x")] = ("x", "Izquierda", "Q2"); // Saltar las x

        // Estado 3: Buscar un '1' en el primer número para tacharlo
        reglas[("Q3", "1")] = ("x", "Derecha", "Q4");   // Marca el 1 de A como x
        reglas[("Q3", "_")] = ("_", "Derecha", "Q7");   // Se acabaron los 1s en A (Resultado negativo/cero)
        reglas[("Q3", "x")] = ("x", "Izquierda", "Q3"); // Saltar las x

        // Estado 4: Volver a la derecha hacia el separador
        reglas[("Q4", "0")] = ("0", "Derecha", "Q1");   // Volver a Q1 para repetir el ciclo
        reglas[("Q4", "x")] = ("x", "Derecha", "Q4");   // Saltar las x

        // Estado 5: Limpieza (El segundo número se acabó)
        reglas[("Q5", "0")] = ("_", "Izquierda", "Q6"); // Borra el separador '-'
        reglas[("Q5", "x")] = ("_", "Izquierda", "Q5"); // Borra las x del lado derecho

        // Estado 6: Limpieza final del resultado
        reglas[("Q6", "1")] = ("1", "Izquierda", "Q6"); // Deja los 1s del resultado quietos
        reglas[("Q6", "_")] = ("_", "Derecha", "QH");   // HALT: Terminó la resta correctamente
        reglas[("Q6", "x")] = ("_", "Izquierda", "Q6"); // Borra las x del lado izquierdo

        // Estado 7: Limpieza total (Caso cuando A < B)
        reglas[("Q7", "1")] = ("_", "Derecha", "Q7");   // Borra todo lo que encuentre
        reglas[("Q7", "0")] = ("_", "Derecha", "Q7");
        reglas[("Q7", "_")] = ("_", "Izquierda", "QH"); // HALT: Resultado es vacío (0)
        reglas[("Q7", "x")] = ("_", "Derecha", "Q7");

        Debug.Log("--- REGLAS DE RESTA (Tabla Imagen) CARGADAS ---");
    }
}
