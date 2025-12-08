## Rich HUD Framework

A retained-mode GUI framework for creating fully mod-defined UI in C# Space Engineers workshop mods. It enables the creation of scalable, resolution-independent, and fully interactive user interfaces using the game’s billboard system, in lieu of support in the mod API.

>See the [API reference](https://zachhembree.github.io/RichHudFramework.Client/index.html) for detailed overview, targeted examples and supplementary materials.

### Core Features
- Persistent node-based UI tree with automatic depth sorting, input routing, and parent-child hierarchy  
- Full support for both screen-space (2D HUD) and world-space (3D) rendering  
- Custom bitmap text renderer supporting rich-text formatting, automatic wrapping, and runtime-loadable custom fonts  
- Centralized, serializable key-bind system with conflict groups, multiple aliases per bind, and automatic suppression of default game inputs  
- Shared, standardized mod settings terminal with ready-made pages, categories, and controls (sliders, checkboxes, dropdowns, key-rebind panels, rich-text help/changelog pages, etc.)  
- Library of pre-built, composable controls  

### Mod Integration

The framework is split into three modules: **Master, Client, and Shared** because Space Engineers mods are compiled to independent assemblies. Data must be shared at runtime using common types and primitives.

* **Master Module:** The core logic (UI tree, text renderer, settings menu). This is the dependency that must be used on the Steam Workshop when publishing your mod.
* **Client Module:** The developer-facing API. This is the module you directly integrate into your scripted C# mod.
* **Shared Module:** Internal module. Common types and libraries duplicated in the other two modules.

To start, download a copy of the **Client module** from the [releases page](https://zachhembree.github.io/RichHudFramework.Client/Releases.html) and include it in your mod.

### Example Mod

A well-documented example **Text Editor Mod** is available [here](https://github.com/ZachHembree/TextEditorExample).

![Text editor screenshot](https://user-images.githubusercontent.com/6527038/117976888-3ffe4d80-b2fe-11eb-82f2-17c690fec3c5.png)

### Demo 

[Rich HUD Master](https://steamcommunity.com/workshop/filedetails/?id=1965654081) includes a demo built into the terminal. You can enable it using the chat command `"/rhd toggledebug"`. This demo allows you to spawn most of the UI elements in the library in their default state while also allowing you to manipulate their [HUD Space](https://zachhembree.github.io/RichHudFramework.Client/articles/HUD-Spaces.html) in real time.

![Demo menu screenshot](https://steamuserimages-a.akamaihd.net/ugc/1722038154210428899/17BC5D4D245402D3E642B36672DC840D1B7207D3/)