# Roadmap
Each release will come with my test world that I use. I'm not a programmer but I'm even more not a writer. But there
will be a *game*.

Wishlist:
- How do I create json schemas for people to make their own stuff.

## Release Track 0.x
### 0.1 This runs and plays
- Scenes
    - **DONE** ~~Movement between scenes and worldspaces~~
- Items
    - Basic Interaction with world (go here push this door unlocks or unhides)
        - **DONE** ~~Player: Search for Item in room~~
        - Player: Use Item
        - **DONE** ~~Look at things~~
    - **DONE** ~~Inventory~~
        - **DONE** ~~Box Class (all inventories are boxes)~~
    - Item functionality
        - **DONE** ~~Item Class~~
        - Player: ~~Take~~, Give, ~~Drop~~, Place Item
    - **DONE** ~~Boxscope~~
- Functionality
    - **DONE** ~~Loading world data from JSON to object instances~~
    - **DONE** ~~Full command parser~~
    - Cleanup Program > Game > Prompt logic
- Narration
    - Fix summary displays (capitalize in the game.narration method?)

### 0.2 This is a game
- Scenes
    - Vertical Movement
        - Each Level is a separate Map object that is loaded when world loads
        - World Class (no not *world class* but a World class) that holds all maps
        - Exits can join two points on two different maps somehow
- Items
    - Basic Puzzles
    - Key Items
        - Special Item Class that can be used to unlock a door or chest or w/e\
- NPCs
    - Basic dialogue with Yes/No/Ok/Cancel options
        - NPC Class
        - Conversation "Scene" Class
        - Display Conversation "Entry" text
        - Exits are player answers. 
- UI
    - Main Menu
        - Not a lot to put in there at this point but it'd be nice to launch not directly to the game
- Functionality
    - Gamestate: Player doing a thing triggers a flag that does another thing.

### 0.3 This is not a bad game
- Scenes
    - Overworld movement
    - Dynamic Information
- Functionality
    - Dynamic narration method
    - Dynamic non-entity narration data loaded from localization json files.

## 0.4 This doesn't hurt my eyes
- UI
    - Text Display methods (wrapping, emphasis for topics and items).
- Functionality
    - Saving and Loading
    - Local Files (i hate appdata i hate appdata)

## Release Track 1.x
### 1.1 Player Update
- Player Character
    - Character Creation
    - RPG Stats
    - Class and Combat Framework
- NPCs
    - Quests

### 1.2 World Update
- NPCs
    - Topic-based conversation tree. 
    - Non-Violent Conflict and Contest
- Items
    - More complex item interaction (puzzles, etc)

### 1.3 Combat Update
- Items
    - Equipment
    - Weapons and Armor
- NPCs
    - Combat
- Player Character
    - Combat Stats
    - Classes


## Unplanned
- Functionality
    - Difficulty Levels
- GUI Terminal
