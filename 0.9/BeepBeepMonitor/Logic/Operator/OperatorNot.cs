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
 * The NOT operator implements the functionalities of the logical negation.
 * 
 * @author Sylvain Hallé
 * @version 2008-05-23
 */
public class OperatorNot : UnaryOperator
{
	private static string symbol = "!";
	private static string symbolUnicode = "\u00AC";

    /**
     * Default constructor
     */
	public OperatorNot() : base()
	{
		m_symbol = symbol;
		m_unicodeSymbol = symbolUnicode;
	}
	
	public OperatorNot(Operator o) : base(o)
	{
		m_symbol = symbol;
		m_unicodeSymbol = symbolUnicode;
	}
	
	public override Operator getNegated()
	{
		return m_operand;
	}
	
	public override Operator getNegatedNormalForm()
	{
		return m_operand.getNegated();
	}
	
	public override Operator toExplicit()
	{
		UnaryOperator uo = new OperatorNot();
		
		uo.setOperand(m_operand.toExplicit());
		
		return uo;
	}
}
