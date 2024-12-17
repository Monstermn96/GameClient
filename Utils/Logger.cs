using GameClient.FormRelated;

public static class Logger
{
    private static ConsoleLogForm consoleLogForm;

    public static void Initialize(ConsoleLogForm form)
    {
        if (consoleLogForm == null || consoleLogForm.IsDisposed)
        {
            consoleLogForm = form;

            if (consoleLogForm.InvokeRequired)
            {
                consoleLogForm.Invoke(new Action(() => consoleLogForm.Show()));
            }
            else
            {
                consoleLogForm.Show();
            }

            consoleLogForm.FormClosed += (s, e) => consoleLogForm = null;
        }
    }

    public static bool IsInitialized()
    {
        return consoleLogForm != null && !consoleLogForm.IsDisposed;
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
