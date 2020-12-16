using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace CSharpRuntimeCameo
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			Bosch.VideoSDK.Core core = new Bosch.VideoSDK.Core();
            // Set VideoSDK in unsecure mode for certain legacy devices, refer to Concepts->Fundamentals->Security Properties in VideoSDK document for more information.
            Bosch.VideoSDK.GCALib.ISecurityProperties sec = (Bosch.VideoSDK.GCALib.ISecurityProperties)core;
            sec.SecurityProperties = (int)(Bosch.VideoSDK.GCALib.SecurityPropertiesEnum.speAllowUnencryptedConnections |
                                           Bosch.VideoSDK.GCALib.SecurityPropertiesEnum.speAllowUnencryptedMediaExports |
                                           Bosch.VideoSDK.GCALib.SecurityPropertiesEnum.speAllowNoForwardSecrecy);
            core.Startup();
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			Application.Run(new MainWindow());
			core.Shutdown(false);
		}
	}
}
