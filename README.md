
# ECC Co-op Class: Brick Breaker (2D)

This repository is used for teaching the ECC co-op class. It is a clone of the classic arcade game Breakout, originally created by [zigurous](https://github.com/zigurous/unity-brick-breaker-tutorial).

## Game Overview
Brick Breaker is a 2D arcade game where the player controls a paddle to bounce a ball and break bricks. The goal is to clear all bricks on the screen to advance to the next level. The player starts with 3 lives and loses a life if the ball falls below the paddle. The game ends when all lives are lost.

## Features
- Multiple levels with increasing difficulty
- Paddle and ball physics using Unity's Rigidbody2D
- Several brick types (weak, strong, unbreakable)
- Power-ups and bonus scoring (expandable)
- ScriptableObjects for game and level configuration
- Prefabs for bricks and game objects
- Customizable controls and difficulty settings
- Visual and audio effects (screen shake, particles, sound)

## Project Structure
- `Assets/Scripts/`: Core game scripts (Ball, Paddle, Brick, GameManager, LevelBuilder, etc.)
- `Assets/Scenes/`: Game levels and test scenes
- `Assets/Prefabs/`: Brick and game object prefabs
- `Assets/Sprites/`: Art assets for ball, paddle, and bricks
- `Assets/ScriptableObjects/`: Data assets for levels and brick types
- `Assets/Physics/`: Physics materials for gameplay

## Teaching Focus
- Unity physics and collision system
- Game state management (score, lives, levels)
- Using prefabs and ScriptableObjects for modular design
- Scene management and level progression
- Customizing controls and difficulty

## Unity Version
- Project Version: **6000.2.2f1** (see `ProjectSettings/ProjectVersion.txt`)

## Resources
- [Original Download](https://github.com/zigurous/unity-brick-breaker-tutorial/archive/refs/heads/main.zip)
- [Original Tutorial Video](https://youtu.be/RYG8UExRkhA)

---
This repo is actively used for in-class demonstrations and student projects. Feel free to fork or clone for your own learning!
