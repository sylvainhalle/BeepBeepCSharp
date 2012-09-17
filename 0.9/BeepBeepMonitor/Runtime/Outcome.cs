/**
 * The set of outcomes returned by the watcher, the meaning of which is the
 * following:
 * <ul>
 * <li>TRUE: the trace fulfills the property, no matter what follows</li>
 * <li>FALSE: the trace violates the property, no matter what follows</li>
 * <li>INCONCLUSIVE: none of the previous choices</li>
 * </ul>
 * 
 * @author Sylvain Hallé
 * 
 */
public enum Outcome
{
	TRUE, 
	FALSE, 
	INCONCLUSIVE
};
