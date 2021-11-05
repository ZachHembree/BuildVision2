## Rich HUD Framework
This is a framework for creating custom retained-mode GUI in Space Engineers workshop mods using the billboards supplied by the mod API. It's nothing fancy, but it has most of the basic functionality you'd expect from a UI framework: text rendering, custom fonts, UI layering, key binds, mouse input, everything you might need to get started creating custom GUI in Space Engineers, sans markup lanugage. Everything is done in C#. If you want an idea of what that looks like, exactly, then have a look at some of the elements in the UI [library](https://github.com/ZachHembree/RichHudFramework.Client/blob/master/Shared/UI/HUD/HudElements/ClickableHudElements/Buttons/BorderedButton.cs); most of it's pretty straightforward.

## Getting Started 
Space Engineer's mods are all compiled to independent assemblies with no references between them, for reasons I trust are apparent, but this has the obvious drawback of preventing any two mods from directly sharing code. Fortunately, the game does provide utilities for sharing data at runtime using types exposed by the game's mod API, like generic delegates and tuples. For this reason, this framework is split into three modules: Master, Client and Shared. The master module contains the implementation for things like the text renderer, bind manager, and manages the UI tree, among other things and the Client module provides wrappers and utilities that provide easy access to those systems.

To get started using this framework, you'll need to download a copy of the [client](https://github.com/ZachHembree/RichHudFramework.Client/releases) module from releases and include it in your mod. Usage details can be found in the [wiki](https://github.com/ZachHembree/RichHudFramework.Client/wiki), in addition to a detailed overview of the framework's functions.

## Demo
If you want to get a better idea of what you can do with this framework, [Rich HUD Master](https://steamcommunity.com/workshop/filedetails/?id=1965654081) has a demo built into the terminal that can be enabled using the chat command "/rhd toggledebug". This demo allows you to spawn most of the UI elements in the library in their default state while also allowing you to maniplate their [HUD Space](https://github.com/ZachHembree/RichHudFramework.Client/wiki/HUD-Spaces) in real time. 

![Demo menu screenshot](https://steamuserimages-a.akamaihd.net/ugc/1722038154210428899/17BC5D4D245402D3E642B36672DC840D1B7207D3/)

## Example Mod
You can find a well-documented example Text Editor Mod [here](https://github.com/ZachHembree/TextEditorExample). Fair warning, the corresponding walkthrough in the wiki is currently outdated, and I probably won't get around to finishing it for a while.

![Text editor screenshot](https://user-images.githubusercontent.com/6527038/117976888-3ffe4d80-b2fe-11eb-82f2-17c690fec3c5.png)
