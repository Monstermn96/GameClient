namespace GameClient.FormRelated
{
    public partial class ConsoleLogForm : Form
    {
        private TextBox logTextBox;

        public ConsoleLogForm()
        {
            InitializeComponent();
            logTextBox = new TextBox
            {
                Multiline = true,
                Dock = DockStyle.Fill,
                ScrollBars = ScrollBars.Vertical,
                ReadOnly = true
            };
            Controls.Add(logTextBox);
        }

        public void LogMessage(string message)
        {
            if (InvokeRequired)
            {
                Invoke(new Action<string>(LogMessage), message);
            }
            else
            {
                logTextBox.AppendText(message + Environment.NewLine);
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            if (keyData == Keys.F1)
            {
                ToggleConsoleLogForm();
                return true; // Indicate that the key has been handled
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void ToggleConsoleLogForm()
        {
            if (Visible)
            {
                Hide();
            }
            else
            {
                Show();
            }
        }
    }
}
