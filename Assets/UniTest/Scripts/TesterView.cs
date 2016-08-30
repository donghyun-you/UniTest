using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace UniTest 
{
	public class TesterView
	{
		private ITestRunner _runner = null;
		private GuiFolder _folder = null;
		private bool _isTesting = false;
		private Vector2 _scrollView = new Vector2();

		public TesterView(ITestRunner runner) 
		{
			_runner = runner;
			_folder = new GuiFolder();
		}

		public void OnGUI() 
		{
			GUILayout.Label("Test Hierarchy");

			if(GUILayout.Button("Reset")) 
			{
				resetRecursively(_runner.Tester);
			}

			_scrollView = GUILayout.BeginScrollView(_scrollView);
			drawElement(_runner.Tester);
			GUILayout.EndScrollView();
		}

		void drawElement(TestElement element) 
		{
			GUILayout.BeginHorizontal();

			if(_isTesting == false) 
			{
				if(GUILayout.Button("▶︎",GUI.skin.box,GUILayout.Height(30f),GUILayout.Width(20f))) 
				{
					test(element);	
				}
			}

			drawState(element);

			GUILayout.BeginVertical();

			if(element is TestNode) 
			{
				var node = element as TestNode;
				if(_folder.Fold(node.InstanceID,node.SelfStory)) 
				{
					if(node.Children != null) 
					{
						foreach(var child in node.Children) 
						{
							GUILayout.BeginHorizontal();
							GUILayout.Space(10f);
							drawElement(child);
							GUILayout.EndHorizontal();
						}
					}
				}
			}
			else if(element is TestMethod) 
			{
				GUILayout.BeginHorizontal();
				GUILayout.Label(element.SelfStory);
				GUILayout.EndHorizontal();

				if(element.Parent != null) 
				{
					List<string> reports;
					if(element.Parent.TestedMethodReports.TryGetValue(element.Name,out reports)) 
					{
						Color colorBefore = GUI.color;
						GUI.color = Color.cyan;
						foreach(var report in reports) 
						{
							GUILayout.Label(report);
						}		
						GUI.color = colorBefore;
					}
				}

				if(element.TestState == TestResultType.kFailed) 
				{
					if(element.FailedException != null) 
					{
						if(_folder.Fold(element.InstanceID+"_ex","Error details")) 
						{
							GUILayout.Label(element.FailedException.ToString(),GUI.skin.textArea);
						}
					}
				}
			}
			else 
			{
				GUILayout.Label("Unable to find any element/node");
			}

			GUILayout.EndVertical();
			GUILayout.EndHorizontal();
		}

		void test(TestElement target) 
		{
			resetRecursively(target);

			_isTesting = true;
			target.Execute(result=>{
				
			},()=>{
				_isTesting = false;
			});
		}

		void resetRecursively(TestElement target) 
		{
			target.Reset();
			if(target is TestNode) 
			{
				foreach(var child in (target as TestNode).Children) 
				{
					resetRecursively(child as TestElement);
				}
			}
		}

		void drawState(TestElement target) 
		{
			Color color = GUI.color;
			switch(target.TestState) 
			{
				case TestResultType.kFailed:
					GUI.color = Color.red;
					GUILayout.Label("\u2716",GUILayout.Width(15f));
				break;

				case TestResultType.kPassed:
					GUI.color = Color.green;
					GUILayout.Label("\u2714",GUILayout.Width(15f));
				break;

				case TestResultType.kIgnored:
					GUI.color = Color.yellow;
					GUILayout.Label("!",GUILayout.Width(15f));
				break;

				case TestResultType.kNotTested:
					GUI.color = Color.gray;
					GUILayout.Label("?",GUILayout.Width(15f));
				break;
			}
			GUI.color = color;
		}
	}
}
