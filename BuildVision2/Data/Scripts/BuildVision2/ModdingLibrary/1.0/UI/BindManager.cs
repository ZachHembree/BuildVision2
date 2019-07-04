using DarkHelmet.Game;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using VRage.Input;

namespace DarkHelmet.UI
{
    /// <summary>
    /// Stores data for serializing individual key binds to XML.
    /// </summary>
    [XmlType(TypeName = "Bind")]
    public struct KeyBindData
    {
        [XmlAttribute]
        public string name;

        [XmlArray("Controls")]
        public string[] controlNames;

        public KeyBindData(string name, string[] controlNames)
        {
            this.name = name;
            this.controlNames = controlNames;
        }
    }

    /// <summary>
    /// Interface for anything used as a control
    /// </summary>
    public interface IControl
    {
        string Name { get; }
        bool IsPressed { get; }
        bool Analog { get; }
    }

    /// <summary>
    /// Key bind interface
    /// </summary>
    public interface IKeyBind
    {
        string Name { get; }

        /// <summary>
        /// True if any controls in the bind are marked analog. For these types of binds, IsPressed == IsNewPressed.
        /// </summary>
        bool Analog { get; }

        /// <summary>
        /// Returns the binds controls as a list of control names in a string.
        /// </summary>
        string BindString { get; }

        /// <summary>
        /// The set of all controls associated with a given key bind.
        /// </summary>
        IControl[] Combo { get; }

        /// <summary>
        /// Events triggered whenever their corresponding booleans are true.
        /// </summary>
        event Action OnPressed, OnNewPress, OnPressAndHold, OnRelease;

        /// <summary>
        /// True if just released; any larger bind with conflicting controls will supersede it if pressed.
        /// </summary>
        bool IsPressed { get; }

        /// <summary>
        /// True on new press and if held for more than 500ms.
        /// </summary>
        bool IsPressedAndHeld { get; }

        /// <summary>
        /// True if just released; any larger bind with conflicting controls will supersede it if pressed.
        /// </summary>
        bool IsNewPressed { get; }

        /// <summary>
        /// True if just released; any larger bind with conflicting controls will supersede it if pressed.
        /// </summary>
        bool IsReleased { get; }

        /// <summary>
        /// Returns bind name and the name of the controls used. 
        /// </summary>
        KeyBindData GetKeyBindData();
    }

    /// <summary>
    /// Manages custom keybinds; singleton
    /// </summary>
    public sealed class BindManager : ModBase.ComponentBase
    {
        private static BindManager Instance
        {
            get { Init(); return instance; }
            set { instance = value; }
        }
        private static BindManager instance;
        private static readonly Dictionary<string, Control> controls;
        private static readonly List<Control> controlList;
        private const int maxBindLength = 3;
        private const long holdTime = TimeSpan.TicksPerMillisecond * 500;

        private readonly List<Group> bindGroups;

        static BindManager()
        {
            controlList = new List<Control>(220);
            controls = new Dictionary<string, Control>();
            GetControls();
        }

        private BindManager()
        {
            bindGroups = new List<Group>();
        }

        private static void Init()
        {
            if (instance == null)
                instance = new BindManager();
        }

        public override void HandleInput()
        {
            foreach (Group group in bindGroups)
                group.HandleInput();
        }

        public override void Close()
        {
            foreach (Group group in bindGroups)
                group.UnregisterBinds();

            Instance = null;
        }

        /// <summary>
        /// Retrieves a copy of the list of all registered groups.
        /// </summary>
        public static Group[] GetBindGroups()
        {
            Group[] currentGroups = new Group[Instance.bindGroups.Count];
            Instance.bindGroups.CopyTo(currentGroups);

            return currentGroups;
        }

        public static Group GetBindGroup(string name)
        {
            foreach (Group group in Instance.bindGroups)
            {
                if (group.name == name)
                    return group;
            }

            return null;
        }

        /// <summary>
        /// Builds dictionary of controls from the set of MyKeys enums and a couple custom controls for the mouse wheel.
        /// </summary>
        private static void GetControls()
        {
            controlList.Add(new Control("MousewheelUp",
                () => MyAPIGateway.Input.DeltaMouseScrollWheelValue() > 0, true));
            controlList.Add(new Control("MousewheelDown",
                () => MyAPIGateway.Input.DeltaMouseScrollWheelValue() < 0, true));

            controls.Add("mousewheelup", controlList[0]);
            controls.Add("mousewheeldown", controlList[1]);

            foreach (MyKeys seKey in Enum.GetValues(typeof(MyKeys)))
            {
                Control con = new Control(seKey);
                string name = con.Name.ToLower();

                if (!controls.ContainsKey(name))
                {
                    controls.Add(name, con);
                    controlList.Add(con);
                }
            }
        }

        /// <summary>
        /// Returns all controls as a string with each control separated by a line break.
        /// </summary>
        public static string GetControlListString()
        {
            StringBuilder sb = new StringBuilder();

            foreach (KeyValuePair<string, Control> pair in controls)
                sb.AppendLine(pair.Key);

            return sb.ToString();
        }

        /// <summary>
        /// Contains a set of keybinds independent of other groups. While a group's own binds cannot conflict with oneanother,
        /// binds from other groups can.
        /// </summary>
        public class Group
        {
            public readonly string name;

            public IKeyBind this[int index] { get { return keyBinds[index]; } }
            public IKeyBind this[string name] { get { return GetBindByName(name); } }
            public int Count { get { return keyBinds.Count; } }

            private List<Bind> keyBinds;
            private List<Control> usedControls;
            private bool[,] controlBindMap; // X = used controls; Y = associated key binds
            private bool[] bindsBeingReleased;
            private int[] bindHits;
            private readonly int index;

            public Group(string name)
            {
                this.name = name;
                keyBinds = new List<Bind>();

                index = Instance.bindGroups.Count;
                Instance.bindGroups.Add(this);

                foreach (Control con in controlList)
                {
                    while (con.usedIndex.Count <= index)
                    {
                        con.usedIndex.Add(-1);
                        con.usedCount.Add(0);
                    }

                    con.usedIndex[index] = -1;
                    con.usedCount[index] = 0;
                }
            }

            /// <summary>
            /// Updates bind presses each time its called. Key binds will not work if this isn't being run.
            /// </summary>
            public void HandleInput()
            {
                if (keyBinds.Count > 0)
                {
                    int bindsPressed;

                    if (controlBindMap == null || (controlBindMap.GetLength(0) != usedControls.Count) || (controlBindMap.GetLength(1) != keyBinds.Count))
                    {
                        UpdateControlsInUse();
                        UpdateBindMap();
                    }

                    bindsPressed = GetPressedBinds();

                    if (bindsPressed > 1)
                        DisambiguatePresses();

                    for (int n = 0; n < keyBinds.Count; n++)
                        keyBinds[n].UpdatePress(bindHits[n] > 0);
                }
            }

            /// <summary>
            /// Finds and counts number of pressed key binds.
            /// </summary>
            private int GetPressedBinds()
            {
                int bindsPressed = 0;

                if (bindHits == null || bindHits.Length != keyBinds.Count)
                {
                    bindHits = new int[keyBinds.Count];
                    bindsBeingReleased = new bool[keyBinds.Count];
                }

                for (int n = 0; n < bindHits.Length; n++)
                    bindHits[n] = 0;

                for (int x = 0; x < usedControls.Count; x++)
                {
                    if (usedControls[x].IsPressed)
                        for (int y = 0; y < keyBinds.Count; y++)
                        {
                            if (controlBindMap[x, y])
                                bindHits[y]++;
                        }
                }

                // Partial presses on previously pressed binds count as full presses.
                for (int y = 0; y < keyBinds.Count; y++)
                {
                    if ((bindHits[y] > 0 && bindHits[y] < keyBinds[y].Count) && keyBinds[y].IsPressed)
                    {
                        bindHits[y] = keyBinds[y].Count;
                        bindsBeingReleased[y] = true;
                    }
                    else if (bindsBeingReleased[y] && bindHits[y] == keyBinds[y].Count)
                        bindHits[y] = 0;

                    if (bindHits[y] == keyBinds[y].Count)
                        bindsPressed++;
                    else
                    {
                        bindHits[y] = 0;
                        bindsBeingReleased[y] = false;
                    }
                }

                return bindsPressed;
            }

            /// <summary>
            /// Resolves conflicts between pressed binds with shared controls.
            /// </summary>
            private void DisambiguatePresses()
            {
                int controlHits, first, longest;

                // If more than one pressed bind shares the same control, the longest
                // binds take precedence. Any binds shorter than the longest will not
                // be counted as being pressed.
                for (int x = 0; x < usedControls.Count; x++)
                {
                    first = -1;
                    controlHits = 0;
                    longest = GetLongestBindPressForControl(x);

                    for (int y = 0; y < keyBinds.Count; y++)
                    {
                        if (controlBindMap[x, y] && bindHits[y] > 0 && (keyBinds[y].Count < longest))
                        {
                            if (controlHits > 0)
                                bindHits[y]--;
                            else if (controlHits == 0)
                                first = y;

                            controlHits++;
                        }
                    }

                    if (controlHits > 0)
                        bindHits[first]--;
                }
            }

            /// <summary>
            /// Determines the length of the longest bind pressed for a given control on the bind map.
            /// </summary>
            private int GetLongestBindPressForControl(int control)
            {
                int longest = 0;

                for (int y = 0; y < keyBinds.Count; y++)
                {
                    if (bindHits[y] > 0 && keyBinds[y].Count > longest && controlBindMap[control, y])
                        longest = keyBinds[y].Count;
                }

                return longest;
            }

            /// <summary>
            /// Unregisters controls from each bind and clears their even subscribers.
            /// </summary>
            public void UnregisterBinds()
            {
                foreach (Bind bind in keyBinds)
                {
                    bind.ClearSubscribers();
                    bind.UnregisterControls();
                }
            }

            private void RegisterControls()
            {
                foreach (Bind bind in keyBinds)
                    bind.RegisterControls();
            }

            private void UnregisterControls()
            {
                foreach (Bind bind in keyBinds)
                    bind.UnregisterControls();
            }

            /// <summary>
            /// Attempts to create a set of empty key binds given only the names of the binds.
            /// </summary>
            public void RegisterBinds(string[] bindNames, bool silent = false)
            {
                bool areAnyDuplicated = false;

                foreach (string name in bindNames)
                    if (!TryRegisterBind(name, true))
                        areAnyDuplicated = true;

                if (areAnyDuplicated && !silent)
                    ModBase.SendChatMessage("Some of the binds supplied contain duplicates or already existed.");
            }

            /// <summary>
            /// Attempts to add a create a new key bind with a given name.
            /// </summary>
            public bool TryRegisterBind(string bindName, bool silent = false)
            {
                if (!DoesBindExist(bindName))
                {
                    keyBinds.Add(new Bind(bindName, index));

                    return true;
                }
                else if (!silent)
                    ModBase.SendChatMessage($"Bind {bindName} already exists.");

                return false;
            }

            /// <summary>
            /// Attempts to create a set of key binds given the name of the bind and the controls names for that bind.
            /// </summary>
            public void RegisterBinds(KeyBindData[] binds, bool silent = false)
            {
                bool areAnyDuplicated = false;

                foreach (KeyBindData bind in binds)
                    if (!TryRegisterBind(bind, true))
                        areAnyDuplicated = true;

                if (areAnyDuplicated && !silent)
                    ModBase.SendChatMessage("Some of the bind names supplied contain duplicates or were added previously.");
            }

            /// <summary>
            /// Attempts to create a new <see cref="IKeyBind"/> from <see cref="KeyBindData"/> and add it to the bind list.
            /// </summary>
            public bool TryRegisterBind(KeyBindData bind, bool silent = false)
            {
                if (!DoesBindExist(bind.name))
                {
                    keyBinds.Add(new Bind(bind.name, index));

                    if (bind.controlNames != null)
                        TryUpdateBind(bind.name, bind.controlNames, silent);

                    return true;
                }
                else if (!silent)
                    ModBase.SendChatMessage($"Bind {bind.name} already exists.");

                return false;
            }

            /// <summary>
            /// Replaces current keybind configuration with a new one based on the given KeyBindData. Will not register new binds.
            /// </summary>
            public bool TryUpdateBinds(KeyBindData[] bindData)
            {
                List<Bind> originalBinds;
                bool bindError = false;

                if (bindData != null && bindData.Length > 0)
                {
                    UnregisterControls();
                    originalBinds = keyBinds;
                    keyBinds = new List<Bind>(keyBinds.Count);

                    foreach (Bind bind in originalBinds)
                        keyBinds.Add(new Bind(bind.Name, index));

                    foreach (KeyBindData bind in bindData)
                    {
                        if (!TryUpdateBind(bind.name, bind.controlNames, true))
                        {
                            bindError = true;
                            break;
                        }
                    }

                    if (bindError)
                    {
                        ModBase.SendChatMessage("One or more keybinds in the given configuration were invalid or conflict with oneanother.");
                        UnregisterControls();

                        keyBinds = originalBinds;
                        RegisterControls();
                        UpdateControlsInUse();
                        UpdateBindMap();

                        return false;
                    }
                    else
                    {
                        for (int n = 0; n < originalBinds.Count; n++)
                            originalBinds[n].TransferSubscribers(keyBinds[n]);

                        return true;
                    }
                }
                else
                {
                    ModBase.SendChatMessage("Bind data cannot be null or empty.");
                    return false;
                }
            }

            /// <summary>
            /// Tries to update a key bind using the name of the key bind and the names of the controls to be bound. Case sensitive.
            /// </summary>
            public bool TryUpdateBind(string bindName, string[] controlNames, bool silent = false)
            {
                if (controlNames.Length <= maxBindLength && controlNames.Length > 0)
                {
                    string[] uniqueControls = controlNames.GetUnique();
                    Bind bind = GetBindByName(bindName) as Bind;
                    Control[] newCombo;

                    if (bind != null)
                    {
                        if (TryGetCombo(uniqueControls, out newCombo))
                        {
                            if (!DoesComboConflict(newCombo, bind))
                            {
                                bind.UpdateCombo(newCombo);
                                UpdateControlsInUse();
                                UpdateBindMap();

                                return true;
                            }
                            else if (!silent)
                                ModBase.SendChatMessage($"Invalid bind for {bindName}. One or more of the given controls conflict with existing binds.");
                        }
                        else if (!silent)
                            ModBase.SendChatMessage($"Invalid bind for {bindName}. One or more control names were not recognised.");
                    }
                }
                else if (!silent)
                {
                    if (controlNames.Length > 0)
                        ModBase.SendChatMessage($"Invalid key bind. No more than {maxBindLength} keys in a bind are allowed.");
                    else
                        ModBase.SendChatMessage("Invalid key bind. There must be at least one control in a key bind.");
                }

                return false;
            }

            /// <summary>
            /// Tries to create a key combo from a list of control names.
            /// </summary>
            private bool TryGetCombo(IList<string> controlNames, out Control[] newCombo)
            {
                Control con;
                newCombo = new Control[controlNames.Count];

                for (int n = 0; n < controlNames.Count; n++)
                    if (controls.TryGetValue(controlNames[n].ToLower(), out con))
                        newCombo[n] = con;
                    else
                        return false;

                return true;
            }

            /// <summary>
            /// Finds all controls associated with a key bind.
            /// </summary>
            private void UpdateControlsInUse()
            {
                int count = 0;
                usedControls = new List<Control>(11);

                foreach (Control con in controlList)
                    if (con.usedCount[index] > 0)
                    {
                        usedControls.Add(con);
                        con.usedIndex[index] = count;
                        count++;
                    }
            }

            /// <summary>
            /// Associates each control with a key bind on a 2D bool array.
            /// </summary>
            private void UpdateBindMap()
            {
                controlBindMap = new bool[usedControls.Count, keyBinds.Count];

                for (int x = 0; x < usedControls.Count; x++)
                    for (int y = 0; y < keyBinds.Count; y++)
                        controlBindMap[x, y] = keyBinds[y].UsesControl(x);
            }

            /// <summary>
            /// Returns true if a keybind with the given name exists.
            /// </summary>
            public bool DoesBindExist(string name)
            {
                foreach (Bind bind in keyBinds)
                    if (bind.Name == name)
                        return true;

                return false;
            }

            public IKeyBind[] GetKeyBinds()
            {
                return keyBinds.ToArray() as IKeyBind[];
            }

            /// <summary>
            /// Retrieves key bind using its index.
            /// </summary>
            public IKeyBind GetBindByIndex(int index) =>
                keyBinds[index];

            /// <summary>
            /// Retrieves key bind using its name.
            /// </summary>
            public IKeyBind GetBindByName(string name)
            {
                name = name.ToLower();

                foreach (Bind bind in keyBinds)
                    if (bind.Name == name)
                        return bind;

                ModBase.SendChatMessage($"{name} is not a valid bind name.");
                return null;
            }

            /// <summary>
            /// Returns true if all key binds are instantiated and have at least one control.
            /// </summary>
            public bool AreAllBindsInitialized()
            {
                int initCount = 0;

                foreach (Bind bind in keyBinds)
                    if (bind != null && bind.Count > 0)
                        initCount++;

                return initCount == keyBinds.Count;
            }

            /// <summary>
            /// Retrieves the set of key binds as an array of KeyBindData
            /// </summary>
            public KeyBindData[] GetBindData()
            {
                KeyBindData[] bindData = new KeyBindData[keyBinds.Count];

                for (int n = 0; n < keyBinds.Count; n++)
                    bindData[n] = keyBinds[n].GetKeyBindData();

                return bindData;
            }

            /// <summary>
            /// Determines if given combo is equivalent to any existing binds.
            /// </summary>
            private bool DoesComboConflict(IControl[] newCombo, IKeyBind exception = null)
            {
                int matchCount;

                for (int n = 0; n < keyBinds.Count; n++)
                    if (keyBinds[n] != exception && keyBinds[n].Count == newCombo.Length)
                    {
                        matchCount = 0;

                        foreach (Control con in newCombo)
                            if (BindUsesControl(n, con))
                                matchCount++;
                            else
                                break;

                        if (matchCount == newCombo.Length)
                            return true;
                    }

                return false;
            }

            /// <summary>
            /// Determines whether or not a bind with a given index uses a given control.
            /// </summary>
            private bool BindUsesControl(int bind, Control con) =>
                (con.usedCount[index] > 0 && con.usedIndex[index] >= 0 && controlBindMap[con.usedIndex[index], bind]);
        }

        /// <summary>
        /// General purpose button wrapper for MyKeys and anything else associated with a name and an IsPressed method.
        /// </summary>
        private class Control : IControl
        {
            public string Name { get; }
            public bool IsPressed { get { return isPressedFunc(); } }
            public bool Analog { get; }

            public readonly List<int> usedCount, usedIndex;
            private readonly Func<bool> isPressedFunc;

            public Control(MyKeys seKey, bool Analog = false) : this(seKey.ToString(), () => MyAPIGateway.Input.IsKeyPress(seKey), Analog)
            { }

            public Control(string name, Func<bool> IsPressed, bool Analog = false)
            {
                Name = name;
                usedCount = new List<int>();
                usedIndex = new List<int>();

                isPressedFunc = IsPressed;
                this.Analog = Analog;
            }
        }

        /// <summary>
        /// Logic and data for individual keybinds
        /// </summary>
        private class Bind : IKeyBind
        {
            // Interface properties
            public string Name { get; private set; }
            public bool Analog { get; private set; }
            public string BindString { get { return GetBindString(); } }
            public IControl[] Combo { get { return combo; } }
            public bool IsPressed { get; private set; }
            public bool IsNewPressed { get { return IsPressed && (!wasPressed || Analog); } }
            public bool IsPressedAndHeld { get { return isPressedAndHeld || IsNewPressed; } }
            public bool IsReleased { get { return !IsPressed && wasPressed; } }

            public event Action OnPressed, OnNewPress, OnPressAndHold, OnRelease;
            public int Count { get { return combo != null ? combo.Length : int.MinValue; } }

            private Control[] combo;
            private long lastTime;
            private readonly int groupIndex;
            private bool isPressedAndHeld, wasPressed;

            public Bind(string Name, int groupIndex, Control[] combo = null)
            {
                this.Name = Name;
                this.combo = combo;
                this.groupIndex = groupIndex;

                lastTime = long.MaxValue;
                isPressedAndHeld = false;
                wasPressed = false;

                if (combo != null)
                {
                    Analog = AreAnyAnalog(); //mix and match analog controls at your own peril
                    RegisterControls();
                }
                else
                    Analog = false;
            }

            /// <summary>
            /// Updates control combination; unregisters old controls and registers the new ones.
            /// </summary>
            public void UpdateCombo(Control[] newCombo)
            {
                if (combo != null) UnregisterControls();
                combo = newCombo;
                Analog = AreAnyAnalog();

                RegisterControls();
            }

            /// <summary>
            /// Used to update the key bind with each tick of the Binds.Update function. 
            /// </summary>
            public void UpdatePress(bool isPressed)
            {
                wasPressed = IsPressed;
                IsPressed = isPressed;

                if (IsPressed)
                {
                    isPressedAndHeld = (DateTime.Now.Ticks >= lastTime + holdTime);
                    OnPressed?.Invoke();
                }
                else
                {
                    lastTime = long.MaxValue;
                    isPressedAndHeld = false;
                }

                if (IsNewPressed)
                {
                    OnNewPress?.Invoke();
                    lastTime = DateTime.Now.Ticks;
                }

                if (IsReleased)
                    OnRelease?.Invoke();

                if (IsPressedAndHeld)
                    OnPressAndHold?.Invoke();
            }

            /// <summary>
            /// Checks if given control is used in the bind.
            /// </summary>
            public bool UsesControl(int b)
            {
                if (combo != null)
                {
                    foreach (Control a in combo)
                        if (a.usedIndex[groupIndex] == b)
                            return true;
                }

                return false;
            }

            /// <summary>
            /// Returns current key bind configuration as key bind data.
            /// </summary>
            /// <returns></returns>
            public KeyBindData GetKeyBindData()
            {
                if (combo != null)
                {
                    string[] controlNames = new string[combo.Length];

                    for (int n = 0; n < combo.Length; n++)
                        controlNames[n] = combo[n].Name;

                    return new KeyBindData(Name, controlNames);
                }
                else
                    return new KeyBindData(Name, new string[] { "bindIsNull" });
            }

            /// <summary>
            /// Determines if any control in the bind is analog.
            /// </summary>
            public bool AreAnyAnalog()
            {
                foreach (Control con in combo)
                    if (con.Analog)
                        return true;

                return false;
            }

            /// <summary>
            /// Builds string of the names of controls in the bind.
            /// </summary>
            public string GetBindString()
            {
                string conString, bindString = "";

                if (combo != null)
                {
                    for (int n = 0; n < combo.Length; n++)
                    {
                        if (n > 0)
                            bindString += " + ";

                        conString = combo[n].Name;
                        bindString += conString == null ? "nullKey" : conString;
                    }
                }
                else
                    bindString = "comboIsNull";

                return bindString;
            }

            /// <summary>
            /// Transfers subscribers from this bind to a new bind.
            /// </summary>
            public void TransferSubscribers(Bind newBind)
            {
                newBind.OnPressed = OnPressed;
                newBind.OnNewPress = OnNewPress;
                newBind.OnPressAndHold = OnPressAndHold;
                newBind.OnRelease = OnRelease;

                ClearSubscribers();
            }

            public void ClearSubscribers()
            {
                OnPressed = null;
                OnNewPress = null;
                OnPressAndHold = null;
                OnRelease = null;
            }

            public void RegisterControls()
            {
                if (combo != null)
                {
                    foreach (Control con in combo)
                        con.usedCount[groupIndex]++;
                }
            }

            public void UnregisterControls()
            {
                if (combo != null)
                {
                    foreach (Control con in combo)
                        con.usedCount[groupIndex]--;
                }
            }
        }
    }
}