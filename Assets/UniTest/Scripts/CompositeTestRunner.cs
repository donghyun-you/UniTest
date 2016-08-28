using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using System.Linq;

namespace UniTest 
{
	public class CompositeTestRunner 
		: ITestRunner 
	{

		public CompositeTestRunner(string description) 
		{
			Tester = TestNode.Factory.CreateCompositionNode(description);	
		}

		public TestNode Tester 
		{
			get; 
			private set;
		}

		List<ITestRunner> _testers = new List<ITestRunner>();

		public void Run(Action<bool> onComplete) 
		{
			run(onComplete).ToObservable().Subscribe();
		}

		IEnumerator run(Action<bool> onComplete) 
		{
			onComplete = onComplete ?? delegate(bool obj) {};
			
			foreach(var tester in _testers) 
			{
				if(tester != null) 
				{
					bool result = false;
					bool isDone = false;

					tester.Run(i_result=>
					{
						result = i_result;
						isDone = true;
					});

					while(isDone == false) 
					{
						yield return null;
					}

					if(result == false) 
					{
						onComplete(false);
						yield break;
					}
				}
			}

			onComplete(true);
		}

		public void Add(ITestRunner runner) 
		{
			_testers.Add(runner);
			Tester.Add(runner.Tester as TestElement);
		}

		public void AddRange(IEnumerable<ITestRunner> runners) 
		{
			if(runners.Any()) 
			{
				_testers.AddRange(runners);
				Tester.AddRange(runners.Select(runner=>runner.Tester as TestElement));
			}
		}
	}
}