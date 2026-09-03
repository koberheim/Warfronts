# WW2 Tower Defense

A WW2-themed isometric tower defense game built with Phaser.js 3 and TypeScript.

## Features

- **Isometric pixel art visual style**
- **Allied forces vs Axis forces** - Play as US, Britain, France, and Canada against Germany, Italy, and Japan
- **Strategic tower placement** - Choose from multiple tower types with unique abilities
- **Wave-based gameplay** - Survive increasingly difficult enemy waves
- **Multiple maps** - Battle across different WW2-themed battlefields

## Technology Stack

- **Phaser.js 3** - 2D game framework
- **TypeScript** - Type-safe game development
- **Vite** - Fast build tool with hot module reloading

## Getting Started

### Prerequisites

- Node.js 18+ and npm

### Installation

```bash
# Install dependencies
npm install

# Start development server
npm run dev

# Build for production
npm run build

# Preview production build
npm run preview
```

## Development

The game is organized into phases:

1. **Phase 1** - Foundation (Phaser setup, isometric grid)
2. **Phase 2** - Core Entities (towers, enemies, projectiles)
3. **Phase 3** - Managers and Systems (game logic, economy, waves)
4. **Phase 4** - Maps and Gameplay (first playable level)
5. **Phase 5** - UI and HUD (menus, displays)
6. **Phase 6** - Content Expansion (more towers, enemies, maps)
7. **Phase 7** - Polish and Audio (sound, effects)
8. **Phase 8** - Balance and Testing (playtesting, optimization)
9. **Phase 9** - Electron Packaging (Steam-ready desktop app)

## Project Structure

```
src/
├── main.ts                 # Entry point
├── scenes/                 # Phaser scenes
├── entities/               # Game objects (towers, enemies)
├── managers/               # Game logic managers
├── systems/                # Core systems (grid, targeting, waves)
├── ui/                     # UI components
├── config/                 # Configuration files
├── data/                   # JSON data files
├── utils/                  # Helper functions
└── types/                  # TypeScript types
```

## License

MIT
