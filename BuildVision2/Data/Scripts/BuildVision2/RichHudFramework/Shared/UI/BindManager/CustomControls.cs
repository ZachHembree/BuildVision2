using System;
using VRage.Input;
using RichHudFramework.UI.Server;
using RichHudFramework.UI.Client;

namespace RichHudFramework
{
    namespace UI
    {
        /// <summary>
        /// Unified control enum type for keyboard, mouse and gamepad/joystick
        /// </summary>
        public enum RichHudControls : int
        {
            // VRage.Input.MyKeys
            None = 0,
            LeftButton = 1,
            RightButton = 2,
            Cancel = 3,
            MiddleButton = 4,
            ExtraButton1 = 5,
            ExtraButton2 = 6,
            Back = 8,
            Tab = 9,
            Clear = 12,
            Enter = 13,
            Shift = 16,
            Control = 17,
            Alt = 18,
            Pause = 19,
            CapsLock = 20,
            Kana = 21,
            Hangeul = 21,
            Hangul = 21,
            Junja = 23,
            Final = 24,
            Hanja = 25,
            Kanji = 25,
            Ctrl_Y = 25,
            Ctrl_Z = 26,
            Escape = 27,
            Convert = 28,
            NonConvert = 29,
            Accept = 30,
            ModeChange = 31,
            Space = 32,
            PageUp = 33,
            PageDown = 34,
            End = 35,
            Home = 36,
            Left = 37,
            Up = 38,
            Right = 39,
            Down = 40,
            Select = 41,
            Print = 42,
            Execute = 43,
            Snapshot = 44,
            Insert = 45,
            Delete = 46,
            Help = 47,
            D0 = 48,
            D1 = 49,
            D2 = 50,
            D3 = 51,
            D4 = 52,
            D5 = 53,
            D6 = 54,
            D7 = 55,
            D8 = 56,
            D9 = 57,
            A = 65,
            B = 66,
            C = 67,
            D = 68,
            E = 69,
            F = 70,
            G = 71,
            H = 72,
            I = 73,
            J = 74,
            K = 75,
            L = 76,
            M = 77,
            N = 78,
            O = 79,
            P = 80,
            Q = 81,
            R = 82,
            S = 83,
            T = 84,
            U = 85,
            V = 86,
            W = 87,
            X = 88,
            Y = 89,
            Z = 90,

            Apps = 93,
            Sleep = 95,
            NumPad0 = 96,
            NumPad1 = 97,
            NumPad2 = 98,
            NumPad3 = 99,
            NumPad4 = 100,
            NumPad5 = 101,
            NumPad6 = 102,
            NumPad7 = 103,
            NumPad8 = 104,
            NumPad9 = 105,
            Multiply = 106,
            Add = 107,
            Separator = 108,
            Subtract = 109,
            Decimal = 110,
            Divide = 111,
            F1 = 112,
            F2 = 113,
            F3 = 114,
            F4 = 115,
            F5 = 116,
            F6 = 117,
            F7 = 118,
            F8 = 119,
            F9 = 120,
            F10 = 121,
            F11 = 122,
            F12 = 123,
            F13 = 124,
            F14 = 125,
            F15 = 126,
            F16 = 127,
            F17 = 128,
            F18 = 129,
            F19 = 130,
            F20 = 131,
            F21 = 132,
            F22 = 133,
            F23 = 134,
            F24 = 135,
            NumLock = 144,
            ScrollLock = 145,
            NEC_Equal = 146,
            Fujitsu_Jisho = 146,
            Fujitsu_Masshou = 147,
            Fujitsu_Touroku = 148,
            Fujitsu_Loya = 149,
            Fujitsu_Roya = 150,

            BrowserBack = 166,
            BrowserForward = 167,
            BrowserRefresh = 168,
            BrowserStop = 169,
            BrowserSearch = 170,
            BrowserFavorites = 171,
            BrowserHome = 172,
            VolumeMute = 173,
            VolumeDown = 174,
            VolumeUp = 175,
            MediaNextTrack = 176,
            MediaPrevTrack = 177,
            MediaStop = 178,
            MediaPlayPause = 179,
            LaunchMail = 180,
            LaunchMediaSelect = 181,
            LaunchApplication1 = 182,
            LaunchApplication2 = 183,
            OemSemicolon = 186,
            OemPlus = 187,
            OemComma = 188,
            OemMinus = 189,
            OemPeriod = 190,
            OemQuestion = 191,
            OemTilde = 192,
            ChatPadGreen = 202,
            ChatPadOrange = 203,
            OemOpenBrackets = 219,
            OemPipe = 220,
            OemCloseBrackets = 221,
            OemQuotes = 222,
            Oem8 = 223,
            OEMAX = 225,
            OemBackslash = 226,
            ICOHelp = 227,
            ICO00 = 228,
            ProcessKey = 229,
            ICOClear = 230,
            Packet = 231,
            OEMReset = 233,
            OEMJump = 234,
            OEMPA1 = 235,
            OEMPA2 = 236,
            OEMPA3 = 237,
            OEMWSCtrl = 238,
            OEMCUSel = 239,
            OEMATTN = 240,
            OEMFinish = 241,
            OEMCopy = 242,
            OEMAuto = 243,
            OEMENLW = 244,
            OEMBackTab = 245,
            ATTN = 246,
            CRSel = 247,
            EXSel = 248,
            EREOF = 249,
            Play = 250,
            Zoom = 251,
            Noname = 252,
            PA1 = 253,
            OEMClear = 254,

            // Custom controls
            MousewheelUp = 256,
            MousewheelDown = 257,

            // Gamepad/joystick
            LeftStickLeft = 258,
            LeftStickRight = 259,
            LeftStickUp = 260,
            LeftStickDown = 261,

            LeftStickX = 262,
            LeftStickY = 263,

            RightStickLeft = 264,
            RightStickRight = 265,
            RightStickUp = 266,
            RightStickDown = 267,

            RightStickX = 268,
            RightStickY = 269,

            LeftTrigger = 270,
            RightTrigger = 271,

            Slider1 = 272,
            Slider2 = 273,

            ReservedEnd = 288,

            DPadLeft = ReservedEnd + MyJoystickButtonsEnum.JDLeft,
            DPadRight = ReservedEnd + MyJoystickButtonsEnum.JDRight,
            DPadUp = ReservedEnd + MyJoystickButtonsEnum.JDUp,
            DPadDown = ReservedEnd + MyJoystickButtonsEnum.JDDown,

            GpadA = ReservedEnd + MyJoystickButtonsEnum.J01,
            GpadB = ReservedEnd + MyJoystickButtonsEnum.J02,
            GpadX = ReservedEnd + MyJoystickButtonsEnum.J03,
            GpadY = ReservedEnd + MyJoystickButtonsEnum.J04,

            LeftBumper = ReservedEnd + MyJoystickButtonsEnum.J05,
            RightBumper = ReservedEnd + MyJoystickButtonsEnum.J06,

            /// <summary>
            /// Back button on Xbox 360, View button on newer controllers
            /// </summary>
            GpadView = ReservedEnd + MyJoystickButtonsEnum.J07,

            /// <summary>
            /// Start button on 360, Menu button on newer controllers
            /// </summary>
            GpadMenu = ReservedEnd + MyJoystickButtonsEnum.J08,

            LeftStickBtn = ReservedEnd + MyJoystickButtonsEnum.J09,
            RightStickBtn = ReservedEnd + MyJoystickButtonsEnum.J10,
        }

        /// <summary>
        /// Interop container for <see cref="MyKeys"/>, <see cref="RichHudControls"/> and
        /// <see cref="MyJoystickButtonsEnum"/>
        /// </summary>
        public struct ControlHandle
        {
            /// <summary>
            /// Index of the first gamepad key
            /// </summary>
            public const int GPKeysStart = (int)RichHudControls.ReservedEnd + 1;

            /// <summary>
            /// Returns interface to underlying <see cref="IControl"/>
            /// </summary>
            public IControl Control => BindManager.GetControl(this);

            /// <summary>
            /// Returns enum corresponding to the handle
            /// </summary>
            public RichHudControls ControlEnum => (RichHudControls)id;

            /// <summary>
            /// Unique RHF control ID
            /// </summary>
            public readonly int id;

            public ControlHandle(string controlName)
            {
                this.id = BindManager.GetControl(controlName);
            }

            public ControlHandle(int id)
            { 
                this.id = id; 
            }

            public ControlHandle(MyKeys id)
            {
                this.id = (int)id;
            }

            public ControlHandle(IControl con)
            {
                this.id = con.Index;
            }

            public ControlHandle(RichHudControls id)
            {
                this.id = (int)id;
            }

            public ControlHandle(MyJoystickButtonsEnum id)
            {
                this.id = GPKeysStart + (int)id;
            }

            public static explicit operator ControlHandle(int con) 
            {
                return new ControlHandle(con);
            }

            public static implicit operator ControlHandle(string controlName)
            {
                return new ControlHandle(controlName);
            }

            public static implicit operator ControlHandle(MyKeys id)
            {
                return new ControlHandle(id);
            }

            public static implicit operator MyKeys(ControlHandle handle)
            {                    
                var id = (MyKeys)handle.id;
                
                if (Enum.IsDefined(typeof(MyKeys), id))
                    return id;
                else
                {
                    throw new Exception($"ControlHandle index {handle.id} cannot be converted to MyKeys.");
                }
            }

            public static implicit operator ControlHandle(RichHudControls id)
            {
                return new ControlHandle(id);
            }

            public static implicit operator RichHudControls(ControlHandle handle)
            {
                var id = (RichHudControls)handle.id;

                if (Enum.IsDefined(typeof(RichHudControls), id))
                    return id;
                else
                {
                    throw new Exception($"ControlHandle index {handle.id} cannot be converted to RichHudControls.");
                }
            }

            public static implicit operator ControlHandle(MyJoystickButtonsEnum id)
            {
                return new ControlHandle(id);
            }

            public static implicit operator MyJoystickButtonsEnum(ControlHandle handle)
            {
                var id = (MyJoystickButtonsEnum)handle.id;

                if (Enum.IsDefined(typeof(MyJoystickButtonsEnum), id))
                    return id;
                else
                {
                    throw new Exception($"ControlHandle index {handle.id} cannot be converted to MyJoystickButtonsEnum.");
                }
            }

            public static implicit operator int(ControlHandle handle)
            {
                return handle.id;
            }

            public override int GetHashCode()
            {
                return id.GetHashCode();
            }
        }
    }
}