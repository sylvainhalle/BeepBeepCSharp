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
 * The AF operator implements the functionalities of the CTL AF temporal
 * modality.
 * 
 * @author Sylvain Hallé
 * @version 2008-05-23
 */
public class OperatorAF : UnaryOperator
{
    private static string symbol = "AF";

    public OperatorAF()
        : base()
    {
        m_symbol = OperatorAF.symbol;
    }

    public OperatorAF(Operator o)
        : base(o)
    {
        m_symbol = OperatorAF.symbol;
    }

    public override Operator getNegated()
    {
        return new OperatorEG(m_operand.getNegated());
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

    public override Operator toExplicit()
    {
        UnaryOperator o = new OperatorAF();

        o.setOperand(m_operand.toExplicit());

        return o;
    }
}
