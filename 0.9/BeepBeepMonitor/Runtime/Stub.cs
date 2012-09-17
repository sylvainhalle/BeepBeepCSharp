/******************************************************************************
  Runtime monitors for message-based workflows
  Copyright (C) 2008-2010 Sylvain Halle
  
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
using System.Collections.Generic;
using System.Text;

/**
 * Implementation of a web service stub.
 * <p>
 * The usage is as follows:
 * <ol>
 * <li>Instantiate the stub by passing a {@link RuntimeGenerator} to it</li>
 * <li>Configure the stub using {@link readConfiguration()}</li>
 * </ol>
 * @author shalle
 * @version 2010-12-12
 *
 */
public class Stub
{
    /**
     * Runtime generator used internally to produce the messages
     */
    private RuntimeGenerator m_generator;
	 
	/**
	 * Keeps the soundness state of the stub
	 */
	private bool m_sound = true;
	 
	/**
	 * If this flag is set to false, the stub will reply with a
	 * message even when the generator cannot guarantee that it
	 * is sound. Defaults to true.
	 */
	private bool m_mustBeSound = true;
	 
	/**
	 * The list of paths defined in all the possible messages.
	 */
	private HashSet<string> m_paths = new HashSet<string>();
	 
	/**
	 * An assignment of a set of constants to each message path,
	 * effectively defining the domains for each message element.
	 */
	private Dictionary<string, HashSet<Constant>> m_domains = new Dictionary<string, HashSet<Constant>>();
	 
	/**
	 * Tells the stub whether the messages sent must be sound or not.
	 * @param b True if messages must be sound, false otherwise.
	 */
	public void setMustBeSound(bool b)
	{
	    m_mustBeSound = b;
	}
	 
	/**
	 * Builds an instance of a web service stub, using the
	 * {@link RuntimeGenerator} provided
	 * @param gen
	 */
	 
	public Stub(RuntimeGenerator gen, string configuration) : base()
	{
	    m_generator = gen;
	    readConfiguration(configuration);
	    m_generator.reset();
	}
	 
	/**
	 * Parses a string representation of the stub's configuration.
	 * The configuration is formatted as follows:
	 * <pre>
	 * # A pound sign stands for a comment line
	 * # Blank lines and leading whitespace are ignored
	 * 
	 * MESSAGES
	 *   message[element,
	 *     element[elem, elem]
	 *     ...];
	 *   otherMessage[...];
	 *   
	 * DOMAINS
	 *   element1 : value1, value2, ..., valuen;
	 *   element2, ..., elementn : value1, value2, ..., valuen;
	 *   ...
	 *  
	 * SPECS
	 *   LTL-FO+ formula 1;
	 *   ...
	 *   LTL-FO+ formula n;
	 * </pre>
	 * The syntax for LTL-FO+ formul&aelig; is that defined by the
	 * {@link ca.uqam.info.logic.LTLStringParser}.
	 * @param configuration
	 */
	private void readConfiguration(string configuration)
	{
        HashSet<string> formulae = new HashSet<string>();

        configuration.Replace("\r\n|\r", "\n");

		char[] split = null;
		split[0] = '\\';
		split[1] = 'n';
		
        string[] lines = configuration.Split(split);
        StringBuilder curLine = new StringBuilder();
        string lineToProcess = "";
        int toProcess = 0; // 0 = nothing, 1 = messages, 2 = domains, 3 = specs

        foreach (string line in lines)
        {
            string line2 = line.Trim();

            if (line2.StartsWith("#"))
            {
                // Comment line, ignore
                continue;
            }

            if (line2.StartsWith("MESSAGES"))
            {
                toProcess = 1;
                continue;
            }

            if (line2.StartsWith("DOMAINS"))
            {
                toProcess = 2;
                continue;
            }

            if (line2.StartsWith("SPECS"))
            {
                toProcess = 3;
                continue;
            }

            curLine.Append(" " + line2);

            if (!line2.EndsWith(";"))
            {
                continue;
            }

            lineToProcess = curLine.ToString().Trim();

            if (lineToProcess.EndsWith(";"))
            {
                lineToProcess = lineToProcess.Substring(0, lineToProcess.Length - 1);
            }

            curLine = new StringBuilder();

            switch (toProcess)
            {
                case 0: //assert(false);
                    break;

                case 1: processMessage(lineToProcess);
                    break;

                case 2: processDomain(lineToProcess);
                    break;

                case 3: formulae.Add(lineToProcess);
                    break;

                default: break;
            }
        }

        // We are done
        Operator o = instantiateFormula(formulae);
        m_generator.setFormula(o);
	}

    public string generate()
    {
        string sOut = "";

        try
        {
            sOut = m_generator.generate();
        }

        catch (LossOfSoundnessException e)
        {
            m_sound = false;

            if (m_mustBeSound)
            {
                return "";
            }

            sOut = m_generator.generateUnsound();
        }

        return sOut;
    }

    public bool isSound()
    {
        return m_sound;
    }

    /**
     * Parses all the fetched formul&aelig; and puts the domains
     * @param formulae
     * @return
     */
    private Operator instantiateFormula(HashSet<string> formulae)
    {
        BinaryOperator big = new OperatorAnd();
        bool first = true;

        foreach (string s in formulae)
        {
            Operator o = LTLStringParser.parseFromString(s, m_domains);

            if (formulae.Count == 1)
            {
                return o;
            }

            if (first)
            {
                first = false;
                big.setLeftOperand(o);
                continue;
            }

            if (big.getRightOperand() == null)
            {
                big.setRightOperand(o);
            }

            else
            {
                BinaryOperator newop = new OperatorAnd();

                newop.setLeftOperand(big);
                newop.setRightOperand(o);
                big = newop;
            }
        }

        return big;
    }

    /**
     * Generates a set of LTL-FO+ constraints from the definition of a message
     * schema.
     * @param message
     */
    private void processMessage(string message)
    {
        HashSet<string> paths = getPathsComma(message);

        // Add leading slash to paths
        foreach (string p in paths)
        {
            m_paths.Add("/" + p);
        }
    }

    private void processDomain(string domain)
    {
        string[] parts = domain.Split(':');

        if (parts.Length != 2)
        {
            //assert(false); // Malformed expression
        }

        string pathPart = parts[0];
        string domainPart = parts[1];
        string[] vars = pathPart.Split(',');
        string[] domainValues = domainPart.Split(',');

        // Process domain
        HashSet<Constant> domainSet = new HashSet<Constant>();

        foreach (string valueString in domainValues)
        {
            domainSet.Add(new Constant(valueString.Trim()));
        }

        // Assigns domain to each variable in list
        foreach (string pathString in vars)
        {
            string pathString2 = pathString.Trim();

            // Finds in all list of paths that end with said variable
            foreach (string candidatePath in m_paths)
            {
                if (candidatePath.EndsWith("/" + pathString2))
                {
                    m_domains.Add(candidatePath, domainSet);
                }
            }
        }
    }

    private HashSet<string> getPathsComma(string s)
    {
        s = s.Trim();

        char[] characters = s.ToCharArray();
        HashSet<string> elements = new HashSet<string>();

        if (!s.Contains(",") && !s.Contains("["))
        {
            elements.Add(s);

            return elements;
        }

        int nest = 0, start = 0;
        int i = 0;

        foreach (char c in characters)
        {
            if (c == '[')
            {
                nest++;
                continue;
            }

            if (c == ']')
            {
                nest--;
                continue;
            }

            if (c == ',' && nest == 0)
            {
                string el = s.Substring(start, i);

                foreach (string bracket in getPathsBracket(el))
                {
                    elements.Add(bracket);
                }

                start = (i + 1);
            }

            i++;
        }

        string el2 = s.Substring(start);

        foreach (string bracket in getPathsBracket(el2))
        {
            elements.Add(bracket);
        }

        return elements;
    }

    private HashSet<string> getPathsBracket(string s)
    {
        s = s.Trim();

        char[] characters = s.ToCharArray();
        HashSet<string> elements = new HashSet<string>();

        if (!s.Contains(",") && !s.Contains("["))
        {
            elements.Add(s);

            return elements;
        }

        int i = 0;

        foreach (char c in characters)
        {
            if (c == '[')
            {
                string el = s.Substring(0, i);
                HashSet<string> subpaths = getPathsComma(s.Substring((i + 1), characters.ToString().Length - 1));

                foreach (string subpath in subpaths)
                {
                    if (subpath.Length == 0)
                    {
                        elements.Add(el);
                    }

                    else
                    {
                        elements.Add(el + "/" + subpath);
                    }
                }

                break;
            }
        }

        return elements;
    }

    public string toString()
    {
        StringBuilder sOut = new StringBuilder();

        sOut.Append("Paths:\n" + m_paths + "\n\n");
        sOut.Append("Domains:\n" + m_domains + "\n\n");
        sOut.Append("Generator:\n" + m_generator);

        return sOut.ToString();
    }
}
