# Roadmap
Each release will come with my test world that I use. I'm not a programmer but I'm even more not a writer. But there
will be a *game*.

## Release Track 0.x
### 0.1 This runs and plays
- Scenes
    - **DONE** ~~Movement between scenes and worldspaces~~
- Items
    - Basic Interaction with world (go here push this door unlocks or unhides)
        - Item Class
        - Player: Search for Item in room
        - Player: Use Item
        - Gamestate: Player doing a thing triggers a flag that does another thing.
        - MAYBE: Basic Puzzles
- Functionality
    - **DONE** ~~Loading world data from JSON to object instances~~
    - **DONE** ~~Full command parser~~

### 0.2 This is a game
- Scenes
    - Vertical Movement
        - Each Level is a separate Map object that is loaded when world loads
        - World Class (no not *world class* but a World class) that holds all maps
        - Exits can join two points on two different maps somehow
    - Key Items
        - Special Item Class that can be used to unlock a door or chest or w/e
- Items
    - Item functionality
        - Item Class
        - Player: Take, Give, Drop, Place Item
    - Inventory
        - Box Class (all inventories are boxes)
    - Boxscope
        - A class that handles items in multiple inventories
        - Playerscope is all inventories on the player (multiple bags and such)
        - Scenescope is all inventories in the scene (on floor or table etc)
        - Localscope is all inventories in player or scene.
        - Yes this sounds way too complicated but it pays off ^^*for me*^^.
- NPCs
    - Basic dialogue with Yes/No/Ok/Cancel options
        - NPC Class
        - Conversation "Scene" Class
        - Display Conversation "Entry" text
        - Exits are player answers. 
- UI
    - Main Menu
        - Not a lot to put in there at this point but it'd be nice to launch not directly to the game

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
