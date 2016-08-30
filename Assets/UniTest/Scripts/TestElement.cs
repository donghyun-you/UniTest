using UnityEngine;
using System;
using System.Collections;
using System.Linq;

namespace UniTest 
{
	public abstract class TestElement
	{
		public TestNode Parent
		{
			get;
			protected set;
		}

		public int Order 
		{
			get; 
			protected set; 
		}

		public TestResultType TestState 
		{
			get;
			protected set;
		}

		public Exception FailedException 
		{
			get;
			protected set;
		}

		public object Instance 
		{
			get;
			protected set; 
		}

		public string SelfStory 
		{
			get;
			protected set;
		}

		public string Name 
		{
			get;
			protected set;
		}

		public virtual string Story 
		{
			get 
			{
				if(this.Parent == null) 
				{
					return this.SelfStory;
				}
				else 
				{
					return (this.Parent.Story ?? "")+" "+(this.SelfStory ?? "");
				}
			}
		}

		public string InstanceID 
		{
			get;
			private set;
		}

		public static int s_instanceIdIncrement = 0;

		public TestElement() 
		{
			InstanceID = "test_element_"+(s_instanceIdIncrement++);
		}

		public abstract void Execute(Action<bool> onFinished,Action on_complete);
		public abstract string Summarize();

		public void MarkAsIgnored() 
		{
			this.TestState = TestResultType.kIgnored;
		}

		public virtual void Reset() 
		{
			this.TestState = TestResultType.kNotTested;
		}
	}
}