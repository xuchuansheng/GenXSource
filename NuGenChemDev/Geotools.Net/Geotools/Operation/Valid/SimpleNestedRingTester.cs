/*
 *  Copyright (C) 2002 Urban Science Applications, Inc. (translated from Java Topology Suite, 
 *  Copyright 2001 Vivid Solutions)
 *
 *  This library is free software; you can redistribute it and/or
 *  modify it under the terms of the GNU Lesser General Public
 *  License as published by the Free Software Foundation; either
 *  version 2.1 of the License, or (at your option) any later version.
 *
 *  This library is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 *  Lesser General Public License for more details.
 *
 *  You should have received a copy of the GNU Lesser General Public
 *  License along with this library; if not, write to the Free Software
 *  Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
 *
 */


#region Using
using System;
using System.Collections;
using Geotools.Algorithms;
using Geotools.Geometries;
using Geotools.Graph;
#endregion

namespace Geotools.Operation.Valid
{
	/// <summary>
	/// Summary description for SimpleNestedRingTester.
	/// </summary>
	internal class SimpleNestedRingTester
	{
		private static  CGAlgorithms _cga = new RobustCGAlgorithms();

		private GeometryGraph _graph;  // used to find non-node vertices
		private ArrayList _rings = new ArrayList();
		private Coordinate _nestedPt;

		#region Constructors
		/// <summary>
		/// Initializes a new instance of the SimpleNestedRingTester class.
		/// </summary>
		public SimpleNestedRingTester(GeometryGraph graph)
		{
			this._graph = graph;
		}
		#endregion

		#region Properties
		#endregion

		#region Methods
		public void Add(LinearRing ring)
		{
			_rings.Add(ring);
		}

		public Coordinate GetNestedPoint()
		{
			return _nestedPt;
		}
		public bool IsNonNested()
		{
			for (int i = 0; i < _rings.Count; i++) 
			{
				LinearRing innerRing = (LinearRing) _rings[i];
				Coordinates innerRingPts = innerRing.GetCoordinates();

				for (int j = 0; j < _rings.Count; j++) 
				{
					LinearRing searchRing = (LinearRing) _rings[j];
					Coordinates searchRingPts = searchRing.GetCoordinates();

					if (innerRing == searchRing)
						continue;

					if (! innerRing.GetEnvelopeInternal().Intersects(searchRing.GetEnvelopeInternal()))
						continue;

					Coordinate innerRingPt = IsValidOp.FindPtNotNode(innerRingPts, searchRing, _graph);
					//Assert.isTrue(innerRingPt != null, "Unable to find a ring point not a node of the search ring");
					//Coordinate innerRingPt = innerRingPts[0];

					bool isInside = _cga.IsPointInRing(innerRingPt, searchRingPts);
					if (isInside) 
					{
						_nestedPt = innerRingPt;
						return false;
					}
				}
			}
			return true;
		}

		#endregion

	}
}
