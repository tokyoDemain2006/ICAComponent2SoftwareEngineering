# App Structure Plan

> **Purpose:** Developer reference for rewriting `Program.cs` into a properly structured, pattern-driven C# application. This document maps every piece of the existing God Class to its new home, specifies which design patterns apply where, and captures important context for the rewrite.

---

## 1. Current State: What's Wrong with `Program.cs`

`Program.cs` is a 590-line "God Class" — a single `Program` class containing every responsibility of the application. The problems are:

| Problem | Detail |
|---|---|
| No OOP | All methods are `static`; no objects, no interfaces, no abstractions |
| Mixed concerns | UI, HTTP calls, business logic, data models, and algorithms all in one class |
| Duplicate code | Device state display logic is copy-pasted verbatim in `case "4"` (lines 106–170) **and** `case "6"` (lines 204–272) |
| Inconsistent types | The three sensor methods return `string`, `int`, and `decimal` respectively — they should all return `double` |
| Hardcoded magic numbers | Device counts (3 fans, 3 heaters, 3 sensors) repeated across every loop |
| `case "5"` has no `break` | It falls through into `case "6"` — a bug |
| Dead code | `Reset()` private method (lines 293–384) is never called; `GetSensorTemperature(int sensorId)` (lines 551–561) is also never called |
| Nested DTO | `FanDTO` is a public class nested inside `Program` |
| HTTP client exposed everywhere | `HttpClient` is passed as a parameter to every static method |

---

## 2. Proposed File Structure

```
/
├── Program.cs                            # Entry point only — creates services and starts MenuController
│
├── Models/
│   └── FanDTO.cs                         # Extracted from nested class in Program (line 584)
│
├── Services/
│   ├── IFanService.cs                    # Facade interface for fan operations
│   ├── IHeaterService.cs                 # Facade interface for heater operations
│   ├── ISensorService.cs                 # Facade interface for sensor operations
│   ├── ISimulationService.cs             # Facade interface for simulation reset
│   ├── FanService.cs                     # HTTP implementation of IFanService
│   ├── HeaterService.cs                  # HTTP implementation of IHeaterService
│   ├── SensorService.cs                  # HTTP implementation of ISensorService
│   └── SimulationService.cs              # HTTP implementation of ISimulationService
│
├── Strategies/
│   ├── ITemperatureControlStrategy.cs    # Strategy interface for temperature phases
│   ├── HeatUpStrategy.cs                 # Heaters on, fans off — raises temperature
│   ├── CoolDownStrategy.cs               # Heaters off, fans on — lowers temperature
│   └── HoldStrategy.cs                   # Maintains temperature within tolerance band
│
├── Controllers/
│   └── TemperatureController.cs          # Orchestrates the phase sequence using strategies
│
└── UI/
    └── MenuController.cs                 # Main interactive menu loop (options 1–6)
```

---

## 3. God Class Code Mapping

Every method and block from `Program.cs` and where it belongs in the new structure:

| Code in `Program.cs` | Lines | New File |
|---|---|---|
| `FanDTO` nested class | 584–588 | `Models/FanDTO.cs` |
| `SetFanState()` | 573–581 | `Services/FanService.cs` → `SetFanStateAsync()` |
| `SetAllFans()` | 497–503 | `Services/FanService.cs` → `SetAllFansAsync()` |
| Fan state display loop (case 4 & 6) | 111–128, 212–227 | `Services/FanService.cs` → `GetAllFanStatesAsync()` |
| `SetHeaterLevel()` | 563–571 | `Services/HeaterService.cs` → `SetHeaterLevelAsync()` |
| `SetAllHeaters()` | 489–494 | `Services/HeaterService.cs` → `SetAllHeatersAsync()` |
| Heater level display loop (case 4 & 6) | 129–148, 229–248 | `Services/HeaterService.cs` → `GetHeaterLevelAsync()` |
| `GetSensor1Temperature()` | 507–515 | `Services/SensorService.cs` → unified `GetTemperatureAsync(1)` |
| `GetSensor2Temperature()` | 517–530 | `Services/SensorService.cs` → unified `GetTemperatureAsync(2)` |
| `GetSensor3Temperature()` | 532–545 | `Services/SensorService.cs` → unified `GetTemperatureAsync(3)` |
| `GetSensorTemperature(int sensorId)` | 551–561 | **Remove** — unused, superseded by the unified method |
| `GetAverageTemperature()` | 478–487 | `Services/SensorService.cs` → `GetAverageTemperatureAsync()` |
| Sensor display block (case 4 & 6) | 150–165, 251–266 | `Services/SensorService.cs` methods, called from `MenuController` |
| `AdjustTemperature()` | 417–447 | Split into `Strategies/HeatUpStrategy.cs` and `Strategies/CoolDownStrategy.cs` |
| `HoldTemperature()` | 449–476 | `Strategies/HoldStrategy.cs` |
| `RunTemperatureControlLoop()` | 386–415 | `Controllers/TemperatureController.cs` |
| Case 6 inline reset + display | 192–283 | `Services/SimulationService.cs` → `ResetAsync()`; display calls reuse service methods |
| `Reset()` private method | 293–384 | **Remove** — dead code; logic absorbed into `SimulationService` |
| `Main()` menu loop + switch cases | 8–291 | `UI/MenuController.cs` |
| `HttpClient` setup + API key | 11–18 | `Program.cs` (construction only) or a `Config` static class |

---

## 4. Design Patterns

### 4.1 Facade Pattern — `Services/`

**What it is:** A Facade provides a simplified interface over a complex subsystem. Here the subsystem is the HTTP API.

**Where it comes from in the God Class:** Every `GetAsync` / `PostAsync` call made directly in `Main()` and the static helper methods. The `HttpClient` is passed around as a raw parameter.

**How to apply it:**

Define one interface per device type. Each interface exposes meaningful, domain-level operations and hides all HTTP details behind it.

```csharp
public interface IFanService
{
    Task SetFanStateAsync(int fanId, bool isOn);
    Task<FanDTO> GetFanStateAsync(int fanId);
    Task SetAllFansAsync(bool isOn);
}

public interface IHeaterService
{
    Task SetHeaterLevelAsync(int heaterId, int level);
    Task<int> GetHeaterLevelAsync(int heaterId);
    Task SetAllHeatersAsync(int level);
}

public interface ISensorService
{
    Task<double> GetTemperatureAsync(int sensorId);   // unified double — no more string/int/decimal
    Task<double> GetAverageTemperatureAsync();
}

public interface ISimulationService
{
    Task ResetAsync();
}
```

Each concrete class (`FanService`, `HeaterService`, `SensorService`, `SimulationService`) takes an `HttpClient` via constructor injection and implements the corresponding interface. **No other class ever touches `HttpClient` directly.**

---

### 4.2 Strategy Pattern — `Strategies/`

**What it is:** Defines a family of algorithms, encapsulates each one, and makes them interchangeable. The consumer uses the algorithm via an interface without knowing the concrete type.

> *"Define a family of algorithms, encapsulate each one, and make them interchangeable. Strategy lets the algorithm vary independently from clients that use it."*  
> — Gang of Four

**Where it comes from in the God Class:** `AdjustTemperature()` (lines 417–447) already contains branching logic — if temperature is below target, heat up; if above, cool down. `HoldTemperature()` (lines 449–476) is a third distinct behaviour. These are three separate algorithms doing the same job (moving temperature toward a target) in different ways.

**How to apply it:**

```csharp
/// <summary>Represents a single temperature control phase strategy.</summary>
public interface ITemperatureControlStrategy
{
    /// <summary>Executes the strategy, returning the temperature when done.</summary>
    Task<double> ExecuteAsync(double currentTemperature, double targetTemperature, int durationSeconds);
}
```

| Concrete Strategy | Behaviour | Source in God Class |
|---|---|---|
| `HeatUpStrategy` | Heaters on (level 3), fans off; polls temperature each second | `AdjustTemperature()` branch where `current < target` |
| `CoolDownStrategy` | Heaters off (level 0), fans on; polls temperature each second | `AdjustTemperature()` branch where `current > target` |
| `HoldStrategy` | Adjusts minimally to keep within tolerance; loops for `durationSeconds` | `HoldTemperature()` entire method |

`TemperatureController` holds an `ITemperatureControlStrategy` and calls `ExecuteAsync()`. It swaps strategies between phases at runtime:

```csharp
// Phase 1: heat to 20°C over 30s
current = await _controller.RunPhaseAsync(new HeatUpStrategy(...), current, 20.0, 30);

// Phase 2: cool to 16°C over 10s
current = await _controller.RunPhaseAsync(new CoolDownStrategy(...), current, 16.0, 10);

// Phase 3: hold at 16°C for 10s
current = await _controller.RunPhaseAsync(new HoldStrategy(...), current, 16.0, 10);

// Phase 4: return to 18°C and maintain
current = await _controller.RunPhaseAsync(new HeatUpStrategy(...), current, 18.0, 20);
current = await _controller.RunPhaseAsync(new HoldStrategy(...), current, 18.0, int.MaxValue);
```

This means adding a new phase requires only a new class implementing `ITemperatureControlStrategy` — `TemperatureController` never changes (Open/Closed Principle).

---

### 4.3 Factory Pattern — `Services/` (recommended)

**What it is:** Centralises object creation so the consuming code never calls `new` on concrete types.

**Where it comes from in the God Class:** The `HttpClient` and all the services need constructing somewhere. Currently everything is constructed inline in `Main()`. A `DeviceServiceFactory` (or static factory method) constructs and wires the service instances from a shared `HttpClient`, keeping `Program.cs` minimal.

```csharp
public static class DeviceServiceFactory
{
    public static (IFanService, IHeaterService, ISensorService, ISimulationService)
        Create(HttpClient client)
    {
        return (
            new FanService(client),
            new HeaterService(client),
            new SensorService(client),
            new SimulationService(client)
        );
    }
}
```

---

### 4.4 Adapter Pattern — `Adapters/`

**What it is:** Converts the interface of a class into another interface that clients expect. The Adapter lets classes work together that could not otherwise because of incompatible interfaces.

**Problem solved:** The three sensor endpoints return temperatures in different types — Sensor 1 returns a plain `string`, Sensor 2 returns an `int`, and Sensor 3 returns a `decimal`. Any code that needs to read all three sensors would otherwise have to know and handle each type separately, spreading type-conversion logic across the codebase.

**How it is applied:**

`Interfaces/ISensor.cs` defines the uniform target interface:

```csharp
public interface ISensor
{
    int SensorId { get; }
    Task<double> GetTemperatureAsync();
}
```

Three concrete adapters in `Adapters/` each wrap one sensor endpoint and normalise its raw response to `double`:

| Adapter | Raw API type | Normalisation |
|---|---|---|
| `Sensor1Adapter` | `string` (e.g. `"21.5"`) | `double.TryParse` |
| `Sensor2Adapter` | `int` (e.g. `21`) | `int.TryParse`, widened to `double` |
| `Sensor3Adapter` | `decimal` (e.g. `21.75`) | `decimal.TryParse`, cast to `double` |

`SensorService` accepts an `IEnumerable<ISensor>` and calls `GetTemperatureAsync()` on each one uniformly — it never sees the raw types. Adding a fourth sensor requires only a new `ISensor` implementation; `SensorService` does not change (Open/Closed Principle).

---



### API Configuration
- Base URL (`https://localhost:44351/`) and API key (`u007-key`) are hardcoded on lines 11 and 16. Extract to a `Config.cs` static class or `appsettings.json`.
- Device counts (3 fans, 3 heaters, 3 sensors) repeated across many loops — extract to constants in `Config.cs`.

### Bug Fixes Required
- `case "5"` in the `Main()` switch is missing a `break` — it falls through into `case "6"`. Fix in `MenuController`.
- The `AdjustTemperature()` direction logic doesn't disambiguate heat-up from cool-down cleanly — the Strategy pattern resolves this by separating them entirely.

### Dead Code to Remove
- `Reset()` private method (lines 293–384) — never called; superseded by inline case 6 logic.
- `GetSensorTemperature(HttpClient client, int sensorId)` (lines 551–561) — never called; superseded by the three individual sensor methods.

### Unified Sensor Return Type
Current sensor methods return `string` (sensor 1), `int` (sensor 2), and `decimal` (sensor 3). `GetAverageTemperature()` casts them together awkwardly. All sensor reads in `SensorService` must return `double`.

### Unit Testing
The spec requires extensive unit test coverage. The interface-based design (`IFanService`, `IHeaterService`, `ISensorService`, `ITemperatureControlStrategy`) makes every class independently mockable. Strategy classes are particularly easy to unit test — they take a temperature in and return a temperature out with no side effects beyond the service calls (which can be mocked).

### XML Documentation
The spec requires XML doc comments (`/// <summary>`) on every class, constructor, method, and field. This is mandatory for the submission mark.

### State Pattern (if needed)
The spec's State Pattern material includes the note: *"If you use the state pattern, please provide a state transition diagram."* The simulation phases (heating → cooling → holding) could be modelled with the State pattern instead of Strategy. Strategy is recommended here because the phases are sequential steps orchestrated externally, not an object that autonomously transitions itself. If State is used, a transition diagram is required.
