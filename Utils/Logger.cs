using GameClient.FormRelated;

public static class Logger
{
    private static ConsoleLogForm consoleLogForm;

    public static void Initialize(ConsoleLogForm form)
    {
        consoleLogForm = form;
        consoleLogForm.FormClosed += (s, e) => consoleLogForm = null; // Set to null when closed
        consoleLogForm.Show();
    }

    public static void Log(string message)
    {
        if (consoleLogForm == null || consoleLogForm.IsDisposed)
        {
            Console.WriteLine("Log form is not available. Message: " + message);
            return; // Avoid further processing
        }

        if (consoleLogForm.InvokeRequired)
        {
            consoleLogForm.Invoke(new Action(() => consoleLogForm.LogMessage(System.DateTime.Now + ": INVOKED " + message)));
        }
        else
        {
            consoleLogForm.LogMessage(System.DateTime.Now+ ": " + message);
        }
    }

    public static void ToggleConsoleLog()
    {
        if (consoleLogForm == null || consoleLogForm.IsDisposed)
            return;

        if (consoleLogForm.Visible)
        {
            consoleLogForm.Hide();
        }
        else
        {
            consoleLogForm.Show();
        }
    }
}
