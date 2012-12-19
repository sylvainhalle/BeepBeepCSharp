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
 * The AND operator implements the functionalities of the logical conjunction.
 * 
 * @author Sylvain Hallé
 * @version 2008-07-18
 */
public class OperatorMinus : BinaryOperator
{
	private static string symbol = "-";

    /**
     * Constructor by parts. The resulting Operator is a shallow copy, that is,
     * the left and right operands are not cloned.
     * 
     * @param left
     *            The left operand of the operator
     * @param right
     *            The right operand of the operator
     */
	public OperatorMinus(Operator left, Operator right) : base(left, right)
	{
		m_symbol = OperatorMinus.symbol;
	}

    /**
     * Evaluates a CTL-FO+ formula by replacing an atom by another in it. You
     * can actually replace an atom by a whole formula.
     * <p>
     * In the case of a Minus operator, evaluates the subtraction if both sides
     * are numerical. Otherwise, the operation cannot be resolved to a constant
     * and is returned partially evaluated.
     * <p>
     * The values in the left- and right-hand side are first evaluated as
     * numbers (floats), if they can both be parsed as numbers. Otherwise, they
     * are compared using alphabetical order.
     * 
     * @param variable
     *            The atom to look for.
     * @param value
     *            The value to replace it with.
     * @return The evaluated formula
     */
	public override Operator evaluate(Atom variable, Operator val)
	{
		Operator leftPart = m_left.evaluate(variable, val);
		Operator rightPart = m_right.evaluate(variable, val);
		float num_left = 0.0f, num_right = 0.0f;
		
		if (leftPart.GetType() == (new Constant()).GetType() && 
			rightPart.GetType() == (new Constant()).GetType())
		{
			try
			{
				num_left = float.Parse(leftPart.ToString());
				num_right = float.Parse(rightPart.ToString());
				
				return new Constant(((float)(num_left - num_right)).ToString());
			}
			
			catch (System.FormatException fe)
			{
				System.Diagnostics.Debug.Print (fe.ToString());
			}

            // We are here: LHS and RHS are not both numbers
            // (Do nothing)
		}
		
		return new OperatorMinus(leftPart, rightPart);
	}

    /**
     * We suppose here that the negation of an equality is simply appending a
     * negation in front of the equality.
     */
	public override Operator getNegated()
	{
		return new OperatorNot(this);
	}
}
