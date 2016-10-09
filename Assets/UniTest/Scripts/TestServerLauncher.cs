using UnityEngine;
using System.Collections;

namespace UniTest.Server 
{
	public class TestServerLauncher : MonoBehaviour {

		TestServer server = null;
		TestServerHandlers handlers = null;

		// Use this for initialization
		void Start () 
		{
			server = TestServer.Factory.Create(7701);	
			handlers = new TestServerHandlers(server);
			server.Start();
		}

		void OnApplicationQuit() 
		{
			handlers.Dispose();
			server.Close();
		}
	}
}