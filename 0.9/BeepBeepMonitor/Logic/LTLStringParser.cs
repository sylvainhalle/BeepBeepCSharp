/******************************************************************************
Basic classes for first-order and modal logic manipulation
Copyright (C) 2007-2008 Sylvain Halle

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
using System.Text.RegularExpressions;

/**
 * Parser for the logic LTL-FO+.
 * <p>
 * The semantics of this logic is defined on a <em>trace</em> of events. Each
 * event contains some data that can be accessed through the quantifiers. For
 * example, an event can be an XML message being sent or received, and the data
 * is the content of this message, accessible through XPath expressions.
 * Properties can then be defined on that sequence of events using the following
 * <strong>LTL temporal operators</strong>:
 * <ol>
 * <li><b>X</b> means "in the next event". For example, if &phi; is a formula,
 * <b>X</b>&nbsp;&phi; says that &phi; will be true in the next event.
 * <li><b>G</b> means "globally". For example, if &phi; is a formula,
 * <b>G</b>&nbsp;&phi; says that &phi; is true in the current event, and will be
 * true for all remaining events in the trace.
 * <li><b>F</b> means "eventually". Writing <b>F</b>&nbsp;&phi; says that &phi;
 * is either true in the current event, or will become true for at least one
 * event in the future.
 * <li><b>U</b> means "until". If &phi; and &psi; are formulas, writing
 * &phi;&nbsp;<b>U</b>&nbsp;&psi; says that &psi; will be true eventually, and
 * in the meantime, &phi; is true for every event until &psi; becomes true.
 * </ol>
 * Data inside an event can be accessed using <strong>first-order
 * quantifiers</strong>:
 * <ol>
 * <li>[<i>x</i>&nbsp;<i>f</i>] means "for every <i>x</i> in <i>f</i>". Here,
 * <i>x</i> is a variable, and <i>f</i> is a function used to fetch possible
 * values for <i>x</i>. Hence, if events are XML messages, <i>f</i> can be an
 * XPath expression retrieving values for <i>x</i>; for example, if &phi; is a
 * formula where <i>x</i> appears, then [<i>x</i>&nbsp;<tt>/tag1/tag2</tt>
 * ]&nbsp;&phi; means: every value at the end of <tt>/tag1/tag2</tt> satisfies
 * &phi;.
 * <li>&lt;<i>x</i>&nbsp;<i>f</i>&gt; means "some <i>x</i> in <i>f</i>". Hence,
 * &lt;<i>x</i>&nbsp;<tt>/tag1/tag2</tt>&gt;&nbsp;&phi; means: there exists some
 * value at the end of <tt>/tag1/tag2</tt> which satisfies &phi;.
 * </ol>
 * These operators can be nested or combined with traditional boolean
 * connectives to create compound statements.
 * 
 * @author Sylvain Hall&eacute;
 * @version 2010-10-25
 * 
 */
public class LTLStringParser : OperatorStringParser
{
    /**
     * Formats a string to be parsed by the LTLStringParser. This method
     * converts all whitespace characters (tabs, carriage returns, etc.) to
     * spaces, and then converts all multiple spaces into a single space.
     * 
     * @param s
     *            The string to be formatted
     * @return A converted string
     */
	private static string formatString(string s)
	{
		if (s == "")
		{
			return s;
		}
		
		s = s.Replace("[\\n\\r\\t]", " ");
		s = s.Replace("[ ]", " ");
        //s = s.replaceAll("([\\)\\(\\[\\]])[ ]+([\\)\\(\\[\\]<>])", "$1$2");
        s = s.Replace("([\\)\\(\\[\\]])[ ]+([\\)\\(\\[\\]])", "$1$2");
		
		return s;
	}
	
	public static new Operator parseFromString(string s)
	{
		return parseFromString(s, null);
	}

    /**
     * Constructs an Operator based on a string. With the exception of possible
     * superfluous parentheses and whitespace, the resulting operator is such
     * that getFromString(s).toString() == s. The input string must respect the
     * following BNF grammar (which corresponds to LTL + first-order;
     * parentheses are important):
     * <ol>
     * <li>string := atom | binary_op | unary_op | quantified
     * <li>binary_op := (string) bin_operator (string)
     * <li>unary_op := un_operator (string)
     * <li>quantified := [qualif_var] (string) | &lt;qualif_var&gt; (string)
     * <li>bin_operator := &amp; | -&gt; | the "pipe" character | U | = | &lt; |
     * &gt; | -
     * <li>un_operator := ! | G | X | F
     * <li>qualif_var := atom qualif (there is a whitespace character between
     * atom and qualif)
     * <li>atom := any literal composed of alphanumerical characters, with the
     * exception of reserved sequences such as operators
     * <li>qualif:= any literal composed of alphanumerical characters, with the
     * exception of reserved sequences such as operators
     * </ol>
     * 
     * @param s
     *            The input string
     * @param domains
     *            A map from a set of paths to a set of values (atoms), providing
     *            finite domains for quantifiers. This parameter is facultative;
     *            domains need to be provided only when quantifiers need to be
     *            expanded into explicit conjunctions/disjunctions of values, or
     *            when messages must be generated (instead of monitored).
     *            
     * @return An Operator equivalent to the string passed as an argument, null
     *         if the string does not correspond to a valid Operator.
     */
	public static Operator parseFromString(string s, IDictionary<string, HashSet<Constant>> domains)
	{
		bool flag = false;
		int quantRight = 0, parLeft = 0;
		Operator o = null, o2 = null;
		FOQuantifier foq = null;
		UnaryOperator uo = null;
		BinaryOperator bo = null;
		Atom a = null;

        // First removes/converts extra whitespace
		s = LTLStringParser.formatString(s);
		
		string sTrim = s.Trim();
		
		if (sTrim.Length == 0)
		{
			return null;
		}

        // Handles first-order quantifiers
		flag = false;
		
		if (sTrim[0] == '[')
		{
			foq = new FOForAll();
			quantRight = sTrim.IndexOf("]");
			flag = true;
		}
		
		if (sTrim[0] == '<')
		{
			foq = new FOExists();
			quantRight = sTrim.IndexOf(">");
			flag = true;
		}
		
		if (flag)
		{
			Regex r = new Regex("^(\\w+)\\s*(.*)$");
			MatchCollection m = r.Matches(sTrim.Substring(1, quantRight - 1));
			
			if (m.Count > 0)
			{
				Match m2 = m[0];
				GroupCollection g = m2.Groups;
				Atom qvar = new Atom(g[1].ToString());
				string qualifier = g[2].ToString();
				
				foq.setQuantifiedVariable(qvar);
				foq.setQualifier(qualifier);
				
				if (domains != null)
				{
                    // If a domain is provided, attaches it to the quantifier
					HashSet<Constant> dom = new HashSet<Constant>();
					
					foreach (KeyValuePair<string, HashSet<Constant>> con in domains)
					{
						if (con.Key == qualifier)
						{
							dom = con.Value;
						}
					}
					
					foq.setDomain(dom);
				}
			}

            //foq.setQualifiedVariable(sTrim.substring(1, quantRight));
			parLeft = sTrim.IndexOf("(", (quantRight + 1));
            //parRight = sTrim.indexOf(")", parLeft);
			o = parseFromString(sTrim.Substring((parLeft + 1), sTrim.Length - 1 - (parLeft + 1)), domains);
			foq.setOperand(o);
			
			return foq;
		}

        // Handles unary operators
		flag = false;
		
		if (sTrim.Substring(0, 1) == "!")
		{
			uo = new OperatorNot();
			flag = true;
		}
		
		if (sTrim.Substring(0, 1) == "F")
		{
			uo = new OperatorF();
			flag = true;
		}
		
		if (sTrim.Substring(0, 1) == "G")
		{
			uo = new OperatorG();
			flag = true;
		}
		
		if (sTrim.Substring(0, 1) == "X")
		{
			uo = new OperatorX();
			flag = true;
		}

        if (sTrim.Length >= 2)
        {
            // This is for CTL, nothing to do here
        }
		
		if (flag)
		{
			parLeft = sTrim.IndexOf("(");
			o = parseFromString(sTrim.Substring((parLeft + 1), sTrim.Length - 1 - (parLeft + 1)), domains);
			uo.setOperand(o);
			
			return uo;
		}

        // Handles binary operators
		flag = false;
		
		if (sTrim[0] == '(')
		{
            // At this point in the method, if first char is a "(",
            // the formula is necessarily a binary operator
			int parNum = 0;
			string sLeft = "", sRight = "";
			string binaryOp = "";
			Regex r = new Regex("(\\(|\\))");
			MatchCollection m = r.Matches(sTrim);
			
			for (int i = 0; i < m.Count; i++)
			{
				Match m2 = m[0];
				GroupCollection g = m2.Groups;
				
				if (g[0].ToString().Equals("("))
				{
					parNum++;
				}
				
				else
				{
					parNum--;
				}
				
				if (parNum == 0)
				{
					sLeft = sTrim.Substring(1, m.Count - 1 - 1);
					break;
				}
			}
			
			parLeft = sTrim.IndexOf("(", m.Count - 1);
			binaryOp = sTrim.Substring(m.Count + 1, parLeft - 1 - (m.Count + 1)).Trim();
			sRight = sTrim.Substring(parLeft + 1, sTrim.Length - 1 - (parLeft + 1)).Trim();
			o = parseFromString(sLeft, domains);
			o2 = parseFromString(sRight, domains);
			
			if (binaryOp == "&")
			{
				bo = new OperatorAnd();
				flag = true;
			}
			
			if (binaryOp == "|")
			{
				bo = new OperatorOr();
				flag = true;
			}
			
			if (binaryOp == "=")
			{
				bo = new OperatorEquals(o, o2);
				flag = true;
			}
			
			if (binaryOp == "->")
			{
				bo = new OperatorImplies(o, o2);
				flag = true;
			}
			
			if (binaryOp == "-")
			{
				bo = new OperatorMinus(o, o2);
				flag = true;
			}
			
			if (binaryOp == "<")
			{
				bo = new OperatorSmallerThan(o, o2);
				flag = true;
			}
			
			if (binaryOp == "<=")
			{
				bo = new OperatorSmallerThan(o, o2, true);
				flag = true;
			}
			
			if (binaryOp == ">")
			{
				bo = new OperatorGreaterThan(o, o2);
				flag = true;
			}
			

			if (binaryOp == ">=")
			{
				bo = new OperatorGreaterThan(o, o2, true);
				flag = true;
			}
			
			if (binaryOp == "U")
			{
				bo = new OperatorU(o, o2);
				flag = true;
			}
			
			if (binaryOp == "V")
			{
				bo = new OperatorV(o, o2);
				flag = true;
			}
			
			if (flag)
			{
				return bo;
			}
		}
		
		else
		{
            // At this point, the only case left is that of a single atom
            // (either a constant or a variable)
			if (sTrim[0] == '{')
			{
                // Constants are surrounded by braces
				a = new Constant(sTrim.Substring(1, sTrim.Length - 1 - 1));
			}
			
			else if (sTrim[0] == '/')
			{
                // XPaths are surrounded by forward slashes
				a = new ConstantPath(sTrim.Substring(1, sTrim.Length - 1 - 1));
			}
			
			else
			{
                // Otherwise, we have an atom
				a = new Atom(sTrim);
			}
			
			return a;
		}

        // Down there, none of the previous cases has fired: return o, which is
        // null
		return o;
	}
}
