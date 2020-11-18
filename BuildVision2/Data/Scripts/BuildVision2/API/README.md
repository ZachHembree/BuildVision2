## Build Vision API
The Build Vision client API is used to allow other mods limited access to its current state. At the moment the following information can be retrieved:

 - Menu state (Open/Closed)
 - Menu mode (Peek/Control/Copy)
 - Current target block as an IMyTerminalBlock


## Usage
The client for this mod is a self-contained session component; to use it you must place the **BvAPIClient.cs** file from this repo in **/{ModName}/Data/Scripts/{ModName}** and call ```BvApiClient.Init(string modName)``` from your main class. To ensure your client registers properly and that any debug messages generated actually make sense, you must pass in a *unique* string, preferably the name of your mod.

**Example:**
```csharp
using DarkHelmet.BuildVision2;
using VRage.Game;
using VRage.Game.Components;

[MySessionComponentDescriptor(MyUpdateOrder.NoUpdate)]
public sealed class MainModClass : MySessionComponentBase
{
    public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
    {
        BvApiClient.Init(DebugName); // DebugName is provided by MySessionComponentBase
    }
}
```

Once that's done, you can start using the accessor properties provided by the client. The client updates the states of those properties once every frame, so don't expect it to be perfectly in sync.

**Note:** The API accessors will not check to see if the client is registered. If you don't initialize it, the accessors will just return the default value for their respective types.
