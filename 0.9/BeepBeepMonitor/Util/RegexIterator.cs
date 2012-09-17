using System.Collections;
using System.Text.RegularExpressions;

/**
 * Splits a string into pieces according to some regular expression
 * and feeds them like an iterator through a next-like method.
 * @author shalle
 * @version 2010-11-27
 * @param <E>
 */
public class RegexIterator<E>
{
    private string m_splitExpression = "";
    private string m_toSplit = "";
    private Regex m_pattern;
    private MatchCollection m_matcher;
    private bool m_checkedNext = false;
    private bool m_hasNext = false;
    private int m_group = 1;

    public RegexIterator(string toSplit, string splitExpression, int group) : base()
    {
        m_splitExpression = splitExpression;
        m_toSplit = toSplit;
        m_group = group;
        reset();
    }

    public void remove()
    {
        // Do nothing in this context
    }

    public void setGroup(int group)
    {
        m_group = group;
    }

    public bool hasNext()
    {
        if (!m_checkedNext)
        {
            m_hasNext = (m_matcher.Count > 0 ? true : false);
            m_checkedNext = true;
        }

        return m_hasNext;
    }

    public void reset()
    {
        m_pattern = new Regex(m_splitExpression, RegexOptions.Singleline);
        m_matcher = m_pattern.Matches(m_toSplit);
    }

    public string next()
    {
        if (hasNext())
        {
            m_checkedNext = false;
            Match m = m_matcher[0];
            GroupCollection g = m.Groups;

            return g[m_group].ToString();
        }

        return null;
    }
}
