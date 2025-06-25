//#define AUTO_TEXTURE_COMPRESSION

#if AUTO_TEXTURE_COMPRESSION && UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
using System.IO;
using Sirenix.OdinInspector;
using Object = UnityEngine.Object;

namespace CFramework
{
    public class TextureCompressionWindow : EditorWindow
    {
        [MenuItem("Tools/Texture Compression Tool")]
        public static void ShowWindow()
        {
            GetWindow<TextureCompressionWindow>("Texture Compression Tool");
        }

        private void OnGUI()
        {
            GUILayout.Label("Texture Compression Tool", EditorStyles.boldLabel);
            
            if (GUILayout.Button("Select Folder and Compress Textures"))
            {
                SelectFolderAndCompressTextures();
            }
        }

        private void SelectFolderAndCompressTextures()
        {
            string folderPath = EditorUtility.OpenFolderPanel("Select Folder with Textures", "", "");
            if (string.IsNullOrEmpty(folderPath))
            {
                Debug.LogWarning("No folder selected.");
                return;
            }

            string[] files = Directory.GetFiles(folderPath, "*.*", SearchOption.AllDirectories);
            foreach (string file in files)
            {
                if (Path.GetExtension(file).ToLower() == ".png" || Path.GetExtension(file).ToLower() == ".jpg")
                {
                    CompressTexture(file);
                }
            }

            AssetDatabase.Refresh();
            Debug.Log("Texture compression completed for folder: " + folderPath);
        }

        private void CompressTexture(string texturePath)
        {
            string assetPath = "Assets" + texturePath.Replace(Application.dataPath, "").Replace('\\', '/');
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;

            if (importer == null)
            {
                Debug.LogError("Failed to load TextureImporter for: " + assetPath);
                return;
            }

            // Thiết lập cấu hình cho quá trình nén texture
            CustomTexturePostprocessor.ProcessTexture(importer);
        }
    }
    
    public class CustomTexturePostprocessor : AssetPostprocessor
    {
        private static int[] sizePatterns = new[]
        {
            32,
            64,
            128,
            256,
            512,
            1024,
            2048,
        };
        
        public static void ProcessTexture(TextureImporter importer)
        {
            // Kiểm tra nếu texture là một phần của Sprite Atlas
            if (importer.textureType == TextureImporterType.Sprite && !string.IsNullOrEmpty(importer.spritePackingTag))
            {
                Debug.Log("Skipping texture because it is part of a Sprite Atlas: " + importer.assetPath);
                return; // Không thực hiện thay đổi đối với texture thuộc Sprite Atlas
            }

            // Đọc file để lấy kích thước thực tế của texture
            string path = importer.assetPath;
            Texture2D tempTexture = new Texture2D(2, 2);
            byte[] fileData = File.ReadAllBytes(path);
            tempTexture.LoadImage(fileData);
            int maxDimension = Mathf.Max(tempTexture.width, tempTexture.height);

            // Tính toán kích thước đúng
            int correctSize = FindSizePattern(maxDimension);

            // Áp dụng cài đặt cho Android
            TextureImporterPlatformSettings androidSettings = new TextureImporterPlatformSettings
            {
                name = "Android",
                overridden = true,
                maxTextureSize = correctSize,
                format = GetAndroidIOSTextureFormat(correctSize),
            };
            importer.SetPlatformTextureSettings(androidSettings);

            // Áp dụng cài đặt cho iOS
            TextureImporterPlatformSettings iOSSettings = new TextureImporterPlatformSettings
            {
                name = "iPhone",
                overridden = true,
                maxTextureSize = correctSize,
                format = GetAndroidIOSTextureFormat(correctSize),
            };
            importer.SetPlatformTextureSettings(iOSSettings);

            // Sao chép cài đặt mặc định hiện tại và chỉ thay đổi maxTextureSize
            TextureImporterPlatformSettings defaultSettings = importer.GetDefaultPlatformTextureSettings();
            defaultSettings.maxTextureSize = correctSize;
            defaultSettings.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SetPlatformTextureSettings(defaultSettings);

            // Hủy texture tạm thời
            Object.DestroyImmediate(tempTexture);
        }
        
        private static int FindSizePattern(int size)
        {
            for (int i = 0; i < sizePatterns.Length; i++)
            {
                if (sizePatterns[i] >= size) return sizePatterns[i];
            }

            return 2048;
        }

        private static TextureImporterFormat GetAndroidIOSTextureFormat(int size)
        {
            switch (size)
            {
                case 32:
                case 64:
                case 128:
                case 256:
                    return TextureImporterFormat.ASTC_4x4;
                case 512:
                    return TextureImporterFormat.ASTC_6x6;
                case 1024:
                    return TextureImporterFormat.ASTC_8x8;
                case 2048:
                    return TextureImporterFormat.ASTC_12x12;
                default:
                    return TextureImporterFormat.Automatic;
            }
        }

        void OnPreprocessTexture()
        {
            TextureImporter importer = (TextureImporter)assetImporter;

            // Kiểm tra nếu texture là một phần của Sprite Atlas
            if (importer.textureType == TextureImporterType.Sprite && !string.IsNullOrEmpty(importer.spritePackingTag))
            {
                Debug.Log("Skipping texture because it is part of a Sprite Atlas: " + assetPath);
                return; // Không thực hiện thay đổi đối với texture thuộc Sprite Atlas
            }

            // Kiểm tra xem texture có mới được tạo hay không
            string path = importer.assetPath;
            FileInfo fileInfo = new FileInfo(path);
            if (fileInfo.CreationTime < DateTime.Now.AddSeconds(-1)) // Điều chỉnh thời gian nếu cần
            {
                Debug.Log("Skipping old texture: " + assetPath);
                return; // Không thực hiện thay đổi đối với texture cũ
            }

            // Đọc file để lấy kích thước thực tế của texture
            Texture2D tempTexture = new Texture2D(2, 2);
            byte[] fileData = File.ReadAllBytes(path);
            tempTexture.LoadImage(fileData);
            int maxDimension = Mathf.Max(tempTexture.width, tempTexture.height);

            // Tính toán kích thước đúng
            int correctSize = FindSizePattern(maxDimension);

            // Áp dụng cài đặt cho Android
            TextureImporterPlatformSettings androidSettings = new TextureImporterPlatformSettings
            {
                name = "Android",
                overridden = true,
                maxTextureSize = correctSize,
                format = GetAndroidIOSTextureFormat(correctSize),
            };
            importer.SetPlatformTextureSettings(androidSettings);

            // Áp dụng cài đặt cho iOS
            TextureImporterPlatformSettings iOSSettings = new TextureImporterPlatformSettings
            {
                name = "iPhone",
                overridden = true,
                maxTextureSize = correctSize,
                format = GetAndroidIOSTextureFormat(correctSize),
            };
            importer.SetPlatformTextureSettings(iOSSettings);

            // Sao chép cài đặt mặc định hiện tại và chỉ thay đổi maxTextureSize
            TextureImporterPlatformSettings defaultSettings = importer.GetDefaultPlatformTextureSettings();
            defaultSettings.maxTextureSize = correctSize;
            defaultSettings.textureCompression = TextureImporterCompression.Uncompressed;
            importer.SetPlatformTextureSettings(defaultSettings);

            // Hủy texture tạm thời
            Object.DestroyImmediate(tempTexture);
        }

        void OnPostprocessTexture(Texture2D texture)
        {
            Debug.Log("Texture imported: " + assetPath);
        }
    }
}
#endif
