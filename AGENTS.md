# AGENTS.md — Repo Context

> Agent reference for this codebase. For the proposed architecture, code mapping, and detailed design pattern specs, see [`AppStructurePlan.md`](./AppStructurePlan.md).

---

## 1. Assessment Spec

**Module:** CIS2057-N — Software Engineering  
**Component:** 2 — Re-Engineering  
**Deadline:** 1st May 2025, 4:00 pm (Blackboard submission)  
**Module Leader:** Steven Mead

### Activities & Weighting

| Activity | Description | Weight |
|---|---|---|
| 1 — Analysis | Critique existing codebase; identify weaknesses, OOP improvements, and recommend a design pattern | 30% |
| 2 — Refactoring | DRY the code, refactor, apply design patterns, write unit tests | 40% |
| 3 — Journal | Technical development journal including sprint records and decision justifications | 30% |

### Mandatory Requirements

- **XML documentation** (`/// <summary>`) on every class, constructor, method, and field — required for marks.
- **Extensive unit test coverage** for both existing and new classes.
- **Agile/Scrum methodology**: 3 sprints of ~2 weeks each, each with a sprint plan and sprint review document.
- Product backlog developed in discussion with the tutor (who acts as product owner).
- The project must **compile, build, and run in the university labs**.

### Marking Signals (from criteria)

- 80+: Goes beyond requirements; all OOP problems resolved; pattern implemented flawlessly; minimal coupling between collaborating classes.
- 70–79: Majority of problems fixed; very good pattern implementation; detailed, informative console output.
- 60–69: Some problems fixed; pattern attempted but coupling present; simulation covers most scenarios.
- <50: Little done; pattern incomplete or broken.

---

## 2. Design Patterns in Scope

The learning materials in `Uni Docs/` cover the following patterns. See `AppStructurePlan.md §4` for how each is applied in this project.

| Pattern | Source Material | Applied As |
|---|---|---|
| **Facade** | `Adaptor&FacardRec.html` | `IFanService`, `IHeaterService`, `ISensorService`, `ISimulationService` — hide all `HttpClient` calls |
| **Strategy** | `Strategy and Decorator for Apprentices.html` | `ITemperatureControlStrategy` → `HeatUpStrategy`, `CoolDownStrategy`, `HoldStrategy` |
| **Factory** | `CIS2057-N-Factory-Patterns.html` | `DeviceServiceFactory` — constructs and wires all services from a shared `HttpClient` |
| **State** | `CIS2057-N-State-Pattern.html` | Not selected (Strategy preferred; see `AppStructurePlan.md §5`). If used, a state transition diagram is required by spec. |
| **Chain of Responsibility** | `CIS2057-N-Chain-of-Responsibility-Pattern.html` | Not in scope for this project |

---

## 3. `Program.cs` — Initial State

`Program.cs` is a **590-line God Class** — a single `Program` class holding every responsibility of the application. For a full critique of its problems, see `AppStructurePlan.md §1`.

### API Connection

```csharp
var client = new HttpClient { BaseAddress = new Uri("https://localhost:44351/") };
// Alternate remote: https://envrosym.azurewebsites.net/
const string apiKey = "u007-key";
client.DefaultRequestHeaders.Add("X-Api-Key", apiKey);
```

- There are 3 fans, 3 heaters, and 3 sensors (hardcoded `<= 3` loops throughout).
- The API key `"u007-key"` must match a dictionary key on the server.

### API Endpoints Used

| Method | Endpoint | Purpose |
|---|---|---|
| `GET` | `api/fans/{id}/state` | Returns `FanDTO` JSON (`{ Id, IsOn }`) |
| `POST` | `api/fans/{id}` | Body: `"true"` or `"false"` — sets fan on/off |
| `GET` | `api/heat/{id}/level` | Returns heater level as plain integer string |
| `POST` | `api/heat/{id}` | Body: integer string — sets heater level (0–5) |
| `GET` | `api/Sensor/sensor1` | Returns temperature as `string` |
| `GET` | `api/Sensor/sensor2` | Returns temperature as `int` |
| `GET` | `api/Sensor/sensor3` | Returns temperature as `decimal` |
| `POST` | `api/Envo/reset` | No body — resets simulation state |

> Sensor endpoints return inconsistent types (`string`, `int`, `decimal`). All must become `double` in `SensorService`. See `AppStructurePlan.md §5`.

### Menu Options

The `Main()` method runs an infinite `while(true)` loop presenting a 6-option menu:

| Option | Label | What it does |
|---|---|---|
| `1` | Control Fan | Prompts for fan ID and on/off, calls `SetFanState()` |
| `2` | Control Heater | Prompts for heater ID and level (0–5), calls `SetHeaterLevel()` |
| `3` | Read Temperature | Prompts for sensor ID, calls `GetSensorTemperature(client, sensorId)` (the only caller of this otherwise dead method variant) |
| `4` | Display State of All Devices | Loops fans, heaters, sensors — makes raw HTTP calls inline |
| `5` | Control Simulation | Runs the temperature control phase sequence; **missing `break`** — falls through into case 6 |
| `6` | Reset Simulation | POSTs to reset endpoint, then re-displays all device state inline (duplicate of case 4 logic) |

### Static Methods (all in `Program`)

| Method | Return Type | Lines | Notes |
|---|---|---|---|
| `Main(string[] args)` | `Task` | 8–291 | Entry point + full menu loop |
| `Reset(HttpClient)` | `Task` | 293–384 | **Dead code** — never called |
| `RunTemperatureControlLoop(HttpClient)` | `Task` | 386–415 | Commented out in case 5; contains duplicate phase logic |
| `AdjustTemperature(client, current, target, duration)` | `Task<double>` | 417–447 | Heats up or cools down based on comparison — both branches in one method |
| `HoldTemperature(client, current, target, duration)` | `Task<double>` | 449–476 | Maintains temperature within ±0 tolerance |
| `GetAverageTemperature(HttpClient)` | `Task<double>` | 478–487 | Calls all three sensor methods, casts to double, averages |
| `SetAllHeaters(HttpClient, int)` | `Task` | 489–494 | Loops 3 heaters |
| `SetAllFans(HttpClient, bool)` | `Task` | 497–503 | Loops 3 fans |
| `GetSensor1Temperature(HttpClient)` | `Task<string>` | 507–515 | Returns raw string |
| `GetSensor2Temperature(HttpClient)` | `Task<int>` | 517–530 | Parses int |
| `GetSensor3Temperature(HttpClient)` | `Task<decimal>` | 532–545 | Parses decimal |
| `GetSensorTemperature(HttpClient, int)` | `Task<double>` | 551–561 | **Dead code** — never called |
| `SetHeaterLevel(HttpClient, int, int)` | `Task` | 563–571 | POSTs level |
| `SetFanState(HttpClient, int, bool)` | `Task` | 573–581 | POSTs state |

### Nested Class

```csharp
public class FanDTO          // lines 584–588, nested inside Program
{
    public int Id { get; set; }
    public bool IsOn { get; set; }
}
```

### Temperature Control Phase Sequence

The control loop (used in both `case "5"` and `RunTemperatureControlLoop`) runs four sequential phases indefinitely:

1. Heat to **20°C** over 30 s → `AdjustTemperature(..., 20.0, 30)`
2. Cool to **16°C** over 10 s → `AdjustTemperature(..., 16.0, 10)`
3. Hold at **16°C** for 10 s → `HoldTemperature(..., 16.0, 10)`
4. Return to **18°C** over 20 s, then hold indefinitely → `AdjustTemperature(..., 18.0, 20)` + `HoldTemperature(..., 18.0, int.MaxValue)`

`AdjustTemperature` polls every 1 second and breaks early if within 0.1°C of target.
