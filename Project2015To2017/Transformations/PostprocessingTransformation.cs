using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Project2015To2017.Definition;

namespace Project2015To2017.Transformations
{
	internal sealed class PostprocessingTransformation
	{
		public static void ChangePackageReferencesToProjectReferences(Project project, Settings settings)
		{
			if (!settings.IsReplacePackageReferencesWithProjectReferences)
			{
				return;
			}
			foreach (var packageRef in project.PackageReferences.ToArray())
			{
				var packageName = packageRef.Id;
				var projectInSolution = settings.ProjectsInSolution.FirstOrDefault(p => p.projectName == packageName);
				
				if (string.IsNullOrEmpty(projectInSolution.projectName))
				{
					continue;
				}
				
				//TODO

				//project.PackageReferences.Remove(packageRef);
				//project.ProjectReferences.Add(new ProjectReference(){Include = });
			}
		}
	}
}
