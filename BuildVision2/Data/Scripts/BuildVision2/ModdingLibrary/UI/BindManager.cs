using System.Xml.Serialization;
using System;
using System.Text;
using System.Collections.Generic;
using VRage.Input;
using VRage.Utils;
using Sandbox.ModAPI;
using System.Collections;
using DarkHelmet.Game;

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
        event Action OnPressed, OnNewPress, OnRelease;

        /// <summary>
        /// True if just released; any larger bind with conflicting controls will supersede it if pressed.
        /// </summary>
        bool IsPressed { get; }

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
    public sealed class BindManager : ModBase.Component<BindManager>
    {
        /// <summary>
        /// Returns key bind by index.
        /// </summary>
        public IKeyBind this[int index] { get { return keyBinds[index]; } }

        /// <summary>
        /// Returns key bind by name.
        /// </summary>
        public IKeyBind this[string name] { get { return GetBindByName(name); } }

        /// <summary>
        /// Total number of key binds
        /// </summary>
        public int Count { get { return keyBinds.Count; } }

        private const int maxBindLength = 3;
        private static Dictionary<string, Control> controls;
        private static List<Control> controlList;

        private List<KeyBind> keyBinds;
        private List<Control> usedControls;
        private bool[,] controlBindMap; // X = used controls; Y = associated key binds
        private int[] bindHits;

        /// <summary>
        /// Initializes binds class. Generates control dictionary, list and key binds array.
        /// </summary>
        public BindManager()
        {
            if (controls == null)
            {
                controlList = new List<Control>(220);
                controls = new Dictionary<string, Control>();
                GetControls();
            }

            keyBinds = new List<KeyBind>();
        }

        protected override void BeforeClose()
        {
            foreach (KeyBind bind in keyBinds)
                bind.ClearSubscribers();
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
                keyBinds.Add(new KeyBind(bindName));

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
                keyBinds.Add(new KeyBind(bind.name));

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
        public static bool TryUpdateBinds(KeyBindData[] bindData)
        {
            BindManager newBinds;
            bool bindError = false;

            if (bindData != null && bindData.Length > 0)
            {
                canInstantiate = true;
                newBinds = new BindManager();
                newBinds.keyBinds = new List<KeyBind>(Instance.keyBinds.Count);

                foreach (KeyBind bind in Instance.keyBinds)
                    newBinds.keyBinds.Add(new KeyBind(bind.Name));

                foreach (KeyBindData bind in bindData)
                {
                    if (!newBinds.TryUpdateBind(bind.name, bind.controlNames, true))
                    {
                        bindError = true;
                        break;
                    }
                }

                if (bindError)
                {
                    ModBase.SendChatMessage("One or more keybinds in the given configuration were invalid or conflict with oneanother.");
                    return false;
                }
                else
                {
                    for (int n = 0; n < Instance.keyBinds.Count; n++)
                        Instance.keyBinds[n].TransferSubscribers(newBinds.keyBinds[n]);

                    Instance = newBinds;
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
        public bool TryUpdateBind(string name, string[] controlNames, bool silent = false)
        {
            if (controlNames.Length <= maxBindLength && controlNames.Length > 0)
            {
                List<string> uniqueControls = Utilities.GetUniqueList(controlNames);
                KeyBind bind = GetBindByName(name) as KeyBind;
                Control[] newCombo;

                if (bind != null)
                {
                    if (TryGetCombo(uniqueControls, out newCombo))
                    {
                        if (!DoesComboConflict(newCombo, bind))
                        {
                            bind.UpdateCombo(newCombo);
                            usedControls = GetControlsInUse();
                            controlBindMap = GetBindMap();

                            return true;
                        }
                        else if (!silent)
                            ModBase.SendChatMessage($"Invalid bind for {name}. One or more of the given controls conflict with existing binds.");
                    }
                    else if (!silent)
                        ModBase.SendChatMessage($"Invalid bind for {name}. One or more control names were not recognised.");
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
        /// Builds dictionary of controls from the set of MyKeys enums and a couple custom controls for the mouse wheel.
        /// </summary>
        private static void GetControls()
        {
            string name;

            controlList.Add(new Control("MousewheelUp",
                () => MyAPIGateway.Input.DeltaMouseScrollWheelValue() > 0, true));
            controlList.Add(new Control("MousewheelDown",
                () => MyAPIGateway.Input.DeltaMouseScrollWheelValue() < 0, true));

            foreach (MyKeys seKey in Enum.GetValues(typeof(MyKeys)))
                controlList.Add(new Control(seKey));

            foreach (Control con in controlList)
            {
                name = con.Name.ToLower();

                if (!controls.ContainsKey(name))
                    controls.Add(name, con);
            }
        }

        /// <summary>
        /// Finds all controls associated with a key bind.
        /// </summary>
        private List<Control> GetControlsInUse()
        {
            int count = 0;
            List<Control> usedControls = new List<Control>(11);

            foreach (Control con in controlList)
                if (con.usedCount > 0)
                {
                    usedControls.Add(con);
                    con.usedIndex = count;
                    count++;
                }

            return usedControls;
        }

        /// <summary>
        /// Associates each control with a key bind on a 2D bool array.
        /// </summary>
        private bool[,] GetBindMap()
        {
            bool[,] controlBindMap = new bool[usedControls.Count, keyBinds.Count];

            for (int x = 0; x < usedControls.Count; x++)
                for (int y = 0; y < keyBinds.Count; y++)
                    controlBindMap[x, y] = keyBinds[y].UsesControl(x);

            return controlBindMap;
        }

        public bool DoesBindExist(string name)
        {
            foreach (KeyBind bind in keyBinds)
                if (bind.Name == name)
                    return true;

            return false;
        }

        /// <summary>
        /// Retrieves key bind using its name.
        /// </summary>
        public IKeyBind GetBindByName(string name)
        {
            name = name.ToLower();

            foreach (KeyBind bind in keyBinds)
                if (bind.Name == name)
                    return bind;

            ModBase.SendChatMessage($"{name} is not a valid bind name.");
            return null;
        }

        /// <summary>
        /// Returns all controls as a string with each control separated by a line break.
        /// </summary>
        public static string GetControlListString()
        {
            StringBuilder sb = new StringBuilder();

            foreach (Control con in controlList)
                sb.AppendLine(con.Name);

            return sb.ToString();
        }

        /// <summary>
        /// Returns true if all key binds are instantiated and have at least one control.
        /// </summary>
        public bool AreAllBindsInitialized()
        {
            int initCount = 0;

            foreach (KeyBind bind in keyBinds)
                if (bind != null && bind.Count > 0)
                    initCount++;

            return initCount == keyBinds.Count;
        }

        /// <summary>
        /// Retrieves the set of key binds as an array of KeyBindData
        /// </summary>
        /// <returns></returns>
        public KeyBindData[] GetBindData() 
        {
            KeyBindData[] bindData = new KeyBindData[Count];

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
            (con.usedCount > 0 && con.usedIndex >= 0 && controlBindMap[con.usedIndex, bind]);

        /// <summary>
        /// Updates bind presses each time its called. Key binds will not work if this isn't being run.
        /// </summary>
        protected override void HandleInput()
        {
            if (keyBinds.Count > 0)
            {
                int bindsPressed;

                if (usedControls == null)
                    usedControls = GetControlsInUse();

                if (controlBindMap == null || (controlBindMap.GetLength(0) != usedControls.Count) || (controlBindMap.GetLength(1) != keyBinds.Count))
                    controlBindMap = GetBindMap();

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
                bindHits = new int[keyBinds.Count];

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

            for (int y = 0; y < keyBinds.Count; y++)
            {
                if (keyBinds[y].IsPressed && (bindHits[y] > 0 && bindHits[y] < keyBinds[y].Count))
                    bindHits[y] = int.MaxValue;

                if (bindHits[y] < keyBinds[y].Count)
                    bindHits[y] = 0;
                else
                    bindsPressed++;
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
            // binds take precedence. Shorter binds are decremented each time there
            // is a conflict. If a bind was previously pressed it wont be released
            // until all its controls are.
            for (int x = 0; x < usedControls.Count; x++)
            {
                first = -1;
                controlHits = 0;
                longest = GetLongestBindPressForControl(x);

                for (int y = 0; y < keyBinds.Count; y++)
                {
                    if (bindHits[y] > 0 && (keyBinds[y].Count < longest && controlBindMap[x, y]))
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
                if (bindHits[y] == int.MaxValue)
                    return int.MaxValue;

                if (bindHits[y] > 0 && controlBindMap[control, y]) //if (bind has at least one control press && bind Y uses control X)
                {
                    if (keyBinds[y].Count >= longest)
                        longest = keyBinds[y].Count;
                }
            }

            return longest;
        }

        /// <summary>
        /// General purpose button wrapper for MyKeys and anything else associated with a name and an IsPressed method.
        /// </summary>
        private class Control : IControl
        {
            public string Name { get; }
            public bool IsPressed { get { return isPressedFunc(); } }
            public bool Analog { get; }

            public int usedCount, usedIndex;
            private Func<bool> isPressedFunc;

            public Control(MyKeys seKey, bool Analog = false)
            {
                Name = seKey.ToString();

                isPressedFunc = () => MyAPIGateway.Input.IsKeyPress(seKey);
                this.Analog = Analog;
                usedCount = 0;
                usedIndex = -1;
            }

            public Control(string name, Func<bool> IsPressed, bool Analog = false)
            {
                Name = name;

                isPressedFunc = IsPressed;
                this.Analog = Analog;
                usedCount = 0;
                usedIndex = -1;
            }
        }

        /// <summary>
        /// Logic and data for individual keybinds
        /// </summary>
        private class KeyBind : IKeyBind
        {
            // Interface properties
            public string Name { get; private set; }
            public bool Analog { get; private set; }
            public string BindString { get; private set; }
            public IControl[] Combo { get { return combo?.Clone() as IControl[]; } }
            public bool IsPressed { get; private set; }
            public bool IsNewPressed { get { return IsPressed && (!wasPressed || Analog); } }
            public bool IsReleased { get { return !IsPressed && wasPressed; } }
            public event Action OnPressed, OnNewPress, OnRelease;

            public int Count { get { return combo != null ? combo.Length : int.MinValue; } }
            private Control[] combo;
            private bool wasPressed;

            public KeyBind(string Name, Control[] combo = null)
            {
                this.Name = Name;
                this.combo = combo;
                wasPressed = false;
                BindString = GetBindString();

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

                BindString = GetBindString();
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
                    OnPressed?.Invoke();

                if (IsNewPressed)
                    OnNewPress?.Invoke();

                if (IsReleased)
                    OnRelease?.Invoke();
            }

            internal bool AreAnyControlsPressed()
            {
                foreach (Control control in combo)
                    if (control.IsPressed)
                        return true;

                return false;
            }

            /// <summary>
            /// Checks if given control is used in the bind.
            /// </summary>
            public bool UsesControl(int b)
            {
                if (combo != null)
                {
                    foreach (Control a in combo)
                        if (a.usedIndex == b)
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
            public void TransferSubscribers(KeyBind newBind)
            {
                newBind.OnPressed = OnPressed;
                newBind.OnNewPress = OnNewPress;
                newBind.OnRelease = OnRelease;

                ClearSubscribers();
            }

            public void ClearSubscribers()
            {
                OnPressed = null;
                OnNewPress = null;
                OnRelease = null;
            }

            public void RegisterControls()
            {
                foreach (Control con in combo)
                    con.usedCount++;
            }

            public void UnregisterControls()
            {
                foreach (Control con in combo)
                    con.usedCount--;
            }
        }
    }
}