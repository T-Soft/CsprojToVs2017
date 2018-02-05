﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Project2015To2017;
using Project2015To2017.Definition;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Project2015To2017.Transformations;
using Project2015To2017Tests.Helpers;

namespace Project2015To2017Tests
{
    [TestClass]
    public class NugetPackageTransformationTest
    {
        [TestMethod]
        public async Task ConvertsNuspecAsync()
        {
            var project = new Project();

            var directoryInfo = new DirectoryInfo(".\\TestFiles");
            var doc = XDocument.Load("TestFiles\\net46console.testcsproj");

            project.AssemblyAttributes = new AssemblyAttributes
            {
                AssemblyName = "TestAssembly",
                InformationalVersion = "7.0",
                Copyright = "copyright from assembly",
                Description = "description from assembly",
                Company = "assembly author"
            };
            await new NugetPackageTransformation().TransformAsync(doc, directoryInfo, project, SettingsFactory.Create()).ConfigureAwait(false);

            Assert.IsNull(project.PackageConfiguration.Id);
            Assert.IsNull(project.PackageConfiguration.Version);
            Assert.AreEqual("some author", project.PackageConfiguration.Authors);
            Assert.AreEqual("copyright from assembly", project.PackageConfiguration.Copyright);
            Assert.AreEqual(true, project.PackageConfiguration.RequiresLicenseAcceptance);
            Assert.AreEqual("a nice description.", project.PackageConfiguration.Description);
            Assert.AreEqual("some tags API", project.PackageConfiguration.Tags);
            Assert.AreEqual("someurl", project.PackageConfiguration.LicenseUrl);
            Assert.AreEqual("Some long\n        text\n        with newlines", project.PackageConfiguration.ReleaseNotes.Trim());
        }

        [TestMethod]
        public async Task ConvertsDependencies()
        {
            var project = new Project();

            var directoryInfo = new DirectoryInfo(".\\TestFiles");
            var doc = XDocument.Load("TestFiles\\net46console.testcsproj");
            
            project.PackageReferences = new[] 
            {
                new PackageReference { Id = "Newtonsoft.Json", Version = "10.0.2" },
                new PackageReference { Id = "Other.Package", Version = "1.0.2" }
            };
            await new NugetPackageTransformation().TransformAsync(doc, directoryInfo, project, SettingsFactory.Create()).ConfigureAwait(false);

            Assert.AreEqual("[10.0.2,11)", project.PackageReferences.Single(x => x.Id == "Newtonsoft.Json").Version);
            Assert.AreEqual("1.0.2", project.PackageReferences.Single(x => x.Id == "Other.Package").Version);
        }
    }
}
