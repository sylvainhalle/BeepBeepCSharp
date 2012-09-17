using System.Collections;
using System.IO;

public class SimpleFileWriter
{
	/**
   	 * Writes a file from the contents of a String
   	 * @param filename The name of the file.
   	 * @return The String to write to the file.
   	 */
	public static void writeFile(string filename, string contents)
	{
        FileStream fs = null;
        BufferedStream bs = null;
        StreamWriter sw = null;

        try
        {
            fs = new FileStream(filename, FileMode.Open);

            // Here BufferedInputStream is added for fast writing.
            bs = new BufferedStream(fs);
            sw = new StreamWriter(bs);
            sw.Write(contents);

            // dispose all the resources after using them.
            fs.Close();
            bs.Close();
            sw.Close();
        }

        catch (FileNotFoundException e)
        {
            System.Console.WriteLine(e.ToString());
        }

        catch (IOException e)
        {
            System.Console.WriteLine(e.ToString());
        }
	}
}
