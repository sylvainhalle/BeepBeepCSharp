using System.Collections;

public class OPlus : FOExists
{
	private bool m_negated = false;

    /**
     * 
     * @return True if the OPlus is negated, false otherwise
     */
	public bool isNegated()
	{
		return m_negated;
	}
	
	public void setNegated(bool b)
	{
		m_negated = b;
	}
	
	public OPlus() : base()
	{
		m_unicodeSymbol = "\u2295";
	}
	
	public OPlus(string qualifier, Atom a) : this()
	{
		if (qualifier != null)
		{
			m_qualifier = qualifier;
		}
		
		if (a != null)
		{
			m_operand = a;
		}
	}
	
	public override bool Equals(object v)
	{
		if (v.GetType() != typeof(OPlus))
		{
			return false;
		}
		
		OPlus op = (OPlus)v;
		
		if (m_qualifier == "" && op.m_qualifier == "")
		{
			return true;
		}
		
		if (m_qualifier == "")
		{
			return false;
		}
		
		return (m_qualifier == op.m_qualifier && m_operand.Equals(op.m_operand));
	}
	
	public override int GetHashCode()
	{
		if (m_qualifier == "")
		{
			return 0;
		}
		
		return m_qualifier.Length;
	}

    /*public override string ToString()
    {
        return "(+) " + m_qualifier + " " + m_operand.toString();
    
    }*/
	
	public string ToString()
	{
		return (m_unicodeSymbol + m_qualifier + " " + m_operand.ToString());
	}
}
