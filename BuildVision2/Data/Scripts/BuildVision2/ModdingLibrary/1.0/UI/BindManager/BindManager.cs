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
    public interface IBind
    {
        string Name { get; }

        /// <summary>
        /// True if any controls in the bind are marked analog. For these types of binds, IsPressed == IsNewPressed.
        /// </summary>
        bool Analog { get; }

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
    }

    /// <summary>
    /// Manages custom keybinds; singleton
    /// </summary>
    public sealed class BindManager : ModBase.ComponentBase
    {
        public const int maxBindLength = 3;

        private static BindManager Instance
        {
            get { Init(); return instance; }
            set { instance = value; }
        }
        private static BindManager instance;
        private static readonly Control[] controls;
        private static readonly Dictionary<string, Control> controlDictionary;
        private static readonly List<MyKeys> controlBlacklist;
        private const long holdTime = TimeSpan.TicksPerMillisecond * 500;

        private readonly List<Group> bindGroups;

        static BindManager()
        {
            controlBlacklist = new List<MyKeys>()
            {
                MyKeys.LeftAlt,
                MyKeys.RightAlt,
                MyKeys.LeftShift,
                MyKeys.RightShift,
                MyKeys.LeftControl,
                MyKeys.RightControl,
                MyKeys.LeftWindows,
                MyKeys.RightWindows,
                MyKeys.CapsLock
            };

            controlDictionary = new Dictionary<string, Control>();
            controls = GenerateControls();
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
                group.ClearBindSubscribers();

            foreach (Control con in controls)
                con.registeredBinds.Clear();

            Instance = null;
        }

        /// <summary>
        /// Returns a copy of the control array.
        /// </summary>
        public static IControl[] GetControls() =>
            controls.Clone() as IControl[];

        public static IControl GetControlByName(string name) =>
            controlDictionary[name];

        /// <summary>
        /// Retrieves a copy of the list of all registered groups.
        /// </summary>
        public static Group[] GetBindGroups()
        {
            Group[] currentGroups = new Group[Instance.bindGroups.Count];
            Instance.bindGroups.CopyTo(currentGroups);

            return currentGroups;
        }

        /// <summary>
        /// Retrieves a bind group using its name.
        /// </summary>
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
        private static Control[] GenerateControls()
        {
            List<Control> controlList = new List<Control>(220);

            controlList.Add(new Control("MousewheelUp",
                () => MyAPIGateway.Input.DeltaMouseScrollWheelValue() > 0, true));
            controlList.Add(new Control("MousewheelDown",
                () => MyAPIGateway.Input.DeltaMouseScrollWheelValue() < 0, true));

            controlDictionary.Add("mousewheelup", controlList[0]);
            controlDictionary.Add("mousewheeldown", controlList[1]);

            foreach (MyKeys seKey in Enum.GetValues(typeof(MyKeys)))
            {
                if (!controlBlacklist.Contains(seKey))
                {
                    Control con = new Control(seKey);
                    string name = con.Name.ToLower();

                    if (!controlDictionary.ContainsKey(name))
                    {
                        controlDictionary.Add(name, con);
                        controlList.Add(con);
                    }
                }
            }

            return controlList.ToArray();
        }

        /// <summary>
        /// Returns all controls as a string with each control separated by a line break.
        /// </summary>
        public static string GetControlListString()
        {
            StringBuilder sb = new StringBuilder();

            foreach (Control control in controls)
                sb.AppendLine(control.Name);

            return sb.ToString();
        }

        /// <summary>
        /// Contains a set of keybinds independent of other groups and determines when/if those binds can be pressed. 
        /// While a group's own binds cannot conflict with oneanother, binds in other groups may.
        /// </summary>
        public class Group
        {
            public readonly string name;

            public IBind this[int index] { get { return keyBinds[index]; } }
            public IBind this[string name] { get { return GetBindByName(name); } }
            public int Count { get { return keyBinds.Count; } }

            private readonly int groupIndex;
            private readonly List<Bind> keyBinds;
            private List<Control> usedControls;
            private List<List<Bind>> bindMap; // X = used controls; Y = associated key binds

            public Group(string name)
            {
                this.name = name;
                keyBinds = new List<Bind>(10);
                usedControls = new List<Control>(10);
                bindMap = new List<List<Bind>>(10);

                groupIndex = Instance.bindGroups.Count;
                Instance.bindGroups.Add(this);

                foreach (Control con in controls)
                {
                    while (con.registeredBinds.Count <= groupIndex)
                        con.registeredBinds.Add(new List<Bind>());
                }
            }

            public void ClearBindSubscribers()
            {
                foreach (Bind bind in keyBinds)
                    bind.ClearSubscribers();
            }

            /// <summary>
            /// Updates bind presses each time its called. Key binds will not work if this isn't being run.
            /// </summary>
            public void HandleInput()
            {
                if (keyBinds.Count > 0)
                {
                    int bindsPressed;

                    bindsPressed = GetPressedBinds();

                    if (bindsPressed > 1)
                        DisambiguatePresses();

                    for (int n = 0; n < keyBinds.Count; n++)
                        keyBinds[n].UpdatePress(keyBinds[n].bindHits == keyBinds[n].length);
                }
            }

            /// <summary>
            /// Finds and counts number of pressed key binds.
            /// </summary>
            private int GetPressedBinds()
            {
                int bindsPressed = 0;

                foreach (Bind bind in keyBinds)
                    bind.bindHits = 0;

                for (int x = 0; x < usedControls.Count; x++)
                {
                    if (usedControls[x].IsPressed)
                    {
                        foreach (Bind bind in usedControls[x].registeredBinds[groupIndex])
                            bind.bindHits++;
                    }
                }

                // Partial presses on previously pressed binds count as full presses.
                foreach (Bind bind in keyBinds)
                {
                    if ((bind.bindHits > 0 && bind.bindHits < bind.length) && bind.IsPressed)
                    {
                        bind.bindHits = bind.length;
                        bind.beingReleased = true;
                    }
                    else if (bind.beingReleased && bind.bindHits == bind.length)
                        bind.bindHits = 0;

                    if (bind.bindHits == bind.length)
                        bindsPressed++;
                    else
                    {
                        bind.bindHits = 0;
                        bind.beingReleased = false;
                    }
                }

                return bindsPressed;
            }

            /// <summary>
            /// Resolves conflicts between pressed binds with shared controls.
            /// </summary>
            private void DisambiguatePresses()
            {
                Bind first;
                int controlHits, longest;

                // If more than one pressed bind shares the same control, the longest
                // binds take precedence. Any binds shorter than the longest will not
                // be counted as being pressed.
                for (int x = 0; x < usedControls.Count; x++)
                {
                    first = null;
                    controlHits = 0;
                    longest = GetLongestBindPressForControl(usedControls[x]);

                    foreach (Bind bind in usedControls[x].registeredBinds[groupIndex])
                    {
                        if (bind.bindHits > 0 && (bind.length < longest))
                        {
                            if (controlHits > 0)
                                bind.bindHits--;
                            else if (controlHits == 0)
                                first = bind;

                            controlHits++;
                        }
                    }

                    if (controlHits > 0)
                        first.bindHits--;
                }
            }

            /// <summary>
            /// Determines the length of the longest bind pressed for a given control on the bind map.
            /// </summary>
            private int GetLongestBindPressForControl(Control con)
            {
                int longest = 0;

                foreach (Bind bind in con.registeredBinds[groupIndex])
                {
                    if (bind.bindHits > 0 && bind.length > longest)
                        longest = bind.length;
                }

                return longest;
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
                    keyBinds.Add(new Bind(bindName));

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
            /// Attempts to create a new <see cref="IBind"/> from <see cref="KeyBindData"/> and add it to the bind list.
            /// </summary>
            public bool TryRegisterBind(KeyBindData bind, bool silent = false)
            {
                if (!DoesBindExist(bind.name))
                {
                    keyBinds.Add(new Bind(bind.name));

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
                List<Control> oldUsedControls;
                List<List<Bind>> oldBindMap;
                bool bindError = false;

                if (bindData != null && bindData.Length > 0)
                {
                    oldUsedControls = usedControls;
                    oldBindMap = bindMap;

                    UnregisterControls();
                    usedControls = new List<Control>(bindData.Length);
                    bindMap = new List<List<Bind>>(bindData.Length);

                    foreach (KeyBindData bind in bindData)
                        if (!TryUpdateBind(bind.name, bind.controlNames, true))
                        {
                            bindError = true;
                            break;
                        }

                    if (bindError)
                    {
                        ModBase.SendChatMessage("One or more keybinds in the given configuration were invalid or conflict with oneanother.");
                        UnregisterControls();

                        usedControls = oldUsedControls;
                        bindMap = oldBindMap;
                        ReregisterControls();

                        return false;
                    }
                    else
                        return true;
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
                                RegisterBindToCombo(bind, newCombo);
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
                    if (controlDictionary.TryGetValue(controlNames[n].ToLower(), out con))
                        newCombo[n] = con;
                    else
                        return false;

                return true;
            }

            private void ReregisterControls()
            {
                for (int n = 0; n < usedControls.Count; n++)
                    usedControls[n].registeredBinds[groupIndex] = bindMap[n];
            }

            private void UnregisterControls()
            {
                foreach (Control con in usedControls)
                    con.registeredBinds[groupIndex] = new List<Bind>();
            }

            /// <summary>
            /// Unregisters a given bind from its current key combination and registers it to a
            /// new one.
            /// </summary>
            private void RegisterBindToCombo(Bind bind, Control[] newCombo)
            {
                UnregisterBindFromCombo(bind);

                foreach (Control con in newCombo)
                {
                    List<Bind> registeredBinds = con.registeredBinds[groupIndex];

                    if (registeredBinds.Count == 0)
                    {
                        usedControls.Add(con);
                        bindMap.Add(registeredBinds);
                    }

                    registeredBinds.Add(bind);

                    if (con.Analog)
                        bind.Analog = true;
                }

                bind.length = newCombo.Length;
            }

            /// <summary>
            /// Unregisters a bind from its key combo if it has one.
            /// </summary>
            private void UnregisterBindFromCombo(Bind bind)
            {
                for (int n = 0; n < usedControls.Count; n++)
                {
                    List<Bind> registeredBinds = usedControls[n].registeredBinds[groupIndex];
                    registeredBinds.Remove(bind);

                    if (registeredBinds.Count == 0)
                    {
                        bindMap.Remove(registeredBinds);
                        usedControls.Remove(usedControls[n]);
                    }
                }

                bind.Analog = false;
                bind.length = 0;
            }

            /// <summary>
            /// Returns true if a keybind with the given name exists.
            /// </summary>
            public bool DoesBindExist(string name)
            {
                name = name.ToLower();

                foreach (Bind bind in keyBinds)
                    if (bind.Name.ToLower() == name)
                        return true;

                return false;
            }

            /// <summary>
            /// Returns an array of all binds currently registered in this group.
            /// </summary>
            public IBind[] GetKeyBinds() =>
                keyBinds.ToArray() as IBind[];

            /// <summary>
            /// Retrieves key bind using its index.
            /// </summary>
            public IBind GetBindByIndex(int index) =>
                keyBinds[index];

            /// <summary>
            /// Retrieves key bind using its name.
            /// </summary>
            public IBind GetBindByName(string name)
            {
                name = name.ToLower();

                foreach (Bind bind in keyBinds)
                    if (bind.Name.ToLower() == name)
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
                    if (bind != null && bind.length > 0)
                        initCount++;

                return initCount == keyBinds.Count;
            }

            public List<string> GetBindControlNames(IBind bind)
            {
                List<string> combo = new List<string>();

                foreach (Control con in usedControls)
                {
                    if (BindUsesControl(bind as Bind, con))
                        combo.Add(con.Name);
                }

                return combo;
            }

            /// <summary>
            /// Retrieves the set of key binds as an array of KeyBindData
            /// </summary>
            public KeyBindData[] GetBindData()
            {
                KeyBindData[] bindData = new KeyBindData[keyBinds.Count];
                List<string>[] combos = new List<string>[keyBinds.Count];

                for (int n = 0; n < keyBinds.Count; n++)
                    combos[n] = GetBindControlNames(keyBinds[n]);

                for (int n = 0; n < keyBinds.Count; n++)
                    bindData[n] = new KeyBindData(keyBinds[n].Name, combos[n].ToArray());

                return bindData;
            }

            /// <summary>
            /// Determines if given combo is equivalent to any existing binds.
            /// </summary>
            public bool DoesComboConflict(IList<IControl> newCombo, IBind exception = null)
            {
                int matchCount;

                for (int n = 0; n < keyBinds.Count; n++)
                    if (keyBinds[n] != exception && keyBinds[n].length == newCombo.Count)
                    {
                        matchCount = 0;

                        foreach (Control con in newCombo)
                            if (BindUsesControl(keyBinds[n], con))
                                matchCount++;
                            else
                                break;

                        if (matchCount == newCombo.Count)
                            return true;
                    }

                return false;
            }

            /// <summary>
            /// Determines whether or not a bind with a given index uses a given control.
            /// </summary>
            private bool BindUsesControl(Bind bind, Control con) =>
                con.registeredBinds[groupIndex].Contains(bind);
        }

        /// <summary>
        /// General purpose button wrapper for MyKeys and anything else associated with a name and an IsPressed method.
        /// </summary>
        private class Control : IControl
        {
            public string Name { get; }
            public bool IsPressed { get { return isPressedFunc(); } }
            public bool Analog { get; }

            public readonly List<List<Bind>> registeredBinds;
            private readonly Func<bool> isPressedFunc;

            public Control(MyKeys seKey, bool Analog = false) : this(seKey.ToString(), () => MyAPIGateway.Input.IsKeyPress(seKey), Analog)
            { }

            public Control(string name, Func<bool> IsPressed, bool Analog = false)
            {
                Name = name;
                registeredBinds = new List<List<Bind>>();

                isPressedFunc = IsPressed;
                this.Analog = Analog;
            }
        }

        /// <summary>
        /// Logic and data for individual keybinds
        /// </summary>
        private class Bind : IBind
        {
            // Interface properties
            public string Name { get; private set; }
            public bool Analog { get; set; }
            public bool IsPressed { get; private set; }
            public bool IsNewPressed { get { return IsPressed && (!wasPressed || Analog); } }
            public bool IsPressedAndHeld { get { return isPressedAndHeld || IsNewPressed; } }
            public bool IsReleased { get { return !IsPressed && wasPressed; } }

            public event Action OnPressed, OnNewPress, OnPressAndHold, OnRelease;
            public int length, bindHits;
            public bool beingReleased;

            private long lastTime;
            private bool isPressedAndHeld, wasPressed;

            public Bind(string Name)
            {
                this.Name = Name;

                lastTime = long.MaxValue;
                isPressedAndHeld = false;
                wasPressed = false;

                bindHits = 0;
                Analog = false;
                beingReleased = false;
                length = 0;
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

                if (IsReleased && !beingReleased)
                    OnRelease?.Invoke();

                if (IsPressedAndHeld)
                    OnPressAndHold?.Invoke();
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
        }
    }
}