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
using System.Diagnostics;

/**
 * Wrapper around XML message
 * 
 * @author Sylvain
 * 
 */
public class WatcherMessage
{
	private XMLDocument m_xd = null;
	private string m_prependPath = "";
	
	public WatcherMessage() : base()
	{
	}
	
	public WatcherMessage(string s) : this()
	{
		m_xd = new XMLDocument(s);
	}
	
	public void setPrependPath(string s)
	{
		if (s != null)
		{
			m_prependPath = s;
		}
	}
	
	/**
   	 * Process a qualifier and returns the result. This methods simply bridges
   	 * the Atom qualifier to the XPath expression, and the returned Vector of
   	 * XML nodes into a Set of Constants. When the method encounters the atom
   	 * equal to m_timeSymbol, it returns the current system time (in
   	 * seconds) as the only domain element.
   	 * 
   	 * @param p
   	 * @return
   	 */
	public HashSet<Atom> getDomain(Atom p)
	{
		HashSet<Atom> aOut = new HashSet<Atom>();
		
		if (LTLFOWatcher.m_timeSymbol.Equals(p))
		{
			// We are processing the special "time" variable: its
      		// domain has 1 value: the current time in SECONDS
			aOut.Add(new Constant(((float)(Stopwatch.GetTimestamp())).ToString()));
			
			return aOut;
		}
		
		string path = p.ToString();
		List<XMLDocument> values = m_xd.evaluateXPath(m_prependPath + path);
		
		foreach (XMLDocument xd in values)
		{
			aOut.Add(new Constant(xd.getText()));
		}
		
		return aOut;
	}
	
	public override string ToString()
	{
		return m_xd.ToString();
	}
}
