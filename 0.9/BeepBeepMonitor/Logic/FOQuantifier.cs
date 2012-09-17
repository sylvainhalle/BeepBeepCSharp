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
using System.Text.RegularExpressions;

/**
 * Basic class for first-order quantifier.
 * 
 * @author Sylvain Hallé
 * @version 2007-08-13
 */
public class FOQuantifier : Operator
{
	public string m_symbolLeft;
	public string m_symbolRight;
	public string m_unicodeSymbol;
	public Atom m_quantifiedVariable;
	public Operator m_operand;
	public string m_qualifier;
	public HashSet<Constant> m_domain;
	
	public FOQuantifier() : base()
	{
		m_symbolLeft = "";
		m_symbolRight = "";
		m_quantifiedVariable = null;
		m_operand = null;
		m_qualifier = "";
		m_domain = null;
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
	public FOQuantifier(Atom a, Operator o) : base(o)
	{
		m_quantifiedVariable = a;
		m_operand = o;
	}

    /**
     * Constructor by parts.
     * 
     * @param a
     *            The quantified variable.
     * @param qualifier
     *            A string expression representing the fields over which the
     *            variable quantifies.
     * @param o
     *            The operand of the quantifier: that is, the formula over which
     *            the quantifier applies.
     */
	public FOQuantifier(Atom a, string qualifier, Operator o) : base(o)
	{
		m_quantifiedVariable = a;
		m_operand = o;
		m_qualifier = qualifier;
	}

    /**
     * Default constructor by copy. The resulting object is a constructor with
     * similar fields than the Operator passed in the argument. Note that the
     * copy is shallow, i.e. it is not recursive.
     * 
     * @param o
     *            An Operator
     */
	public FOQuantifier(Operator o) : base(o)
	{
		if (o.GetType() == this.GetType())
		{
			FOQuantifier foq = (FOQuantifier)o;
			
			m_symbolLeft = foq.m_symbolLeft;
			m_symbolRight = foq.m_symbolRight;
			m_quantifiedVariable = foq.m_quantifiedVariable;
			m_operand = foq.m_operand;
		}
	}

    /**
     * Assigns a domain to the quantifier's qualifier
     * @param domain A set of constants representing the possible values
     */
	public void setDomain(HashSet<Constant> domain)
	{
		m_domain = domain;
	}

    /**
     * Returns the domain for the quantifier
     * @return The set of atoms corresponding to the possible values
     * the quantified variable can take.
     */
	public HashSet<Constant> getDomain()
	{
		return m_domain;
	}
	
	public override bool Equals(object v)
	{
		FOQuantifier foq;
		
		if (v.GetType() == this.GetType())
		{
			foq = (FOQuantifier)v;
			
			if (m_qualifier == foq.m_qualifier && 
				m_quantifiedVariable.Equals(foq.m_quantifiedVariable) && 
				m_operand.Equals(foq.m_operand))
			{
				return true;
			}
		}
		
		return false;
	}
	
	public override Operator evaluate (Atom variable, Operator val)
	{
        //try
        //{
            FOQuantifier foq = (FOQuantifier)this.MemberwiseClone();

            foq.m_quantifiedVariable = m_quantifiedVariable;
            foq.m_qualifier = m_qualifier;
            foq.m_domain = m_domain;
            foq.m_operand = m_operand.evaluate(variable, val);

            return foq;
        /*}

        catch (CloneNotSupportedException ex)
        {
            // Should not happen!
            //assert(false);
            return new FOQualifier();
        }*/
	}
	
	public override int getLength()
	{
		if (m_operand != null)
		{
			return (3 + m_operand.getLength());
		}
		
		return 3;
	}
	
	public override Operator getNegatedNormalForm()
	{
        //try
        //{
            FOQuantifier foq = (FOQuantifier)this.MemberwiseClone();

            foq.setOperand(m_operand.getNegatedNormalForm());

            return foq;
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
     * Returns the qualifier for the quantified variable.
     * 
     * @return The string expression for the qualifier.
     */
	public string getQualifier()
	{
		return m_qualifier;
	}

    /**
     * Returns the particular variable bound by this quantifier. This method is
     * not recursive and returns only the variable bound by this quantifier. To
     * get the list of all quantified variables, use
     * {@link getQuantifiedVariables}.
     * 
     * @return The quantified variable.
     */
	public Atom getQuantifiedVariable()
	{
		return m_quantifiedVariable;
	}

    /**
     * Returns the set of quantified variables in a formula.
     * 
     * @return A set of Atoms, these atoms being the quantified variables in the
     *         formula.
     */
	public override HashSet<Atom> getQuantifiedVariables()
	{
		HashSet<Atom> variables = new HashSet<Atom>();
		
		variables.Add(m_quantifiedVariable);
		
		foreach (Atom a in m_operand.getQuantifiedVariables())
		{
			variables.Add(a);
		}
		
		return variables;
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
		
		m = m_operand.getVariableAssociations();
		m.Add(m_quantifiedVariable.ToString(), m_qualifier);
		
		return m;
	}

    /**
     * Sets the operand of the quantifier.
     * 
     * @param o
     *            The operand of the quantifier: that is, the formula over which
     *            the quantifier applies.
     */
	public void setOperand(Operator o)
	{
		m_operand = o;
	}

    /**
     * Assigns both the name of the quantified variable and its qualifier, based
     * on a string expression.
     * 
     * @param s
     *            The string expression. The string must be of the form
     *            <tt>atom qualifier</tt>, where atom is any alphanumeric
     *            sequence, qualifier is a string, and there is any number of
     *            whitespace characters between both.
     */
	public void setQualifiedVariable(string s)
	{
		Regex r = new Regex("^(\\w+)\\s*(.*)$");
		MatchCollection m = r.Matches(s);
		
		if (m.Count > 0)
		{
			Match m2 = m[0];
			GroupCollection g = m2.Groups;
			
			m_quantifiedVariable = new Atom(g[1].ToString());
			m_qualifier = g[2].ToString();
		}
	}

    /**
     * Sets the quantified variable for the quantifier
     */
	public void setQuantifiedVariable(Atom a)
	{
		if (a != null)
		{
			m_quantifiedVariable = new Atom(a);
		}
	}

    /**
     * Sets the qualifier for the quantifier
     * @param a
     */
	public void setQualifier(string qualifier)
	{
		if (qualifier != null)
		{
			m_qualifier = qualifier;
		}
	}

    /**
     * Outputs a string rendition of the operator and of all its sub-operators
     * in a recursive fashion.
     * 
     * @return A String output of the operator
     */
    /*public String toString(String indent) {
        return indent.concat(m_symbolLeft).concat(
                m_quantifiedVariable.toString()).concat("=")
                .concat(m_qualifier).concat(m_symbolRight).concat(" (").concat(
                        m_operand.toString()).concat(")");
    }*/
	public override string toString (string indent)
	{
		return (indent + m_unicodeSymbol + m_quantifiedVariable + "\u220A" + m_qualifier + " : (" + m_operand.ToString() + ")");
	}
}
