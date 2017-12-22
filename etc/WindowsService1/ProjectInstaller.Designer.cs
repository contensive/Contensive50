using System.ComponentModel;
using System.Configuration.Install;

//========================================================================
// This conversion was produced by the Free Edition of
// Instant C# courtesy of Tangible Software Solutions.
// Order the Premium Edition at https://www.tangiblesoftwaresolutions.com
//========================================================================

//INSTANT C# NOTE: Formerly VB project-level imports:
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;
namespace Contensive
{


	[System.ComponentModel.RunInstaller(true)]
	public partial class ProjectInstaller : System.Configuration.Install.Installer
	{
		//Installer overrides dispose to clean up the component list.
		[System.Diagnostics.DebuggerNonUserCode()]
		protected override void Dispose(bool disposing)
		{
			try
			{
				if (disposing && components != null)
				{
					components.Dispose();
				}
			}
			finally
			{
				base.Dispose(disposing);
			}
		}

		//Required by the Component Designer
		private System.ComponentModel.IContainer components;

		//NOTE: The following procedure is required by the Component Designer
		//It can be modified using the Component Designer.  
		//Do not modify it using the code editor.
		[System.Diagnostics.DebuggerStepThrough()]
		private void InitializeComponent()
		{
			this.ServiceProcessInstaller1 = new System.ServiceProcess.ServiceProcessInstaller();
			this.ServiceInstaller1 = new System.ServiceProcess.ServiceInstaller();
			//
			//ServiceProcessInstaller1
			//
			this.ServiceProcessInstaller1.Account = System.ServiceProcess.ServiceAccount.LocalSystem;
			this.ServiceProcessInstaller1.Password = null;
			this.ServiceProcessInstaller1.Username = null;
			//
			//ServiceInstaller1
			//
			this.ServiceInstaller1.Description = "Essential service for running Contensive Applications such as websites";
			this.ServiceInstaller1.DisplayName = "Contensive Service";
			this.ServiceInstaller1.ServiceName = "Contensive Service";
			this.ServiceInstaller1.StartType = System.ServiceProcess.ServiceStartMode.Automatic;
			//
			//ProjectInstaller
			//
			this.Installers.AddRange(new System.Configuration.Install.Installer[] {this.ServiceProcessInstaller1, this.ServiceInstaller1});

//INSTANT C# NOTE: Converted design-time event handler wireups:
			ServiceProcessInstaller1.AfterInstall += new System.Configuration.Install.InstallEventHandler(ServiceProcessInstaller1_AfterInstall);
			ServiceInstaller1.AfterInstall += new System.Configuration.Install.InstallEventHandler(ServiceInstaller1_AfterInstall);
		}
		internal System.ServiceProcess.ServiceInstaller ServiceInstaller1;
		public System.ServiceProcess.ServiceProcessInstaller ServiceProcessInstaller1;

	}
}
