/******************************************************************************
  Runtime monitors for message-based workflows
  Copyright (C) 2008-2010 Sylvain Halle
  
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
using System.Diagnostics;

public class SymbolicGenerator : RuntimeGenerator
{
	/**
   	 * The nodes representing the state of the watcher
   	 */
	protected HashSet<GeneratorNode> m_nodes;
	
	/**
   	 * The maximum number of atoms in the watcher's state since its last
   	 * {@link reset}.
   	 */
	protected int m_maxAtoms;
	
	/**
   	 * The maximum number of nodes in the watcher's state since its last
   	 * {@link reset}.
   	 */
  	protected int m_maxNodes;

  	/**
   	 * The maximum number of formulae in the watcher's state since its last
   	 * {@link reset}.
   	 */
  	protected int m_maxFormulae;

  	/**
   	 * The LTL-FO+ formula the watcher is assigned to
   	 */
  	protected Operator m_formulaToWatch;

  	/**
   	 * Default constructor.
   	 */
	public SymbolicGenerator() : base()
	{
		m_nodes = new HashSet<GeneratorNode>();
		m_maxNodes = 0;
		m_maxFormulae = 0;
	}
	
	/**
   	 * Constructs a watcher and assigns a LTL-FO+ formula to watch. This also
   	 * automatically resets the watcher (see {@link reset}).
   	 * 
   	 * @param o
   	 *          The LTL-FO+ formula to watch
   	 */
	public SymbolicGenerator(Operator o) : this()
	{
		m_formulaToWatch = o;
	}
	
	public string generateUnsound()
	{
		// Get the node
		GeneratorNode pickedNode = generateNode();
		
		// Gets the message satisfying the conditions
		XMLDocument xd = getSatisfyingMessage(pickedNode);
		
		// Returns the message created earlier
		return xd.ToString();
	}
	
	public string generate()
	{
		// Get the node
		GeneratorNode pickedNode = generateNode();
		
		// If nothing is generated, return nothing
		if (pickedNode == null)
		{
			return "";
		}
		
		// Checks if the node is sound
		if (!pickedNode.Sound)
		{
			throw new LossOfSoundnessException();
		}
		
		// Gets the message satisfying the conditions
		XMLDocument xd = getSatisfyingMessage(pickedNode);
		
		// Returns the message created earlier
		return xd.ToString();
	}
	
	private GeneratorNode generateNode()
	{
		HashSet<GeneratorNode> spawnedNodes;
		HashSet<GeneratorNode> newNodes = new HashSet<GeneratorNode>();
		int size = 0, atoms = 0;
		
		long timeBegin = Stopwatch.GetTimestamp();
		
		// Takes each node and applies its decomposition
		foreach (GeneratorNode wn in m_nodes)
		{
			GeneratorNode wn2 = new GeneratorNode();
			
			wn2.setGamma(wn.getDelta());
			spawnedNodes = wn2.spawn();
			
			foreach (GeneratorNode gn in spawnedNodes)
			{
				newNodes.Add(gn);
			}
			
			// Shortcut: as soon as one branch returns something, we skip the others
			if (spawnedNodes.Count > 0)
			{
				break;
			}
		}
		
		long timeEnd = Stopwatch.GetTimestamp();
			
		System.Console.WriteLine("Finished producing the " + newNodes.Count + "nodes " + (timeEnd - timeBegin));
		timeBegin = Stopwatch.GetTimestamp();
			
		// Remember the new nodes
		m_nodes = newNodes;
			
		// Updates statistics about maximum number of nodes
		size = m_nodes.Count;
		
		if (size > m_maxNodes)
		{
			m_maxNodes = size;
		}
		
		// Updates statistics about total number of formulae
		size = 0;
		atoms = 0;
		
		foreach (GeneratorNode wn in m_nodes)
		{
			size += wn.getSize();
			atoms += wn.getAtoms();
		}
		
		if (size > m_maxFormulae)
		{
			m_maxFormulae = size;
		}
		
		if (atoms > m_maxAtoms)
		{
			m_maxAtoms = atoms;
		}
		
		// We remove nodes that don't contain any OPlus
    	// (i.e., that don't tell us to do anything)
    	// and check at the same time if at least one node is sound
		bool hasSound = false;
		
		foreach (GeneratorNode wn in m_nodes)
		{
			HashSet<OPlus> opluses = wn.getOPluses();
			bool contains_element = false;
			
			foreach (OPlus op in opluses)
			{
				// Remove conditions that only assert path existence
				if (!op.getOperand().Equals(Operator.m_falseAtom) && 
					!op.getOperand().Equals(Operator.m_trueAtom))
				{
					contains_element = true;
				}
			}
			
			if (!contains_element)
			{
				m_nodes.Remove(wn);
			}
			
			else
			{
				if (wn.Sound)
				{
					hasSound = true;
				}
			}
		}
		
		// Can we produce a new message?
		if (m_nodes.Count == 0)
		{
			// No!
			return null;
		}
		
		// We remove all nodes whose OPluses are contradictory
	    /*it = m_nodes.iterator();
	    while (it.hasNext())
	    {
	      GeneratorNode wn = it.next();
	      if (wn.containsOPlusContradiction())
	        it.remove();
	    }*/
	    
	    // Is there at least one remaining sound node?
		if (hasSound)
		{
			// Yes: then let's remove any unsound node
			foreach (GeneratorNode wn in m_nodes)
			{
				if (!wn.Sound)
				{
					m_nodes.Remove(wn);
				}
			}
		}
		
		timeEnd = Stopwatch.GetTimestamp();
		System.Console.WriteLine("Finished pruning " + (timeEnd - timeBegin));
		
		// Pick *one* of the nodes randomly
		long pickIndex = (long)System.Math.Round(new System.Random().NextDouble() * (double)(m_nodes.Count - 1));
		int i = 0;
		GeneratorNode pickedNode = null;
		
		foreach (GeneratorNode gn in m_nodes)
		{
			if (i == pickIndex)
			{
				pickedNode = gn;
				break;
			}
			
			i++;
		}
		
		// Keep only the selected node for the next state
		m_nodes = new HashSet<GeneratorNode>();
		m_nodes.Add(pickedNode);
		
		// Return the picked node
		return pickedNode;
	}
	
	/**
   	 * Resets the watcher's state, i.e. considers the next message to be generated
   	 * as the first one.
   	 */
	public void reset()
	{
		GeneratorNode wn = new GeneratorNode();
		HashSet<Operator> hs = new HashSet<Operator>();
		
		hs.Add(m_formulaToWatch);
		wn.setDelta(hs);
		
		m_nodes.Clear();
		m_nodes.Add(wn);
		
		m_maxNodes = 1;
		m_maxFormulae = 0;
		m_maxAtoms = 0;
	}
	
	/**
   	 * Counts the maximum number of atoms in the watcher's state since its last
   	 * {@link reset}.
   	 * 
   	 * @return The number
   	 */
	public int getMaxAtoms()
	{
		return m_maxAtoms;
	}
	
	/**
   	 * Counts the maximum number of nodes in the watcher's state since its last
   	 * {@link reset}.
   	 * 
   	 * @return The number
   	 */
	public int getMaxNodes()
	{
		return m_maxNodes;
	}
	
	/**
   	 * Counts the maximum number of formulae in the watcher's state since its last
   	 * {@link reset}.
   	 * 
   	 * @return The number
   	 */
	public int getMaxSize()
	{
		return m_maxFormulae;
	}
	
	public bool setFormula(Operator o)
	{
		if (o != null)
		{
			m_formulaToWatch = o;
		}
		
		return true;
	}
	
	public bool setFormula(string s)
	{
		setFormula(LTLStringParser.parseFromString(s));
		
		return true;
	}
	
	public override string ToString()
	{
		return m_nodes.ToString();
	}
	
	protected static XMLDocument getSatisfyingMessage(GeneratorNode gn)
	{
		XMLDocument xd = new XMLDocument();
		
		if (gn == null)
		{
			return xd;
		}
		
		HashSet<OPlus> opluses = gn.getOPluses();
		
		foreach (OPlus op in opluses)
		{
			if (op.getOperand().Equals(Operator.m_trueAtom) || 
				op.getOperand().Equals(Operator.m_falseAtom))
			{
				// This only asserts that the path should exist
				continue;
			}
			
			xd.createPath(op.getQualifier(), op.getOperand().ToString());
		}
		
		return xd;
	}
}
