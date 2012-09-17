using System.Collections;
using System.IO;

public class ProcessCommunicator
{
    public static string newline = "\r\n";

    /**
     * Front-end to the real command.
     * @param command The command to execute
     * @param stdin The input that will be passed to stdin
     * @return Whatever has been read from stdout after executing the command
     */
    public static string communicate(string command, string stdin)
    {
        StringWriter p_out = new StringWriter(), p_err = new StringWriter();
        StringReader p_in = new StringReader(stdin);

        return "";
    }
}
