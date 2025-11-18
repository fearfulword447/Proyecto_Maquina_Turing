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

        // --- REGLAS DE RESTA (Versión Corregida 3.0) ---

        // Q0: Ir al separador 
        reglas[("Q0", "1")] = ("1", "Derecha", "Q0");
        reglas[("Q0", "0")] = ("0", "Derecha", "Q1_BUSCAR"); // Va al separador y pasa a Q1_BUSCAR
        reglas[("Q0", "_")] = ("_", "Stay", "QH");

        // Q1_BUSCAR: (Reemplaza a Q1 de tu PDF )
        // Este estado busca un '1' en B, omitiendo blancos
        reglas[("Q1_BUSCAR", "1")] = ("_", "Izquierda", "Q2"); // ¡Encontró un 1! Bórralo y ve a Q2.
        reglas[("Q1_BUSCAR", "_")] = ("_", "Derecha", "Q1_BUSCAR"); // Omite blancos, sigue buscando

        // ¡ESTA ES LA REGLA CLAVE QUE FALTABA!
        // Si Q1_BUSCAR da la vuelta y encuentra el '0' de nuevo, B está vacío.
        reglas[("Q1_BUSCAR", "0")] = ("0", "Izquierda", "Q5_LIMPIAR");

        // Q2: Retroceder al separador 
        reglas[("Q2", "1")] = ("1", "Izquierda", "Q2");
        reglas[("Q2", "0")] = ("0", "Izquierda", "Q3");
        reglas[("Q2", "_")] = ("_", "Izquierda", "Q2"); // Omitir blancos

        // Q3: Borrar de la izquierda (A) 
        reglas[("Q3", "1")] = ("_", "Derecha", "Q4");    // OK 
        reglas[("Q3", "0")] = ("0", "Izquierda", "Q3");  // OK 
        reglas[("Q3", "_")] = ("_", "Derecha", "Q6");    // OK: A se acabó (A < B) 

        // Q4: Avanzar al separador 
        reglas[("Q4", "1")] = ("1", "Derecha", "Q4");
        reglas[("Q4", "_")] = ("_", "Derecha", "Q4"); // Omitir blancos
        reglas[("Q4", "0")] = ("0", "Derecha", "Q1_BUSCAR"); // Vuelve a Q1_BUSCAR para el prox. ciclo

        // Q5_LIMPIAR (Reemplaza al Q5 de tu PDF )
        // Se activa cuando A > B. Limpia el separador '0' y termina.
        reglas[("Q5_LIMPIAR", "1")] = ("1", "Izquierda", "Q5_LIMPIAR"); // Moverse a la izq. hasta el '0'
        reglas[("Q5_LIMPIAR", "_")] = ("_", "Izquierda", "Q5_LIMPIAR"); // Moverse a la izq. hasta el '0'
        reglas[("Q5_LIMPIAR", "0")] = ("_", "Stay", "QH"); // Borrar el '0' y terminar

        // Q6: Borrar todo (A < B) 
        reglas[("Q6", "1")] = ("_", "Derecha", "Q6");
        reglas[("Q6", "0")] = ("_", "Derecha", "Q6");
        reglas[("Q6", "_")] = ("_", "Stay", "QH"); // OK 

        // --- MENSAJE DE CONFIRMACIÓN ---
        Debug.Log("--- REGLAS DE RESTA (3.0) CARGADAS ---");
    }
}
