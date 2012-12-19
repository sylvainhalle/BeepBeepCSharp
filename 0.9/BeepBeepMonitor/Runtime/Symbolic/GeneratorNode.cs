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
using System.Text;

/**
 * Private inner class used to represent the watcher's state.
 * The GeneratorNode2 adds a third set, the set of "OPluses"
 * 
 * @author Sylvain Hallé
 * @version 2010-12-09
 */
public class GeneratorNode : SequentNode
{
	protected HashSet<OPlus> m_opluses;
	
	/**
	 * This flag can be set to false if the node has been generated
	 * without the guarantee that it is sound.
	 * More precisely: soundness is lost exactly
	 * when an existential quantifier
	 * is evaluated after a universal quantifier with the same path. 
	 */
	protected bool m_sound = true;
	public bool Sound
	{
		get{ return this.m_sound; }
	}
	
	/**
   	 * Remembers the set of encountered qualifiers for the
   	 * current round of decompositions
   	 */
	protected HashSet<string> m_encounteredQualifiers;
	
	/**
	 * Remembers whether a universal quantifier has been evaluated
	 * during the decomposition of this branch.
	 */
	protected bool m_decomposedAForAll = false;
	
	/**
	 * If this flag is set to true, performs a depth-first
	 * exploration of the decomposition tree and terminates
	 * as soon as one node is generated.
	 */
	protected bool m_shortcut = false;
	
	/**
	 * Default constructor.
	 */
	public GeneratorNode() : base()
	{
		m_opluses = new HashSet<OPlus>();
		m_encounteredQualifiers = new HashSet<string>();
	}
	
	/**
	 * Constructs a GeneratorNode2 based on another GeneratorNode2. (NOTE: deep or
	 * shallow copy?)
	 * 
	 * @param wn
	 *          The GeneratorNode2 of which to make a copy.
	 */
	public GeneratorNode(GeneratorNode wn) : this()
	{
		m_gamma = new HashSet<Operator>();
		m_delta = new HashSet<Operator>();
		m_opluses = new HashSet<OPlus>();
		m_encounteredQualifiers = new HashSet<string>();
		m_decomposedAForAll = wn.m_decomposedAForAll;
		m_sound = wn.m_sound;
	}
	
	/**
	 * Checks if the set of all &oplus; conditions contains a
	 * contradiction. A contradiction occurs when:
	 * <ol>
	 * <li>there exist
	 * two terms &oplus;<sub><i>p</i></sub>&nbsp;<i>k</i><sub>1</sub> and
	 * &oplus;<sub><i>p</i></sub>&nbsp;<i>k</i><sub>2</sub> for
	 * <i>k</i><sub>1</sub> &ne; <i>k</i><sub>2</sub>, <em>unless</em>
	 * <i>p</i> allows multiple cardinalities.</li>
	 * </ol>
	 * Currently this is the only condition considered.
	 * @return True if the node contains a contradiction, false otherwise.
	 */
	public bool containsOPlusContradiction()
	{
		HashSet<string> encounteredQualifiers = new HashSet<string>();
		
		foreach (OPlus op in m_opluses)
		{
			string qualifier = op.getQualifier();
			
			if (encounteredQualifiers.Contains(qualifier))
			{
				return true;
			}
		}
		
		return false;
	}
	
	public override bool Equals(object o)
	{
		if (o.GetType() != typeof(GeneratorNode))
		{
			return false;
		}
		
		GeneratorNode wn = (GeneratorNode)o;
		
		if (m_gamma.Equals(wn.m_gamma) && 
			m_delta.Equals(wn.m_delta) && 
			m_opluses.Equals(wn.m_opluses))
		{
			return true;
		}
		
		return false;
	}
	
	public override int GetHashCode()
	{
		return (m_gamma.Count + m_delta.Count + m_opluses.Count);
	}
	
	protected bool addToOPluses(OPlus o)
	{
		if (o == null)
		{
			return false;
		}
		
		if (m_opluses.Count == 0)
		{
			m_opluses.Add(o);
			
			return true;
		}
		
		// We prevent the OPlus to be added if it does not have the same root
    	// as the other OPluses
		string qualifier = o.getQualifier();
		string[] pathParts = qualifier.Split('/');
		string prefix = "/" + pathParts[1];
		
		foreach (OPlus op in m_opluses)
		{
			if (!op.getQualifier().StartsWith(prefix))
			{
				if (!op.getOperand().Equals(Operator.m_falseAtom) && 
					!o.getOperand().Equals(Operator.m_falseAtom))
				{
					// We try to add elements that belong to different
          			// message schemas! Stop there.
					return false;
				}
			}
			
			if (o.getQualifier().StartsWith(op.getQualifier()))
			{
				if (op.getOperand().Equals(Operator.m_falseAtom))
				{
					// We try to add an element while a condition tells us we shouldn't
					return false;
				}
			}
			
			if (o.getQualifier().StartsWith(op.getQualifier()))
			{
				if (o.getOperand().Equals(Operator.m_falseAtom))
				{
					return false;
				}
			}
			
			m_opluses.Add(o);
			
			return true;
		}
		
		return true;
	}
	
	public new void addToDelta(Operator o)
	{
		base.addToDelta(o);
		
		if (o.GetType() == typeof(FOExists))
		{
			if (((FOExists)o).isPathAssertion() && 
				((FOExists)o).getQualifier() == "/confirmed")
			{
				System.Console.WriteLine("PELLETE");
			}
		}
	}
	
	public HashSet<GeneratorNode> spawn(Operator o)
	{
		if (o.GetType() == typeof(Constant)) { return spawn((Constant)o); }
		else if (o.GetType() == typeof(FOExists)) { return spawn((FOExists)o); }
		else if (o.GetType() == typeof(FOForAll)) { return spawn((FOForAll)o); }
		else if (o.GetType() == typeof(OperatorAnd)) { return spawn((OperatorAnd)o); }
		else if (o.GetType() == typeof(OperatorEquals)) { return spawn((OperatorEquals)o); }
		else if (o.GetType() == typeof(OperatorF)) { return spawn((OperatorF)o); }
		else if (o.GetType() == typeof(OperatorG)) { return spawn((OperatorG)o); }
		else if (o.GetType() == typeof(OperatorImplies)) { return spawn((OperatorImplies)o); }
		else if (o.GetType() == typeof(OperatorNot)) { return spawn((OperatorNot)o); }
		else if (o.GetType() == typeof(OperatorOr)) { return spawn((OperatorOr)o); }
		else if (o.GetType() == typeof(OperatorU)) { return spawn((OperatorU)o); }
		else if (o.GetType() == typeof(OperatorV)) { return spawn((OperatorV)o); }
		else if (o.GetType() == typeof(OperatorX)) { return spawn((OperatorX)o); }
		else { return null; }
	}
	
	public HashSet<GeneratorNode> spawn(FOExists op)
	{
		HashSet<GeneratorNode> outSet = new HashSet<GeneratorNode>();
		Atom x = op.getQuantifiedVariable();
		string qualifier = op.getQualifier();
		
		if (m_encounteredQualifiers.Contains(qualifier))
		{
			// We add something along a path where a ForAll has already
      		// been evaluated: soundness is no longer guaranteed for this node
			m_sound = false;
		}
		
		if (op.isAnOPlus())
		{
			// This is an OPlus; return a node with op transferred to the OPlus set
			GeneratorNode wn = new GeneratorNode(this);
			
			if (!wn.addToOPluses(op.toOPlus()))
			{
				// We can't add this OPlus to the current set. Contradiction! Return the empty set
				return new HashSet<GeneratorNode>();
			}
			
			return wn.spawn();
		}
		
		if (op.isPathAssertion())
		{
			GeneratorNode wn = new GeneratorNode();
			OPlus opl = new OPlus();
			
			opl.setQualifier(op.getQualifier());
			opl.setOperand(Operator.m_trueAtom);
			
			if (!wn.addToOPluses(opl))
			{
				// We can't add this OPlus to the current set. Contradiction! Return the empty set
				return new HashSet<GeneratorNode>();
			}
			
			return wn.spawn();
		}
		
		// Iterate over domain
		//HashSet<Constant> oplus_domain = getOPlusDomain(qualifier);
		HashSet<Constant> domain = op.getDomain();
		SubsetIterator<Constant> it = new SubsetIterator<Constant>(domain); //,oplus_domain
		
		while (it.hasNext())
		{
			GeneratorNode wn = new GeneratorNode(this);
			HashSet<Constant> subset = it.next();
			
			foreach (Atom v in subset)
			{
				Operator o2 = op .getOperand();
				Operator o3 = o2.evaluate(x, v);
				
				if (!op.isPathAssertion())
				{
					wn.addToGamma(o3);
				}
				
				OPlus opl = new OPlus(qualifier, v);
				
				if (!wn.addToOPluses(opl))
				{
					// We can't add this OPlus to the current set. Contradiction! Skip that branch
					continue;
				}
			}
			
			HashSet<GeneratorNode> spawnedSet = wn.spawn();

            if (spawnedSet != null)
            {
                foreach (GeneratorNode gn in spawnedSet)
                {
                    outSet.Add(gn);
                }
            }
		}
		
		return outSet;
	}
	
	public HashSet<GeneratorNode> spawn(FOForAll op)
	{
		HashSet<GeneratorNode> spawnedSet, outSet = new HashSet<GeneratorNode>();
		Atom x = op.getQuantifiedVariable();
		string qualifier = op.getQualifier();
		
		// Iterate over domain
		HashSet<Constant> oplus_domain = getOPlusDomain(qualifier);
		HashSet<Constant> domain = op.getDomain();
		SubsetIterator<Constant> it;
		
		if (!m_encounteredQualifiers.Contains(qualifier))
		{
			// We haven't decomposed a For All in the past, so we can
      		// add elements to the message
			it = new SubsetIterator<Constant>(domain, oplus_domain);
		}
		
		else
		{
			// Otherwise, we stick to the elements we already have to
      		// evaluate this quantifier
			it = new SubsetIterator<Constant>(oplus_domain);
		}
		
		m_encounteredQualifiers.Add(op.getQualifier());
		m_decomposedAForAll = true;
		
		if (op.isPathNegation())
		{
			// The quantifier asserts the absence of a path
			GeneratorNode wn = new GeneratorNode(this);
			OPlus opl = new OPlus();
			
			opl.setQualifier(op.getQualifier());
			opl.setOperand(Operator.m_falseAtom);
			
			if (!wn.addToOPluses(opl))
			{
				// We can't add this OPlus to the current set. Contradiction! Return the empty set
				return new HashSet<GeneratorNode>();
			}
			
			return wn.spawn();
		}
		
		if (op.isPathAssertion())
		{
			// In negated form, the quantifier may assert the existence of a path
			GeneratorNode wn = new GeneratorNode(this);
			OPlus opl = new OPlus();
			
			opl.setQualifier(op.getQualifier());
			opl.setOperand(Operator.m_trueAtom);
			
			if (!wn.addToOPluses(opl))
			{
				// We can't add this OPlus to the current set. Contradiction! Return the empty set
				return new HashSet<GeneratorNode>();
			}
			
			return wn.spawn();
		}
		
		while (it.hasNext())
		{
			GeneratorNode wn = new GeneratorNode(this);
			HashSet<Constant> subset = it.next();
			
			foreach (Atom v in subset)
			{
				Operator o2 = op.getOperand();
				Operator o3 = o2.evaluate(x, v);
				OPlus opl = new OPlus(qualifier, v);
				
				wn.addToGamma(o3);
				
				if (!wn.addToOPluses(opl))
				{
					// Contradiction! Skip that branch
					continue;
				}
			}
			
			spawnedSet = wn.spawn();

            if (spawnedSet != null)
            {
                foreach (GeneratorNode gn in spawnedSet)
                {
                    outSet.Add(gn);
                }
            }
		}
		
		return outSet;
	}
	
	public HashSet<GeneratorNode> spawn(OperatorOr op)
	{
		HashSet<GeneratorNode> spawnedSet, outSet = new HashSet<GeneratorNode>();
		
		// Do for left operand
		GeneratorNode wn = new GeneratorNode(this);
		Operator o2 = op.getLeftOperand();
		
		wn.addToGamma(o2);
		spawnedSet = wn.spawn();
		
		foreach (GeneratorNode gn in spawnedSet)
		{
			outSet.Add(gn);
		}
		
		// Do for right operand
		wn = new GeneratorNode(this);
		o2 = op.getRightOperand();
		wn.addToGamma(o2);
		spawnedSet = wn.spawn();

        if (spawnedSet != null)
        {
            foreach (GeneratorNode gn in spawnedSet)
            {
                outSet.Add(gn);
            }
        }
		
		return outSet;
	}
	
	public HashSet<GeneratorNode> spawn(OperatorImplies op)
	{
		HashSet<GeneratorNode> spawnedSet, outSet = new HashSet<GeneratorNode>();
		
		// Do for right operand
		GeneratorNode wn = new GeneratorNode(this);
		Operator o2 = op.getRightOperand();
		
		wn.addToGamma(o2);
		spawnedSet = wn.spawn();

        if (spawnedSet != null)
        {
            foreach (GeneratorNode gn in spawnedSet)
            {
                outSet.Add(gn);
            }
        }
		
		// Do for right operand
		wn = new GeneratorNode(this);
		o2 = op.getLeftOperand().getNegated();
		wn.addToGamma(o2.getNegatedNormalForm());
		spawnedSet = wn.spawn();

        if (spawnedSet != null)
        {
            foreach (GeneratorNode gn in spawnedSet)
            {
                outSet.Add(gn);
            }
        }
		
		// Do for both (NEW)
		wn = new GeneratorNode(this);
		o2 = op.getRightOperand();
		wn.addToGamma(o2);
		o2 = op.getLeftOperand();
		wn.addToGamma(o2);
		spawnedSet = wn.spawn();

        if (spawnedSet != null)
        {
            foreach (GeneratorNode gn in spawnedSet)
            {
                outSet.Add(gn);
            }
        }
		
		return outSet;
	}
	
	public HashSet<GeneratorNode> spawn(OperatorAnd op)
	{
		HashSet<GeneratorNode> spawnedSet, outSet = new HashSet<GeneratorNode>();
		
		// Do for left operand
		GeneratorNode wn = new GeneratorNode(this);
		Operator o2 = op.getLeftOperand();
		
		wn.addToGamma(o2);
		
		// Do for left operand
		o2 = op.getRightOperand();
		wn.addToGamma(o2);
		spawnedSet = wn.spawn();

        if (spawnedSet != null)
        {
            foreach (GeneratorNode gn in spawnedSet)
            {
                outSet.Add(gn);
            }
        }
		
		return outSet;
	}
	
	public HashSet<GeneratorNode> spawn(OperatorX op)
	{
		HashSet<GeneratorNode> spawnedSet, outSet = new HashSet<GeneratorNode>();
		GeneratorNode wn = new GeneratorNode(this);
		Operator o2 = op.getOperand();
		
		wn.addToDelta(o2);
		spawnedSet = wn.spawn();

        if (spawnedSet != null)
        {
            foreach (GeneratorNode gn in spawnedSet)
            {
                outSet.Add(gn);
            }
        }
		
		return outSet;
	}
	
	public HashSet<GeneratorNode> spawn(OperatorG op)
	{
		HashSet<GeneratorNode> spawnedSet, outSet = new HashSet<GeneratorNode>();
		
		// Do for left operand
		GeneratorNode wn = new GeneratorNode(this);
		Operator o2 = op.getOperand();
		
		wn.addToGamma(o2);
		wn.addToDelta(op);
		spawnedSet = wn.spawn();

        if (spawnedSet != null)
        {
            foreach (GeneratorNode gn in spawnedSet)
            {
                outSet.Add(gn);
            }
        }
		
		return outSet;
	}
	
	public HashSet<GeneratorNode> spawn(OperatorF op)
	{
		HashSet<GeneratorNode> spawnedSet, outSet = new HashSet<GeneratorNode>();
		
		// Do for left node
		GeneratorNode wn = new GeneratorNode(this);
		Operator o2 = op.getOperand();
		
		wn.addToGamma(o2);
		spawnedSet = wn.spawn();

        if (spawnedSet != null)
        {
            foreach (GeneratorNode gn in spawnedSet)
            {
                outSet.Add(gn);
            }
        }
		
		// Do for right node
		wn = new GeneratorNode(this);
		wn.addToDelta(op);
		spawnedSet = wn.spawn();

        if (spawnedSet != null)
        {
            foreach (GeneratorNode gn in spawnedSet)
            {
                outSet.Add(gn);
            }
        }
		
		return outSet;
	}
	
	public HashSet<GeneratorNode> spawn(OperatorU op)
	{
		HashSet<GeneratorNode> spawnedSet, outSet = new HashSet<GeneratorNode>();
		
		// Do for left node
		GeneratorNode wn = new GeneratorNode(this);
		Operator o2 = op.getRightOperand();
		
		wn.addToGamma(o2);
		spawnedSet = wn.spawn();

        if (spawnedSet != null)
        {
            foreach (GeneratorNode gn in spawnedSet)
            {
                outSet.Add(gn);
            }
        }
		
		// Do for right node
		wn = new GeneratorNode(this);
		wn.addToDelta(op);
		spawnedSet = wn.spawn();

        if (spawnedSet != null)
        {
            foreach (GeneratorNode gn in spawnedSet)
            {
                outSet.Add(gn);
            }
        }
		
		return outSet;
	}
	
	public HashSet<GeneratorNode> spawn(OperatorV op)
	{
		HashSet<GeneratorNode> spawnedSet, outSet = new HashSet<GeneratorNode>();
		
		// Do for left node
		GeneratorNode wn = new GeneratorNode(this);
		Operator o2 = op.getLeftOperand();
		
		wn.addToGamma(o2);
		o2 = op.getRightOperand();
		wn.addToGamma(o2);
		spawnedSet = wn.spawn();

        if (spawnedSet != null)
        {
            foreach (GeneratorNode gn in spawnedSet)
            {
                outSet.Add(gn);
            }
        }
		
		// Do for right node
		wn = new GeneratorNode(this);
		o2 = op.getRightOperand();
		wn.addToGamma(o2);
		wn.addToDelta(op);
		spawnedSet = wn.spawn();

        if (spawnedSet != null)
        {
            foreach (GeneratorNode gn in spawnedSet)
            {
                outSet.Add(gn);
            }
        }
		
		return outSet;
	}
	
	public HashSet<GeneratorNode> spawn(OperatorEquals op)
	{
		HashSet<GeneratorNode> outSet = new HashSet<GeneratorNode>();
		
		// This should never happen! When down to the evaluation
    	// of an equality, all variables should be evaluated!
		//assert(false);
		return outSet;
	}
	
	public HashSet<GeneratorNode> spawn(OperatorNot op)
	{
		HashSet<GeneratorNode> spawnedSet, outSet = new HashSet<GeneratorNode>();
		
		// Do for operand
		Operator o2 = op.getOperand();
		
		/*if (o2.GetType() != typeof(Constant))
		{
			// This should not happen! Negations should be pushed
      		// to atoms
			assert(false);
		}*/
		
		// TODO: true and false are checked by comparing their
	    // string representations; there should be a more graceful
	    // way
	    // to check for true and false
		/*else*/if (((Constant)o2).getSymbol() == Operator.m_trueAtom.getSymbol())
		{
			// Constant TRUE, i.e. evaluates to FALSE: this branch
			// does not return anything
			// i.e. do nothing
		}
		
		else if (((Constant)o2).getSymbol() == Operator.m_falseAtom.getSymbol())
		{
			// Constant FALSE, i.e. evaluates to TRUE: just pass on
      		// the recursive evaluation
			spawnedSet = spawn();

            if (spawnedSet != null)
            {
                foreach (GeneratorNode gn in spawnedSet)
                {
                    outSet.Add(gn);
                }
            }
		}
		
		/*else
		{
			// This should never happen! All atoms should evaluate
     	 	// down
      		// to either true or false.
			assert(false);
		}*/
		
		return outSet;
	}
	
	public HashSet<GeneratorNode> spawn(Constant op)
	{
		HashSet<GeneratorNode> spawnedSet, outSet = new HashSet<GeneratorNode>();
		
		// TODO: true and false are checked by comparing their
	    // string representations; there should be a more graceful
	    // way
	    // to check for true and false
		if (op.getSymbol() == Operator.m_trueAtom.getSymbol())
		{
			// Constant TRUE, just pass on the recursive evaluation
			spawnedSet = spawn();

            if (spawnedSet != null)
            {
                foreach (GeneratorNode gn in spawnedSet)
                {
                    outSet.Add(gn);
                }
            }
		}
		
		else if (op.getSymbol() == Operator.m_falseAtom.getSymbol())
		{
		}
		
		// Constant FALSE, return empty set
		/*else
		{
			// This should never happen! All atoms should evaluate
			// down
			// to either true or false.
			assert(false);
		}*/
		
		return outSet;
	}
	
	/**
	 * Decomposes a watcher's node, given a received message.
	 * 
	 * @param m
	 *          The message on which to make the decomposition
	 * @return A Set of GeneratorNode2s consisting of the resulting decomposition
	 *         for that node. If the left-hand side of the node is empty,
	 *         returns an empty set.
	 */
	public HashSet<GeneratorNode> spawn()
	{
		HashSet<GeneratorNode> outSet = new HashSet<GeneratorNode>();
		FOForAll fotemp = null;
		
		// If we have an empty node, nothing left to span; just
    	// return ourself
		if (m_gamma.Count == 0)
		{
			outSet.Add(this);
			
			return outSet;
		}
		
		// Otherwise, there remain operators to decompose, pick one
		foreach (Operator o in m_gamma)
		{
			// Picks a formula, removes it from Gamma, and decomposes
			if (o.GetType() != typeof(FOForAll) || 
				((FOForAll)o).isPathAssertion())
			{
				// If the operator we pick is not a [], we decompose it 
				m_gamma.Remove(o);
				
				return spawn(o);
			}
			
			else
			{
				// Otherwise, we keep it in reserve in case we find
        		// other operators
				fotemp = (FOForAll)o;
			}
		}
		
		// If we are here, it means that all operators left
		// are universal quantifiers, so we pick the one in memory
		// (this effectively postpones the evaluation of universals
		// at the end)
		m_gamma.Remove(fotemp);
		
		return spawn(fotemp);
	}
	
	/**
	 * Returns the set of values, present in the current OPlus set
	 * @param qualifier
	 * @return
	 */
	public HashSet<OPlus> getOPluses()
	{
		return m_opluses;
	}
	
	private HashSet<Constant> getOPlusDomain(string qualifier)
	{
		HashSet<Constant> hOut = new HashSet<Constant>();
		
		foreach (OPlus op in m_opluses)
		{
			if (qualifier == op.getQualifier())
			{
				hOut.Add((Constant)op.getOperand());
			}
		}
		
		return hOut;
	}
	
	public override string ToString()
	{
		StringBuilder sbOut = new StringBuilder("{");
		
		if (m_opluses == null || m_opluses.Count == 0)
		{
			sbOut.Append("\u2205");
		}
		
		else
		{
			bool first = true;
			
			foreach (OPlus mop in m_opluses)
			{
				if (!first)
				{
					sbOut = sbOut.Append(", ");
				}
				
				first = false;
				sbOut = sbOut.Append(mop.ToString());
			}
		}
		
		sbOut = sbOut.Append(" : ");
		
		if (m_gamma == null || m_gamma.Count == 0)
		{
			sbOut = sbOut.Append("\u2205");
		}
		
		else
		{
			bool first = true;
			
			foreach (Operator o in m_gamma)
			{
				if (!first)
				{
					sbOut = sbOut.Append(", ");
				}
				
				first = false;
				sbOut = sbOut.Append(o.ToString());
			}
		}
		
		sbOut = sbOut.Append(" \u22A7 ");
		
		if (m_delta == null || m_delta.Count == 0)
		{
			sbOut = sbOut.Append("\u2205");
		}
		
		else
		{
			bool first = true;
			
			foreach (Operator o in m_delta)
			{
				if (!first)
				{
					sbOut = sbOut.Append(", ");
				}
				
				first = false;
				sbOut = sbOut.Append(o.ToString());
			}
		}
		
		return sbOut.ToString();
	}
}
