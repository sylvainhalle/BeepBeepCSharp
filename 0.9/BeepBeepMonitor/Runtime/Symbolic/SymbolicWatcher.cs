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
using System.IO;

/**
 * Watcher based on a modified version of the LTL runtime monitoring algorithm.
 * This algorithm is presented in:
 * <ul>
 * <li>Gerth, R., Peled, D., Vardi, M.Y., Wolper, P. (1995). Simple on-the-fly
 * automatic verification of linear temporal logic. Proc. PSTV 1995, 3-18.</li>
 * </ul>
 * The complete algorithm, adapted for LTL-FO+, and an experimental discussion
 * based on results obtained with this watcher can be found in:
 * <ul>
 * <li>Hallé, S., Villemaire, R. (2008). Runtime Monitoring of Message-Based
 * Workflows with Data. Proc. EDOC 2008, IEEE Computer Society, 63-72.</li>
 * </ul>
 * 
 * @author Sylvain Hallé
 * @version 2009-10-09
 */

public class SymbolicWatcher : LTLFOWatcher
{
	/**
	 * The nodes representing the state of the watcher
	 */
	protected HashSet<WatcherNode> m_nodes;
	
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
	 * Default constructor.
	 */
	public SymbolicWatcher() : base()
	{
		m_nodes = new HashSet<WatcherNode>();
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
	public SymbolicWatcher(Operator o) : this()
	{
		m_formulaToWatch = o;
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
	
	/**
	 * Computes the outcome of the watcher on the trace observed up to now.
	 * 
	 * @return The outcome of the trace.
	 */
	public Outcome getOutcome()
	{		
		if (m_nodes.Count == 0)
		{
			return Outcome.FALSE;
		}
		
		foreach (WatcherNode wn in m_nodes)
		{
			if (wn.getGamma().Count == 0 && wn.getDelta().Count == 0)
			{
				return Outcome.TRUE;
			}
		}
		
		return Outcome.INCONCLUSIVE;
	}
	
	public Outcome checkFile(string filename)
	{
		Outcome oc = Outcome.FALSE;
		
		// Reads file
        string trace = SimpleFileReader.readFile(filename);
        RegexIterator<string> sf = new RegexIterator<string>(trace, "(<" + m_messageName + ">.+?</" + m_messageName + ">)", 1);
		
		// Splits trace into messages
		int max_messages = 10000;
		
        for (int i = 0; i < max_messages && sf.hasNext(); i++)
		{
            string currentMessage = sf.next();
			
			if (currentMessage == null)
			{
				break;
			}
			
			update(currentMessage);
			oc = getOutcome();
			
			if (m_verbosity > 1)
			{
				System.Console.Error.WriteLine("Message #" + i.ToString());
				System.Console.Error.WriteLine(currentMessage);
			}
			
			if (oc == Outcome.TRUE || oc == Outcome.FALSE)
			{
				break;
			}
		}
		
		reset();
		
		return oc;
	}
	
	/**
	 * Prints the state of the watcher. This is for demonstration purposes only
	 * and is not "useful" per se in the monitoring process.
	 * 
	 * @return A String rendition of the watcher's current state.
	 */
	public string printState()
	{
		return m_nodes.ToString();
	}
	
	/**
	 * Resets the watcher's state, i.e. considers the next message to be received
	 * as the first one.
	 */
	public override void reset()
	{
		WatcherNode wn = new WatcherNode();
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
	 * Updates the state of the watcher, given reception of a new message in a
	 * stringified, XML-ised form.
	 * 
	 * @param s
	 *          A String containing the XML snippet corresponding to the message.
	 *          For the format of XML supported by the method, see {@link
	 *          WatcherMessage(String)}.
	 */
	public override void update (string s)
	{
		WatcherMessage m = new WatcherMessage(s);
		
		// Prepends the default message name to the queried paths
		m.setPrependPath("/" + m_messageName + "/");
		update(m);
	}
	
	/**
	 * Updates the state of the watcher, given reception of a new message.
	 * 
	 * @param m
	 *          The message that was received.
	 */
	private void update(WatcherMessage m)
	{
		WatcherNode wn2;
		HashSet<WatcherNode> spawnedNodes;
		HashSet<WatcherNode> newNodes = new HashSet<WatcherNode>();
		int size = 0, atoms = 0;
		
		foreach (WatcherNode wn in m_nodes)
		{
			wn2 = new WatcherNode();
			wn2.setGamma(wn.getDelta());
			spawnedNodes = wn2.spawn(m);
			
			foreach (WatcherNode wn3 in spawnedNodes)
			{
				newNodes.Add(wn3);
			}
		}
		
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
		
		foreach (WatcherNode wn in m_nodes)
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
	}
}
