using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

namespace SPH3
{
    /// <summary>
    /// Unity Editor only utilities
    /// </summary>
    public static class UtilEditor
    {

        #region I/O
        /// <summary>
        /// Retrieves selected folder on Project view.
        /// </summary>
        /// <returns></returns>
        public static string GetSelectedPathOrFallback()
        {
            string path = "Assets";

            foreach (UnityEngine.Object obj in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets))
            {
                path = AssetDatabase.GetAssetPath(obj);
                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                {
                    path = Path.GetDirectoryName(path);
                    break;
                }
            }
            return path;
        }

        /// <summary>
        /// Recursively gather all files under the given path including all its subfolders.
        /// </summary>
        public static IEnumerable<string> GetFiles(string path)
        {
            Queue<string> queue = new Queue<string>();
            queue.Enqueue(path);
            while (queue.Count > 0)
            {
                path = queue.Dequeue();
                try
                {
                    foreach (string subDir in Directory.GetDirectories(path))
                    {
                        queue.Enqueue(subDir);
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError(ex.Message);
                }
                string[] files = null;
                try
                {
                    files = Directory.GetFiles(path);
                }
                catch (System.Exception ex)
                {
                    Debug.LogError(ex.Message);
                }
                if (files != null)
                {
                    for (int i = 0; i < files.Length; i++)
                    {
                        yield return files[i];
                    }
                }
            }
        }
        #endregion

    }
}
