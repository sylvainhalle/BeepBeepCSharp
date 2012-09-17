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
 * The Operator class is the basic object for the management of CTL-FO+
 * formulas. It only provides limited functionality and spawns a number of
 * subclasses for the various first-order and temporal operators defined in the
 * logic.
 * 
 * @author Sylvain Hallé
 * @version 2008-07-18
 */
public class Operator
{
    /**
     * The Atom representing the static value "TRUE"
     */
	public static Constant m_trueAtom = new Constant("TRUE");

    /**
     * The Atom representing the static value "FALSE"
     */
	public static Constant m_falseAtom = new Constant("FALSE");

    /**
     * The Atom representing the static value "UNDEFINED"
     */
	public static Constant m_undefinedAtom = new Constant("_#");

    /**
     * Default empty constructor.
     */
	public Operator()
	{
        // Do nothing but call child constructor
	}

    /**
     * Default constructor by copy. The resulting object is a constructor with
     * similar fields than the Operator passed in the argument. Note that the
     * copy is shallow, i.e. it is not recursive.
     * 
     * @param o
     *            An Operator
     */
	public Operator(Operator o)
	{
        // Do nothing but call child constructor
	}
	
	public override bool Equals(object v)
	{
		if (v.GetType() == this.GetType())
		{
			return true;
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
	public virtual Operator evaluate(Atom variable, Operator val)
	{
		return this;
	}

    /**
     * Returns an explicitly quantified version of the formula
     * @return
     */
	public virtual Operator toExplicit()
	{
		return this;
	}

    /**
     * Computes the length of an expression, i.e. the number of distinct "atoms"
     * that compose it, recursively, according to the following rules:
     * <ol>
     * <li>length(constant) = length(variable) = 1</li>
     * <li>length(unary_op &phi;) = 1 + length(&phi;)</li>
     * <li>length(&phi; binary_op &psi;) = 1 + length(&phi;) + length(&psi;)</li>
     * <li>length(quantifier<sub>&pi;</sub><i>x</i> : &phi;) = 3 + length(&phi;)
     * </li>
     * </ol>
     * Normally, the length of an operator is directly proportional to the
     * actual amount of memory it uses.
     * 
     * @return The number of "symbols" in the operator
     */
	public virtual int getLength()
	{
		return 0;
	}

    /**
     * Produces the negation of the current Operator. The negation is obtained
     * by recursively applying the negation to its subformulae, using the
     * identities described in {@link getNegatedNormalForm}; therefore, the
     * negations will be pushed as deep as possible in the formula.
     * 
     * @return An Operator which is the negation of the current formula.
     */
	public virtual Operator getNegated()
	{
		return this;
	}

    /**
     * Produces the negated normal form (NNF) of the operator. The NNF is
     * obtained by repeatedly applying the following identities:
     * <ul>
     * <li>&not; TRUE = FALSE</li>
     * <li>&not; FALSE = TRUE</li>
     * <li>&not; &not; &phi; = &phi;</li>
     * <li>&not; (&phi; &and; &psi;) = &not; &phi; &or; &not; &psi;</li>
     * <li>&not; (&phi; &or; &psi;) = &not; &phi; &and; &not; &psi;</li>
     * <li>&phi; &rarr; &psi; = &not; &phi; &or; &psi;</li>
     * <li>&not G &phi; = F &not; &phi;</li>
     * <li>&not F &phi; = G &not; &phi;</li>
     * <li>&not; X &phi; = X &not; &phi;</li>
     * <li>&not; (&phi; U &psi;) = &not; &phi; V &not; &psi;</li>
     * <li>&not; (&phi; V &psi;) = &not; &phi; U &not; &psi;</li>
     * <li>&not; (&exist;<sub>p</sub> x : &phi;) = &forall;<sub>p</sub> x: &not;
     * &phi;</li>
     * <li>&not; (&forall;<sub>p</sub> x : &phi;) = &exist;<sub>p</sub> x: &not;
     * &phi;</li>
     * </ul>
     * The identities for CTL operators are similar to the LTL ones; simply
     * replace the A by E and vice-versa; for example, &not AG &phi; = EF &not;
     * &phi; and &not EG &phi; = AF &not; &phi;.
     * 
     * @return An equivalent Operator in NNF.
     */
	public virtual Operator getNegatedNormalForm()
	{
		return this;
	}

    /**
     * Returns the set of quantified variables in a formula.
     * 
     * @return A set of Atoms, these atoms being the quantified variables in the
     *         formula.
     */
	public virtual HashSet<Atom> getQuantifiedVariables()
	{
		return new HashSet<Atom>();
	}

    /**
     * Computes the list of quantified variables in the formula.
     * 
     * @return A Map which associates, for each variable name, the qualified
     *         fields over which it quantifies.
     */
	public virtual Dictionary<string, string> getVariableAssociations()
	{
		return null;
	}

    /**
     * Re-implementation of the hashCode method of class Object, so that object
     * equality is not referential equality, but content equality.
     * 
     * @return 0, always
     * @see http
     *      ://www.javaworld.com/javaworld/jw-01-1999/jw-01-object.html?page=4
     */
	public override int GetHashCode()
	{
		return 0;
	}

    /**
     * Outputs a string rendition of the operator and of all its sub-operators
     * in a recursive fashion.
     * NOTE: since the string contains multiple mathematical characters, it is
     * encoded using Unicode (UTF-8). To display the characters, you must use
     * a font that contains all the glyphs, such as Unifont:
     * http://unifoundry.com/unifont.html
     * 
     * 
     * @return A String output of the operator
     */
	public override string ToString ()
	{
		return toString("");
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
	public virtual string toString(string indent)
	{
		return "";
	}
	
	//public override string ToHTML()
}
