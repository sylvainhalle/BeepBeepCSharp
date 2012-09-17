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
 * Defines a runtime generator, i.e. a machine that produces sequences of
 * messages based on a specification.
 * 
 * @author Sylvain Hall&eacute;
 *
 */
public interface RuntimeGenerator
{
	/**
   	 * Like {@link generate()}, but does not throw an exception if soundness
   	 * cannot be guaranteed.  
   	 * 
   	 * @return A String rendition of the generated message, and an
   	 * empty string otherwise.
   	 */
	string generateUnsound();
	
	/**
   	 * From the current state of the generator, produce a new message.
   	 * 
   	 * @throws LossOfSoundnessException - if the generator could not
   	 * produce any message whose soundness is guaranteed, throws this
   	 * exception to warn the user of that fact. To obtain the message
   	 * nonetheless, one must call {@link generateUnsound()}.
   	 * Note that this does not mean that the message does not satisfy
   	 * the property; only that it cannot be <em>guaranteed</em> to
   	 * satisfy it. The generator might use a conservative approximation
   	 * of soundness.
   	 * 
   	 * @return A String rendition of the generated message, and an
   	 * empty string otherwise.
   	 */
	// Dom: throws LossOfSoundnessException, no equivalent in C#
	string generate();
	
	/**
   	 * Reset the current state of the generator to its initial state
   	 * @return
   	 */
	void reset();
	
	/**
     * Assigns a specification to the generator.
   	 * @param s A String rendition of the specification
   	 * @return
   	 */
	bool setFormula(Operator o);
}
