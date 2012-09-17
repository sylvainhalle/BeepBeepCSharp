/******************************************************************************
Basic classes for first-order and modal logic manipulation
Copyright (C) 2007-2008 Sylvain Halle

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
 * The AU operator implements the functionalities of the CTL AU temporal
 * modality.
 * 
 * @author Sylvain Hallé
 * @version 2008-05-23
 */
public class OperatorEU : BinaryOperator
{
    private static string symbol = "U";
    private static string symbolLeft = "E";

    protected string m_symbolLeft;

    public OperatorEU()
        : base()
    {
        m_symbol = OperatorEU.symbol;
        m_symbolLeft = OperatorEU.symbolLeft;
    }

    public OperatorEU(Operator o1, Operator o2)
        : base(o1, o2)
    {
        m_symbol = OperatorEU.symbol;
        m_symbolLeft = OperatorEU.symbolLeft;
    }

    public override Operator getNegated()
    {
        return new OperatorAV(m_left.getNegated(), m_right.getNegated());
    }

    /**
     * Outputs a string rendition of the operator and of all its sub-operators
     * in a recursive fashion.
     * 
     * @param indent
     *            The amount of indentation to be prefixed to every line of the
     *            output
     * @return A String output of the operator
     */
    public override string toString(string indent)
    {
        return (indent + m_symbolLeft + " [" + m_left.ToString() + " " + m_symbol + " " + m_right.ToString() + "]");
    }

    /**
     * Converts an LTL-FO+ formula into an equivalent XQuery FLWOR expression.
     * This method is a front-end to {@link translateXQuery(String)}.
     * <p>
     * NOTE: CTL operators do not (yet) have a translation to XQuery. This
     * method provokes an assertion violation.
     */
    protected string translateXQuerySibling(string root)
    {
        //assert(false);
        return null;
    }

    public string translateXQuerySibling(string root, string indent, int variableCount, bool addComments)
    {
        //assert(false);
        return null;
    }
}
