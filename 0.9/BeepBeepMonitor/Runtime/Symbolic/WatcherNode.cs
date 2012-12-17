using System.Collections;
using System.Collections.Generic;

/**
 * Private inner class used to represent the watcher's state.
 * 
 * @author Sylvain Hallé
 * @version 2008-04-24
 */
public class WatcherNode : SequentNode
{
	/**
	 * Default constructor.
	 */
	public WatcherNode() : base()
	{
	}
	
	/**
	 * Constructor specifying the left and right parts of the node.
	 * 
	 * @param gamma
	 *          The left set of formulae
	 * @param delta
	 *          The right set of formulae
	 */
	public WatcherNode(HashSet<Operator> gamma, HashSet<Operator> delta) : base(gamma, delta)
	{
	}
	
	/**
	 * Constructs a WatcherNode based on another WatcherNode. (NOTE: deep or
	 * shallow copy?)
	 * 
	 * @param wn
	 *          The WatcherNode of which to make a copy.
	 */
	public WatcherNode(WatcherNode wn) : base(wn.getGamma(), wn.getDelta())
	{
	}
	
	public override bool Equals (object o)
	{
		if (o.GetType() != typeof(WatcherNode))
		{
			return false;
		}
		
		WatcherNode wn = (WatcherNode)o;
		
		if (m_gamma.Equals(wn.m_gamma) && 
			m_delta.Equals(wn.m_delta))
		{
			return true;
		}
		
		return false;
	}
	
	/**
	 * Decomposes a watcher's node, given a received message.
	 * 
	 * @param m
	 *          The message on which to make the decomposition
	 * @return A Set of WatcherNodes consisting of the resulting decomposition
	 *         for that node. If the left-hand side of the node is empty,
	 *         returns an empty set.
	 */
	public HashSet<WatcherNode> spawn(WatcherMessage m)
	{
		// This method uses getClass to branch on the type of operator
    	// used; TODO: redesign with method overriding
		HashSet<WatcherNode> spawnedSet, outSet = new HashSet<WatcherNode>();
		WatcherNode wn;
		Operator o1, o2, o3;
		
		// Picks a formula and removes it from Gamma
		foreach (Operator o in m_gamma)
		{
			m_gamma.Remove(o);
			
			// Optimization from original algorithm: if formula can readily
      		// be decided in current state, bypass decomposition
			Outcome oc = isSatisfiedInCurrentState(o, m);
			
			if (oc == Outcome.TRUE)
			{
				// Formula is true: stop decomposition of this formula and
        		// continue spawning
				return spawn(m);
			}
			
			else if (oc == Outcome.FALSE)
			{
				// Formula is false: stop branch and return empty set
				return outSet;
			}
			
			// Operator OR
			else if (o.GetType() == typeof(OperatorOr))
			{
				// Do for left operand
				wn = new WatcherNode(this);
				o2 = ((OperatorOr)o).getLeftOperand();
				wn.addToGamma(o2);
				spawnedSet = wn.spawn(m);
				
				foreach (WatcherNode wn2 in spawnedSet)
				{
					outSet.Add(wn2);
				}
				
				// Do for right operand
				wn = new WatcherNode(this);
				o2 = ((OperatorOr)o).getRightOperand();
				wn.addToGamma(o2);
				spawnedSet = wn.spawn(m);
				
				foreach (WatcherNode wn2 in spawnedSet)
				{
					outSet.Add(wn2);
				}
				
				return outSet;
			}
			
			// Operator IMPLIES
			else if (o.GetType() == typeof(OperatorImplies))
			{
				// Do for right operand
				wn = new WatcherNode(this);
				o2 = ((OperatorImplies)o).getRightOperand();
				wn.addToGamma(o2);
				spawnedSet = wn.spawn(m);
				
				foreach (WatcherNode wn2 in spawnedSet)
				{
					outSet.Add(wn2);
				}
				
				// Do for right operand
				wn = new WatcherNode(this);
				o2 = ((OperatorImplies)o).getLeftOperand().getNegated();
				wn.addToGamma(o2.getNegatedNormalForm());
				spawnedSet = wn.spawn(m);
				
				foreach (WatcherNode wn2 in spawnedSet)
				{
					outSet.Add(wn2);
				}
				
				return outSet;
			}
			
			// Operator AND
			else if (o.GetType() == typeof(OperatorAnd))
			{
				// Do for left operand
				wn = new WatcherNode(this);
				o2 = ((OperatorAnd)o).getLeftOperand();
				wn.addToGamma(o2);
				
				// Do for left operand
				o2 = ((OperatorAnd)o).getRightOperand();
				wn.addToGamma(o2);
				spawnedSet = wn.spawn(m);
				
				foreach (WatcherNode wn2 in spawnedSet)
				{
					outSet.Add(wn2);
				}
				
				return outSet;
			}
			
			// Operator X
			else if (o.GetType() == typeof(OperatorX))
			{
				wn = new WatcherNode(this);
				o2 = ((OperatorX)o).getOperand();
				wn.addToDelta(o2);
				spawnedSet = wn.spawn(m);
				
				foreach (WatcherNode wn2 in spawnedSet)
				{
					outSet.Add(wn2);
				}
				
				return outSet;
			}
			
			// Operator G
			else if (o.GetType() == typeof(OperatorG))
			{
				// Do for left operand
				wn = new WatcherNode(this);
				o2 = ((OperatorG)o).getOperand();
				wn.addToGamma(o2);
				wn.addToDelta(o);
				spawnedSet = wn.spawn(m);
				
				foreach (WatcherNode wn2 in spawnedSet)
				{
					outSet.Add(wn2);
				}
				
				return outSet;
			}
			
			// Operator F
			else if (o.GetType() == typeof(OperatorF))
			{
				// Do for left node
				wn = new WatcherNode(this);
				o2 = ((OperatorF)o).getOperand();
				wn.addToGamma(o2);
				spawnedSet = wn.spawn(m);
				
				foreach (WatcherNode wn2 in spawnedSet)
				{
					outSet.Add(wn2);
				}
				
				// Do for right node
				wn = new WatcherNode(this);
				wn.addToDelta(o);
				spawnedSet = wn.spawn(m);
				
				foreach (WatcherNode wn2 in spawnedSet)
				{
					outSet.Add(wn2);
				}
				
				return outSet;
			}
			
			// Operator U
			else if (o.GetType() == typeof(OperatorU))
			{
				// Do for left node
				wn = new WatcherNode(this);
				o2 = ((OperatorU)o).getRightOperand();
				wn.addToGamma(o2);
				spawnedSet = wn.spawn(m);
				
				foreach (WatcherNode wn2 in spawnedSet)
				{
					outSet.Add(wn2);
				}
				
				// Do for right node
				wn = new WatcherNode(this);
				wn.addToDelta(o);
				spawnedSet = wn.spawn(m);
				
				foreach (WatcherNode wn2 in spawnedSet)
				{
					outSet.Add(wn2);
				}
				
				return outSet;
			}
			
			// Operator V
			else if (o.GetType() == typeof(OperatorV))
			{
				// Do for left node
				wn = new WatcherNode(this);
				o2 = ((OperatorV)o).getLeftOperand();
				wn.addToGamma(o2);
				o2 = ((OperatorV)o).getRightOperand();
				wn.addToGamma(o2);
				spawnedSet = wn.spawn(m);
				
				foreach (WatcherNode wn2 in spawnedSet)
				{
					outSet.Add(wn2);
				}
				
				// Do for right node
				wn = new WatcherNode(this);
				o2 = ((OperatorV)o).getRightOperand();
				wn.addToGamma(o2);
				wn.addToDelta(o);
				spawnedSet = wn.spawn(m);
				
				foreach (WatcherNode wn2 in spawnedSet)
				{
					outSet.Add(wn2);
				}
				
				return outSet;
			}
			
			// Operator [p=x]
			else if (o.GetType() == typeof(FOForAll))
			{
				// Iterate over domain
				wn = new WatcherNode(this);
				
				// TODO: supposes that the string qualifier is an atom
				Atom p = new Atom(((FOForAll)o).getQualifier());
				Atom x = ((FOForAll)o).getQuantifiedVariable();
				HashSet<Atom> s = m.getDomain(p);
				
				foreach (Atom v in s)
				{
					o2 = ((FOForAll)o).getOperand();
					o3 = o2.evaluate(x, v);
					wn.addToGamma(o3);
				}
				
				spawnedSet = wn.spawn(m);
				
				foreach (WatcherNode wn2 in spawnedSet)
				{
					outSet.Add(wn2);
				}
				
				return outSet;
			}
			
			// Operator <p=x>
			else if (o.GetType() == typeof(FOExists))
			{
				// Iterate over domain
				// TODO: supposes that the string qualifier is an atom
				Atom p = new Atom(((FOExists)o).getQualifier());
				Atom x = ((FOExists)o).getQuantifiedVariable();
				HashSet<Atom> s = m.getDomain(p);
				
				foreach (Atom v in s)
				{
					wn = new WatcherNode(this);
					o2 = ((FOExists)o).getOperand();
					o3 = o2.evaluate(x, v);
					wn.addToGamma(o3);
					spawnedSet = wn.spawn(m);
				
					foreach (WatcherNode wn2 in spawnedSet)
					{
						outSet.Add(wn2);
					}
				}
				
				// TODO (Dominic): Validate this ?
				return outSet;
			}
			
			// Operator =
			else if (o.GetType() == typeof(OperatorEquals))
			{
				// This should never happen! When down to the evaluation
        		// of an equality, all variables should be evaluated!
				//assert(false);
				return outSet;
			}
			
			// Operator NOT
			else if (o.GetType() == typeof(OperatorNot))
			{
				// Do for operand
				wn = new WatcherNode(this);
				o2 = ((OperatorNot)o).getOperand();
				
				if (o2.GetType() != typeof(Constant))
				{
					// This should not happen! Negations should be pushed
          			// to atoms
					//assert(false);
					return outSet;
				}
				
				// TODO: true and false are checked by comparing their
        		// string representations; there should be a more graceful
        		// way
        		// to check for true and false
				if (((Constant)o).getSymbol() == Operator.m_trueAtom.getSymbol())
				{
					// Constant TRUE, i.e. evaluates to FALSE: this branch
          			// does not return anything
          			// i.e. do nothing
				}
				
				else if (((Constant)o).getSymbol() == Operator.m_falseAtom.getSymbol())
				{
					// Constant FALSE, i.e. evaluates to TRUE: just pass on
          			// the recursive evaluation
					spawnedSet = spawn(m);
					
					foreach (WatcherNode wn2 in spawnedSet)
					{
						outSet.Add(wn2);
					}
					
					return outSet;
				}
				
				else
				{
					// This should never happen! All atoms should evaluate
          			// down
          			// to either true or false.
					System.Diagnostics.Debug.Assert (false, "Unrecognized operator in watcher node");
					return outSet;
				}
			}
			
			// Constants (true or false)
			else if (o.GetType() == typeof(Constant))
			{
				// TODO: true and false are checked by comparing their
        		// string representations; there should be a more graceful
        		// way
        		// to check for true and false
				if (((Constant)o).getSymbol() == Operator.m_trueAtom.getSymbol())
				{
					// Constant TRUE: just pass on the recursive evaluation
					spawnedSet = spawn(m);
					
					foreach (WatcherNode wn2 in spawnedSet)
					{
						outSet.Add(wn2);
					}
				}
				
				else if (((Constant)o).getSymbol() == Operator.m_falseAtom.getSymbol())
				{
					// This branch is stopped: this branch does not return
          			// anything
          			// i.e. do nothing
					return outSet;
				}
				
				else
				{
					// This should never happen! All atoms should evaluate
          			// down to either true or false.
					//assert(false);
					return outSet;
				}
			}
		}
		
		if (m_gamma.Count == 0)
		{
			// Gamma is empty: return a set with myself
			outSet.Add(this);
		}
		
		return outSet;
	}
	
	/**
	 * Provides a string rendition of the node.
   	 */
	public override string ToString()
	{
		return (m_gamma.ToString() + (" ||- ") + (m_delta.ToString()));
	}
}
