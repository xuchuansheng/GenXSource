#!/usr/bin/env python
#******************************************************************************
#  $Id: gcps2vec.py 9355 2006-03-21 21:54:00Z fwarmerdam $
# 
#  Project:  GDAL
#  Purpose:  Convert GCPs to a point layer.
#  Author:   Frank Warmerdam, warmerdam@pobox.com
# 
#******************************************************************************
#  Copyright (c) 2005, Frank Warmerdam <warmerdam@pobox.com>
# 
#  Permission is hereby granted, free of charge, to any person obtaining a
#  copy of this software and associated documentation files (the "Software"),
#  to deal in the Software without restriction, including without limitation
#  the rights to use, copy, modify, merge, publish, distribute, sublicense,
#  and/or sell copies of the Software, and to permit persons to whom the
#  Software is furnished to do so, subject to the following conditions:
# 
#  The above copyright notice and this permission notice shall be included
#  in all copies or substantial portions of the Software.
# 
#  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS
#  OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
#  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
#  THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
#  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
#  FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
#  DEALINGS IN THE SOFTWARE.
#******************************************************************************
# 
# $Log$
# Revision 1.2  2006/03/21 21:54:00  fwarmerdam
# fixup headers
#
# Revision 1.1  2005/02/07 14:36:12  fwarmerdam
# New
#
#

import gdal
import ogr
import sys

def Usage():
    print 'Usage: gcp2vec.py [-of <ogr_drivername>] <raster_file> <vector_file>'
    sys.exit(1)

# =============================================================================
# 	Mainline
# =============================================================================

format = 'GML'
in_file = None
out_file = None
pixel_out = 0

gdal.AllRegister()
argv = gdal.GeneralCmdLineProcessor( sys.argv )
if argv is None:
    sys.exit( 0 )

# Parse command line arguments.
i = 1
while i < len(argv):
    arg = argv[i]

    if arg == '-of':
        format = argv[i]

    if arg == '-p':
        pixel_out = 1

    elif in_file is None:
        in_file = argv[i]

    elif out_file is None:
        out_file = argv[i]

    else:
        Usage()

    i = i + 1

if out_file is None:
    Usage()

# ----------------------------------------------------------------------------
# Open input file, and fetch GCPs. 
# ----------------------------------------------------------------------------
ds = gdal.Open( in_file )
if ds is None:
    print 'Unable to open ', filename
    sys.exit(1)

gcp_srs = ds.GetGCPProjection()
gcps = ds.GetGCPs()

ds = None

if gcps is None or len(gcps) == 0:
    print 'No GCPs on file %s!' % in_file
    sys.exit(1)

# ----------------------------------------------------------------------------
# Create output file, and layer.
# ----------------------------------------------------------------------------

drv = ogr.GetDriverByName( format )
if drv is None:
    print 'No driver named %s available.' % format
    sys.exit(1)

ds = drv.CreateDataSource( out_file )

srs = None

layer = ds.CreateLayer( 'gcps', srs, geom_type = ogr.wkbPoint25D )

if pixel_out == 0:
    fd = ogr.FieldDefn( 'Pixel', ogr.OFTReal )
    layer.CreateField( fd )

    fd = ogr.FieldDefn( 'Line', ogr.OFTReal )
    layer.CreateField( fd )
else:
    fd = ogr.FieldDefn( 'X', ogr.OFTReal )
    layer.CreateField( fd )

    fd = ogr.FieldDefn( 'Y', ogr.OFTReal )
    layer.CreateField( fd )

fd = ogr.FieldDefn( 'Id', ogr.OFTString )
layer.CreateField( fd )

fd = ogr.FieldDefn( 'Info', ogr.OFTString )
layer.CreateField( fd )

# ----------------------------------------------------------------------------
# Write GCPs. 
# ----------------------------------------------------------------------------

for gcp in gcps:

    feat = ogr.Feature( layer.GetLayerDefn() )

    geom = ogr.Geometry( ogr.wkbPoint25D )

    if pixel_out == 0:
        feat.SetField( 'Pixel', gcp.GCPPixel )
        feat.SetField( 'Line',  gcp.GCPLine )
        geom.SetPoint( 0, gcp.GCPX, gcp.GCPY, gcp.GCPZ )
    else:
        feat.SetField( 'X', gcp.GCPX )
        feat.SetField( 'Y',  gcp.GCPY )
        geom.SetPoint( 0, gcp.GCPPixel, gcp.GCPLine )
        
    feat.SetField( 'Id',    gcp.Id )
    feat.SetField( 'Info',  gcp.Info )

    feat.SetGeometryDirectly( geom )
    layer.CreateFeature( feat )

feat = None
ds.Destroy()


