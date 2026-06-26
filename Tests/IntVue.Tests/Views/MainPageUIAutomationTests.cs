
using System.Xml.Linq;

namespace IntVue.Tests.Views;

[TestClass]
public sealed class MainPageUIAutomationTests
{
    private XDocument? _xamlDocument;

    private static string FindProjectRoot()
    {
        var dir = new DirectoryInfo(
            Path.GetDirectoryName(typeof(MainPageUIAutomationTests).Assembly.Location)!);
        while (dir != null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "IntVue.csproj")))
            {
                return dir.FullName;
            }

            dir = dir.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate IntVue project root (no IntVue.csproj found in parent hierarchy)");
    }

    [TestInitialize]
    public void TestInitialize()
    {
        var projectRoot = FindProjectRoot();
        var xamlPath = Path.Combine(projectRoot, "Views", "MainPage.xaml");

        if (!File.Exists(xamlPath))
        {
            Assert.Fail($"MainPage.xaml not found at {xamlPath}");
        }

        _xamlDocument = XDocument.Load(xamlPath);
    }

    [TestMethod]
    public void MainPage_AllButtons_HaveTabIndex()
    {
        Assert.IsNotNull(_xamlDocument);
        var ns = XNamespace.Get("http://schemas.microsoft.com/winfx/2006/xaml/presentation");
        var buttons = _xamlDocument!.Descendants(ns + "Button").ToList();

        Assert.IsTrue(buttons.Count > 0, "No buttons found in MainPage.xaml");

        foreach (var button in buttons)
        {
            var tabIndex = button.Attribute("TabIndex");
            Assert.IsNotNull(tabIndex, $"Button {button.Attribute(XNamespace.Get("http://schemas.microsoft.com/winfx/2006/xaml") + "Name")} missing TabIndex attribute");
        }
    }

    [TestMethod]
    public void MainPage_ComboBox_HasTabIndex()
    {
        Assert.IsNotNull(_xamlDocument);
        var ns = XNamespace.Get("http://schemas.microsoft.com/winfx/2006/xaml/presentation");
        var comboBox = _xamlDocument!.Descendants(ns + "ComboBox").FirstOrDefault();

        Assert.IsNotNull(comboBox, "ComboBox not found in MainPage.xaml");
        var tabIndex = comboBox!.Attribute("TabIndex");
        Assert.IsNotNull(tabIndex, "ComboBox missing TabIndex attribute");
        Assert.AreEqual("0", tabIndex.Value, "CbCameraList should have TabIndex=0");
    }

    [TestMethod]
    public void MainPage_TabIndexOrder_IsCorrect()
    {
        Assert.IsNotNull(_xamlDocument);
        var ns = XNamespace.Get("http://schemas.microsoft.com/winfx/2006/xaml/presentation");
        var xNamespace = XNamespace.Get("http://schemas.microsoft.com/winfx/2006/xaml");

        var controlsByTabIndex = _xamlDocument!
            .Descendants()
            .Where(e => e.Attribute("TabIndex") != null)
            .OrderBy(e => int.TryParse(e.Attribute("TabIndex")?.Value, out var idx) ? idx : int.MaxValue)
            .ToList();

        var expectedOrder = new[]
        {
            ("CbCameraList", 0),
            ("BtnInitializeDevice", 1),
            ("BtnPreview", 2),
            ("BtnRecord", 3),
            ("BtnPlay", 4),
            ("BtnDelete", 5)
        };

        Assert.AreEqual(expectedOrder.Length, controlsByTabIndex.Count, "TabIndex count mismatch");

        for (int i = 0; i < expectedOrder.Length; i++)
        {
            var (expectedName, expectedIndex) = expectedOrder[i];
            var control = controlsByTabIndex[i];
            var name = control.Attribute(xNamespace + "Name")?.Value;
            var tabIndex = control.Attribute("TabIndex")?.Value;

            Assert.AreEqual(expectedName, name, $"Expected {expectedName} at TabIndex position {i}");
            Assert.AreEqual(expectedIndex.ToString(), tabIndex, $"Expected TabIndex={expectedIndex} for {expectedName}");
        }
    }

    [TestMethod]
    public void MainPage_MainButtons_HaveAccessKeys()
    {
        Assert.IsNotNull(_xamlDocument);
        var ns = XNamespace.Get("http://schemas.microsoft.com/winfx/2006/xaml/presentation");
        var xNamespace = XNamespace.Get("http://schemas.microsoft.com/winfx/2006/xaml");

        var expectedAccessKeys = new Dictionary<string, string>
        {
            { "BtnRecord", "R" },
            { "BtnPlay", "P" },
            { "BtnDelete", "D" },
            { "BtnPreview", "V" },
            { "BtnInitializeDevice", "I" }
        };

        foreach (var (buttonName, expectedKey) in expectedAccessKeys)
        {
            var button = _xamlDocument!.Descendants(ns + "Button")
                .FirstOrDefault(b => b.Attribute(xNamespace + "Name")?.Value == buttonName);

            Assert.IsNotNull(button, $"Button {buttonName} not found");
            var accessKey = button!.Attribute("AccessKey");
            Assert.IsNotNull(accessKey, $"{buttonName} missing AccessKey attribute");
            Assert.AreEqual(expectedKey, accessKey.Value, $"{buttonName} should have AccessKey={expectedKey}");
        }
    }

    [TestMethod]
    public void MainPage_AllInteractiveControls_HaveAutomationId()
    {
        Assert.IsNotNull(_xamlDocument);
        var ns = XNamespace.Get("http://schemas.microsoft.com/winfx/2006/xaml/presentation");
        var xNamespace = XNamespace.Get("http://schemas.microsoft.com/winfx/2006/xaml");
        var autoNamespace = XNamespace.Get("using:Microsoft.UI.Xaml.Automation");

        var interactiveControls = _xamlDocument!
            .Descendants()
            .Where(e => (e.Name.LocalName == "Button" || e.Name.LocalName == "ComboBox") &&
                       e.Attribute(xNamespace + "Name") != null)
            .ToList();

        Assert.IsTrue(interactiveControls.Count > 0, "No interactive controls found");

        foreach (var control in interactiveControls)
        {
            var automationId = control.Attributes()
                .FirstOrDefault(a => a.Name.LocalName == "AutomationProperties.AutomationId");

            Assert.IsNotNull(automationId,
                $"Control {control.Attribute(xNamespace + "Name")?.Value} missing AutomationProperties.AutomationId");
        }
    }

    [TestMethod]
    public void MainPage_AllInteractiveControls_HaveAutomationName()
    {
        Assert.IsNotNull(_xamlDocument);
        var ns = XNamespace.Get("http://schemas.microsoft.com/winfx/2006/xaml/presentation");
        var xNamespace = XNamespace.Get("http://schemas.microsoft.com/winfx/2006/xaml");

        var interactiveControls = _xamlDocument!
            .Descendants()
            .Where(e => (e.Name.LocalName == "Button" || e.Name.LocalName == "ComboBox") &&
                       e.Attribute(xNamespace + "Name") != null)
            .ToList();

        Assert.IsTrue(interactiveControls.Count > 0, "No interactive controls found");

        foreach (var control in interactiveControls)
        {
            var automationName = control.Attributes()
                .FirstOrDefault(a => a.Name.LocalName == "AutomationProperties.Name");

            Assert.IsNotNull(automationName,
                $"Control {control.Attribute(xNamespace + "Name")?.Value} missing AutomationProperties.Name");
        }
    }

    [TestMethod]
    public void MainPage_AutomationIds_AreUnique()
    {
        Assert.IsNotNull(_xamlDocument);
        var ns = XNamespace.Get("http://schemas.microsoft.com/winfx/2006/xaml/presentation");
        var xNamespace = XNamespace.Get("http://schemas.microsoft.com/winfx/2006/xaml");

        var automationIds = _xamlDocument!
            .Descendants()
            .Select(e => e.Attributes().FirstOrDefault(a => a.Name.LocalName == "AutomationProperties.AutomationId"))
            .Where(attr => attr != null)
            .Select(attr => attr!.Value)
            .ToList();

        var uniqueIds = new HashSet<string>(automationIds);
        Assert.AreEqual(automationIds.Count, uniqueIds.Count,
            "Duplicate AutomationId values found. All IDs must be unique.");
    }

    [TestMethod]
    public void MainPage_SpecificControlProperties_AreCorrect()
    {
        Assert.IsNotNull(_xamlDocument);
        var ns = XNamespace.Get("http://schemas.microsoft.com/winfx/2006/xaml/presentation");
        var xNamespace = XNamespace.Get("http://schemas.microsoft.com/winfx/2006/xaml");

        var expectedProperties = new Dictionary<string, (string TabIndex, string AccessKey, string AutomationId)>
        {
            {
                "CbCameraList",
                ("0", "", "CameraListAutomationId")
            },
            {
                "BtnInitializeDevice",
                ("1", "I", "InitializeDeviceButtonAutomationId")
            },
            {
                "BtnPreview",
                ("2", "V", "PreviewButtonAutomationId")
            },
            {
                "BtnRecord",
                ("3", "R", "RecordButtonAutomationId")
            },
            {
                "BtnPlay",
                ("4", "P", "PlayButtonAutomationId")
            },
            {
                "BtnDelete",
                ("5", "D", "DeleteRecordingButtonAutomationId")
            }
        };

        foreach (var (controlName, (expectedTabIndex, expectedAccessKey, expectedAutomationId)) in expectedProperties)
        {
            var control = _xamlDocument!
                .Descendants()
                .FirstOrDefault(e => e.Attribute(xNamespace + "Name")?.Value == controlName);

            Assert.IsNotNull(control, $"Control {controlName} not found");

            var tabIndex = control!.Attribute("TabIndex")?.Value;
            Assert.AreEqual(expectedTabIndex, tabIndex, $"{controlName} TabIndex mismatch");

            if (!string.IsNullOrEmpty(expectedAccessKey))
            {
                var accessKey = control.Attribute("AccessKey")?.Value;
                Assert.AreEqual(expectedAccessKey, accessKey, $"{controlName} AccessKey mismatch");
            }

            var automationId = control.Attributes()
                .FirstOrDefault(a => a.Name.LocalName == "AutomationProperties.AutomationId")?.Value;
            Assert.AreEqual(expectedAutomationId, automationId, $"{controlName} AutomationId mismatch");
        }
    }
}
