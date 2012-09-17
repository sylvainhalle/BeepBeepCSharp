using System.Collections;
using System.Collections.Generic;

public class CommandLineProcessor
{
    /**
     * Processes an array of Strings (intended as a list of command line
     * arguments) and returns it as Map of type "argument name"&nbsp;&rarr;&nbsp;"value".
     * @param args
     * @return The map
     */
    public static Dictionary<string, string> toMap(string[] args)
    {
        string argname = "", argvalue = "";
        Dictionary<string, string> outmap = new Dictionary<string, string>();

        for (int i = 0; i < args.Length; i++)
        {
            string arg = args[i];

            if (arg.StartsWith("-"))
            {
                if (argname != "")
                {
                    // This is a new argument; put old one in map
                    outmap.Add(argname, argvalue);
                    argvalue = "";
                }

                argname = arg;
            }

            else
            {
                argvalue = arg;
            }
        }

        outmap.Add(argname, argvalue);

        return outmap;
    }
}
