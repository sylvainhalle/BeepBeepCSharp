using System.Collections;
using System.IO;

/**
 * Simple class for reading a file and outputting it to a String.
 * @author shalle
 *
 */
public class SimpleFileReader
{
    /**
     * Reads a file and outputs its content to a String.
     * @param filename The name of the file.
     * @return The String containing the file.
     */
    public static string readFile(string filename)
    {
        FileStream fs = null;
        BufferedStream bs = null;
        StreamReader sr = null;
        string content = "";

        try
        {
            fs = new FileStream(filename, FileMode.OpenOrCreate);

            // Here BufferedInputStream is added for fast reading.
            bs = new BufferedStream(fs);
            sr = new StreamReader(bs);

            string line = "";

            // sr.available() returns 0 if the file does not have more lines.
            while ((line = sr.ReadLine()) != null)
            {
                // this statement reads the line from the file and print it to
                // the console.
                content = content + line + "\n";
            }

            // dispose all the resources after using them.
            fs.Close();
            bs.Close();
            sr.Close();
        }

        catch (FileNotFoundException e)
        {
            System.Console.WriteLine(e.ToString());
        }

        catch (IOException e)
        {
            System.Console.WriteLine(e.ToString());
        }

        return content;
    }
}
