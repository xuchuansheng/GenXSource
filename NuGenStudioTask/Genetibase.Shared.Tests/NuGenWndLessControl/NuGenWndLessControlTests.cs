/* -----------------------------------------------
 * NuGenWndLessControlTests.cs
 * Copyright � 2007 Alex Nesterov
 * mailto:a.nesterov@genetibase.com
 * --------------------------------------------- */

using Genetibase.Shared.Windows;

using NUnit.Framework;

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace Genetibase.Shared.Tests
{
	[TestFixture]
	public partial class NuGenWndLessControlTests
	{
		private NuGenWndLessControl _ctrl = null;
		private StubControl _parentCtrl = null;
		private EventSink _eventSink = null;

		private MouseEventArgs _goodMouseEventArgs = null;
		private MouseEventArgs _goodMouseEventArgs2 = null;
		private MouseEventArgs _badMouseEventArgs = null;

		[SetUp]
		public void SetUp()
		{
			_parentCtrl = new StubControl();
			_parentCtrl.Bounds = new Rectangle(0, 0, 200, 200);

			_ctrl = new NuGenWndLessControl();
			_ctrl.Bounds = new Rectangle(20, 20, 50, 50);
			_ctrl.Parent = _parentCtrl;

			_eventSink = new EventSink(_ctrl);

			_goodMouseEventArgs = new MouseEventArgs(MouseButtons.Left, 1, 30, 30, 0);
			_goodMouseEventArgs2 = new MouseEventArgs(MouseButtons.Left, 1, 50, 50, 0);
			_badMouseEventArgs = new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0);
		}

		[TearDown]
		public void TearDown()
		{
			_eventSink.Verify();
		}

		[Test]
		public void BoundsTest()
		{
			Rectangle boundsRectangle = new Rectangle(0, 0, 100, 100);

			_ctrl.Bounds = boundsRectangle;
			Assert.AreEqual(boundsRectangle, _ctrl.Bounds);
		}

		[Test]
		public void ClickTest()
		{
			_eventSink.ExpectedClickCount = 2;

			_parentCtrl.InvokeClick(_goodMouseEventArgs);
			_parentCtrl.InvokeClick(_badMouseEventArgs);
			_parentCtrl.InvokeClick(_goodMouseEventArgs2);

			_ctrl.Enabled = false;

			_parentCtrl.InvokeClick(_goodMouseEventArgs);
			_parentCtrl.InvokeClick(_badMouseEventArgs);
			_parentCtrl.InvokeClick(_goodMouseEventArgs);
		}

		[Test]
		public void MouseDownTest()
		{
			_eventSink.ExpectedMouseDownCount = 2;

			_parentCtrl.InvokeMouseDown(_goodMouseEventArgs);
			_parentCtrl.InvokeMouseDown(_badMouseEventArgs);
			_parentCtrl.InvokeMouseDown(_goodMouseEventArgs2);

			_ctrl.Enabled = false;

			_parentCtrl.InvokeMouseDown(_goodMouseEventArgs);
			_parentCtrl.InvokeMouseDown(_badMouseEventArgs);
			_parentCtrl.InvokeMouseDown(_goodMouseEventArgs2);
		}

		[Test]
		public void MouseEnterLeaveTest()
		{
			_eventSink.ExpectedMouseEnterCount = 3;
			_eventSink.ExpectedMouseLeaveCount = 2;

			_parentCtrl.InvokeMouseMove(_goodMouseEventArgs);
			_parentCtrl.InvokeMouseMove(_badMouseEventArgs);
			_parentCtrl.InvokeMouseMove(_goodMouseEventArgs2);
			_parentCtrl.InvokeMouseMove(_goodMouseEventArgs);

			_ctrl.Enabled = false;

			_parentCtrl.InvokeMouseMove(_goodMouseEventArgs);
			_parentCtrl.InvokeMouseMove(_badMouseEventArgs);
			_parentCtrl.InvokeMouseMove(_goodMouseEventArgs2);
			_parentCtrl.InvokeMouseMove(_goodMouseEventArgs);

			_ctrl.Enabled = true;

			_parentCtrl.InvokeMouseEnter(_goodMouseEventArgs);
			_parentCtrl.InvokeMouseLeave(_goodMouseEventArgs2);
		}

		[Test]
		public void MouseMoveTest()
		{
			_eventSink.ExpectedMouseMoveCount = 2;

			_parentCtrl.InvokeMouseMove(_goodMouseEventArgs);
			_parentCtrl.InvokeMouseMove(_badMouseEventArgs);
			_parentCtrl.InvokeMouseMove(_goodMouseEventArgs2);
		}

		[Test]
		public void MouseUpTest()
		{
			_eventSink.ExpectedMouseDownCount = 1;
			_eventSink.ExpectedMouseUpCount = 1;

			_parentCtrl.InvokeMouseDown(_goodMouseEventArgs);
			_parentCtrl.InvokeMouseUp(_badMouseEventArgs);

			_ctrl.Enabled = false;

			_parentCtrl.InvokeMouseDown(_goodMouseEventArgs);
			_parentCtrl.InvokeMouseUp(_badMouseEventArgs);
		}

		[Test]
		public void ParentTest()
		{
			_eventSink.ExpectedPaintCount = 1;
			_eventSink.ExpectedParentChangedCount = 2;

			Form parentForm = new Form();
			_ctrl.Parent = parentForm;
			parentForm.Show();

			Assert.AreEqual(parentForm, _ctrl.Parent);

			parentForm.Refresh();

			_ctrl.Parent = null;
			parentForm.Refresh();
		}
	}
}
