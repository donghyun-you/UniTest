using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace UniTest.Server 
{
	public class TestServerLauncher : MonoBehaviour {

		TestServer server = null;
		TestServerHandlers handlers = null;

		public const string IP_STORE_FILENAME = "ip_address";

		public void Start () 
		{
			server = TestServer.Factory.Create(7701);	
			handlers = new TestServerHandlers(server);
			server.Start();

			updateIp();
		}

		private string IPSavePath 
		{
			get 
			{
				return Application.persistentDataPath + "/" + IP_STORE_FILENAME;
			}
		}

		private void updateIp() 
		{
			var ips 		= getIps();
			var savePath 	= this.IPSavePath;

			Debug.Log("ip("+string.Join(",",ips.ToArray())+") of "+System.Net.Dns.GetHostName()+" updated on "+savePath);
			File.WriteAllText(savePath,string.Join("\n",ips.ToArray()));
		}

		private IEnumerable<string> getIps() 
		{
			if(System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable() == false) 
			{
				yield break;
			}
			else 
			{
				yield return Network.player.ipAddress;

				var hostname = System.Net.Dns.GetHostName();
				var host = System.Net.Dns.GetHostEntry(hostname);
				foreach(var ip in host.AddressList.Select(entry=>entry.ToString())) 
				{
					yield return ip;	
				}
			}
		}

		private void deleteIp() 
		{
			if(File.Exists(this.IPSavePath)) 
			{
				File.Delete(this.IPSavePath);
			}
		}

		public void OnApplicationQuit() 
		{
			handlers.Dispose();
			server.Close();
		}
	}
}