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

public class Atom : Operator
{
	protected string m_symbol;
	
	public Atom() : base()
	{
		m_symbol = "";
	}

    /**
     * Constructor by copy.
     * 
     * @param o
     */
	public Atom(Operator o) : base(o)
	{
		if (o.GetType() == this.GetType())
		{
			Atom oa = (Atom)o;
			m_symbol = oa.m_symbol;
		}
	}

    /**
     * Constructs an atom by passing a string.
     * 
     * @param s
     *            The symbol that will represent this atom
     */
	public Atom(string s) : base()
	{
		m_symbol = s;
	}
	
	public override bool Equals(object v)
	{
		Atom a;
		
		if (v.GetType() == this.GetType())
		{
			a = (Atom)v;
			
			if (a.m_symbol == m_symbol)
			{
				return true;
			}
		}
		
		return false;
	}

    /**
     * Evaluates a CTL-FO+ formula by replacing an atom by another in it. You
     * can actually replace an atom by a whole formula.
     * <p>
     * In the case of an Atom, checks whether <i>variable</i> is equal to the
     * atom on which the method is called; if not, returns the atom, otherwise,
     * returns <i>value</i>.
     * 
     * @param variable
     *            The atom to look for.
     * @param value
     *            The value to replace it with.
     * @return The evaluated formula
     */
	public override Operator evaluate(Atom variable, Operator val)
	{
		if (Equals(variable))
		{
			return val;
		}
		
		return this;
	}
	
	public override int getLength()
	{
		return 1;
	}

    /**
     * Re-implementation of hashValue for atoms. Two atoms are hashed
     * in the same way if their symbol has the same length.
     * @return
     */
	public int hashValue()
	{
		return m_symbol.Length;
	}

    /**
     * Retrieves the symbol of the atom.
     * 
     * @return A reference to the string representing the symbol of this atom
     */
	public string getSymbol()
	{
		return m_symbol;
	}

    /**
     * Computes the list of quantified variables in the formula.
     * 
     * @return A Map which associates, for each variable name, the qualified
     *         fields over which it quantifies.
     */
	public override Dictionary<string, string> getVariableAssociations()
	{
		Dictionary<string, string> m = new Dictionary<string, string>();
		
		return m;
	}

    /**
     * Sets the symbol of the atom. Note that only a reference to the original
     * symbol is stored; therefore, the copy is shallow.
     * 
     * @param s
     *            The symbol that represents this atom
     */
	public void setSymbol(string s)
	{
		m_symbol = s;
	}

    /**
     * Outputs a string rendition of the atom.
     * 
     * @param indent
     *            The amount of indentation to be prefixed to every line of the
     *            output
     * @return A String output of the atom
     */
	public override string toString(string indent)
	{
		return indent.Insert(indent.Length, m_symbol);
	}
	
	protected virtual string translateXQueryChildren(string root, string indent, int variableCount, int untilCount, bool addComments)
	{
		return "$".Insert(1, m_symbol);
	}

    /**
     * Converts an LTL-FO+ formula into an equivalent XQuery FLWOR expression.
     * This method should not be called directly; rather use {@link
     * translateXQuery()}.
     * 
     * @param root
     *            A String reference to an XQuery variable pointing to the root
     *            of the current message
     * @return A String expression corresponding to XQuery query
     */
	protected virtual string translateXQuerySibling(string root, string indent, int variableCount, bool addComments)
	{
		return "$".Insert(1, m_symbol);
	}
}
