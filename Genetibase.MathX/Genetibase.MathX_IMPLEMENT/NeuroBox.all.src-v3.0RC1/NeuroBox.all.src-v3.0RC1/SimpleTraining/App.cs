#region Copyright 2003 Christoph Daniel R�egg [Modified BSD License]
/*
A Simple Training Demonstration using NeuroBox Neural Network Library
Copyright (c) 2003, Christoph Daniel Rueegg, Switzerland
http://cdrnet.net/. All rights reserved.

Redistribution and use in source and binary forms, with or without modification,
are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice,
this list of conditions and the following disclaimer. 

2. Redistributions in binary form must reproduce the above copyright notice,
this list of conditions and the following disclaimer in the documentation
and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE
ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE
LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN
CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE)
ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF
THE POSSIBILITY OF SUCH DAMAGE.
*/
#endregion

using System;
using System.Collections;

namespace SimpleTraining
{
	class App
	{
		[STAThread]
		static void Main(string[] args)
		{
			DemoCollection dc = new DemoCollection();
			
			dc.Add(new BasicBoundDemo());
			dc.Add(new BasicUnboundDemo());
			dc.Add(new BuildingBlockDemo());
			dc.Add(new NetworkStructureDemo());
			
			dc.RunDemos();

			Console.ReadLine();
		}

		public static void PrintArray(double[] data)
		{
			for(int i=0;i<data.Length;i++)
				Console.Write(Math.Round(data[i],4).ToString() + " ");
			Console.WriteLine();
		}
	}

	public class DemoCollection: CollectionBase
	{
		public IDemo this[int index]
		{
			get {return (IDemo)InnerList[index];}
			set {InnerList[index] = value;}
		}
		public int Add(IDemo demo)
		{
			return InnerList.Add(demo);
		}
		public void RunDemos()
		{
			foreach(IDemo demo in InnerList)
				demo.RunDemo();
		}
	}
}
