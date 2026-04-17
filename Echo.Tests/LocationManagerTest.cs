using System.IO;
using Echo.Core;
using UnityEngine;

namespace Echo.Tests;

public class LocationManagerTest
{
    private string _filePath = null!;
    private SavedLocationManager _mgr = null!;

    [SetUp]
    public void SetUp()
    {
        _filePath = Path.GetTempFileName();
        _mgr = new SavedLocationManager(_filePath);
    }

    [TearDown]
    public void TearDown()
    {
        if (File.Exists(_filePath)) File.Delete(_filePath);
    }

    [Test]
    public void TryGet_UnknownName_ReturnsFalse()
    {
        Assert.That(_mgr.TryGet("nowhere", out _), Is.False);
    }

    [Test]
    public void Save_ThenGet_ReturnsSameValue()
    {
        var pos = new Vector3(1f, 2f, 3f);
        _mgr.Save("home", pos);

        Assert.That(_mgr.TryGet("home", out var result), Is.True);
        Assert.That(result.x, Is.EqualTo(1f));
        Assert.That(result.y, Is.EqualTo(2f));
        Assert.That(result.z, Is.EqualTo(3f));
    }

    [Test]
    public void Save_Overwrites_PreviousValue()
    {
        _mgr.Save("home", new Vector3(1f, 2f, 3f));
        _mgr.Save("home", new Vector3(9f, 8f, 7f));

        Assert.That(_mgr.TryGet("home", out var result), Is.True);
        Assert.That(result.x, Is.EqualTo(9f));
    }

    [Test]
    public void TryGet_IsCaseInsensitive()
    {
        _mgr.Save("Home", new Vector3(5f, 0f, 0f));

        Assert.That(_mgr.TryGet("HOME", out _), Is.True);
        Assert.That(_mgr.TryGet("home", out _), Is.True);
    }

    [Test]
    public void Delete_ExistingName_ReturnsTrueAndRemoves()
    {
        _mgr.Save("camp", new Vector3(1f, 1f, 1f));

        Assert.That(_mgr.Delete("camp"), Is.True);
        Assert.That(_mgr.TryGet("camp", out _), Is.False);
    }

    [Test]
    public void Delete_UnknownName_ReturnsFalse()
    {
        Assert.That(_mgr.Delete("nowhere"), Is.False);
    }

    [Test]
    public void Persistence_ReloadsFromDisk()
    {
        _mgr.Save("spawn", new Vector3(10f, 20f, 30f));

        // New instance from same file - simulates restart
        var mgr2 = new SavedLocationManager(_filePath);

        Assert.That(mgr2.TryGet("spawn", out var result), Is.True);
        Assert.That(result.x, Is.EqualTo(10f));
        Assert.That(result.y, Is.EqualTo(20f));
        Assert.That(result.z, Is.EqualTo(30f));
    }

    [Test]
    public void Persistence_Delete_IsReflectedOnDisk()
    {
        _mgr.Save("camp", new Vector3(1f, 2f, 3f));
        _mgr.Delete("camp");

        var mgr2 = new SavedLocationManager(_filePath);
        Assert.That(mgr2.TryGet("camp", out _), Is.False);
    }

    [Test]
    public void Persistence_MultipleLocations_AllReloaded()
    {
        _mgr.Save("a", new Vector3(1f, 0f, 0f));
        _mgr.Save("b", new Vector3(2f, 0f, 0f));
        _mgr.Save("c", new Vector3(3f, 0f, 0f));

        var mgr2 = new SavedLocationManager(_filePath);

        Assert.That(mgr2.TryGet("a", out var a), Is.True);
        Assert.That(mgr2.TryGet("b", out var b), Is.True);
        Assert.That(mgr2.TryGet("c", out var c), Is.True);
        Assert.That(a.x, Is.EqualTo(1f));
        Assert.That(b.x, Is.EqualTo(2f));
        Assert.That(c.x, Is.EqualTo(3f));
    }

    [Test]
    public void NewInstance_EmptyFile_NoLocations()
    {
        // File exists but is empty
        File.WriteAllText(_filePath, "");
        var mgr = new SavedLocationManager(_filePath);
        Assert.That(mgr.TryGet("anything", out _), Is.False);
    }
}
