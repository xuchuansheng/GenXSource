/*
 * $Id: MetalLookAndFeelAddons.java,v 1.3 2007/03/16 21:38:13 rbair Exp $
 *
 * Copyright 2004 Sun Microsystems, Inc., 4150 Network Circle,
 * Santa Clara, California 95054, U.S.A. All rights reserved.
 *
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 * 
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser General Public
 * License along with this library; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin St, Fifth Floor, Boston, MA  02110-1301  USA
 */
package n2f.mediauploader.common.plaf.metal;

import n2f.mediauploader.common.plaf.basic.BasicLookAndFeelAddons;

/**
 * MetalLookAndFeelAddons.<br>
 *
 */
public class MetalLookAndFeelAddons extends BasicLookAndFeelAddons {

    @Override
  public void initialize() {
    super.initialize();
    loadDefaults(getDefaults());
  }

    @Override
  public void uninitialize() {
    super.uninitialize();
    unloadDefaults(getDefaults());
  }
  
  private Object[] getDefaults() {
    return new Object[] {
//        "DirectoryChooserUI",
//        "n2f.mediauploader.common.plaf.windows.WindowsDirectoryChooserUI",
    };
  }
  
}
