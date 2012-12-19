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
 * The GreaterThan operator implements the functionalities of the &gt; operator.
 * If both operands are numbers, they are compared numerically instead of
 * alphabetically.
 * 
 * @author Sylvain Hall&eacute;
 * @version 2008-12-23
 */
public class OperatorGreaterThan : BinaryOperator
{
	private static string symbol = ">";

    /**
     * If set to true, the comparison includes equality (i.e. it is a "Greater
     * than or equal"). Defaults to false.
     */
	protected bool m_inclusive = false;

    /**
     * Constructor by parts. The resulting Operator is a shallow copy, that is,
     * the left and right operands are not cloned.
     * 
     * @param left
     *            The left operand of the operator
     * @param right
     *            The right operand of the operator
     */
	public OperatorGreaterThan(Operator left, Operator right) : base(left, right)
	{
		m_symbol = OperatorGreaterThan.symbol;
	}
	
	public OperatorGreaterThan(Operator left, Operator right, bool inconclusive) : this(left, right)
	{
		m_inclusive = true;
		m_symbol = OperatorGreaterThan.symbol + "=";
	}

    /**
     * Evaluates a CTL-FO+ formula by replacing an atom by another in it. You
     * can actually replace an atom by a whole formula.
     * <p>
     * In the case of a GreaterThan operator, evaluates both sides of the
     * inequality. If both sides are constants, the equality is replaced by
     * either TRUE or FALSE whether the left and right constants are equal.
     * Otherwise, the equality cannot be resolved to a boolean and is returned
     * partially evaluated.
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

                if ((!m_inclusive && num_left > num_right) ||
                    (m_inclusive && num_left >= num_right))
                {
                    return Operator.m_trueAtom;
                }

                else
                {
                    return Operator.m_falseAtom;
                }
            }

            catch (System.FormatException fe)
            {
				System.Diagnostics.Debug.Print (fe.ToString());
            }

            // We are here: LHS and RHS are not both numbers
            // Compare alphabetically
            if ((!m_inclusive && (string.Compare(leftPart.ToString(), rightPart.ToString()) > 0)) ||
                (m_inclusive && (string.Compare(leftPart.ToString(), rightPart.ToString()) >= 0)))
            {
                return Operator.m_trueAtom;
            }

            else
            {
                return Operator.m_falseAtom;
            }

        }

		return new OperatorGreaterThan(leftPart, rightPart);
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
