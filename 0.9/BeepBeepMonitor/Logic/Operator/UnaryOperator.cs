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

public class UnaryOperator : Operator
{
	protected Operator m_operand;
	protected string m_symbol;
	protected string m_unicodeSymbol;

    /**
     * Default constructor
     */
	public UnaryOperator() : base()
	{
		m_operand = null;
		m_symbol = "";
	}

    /**
     * Constructor by copy.
     * 
     * @param o
     */
	public UnaryOperator(Operator o) : base(o)
	{
		m_operand = o;
	}
	
	public override bool Equals(object v)
	{
		UnaryOperator op;
		
		if (v.GetType() == this.GetType())
		{
			op = (UnaryOperator)v;
			
			if (m_symbol == op.m_symbol && 
				m_operand.Equals(op.m_operand))
			{
				return true;
			}
		}
		
		return false;
	}

    /**
     * Evaluates a CTL-FO+ formula by replacing an atom by another in it. You
     * can actually replace an atom by a whole formula.
     * 
     * @param variable
     *            The atom to look for.
     * @param value
     *            The value to replace it with.
     * @return The evaluated formula
     */
	public override Operator evaluate (Atom variable, Operator val)
	{
        //try
        //{
            UnaryOperator op = (UnaryOperator)this.MemberwiseClone();

            op.m_symbol = m_symbol;
            op.m_operand = m_operand.evaluate(variable, val);

            return op;
        /*}

        catch (CloneNotSupportedExcepton ex)
        {
            // Should not happen!
            //assert(false);
            return new UnaryOperator();
        }*/
	}
	
	public override int getLength()
	{
		if (m_operand != null)
		{
			return (1 + m_operand.getLength());
		}
		
		return 1;
	}
	
	public override Operator getNegatedNormalForm()
	{
        //try
        //{
            UnaryOperator op = new UnaryOperator();
            op = this;

            op.setOperand(m_operand.getNegatedNormalForm());

            return op;
        /*}

        catch (CloneNotSupportedException e)
        {
            // This should not happen!
            //assert(false);
            return this;
        }*/
	}

    /**
     * Returns the operand of the unary operator.
     * 
     * @return A reference to the Operator that is the operand of the current
     *         object.
     */
	public Operator getOperand()
	{
		return m_operand;
	}

    /**
     * Returns the set of quantified variables in a formula.
     * 
     * @return A set of Atoms, these atoms being the quantified variables in the
     *         formula.
     */
	public override HashSet<Atom> getQuantifiedVariables()
	{
		return m_operand.getQuantifiedVariables();
	}

    /**
     * Computes the list of quantified variables in the formula.
     * 
     * @return A Map which associates, for each variable name, the qualified
     *         fields over which it quantifies.
     */
	public override Dictionary<string, string> getVariableAssociations()
	{
		return m_operand.getVariableAssociations();
	}

    /**
     * Sets the operand of the unary operator. The resulting object contains a
     * reference to the operator passed as an argument; therefore the copy is
     * shallow.
     * 
     * @param o
     *            The operand
     */
	public void setOperand(Operator o)
	{
		m_operand = o;
	}

    /**
     * Outputs a string rendition of the operator and of all its sub-operators
     * in a recursive fashion.
     * 
     * @param indent
     *            The amount of indentation to be prefixed to every line of the
     *            output
     * @return A String output of the operator
     */
	public override string toString (string indent)
	{
		return indent + "(" + m_unicodeSymbol + " " + m_operand.ToString() + ")";
	}
	
	public override Operator toExplicit()
	{
		UnaryOperator op = new UnaryOperator();
		
		op.setOperand(m_operand.toExplicit());
		
		return op;
	}
}
