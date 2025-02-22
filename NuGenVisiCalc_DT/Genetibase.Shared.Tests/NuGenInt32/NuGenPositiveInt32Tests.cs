/* -----------------------------------------------
 * NuGenPositiveInt32Tests.cs
 * Copyright � 2006 Anthony Nystrom
 * mailto:a.nystrom@genetibase.com
 * --------------------------------------------- */

using NUnit.Framework;

using System;
using System.Collections.Generic;

namespace Genetibase.Shared.Tests
{
	[TestFixture]
	public partial class NuGenPositiveInt32Tests
	{
		private NuGenPositiveInt32 _int;
		private EventSink _eventSink;

		[SetUp]
		public void SetUp()
		{
			_int = new NuGenPositiveInt32();
			_eventSink = new EventSink(_int);
		}

		[TearDown]
		public void TearDown()
		{
			_eventSink.Verify();
		}

		[Test]
		public void EqualsTest()
		{
			_eventSink.ExpectedValueChangedCount = 2;

			NuGenPositiveInt32 compared = new NuGenPositiveInt32();
			compared.Value = 20;

			_int.Value = 20;
			Assert.IsTrue(_int.Equals(compared));

			_int.Value = 30;
			Assert.IsFalse(_int.Equals(compared));
			Assert.IsFalse(_int.Equals(null));
		}

		[Test]
		[ExpectedException(typeof(ArgumentException))]
		public void ValueArgumentExceptionTest()
		{
			_eventSink.ExpectedValueChangedCount = 0;
			_int.Value = -10;
		}
	}
}
