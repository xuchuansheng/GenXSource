#region Copyright 2001-2006 Christoph Daniel R�egg [GPL]
//Math.NET Symbolics: Yttrium, part of Math.NET
//Copyright (c) 2001-2006, Christoph Daniel Rueegg, http://cdrnet.net/.
//All rights reserved.
//This Math.NET package is available under the terms of the GPL.

//This program is free software; you can redistribute it and/or modify
//it under the terms of the GNU General Public License as published by
//the Free Software Foundation; either version 2 of the License, or
//(at your option) any later version.

//This program is distributed in the hope that it will be useful,
//but WITHOUT ANY WARRANTY; without even the implied warranty of
//MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//GNU General Public License for more details.

//You should have received a copy of the GNU General Public License
//along with this program; if not, write to the Free Software
//Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
#endregion

using System;
using System.Collections.Generic;
using System.Text;

using MathNet.Symbolics.Core;

namespace MathNet.Symbolics.Backend.Traversing
{
    public class ExistsSignalVisitor : AbstractScanVisitor
    {
        private Predicate<Signal> _match;
        private Signal _foundSignal;
        private Port _foundSignalTarget;

        public ExistsSignalVisitor(Predicate<Signal> match)
        {
            _match = match;
        }

        public void Reset()
        {
            _foundSignal = null;
            _foundSignalTarget = null;
        }

        public bool Exists
        {
            get { return _foundSignal != null; }
        }

        public Signal FoundSignal
        {
            get { return _foundSignal; }
        }

        public Port FoundSignalTarget
        {
            get { return _foundSignalTarget; }
        }

        public override bool EnterSignal(Signal signal, Port parent, bool again, bool root)
        {
            if(again)
                return false;
            if(_match(signal))
            {
                _foundSignal = signal;
                _foundSignalTarget = parent;
                return false;
            }
            return true;
        }

        public override bool LeaveSignal(Signal signal, Port parent, bool again, bool root)
        {
            return _foundSignal == null;
        }
    }
}
