#nullable disable

using System;
using System.Collections.Generic;
using System.Management;
using System.Threading;
using Microsoft.Win32;

namespace TubaToolbox.Services
{
    public class HardwareInfo
    {
        public string ComputerModel { get; set; } = "无法获取";
        public string SystemInfo { get; set; } = "无法获取";
        public string Uptime { get; set; } = "无法获取";
        public string Processor { get; set; } = "无法获取";
        public string Motherboard { get; set; } = "无法获取";
        public string Memory { get; set; } = "无法获取";
        public string Graphics { get; set; } = "无法获取";
        public string Monitor { get; set; } = "无法获取";
        public string Disk { get; set; } = "无法获取";
        public string SoundCard { get; set; } = "无法获取";
        public string NetworkAdapter { get; set; } = "无法获取";
    }

    public static class HardwareInfoService
    {
        public static HardwareInfo GetHardwareInfo(CancellationToken token)
        {
            return new HardwareInfo
            {
                ComputerModel = GetComputerModel(token),
                SystemInfo = GetSystemInfo(token),
                Uptime = GetUptime(token),
                Processor = GetProcessorInfo(token),
                Motherboard = GetMotherboardInfo(token),
                Memory = GetMemoryInfo(token),
                Graphics = GetGraphicsInfo(token),
                Monitor = GetMonitorInfo(token),
                Disk = GetDiskInfo(token),
                SoundCard = GetSoundCardInfo(token),
                NetworkAdapter = GetNetworkAdapterInfo(token)
            };
        }

        private static T QueryWmiSingle<T>(string className, Func<ManagementObject, T> selector, CancellationToken token)
        {
            try
            {
                using (var mc = new ManagementClass(className))
                using (var instances = mc.GetInstances())
                {
                    foreach (ManagementObject mo in instances)
                    {
                        token.ThrowIfCancellationRequested();
                        using (mo)
                        {
                            return selector(mo);
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch
            {
                return default;
            }
            return default;
        }

        private static List<T> QueryWmiMultiple<T>(string className, string condition, Func<ManagementObject, T> selector, CancellationToken token)
        {
            var results = new List<T>();
            try
            {
                var query = string.IsNullOrEmpty(condition) ? $"SELECT * FROM {className}" : $"SELECT * FROM {className} WHERE {condition}";
                using (var searcher = new ManagementObjectSearcher(query))
                using (var instances = searcher.Get())
                {
                    foreach (ManagementObject mo in instances)
                    {
                        token.ThrowIfCancellationRequested();
                        using (mo)
                        {
                            var result = selector(mo);
                            if (result != null)
                            {
                                results.Add(result);
                            }
                        }
                    }
                }
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch { }
            return results;
        }

        private static string GetComputerModel(CancellationToken token)
        {
            var result = QueryWmiSingle("Win32_ComputerSystem", mo => 
                $"{mo["Manufacturer"]} {mo["Model"]}".Trim(), token);
            return string.IsNullOrEmpty(result) ? "无法获取" : result;
        }

        private static string GetSystemInfo(CancellationToken token)
        {
            try
            {
                token.ThrowIfCancellationRequested();
                string arch = Environment.Is64BitOperatingSystem ? "64位" : "32位";
                
                // 从注册表获取 Windows 版本
                using (var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion"))
                {
                    if (key != null)
                    {
                        var productName = key.GetValue("ProductName")?.ToString();
                        var major = key.GetValue("CurrentMajorVersionNumber")?.ToString();
                        var minor = key.GetValue("CurrentMinorVersionNumber")?.ToString();
                        var build = key.GetValue("CurrentBuild")?.ToString();
                        var ubr = key.GetValue("UBR")?.ToString();
                        
                        // 构建完整版本号
                        string version = "";
                        if (!string.IsNullOrEmpty(major))
                        {
                            version = $"{major}.{minor ?? "0"}.{build ?? "0"}";
                            if (!string.IsNullOrEmpty(ubr)) version += $".{ubr}";
                        }
                        
                        // 简化系统名称
                        string osName = "Windows";
                        if (!string.IsNullOrEmpty(productName))
                        {
                            osName = productName.Replace("Microsoft ", "");
                        }
                        
                        // 根据 Build 号区分 Windows 10/11
                        if (!string.IsNullOrEmpty(build) && int.TryParse(build, out int buildNum))
                        {
                            if (buildNum >= 22000)
                            {
                                osName = "Windows 11";
                            }
                            else if (buildNum >= 10240)
                            {
                                osName = "Windows 10";
                            }
                        }
                        
                        return $"{osName} {arch} (版本 {version})";
                    }
                }
                
                return $"Windows {arch}";
            }
            catch
            {
                return "无法获取";
            }
        }

        private static string GetUptime(CancellationToken token)
        {
            try
            {
                token.ThrowIfCancellationRequested();
                
                using (var query = new ManagementObjectSearcher("SELECT LastBootUpTime FROM Win32_OperatingSystem"))
                using (var instances = query.Get())
                {
                    foreach (ManagementObject mo in instances)
                    {
                        using (mo)
                        {
                            var lastBoot = ManagementDateTimeConverter.ToDateTime(mo["LastBootUpTime"].ToString());
                            var uptime = DateTime.Now - lastBoot;
                            return $"{(int)uptime.TotalDays}天 {uptime.Hours}小时 {uptime.Minutes}分钟";
                        }
                    }
                }
                return "无法获取";
            }
            catch
            {
                return "无法获取";
            }
        }

        private static string GetProcessorInfo(CancellationToken token)
        {
            var result = QueryWmiSingle("Win32_Processor", mo => 
                $"{mo["Name"]} ({mo["NumberOfCores"]}核{mo["NumberOfLogicalProcessors"]}线程)", token);
            return string.IsNullOrEmpty(result) ? "无法获取" : result;
        }

        private static string GetMotherboardInfo(CancellationToken token)
        {
            var result = QueryWmiSingle("Win32_BaseBoard", mo => 
                $"{mo["Manufacturer"]} {mo["Product"]}".Trim(), token);
            return string.IsNullOrEmpty(result) ? "无法获取" : result;
        }

        private static string GetMemoryInfo(CancellationToken token)
        {
            try
            {
                long totalBytes = 0;
                var modules = new List<string>();
                
                using (var mc = new ManagementClass("Win32_PhysicalMemory"))
                using (var instances = mc.GetInstances())
                {
                    foreach (ManagementObject mo in instances)
                    {
                        token.ThrowIfCancellationRequested();
                        using (mo)
                        {
                            var capacity = Convert.ToInt64(mo["Capacity"]);
                            totalBytes += capacity;
                            double gb = capacity / (1024.0 * 1024.0 * 1024.0);
                            modules.Add($"{gb:F0}GB");
                        }
                    }
                }
                
                double totalGB = totalBytes / (1024.0 * 1024.0 * 1024.0);
                return modules.Count > 0 ? $"{totalGB:F1}GB ({string.Join(" + ", modules)})" : $"{totalGB:F1}GB";
            }
            catch
            {
                return "无法获取";
            }
        }

        private static string GetGraphicsInfo(CancellationToken token)
        {
            var cards = QueryWmiMultiple("Win32_VideoController", null, mo => 
                mo["Name"]?.ToString(), token);
            return cards.Count > 0 ? string.Join(" / ", cards) : "无法获取";
        }

        private static string GetMonitorInfo(CancellationToken token)
        {
            var monitors = QueryWmiMultiple("Win32_DesktopMonitor", null, mo => 
                mo["Name"]?.ToString(), token);
            return monitors.Count > 0 ? string.Join(" / ", monitors) : "无法获取";
        }

        private static string GetDiskInfo(CancellationToken token)
        {
            var disks = QueryWmiMultiple("Win32_DiskDrive", null, mo => 
            {
                var model = mo["Model"]?.ToString();
                if (!string.IsNullOrEmpty(model))
                {
                    long sizeBytes = Convert.ToInt64(mo["Size"]);
                    double sizeGB = sizeBytes / (1024.0 * 1024.0 * 1024.0);
                    return $"{model.Trim()} ({sizeGB:F0}GB)";
                }
                return null;
            }, token);
            
            return disks.Count > 0 ? string.Join(" | ", disks) : "无法获取";
        }

        private static string GetSoundCardInfo(CancellationToken token)
        {
            var devices = QueryWmiMultiple("Win32_SoundDevice", null, mo => 
                mo["Name"]?.ToString(), token);
            
            var uniqueDevices = new HashSet<string>();
            foreach (var device in devices)
            {
                if (!string.IsNullOrEmpty(device))
                    uniqueDevices.Add(device);
            }
            
            return uniqueDevices.Count > 0 ? string.Join(" / ", uniqueDevices) : "无法获取";
        }

        private static string GetNetworkAdapterInfo(CancellationToken token)
        {
            var adapters = QueryWmiMultiple("Win32_NetworkAdapter", "NetConnectionID IS NOT NULL", mo => 
            {
                var name = mo["Name"]?.ToString();
                if (!string.IsNullOrEmpty(name) && 
                    !name.Contains("Virtual") && 
                    !name.Contains("vEthernet") &&
                    !name.Contains("Bluetooth"))
                {
                    return name;
                }
                return null;
            }, token);
            
            return adapters.Count > 0 ? string.Join(" / ", adapters) : "无法获取";
        }
    }
}