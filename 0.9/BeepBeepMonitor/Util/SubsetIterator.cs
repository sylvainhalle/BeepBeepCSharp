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

/**
 * The SubsetIterator takes a set of elements and iterates over subsets
 * of that set. It effectively generates all 2<sup><i>n</i></sup> subsets,
 * not necessarily in order.
 * @author Sylvain Hall&eacute;
 *
 * @param <T> The type for the elements contained in the set
 */
public class SubsetIterator<T>
{
	/**
   	 * The set of elements
   	 */
	private HashSet<T> m_elements = null;
	
	/**
   	 * The set of mandatory elements
   	 */
	private HashSet<T> m_mandatory = null;
	
	/**
   	 * 
   	 */
	private int m_counter = 0;
	
	/**
   	 * Constructor.
   	 * @param startSet
   	 */
	public SubsetIterator(HashSet<T> startSet) : base()
	{
		if (startSet != null)
		{
			m_elements = new HashSet<T>(startSet);
		}
		
		else
		{
			m_elements = new HashSet<T>();
		}
	}
	
	/**
   	 * Forces the iterator to include elements of mandatorySet in all returned subsets.
   	 * That is, the iterator will enumerate the sets of the form x &cup; mandatorySet, with x &sube; facultativeSet.
   	 * If facultativeSet is empty, this is equivalent to enumerating all subsets of facultativeSet.
   	 * @param facultativeSet
   	 *  @param mandatorySet
   	 */
	public SubsetIterator(HashSet<T> facultativeSet, HashSet<T> mandatorySet) : this(facultativeSet)
	{
		if (mandatorySet != null)
		{
			m_mandatory = new HashSet<T>(mandatorySet);
		}
		
		else
		{
			m_mandatory = new HashSet<T>();
		}
		
		// Remove from the facultative set the elements that are
      	// already in mandatory
		foreach (T e in m_elements)
		{
			if (m_mandatory.Contains(e))
			{
				m_elements.Remove(e);
			}
		}
	}
	
	public bool hasNext()
	{
		if (m_elements == null)
		{
			return false;
		}
		
		return m_counter < System.Math.Pow(2, m_elements.Count);
	}
	
	public HashSet<T> next()
	{
		int counter = m_counter;
		HashSet<T> hOut = new HashSet<T>();
		
		foreach (T elem in m_elements)
		{
			if (counter % 2 == 1)
			{
				hOut.Add(elem);
			}
			
			counter = counter >> 1;
		}
		
		if (m_mandatory != null)
		{
			foreach (T e in m_mandatory)
			{
				hOut.Add(e);
			}
		}
		
		m_counter++;
		
		return hOut;
	}
	
	public void Remove()
	{
		// TODO Auto-generated method stub
    	// Unimplemented for the moment
	}
}
