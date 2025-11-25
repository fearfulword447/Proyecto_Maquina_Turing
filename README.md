# Proyecto Máquina de Turing (Unity)

Este es el proyecto para el taller de la Máquina de Turing, implementado en Unity.

- El script `TuringController.cs` (en la carpeta `Assets`) contiene toda la lógica de la máquina (la cinta, los estados y las reglas de transición).
- El objetivo es que la parte visual (modelos 3D, luces) se conecte a este script para funcionar de manera interactiva.

---

## ¡¡CONFIGURACIÓN OBLIGATORIA DE UNITY!!

**Debes hacer esto antes de poder darle "Play" al proyecto.**

Para que el script funcione, Unity necesita poder leer el teclado con el sistema antiguo y el nuevo al mismo tiempo.

1.  En Unity, ve al menú **Edit** > **Project Settings...**
2.  En la ventana que se abre, selecciona **Player** en la lista de la izquierda.
3.  Busca la sección **Other Settings**.
4.  Baja hasta que encuentres la opción **"Active Input Handling"**.
5.  Cámbiala de "Input System Package" a **"Both"**.
6.  Unity te pedirá reiniciar. Acepta.

*Esto es necesario porque el script usa `Input.GetKeyDown(KeyCode.Space)` para leer la barra espaciadora.*

---

## Manual de Uso (Interfaz Gráfica)

La máquina cuenta con una interfaz visual completa que permite ingresar datos manualmente en la cinta y controlar la ejecución sin necesidad de código.

### Formato de Datos
La máquina opera en sistema **Unario**.
* **1 (Uno):** Representado por una casilla activa/verde.
* **0 (Separador):** Representado por una casilla ploma/gris.
* **Ejemplo:** Para realizar la operación `3 - 2`, la cinta debe configurarse visualmente como: `111` (separador) `11`.

### Guía de Botones y Controles

**1. Navegación y Edición de Cinta**
* **Botones Izquierda / Derecha:** Mueven el selector a través de las casillas de la cinta para elegir dónde escribir.
* **Botón "Colocar":** Es un botón multifunción para escribir en la cinta:
    * **1 Clic:** Escribe un `1` (Marca la casilla como número/verde).
    * **2 Clics:** Escribe un `0` (Marca la casilla como separador/plomo).
    * *Nota:* Se pueden ingresar números del 1 al 9.

**2. Selección de Operación**
Una vez ingresados los datos en la cinta (Número A + Separador + Número B), se debe cargar la lógica deseada:
* **Botón "Cargar Suma":** Prepara la máquina con las reglas para sumar A + B.
* **Botón "Cargar Resta":** Prepara la máquina con las reglas para restar A - B.

**3. Control de Ejecución**
* **Botón "Play / Pause":** Inicia o pausa la ejecución automática de la máquina. Al darle Play, la máquina comenzará a leer la cinta y modificarla según la operación cargada.
* **Botón "Limpiar":** Reinicia la máquina por completo, borrando la cinta y restableciendo el estado a Q0 para realizar una nueva operación.

### Notas sobre Resultados
* **Suma:** Une ambas cadenas de unos.
* **Resta:** Si el primer número es mayor o igual al segundo, muestra el resultado correcto.
* **Resultado Negativo:** El sistema no acepta números negativos. Si se intenta restar un número menor menos uno mayor (Ej: `2 - 3`), el resultado final será **0** (cinta vacía).

---

## Tablas de Estados (Lógica Interna)

A continuación se detallan las reglas que sigue el "cerebro" de la máquina para cada operación una vez que se presiona Play.

### 1. Suma (A + B)
**Algoritmo:** Une las dos cadenas de unos reemplazando el cero central con un uno y borrando un uno del final.

| Estado Actual | Lee (Input) | Escribe (Output) | Movimiento | Nuevo Estado | Descripción de la Acción |
| :---: | :---: | :---: | :---: | :---: | :--- |
| **Q0** | `1` | `1` | Der | **Q0** | Avanza por el primer número. |
| **Q0** | `0` | `1` | Der | **Q1** | Encuentra el separador y lo convierte en 1. |
| **Q1** | `1` | `1` | Der | **Q1** | Avanza hasta el final de la cadena combinada. |
| **Q1** | `_` | `_` | Izq | **Q2** | Llega al final (vacío) y retrocede. |
| **Q2** | `1` | `_` | Stay | **QH** | Borra el último 1 sobrante y termina. |

---

### 2. Resta (A - B)
**Algoritmo:** Utiliza "marcado". Tacha (`x`) un 1 de la derecha (B) y viaja a la izquierda para tachar un 1 de la izquierda (A).

| Estado | Lee | Escribe | Mueve | Siguiente | Descripción de la Acción |
| :---: | :---: | :---: | :---: | :---: | :--- |
| **Q0** | `1` | `1` | Der | **Q0** | Inicio: Avanza buscando el separador. |
| **Q0** | `0` | `0` | Der | **Q1** | Encuentra separador, entra en zona B. |
| | | | | | |
| **Q1** | `1` | `x` | Izq | **Q2** | **Marca un 1 de B** y va a buscar su pareja. |
| **Q1** | `x` | `x` | Der | **Q1** | (Salta marcas `x` ya procesadas). |
| **Q1** | `_` | `_` | Izq | **Q5** | B está vacío. **Fin de la resta**, ir a limpiar. |
| | | | | | |
| **Q2** | `0` | `0` | Izq | **Q3** | Cruza el separador hacia la izquierda (A). |
| **Q2** | `x` | `x` | Izq | **Q2** | (Salta marcas `x` mientras retrocede). |
| | | | | | |
| **Q3** | `1` | `x` | Der | **Q4** | **Marca un 1 de A** (pareja encontrada). |
| **Q3** | `x` | `x` | Izq | **Q3** | (Salta marcas `x` buscando un 1 libre). |
| **Q3** | `_` | `_` | Der | **Q7** | A se acabó antes que B (Resultado negativo/cero). |
| | | | | | |
| **Q4** | `0` | `0` | Der | **Q1** | Vuelve a la zona B para repetir el ciclo. |
| **Q4** | `x` | `x` | Der | **Q4** | (Salta marcas hacia la derecha). |
| | | | | | |
| **Q5** | `0` | `_` | Izq | **Q6** | **Limpieza:** Borra el separador central. |
| **Q5** | `x` | `_` | Izq | **Q5** | Borra las marcas `x` sobrantes de la derecha. |
| | | | | | |
| **Q6** | `1` | `1` | Izq | **Q6** | Deja intactos los 1s sobrantes en A (Resultado). |
| **Q6** | `x` | `_` | Izq | **Q6** | Borra las marcas `x` de la izquierda. |
| **Q6** | `_` | `_` | Der | **QH** | **FIN.** Cinta limpia con el resultado correcto. |
| | | | | | |
| **Q7** | *Todo*| `_` | Der | **Q7** | (Caso A < B) Borra todo lo que encuentre. |
| **Q7** | `_` | `_` | Izq | **QH** | Fin (Resultado 0). |
