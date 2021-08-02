﻿using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using Utilities.Importer;

namespace Importer
{
    public static class ImporterUtility
    {
        private static string NAME = "[ImporterUtility]: ";
        private static readonly ImportSettings Settings = ImportSettings.Instance;

        public static void Import(string path)
        {
            // fbx import
            string pattern = Settings.regex;
            Debug.Log(pattern);
            Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);
            string commonPath = regex.Replace(path, "");
            string filename = path.Split('\\').Last().Split('.').First().Split('_')[1];
            string currentDir = Environment.CurrentDirectory + "\\Assets\\";

            if (!AssetDatabase.IsValidFolder(Settings.parentFolder + filename))
            {
                AssetDatabase.CreateFolder(Settings.parentFolder.Remove(Settings.parentFolder.Length - 1),
                    filename); // -1 because of redundant "/" at the end
            }

            File.Copy(path, $"{currentDir}Models\\{filename}\\mesh_{filename.Split('_').First()}.fbx");
            Debug.Log($"{NAME}fbx imported to folder {Settings.parentFolder}{filename}\\");
            string currentModelPath = $"{Settings.parentFolder}{filename}\\mesh_{filename}.fbx";
            AssetDatabase.ImportAsset(currentModelPath);

            // materials import
            var importedModels = AssetDatabase.LoadAllAssetsAtPath(currentModelPath)
                .Where(x => x.GetType() == typeof(Material));

            if (!AssetDatabase.IsValidFolder(Settings.parentFolder + filename + "/Materials/"))
            {
                AssetDatabase.CreateFolder(Settings.parentFolder + filename, "Materials");
            }

            foreach (var model in importedModels)
            {
                var message = AssetDatabase.ExtractAsset(model,
                    Settings.parentFolder + filename + @"\Materials\" + model.name + ".mat");
                if (!string.IsNullOrEmpty(message))
                {
                    Debug.Log(NAME + message);
                }
            }

            Material material =
                (Material) AssetDatabase.LoadMainAssetAtPath(
                    $"{Settings.parentFolder}{filename}\\Materials\\mat_{filename}.mat");

            // textures import
            string texturePath = commonPath + @"Textures\";
            pattern = @"tex_\w*\.png";
            Regex textureRegex = new Regex(pattern, RegexOptions.IgnoreCase);

            if (!AssetDatabase.IsValidFolder("Assets/Models/" + filename + "/Textures/"))
            {
                AssetDatabase.CreateFolder("Assets/Models/" + filename, "Textures");
            }

            foreach (var sourceTexturePath in Directory.GetFiles(texturePath))
            {
                Match textureName = textureRegex.Match(sourceTexturePath);
                string currentTexturePath = currentDir + "\\Models\\" + filename + "\\Textures\\" + textureName;
                File.Copy(sourceTexturePath, currentTexturePath);
                string projectTexturePath = "Assets\\Models\\" + filename + "\\Textures\\" + textureName;
                AssetDatabase.ImportAsset(projectTexturePath);
                switch (textureName.ToString().Split('_').Last().Split('.').First())
                {
                    case "BC":
                        material.SetTexture("_BaseMap",
                            (Texture) AssetDatabase.LoadMainAssetAtPath(projectTexturePath));
                        break;
                    case "N":
                        material.SetTexture("_BumpMap",
                            (Texture) AssetDatabase.LoadMainAssetAtPath(projectTexturePath));
                        TextureImporter textureImporter =
                            AssetImporter.GetAtPath(projectTexturePath) as TextureImporter;
                        textureImporter.textureType = TextureImporterType.NormalMap;
                        break;
                    case "M":
                        material.SetTexture("_MetallicGlossMap",
                            (Texture) AssetDatabase.LoadMainAssetAtPath(projectTexturePath));
                        break;
                    case "AO":
                        material.SetTexture("_OcclusionMap",
                            (Texture) AssetDatabase.LoadMainAssetAtPath(projectTexturePath));
                        break;
                }
            }

            EditorApplication.ExecuteMenuItem("File/Save Project");
        }

        /// <summary>
        /// Create folder under ImportSettings.parentFolder at path
        /// </summary>
        /// <param name="path"></param>
        private static void CreateFolder(string path)
        {
            if (!AssetDatabase.IsValidFolder(Settings.parentFolder + path))
            {
                Debug.Log("___" + Settings.parentFolder + path);

                AssetDatabase.CreateFolder(Settings.parentFolder.Remove(Settings.parentFolder.Length - 1),
                    path); // -1 because of redundant "/" at the end    
            }
            else Debug.Log(NAME + path + " already exists!");
        }

        public static void CreateObject()
        {
            //create folder
            //file.copy
        }
    }
}