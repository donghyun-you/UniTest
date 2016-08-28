using System;
using UniRx;
using System.Collections;
using System.Collections.Generic;

using System.Reflection;
using System.Linq;

namespace UniTest 
{
	public class TestRunner
		: ITestRunner
	{
		public TestNode Tester 
		{
			get; 
			private set; 
		}

		public Type RootType 
		{
			get 
			{
				return Tester.NodeType;
			}
		}

		public TestRunner(Type type) 
		{
			Tester = TestNode.Factory.Create(type,null);
		}

		public void Run(Action<bool> onComplete)
		{
			Tester.Execute(result=>
			{
				TestLogger.Info(this,"test done");
				if(onComplete != null) onComplete(result);
			});
		}

		public static IEnumerable<Type> GetRootStories() 
		{
			return Assembly.GetExecutingAssembly().GetTypes().Where(type=>type.IsNested == false && type.GetCustomAttributes(typeof(TestStoryAttribute),true).Any());
		}
	}
}