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

public class OperatorStringParser
{
    /**
     * Constructs an Operator based on a string. This class returns an empty
     * operator. You should use one of its subclasses instead.
     * 
     * @param s
     *            The input string
     * @return An empty Operator.
     */
	public static Operator parseFromString(string s)
	{
		return new Operator();
	}
}
