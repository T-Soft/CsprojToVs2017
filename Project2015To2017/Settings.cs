using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project2015To2017
{
	public class Settings
	{
		/// <summary>
		/// Gets or sets a value indicating whether TargetFramework node should change its value from existing to netstandard2.0.
		/// </summary>
		/// <value>
		///   <c>true</c> if TargetFramework node should change its value from existing to netstandard2.0; otherwise, <c>false</c>.
		/// </value>
		public bool IsMigrateToNetStandard { set; get; }

		/// <summary>
		/// Gets or sets a value indicating whether the AssemblyInfo file should be used instead of palcing all the information from it to *.csproj.
		/// </summary>
		/// <value>
		///   <c>true</c> if this instance is use assembly information file; otherwise, <c>false</c>.
		/// </value>
		/// <remarks>This results in Generate*Attribute nodes generation with default false values. AssemblyInfo file is not deleted and still used.</remarks>
		public bool IsUseAssemblyInfoFile { set; get; }

		/// <summary>
		/// Gets or sets a value indicating whether VS PSCC TFS configuration file should be used to configure version control.
		/// </summary>
		/// <value>
		///   <c>true</c> if VS PSCC TFS configuration file should be used to configure version control; otherwise, <c>false</c>.
		/// </value>
		/// <remarks>This results in *Scc nodes with SAK default value generation </remarks>
		public bool IsUseVsPsccFileToConfigureVersionControl { set; get; }

		/// <summary>
		/// Gets or sets a value indicating whether *.old files should be deleted. This option does not delete *.csproj.old file.
		/// </summary>
		/// <value>
		///   <c>true</c> if this instance is delete old files except project; otherwise, <c>false</c>.
		/// </value>
		public bool IsDeleteOldFilesExceptProject { set; get; }

		/// <summary>
		/// Gets or sets a value indicating whether the TFS settings file should be deleted.
		/// </summary>
		/// <value>
		///   <c>true</c> if the TFS settings file should be deleted; otherwise, <c>false</c>.
		/// </value>
		public bool IsDeleteTfsSettingsFile { set; get; }

		/// <summary>
		/// Gets or sets a value indicating whether the Compile items definitions from old project file should be included as-is instead of being automatically generated.
		/// </summary>
		/// <value>
		///   <c>true</c> if the Compile items definitions from old project file should be included as-is instead of being automatically generated.; otherwise, <c>false</c>.
		/// </value>
		public bool IsDisableDefaultCompileItems { set; get; }

		/// <summary>
		/// Gets or sets a value indicating whether <c>Version</c> and <c>FileVersion</c> Property nodes should be generated during poject write.
		/// </summary>
		/// <value>
		///   <c>true</c> if <c>Version</c> and <c>FileVersion</c> Property nodes should be generated during poject write; otherwise, <c>false</c>.
		/// </value>
		public bool IsGenerateVersionsElements { set; get; }

		/// <summary>
		/// Gets or sets a value indicating whether <c>EnableDefaultContentItems</c> node should be generated with <c>false</c> value or not generated at all.
		/// </summary>
		/// <value>
		///   <c>true</c> if <c>EnableDefaultContentItems</c> should be generated with <c>false</c> value; otherwise, <c>false</c>.
		/// </value>
		public bool IsDisableDefaultContentItems { set; get; }
	}
}
