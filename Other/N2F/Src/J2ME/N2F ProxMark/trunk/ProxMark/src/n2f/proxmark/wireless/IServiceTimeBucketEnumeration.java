/* ------------------------------------------------
 * IServiceTimeBucketEnumeration.java
 * Copyright � 2007 Alex Nesterov
 * mailto:alex@next2friends.com
 * ---------------------------------------------- */

package n2f.proxmark.wireless;

/**
 * An object that implements IServiceTimeBucketEnumeration interface provides 
 * functionality to iterate a ServiceTimeBucket collection.
 *
 * @author Alex Nesterov
 */
public interface IServiceTimeBucketEnumeration
{
    /**
     * Tests if this enumeration contains more elements.
     *
     * @return	<code>true</code> if and only if this enumeration object
     *		contains at least one more element to provide;
     *		<code>false</code> otherwise.
     */
    boolean hasMoreElements();
    
    /**
     * Returns the next element of this enumeration if this enumeration object
     * has at least one more element to provide.
     *
     * @return The next element of this enumeration.
     * @throws NoSuchElementException if no more elements exist.
     */
    ServiceTimeBucket nextElement();
}
