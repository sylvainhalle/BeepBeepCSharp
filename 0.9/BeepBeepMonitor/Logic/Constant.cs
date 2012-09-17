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

/**
 * Simple implementation of a Constant (i.e. a "non-variable").
 * 
 * @author Sylvain Hallé
 * @version 2010-10-24
 */
public class Constant : Atom
{
	public Constant()
	{
	}

    /**
     * Constructor by copy.
     * 
     * @param o
     */
	public Constant(Operator o)
	{
		if (o.GetType() == this.GetType())
		{
			Constant oa = (Constant)o;
			m_symbol = oa.m_symbol;
		}
	}

    /**
     * Constructs a constant by passing a string.
     * 
     * @param s
     *            The symbol that will represent this constant
     */
	public Constant(string s) : base(s)
	{
	}
	
	public override bool Equals(object o)
	{
		if (o.GetType() != typeof(Constant))
		{
			return false;
		}
		
		return (m_symbol == ((Constant)o).m_symbol);
	}
	
	public override Operator getNegated()
	{
		if (m_symbol == "TRUE")
		{
			return Operator.m_falseAtom;
		}
		
		else if (m_symbol == "FALSE")
		{
			return Operator.m_trueAtom;
		}
		
		return this;
	}
	
	public Operator evaluate(Atom variable, Operator val)
	{
		if (m_symbol == "TRUE")
		{
			return Operator.m_trueAtom;
		}
		
		else if (m_symbol == "FALSE")
		{
			return Operator.m_falseAtom;
		}
		
		return this;
	}
	
	protected override string translateXQueryChildren(string root, string indent, int variableCount, int untilCount, bool addComments)
    {
        /*if (m_symbol.contains("/"))
        {
            return "\"" + m_symbol + "\"";
        }*/

        return m_symbol;
    }

    /**
     * Converts an LTL-FO+ formula into an equivalent XQuery FLWOR expression.
     * This method should not be called directly; rather use {@link
     * translateXQuery()}.
     * <p>
     * This method tries to distinguish between paths and literals. Any string
     * containing "/" are taken as XPath paths and are output without quotes;
     * any other string is taken as a literal and output surrounded by quotes.
     * 
     * @param root
     *            A String reference to an XQuery variable pointing to the root
     *            of the current message
     * @return A String expression corresponding to XQuery query
     */
	protected override string translateXQuerySibling(string root, string indent, int variableCount, bool addComments)
	{
        /*if (m_symbol.contains("/"))
        {
            return "\"" + m_symbol + "\"";
        }*/

		return m_symbol;
	}
}
