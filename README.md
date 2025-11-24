# Proyecto Máquina de Turing (Unity)

Implementación en Unity de una máquina de Turing con visualización 3D y control completo por botones. Los scripts principales se encuentran en `Assets/`:

- `TuringController.cs`: cinta, reglas de suma/resta, motor paso a paso, auto‑run, limpieza y edición de celdas.
- `TuringVisualizer3D.cs`: genera o reutiliza los `CellPrefab`, actualiza los colores y posiciona el cabezal sobre la celda activa.
- `TuringUIController.cs`: conecta los botones del Canvas (`Cargar Suma`, `Cargar Resta`, `Paso`, `Play/Pause`, `Izquierda`, `Derecha`, `Colocar`, `Limpiar`).
- `StateLEDController.cs`: usa el estado actual para encender/apagar LEDs decorativos.
- Escena: `turingmachine.unity`.

---

## Preparar la escena

1. Abre `turingmachine.unity`.
2. En el objeto con `TuringVisualizer3D`, asigna `cellPrefab`, `headObject` y los puntos `inicioCinta`/`finCinta`, o marca `usarCeldasPrecolocadas` y arrastra el contenedor de las 22 celdas.
3. En el objeto con `TuringUIController`, vincula los botones existentes y, si lo deseas, los textos `EstadoLabel` / `MensajeLabel`.
4. (Opcional) Habilita `Edit → Project Settings → Player → Active Input Handling = Both` si vas a usar atajos de teclado durante el desarrollo.

---

## Uso rápido

1. `Cargar Suma` / `Cargar Resta`: cargan las tablas de transición, dejan el estado en `Q0` y mantienen la cinta actual.
2. `Limpiar`: genera 22 celdas en blanco, fija el cabezal en la primera celda editable.
3. `Izquierda` / `Derecha`: mueven el cabezal dentro de las celdas editables.
4. `Colocar`: cicla el símbolo de la celda actual (`1 → 0 → _`).
5. `Paso`: ejecuta una transición.
6. `Play/Pause`: resetea a la primera celda editable, reinicia el estado y ejecuta auto‑run hasta llegar a `QH`.

---

## Cinta y límites

- Longitud fija de 22 celdas (`longitudCintaLimpia`).
- Índice 0 y los dos últimos índices son guardas (no se escriben). Las celdas editables son 1..19 (suficientes para 9 + separador + 9).
- `CargarCinta()` rellena desde la celda 1 y trunca si el contenido excede las celdas permitidas.
- Los métodos de edición (mover, colocar símbolo) validan automáticamente que el cabezal permanezca en la zona editable.

---

## Parámetros útiles

- `TuringController`: `autoRunDelay`, `maxOperandValue`, `longitudCintaLimpia`.
- `TuringVisualizer3D`: `headHeight`, `headOffset`, `snapHeadToCell`, `usarNormalDeCelda`, `cellsToShow` (si se instancian celdas).

---

## Problemas comunes

- **Cabezal desalineado**: ajusta `headHeight` o activa `usarNormalDeCelda` en `TuringVisualizer3D`.
- **Botones sin efecto**: revisa que cada referencia del `TuringUIController` esté asignada; el script añade listeners en `Awake` solo si el botón no es `null`.
- **Tamaño de cinta incorrecto**: usa el botón `Limpiar` para regenerar la cinta antes de editar; se restauran las guardas y la longitud fija.
