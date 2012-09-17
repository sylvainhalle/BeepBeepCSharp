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
 * First-order universal quantifier.
 * 
 * @author Sylvain Hallé
 * @version 2008-05-23
 */
public class FOForAll : FOQuantifier
{
	private static string symbolLeft = "[";
	private static string symbolRight = "]";
	private static string symbolUnicode = "\u2200";

    /**
     * Default constructor
     */
	public FOForAll()
	{
		m_symbolLeft = FOForAll.symbolLeft;
		m_symbolRight = FOForAll.symbolRight;
		m_unicodeSymbol = FOForAll.symbolUnicode;
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
	public FOForAll(Atom a, Operator o) : base(a, o)
	{
		m_symbolLeft = FOForAll.symbolLeft;
		m_symbolRight = FOForAll.symbolRight;
	}

    /**
     * Constructor by copy.
     * 
     * @param o
     *            An Operator
     */
	public FOForAll(Operator o) : base(o)
	{
		m_symbolLeft = FOForAll.symbolLeft;
		m_symbolRight = FOForAll.symbolRight;
	}

    /**
   * Returns true if the operator asserts that a path does not exist, and
   * false otherwise
   * @return
   */
	public bool isPathNegation()
	{
		return Operator.m_falseAtom.Equals(m_operand);
	}

    /**
   * Returns true if the operator asserts that a path exists, and
   * false otherwise
   * @return
   */
	public bool isPathAssertion()
	{
        // A universal cannot be an assertion that a path exists
        //return false;//Operator.m_trueAtom.equals(m_operand);
		return Operator.m_falseAtom.Equals(m_operand);
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
        /*
         * TODO: The code for evaluate here and in the FOExists class are
         * identical except for the first line (construction of a new instance
         * of either an FOExists or an FOForAll). Isn't there a way to
         * centralize that function up in the FOQuantifier class?
         */
		FOForAll fq = new FOForAll();
		
		fq.m_quantifiedVariable = m_quantifiedVariable;
		fq.m_qualifier = m_qualifier;
		fq.m_domain = m_domain;
		fq.m_operand = m_operand.evaluate(variable, val);
		
		return fq;
	}
	
	public override Operator getNegated()
	{
		FOExists fq = new FOExists();
		
		fq.m_quantifiedVariable = m_quantifiedVariable;
		fq.m_qualifier = m_qualifier;
		fq.m_operand = m_operand.getNegated();
		
		return fq;
	}
	
	public override Operator toExplicit()
	{
		if (m_domain == null || m_domain.Count == 0)
		{
			return null;
		}
		
		BinaryOperator bo = new BinaryOperator();
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
				BinaryOperator newbo = new OperatorAnd();
				
				newbo.setLeftOperand(bo);
				newbo.setRightOperand(o);
				bo = newbo;
			}
		}
		
		return bo;
	}
}
