/* ==========================================
* JGraphT : a free Java graph-theory library
* ==========================================
*
* Project Info:  http://jgrapht.sourceforge.net/
* Project Lead:  Barak Naveh (http://sourceforge.net/users/barak_naveh)
*
* (C) Copyright 2003-2004, by Barak Naveh and Contributors.
*
* This library is free software; you can redistribute it and/or modify it
* under the terms of the GNU Lesser General Public License as published by
* the Free Software Foundation; either version 2.1 of the License, or
* (at your option) any later version.
*
* This library is distributed in the hope that it will be useful, but
* WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
* or FITNESS FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public
* License for more details.
*
* You should have received a copy of the GNU Lesser General Public License
* along with this library; if not, write to the Free Software Foundation, Inc.,
* 59 Temple Place, Suite 330, Boston, MA 02111-1307, USA.
*/
/* -----------------------------
* DirectedWeightedSubgraph.java
* -----------------------------
* (C) Copyright 2003, by Barak Naveh and Contributors.
*
* Original Author:  Barak Naveh
* Contributor(s):   -
*
* $Id: DirectedWeightedSubgraph.java,v 1.5 2004/11/19 10:42:38 barak_naveh Exp $
*
* Changes
* -------
* 05-Aug-2003 : Initial revision (BN);
*
*/
using System;
using DirectedGraph = org._3pq.jgrapht.DirectedGraph;
using WeightedGraph = org._3pq.jgrapht.WeightedGraph;
namespace org._3pq.jgrapht.graph
{
	
	/// <summary> A directed weighted graph that is a subgraph on other graph.
	/// 
	/// </summary>
	/// <seealso cref="Subgraph">
	/// </seealso>
	[Serializable]
	public class DirectedWeightedSubgraph:DirectedSubgraph, WeightedGraph
	{
		private const long serialVersionUID = 3905799799168250680L;
		
		/// <summary> Creates a new weighted directed subgraph.
		/// 
		/// </summary>
		/// <param name="base">the base (backing) graph on which the subgraph will be
		/// based.
		/// </param>
		/// <param name="vertexSubset">vertices to include in the subgraph. If
		/// <code>null</code> then all vertices are included.
		/// </param>
		/// <param name="edgeSubset">edges to in include in the subgraph. If
		/// <code>null</code> then all the edges whose vertices found in the
		/// graph are included.
		/// </param>
		public DirectedWeightedSubgraph(WeightedGraph base_Renamed, SupportClass.SetSupport vertexSubset, SupportClass.SetSupport edgeSubset):base((DirectedGraph) base_Renamed, vertexSubset, edgeSubset)
		{
		}
	}
}