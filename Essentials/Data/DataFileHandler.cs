using System;
using System.IO;
using CodeSketch.Diagnostics;
using UnityEngine;

namespace CodeSketch.Data
{
    public static class DataFileHandler
    {
        const string RootFolderName = "CodeSketch.Data";

        static string GetDevicePath(string filePath)
        {
            return Path.Combine(Application.persistentDataPath, RootFolderName, filePath);
        }

        static string GetProjectPath(string filePath)
        {
            return Path.Combine(Application.dataPath, filePath);
        }

        static void Save<T>(T data, string filePath) where T : class
        {
            try
            {
                byte[] bytes = DataSerializer.Serialize<T>(data);

                // Create folder if needed
                {
                    string path = Path.GetDirectoryName(filePath);

                    if (!Directory.Exists(path) && path != null)
                            Directory.CreateDirectory(path);
                }

                File.WriteAllBytes(filePath, bytes);
            }
            catch (Exception e)
            {
                CodeSketchDebug.Log(typeof(DataFileHandler), $"Save failed: {e}");
            }
        }

        static T Load<T>(string filePath) where T : class
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    CodeSketchDebug.Log(typeof(DataFileHandler),
                        $"Can't load, file {filePath} does not exist! creating new file");
                    return null;
                }

                byte[] bytes = File.ReadAllBytes(filePath);

                return DataSerializer.Deserialize<T>(bytes);
            }
            catch (Exception e)
            {
                CodeSketchDebug.Log(typeof(DataFileHandler), $"Load failed: {e}");

                return null;
            }
        }

        static void Delete(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    CodeSketchDebug.Log(typeof(DataFileHandler), $"Can't delete, file {filePath} does not exist!");
                    return;
                }

                File.Delete(filePath);
            }
            catch (Exception e)
            {
                CodeSketchDebug.Log(typeof(DataFileHandler), $"Delete failed: {e}");
            }
        }

        public static T Load<T>(TextAsset textAsset) where T : class
        {
            try
            {
                return DataSerializer.Deserialize<T>(textAsset.bytes);
            }
            catch (Exception e)
            {
                CodeSketchDebug.Log(typeof(DataFileHandler), $"Load failed: {e}");

                return null;
            }
        }

        public static void SaveToDevice<T>(T data, string filePath) where T : class
        {
            Save(data, GetDevicePath(filePath));
        }

        public static void SaveToProject<T>(T data, string filePath) where T : class
        {
            Save(data, GetProjectPath(filePath));
        }

        public static T LoadFromDevice<T>(string filePath) where T : class
        {
            return Load<T>(GetDevicePath(filePath));
        }

        public static T LoadFromProject<T>(string filePath) where T : class
        {
            return Load<T>(GetProjectPath(filePath));
        }

        public static void DeleteInDevice(string filePath)
        {
            Delete(GetDevicePath(filePath));
        }

        public static void DeleteInProject(string filePath)
        {
            Delete(GetProjectPath(filePath));
        }

        public static void DeleteAllInDevice()
        {
            string path = Path.Combine(Application.persistentDataPath, RootFolderName);

            var info = new DirectoryInfo(path);

            if (!info.Exists)
                return;

            var files = info.GetFiles();

            for (int i = 0; i < files.Length; i++)
            {
                files[i].Delete();
            }
        }
    }
}