# 🏙️ AI City Simulation

**AI City Simulation** is a 3D simulation built in **Unity and C#**, featuring autonomous cars and pedestrians sharing a city environment.

Cars navigate roads, respond to traffic lights, and yield at crossings, while pedestrians explore the map, cross streets, and stop to talk with one another.

The project explores **rule-based AI, state machines, navigation, and interactions between autonomous agents**.

> Built with Unity 6, C#, and Unity AI Navigation.

---

## 🚗 Vehicle Behaviour

Cars move through a connected network of road nodes, choosing a neighbouring node as they reach each junction.

- 🚦 **Traffic lights** — Stop at red and yellow lights and proceed when their lane turns green.
- 🛑 **Stop signs** — Wait for a configurable duration before continuing.
- 🚶 **Pedestrian crossings** — Yield while pedestrians occupy a linked crosswalk.
- 🚘 **Vehicle detection** — Detect cars ahead and stop behind them.
- ⚙️ **Acceleration and braking** — Gradually adjust speed when starting and stopping.
- 🔄 **Stuck recovery** — Briefly reverse and attempt another available route when movement stalls.

World-space status labels show why each car is stopped or what it is currently doing.

---

## 🚶 Pedestrian Behaviour

Pedestrians use **Unity NavMesh navigation** to roam around the environment and select new destinations.

- 🗺️ **Autonomous roaming** — Move between randomly selected NavMesh destinations.
- 🚸 **Road crossing** — Wait at crossing zones, check nearby vehicles, and move towards the opposite side.
- 🎨 **Appearance variation** — Receive randomized colours to make individuals easier to distinguish.
- 🏷️ **Random names** — Display individual names above their heads.
- 🚶 **Varied walking speeds** — Move at slightly different speeds.
- 👥 **Population spawning** — Spawn a configurable number of pedestrians on the NavMesh.

Crossing behaviour uses three states: **Roaming, Waiting, and Crossing**.

---

## 💬 Social Interactions

Nearby pedestrians can pause their journey, face each other, and start a short conversation.

Conversations feature:

- **Speech bubbles** displayed above the pedestrians.
- **Name-based greetings** and alternating dialogue turns.
- **Randomized topics**, including weather, traffic, food, games, and everyday life.
- **Varied conversation lengths** and cooldown tracking.
- **Crosswalk checks** that prevent or cancel conversations while crossing.

Dialogue comes from predefined text collections, with randomized selection and attempts to avoid repeated lines.

---

## 🚦 Traffic System

An intersection controller alternates between **north/south** and **east/west** traffic, using configurable green and yellow phases.

Vehicles combine traffic-light permissions with local checks for cars ahead, stop signs, and occupied crossings. Pedestrians use nearby-car checks when deciding whether to cross.

These interacting systems create changing traffic queues and pedestrian activity throughout the simulation.

---

## 🎮 Camera Controls

Explore the city using a free-flying camera.

| Input | Action |
| --- | --- |
| **W / A / S / D** | Move the camera |
| **Mouse** | Look around |
| **Mouse wheel** | Adjust camera height |

The cursor locks automatically when the simulation starts. Camera movement follows the viewing direction.

---

## 🛠️ Technical Highlights

- **Rule-based AI** for vehicle decisions and pedestrian interactions.
- **State machines** for driving, stopping, waiting, and crossing.
- **NavMesh navigation** for pedestrian movement.
- **Road-node graphs** for vehicle navigation.
- **Trigger-based sensors** for vehicles, stop zones, and crosswalks.
- **Timers and cooldowns** for signals, conversations, and recovery behaviour.
- **World-space TextMeshPro labels** for names, dialogue, and debugging.

### 📂 Main Scripts

| Script | Purpose |
| --- | --- |
| `CarAgent.cs` | Vehicle movement, stopping rules, node selection, and recovery |
| `CarFrontSensor.cs` | Detects vehicles ahead |
| `RoadNode.cs` | Defines road connections |
| `IntersectionTrafficController.cs` | Controls traffic-light phases |
| `PedestrianAgent.cs` | Handles pedestrian roaming and navigation |
| `PedestrianCrossing.cs` | Manages waiting and crossing states |
| `CrosswalkZone.cs` | Tracks crossing occupancy and nearby cars |
| `PedestrianSocial.cs` | Controls conversations and speech bubbles |
| `PedestrianSpawner.cs` | Spawns pedestrians on the NavMesh |
| `CameraMovement.cs` | Implements the free-flying camera |

The simulation scripts are in [`Assets/Scripts`](Assets/Scripts).

---

## 🚀 Running the Project

1. **Clone or download** the repository.
2. Add the project folder in **Unity Hub**.
3. Open it with **Unity 6000.0.57f1**.
4. Allow Unity to import assets and resolve packages.
5. Open **`Assets/Scenes/SampleScene.unity`**.
6. Press **Play** to start the simulation.

The project uses **AI Navigation 2.0.8** and **Universal Render Pipeline 17.0.4**. If you modify the walkable environment, update the pedestrian NavMesh to match.

---

## 🧠 Project Focus

This project explores how relatively simple agent behaviours can interact to create an active city environment. It demonstrates **C# programming, navigation, state management, environmental detection, and coordination between different agent types**.

The implemented city behaviours are rule-based. Vehicle routes currently use random neighbour selection; the included road-segment cost system is not connected to route selection.
