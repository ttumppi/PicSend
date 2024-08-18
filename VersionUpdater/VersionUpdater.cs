using System;
using System.Windows;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace VersionUpdater
{
    public static class VersionUpdater
    {
        public static string CurrentVersion { get; private set; } = string.Empty;
        private static object _fileLock = new object();

        static  VersionUpdater()
        {
            SetCurrentVersion();
        }
        public static void Update()
        {
            try
            {
                lock (_fileLock)
                {
                    string ProjectDataFolderPath = GetProjectDataFolderPath();

                    if (ProjectDataFolderPath == string.Empty)
                    {
                        throw new Exception("Could not find Project Data folder");
                    }

                    string versionFilePath = Path.Combine(ProjectDataFolderPath, "Version.json");

                    string fileContent = string.Empty;

                    FileStream? readStream = null;
                    StreamReader? streamReader = null;
                    try
                    {
                        readStream = new FileStream(versionFilePath, FileMode.Open);
                        streamReader = new StreamReader(readStream);

                        fileContent = streamReader.ReadToEnd();
                        readStream.Dispose();
                        streamReader.Dispose();
                    }
                    catch (Exception e)
                    {
                        throw new Exception($"Failed to open Version file for read : {e.Message}");
                    }

                    finally
                    {
                        readStream?.Close();
                        streamReader?.Close();
                    }
                    
                    

                    if (fileContent == string.Empty)
                    {

                    }


                    ProjectData? projectData = JsonSerializer.Deserialize<ProjectData>(fileContent);





                    if (projectData is null)
                    {
                        throw new Exception("Project Data is null");
                    }

                    projectData.IncrementVersion();

                    CurrentVersion = projectData.ProjectVersion;

                    using (StreamWriter writer = new StreamWriter(versionFilePath))
                    {
                        string updatedVersionData = JsonSerializer.Serialize(projectData);

                        writer.Write(updatedVersionData);
                    }
                }
                
            }
            catch(Exception ex)
            {
                throw new Exception($"Failed to update build version : {ex.Message}"); 
            }
        }

        private static void SetCurrentVersion()
        {
            string ProjectDataFolderPath = GetProjectDataFolderPath();

            if (ProjectDataFolderPath == string.Empty)
            {
                return;
            }
            string VersionFilePath = Path.Combine(ProjectDataFolderPath, "Version.json");

            string fileContent = File.ReadAllText(VersionFilePath);


            ProjectData? projectData = JsonSerializer.Deserialize<ProjectData>(fileContent);

            if (projectData is null)
            {
                return;
            }

            CurrentVersion = projectData.ProjectVersion;
        }

        private static string GetProjectDataFolderPath()
        {
            string traversedPaths = string.Empty;
            foreach (string folder in AppDomain.CurrentDomain.BaseDirectory.Split(['\\']))
            {

                string testPath = traversedPaths + "ProjectData";

                if (Directory.Exists(testPath))
                {
                    return testPath;
                }

                traversedPaths += folder + @"\";
            }
            return string.Empty;
        }
    }


    public class ProjectData
    {
        [JsonInclude]
        public string ProjectVersion { get; private set; } = "00.00.01";


        public void IncrementVersion()
        {
            Version? version = Version.CreateFromString(ProjectVersion);

            if (version is null)
            {
                throw new Exception("Failed to update version");
            }


            version.Value.Increment();

            ProjectVersion = version.Value.ToString();

        }
    }

    public struct Version
    {
        public VersionPart[] VersionNumber { get; private set; }

        public Version()
        {
            VersionNumber = [
                new VersionPart(),
                new VersionPart(),
                new VersionPart(),
                ];
            VersionNumber[2].Increment();

        }

        public override string ToString()
        {
            string version = string.Empty;

            int currentIndex = 0;
            foreach (VersionPart part in VersionNumber)
            {
                if (currentIndex != VersionNumber.Length -1)
                {
                    version += part.ToString() + ".";
                }
                else
                {
                    version += part.ToString();
                }
                currentIndex++;
            }

            return version;
        }

        public static Version? CreateFromString(string version)
        {
            string[] numbers = version.Split('.');

            VersionPart[] versionParts = new VersionPart[numbers.Length];

            int currentIndex = 0;
            foreach (string number in numbers)
            {
                

                VersionPart? part = VersionPart.CreateFromString(number);

                if (part is null)
                {
                    return null;
                }

                versionParts[currentIndex] = (VersionPart)part;
                currentIndex++;
            }

            return new Version() { VersionNumber = versionParts };
        }

        public void Increment()
        {
           for (int i = VersionNumber.Length -1; i > -1; i--)
           {
                VersionNumber[i].Increment();
                if (VersionPartReset(VersionNumber[i]))
                {
                    continue;
                }
                break;
           }
        }

        private void IncrementNextVersion(int versionPartIndex)
        {
            if (versionPartIndex > VersionNumber.Length - 1)
            {
                return;
            }
            VersionNumber[versionPartIndex].Increment();
        }

        private bool VersionPartReset(VersionPart versionPart)
        {
            return versionPart.Second == 0 && versionPart.First == 0;
        }
    }

    public struct VersionPart
    {
        public ushort First { get; private set; } = 0;
        public ushort Second { get; private set; } = 0;

        public VersionPart() { }

        public void Increment()
        {
            Second++;

            if (Second > 9)
            {
                Second = 0;
                First++;
            }

            if (First > 9)
            {
                Second = 0;
                First = 0;
            }
        }

        public override string ToString()
        {
            return First.ToString() + Second.ToString();
        }

        public static VersionPart? CreateFromString(string versionPart)
        {
            if (versionPart.Length != 2)
            {
                return null;
            }

            ushort first = ushort.Parse(versionPart[0].ToString());
            ushort second = ushort.Parse(versionPart[1].ToString());

            return new VersionPart() { First = first, Second = second };
        }
    }

} 