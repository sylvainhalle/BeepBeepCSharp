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
 * First-order existential quantifier.
 * 
 * @author Sylvain Hallé
 * @version 2008-05-23
 */
public class FOExists : FOQuantifier
{
	private static string symbolLeft = "<";
	private static string symbolRight = ">";
	private static string symbolUnicode = "\u2203";

    /**
     * Default constructor.
     */
	public FOExists()
	{
		m_symbolLeft = FOExists.symbolLeft;
		m_symbolRight = FOExists.symbolRight;
		m_unicodeSymbol = FOExists.symbolUnicode;
	}

    /**
     * Constructor by parts.
     * 
     * @param a
     *            The quantified variable.
     * @param o
     *            The operand of the quantifier: that is, the formula over which
     *            the quantifier applies.
     */
	public FOExists(Atom a, Operator o) : base(a, o)
	{
		m_symbolLeft = FOExists.symbolLeft;
		m_symbolRight = FOExists.symbolRight;
	}

    /**
     * Returns true if the operator asserts that a path does not exist, and
     * false otherwise
     * @return
     */
	public bool isPathNegation()
	{

        // An existential cannot be an assertion that a path does nor exist
        return false; //Operator.m_falseAtom.equals(m_operand);
	}

    /**
   * Returns true if the operator asserts that a path exists, and
   * false otherwise
   * @return
   */
	public bool isPathAssertion()
	{
		return Operator.m_trueAtom.Equals(m_operand);
	}

    /**
     * Determines if the operator has the structure of an OPlus.
     * An OPlus is an existential quantifier whose operand is of the
     * form x=c, where x is a variable and c is a constant.
     * @return
     */
	public bool isAnOPlus()
	{
		if (m_operand.GetType() != typeof(OperatorEquals))
		{
			return false;
		}
		
		OperatorEquals oe = (OperatorEquals)m_operand;
		
		if (oe.getLeftOperand().GetType() == typeof(Constant))
		{
			if (oe.getRightOperand().GetType() == typeof(Atom))
			{
				return true;
			}
		}
		
		else if (oe.getLeftOperand().GetType() == typeof(Atom))
		{
			if (oe.getRightOperand().GetType() == typeof(Constant))
			{
				return true;
			}
		}
		
		return false;
	}

    /**
     * Returns an OPlus equivalent to the quantifier.
     * For example, the quantifier &exist;<sub>a/b</sub>&nbsp;<i>x</i>&nbsp;:&nbsp;<i>x</i>=<i>k</i>
     * will return the OPlus: &oplus;<sub>a/b</sub>&nbsp;<i>k</i>.
     * @return The OPlus equivalent to the quantifier, null if
     * the quantifier is not equivalent to an OPlus.
     */
	public OPlus toOPlus()
	{
		if (m_operand.GetType() != typeof(OperatorEquals))
		{
			return null;
		}
		
		OperatorEquals oe = (OperatorEquals)m_operand;
		Operator o = oe.getLeftOperand();
		OPlus op = new OPlus();
		
		op.m_qualifier = m_qualifier;
		
		if (o.GetType() == typeof(Constant))
		{
			op.m_operand = o;
		}
		
		else
		{
			op.m_operand = oe.getRightOperand();
		}
		
		return op;
	}

    /**
     * Constructor by copy.
     * 
     * @param o
     *            An Operator
     */
	public FOExists(Operator o) : base(o)
	{
		m_symbolLeft = FOExists.symbolLeft;
		m_symbolRight = FOExists.symbolRight;
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
		FOExists fq = new FOExists();
		
		fq.m_quantifiedVariable = m_quantifiedVariable;
		fq.m_qualifier = m_qualifier;
		fq.m_domain = m_domain;
		fq.m_operand = m_operand.evaluate(variable, val);
		
		return fq;
	}
	
	public override Operator getNegated ()
	{
		FOForAll fq = new FOForAll();
		
		fq.m_quantifiedVariable = m_quantifiedVariable;
		fq.m_qualifier = m_qualifier;
		fq.m_operand = m_operand.getNegated();
		
		return fq;
	}
	
	public override Operator toExplicit ()
	{
		if (m_domain == null || m_domain.Count == 0)
		{
			return null;
		}
		
		BinaryOperator bo = new OperatorOr();
		bool first = true;
		
		foreach (Atom a in m_domain)
		{
			Operator o = m_operand.evaluate(m_quantifiedVariable, a);
			
			if (m_domain.Count == 1)
			{
				return o;
			}
			
			if (first)
			{
				first = false;
				bo.setLeftOperand(o);
				
				continue;
			}
			
			if (bo.getRightOperand() == null)
			{
				bo.setRightOperand(o);
			}
			
			else
			{
				BinaryOperator newbo = new OperatorOr();
				
				newbo.setLeftOperand(bo);
				newbo.setRightOperand(o);
				bo = newbo;
			}
		}
		
		return bo;
	}
}
