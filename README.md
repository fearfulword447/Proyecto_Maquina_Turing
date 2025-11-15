# Proyecto Máquina de Turing (Unity)

Este es el proyecto para el taller de la Máquina de Turing, implementado en Unity.

- El script `TuringController.cs` (en la carpeta `Assets`) contiene toda la lógica de la máquina (la cinta, los estados y las reglas de transición).
- El objetivo es que la parte visual (modelos 3D, luces) se conecte a este script para funcionar.

---

## ⚠️ ¡¡CONFIGURACIÓN OBLIGATORIA DE UNITY!!

**Debes hacer esto antes de poder darle "Play" al proyecto.**

Para que el script funcione, Unity necesita poder leer el teclado con el sistema antiguo y el nuevo al mismo tiempo.

1.  En Unity, ve al menú **Edit** > **Project Settings...**
2.  En la ventana que se abre, selecciona **Player** en la lista de la izquierda.
3.  Busca la sección **Other Settings**.
4.  Baja hasta que encuentres la opción **"Active Input Handling"**.
5.  Cámbiala de "Input System Package" a **"Both"**.
6.  Unity te pedirá reiniciar. Acepta.

*Esto es necesario porque el script usa `Input.GetKeyDown(KeyCode.Space)` para leer la barra espaciadora.*
