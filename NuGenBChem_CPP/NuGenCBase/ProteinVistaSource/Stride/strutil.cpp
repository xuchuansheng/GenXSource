#include "StdAfx.h"

/*
 * Stride has been included in Qmol with the kind permission of 
 * 
 * Dmitrij Frishman, PhD
 * Institute for Bioinformatics
 * GSF - Forschungszentrum f? Umwelt und Gesundheit, GmbH
 * Ingolst?ter Landstra? 1,
 * D-85764 Neuherberg, Germany
 *
 * Telephone: +49-89-3187-4201
 * Fax: +49-89-31873585
 * e-mail: d.frishman@gsf.de
 * WWW: http://mips.gsf.de/mips/staff/frishman/
 *
 * Stride copyright (see http://www.embl-heidelberg.de/stride/stride.html):
 *
 * All rights reserved, whether the whole  or  part  of  the  program  is
 * concerned.  Permission  to use, copy, and modify this software and its
 * documentation is granted for academic use, provided that:
 *
 *
 * i.	this copyright notice appears in all copies of the software  and
 *		related documentation;
 *
 * ii.  the reference given below (Frishman and  Argos,  1995)  must  be
 *		cited  in any publication of scientific results based in part or
 *		completely on the use of the program;
 *
 * iii.  bugs will be reported to the authors.
 *
 * The use of the  software  in  commercial  activities  is  not  allowed
 * without a prior written commercial license agreement.
 * 
 * WARNING: STRIDE is provided "as-is" and without warranty of any  kind,
 * express,  implied  or  otherwise,  including  without  limitation  any
 * warranty of merchantability or fitness for a particular purpose. In no
 * event will the authors be liable for any special, incidental, indirect
 * or consequential damages  of  any  kind,  or  any  damages  whatsoever
 * resulting  from loss of data or profits, whether or not advised of the
 * possibility of damage, and on any theory of liability, arising out  of
 * or in connection with the use or performance of this software.
 * 
 * For calculation of the residue solvent accessible area the program NSC
 * [3,4]   is   used   and   was  kindly  provided  by  Dr.  F.Eisenhaber
 * (EISENHABER@EMBL-HEIDELBERG.DE). Please direct to  him  all  questions
 * concerning specifically accessibility calculations.
 * 
 * Stride References:
 * 
 * 1.	Frishman,D & Argos,P. (1995) Knowledge-based secondary structure
 * 		assignment.  Proteins:  structure, function and genetics, 23,
 * 		566-579.
 * 
 * 2.	Kabsch,W. & Sander,C. (1983)  Dictionary  of  protein  secondary
 * 		structure:    pattern   recognition   of   hydrogen-bonded   and
 * 		geometrical features. Biopolymers, 22: 2577-2637.
 * 
 * 3.	Eisenhaber,  F.  and  Argos,  P.  (1993)  Improved  strategy  in
 * 		analytic  surface calculation for molecular systems: handling of
 * 		singularities and computational efficiency. J. comput. Chem. 14,
 * 		1272-1280.
 * 
 * 4.	Eisenhaber, F., Lijnzaad, P., Argos, P., Sander, C., and Scharf,
 * 		M.  (1995) The double cubic lattice method: efficient approaches
 * 		to numerical integration of surface area and volume and  to  dot
 * 		surface contouring of molecular assemblies. J. comput. Chem. 16,
 * 		273-284.
 * 
 * 5.	Bernstein, F.C., Koetzle, T.F.,  Williams,  G.J.,  Meyer,  E.F.,
 * 		Brice,  M.D.,  Rodgers,  J.R., Kennard, O., Shimanouchi, T., and
 * 		Tasumi, M.  (1977)  The  protein  data  bank:  a  computer-based
 * 		archival  file for macromolecular structures. J. Mol. Biol. 112,
 * 		535-542.
 * 
 * 6.	Kraulis, P.J.  (1991)  MOLSCRIPT:  a  program  to  produce  both
 * 		detailed  and  schematic  plots  of protein structures. J. Appl.
 * 		Cryst. 24, 946-950.
 * 
 * 7.	Pearson, W.R. (1990) Rapid  and  sensitive  sequence  comparison
 * 		with FASTP and FASTA. Methods. Enzymol. 183, 63-98.
 * 
 */
#include "stride.h"

/*************************************************************************
**                                                                      **
** Find sequential residue number for a PDB residue number              **
**                                                                      **
** INPUT:  *Chain    Pointer to a protein chain                         **
**         *PdbN     String containing PDB residue number               **
**                                                                      **
** OUTPUT: *SeqN     Pointer to the sequential residue number           **
**                                                                      **
*************************************************************************/
int PdbN2SeqN(CHAIN *Chain, char *PdbN, int *SeqN)
{

  for( (*SeqN)=0; (*SeqN)<Chain->NRes; (*SeqN)++ )
    if( !strcmp(Chain->Rsd[(*SeqN)]->PDB_ResNumb,PdbN) ) 
      return(SUCCESS);

  return(FAILURE);
}

/*************************************************************************
**                                                                      **
** Find atom of specified type in a residue                             **
**                                                                      **
** INPUT:  *Chain    Pointer to a protein chain                         **
**         ResNumb   Number of residue in the protein chain             **
**         *Atom     String containing atom name                        **
**                                                                      **
** OUTPUT: *AtNumb   Pointer to the atom number in the residue          **
**                                                                      **
*************************************************************************/
int stride_FindAtom(CHAIN *Chain, int ResNumb, char *Atom, int *AtNumb)
{

  for( (*AtNumb)=0; (*AtNumb)<Chain->Rsd[ResNumb]->NAtom; (*AtNumb)++ )
    if( !strcmp(Atom,Chain->Rsd[ResNumb]->AtomType[(*AtNumb)]) )
       return(SUCCESS);

  *AtNumb = ERR;
  return(FAILURE);
}

/*************************************************************************
**                                                                      **
** Find beginning and end residues of each secondary structure element  **
** in a secondary structure assignment                                  **
**                                                                      **
** INPUT:   *Asn       One letter secondary structure assignment        **
**          L          Length of the protein chain                      **
**          SecondStr  Secondary structure type                         **
**                                                                      **
** OUTPUT:  *(Bound)[2] Two dimensional array containing numbers of     **
**                      first and last residue of each secondary        **
**                      structure element                               **
**                                                                      **
** RETURNS: Number of the Secondary structure elements                  **
**                                                                      **
*************************************************************************/
int Boundaries(char *Asn, int L, char SecondStr, int (*Bound)[2])
{
  register int Res;
  int NStr = 0, Flag = 0;
  
  for( Res=0; Res<L; Res++ ) {
    if( Asn[Res] == SecondStr && Flag == 0 ) {
      Flag = 1; 
      Bound[NStr][0] = Res;
    }
    else
    if( Asn[Res] != SecondStr && Flag == 1 ) {
      Flag = 0; 
      Bound[NStr++][1] = Res-1;
    }
  }
  return(NStr);
}

/*************************************************************************
**                                                                      **
** Find protein chain with a given chain identifier number              **
**                                                                      **
** INPUT:  **Chain   Array of protein chains                            **
**         NChain    Number of chains                                   **
**         ChainId   Chain identifier                                   **
**                                                                      **
** RETURNS: Number of the protein chain with identifier ChainId         **
**                                                                      **
*************************************************************************/
int FindChain(CHAIN **Chain, int NChain, char ChainId)
{
  register int i;

  for( i=0; i<NChain; i++ )
    if( Chain[i]->Id == ChainId )
      return(i);

  return(ERR);
}

STRIDE_BOOL IsHydrogen(char *AtomName) 
{

  if( ( isdigit(AtomName[0]) &&
       ( AtomName[1] == 'H' || AtomName[1] == 'D' || 
	AtomName[1] == 'T' || AtomName[1] == 'Q' ) ) ||
     AtomName[0] == 'H' || AtomName[0] == 'D' || 
     AtomName[0] == 'T' || AtomName[0] == 'Q' )
    return(SUCCESS);
  else
    return(FAILURE);
}

char *Translate(char Code)
{

  static char *Dictionary[18] = {
    "AlphaHelix","310Helix","PiHelix","Strand","Bridge","Coil","TurnI","TurnI'",
    "TurnII","TurnII'","TurnVIa","TurnVIb","TurnVIII","TurnIV","GammaClassic",
    "GammaInv","Turn","Unknown" };

  switch(Code) {
  case 'H': return(Dictionary[0]);
  case 'G': return(Dictionary[1]); 
  case 'I': return(Dictionary[2]); 
  case 'E': return(Dictionary[3]);
  case 'B': 
  case 'b': 
            return(Dictionary[4]);
  case 'C': return(Dictionary[5]);
  case '1': return(Dictionary[6]);
  case '2': return(Dictionary[7]);
  case '3': return(Dictionary[8]);
  case '4': return(Dictionary[9]);
  case '5': return(Dictionary[10]);
  case '6': return(Dictionary[11]);
  case '7': return(Dictionary[12]);
  case '8': return(Dictionary[13]); 
  case '@': return(Dictionary[14]); 
  case '&': return(Dictionary[15]); 
  case 'T': return(Dictionary[16]); 
  default:  return(Dictionary[17]);
  }

}

char SpaceToDash(char Id)
{
  static char NewId;

  if( Id == ' ' )
    NewId = '-';
  else
    NewId = Id;

  return(NewId);
}

STRIDE_BOOL ChInStr(char *String, char Char)
{

  if( strchr(String,toupper(Char)) || 
      strchr(String,Char) ||
      strchr(String,tolower(Char)) )
    return(YES);
  
  return(NO);
}
    
void ExtractAsn(CHAIN **Chain, int Cn, char *Asn)
{
  register int Res;

  for( Res=0; Res<Chain[Cn]->NRes; Res++ )
    Asn[Res] = Chain[Cn]->Rsd[Res]->Prop->Asn;
}

void ExtractPdbAsn(CHAIN **Chain, int Cn, char *Asn)
{
  register int Res;

  for( Res=0; Res<Chain[Cn]->NRes; Res++ )
    Asn[Res] = Chain[Cn]->Rsd[Res]->Prop->PdbAsn;
}

void ExtractDsspAsn(CHAIN **Chain, int Cn, char *Asn)
{
  register int Res;

  for( Res=0; Res<Chain[Cn]->NRes; Res++ )
    Asn[Res] = Chain[Cn]->Rsd[Res]->Prop->DsspAsn;
}

STRIDE_BOOL ExistsSecStr(CHAIN **Chain, int NChain)
{
  register int i, Cn;
  
  for( Cn=0; Cn<NChain; Cn++ )
    for( i=0; i<Chain[Cn]->NRes; i++ )
      if( Chain[Cn]->Rsd[i]->Prop->Asn != 'C' )
	return(YES);

  return(NO);
}
  

/*************************************************************************
**                                                                      **
** Calculate the number of residues in helical or beta sheet state and  **
** what percent they constitute from the total number of residues in a  **
** protein chain                                                        **
**                                                                      **
** INPUT:   *Chain     Pointer to a protein chain                       **
**                                                                      **
** OUTPUT:  *HelAlp    Number of alpha-helical residues                 **
**          *HelPI     Number of residues in pi-helices                 **
**          *Hel310    Number of residues in 3-10 helices               **
**          *Sheet     Number of residues in beta sheet                 **
**                                                                      **
** RETURNS: Secondary structure content                                 **
**                                                                      **
*************************************************************************/
float SecStrContent(CHAIN *Chain, int *HelAlp, int *HelPI, int *Hel310, int *Sheet, int *Turn)
{

  int Res;
  float Content = 0.0;

  *HelAlp = 0; *HelPI = 0; *Hel310 = 0; *Sheet = 0; *Turn = 0;
  
  for( Res=0; Res<Chain->NRes; Res++ ) {
    switch( Chain->Rsd[Res]->Prop->PdbAsn ) {
    case 'H' : (*HelAlp)++;
               break;
    case 'G' : (*Hel310)++;
               break;
    case 'I' : (*HelPI)++;
               break;
    case 'E' : (*Sheet)++;
               break;
    case 'T' : (*Turn)++;
               break;
    }
  }

  Content = ( (float)( (*HelAlp)+(*HelPI)+(*Hel310)+(*Sheet)+(*Turn) ) )/(float)Chain->NRes;

  return(Content);
}


