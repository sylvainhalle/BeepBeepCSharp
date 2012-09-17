using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;

public class BeepBeepMonitor
{
	public static long serialVersionUID = 1;
	
	private int m_verbosity = 0;
	private int m_numMessages = 0;
	private long m_totalElapsed = 0;
	private int m_guiWidth = 96;
	private int m_guiHeight = 16;
	private string m_display = "";
	private List<LTLFOWatcher> m_watchers = new List<LTLFOWatcher>();
	
	public string eatMessage(string currentMessage)
	{
		long timeBegin = 0, timeEnd = 0;
		
		m_numMessages++;
		m_display = "";
		timeBegin = Stopwatch.GetTimestamp();
		
		foreach (LTLFOWatcher w in m_watchers)
		{
			m_display += (m_display == "" ? "" : "/");
			
			if (w == null)
			{
				m_display += "x";
				
				continue;
			}
			
			Outcome oc = w.getOutcome();
			
			if (oc == Outcome.INCONCLUSIVE)
			{
				w.update(currentMessage);
				oc = w.getOutcome();
				
				switch (oc)
				{
				case Outcome.FALSE: m_display += "F";
					break;
					
				case Outcome.TRUE: m_display += "T";
					break;
					
				default: m_display += "?";
					break;
				}
			}
			
			else
			{
				switch (oc)
				{
				case Outcome.FALSE: m_display += "f";
					break;
					
				case Outcome.TRUE: m_display += "t";
					break;
					
				default: m_display += "?";
					break;
				}
			}
		}
			
		timeEnd = Stopwatch.GetTimestamp();
		m_totalElapsed += (timeEnd - timeBegin);
		//repaint();
		
		return m_display;
	}
	
	public string getCaption(int i)
	{
		if (i < 0 || i >= m_watchers.Count)
		{
			return "";
		}
		
		LTLFOWatcher w = m_watchers[i];
		
		if (w == null)
		{
			return "";
		}
		
		return w.getCaption();
	}
	
	public string getStatus()
	{
		return m_display;
	}
}
