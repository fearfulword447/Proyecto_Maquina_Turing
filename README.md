# Proyecto Máquina de Turing (Unity)

Este es el proyecto para el taller de la Máquina de Turing, implementado en Unity.

- El script `TuringController.cs` (en la carpeta `Assets`) contiene toda la lógica de la máquina (la cinta, los estados y las reglas de transición).
- El objetivo es que la parte visual (modelos 3D, luces) se conecte a este script para funcionar.

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

## Manual de Uso

La máquina simula operaciones en sistema **Unario** (palitos).
* **Formato de Entrada:** `111011` representa `3` (operación) `2`. El `0` actúa como separador.
* **Símbolos:** `1` (Dato), `0` (Separador), `_` (Blanco/Vacío), `x` (Marca de proceso).

### Controles
* **Tecla `S`**: Carga la operación de **SUMA** (Ejemplo: 2 + 1).
* **Tecla `R`**: Carga la operación de **RESTA** (Ejemplo: 3 - 2).
* **Barra Espaciadora**: Ejecuta un **paso** de la máquina (Step). Mantener presionado para avanzar rápido.

---

## Tablas de Estados (Lógica)

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
