/******************************************************************************
XML manipulation classes
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
using System.Text.RegularExpressions;

/**
 * Implementation of basic XPath functionalities for XML message manipulation.
 * An XMLDocument can be of four types:
 * <ol>
 * <li>ROOT: the root of the document</li>
 * <li>NODE: intermediate node in the document; it has a name, and possibly a
 * list of children</li>
 * <li>TEXT: leaf node containing a string of text</li>
 * <li>TRUE, FALSE: return type for boolean queries</li>
 * </ol>
 * An XMLDocument object can be created from a string using {@link
 * XMLDocument(string)}. The document is assumed to have a "normal structure",
 * i.e. no <tt>&lt;!CDATA&gt;</tt> sections or things of that kind. Attributes
 * inside elements (such as <tt>&lt;a attr="value"&gt;</tt>) do not cause
 * parsing errors, but they are ignored. Self-closing tags (e.g.
 * <tt>&lt;a/&gt;</tt>) are supported.
 * <p>
 * An XMLDocument can then be queried using a bare-bones implementation of
 * XPath. Only a tiny fragment of XPath is supported. Expressions can have three
 * forms:
 * <dl>
 * <dt><tt>p/p/p/.../p</tt></dt>
 * <dd>Returns a list of subtrees at the end of the path</dd>
 * <dt><tt>p/p/p/.../p = "exp"</tt></dt>
 * <dd>Returns a node with type TRUE if there is exactly one text node at the
 * end of the path with value "exp". Returns a node with type FALSE otherwise.</dd>
 * <dt><tt>p/p/p/.../p = p/p/p/.../p</tt></dt>
 * <dd>Same as above, but comparing two leafs in the tree.</dd>
 * </dl>
 * Please note that equality testings always operate on text leafs, and not
 * subtrees. Trying to compare two subtrees, or a subtree and a text value,
 * always returns false. This is different from standard XPath, and means that
 * you should not use expressions like that here! It returns false only so that
 * it returns something.
 * <p>
 * Moreover, comparison between elements is by default <strong>insensitive</strong>
 * to case.  This can be changed by instantiating XMLDocument with the
 * <tt>true</tt> argument. 
 * <p>
 * This class is kept as simple as possible <strong>on purpose</strong>. It is
 * not expected (nor desirable) that other functionalities be supported.
 * 
 * @author Sylvain Hall&eacute;
 * @version 2010-10-24
 * 
 */
public class XMLDocument
{
	protected List<XMLDocument> m_children = new List<XMLDocument>();
	protected bool m_caseSensitive = false;
	protected string m_elementName = "";
	protected NodeType m_type = NodeType.NODE;
	
	public enum NodeType
	{
		NODE, 
		TEXT, 
		ROOT, 
		TRUE, 
		FALSE
	};
	
	public XMLDocument() : base()
	{
	}
	
	public XMLDocument(bool caseSensitive)
	{
		m_caseSensitive = caseSensitive;
	}

    /**
     * Delegate to {@link XMLDocument(String, boolean)} which ignores whitespace
     * by default.
     * 
     * @param s
     */
	public XMLDocument(string s) : this(s, true)
	{
	}

    /**
     * Creates an XML document from a string.
     * 
     * @param s
     *            The XML code from which to create the document
     * @param ignoreWhitespace
     *            Optional. If set to false, ignores all intra-element
     *            whitespace. Defaults to true (recommended, unless whitespace
     *            preservation is important).
     */
	public XMLDocument(string s, bool ignoreWhiteSpace) : this()
	{
		m_children = parse(s, ignoreWhiteSpace);
		m_type = NodeType.ROOT;
	}
	
	protected static List<XMLDocument> parse(string s, bool ignoreWhiteSpace)
	{
		Regex r = null;
		MatchCollection m = null;
		int inside_begin = 0, inside_end = 0, tag_end = 0;
		bool tag_has_children, parse_error = false;
		XMLDocument xd;
		List<XMLDocument> out_children = new List<XMLDocument>();

        // This is an "XML declaration" tag: we trim it
		if (s.Length >= 5 && s.Substring(0, 5) == "<?xml")
		{
			int new_beg = s.IndexOf(">") + 1;
			
			s = s.Substring(new_beg).Trim();
		}
		
		while (s.Length > 0 && !parse_error)
		{
			xd = new XMLDocument();
			inside_begin = 0;
			inside_end = 0;
			tag_end = 0;
			tag_has_children = true;
			
			r = new Regex("^<\\s*([^\\s>]+)\\s*([^>]*)>");
			m = r.Matches(s);
			
			if (m.Count > 0)
			{
				Match m2 = m[0];
				GroupCollection g = m2.Groups;

                // Opening tag found at first position
				xd.m_elementName = g[1].ToString();
				inside_begin = g[2].Index + g[2].ToString().Length + 1;
				
				if (g[2].ToString().EndsWith("/"))
				{
                    // Self-closing tag
					// TODOC#: vérifier si les bornes sont exactes en C#
					inside_end = g[g.Count - 1].Index;
					tag_end = g[g.Count - 1].Index;
					tag_has_children = false;
				}
				
				else
				{
					r = new Regex("<\\s*(/{0,1})" + xd.m_elementName + "\\s*>");
					m = r.Matches(s.Substring(inside_begin));
					
					int level = 1;
					bool tag_found = false;
					
					foreach (Match m3 in m)
					{
						g = m3.Groups;
						
						if (g[1].ToString().StartsWith("/"))
						{
                            // Closing tag
							level--;
						}
						
						if (g[1].ToString().Length == 0)
						{
                            // Closing tag
							level++;
						}
						
						if (level == 0)
						{
                            // Matching closing tag
							inside_end = inside_begin + m[0].Index;
							tag_end = inside_begin + m[0].Index + m[0].ToString().Length;
							tag_found = true;
							
							break;
						}
					}

                    // If we get here, we didn't find matching closing tag:
                    // parse error
					if (!tag_found)
					{
                        // Do nothing with it
						parse_error = true;
					}
				}
			}
			
			else
			{
                // No opening tag at first position: this is text
				r = new Regex("<");
				m = r.Matches(s);
				
				if (m.Count > 0)
				{
					inside_end = m[0].Index;
				}
				
				else
				{
					inside_end = s.Length;
				}
				
				tag_end = inside_end;
				string inside = s.Substring(inside_begin, inside_end);
				
				if (ignoreWhiteSpace)
				{
					inside = inside.Trim();
				}
				
				if (inside.Length == 0)
				{
                    // This text is null, don't create an element
					xd = null;
				}
				
				else
				{
					xd.m_elementName = inside;
					xd.m_type = NodeType.TEXT;
					tag_has_children = false;
				}
			}
			
			if (!parse_error)
			{
				string inside = s.Substring(inside_begin, (inside_end - inside_begin));
				
				s = s.Substring(tag_end);
				
				if (xd != null)
				{
					if (tag_has_children)
					{
						xd.m_children = parse (inside, ignoreWhiteSpace);
					}
					
					out_children.Add(xd);
				}
			}
		}
		
		return out_children;
	}

    /**
     * Creates a path in the document with specific value at the end
     * @param path A slash-separated path
     * @param value The value to put at the end of the path
     */
	public void createPath(string path, string val)
	{
        // Set myself as root
		m_type = NodeType.ROOT;

        // Splits the path
		if (path == null || val == null)
		{
			return;
		}
		
		string[] elements = path.Split('/');
		int start = 0;
		
		if (path.StartsWith("/"))
		{
			start = 1;
		}
		
		List<string> splitPath = new List<string>();
		
		for (int i = start; i < elements.Length; i++)
		{
			splitPath.Add(elements[i]);
		}
		
		createPath(splitPath, val);
	}
	
	private void createPath(List<string> path, string val)
	{
		if (path.Count == 0)
		{
            // We are at the leaf; we create this leaf
			XMLDocument leaf = new XMLDocument();
			
			leaf.m_type = NodeType.TEXT;
			leaf.m_elementName = val;
			this.m_children.Add(leaf);
			
			return;
		}
		
		string firstElement = path[0];
		bool added = false;
		
		path.RemoveAt(0);
		
		foreach (XMLDocument xd in m_children)
		{
			if (xd.m_elementName != firstElement)
			{
				continue;
			}
			
			xd.createPath(path, val);
			added = true;
			
			break;
		}
		
		if (!added)
		{
			XMLDocument xd = new XMLDocument();
			xd.m_elementName = firstElement;
			xd.createPath(path, val);
			m_children.Add(xd);
		}
	}

    /**
     * Evaluates an XPath expression on a document. See the class documentation
     * for a description of the supported syntax. In case of a syntax error, the
     * expression evaluates to false as soon as the method's (very) basic parser
     * no longer recognizes how to read the string.
     * 
     * @param e
     *            The XPath expression
     * @return A list of nodes, corresponding to the result of the operation.
     *         The method always returns a list, even when the expected result
     *         is true or false. In such a case, the list contains only one
     *         element, a node of type TRUE or FALSE (not to be confused with a
     *         <em>text</em> node whose text is "TRUE" or "FALSE"). Hence an
     *         empty list is just an empty list of nodes, it should not be
     *         confused with FALSE.
     */
	public List<XMLDocument> evaluateXPath(string e)
	{
		Regex r;
		MatchCollection m;
		XMLDocument xd_left, xd_right, xd_false, xd_true;
		string path_left = "", path_right = "";
		List<XMLDocument> xdOut = new List<XMLDocument>();
		List<XMLDocument> left, right;

        // Creates element false
		xd_false = new XMLDocument();
		xd_false.m_type = NodeType.FALSE;

        // Creates element true
		xd_true = new XMLDocument();
		xd_true.m_type = NodeType.TRUE;

        // Get left path
		r = new Regex("(^[^\\s=]+)={0,1}");
		m = r.Matches(e);
		
		if (m.Count == 0)
		{
            // Parse error: return false
			xdOut.Add(xd_false);
			
			return xdOut;
		}
		
		Match m2 = m[0];
		GroupCollection g = m2.Groups;
		
		path_left = g[1].ToString();
		e = e.Substring(m[0].Index + m[0].Length).Trim();
		left = getPath(path_left);
		
		if (e.Length == 0)
		{
            // Nothing else: return subtree
			return left;
		}

        // Check if equality
		if (e.Length > 0 && !e.StartsWith("="))
		{
            // Parse error: return false
			xdOut.Add(xd_false);
			
			return xdOut;
		}

        // Remove = sign
		e = e.Substring(1).Trim();

        // Check if constant
		if (e.StartsWith("\""))
		{
			r = new Regex("\"([^=]*)\"");
			m = r.Matches(e);
			
			if (m.Count == 0)
			{
                // Parse error: return false
				xdOut.Add(xd_false);
				
				return xdOut;
			}
			
			m2 = m[0];
			g = m2.Groups;
			
			path_right = g[1].ToString();
			
			if (left.Count != 1)
			{
                // LHS is a set of nodes and RHS is constant
				xdOut.Add(xd_false);
				
				return xdOut;
			}
			
			if (left[0].m_type != NodeType.TEXT)
			{
                // LHS is not text
				xdOut.Add(xd_false);
				
				return xdOut;
			}
			
			if (path_right != left[0].m_elementName)
			{
                // LHS and RHS text, but not same text
				xdOut.Add(xd_false);
				
				return xdOut;
			}
			
			xdOut.Add(xd_true);
			
			return xdOut;
		}
		
		else
		{
            // RHS is a path
			path_right = e;
			right = getPath(path_right);
			
			if (left.Count != 1 || right.Count != 1)
			{
                // One of the sides is not a single node
				xdOut.Add(xd_false);
				
				return xdOut;
			}
			
			xd_left = left[0];
			xd_right = right[0];
			
			if (xd_left.m_type != NodeType.TEXT || 
				xd_right.m_type != NodeType.TEXT)
			{
                // One of the sides is not a text node
				xdOut.Add(xd_false);
				
				return xdOut;
			}
			
			if (xd_left.m_elementName != xd_right.m_elementName)
			{
                // LHS and RHS have different texts
				xdOut.Add(xd_false);
				
				return xdOut;
			}
			
			xdOut.Add(xd_true);
			
			return xdOut;
		}
	}
	
	protected List<XMLDocument> getPath(string path)
	{
		List<XMLDocument> xdOut = new List<XMLDocument>();
		
		if (path.Length == 0 || path == "/")
		{
            // End of path, return what's below
			return m_children;
		}
		
		Regex r = new Regex("/([^/\\s]+)");
		MatchCollection m = r.Matches(path);
		
		if (m.Count == 0)
		{
			return xdOut;
		}
		
		Match m2 = m[0];
		GroupCollection g = m2.Groups;
		
		string elementName = g[1].ToString();
		string subPath = path.Substring(m[0].Index + m[0].ToString().Length);
		
		foreach (XMLDocument xd in m_children)
		{
			if (elementName == xd.m_elementName)
			{
				foreach (XMLDocument xd2 in xd.getPath(subPath))
				{
					xdOut.Add(xd2);
				}
			}
		}
		
		return xdOut;
	}
	
	public string getText()
	{
		if (m_type != NodeType.TEXT)
		{
			return "";
		}
		
		return m_elementName;
	}
	
	public override string ToString()
	{
		return toString("");
	}
	
	protected string toString(string indent)
	{
		string sOut = "";
		string subindent = (indent == null || m_type == NodeType.ROOT ? "" : indent + "  ");
		
		switch (m_type)
		{
		case NodeType.TEXT: return (indent + m_elementName + "\n");
		case NodeType.TRUE: return (indent + "TRUE\n");
		case NodeType.FALSE: return (indent + "FALSE\n");
		case NodeType.NODE: 
			
			sOut = indent + "<" + m_elementName;
			
			if (m_children == null || m_children.Count == 0)
			{
				return (sOut + "/>\n");
			}
			
			sOut += ">\n";
			break;
			
		default: break;
		}
		
		if (m_children != null && m_children.Count > 0)
		{
			foreach (XMLDocument xd in m_children)
			{
				sOut += xd.toString(subindent);
			}
		}
		
		if (m_type == NodeType.NODE)
		{
			sOut += indent + "</" + m_elementName + ">";
		}
		
		sOut += "\n";
		
		return sOut;
	}
}
