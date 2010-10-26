#region License

/*
 * Copyright � 2004, 2005 Griffin Caprio & Choy Rim. All rights reserved.
 * 
 * Copyright � 2007 Alex Nesterov [Modifier]. All rights reseved.
 * mailto:a.nesterov@genetibase.com
 * 
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions
 * are met:
 * 1. Redistributions of source code must retain the above copyright
 * notice, this list of conditions and the following disclaimer.
 * 2. Redistributions in binary form must reproduce the above copyright
 * notice, this list of conditions and the following disclaimer in the
 * documentation and/or other materials provided with the distribution.
 * 3. The name of the author may not be used to endorse or promote products
 * derived from this software without specific prior written permission.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE AUTHOR ``AS IS'' AND ANY EXPRESS OR
 * IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES
 * OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED.
 * IN NO EVENT SHALL THE AUTHOR BE LIABLE FOR ANY DIRECT, INDIRECT,
 * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT
 * NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
 * DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF
 * THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

#endregion

using System;
using System.Runtime.Serialization;

namespace Genetibase.NuGenMock
{
	/// <summary>
	/// Assertion exception to encapsulate framework specific exceptions into a general <see cref="AssertionException"/>
	/// that is used throughout the NuGenMock library.
	/// </summary>
	public class AssertionException : Exception
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="AssertionException"/> class.
		/// </summary>
		public AssertionException() : base()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AssertionException"/> class.
		/// </summary>
		/// <param name="message">Message for this exception.</param>
		public AssertionException(string message) : base(message)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AssertionException"/> class.
		/// </summary>
		/// <param name="message">Message for this exception.</param>
		/// <param name="innerException">Inner exception for this exception.</param>
		public AssertionException(string message, Exception innerException) : base(message, innerException)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AssertionException"/> class.
		/// </summary>
		/// <param name="serialInfo">Object that holds the serialized object data.</param>
		/// <param name="streamingContext">The contextual information about the source or destination.</param>
		protected AssertionException(SerializationInfo serialInfo, StreamingContext streamingContext) : base(serialInfo, streamingContext)
		{
		}
	}
}