# Hunter Rush: Nen Chronicles

A 3D endless runner/action game capturing the exact movement, combat, and character essence of Hunter x Hunter anime with fluid transitions between running, fighting, and exploration.

## 🎮 Game Overview

**Hunter Rush: Nen Chronicles** is an anime-accurate 3D action game featuring the beloved characters from Hunter x Hunter. Experience fluid combat, Nen abilities, and iconic locations from the series in an endless runner format with multiple game modes.

### ✨ Key Features

- **Authentic Characters**: Play as Gon, Killua, Kurapika, and Leorio with their exact anime abilities
- **Nen System**: Complete implementation of Ten, Zetsu, Ren, and Hatsu mechanics
- **Fluid Combat**: Anime-style combat with combos, special abilities, and character-specific movesets
- **Dynamic Movement**: Wall-running, enhanced jumps, and lightning-fast dashes
- **Multiple Game Modes**: Story Mode, Endless Run, Boss Rush, and PvP Arena
- **Iconic Locations**: Hunter Association HQ, Heaven's Arena, Yorknew City, and more

## 🎯 Character Roster

### Gon Freecss
- **Movement**: Enhanced leg strength, agile jumps, wall-running
- **Combat**: Rock-Paper-Scissors (Jajanken) combo system
- **Special**: Jajanken Rock/Paper/Scissors with charge mechanics
- **Voice**: "I can do it!" "Jajanken!" battle cries

### Killua Zoldyck
- **Movement**: Lightning-speed dashes, afterimage effects, silent steps
- **Combat**: Yo-yo weapons, electric discharge, claw strikes
- **Special**: Godspeed mode, Thunderbolt attacks
- **Voice**: Cool, confident combat quips

### Kurapika Kurta
- **Movement**: Chain-swinging traversal, graceful steps
- **Combat**: Chain-based attacks, binding enemies, defensive stances
- **Special**: Emperor Time, Judgement Chain, Scarlet Eyes
- **Voice**: Vengeful determination, especially against Phantom Troupe

### Leorio Paradinight
- **Movement**: Determined running, briefcase as tool
- **Combat**: Remote punches through portals, medical support
- **Special**: Healing aura, emergency medical abilities
- **Voice**: Mix of comedic and serious moments

## 🎨 Visual Style

- **Art Direction**: Madhouse Studio animation style with clean lines and vibrant colors
- **Nen Effects**: Character-specific colored auras (Gon=green, Killua=blue, etc.)
- **Combat Feedback**: Manga-style impact lines, speed effects, energy bursts
- **Animations**: Anime-accurate expressions and battle poses

## 🏗️ Project Structure

```
HunterRushNenChronicles/
├── Assets/
│   ├── Scripts/
│   │   ├── Characters/          # Character implementations
│   │   ├── Combat/              # Combat system
│   │   ├── Movement/            # Movement mechanics
│   │   ├── Nen/                 # Nen ability system
│   │   ├── Audio/               # Audio management
│   │   ├── UI/                  # User interface
│   │   ├── Levels/              # Level generation
│   │   ├── GameModes/           # Different game modes
│   │   ├── Effects/             # Visual effects
│   │   └── Managers/            # Core managers
│   ├── Prefabs/                 # Game object prefabs
│   ├── Materials/               # Shaders and materials
│   ├── Textures/                # Character and environment textures
│   ├── Audio/                   # Music, SFX, and voice lines
│   ├── Scenes/                  # Game scenes
│   └── Animations/              # Character animations
└── ProjectSettings/             # Unity project configuration
```

## 🚀 Getting Started

### Prerequisites
- Unity 2022.3.21f1 or later
- Mobile development modules (for iOS/Android builds)
- Version control system (Git recommended)

### Setup Instructions

1. **Clone the Project**
   ```bash
   git clone <repository-url>
   cd HunterRushNenChronicles
   ```

2. **Open in Unity**
   - Launch Unity Hub
   - Click "Open" and select the project folder
   - Wait for Unity to import all assets

3. **Configure Build Settings**
   - Go to File > Build Settings
   - Select your target platform (PC, iOS, or Android)
   - Click "Switch Platform"

4. **Install Dependencies**
   - The project uses Unity's built-in packages
   - Additional packages will be automatically imported

### 🎮 Controls

#### PC Controls
- **WASD**: Movement
- **Space**: Jump
- **Mouse**: Camera control
- **Left Click**: Light attack
- **Right Click**: Heavy attack/Special ability
- **Q**: Character special ability
- **E**: Nen state toggle
- **Shift**: Sprint/Block
- **Ctrl**: Dash

#### Mobile Controls
- **Touch & Drag**: Movement
- **Tap**: Attack
- **Swipe Up**: Jump
- **Swipe Down**: Block/Slide
- **Hold**: Charge special abilities
- **Gestures**: Nen ability activation

## 🎯 Game Modes

### Story Mode
Follow the Hunter x Hunter anime arcs with cutscenes and character progression:
- Hunter Exam Arc
- Yorknew City Arc
- Greed Island Arc
- Chimera Ant Arc

### Endless Run
Infinite runner with increasing difficulty:
- Procedurally generated levels
- Dynamic difficulty scaling
- Power-ups and collectibles
- Global leaderboards

### Boss Rush
Fight all major villains consecutively:
- Hisoka, Phantom Troupe, Chimera Ants
- Perfect run challenges
- Time attack mode
- Unlockable boss characters

### PvP Arena
Real-time multiplayer battles:
- 1v1 and team battles
- Character-specific strategies
- Ranked matchmaking
- Tournament mode

## 🔧 Development Features

### Nen System Implementation
- **Ten**: Defensive aura with damage reduction
- **Zetsu**: Stealth mode with suppressed aura
- **Ren**: Attack enhancement with visible aura
- **Hatsu**: Character-specific special abilities

### Combat Mechanics
- Combo system with timing windows
- Counter-attacks and perfect blocks
- Environmental interactions
- Character-specific movesets

### Level Generation
- Procedural segment creation
- Theme-based environments
- Dynamic difficulty adjustment
- Seamless transitions

## 📱 Mobile Optimization

- **Performance**: 60 FPS target with dynamic resolution scaling
- **Controls**: Intuitive touch gestures for all abilities
- **UI**: Mobile-optimized interface with large touch targets
- **Battery**: Efficient rendering and effect management

## 🎵 Audio Design

- **Voice Acting**: Japanese voice actors from the anime
- **Sound Effects**: Nen activation sounds, combat impacts
- **Music**: Orchestral soundtrack inspired by the anime OST
- **Spatial Audio**: 3D positioned sound effects

## 🏆 Progression System

### Character Development
- Nen capacity increases
- New ability unlocks
- Technique mastery improvements
- Alternate costumes and aura effects

### Achievements
- Story completion milestones
- Combat performance awards
- Collection achievements
- Perfect run challenges

## 🛠️ Technical Specifications

- **Engine**: Unity 3D 2022.3.21f1
- **Target Platforms**: iOS, Android, PC, Console
- **Graphics**: Anime shader system with cel-shading
- **Audio**: 3D spatial audio with voice line system
- **Networking**: Multiplayer support for co-op and PvP

## 📋 Development Status

- ✅ Project Structure Setup
- ✅ Character System (Gon, Killua, Kurapika, Leorio)
- ✅ Nen System Implementation
- ✅ Movement System with Wall-Running
- ✅ Combat System with Combos
- ✅ Audio Management System
- ✅ UI System with Character Selection
- ✅ Level Generation System
- ✅ Game Modes (Story, Endless, Boss Rush)
- 🔄 Visual Effects System
- ⏳ Asset Creation (Models, Textures, Audio)
- ⏳ Animation System
- ⏳ Mobile Controls Implementation
- ⏳ Multiplayer Networking

## 🤝 Contributing

This project follows the Hunter x Hunter anime faithfully. When contributing:

1. Maintain anime accuracy in character abilities and personalities
2. Follow the established code architecture
3. Test on both mobile and PC platforms
4. Ensure performance optimization for mobile devices

## 📄 License

This project is for educational and portfolio purposes. Hunter x Hunter is owned by Yoshihiro Togashi and Shueisha.

## 🎌 Credits

- **Original Creator**: Yoshihiro Togashi
- **Animation Studio**: Madhouse (2011 anime adaptation)
- **Game Development**: Hunter Rush Studio
- **Engine**: Unity Technologies

---

*"The moment someone knows they're about to die, they'll do anything to survive. You'll become both the strongest and ugliest creature in the world."* - Killua Zoldyck