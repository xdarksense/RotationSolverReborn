using Dalamud.Interface.Colors;
using RotationSolver.Data;
using System.Diagnostics;

namespace RotationSolver.Helpers;

internal static class RotationHelper
{
    private static readonly Dictionary<Assembly, AssemblyInfo> _assemblyInfos = [];

    public static List<LoadedAssembly> LoadedCustomRotations { get; } = [];

    public static AssemblyInfo GetInfo(this Assembly assembly)
    {
        if (_assemblyInfos.TryGetValue(assembly, out AssemblyInfo? info))
        {
            return info;
        }

        string? name = assembly.GetName().Name;
        string location = assembly.Location;
        string? company = assembly.GetCustomAttribute<AssemblyCompanyAttribute>()?.Company;
        AssemblyInfo assemblyInfo = new(name, company, location, string.Empty, company, name, DateTime.Now);

        _assemblyInfos[assembly] = assemblyInfo;

        return assemblyInfo;
    }

    public static unsafe Vector4 GetColor(this ICustomRotation rotation)
    {
        if (!rotation.IsEnabled)
        {
            return *ImGui.GetStyleColorVec4(ImGuiCol.TextDisabled);
        }

        if (!rotation.IsValid)
        {
            return ImGuiColors.DPSRed;
        }

        return rotation.IsBeta() ? ImGuiColors.DalamudOrange : ImGuiColors.DalamudWhite;
    }

    public static bool IsBeta(this ICustomRotation rotation)
    {
        BetaRotationAttribute? betaAttribute = rotation.GetType().GetCustomAttribute<BetaRotationAttribute>();
        return betaAttribute != null;
    }

    public static Assembly LoadCustomRotationAssembly(string filePath)
    {
        DirectoryInfo? directoryInfo = new FileInfo(filePath).Directory;
        RotationLoadContext loadContext = new(directoryInfo);
        Assembly assembly = loadContext.LoadFromFile(filePath);

        string? assemblyName = assembly.GetName().Name;
        string author = GetAuthor(filePath, assemblyName);

        AssemblyLinkAttribute? link = assembly.GetCustomAttribute<AssemblyLinkAttribute>();
        AssemblyInfo assemblyInfo = new(
            assemblyName,
            author,
            filePath,
            link?.Donate,
            link?.UserName,
            link?.Repository,
            DateTime.Now);

        Assembly? existingAssembly = GetAssemblyFromPath(filePath);
        if (existingAssembly != null)
        {
            _ = _assemblyInfos.Remove(existingAssembly);
        }

        _assemblyInfos[assembly] = assemblyInfo;

        LoadedAssembly loadedAssembly = new(
            filePath,
            File.GetLastWriteTimeUtc(filePath).ToString());

        _ = LoadedCustomRotations.RemoveAll(item => item.FilePath == loadedAssembly.FilePath);
        LoadedCustomRotations.Add(loadedAssembly);

        return assembly;
    }

    private static Assembly? GetAssemblyFromPath(string filePath)
    {
        foreach (KeyValuePair<Assembly, AssemblyInfo> asm in _assemblyInfos)
        {
            if (asm.Value.FilePath == filePath)
            {
                return asm.Key;
            }
        }
        return null;
    }

    private static string GetAuthor(string filePath, string? assemblyName)
    {
        FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(filePath);
        return string.IsNullOrWhiteSpace(fileVersionInfo.CompanyName) ? assemblyName ?? string.Empty : fileVersionInfo.CompanyName;
    }
}
