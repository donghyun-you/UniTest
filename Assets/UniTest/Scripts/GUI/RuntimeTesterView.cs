using UnityEngine;
using System.Collections;
using System.Linq;

namespace UniTest 
{
	public class RuntimeTesterView 
		: MonoBehaviour 
	{

		private TesterView _view = null;

		public void Awake() 
		{
			var compositeRunner = new CompositeTestRunner("All Tests");
			compositeRunner.AddRange(TestRunner.GetRootStories().Select(type=>new TestRunner(type) as ITestRunner));
			_view = new TesterView(compositeRunner);
		}

		public void OnGUI() 
		{
			_view.OnGUI();
		}
	}
}