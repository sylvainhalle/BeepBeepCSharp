/******************************************************************************
  Runtime monitors for message-based workflows
  Copyright (C) 2008-2010 Sylvain Halle
  
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
 * This exception should be thrown by any method when it cannot guarantee
 * the soundness of its result.
 * It does not indicate a "fault" <em>per se</em>, but rather a warning.
 * Normally the
 * thrower should still be able to produce a result. 
 * @author shalle
 * @version 2010-12-12
 *
 */
public class LossOfSoundnessException : System.Exception
{	
	/**
	* 
	*/
	public LossOfSoundnessException()
	{
		// TODO Auto-generated constructor stub
	}

    /**
     * @param arg0
     */
    public LossOfSoundnessException(string arg0) : base(arg0)
    {
        // TODO Auto-generated constructor stub
    }

    /**
     * @param arg0
     */
    /*public LossOfSoundnessException(Throwable arg0) : base(arg0)
    {
        // TODO Auto-generated constructor stub
    }*/

    /**
     * @param arg0
     * @param arg1
     */
    /*public LossOfSoundnessException(string arg0, Throwable arg1) : base(arg0, arg1)
    {
        // TODO Auto-generated constructor stub
    }*/
}
