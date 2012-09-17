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

/**
 * The LTLFOWatcher class allows the runtime monitoring of LTL-FO+ formulae on a
 * sequence of XML messages.
 * 
 * @author Sylvain Hall&eacute;
 * @version 2010-11-27
 */
public class LTLFOWatcher : RuntimeMonitor
{
	/**
	 * Defines a special constant, that will always refer to the current system
	 * time.
	 */
	public static Atom m_timeSymbol = new Atom("$_TIME");
	
	/**
	 * The LTL-FO+ formula the watcher is assigned to
	 */
	protected Operator m_formulaToWatch;
	
	/**
	 * Verbosity level for watcher
	 */
	protected int m_verbosity = 0;
	
	/**
	 * Default name for a message
	 */
	protected string m_messageName = "message";
	
	/**
	 * A caption associated with the watcher. This caption can be used to
	 * describe the formula that is being watched.
	 */
	private string m_caption;
	
	/**
	 * Default constructor
	 */
	public LTLFOWatcher() : base()
	{
		m_formulaToWatch = null;
		m_caption = "";
	}
	
	/**
	 * Constructs a watcher and assigns a LTL-FO+ formula to watch. This also
	 * automatically resets the watcher (see {@link reset}).
	 * 
	 * @param o
	 *            The LTL-FO+ formula to watch
	 */
	public LTLFOWatcher(Operator o) : this()
	{
		m_formulaToWatch = o;
		reset();
	}
	/**
	 * Sets a verbosity level for the watcher. Verbosity messages are to be sent
	 * to System.err. A verbosity of 0 means no messages at all. It is up to
	 * each LTLFOWatcher implementation to define the meaning of higher
	 * verbosity levels.
	 * @param verbosity
	 */
	public void setVerbosity(int verbosity)
	{
		m_verbosity = verbosity;
	}
	
	/**
	 * Applies runtime monitoring to a trace of messages contained in a file.
	 * Contrarily to {@link update}, this method operates <i>post mortem</i> on
	 * a file containing the whole sequence of messages to be considered. The
	 * method automatically applies {@link reset} to the watcher once the 
	 * result has been computed. This means that the trace in the file cannot be
	 * continued with messages fed to the watcher through subsequent
	 * {@link update} or {@checkFile}: these messages will be considered as part
	 * of a <strong>new</strong> trace.
	 * 
	 * @param filename
	 *            A String containing the (relative) filename
	 * @return The watcher's outcome for that trace
	 */
	public Outcome checkFile(string filename)
	{
		return Outcome.FALSE;
	}
	
	/**
	 * Returns the caption associated to this watcher.
	 * 
	 * @return The caption
	 */
	public string getCaption()
	{
		return m_caption;
	}
	
	/**
	 * Returns the formula the watcher is currently assigned to.
	 * 
	 * @return An Operator representing the formula that is being watched.
	 */
	public Operator getFormula()
	{
		return m_formulaToWatch;
	}
	
	/**
	 * Computes the outcome of the watcher on the trace observed up to now.
	 * 
	 * @return The outcome of the trace.
	 */
	public Outcome getOutcome()
	{
		return Outcome.FALSE;
	}
	
	/**
	 * Resets the watcher's state, i.e. considers the next message to be
	 * received as the first one.
	 */
	public virtual void reset()
	{
		return;
	}
	
	/**
	 * Sets a caption for the watcher. This caption can be used to describe the
	 * formula that is being watched.
	 * 
	 * @param caption
	 *            A short caption for the watcher.
	 */
	public void setCaption(string caption)
	{
		if (caption != "")
		{
			m_caption = caption;
		}
		
		else
		{
			m_caption = "";
		}
	}
	
	/**
	 * Assigns a formula to the watcher. NOTE: setting a formula to the watcher
	 * automatically resets the watcher to an initial state, i.e. the watcher
	 * "forgets" anything it has done previously. This is because it would not
	 * make sense to change the formula to watch in the middle of a message
	 * trace.
	 * 
	 * @param o
	 * @return true if the formula could be set, false otherwise (e.g., when o
	 *         is null).
	 */
	public bool setFormula(Operator o)
	{
		if (o == null)
		{
			return false;
		}
		
		m_formulaToWatch = o;
		reset ();
		
		return true;
	}
	
	public bool setFormula(string s)
	{
		Operator o = LTLStringParser.parseFromString(s);
		
		return setFormula(o);
	}
	
	/**
	 * Updates the state of the watcher, given reception of a new message in a
	 * stringified, XML-ised form.
	 * 
	 * @param s
	 *            A String containing the XML snippet corresponding to the
	 *            message.
	 */
	public virtual void update(string s)
	{
		return;
	}
}
