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

/**
 * The AND operator implements the functionalities of the logical conjunction.
 * 
 * @author Sylvain Hallé
 * @version 2008-04-24
 */
public class BinaryOperator : Operator
{
	protected Operator m_left;
	protected Operator m_right;
	protected string m_symbol;
	protected string m_unicodeSymbol;
	protected bool m_commutative;

    /**
     * Default constructor
     */
	public BinaryOperator() : base()
	{
		m_left = null;
		m_right = null;
		m_symbol = "";
		m_unicodeSymbol = "";
		m_commutative = false;
	}

    /**
     * Default constructor by copy. The resulting object is a constructor with
     * similar fields than the Operator passed in the argument. Note that the
     * copy is shallow, i.e. it is not recursive.
     * 
     * @param o
     *            An Operator
     */
	public BinaryOperator(Operator o) : base(o)
	{		
		if (o.GetType() == this.GetType())
		{
			BinaryOperator bo = (BinaryOperator)o;
			
			m_left = bo.m_left;
			m_right = bo.m_right;
			m_commutative = bo.m_commutative;
		}
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
	public BinaryOperator(Operator left, Operator right)
	{
		m_left = null;
		m_right = null;
		m_symbol = "";
		m_unicodeSymbol = "";
		m_commutative = false;
		
		m_left = left;
		m_right = right;
	}
	
	public override bool Equals(object v)
	{
		BinaryOperator bo;
		
		if (v.GetType() == this.GetType())
		{
			bo = (BinaryOperator)v;
			
			if (m_symbol == bo.m_symbol && 
				m_left.Equals(bo.m_left) && 
				m_right.Equals(bo.m_right))
			{
				return true;
			}
			
			if (m_commutative)
			{
                // If the binary operator is defined as commutative, then check
                // if swapping left and right leads to an equality
				if (m_symbol == bo.m_symbol && 
					m_left.Equals(bo.m_right) && 
					m_right.Equals(bo.m_left))
				{
					return true;
				}
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
            BinaryOperator bo = (BinaryOperator)this.MemberwiseClone();

            bo.m_symbol = m_symbol;
            bo.m_left = m_left.evaluate(variable, val);
            bo.m_right = m_right.evaluate(variable, val);

            return bo;
        /*}

        catch (CloneNotSupportedException ex)
        {
            // Should not happen!
            //assert(false);
            return new BinaryOperator();
        }*/
	}

    /**
     * Returns the left operand of the operator.
     * 
     * @return A reference to the left Operator object
     */
	public Operator getLeftOperand()
	{
		return m_left;
	}
	
	public override int getLength()
	{
		if (m_left != null && m_right != null)
		{
			return (1 + m_left.getLength() + m_right.getLength());
		}
		
		return 1;
	}
	
	public override Operator getNegatedNormalForm()
	{
        //try
        //{
            BinaryOperator bo = new BinaryOperator();
            bo = this;

            bo.setOperands(m_left.getNegatedNormalForm(), m_right.getNegatedNormalForm());

            return bo;
        /*}

        catch (CloneNotSupportedException e)
        {
            // This should not happen!
            //assert(false);
            return this;
        }*/
	}

    /**
     * Returns the set of quantified variables in a formula.
     * 
     * @return A set of Atoms, these atoms being the quantified variables in the
     *         formula.
     */
	public override HashSet<Atom> getQuantifiedVariables()
	{
		HashSet<Atom> variables;
		
		variables = m_left.getQuantifiedVariables();
		
		
		foreach (Atom a in m_right.getQuantifiedVariables())
		{
			variables.Add(a);
		}
		
		return variables;
	}

    /**
     * Returns the right operand of the operator.
     * 
     * @return A reference to the right Operator object
     */
	public Operator getRightOperand()
	{
		return m_right;
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
		
		foreach (KeyValuePair<string, string> val in m_left.getVariableAssociations())
		{
			m.Add(val.Key, val.Value);
		}
		
		foreach (KeyValuePair<string, string> val in m_right.getVariableAssociations())
		{
			m.Add(val.Key, val.Value);
		}
		
		return m;
	}

    /**
     * Sets the left operand of the operator. The operator is only copied by
     * reference.
     * 
     * @param o
     *            The left operand
     */
	public void setLeftOperand(Operator o)
	{
		m_left = o;
	}

    /**
     * Sets the two operands of the operator.
     * 
     * @param left
     *            Left operand
     * @param right
     *            Right operand
     */
	public void setOperands(Operator left, Operator right)
	{
		m_left = left;
		m_right = right;
	}

    /**
     * Sets the right operand of the operator. The operator is only copied by
     * reference.
     * 
     * @param o
     *            The right operand
     */
	public void setRightOperand(Operator o)
	{
		m_right = o;
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
		return (indent + "(" + m_left.ToString() + " " + m_unicodeSymbol + " " + m_right.ToString() + ")");
	}
	
	public override Operator toExplicit()
	{
		BinaryOperator bo = new BinaryOperator();
		
		bo.setLeftOperand(m_left.toExplicit());
		bo.setRightOperand(m_left.toExplicit());
		
		return bo;
	}
}
