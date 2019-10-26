using DarkHelmet.Game;
using Sandbox.ModAPI;
using System.Text;
using VRage.Collections;
using System;

namespace DarkHelmet.UI
{
    /// <summary>
    /// Captures text input and allows for backspacing. Special characters are ignored.
    /// </summary>
    public class TextInput
    {
        public string CurrentText
        {
            get { return currentText.ToString(); }
            set { currentText.Clear(); currentText.Append(value); }
        }
        public bool Open { get; set; }

        private readonly StringBuilder currentText;
        private readonly Utils.Stopwatch stopwatch;
        private readonly Func<char, bool> IsFilteredFunc;

        public TextInput(Func<char, bool> IsFilteredFunc = null)
        {
            currentText = new StringBuilder(50);
            stopwatch = new Utils.Stopwatch();
            this.IsFilteredFunc = IsFilteredFunc;
        }

        public void Clear() =>
            currentText.Clear();

        private void Backspace()
        {
            if (Open && currentText.Length > 0)
                currentText.Remove(CurrentText.Length - 1, 1);
        }

        public void HandleInput()
        {
            if (Open)
            {
                ListReader<char> input = MyAPIGateway.Input.TextInput;

                if (SharedBinds.Back.IsPressedAndHeld)
                    Backspace();

                for (int n = 0; n < input.Count; n++)
                {
                    if (input[n] >= ' ' && (IsFilteredFunc == null || IsFilteredFunc(input[n])))
                        currentText.Append(input[n]);
                }
            }
        }
    }
}