#region Copyright
/////////////////////////////////////////////////////////////////////////////
//    Altaxo:  a data processing and data plotting program
//    Copyright (C) 2002-2005 Dr. Dirk Lellinger
//
//    This program is free software; you can redistribute it and/or modify
//    it under the terms of the GNU General Public License as published by
//    the Free Software Foundation; either version 2 of the License, or
//    (at your option) any later version.
//
//    This program is distributed in the hope that it will be useful,
//    but WITHOUT ANY WARRANTY; without even the implied warranty of
//    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//    GNU General Public License for more details.
//
//    You should have received a copy of the GNU General Public License
//    along with this program; if not, write to the Free Software
//    Foundation, Inc., 675 Mass Ave, Cambridge, MA 02139, USA.
//
/////////////////////////////////////////////////////////////////////////////
#endregion

/*
 * ComplexFloatCholeskyDecomp.cs
 * Managed code is a port of JAMA and Jampack code.
 * Copyright (c) 2003-2004, dnAnalytics Project. All rights reserved.
*/

using System;

namespace Altaxo.Calc.LinearAlgebra
{
  ///<summary>This class computes the Cholesky factorization of a n by n <c>ComplexFloatMatrix</c>.</summary>
  /// <remarks>
  /// <para>Copyright (c) 2003-2004, dnAnalytics Project. All rights reserved. See <a>http://www.dnAnalytics.net</a> for details.</para>
  /// <para>Adopted to Altaxo (c) 2005 Dr. Dirk Lellinger.</para>
  /// </remarks>
  public sealed class ComplexFloatCholeskyDecomp : Algorithm 
  {
    private readonly int order;
    private bool ispd = true;
    private ComplexFloatMatrix l;
    private ComplexFloatMatrix matrix;

    ///<summary>Constructor for Cholesky decomposition class. The constructor performs the factorization of a Hermitian positive
    ///definite matrax and the Cholesky factored matrix is accessible by the <c>Factor</c> property. The factor is the lower 
    ///triangular factor.</summary>
    ///<param name="matrix">The matrix to factor.</param>
    ///<exception cref="ArgumentNullException">matrix is null.</exception>
    ///<exception cref="NotSquareMatrixException">matrix is not square.</exception>
    ///<remarks>This class only uses the lower triangle of the input matrix. It ignores the
    ///upper triangle.</remarks>
    public ComplexFloatCholeskyDecomp(IROComplexFloatMatrix matrix)
    {
      if ( matrix == null ) 
      {
        throw new System.ArgumentNullException("matrix cannot be null.");
      }

      if ( matrix.Rows != matrix.Columns ) 
      {
        throw new NotSquareMatrixException("Matrix must be square.");
      }

      order = matrix.Columns;
      this.matrix = new ComplexFloatMatrix(matrix);
    }

    ///<summary>Computes the algorithm.</summary>
    protected override void InternalCompute()
    {
#if MANAGED
      l = new ComplexFloatMatrix(matrix);
      for (int j = 0; j < order; j++) 
      {
        ComplexFloat[] rowj = l.data[j];
        float d = 0.0f;
        for (int k = 0; k < j; k++) 
        {
          ComplexFloat[] rowk = l.data[k];
          ComplexFloat s = ComplexFloat.Zero;
          for (int i = 0; i < k; i++) 
          {
            s += rowk[i]*rowj[i];
          }
          rowj[k] = s = (matrix.data[j][k] - s)/l.data[k][k];
          d = d + (s*ComplexMath.Conjugate(s)).Real;
        }
        d = matrix.data[j][j].Real - d;
        if ( d <= 0.0 ) 
        {
          ispd = false;
          return;
        }
        l.data[j][j] = new ComplexFloat((float)System.Math.Sqrt(d));
        for (int k = j+1; k < order; k++) 
        {
          l.data[j][k] = ComplexFloat.Zero;
        }
      }
#else
            ComplexFloat[] factor = new ComplexFloat[matrix.data.Length];
            Array.Copy(matrix.data, factor, matrix.data.Length);
            int status = Lapack.Potrf.Compute(Lapack.UpLo.Lower, order, factor, order);
            if (status != 0 ) {
                ispd = false;
            }
            l = new ComplexFloatMatrix(order);
            l.data = factor;
            for (int i = 0; i < order; i++) {
                for (int j = 0; j < order; j++) {
                    if ( j > i) {
                        l.data[j*order+i] = 0;
                    }
                }
            }

#endif    
    }

    ///<summary>Return a value indicating whether the matrix is positive definite.</summary>
    ///<returns>true if the matrix is singular; otherwise, false.</returns>
    public bool IsPositiveDefinite
    {
      get 
      {
        Compute();
        return ispd;
      }
    }

    ///<summary>Returns the Cholesky factored matrix (lower triangular form).</summary>
    ///<returns>the lower Cholesky factored matrix.</returns>
    public ComplexFloatMatrix Factor
    {
      get 
      {
        Compute();
        return l;
      }
    }

    ///<summary>Calculates the determinant of the matrix.</summary>
    ///<returns>the determinant of the matrix.</returns>
    ///<exception cref="NotPositiveDefiniteException">A is not positive definite.</exception>
    public ComplexFloat GetDeterminant()
    {
      Compute();
      if ( !ispd ) 
      {
        throw new NotPositiveDefiniteException();
      }
      ComplexFloat ret = ComplexFloat.One;
      for ( int j = 0; j < order; j++ ) 
      {
#if MANAGED
        ret *= (l.data[j][j] * l.data[j][j]);
#else
                ret *= (l.data[j*order+j]*l.data[j*order+j]);
#endif
      }
      return ret;
    }

    ///<summary>Solves a system on linear equations, AX=B, where A is the factored matrixed.</summary>
    ///<param name="B">RHS side of the system.</param>
    ///<returns>the solution matrix, X.</returns>  
    ///<exception cref="ArgumentNullException">B is null.</exception>
    ///<exception cref="NotPositiveDefiniteException">A is not positive definite.</exception>
    ///<exception cref="ArgumentException">The number of rows of A and B must be the same.</exception>
    public ComplexFloatMatrix Solve (IROComplexFloatMatrix B) 
    {
      if ( B == null ) 
      {
        throw new System.ArgumentNullException("B cannot be null.");
      }
      Compute();
      if ( !ispd ) 
      {
        throw new NotPositiveDefiniteException();
      } 
      else 
      {
        if ( B.Rows != order ) 
        {
          throw new System.ArgumentException("Matrix row dimensions must agree." );
        }
#if MANAGED
        // Copy right hand side.
        int cols = B.Columns;
        ComplexFloatMatrix X = new ComplexFloatMatrix(B);
        for (int c = 0; c < cols; c++ ) 
        {
          // Solve L*Y = B;
          for (int i = 0; i < order; i++) 
          {
            ComplexFloat sum = B[i,c];
            for (int k = i-1; k >= 0; k--) 
            {
              sum -= l.data[i][k] * X.data[k][c];
            }
            X.data[i][c] = sum / l.data[i][i];
          }

          // Solve L'*X = Y;
          for (int i =order-1; i >= 0; i--) 
          {
            ComplexFloat sum = X.data[i][c];
            for (int k = i+1; k < order; k++) 
            {
              sum -= ComplexMath.Conjugate(l.data[k][i]) * X.data[k][c];
            }
            X.data[i][c] = sum / ComplexMath.Conjugate(l.data[i][i]);
          }
        }

        return X;
#else
                ComplexFloat[] rhs = ComplexFloatMatrix.ToLinearComplexArray(B);
                Lapack.Potrs.Compute(Lapack.UpLo.Lower,order,B.Columns,l.data,order,rhs,B.Rows);
                ComplexFloatMatrix ret = new ComplexFloatMatrix(order,B.Columns);
                ret.data = rhs;
                return ret;
#endif
      }
    }

    ///<summary>Solves a system on linear equations, AX=B, where A is the factored matrixed.</summary>
    ///<param name="B">RHS side of the system.</param>
    ///<returns>the solution vector, X.</returns>  
    ///<exception cref="ArgumentNullException">B is null.</exception>
    ///<exception cref="NotPositiveDefiniteException">A is not positive definite.</exception>
    ///<exception cref="ArgumentException">The number of rows of A and the length of B must be the same.</exception>
    public ComplexFloatVector Solve (IROComplexFloatVector B) 
    {
      if ( B == null ) 
      {
        throw new System.ArgumentNullException("B cannot be null.");
      }
      Compute();
      if ( !ispd ) 
      {
        throw new NotPositiveDefiniteException();
      } 
      else 
      {
        if ( B.Length != order ) 
        {
          throw new System.ArgumentException("The length of B must be the same as the order of the matrix." );
        }
#if MANAGED
        // Copy right hand side.
        ComplexFloatVector X = new ComplexFloatVector(B);
        // Solve L*Y = B;
        for (int i = 0; i < order; i++) 
        {
          ComplexFloat sum = B[i];
          for (int k = i-1; k >= 0; k--) 
          {
            sum -= l.data[i][k] * X.data[k];
          }
          X.data[i] = sum / l.data[i][i];
        }
        // Solve L'*X = Y;
        for (int i =order-1; i >= 0; i--) 
        {
          ComplexFloat sum = X.data[i];
          for (int k = i+1; k < order; k++) 
          {
            sum -= ComplexMath.Conjugate(l.data[k][i]) * X.data[k];
          }
          X.data[i] = sum / ComplexMath.Conjugate(l.data[i][i]);
        }

        return X;

#else
                ComplexFloat[] rhs = ComplexFloatMatrix.ToLinearComplexArray(B);
                Lapack.Potrs.Compute(Lapack.UpLo.Lower,order,1,l.data,order,rhs,B.Length);
                ComplexFloatVector ret = new ComplexFloatVector(order,B.Length);
                ret.data = rhs;
                return ret;
#endif
      }
    }

    ///<summary>Calculates the inverse of the matrix.</summary>
    ///<returns>the inverse of the matrix.</returns>  
    ///<exception cref="NotPositiveDefiniteException">A is not positive definite.</exception>
    public ComplexFloatMatrix GetInverse()
    {
      Compute();
      if ( !ispd ) 
      {
        throw new NotPositiveDefiniteException();
      } 
      else 
      {
#if MANAGED
        ComplexFloatMatrix ret = ComplexFloatMatrix.CreateIdentity(order);
        ret = Solve(ret);
        return ret;
#else
                ComplexFloat[] inverse = new ComplexFloat[l.data.Length];
                Array.Copy(l.data,inverse,l.data.Length);
                Lapack.Potri.Compute(Lapack.UpLo.Lower, order, inverse, order);
                ComplexFloatMatrix ret = new ComplexFloatMatrix(order,order);
                ret.data = inverse;
                for (int i = 0; i < order; i++) {
                    for (int j = 0; j < order; j++) {
                        if ( j > i) {
                            ret.data[j*order+i] = ComplexMath.Conjugate(ret.data[i*order+j]);
                        }
                    }
                }
                return ret;
#endif
      }
    }
  }
}
