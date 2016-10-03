using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

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
		}

		public void Dispose() 
		{
			if(_isDisposed == false) 
			{
				_isDisposed = true;
				disposable.Dispose();
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
					return Type.GetType(args.Trim());
				}
			}
		}

		private void OnMessageReceived(TestServer.ClientConnection sender,Protocol.RunAllTest message) 
		{
			TestLogger.Verbose(this,"RunAllTest");

			var tester = TesterManager.Instance.Tester;

			TestLogger.LogEvent onLogged = delegate(TestLogger.LogType type, object invoker, string text) {

				string invokerName = invoker == null ? "null" : invoker.GetType().Name;

				switch(type) 
				{
					case TestLogger.LogType.kWarning:
						sender.SendOut("[UniTest/"+invokerName+"/Warning] "+text);
					break;
					case TestLogger.LogType.kError:
						sender.SendError("[UniTest/"+invokerName+"/Error] "+text);
					break;
					case TestLogger.LogType.kInfo:
						sender.SendOut("[UniTest/"+invokerName+"/Info]: "+text);
					break;
					case TestLogger.LogType.kVerbose:
						sender.SendOut("[UniTest/"+invokerName+"/Verbose]: "+text);
					break;
				}
			};

			TestLogger.LogExceptionEvent onExceptionLogged = delegate(object invoker, Exception ex) {

				sender.SendError("[UniTest/"+invoker+"/Exception]: "+ex.Message+"\n"+ex.StackTrace);

			};

			TestLogger.OnLogged += onLogged;
			TestLogger.OnExceptionLogged += onExceptionLogged;

			var loggerDisposables = DisposableCreator.Create(()=>{

				TestLogger.OnLogged -= onLogged;
				TestLogger.OnExceptionLogged -= onExceptionLogged;

			});

			tester.Run(result=>{
				TestLogger.Info(this,"test done: "+result);
			},()=>{
				loggerDisposables.Dispose();
				_server.CloseClient(sender);
			});
		}

		private void OnMessageReceived(TestServer.ClientConnection sender,Protocol.RunTestOfType message) 
		{
			var testType = message.GetTestType();
			TestLogger.Verbose(this,"RunTestOfType: "+(testType == null ? "?":testType.ToString()));
			sender.SendOut(JsonUtility.ToJson(new string[] { "TestOut" }));
			sender.SendError(JsonUtility.ToJson(new string[] { "TestError" }));
			_server.CloseClient(sender);
		}
		#endregion
		

	}
}