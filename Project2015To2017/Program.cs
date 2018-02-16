﻿using Project2015To2017.Definition;
using Project2015To2017.Writing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Xml.Linq;
using Project2015To2017.Transformations;

[assembly: InternalsVisibleTo("Project2015To2017Tests")]

namespace Project2015To2017
{
	class Program
	{

		#region Props

		private static readonly IReadOnlyList<ITransformation> _transformationsToApply = new ITransformation[]
		{
			new ProjectPropertiesTransformation(),
			new ProjectReferenceTransformation(),
			new PackageReferenceTransformation(),
			new AssemblyReferenceTransformation(),
			new RemovePackageAssemblyReferencesTransformation(),
			new FileTransformation(),
			new AssemblyInfoTransformation(),
			new NugetPackageTransformation()
		};

		private static Settings _settings;

		#endregion

		/// <summary>
		/// Entry point.
		/// </summary>
		/// <param name="args">The arguments.</param>
		static void Main(string[] args)
		{
			if (args.Length == 0)
			{
				Console.WriteLine($"Please specify a project file or a solution directory.");
				Console.WriteLine("Usage example :");
				Console.WriteLine(
					"Project2015To2017.exe <.csproj file or solution directory> "
					+ "[--netstandard | -s] [--assemblyinfo | -a] [--versions | -v] "
					+ "[--del_old | -d] [--del_tfs_settings | -t] [--compile_items | -c] [--project_references | -p]");
				return;
			}

			_settings = ReadSettings(args);

			var projectFileOrSolutionFolder = args[0];
			
			// Process all csproj files found in given directory and subderictories
			if (Path.GetExtension(projectFileOrSolutionFolder) != ".csproj")
			{
				var projectFiles = _settings.ProjectsInSolution.Select(p => p.projectFileName).ToArray();
				if (projectFiles.Length == 0)
				{
					Console.WriteLine($"Please specify a project file or a solution directory.");
					return;
				}
				Console.WriteLine($"Multiple project files found under directory {args[0]}:");
				Console.WriteLine(string.Join(Environment.NewLine, projectFiles));
				foreach (var projectFile in projectFiles)
				{
					ProcessFile(projectFile);
				}

				return;
			}

			// Process only the given project file
			ProcessFile(projectFileOrSolutionFolder);
		}

		#region Methods for reading settings
		
		private static Settings ReadSettings(string[] args)
		{
			Settings ret = new Settings
			{
				IsMigrateToNetStandard = args.FirstOrDefault(a => a == "--netstandard" || a == "-s") != null,
				IsUseAssemblyInfoFile = args.FirstOrDefault(a => a == "--assemblyinfo" || a== "-a") != null,
				IsGenerateVersionsElements = args.FirstOrDefault(a => a == "--versions" || a== "-v") != null,
				IsDeleteOldFilesExceptProject = args.FirstOrDefault(a => a == "--del_old" || a== "-d") != null,
				IsDeleteTfsSettingsFile = args.FirstOrDefault(a => a == "--del_tfs_settings" || a== "-t") != null,
				IsDisableDefaultCompileItems = args.FirstOrDefault(a => a == "--compile_items" || a== "-c") != null,
				IsReplacePackageReferencesWithProjectReferences = args.FirstOrDefault(a => a == "--project_references" || a == "-r") != null,
				ProjectsInSolution = ReadProjectsFromSolution(args[0])
			};
			
			return ret;
		}

		private static HashSet<(string projectFileName, string projectName)> ReadProjectsFromSolution(string path)
		{
			if (path == null)
			{
				throw new ArgumentNullException(nameof(path));
			}

			string solutionDirectory = path;

			if (Path.GetExtension(path) == ".csproj")
			{
				solutionDirectory = Path.GetDirectoryName(path);
			}

			return new HashSet<(string projectFileName, string projectName)>(
				Directory.EnumerateFiles(solutionDirectory, "*.csproj", SearchOption.AllDirectories)
					.Select(f => (f, Path.GetFileNameWithoutExtension(f)))
			);
		}

		#endregion

		#region Methods for file processing

		private static void ProcessFile(string filePath)
		{
			var file = new FileInfo(filePath);
			if (!Validate(file))
			{
				return;
			}

			XDocument xmlDocument;
			using (var stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				xmlDocument = XDocument.Load(stream);
			}

			XNamespace nsSys = "http://schemas.microsoft.com/developer/msbuild/2003";
			if (xmlDocument.Element(nsSys + "Project") == null)
			{
				Console.WriteLine($"This is not a VS2015 project file.");
				return;
			}

			var projectDefinition = new Project();

			var fileInfo = new FileInfo(filePath);
			var directory = fileInfo.Directory;

			string vsPsccFilePath = fileInfo.FullName + ".vspscc";

			if (_settings.IsDeleteTfsSettingsFile)
			{
				File.Delete(vsPsccFilePath);
			}

			_settings.IsUseVsPsccFileToConfigureVersionControl = File.Exists(vsPsccFilePath);

			Task.WaitAll(_transformationsToApply.Select(
					t => t.TransformAsync(xmlDocument, directory, projectDefinition, _settings))
				.ToArray());

			AssemblyReferenceTransformation.RemoveExtraAssemblyReferences(projectDefinition);

			PostprocessingTransformation.ChangePackageReferencesToProjectReferences(projectDefinition, _settings);

			var projectFile = fileInfo.FullName;
			if (!SaveBackup(projectFile))
			{
				return;
			}
			
			new ProjectWriter(_settings).Write(projectDefinition, fileInfo);

			var packagesFile = Path.Combine(fileInfo.DirectoryName, "packages.config");
			if (File.Exists(packagesFile))
			{
				if (!RenameFile(packagesFile, _settings.IsDeleteOldFilesExceptProject))
				{
					return;
				}
			}

			if (!_settings.IsUseAssemblyInfoFile)
			{
				var assemblyInfoFile = Path.Combine(fileInfo.DirectoryName, "Properties", "AssemblyInfo.cs");
				if (File.Exists(assemblyInfoFile))
				{
					if (!RenameFile(assemblyInfoFile, _settings.IsDeleteOldFilesExceptProject))
					{
						return;
					}
				}
			}
		}

		internal static bool Validate(FileInfo file)
		{
			if (!file.Exists)
			{
				Console.WriteLine($"File {file.FullName} could not be found.");
				return false;
			}

			if (file.IsReadOnly)
			{
				Console.WriteLine($"File {file.FullName} is readonly, please make the file writable first (checkout from source control?).");
				return false;
			}

			return true;
		}

		private static bool SaveBackup(string filename)
		{
			var output = false;

			var backupFileName = filename + ".old";
			if (File.Exists(backupFileName))
			{
				Console.Write($"Cannot create backup file. Please delete {backupFileName}.");
			}
			else
			{
				File.Copy(filename, filename + ".old");
				output = true;
			}

			return output;
		}

		private static bool RenameFile(string filename, bool isDeleteInsteadOfRenaming = false)
		{
			var output = false;

			var backupFileName = filename + ".old";
			if (File.Exists(backupFileName))
			{
				Console.Write($"Cannot create backup file. Please delete {backupFileName}.");
			}
			else
			{
				if (isDeleteInsteadOfRenaming)
				{
					File.Delete(filename);

					var dirPath = Path.GetDirectoryName(filename);
					DirectoryInfo di = new DirectoryInfo(dirPath);
					if (!di.EnumerateDirectories().Any()
						&& !di.EnumerateFiles().Any())
					{
						di.Delete();
					}

					output = true;
				}
				else
				{
					File.Move(filename, filename + ".old");
					output = true;
				}
			}

			return output;
		} 

		#endregion

	}
}
