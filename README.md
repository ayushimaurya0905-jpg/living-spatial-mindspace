# living-spatial-mindspace
A 3D persistent spatial workspace where users organize thoughts, memories, and knowledge objects in an interactive virtual environment with an AI companion.
# Living Spatial Mindspace

A first-person 3D spatial knowledge environment built in Unity 6.

## What it is
A personal digital workspace inside a virtual room. Instead of 
opening apps on a flat screen, you walk through a 3D space and 
physically place, arrange, and edit knowledge cards — notes, tasks, 
and ideas — that persist between sessions.

## Features
- First-person navigation (WASD + mouse)
- Three knowledge card types: Note, Task, Idea
- Spatial card placement, movement and rotation
- Full session persistence (JSON save file)
- AI Curator — an autonomous orb that semantically analyses card 
  content and draws glowing connection threads between related ideas
- Main menu with Enter / Continue / Reset / Quit
- Environmental atmosphere: dark spatial room, point light that 
  reacts to AI discoveries, particle effects on card spawn

## Controls
| Key | Action |
|-----|--------|
| WASD | Move |
| Mouse | Look |
| N | Spawn Note |
| T | Spawn Task |
| I | Spawn Idea |
| E | Edit card |
| Escape | Save and close edit / Return to menu |
| G | Grab / release card |
| R | Rotate held card |
| Delete | Delete card |

## AI System
The AI Curator is a floating orb that autonomously scans all card 
text every 30 seconds using word-overlap similarity analysis. It 
identifies semantically related cards and draws glowing purple 
connection threads between them in 3D space. Connections persist 
across sessions and update whenever cards are edited.

## Architecture
See ARCHITECTURE.md for technical details.

## Built with
Unity 6.3 LTS · C# · TextMeshPro · URP
