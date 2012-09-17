/******************************************************************************
Runtime monitors for message-based workflows
Copyright (C) 2008 Sylvain Halle

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

public class SequentNode
{
	/**
	 * The set of LTL-FO+ formulae that have to be true on the current state
	 */
	protected HashSet<Operator> m_gamma;
	
	/**
	 * The set of LTL-FO+ formulae that have to be true on the next state
	 */
	protected HashSet<Operator> m_delta;
	
	/**
	 * Default constructor.
	 */
	public SequentNode() : base()
	{
		m_gamma = new HashSet<Operator>();
		m_delta = new HashSet<Operator>();
	}
	
	/**
	 * Constructor specifying the left and right parts of the node.
	 * 
	 * @param gamma
	 *          The left set of formulae
	 * @param delta
	 *          The right set of formulae
	 */
	public SequentNode(HashSet<Operator> gamma, HashSet<Operator> delta) : this()
	{
		m_gamma = new HashSet<Operator>(gamma);
		m_delta = new HashSet<Operator>(delta);
	}
	
	/**
	 * Determines the value of an LTL-FO+ formula using a 3-valued logic. In
	 * addition to TRUE and FALSE, we add a third value "?". The following rules
	 * apply:
	 * <ul>
	 * <li>&not; ? = ?</li>
	 * <li>TRUE &and; ? = ?</li>
	 * <li>FALSE &and; ? = FALSE</li>
	 * <li>TRUE &or; ? = TRUE</li>
	 * <li>FALSE &or; ? = ?</li>
	 * <li>X &phi; = G &phi; = ? no matter &phi;</li>
	 * <li>F &phi; = &phi; &or; ?</li>
	 * <li>&phi; U &psi; = &psi; &or; ?</li>
	 * <li>&exist;<sub>p</sub> x: &phi; = &phi;(x<sub>1</sub>) &or; ... &or;
	 * &phi;(x<sub>n</sub>) for x<sub>i</sub> in Dom<sub>s</sub>(p)</li>
	 * <li>&forall;<sub>p</sub> x: &phi; = &phi;(x<sub>1</sub>) &and; ... &and;
	 * &phi;(x<sub>n</sub>) for x<sub>i</sub> in Dom<sub>s</sub>(p)</li>
	 * 
	 * @param o
	 * @param m
	 *          TODO
	 * @return
	 */
	protected static Outcome isSatisfiedInCurrentState(Operator o, WatcherMessage m)
	{
		// This method uses getClass to branch on the type of operator
    	// used; TODO: redesign with method overriding
		Outcome oc1, oc2;
		Operator o1, o2, o3;
		
		if (o.GetType() == typeof(OperatorNot))
		{
			o1 = ((OperatorNot)o).getOperand();
			oc1 = isSatisfiedInCurrentState(o1, m);
			
			return threeValuedNot(oc1);
		}
		
		else if (o.GetType() == typeof(OperatorX) || 
			o.GetType() == typeof(OperatorG) ||
			o.GetType() == typeof(OperatorEquals))
		{
			// TODO
			// assert(false);
			return Outcome.INCONCLUSIVE;
		}
		
		else if (o.GetType() == typeof(OperatorAnd))
		{
			o1 = ((OperatorAnd)o).getLeftOperand();
			oc1 = isSatisfiedInCurrentState(o1, m);
			
			o2 = ((OperatorAnd)o).getRightOperand();
			oc2 = isSatisfiedInCurrentState(o2, m);
			
			return threeValuedAnd(oc1, oc2);
		}
		
		else if (o.GetType() == typeof(OperatorOr))
		{
			o1 = ((OperatorOr)o).getLeftOperand();
			oc1 = isSatisfiedInCurrentState(o1, m);
			
			o2 = ((OperatorOr)o).getRightOperand();
			oc2 = isSatisfiedInCurrentState(o2, m);
			
			return threeValuedOr(oc1, oc2);
		}
		
		else if (o.GetType() == typeof(OperatorF))
		{
			o1 = ((OperatorF)o).getOperand();
			oc1 = isSatisfiedInCurrentState(o1, m);
			
			return threeValuedOr(oc1, Outcome.INCONCLUSIVE);
		}
		
		else if (o.GetType() == typeof(OperatorImplies))
		{
			o1 = ((OperatorImplies)o).getLeftOperand();
			oc1 = isSatisfiedInCurrentState(o1, m);
			
			o2 = ((OperatorImplies)o).getRightOperand();
			oc2 = isSatisfiedInCurrentState(o2, m);
			
			return threeValuedOr(oc1, oc2);
		}
		
		else if (o.GetType() == typeof(OperatorU))
		{
			o1 = ((OperatorU)o).getLeftOperand();
			oc1 = isSatisfiedInCurrentState(o1, m);
			
			o2 = ((OperatorU)o).getRightOperand();
			oc2 = isSatisfiedInCurrentState(o2, m);
			
			return threeValuedOr(oc1, threeValuedAnd(oc1, Outcome.INCONCLUSIVE));
		}
		
		else if (o.GetType() == typeof(OperatorV))
		{
			o1 = ((OperatorV)o).getLeftOperand();
			oc1 = isSatisfiedInCurrentState(o1, m);
			
			o2 = ((OperatorV)o).getRightOperand();
			oc2 = isSatisfiedInCurrentState(o2, m);
			
			return threeValuedAnd(oc1, threeValuedOr(oc2, Outcome.INCONCLUSIVE));
		}
		
		else if (o.GetType() == typeof(FOExists))
		{
			// Iterate over domain
      		// TODO: supposes that the string qualifier is an atom
			Atom p = new Atom(((FOExists)o).getQualifier());
			Atom x = ((FOExists)o).getQuantifiedVariable();
			HashSet<Atom> s = m.getDomain(p);
			
			oc1 = Outcome.FALSE;
			
			foreach (Atom a in s)
			{
				o2 = ((FOExists)o).getOperand();
				o3 = o2.evaluate(x, a);
				oc2 = isSatisfiedInCurrentState(o3, m);
				oc1 = threeValuedOr(oc1, oc2);
			}
			
			return oc1;
		}
		
		else if (o.GetType() == typeof(FOForAll))
		{
			// Iterate over domain
      		// TODO: supposes that the string qualifier is an atom
			Atom p = new Atom(((FOForAll)o).getQualifier());
			Atom x = ((FOForAll)o).getQuantifiedVariable();
			HashSet<Atom> s = m.getDomain(p);
			
			oc1 = Outcome.FALSE;
			
			foreach (Atom a in s)
			{
				o2 = ((FOForAll)o).getOperand();
				o3 = o2.evaluate(x, a);
				oc2 = isSatisfiedInCurrentState(o3, m);
				oc1 = threeValuedAnd(oc1, oc2);
			}
			
			return oc1;
		}
		
		else if (o.GetType() == typeof(Constant))
		{
			// TODO: true and false are checked by comparing their
			// string representations; there should be a more graceful
			// way to check for true and false
			if (((Constant)o).getSymbol() == Operator.m_trueAtom.getSymbol())
			{
				return Outcome.TRUE;
			}
			
			else if (((Constant)o).getSymbol() == Operator.m_falseAtom.getSymbol())
			{
				return Outcome.FALSE;
			}
			
			return Outcome.INCONCLUSIVE;
		}
		
		return Outcome.INCONCLUSIVE;
	}
	
	protected static Outcome threeValuedAnd(Outcome o1, Outcome o2)
	{
		if (o1 == Outcome.FALSE || o2 == Outcome.FALSE)
		{
			return Outcome.FALSE;
		}
		
		if (o1 == Outcome.TRUE && o2 == Outcome.TRUE)
		{
			return Outcome.TRUE;
		}
		
		return Outcome.INCONCLUSIVE;
	}
	
	protected static Outcome threeValuedNot(Outcome o1)
	{
		if (o1 == Outcome.FALSE)
		{
			return Outcome.TRUE;
		}
		
		if (o1 == Outcome.TRUE)
		{
			return Outcome.FALSE;
		}
		
		return Outcome.INCONCLUSIVE;
	}
	
	protected static Outcome threeValuedOr(Outcome o1, Outcome o2)
	{
		if (o1 == Outcome.FALSE && o2 == Outcome.FALSE)
		{
			return Outcome.FALSE;
		}
		
		if (o1 == Outcome.TRUE || o2 == Outcome.TRUE)
		{
			return Outcome.TRUE;
		}
		
		return Outcome.INCONCLUSIVE;
	}
	
	/**
	 * Adds an Operator to the set Delta of the current watcher node.
	 * 
	 * @param o
	 *          The Operator to add.
	 */
	public void addToDelta(Operator o)
	{
		m_delta.Add(o);
	}
	
	/**
	 * Adds an Operator to the set Gamma of the current watcher node.
	 * 
	 * @param o
	 *          The Operator to add.
	 */
	public void addToGamma(Operator o)
	{
		m_gamma.Add(o);
	}
	
	public override bool Equals(object o)
	{
		if (o.GetType() != typeof(GeneratorNode))
		{
			return false;
		}
		
		SequentNode wn = (SequentNode)o;
		
		if (m_gamma.Equals(wn.m_gamma) && m_delta.Equals(wn.m_delta))
		{
			return true;
		}
		
		return false;
	}
	
	/**
	 * Gets the total "atom" size of a node. The size of a node is the sum of the
	 * lengths of formulas inside Gamma + the sum of the lengths of formulae
	 * inside Delta, as computed by
	 * {@link ca.uqam.info.logic.Operator#getLength()}.
	 * 
	 * @return The size
	 */
	public int getAtoms()
	{
		int size = 0;
		
		foreach (Operator op in m_gamma)
		{
			size += op.getLength();
		}
		
		foreach (Operator op in m_delta)
		{
			size += op.getLength();
		}
		
		return size;
	}
	
	/**
	 * Gets the set Delta.
	 * 
	 * @return The right-hand set of LTL-FO+ formulae
	 */
	public HashSet<Operator> getDelta()
	{
		return m_delta;
	}
	
	/**
	 * Gets the set Gamma.
	 * 
	 * @return The left-hand set of LTL-FO+ formulae
	 */
	public HashSet<Operator> getGamma()
	{
		return m_gamma;
	}
	
	/**
	 * Gets the total size of a node. The size of a node is the number of formulae
	 * inside Gamma + the number of formulae inside Delta.
	 * 
	 * @return The size
	 */
	public int getSize()
	{
		return (m_gamma.Count + m_delta.Count);
	}
	
	public override int GetHashCode ()
	{
		return getSize();
	}
	
	/**
	 * Sets the set Delta.
	 * 
	 * @param gamma
	 *          The right-hand set of LTL-FO+ formulae
	 */
	public void setDelta(HashSet<Operator> delta)
	{
		m_delta.Clear();
		
		foreach (Operator op in delta)
		{
			m_delta.Add(op);
		}
	}
	
	/**
	 * Sets the set Gamma.
	 * 
	 * @param gamma
	 *          The left-hand set of LTL-FO+ formulae
	 */
	public void setGamma(HashSet<Operator> gamma)
	{
		m_gamma.Clear();
		
		foreach (Operator op in gamma)
		{
			m_gamma.Add(op);
		}
	}
}
