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
 * The U operator implements the functionalities of the LTL U temporal modality.
 * 
 * @author Sylvain Hallé
 * @version 2008-05-26
 */

public class OperatorU : BinaryOperator
{
	private static string symbol = "U";
	private static string symbolUnicode = "U";
	
	public OperatorU() : base()
	{
		m_symbol = OperatorU.symbol;
		m_unicodeSymbol= symbolUnicode;
	}
	
	/**
	 * Constructor by parts. The resulting Operator is a shallow copy, that is,
	 * the left and right operands are not cloned.
	 * 
	 * @param left
	 *            The left operand of the operator
	 * @param right
	 *            The right operand of the operator
	 */
	public OperatorU(Operator left, Operator right) : base(left, right)
	{
		m_symbol = OperatorU.symbol;
		m_unicodeSymbol= symbolUnicode;
	}
	
	public override Operator getNegated()
	{
		return new OperatorV(m_left.getNegated(), m_right.getNegated());
	}
	
	public override Operator toExplicit()
	{
		BinaryOperator o = new OperatorU();
		
		o.setLeftOperand(m_left.toExplicit());
		o.setRightOperand(m_right.toExplicit());
		
		return o;
	}
}
