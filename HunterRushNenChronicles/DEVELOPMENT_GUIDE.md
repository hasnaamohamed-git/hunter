# Hunter Rush: Nen Chronicles - Development Guide

## 🏗️ Architecture Overview

The game is built using a modular architecture with clear separation of concerns:

### Core Systems
- **GameManager**: Central game state management
- **Character System**: Base character class with specific implementations
- **Nen System**: Complete Nen mechanics (Ten, Zetsu, Ren, Hatsu)
- **Combat System**: Anime-accurate combat with combos
- **Movement System**: Wall-running, dashing, enhanced jumping
- **Audio System**: Voice lines, music, and sound effects
- **UI System**: Menus, HUD, and character selection

## 🎯 Character Implementation Guide

### Creating New Characters

1. **Inherit from BaseCharacter**
   ```csharp
   public class NewCharacter : BaseCharacter
   {
       protected override void InitializeCharacter()
       {
           // Set character-specific stats and properties
       }
       
       public override void PerformSpecialAbility()
       {
           // Implement character's unique ability
       }
   }
   ```

2. **Define Character Stats**
   - `runSpeed`: Base movement speed
   - `jumpPower`: Jump strength
   - `attackDamage`: Base attack power
   - `maxHealth`: Health pool
   - `maxNenCapacity`: Nen pool size
   - `auraColor`: Character's aura color

3. **Implement Required Methods**
   - `LightAttack()`: Basic attack
   - `HeavyAttack()`: Powerful attack
   - `ComboAttack()`: Combo system
   - `PerformSpecialAbility()`: Unique ability
   - `PerformHatsuAbility()`: Nen-based ability

### Character-Specific Features

#### Gon Freecss
- **Jajanken System**: Rock/Paper/Scissors charge attacks
- **Enhancement Focus**: High physical stats, lower Nen capacity
- **Determination**: Stat boosts when health is low

#### Killua Zoldyck
- **Lightning Abilities**: Electric attacks and Godspeed mode
- **Assassin Training**: Stealth capabilities and afterimages
- **Speed Focus**: Highest movement speed and agility

#### Kurapika Kurta
- **Chain Abilities**: Five different chain types
- **Emperor Time**: All Nen categories at 100% efficiency
- **Scarlet Eyes**: Massive boosts against Phantom Troupe

#### Leorio Paradinight
- **Emission Abilities**: Remote punch attacks
- **Medical Support**: Healing and buff abilities
- **Tank Role**: Highest health, support-focused

## ⚔️ Combat System Details

### Attack Flow
1. **Input Detection**: Mouse/touch input queued
2. **Animation Trigger**: Character animation plays
3. **Hitbox Activation**: Damage detection sphere
4. **Damage Calculation**: Apply modifiers and effects
5. **Visual Feedback**: Hit effects and screen shake

### Combo System
- **Timing Windows**: 1.5 second combo window
- **Escalating Damage**: Each hit increases damage
- **Character Variety**: Each character has unique combo chains
- **Finisher Abilities**: 4th combo hit triggers special ability

### Nen Integration
- **Ten**: 50% damage reduction when active
- **Ren**: 50% attack boost with aura effects
- **Zetsu**: Stealth mode, reduced detection
- **Hatsu**: Character-specific special abilities

## 🏃‍♂️ Movement System

### Core Mechanics
- **Running**: Base movement with Nen enhancement trails
- **Jumping**: Enhanced with Nen for higher/longer jumps
- **Wall-Running**: Temporary vertical movement
- **Dashing**: Quick evasion with invincibility frames

### Nen Enhancement
- **Speed Trails**: Visible energy trails during enhanced movement
- **Wall-Running**: Requires Nen consumption
- **Enhanced Jumps**: Nen boosts jump power and distance
- **Dash**: Nen-powered quick movement

## 🎵 Audio Implementation

### Voice Line System
```csharp
// Play character-specific voice line
AudioManager.Instance.PlayVoiceLine(CharacterType.Gon, VoiceLineType.BattleCry);
```

### Dynamic Music
- **Context-Aware**: Music changes based on game state
- **Smooth Transitions**: Fade between tracks
- **Boss Music**: Special tracks for boss encounters

### Sound Effects
- **Nen Activation**: Unique sounds for each Nen state
- **Combat Impact**: Satisfying hit sounds with variation
- **Environmental**: Footsteps adapt to surface type

## 🎨 Visual Effects Guide

### Aura System
```csharp
// Create character aura
VisualEffectsManager.Instance.CreateAuraEffect(
    position, 
    character.auraColor, 
    AuraType.Ren, 
    duration
);
```

### Particle Effects
- **Nen Auras**: Character-specific colored particles
- **Combat Effects**: Hit sparks, energy bursts
- **Movement Trails**: Speed lines and energy trails
- **Environmental**: Dust, debris, atmosphere

### Anime-Style Rendering
- **Cel Shading**: Custom shader for anime look
- **Outline Rendering**: Character outlines for clarity
- **Impact Lines**: Manga-style action lines
- **Screen Effects**: Speed lines, camera shake

## 📱 Mobile Optimization

### Performance Targets
- **60 FPS**: Consistent frame rate on mid-range devices
- **Dynamic Resolution**: Automatic quality scaling
- **LOD System**: Distance-based detail reduction
- **Effect Culling**: Disable off-screen particles

### Touch Controls
- **Gesture Recognition**: Swipes for movement, taps for attacks
- **Nen Gestures**: Special patterns for abilities
- **Adaptive UI**: Responsive interface for different screen sizes

### Battery Optimization
- **Efficient Rendering**: Minimize overdraw and state changes
- **Audio Compression**: Optimized audio formats
- **Asset Streaming**: Load content as needed

## 🏆 Game Mode Development

### Adding New Game Modes
1. **Create Mode Script**: Inherit from MonoBehaviour
2. **Define Rules**: Objectives, win conditions, scoring
3. **Setup Scene**: Configure environment and spawning
4. **UI Integration**: Add mode selection and HUD elements

### Existing Modes

#### Story Mode
- **Arc Progression**: Follow anime storyline
- **Cutscenes**: Narrative sequences
- **Character Development**: Unlock abilities
- **Save System**: Progress persistence

#### Endless Run
- **Procedural Generation**: Infinite level creation
- **Difficulty Scaling**: Progressive challenge increase
- **Power-ups**: Temporary ability boosts
- **Leaderboards**: Global score competition

#### Boss Rush
- **Sequential Bosses**: Fight all major villains
- **Perfect Run**: No damage challenges
- **Time Attack**: Speed completion rewards
- **Stat Restoration**: Health/Nen between fights

## 🔧 Development Tools

### Debug Features
- **Character Stats Display**: Real-time stat monitoring
- **Nen State Visualization**: Current Nen state indication
- **Performance Metrics**: FPS, memory usage tracking
- **Level Generation Debug**: Segment visualization

### Testing Utilities
- **Character Switcher**: Quick character testing
- **Ability Tester**: Test all character abilities
- **Level Skipper**: Jump between level segments
- **God Mode**: Invincibility for testing

## 📋 Asset Requirements

### 3D Models
- **Characters**: Gon, Killua, Kurapika, Leorio models
- **Enemies**: Various Hunter x Hunter antagonists
- **Environment**: Iconic locations from the series
- **Props**: Weapons, objects, collectibles

### Animations
- **Character Animations**: Idle, run, jump, attack, special abilities
- **Facial Animations**: Emotion changes, battle expressions
- **Effect Animations**: Aura effects, impact animations
- **UI Animations**: Menu transitions, button effects

### Audio Assets
- **Voice Lines**: Japanese voice actors from anime
- **Music**: Orchestral tracks inspired by anime OST
- **Sound Effects**: Nen activation, combat sounds
- **Ambient Audio**: Environmental atmosphere

### Textures
- **Character Textures**: Anime-accurate character appearance
- **Environment Textures**: Location-specific materials
- **Effect Textures**: Particle and aura textures
- **UI Textures**: Menu backgrounds, icons, buttons

## 🚀 Build Pipeline

### Platform Builds

#### Mobile (iOS/Android)
```bash
# Build for Android
Unity -batchmode -quit -projectPath . -buildTarget Android -executeMethod BuildScript.BuildAndroid

# Build for iOS
Unity -batchmode -quit -projectPath . -buildTarget iOS -executeMethod BuildScript.BuildiOS
```

#### PC/Console
```bash
# Build for Windows
Unity -batchmode -quit -projectPath . -buildTarget Win64 -executeMethod BuildScript.BuildWindows

# Build for macOS
Unity -batchmode -quit -projectPath . -buildTarget OSXUniversal -executeMethod BuildScript.BuildMac
```

### Optimization Steps
1. **Asset Optimization**: Compress textures, optimize models
2. **Code Optimization**: Remove debug code, optimize algorithms
3. **Audio Compression**: Use appropriate audio formats
4. **Build Size**: Minimize final package size

## 🧪 Testing Strategy

### Unit Testing
- **Character Abilities**: Test all special abilities
- **Nen System**: Verify state transitions and costs
- **Combat System**: Validate damage calculations
- **Movement System**: Test physics and collisions

### Integration Testing
- **Scene Transitions**: Verify smooth scene loading
- **Save System**: Test progress persistence
- **Audio Integration**: Verify all audio triggers
- **UI Flow**: Test complete user journey

### Performance Testing
- **Frame Rate**: Maintain 60 FPS target
- **Memory Usage**: Monitor for memory leaks
- **Battery Life**: Test on actual mobile devices
- **Network Performance**: Test multiplayer features

## 📚 Code Style Guide

### Naming Conventions
- **Classes**: PascalCase (e.g., `BaseCharacter`)
- **Methods**: PascalCase (e.g., `PerformAttack`)
- **Variables**: camelCase (e.g., `currentHealth`)
- **Constants**: UPPER_CASE (e.g., `MAX_COMBO_COUNT`)

### Documentation
- **XML Comments**: Document all public methods
- **Code Comments**: Explain complex logic
- **README Updates**: Keep documentation current

### Best Practices
- **Single Responsibility**: Each class has one purpose
- **Dependency Injection**: Use constructor injection
- **Event System**: Loose coupling between systems
- **Performance**: Profile and optimize regularly

## 🐛 Common Issues & Solutions

### Performance Issues
- **Too Many Particles**: Use object pooling
- **High Poly Models**: Implement LOD system
- **Overdraw**: Optimize transparent materials
- **GC Pressure**: Minimize allocations in Update()

### Mobile Issues
- **Touch Sensitivity**: Adjust touch thresholds
- **Screen Sizes**: Test on various resolutions
- **Battery Drain**: Optimize rendering pipeline
- **Memory Crashes**: Implement asset streaming

### Audio Issues
- **Voice Sync**: Ensure subtitle timing matches audio
- **Volume Balance**: Test on different devices
- **Compression**: Balance quality vs file size
- **Latency**: Minimize audio delay on mobile

## 🔮 Future Enhancements

### Planned Features
- **Multiplayer Co-op**: Team up with friends
- **Character Customization**: Unlock alternate costumes
- **Training Modes**: Practice Nen abilities
- **Photo Mode**: Capture epic moments
- **Mod Support**: Community content creation

### Technical Improvements
- **Advanced Shaders**: More sophisticated anime rendering
- **AI Enemies**: Smarter enemy behavior
- **Physics Simulation**: More realistic interactions
- **Cloud Saves**: Cross-platform progress sync

## 📞 Support & Resources

### Documentation
- [Unity Documentation](https://docs.unity3d.com/)
- [Hunter x Hunter Wiki](https://hunterxhunter.fandom.com/)
- [Anime Reference Materials](./docs/anime_reference/)

### Community
- [Discord Server](#)
- [Development Blog](#)
- [GitHub Issues](./issues/)

---

*"The moment someone knows they're about to die, they'll do anything to survive."* - Killua Zoldyck

Happy coding! 🎮⚡