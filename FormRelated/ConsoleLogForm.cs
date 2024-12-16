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
    }
}
