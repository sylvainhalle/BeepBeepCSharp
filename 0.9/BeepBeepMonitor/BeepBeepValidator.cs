/******************************************************************************
A simple LTL-FO+ trace validator
Copyright (C) 2009 Sylvain Halle

This program is free software; you can redistribute it and/or modify
it under the terms of the GNU Lesser General Public License as published by
the Free Software Foundation; either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU Lesser General Public License along
with this program; if not, write to the Free Software Foundation, Inc.,
51 Franklin Street, Fifth Floor, Boston, MA 02110-1301 USA.
 ******************************************************************************/
using System.Collections;
using System.IO;
using System.Diagnostics;

/**
 * The BeepBeepValidator is a simple LTL-FO+ trace validator.
 * As a stand-alone command-line application, it can be invoked in the
 * following way:
 * <blockquote>
 * java -jar BeepBeepValidator.jar &lt;tracefile&gt; &lt;propertyfile&gt;
 * </blockquote>
 * where <tt>tracefile</tt> designates a text file containing the sequence
 * of messages to work on, and <tt>propertyfile</tt> is a text file contaning
 * the LTL-FO+ formula to validate on that trace.
 * <p>
 * Some conventions apply to the trace file:
 * <ul>
 * <li>Messages must be formatted in XML</li>
 * <li>The top-level element must be called &lt;trace&gt;</li>
 * <li>Each message must be contained in a &lt;message&gt;...&lt;/message&gt; element</li>
 * <li>For other conventions regarding the format of XML messages, please see
 * {@link ca.uqam.info.xml#XMLDocument}</li>
 * </ul>
 * <p>
 * Some conventions apply to the property file:
 * <ul>
 * <li>The file can only contain one LTL-FO+ formula, formatted
 * following the rules of {@link ca.info.uqam.logic.LTLStringParser}</li>
 * <li>Any extra whitespace is ignored</li>
 * </ul>
 * In the background, the BeepBeepValidator uses exactly the same methods
 * as the {@link BeepBeepMonitor}.  The two differ only in their front-end.
 */
public class BeepBeepValidator
{
	/**
	 * Gets the next message from an input stream. Crudely, this method keeps
	 * reading lines from the stream until it finds one that contains the string
	 * "&lt;/message&gt;". It ignores lines with elements called "&lt;trace&gt;"
	 * or "&lt;/trace&gt;". It returns null if it does not read anything.
	 * 
	 * @param dis
	 *          A data input stream, corresponding to some opened file
	 * @return A string corresponding to the next message, or null if no message
	 *         could be read
	 */
	private static string getNextMessage(BinaryReader br)
	{
		string message = "";
		
		try
		{
			while (br.Read() != 0)
			{
				string line = br.ReadString();
				
				line = line.Trim();
				
				if (line.IndexOf("<Trace>") != -1 || 
					line.IndexOf("</Trace>") != -1)
				{
					continue;
				}
				
				message += line;
				
				if (line.IndexOf("</Event>") != -1)
				{
					break;
				}
			}
		}
		
		catch (IOException o)
		{
			return null;
		}
		
		return message;
	}
	
	/**
	 * @param args
	 */
	public static void main(string[] args)
	{
		FileStream fs = null;
		BufferedStream bs = null;
		BinaryReader br = null;
		long heapsize = 0;
		
		// Prints credits
		System.Console.WriteLine("BeepBeep Trace Validator, version 0.9.3");
		System.Console.WriteLine("(C) 2007-2012 Sylvain Halle, UQAM/UCSB");
		
		if (args.Length < 2)
		{
			System.Console.Error.WriteLine("ERROR: wrong command line parameters");
			
			return;
		}
		
		// Parses arguments
		string inputFilename = args[0];
		string queryFilename = args[1];
		
		// Gets property to check
		string formula = readProperty(queryFilename);
		
		if (formula == null)
		{
			System.Console.Error.WriteLine("ERROR: cannot open " + queryFilename);
			
			return;
		}
		
		// Parses property to check
		Operator property = LTLStringParser.parseFromString(formula);
		
		if (property == null)
		{
			System.Console.Error.WriteLine("ERROR: invalid property");
			
			return;
		}
		
		// Opens file
		//StreamReader traceFile = File.OpenText(inputFilename);
		
		try
		{
			//fs = new FileStream(traceFile, FileAccess.ReadWrite);
			fs = new FileStream(inputFilename, FileMode.Open);
			bs = new BufferedStream(fs);
			br = new BinaryReader(bs);
		}
		
		catch (FileNotFoundException e)
		{
			System.Console.Error.WriteLine("ERROR: cannot open " + inputFilename);
			
			return;
		}
		
		// A: here, br points to a valid, opened trace file
		// Start stopwatch
		long timeBegin = Stopwatch.GetTimestamp();
		SymbolicWatcher w = new SymbolicWatcher();
		
		w.setFormula(property);
		
		// Start processing the trace
		long numMessages = 0;
		string message = getNextMessage(br);
		
		while (message != null)
		{
			numMessages++;
			w.update(message);
			message = getNextMessage(br);
			//heapsize = Mathf.Max(heapsize, RuntimeGenerator.
		}
		
		// Stop stopwatch
		long timeEnd = Stopwatch.GetTimestamp();
		
		// Processing is over, get stats...
		int milliseconds = (int)((timeEnd - timeBegin) / (float)100000);
		string outcome = "INCONCLUSIVE";
		Outcome oOut = w.getOutcome();
		
		if (oOut == Outcome.TRUE)
		{
			outcome = "TRUE";
		}
		
		else if (oOut == Outcome.FALSE)
		{
			outcome = "FALSE";
		}
		
		int maxNodes = w.getMaxNodes();
		int maxSize = w.getMaxSize();
		int maxAtoms = w.getMaxAtoms();
		
		System.Console.WriteLine("Messages:  " + numMessages);
		System.Console.WriteLine("Outcome:   " + outcome);
		System.Console.WriteLine("Max nodes: " + maxNodes);
		System.Console.WriteLine("Max size:  " + maxSize);
		System.Console.WriteLine("Max atoms: " + maxAtoms);
		System.Console.WriteLine("Time (ms): " + milliseconds);
		System.Console.WriteLine("Max heap:  " + heapsize);
		
		try
		{
			if (br != null)
			{
				br.Close();
			}
			
			if (bs != null)
			{
				bs.Close();
			}
			
			if (fs != null)
			{
				fs.Close();
			}
		}
		
		catch (IOException e)
		{
			System.Console.Error.WriteLine("ERROR: while closing trace file");
			
			return;
		}
	}
	
	private static string readProperty(string filename)
	{
		// Opens the file
		string formula = "";
		//File queryFile = File.OpenText(filename);
		
		try
		{
			//FileStream fs = new FileStream(queryFile, FileAccess.ReadWrite);
			FileStream fs = new FileStream(filename, FileMode.Open);
			BufferedStream bs = new BufferedStream(fs);
			BinaryReader br = new BinaryReader(bs);
			
			while (br.Read() != 0)
			{
				formula += br.ReadString();
			}
			
			fs.Close();
			bs.Close();
			br.Close();
		}
		
		catch (FileNotFoundException e)
		{
			return null;
		}
		
		catch (IOException e)
		{
			System.Console.Error.WriteLine(e.ToString());
			
			return null;
		}
		
		return formula;
	}
}
