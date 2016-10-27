using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace UniTest.Server 
{
	public class TestServerHandlers 
		: IDisposable
	{
		#region internal
		private TestServer _server = null;
		private bool _isDisposed = false;
		public IDisposable disposable = null;

		public TestServerHandlers(TestServer server) 
		{
			_server = server;
			disposable = BindPackets(server);	
			TestLogger.OnLogged += onLogReceived;
			TestLogger.OnExceptionLogged += onExceptionReceived;
		}

		public void Dispose() 
		{
			if(_isDisposed == false) 
			{
				_isDisposed = true;
				disposable.Dispose();
				TestLogger.OnLogged -= onLogReceived;
				TestLogger.OnExceptionLogged -= onExceptionReceived;
			}
		}

		private IDisposable BindPackets(TestServer server) 
		{
			List<IDisposable> disposables = new List<IDisposable>();

			disposables.Add(server.RegisterHandler(TestServer.MessageType.STDIN,OnStdin));
				
			return DisposableCreator.Create(()=>
			{
				foreach(var disposable in disposables) 
				{
					disposable.Dispose();
				}
				disposables.Clear();
			});
		}

		private Protocol _receiver = new Protocol();
		private void OnStdin(TestServer.ClientConnection sender,TestServer.MessageType message_type,string json) 
		{
			TestLogger.Verbose(this,"<<< "+json);
			JsonUtility.FromJsonOverwrite(json,_receiver);

			Type containerType = Type.GetType("UniTest.Server.TestServerHandlers+Protocol+"+_receiver.func);

			TestLogger.Info(this,"received type: "+containerType);

			if(containerType == null) 
			{
				TestLogger.Warning(this,"dropping stdin message. unexpected protocol: "+json);
			}
			else 
			{
				Protocol received = JsonUtility.FromJson(json,containerType) as Protocol;
				this.GetType().InvokeMember("OnMessageReceived",BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.InvokeMethod,null,this,new object[] { sender, received });
			}
		}

		#endregion

		#region business logics
		public class Protocol 
		{
			public string func;
			public string args;

			public class RunAllTest : Protocol 
			{
			}

			public class RunTestOfType : Protocol 
			{
				public Type GetTestType() 
				{
					var type = Type.GetType(args.Trim());

					if(type == null) 
					{
						throw new InvalidOperationException("Type: "+(args.Trim() ?? "")+" cannot be parsed");
					}

					return type;
				}
			}
		}

		private bool _isTestRunning = false;
		private List<TestServer.ClientConnection> _senders = new List<TestServer.ClientConnection>();

		private void OnMessageReceived(TestServer.ClientConnection sender,Protocol.RunAllTest message) 
		{
			if(_isTestRunning) 
			{
				sender.SendError("[UniTest/Error]: Test is already invoked and running. ignoring your request");
				sender.SendExit("1");
			}
			else 
			{
				_isTestRunning = true;

				CompositeTestRunner tester = new CompositeTestRunner("All Tests");
				tester.AddRange(TestRunner.GetRootStories().Select(type=>new TestRunner(type) as ITestRunner));

				TestLogger.Verbose(this,"RunAllTest");

				_senders.Add(sender);

				tester.Run(result=>{
					TestLogger.Info(this,"Test result: "+result);

					// NOTE(donghyun-you): equivalent with standard stream "exit"
					sender.SendExit(result ? "0" : "1");
				},()=>{
					_senders.Remove(sender);
					_server.CloseClient(sender);
					_isTestRunning = false;
					Debug.Log("[UniTest/Info] Test completed");
				});
			}
		}

		private void OnMessageReceived(TestServer.ClientConnection sender,Protocol.RunTestOfType message) 
		{
			var testType = message.GetTestType();

			if(_isTestRunning) 
			{
				sender.SendError("[UniTest/Error]: Test is already invoked and running. ignoring your request");
				sender.SendExit("1");
			}
			else 
			{
				_isTestRunning = true;

				ITestRunner tester = new TestRunner(testType) as ITestRunner;

				TestLogger.Verbose(this,"RunTestOfType: "+(testType == null ? "?":testType.ToString()));

				_senders.Add(sender);

				tester.Run(result=>{
					TestLogger.Info(this,"Test result: "+result);

					// NOTE(donghyun-you): equivalent with standard stream "exit"
					sender.SendExit(result ? "0" : "1");
				},()=>{
					_senders.Remove(sender);
					_server.CloseClient(sender);
					_isTestRunning = false;
					Debug.Log("[UniTest/Info] Test completed");
				});
			}
		}

		private void onLogReceived(TestLogger.LogType type, object invoker, string text) 
		{
			foreach(var sender in _senders) 
			{
				if(sender.IsDisposed) 
				{
					Debug.Log("["+this.GetType().Name+"] sender("+sender.InstanceId+") is disposed. this log will not be sent: "+text);
				}
				else 
				{
					string invokerName = invoker == null ? "null" : invoker.GetType().Name;

					switch(type) 
					{
					case TestLogger.LogType.kWarning:
						sender.SendOut("<color=yellow>[UniTest/"+invokerName+"/Warning]</color> "+text);
						break;
					case TestLogger.LogType.kError:
						sender.SendError("<color=red>[UniTest/"+invokerName+"/Error]</color> "+text);
						break;
					case TestLogger.LogType.kInfo:
						sender.SendOut("[UniTest/"+invokerName+"/Info]: "+text);
						break;
					case TestLogger.LogType.kVerbose:
						sender.SendOut("<color=cyan>[UniTest/"+invokerName+"/Verbose]</color> "+text);
						break;
					}
				}
			}
		}

		private void onExceptionReceived(object invoker, Exception ex) 
		{

			foreach(var sender in _senders) 
			{
				if(sender.IsDisposed) 
				{
					Debug.Log("["+this.GetType().Name+"] sender("+sender.InstanceId+") is disposed. this log will not be sent: "+ex.ToString());
				}
				else 
				{
					sender.SendError("[UniTest/"+invoker+"/Exception]: "+ex.Message+"\n"+ex.StackTrace);
				}
			}

		}

		#endregion
		

	}
}