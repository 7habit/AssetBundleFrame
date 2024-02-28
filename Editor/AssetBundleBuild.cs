using System.IO;
using UnityEngine;
using UnityEditor;

namespace AssetBundleFrame
{
    public class AssetBundleBuild : Editor
    {      
        [MenuItem("AssetBundle/BuildBundles")]
        public static void AssetBundleAutoBuildMacOs()
        {
            //清空当前所有的Bundle名称
            ClearAssetBundleName();
            //设置资源文件下的文件的Bundle名称
            SetAssetBundlesName(AssetBundleDefine.ResPath);
            //打包
            BeginAssetBundleBuild();
            //清空当前所有的Bundle名称
            ClearAssetBundleName();
            //刷新
            AssetDatabase.Refresh();
        }
        
        /// <summary>
        /// 一键生成所有有AB名称的资源的AB包
        /// </summary>
        private static void BeginAssetBundleBuild()
        {
            //打包
            BuildPipeline.BuildAssetBundles(
                AssetBundleDefine.AbPath, //输出路径
                //压缩选项：None-包小加载慢，ChunkBasedCompression-包中等加载快，UncompressedAssetBundle-不压缩加载快
                BuildAssetBundleOptions.ChunkBasedCompression, 
                EditorUserBuildSettings.activeBuildTarget); //根据BuildSetting中的选项来选择对应的构建平台
        }

        /// <summary>
        /// 清空当前所有的Bundle名称
        /// </summary>
        private static void ClearAssetBundleName()
        {
            //获取所有的Bundle名称
            string[] bundleNames = AssetDatabase.GetAllAssetBundleNames();
            //遍历删除AB名称
            for (int i = 0; i < bundleNames.Length; i++)
            {
                //删除Bundle名称
                AssetDatabase.RemoveAssetBundleName(bundleNames[i], true);
            }
        }

        /// <summary>
        /// 设置资源文件下的文件的Bundle名称
        /// </summary>
        /// <param name="path">需要设置的资源文件夹路径</param>
        private static void SetAssetBundlesName(string path)
        {
            //获取资源文件夹
            DirectoryInfo rootInfo = new DirectoryInfo(path);
            //获取子文件
            FileSystemInfo[] fileInfo = rootInfo.GetFileSystemInfos();
            //遍历子文件
            for (int i = 0; i < fileInfo.Length; i++)
            {
                //如果是文件夹
                if (fileInfo[i] is DirectoryInfo)
                {
                    //递归解析
                    SetAssetBundlesName(fileInfo[i].FullName);
                }
                //排除meta文件
                else if (!fileInfo[i].Name.EndsWith(".meta"))
                {
                    //设置单个资源的Bundle名称
                    SetAssetBundleName(fileInfo[i].FullName);
                }
            }
        }

        /// <summary>
        /// 设置单个资源的Bundle名称
        /// </summary>
        /// <param name="path">资源路径</param>
        private static void SetAssetBundleName(string path)
        {
            //拿到资源导入器的路径
            string importerPath = "Assets/" + path.Substring(Application.dataPath.Length + 1);
            //创建资源导入器
            AssetImporter importer = AssetImporter.GetAtPath(importerPath);
            //生成bundle名称
            if (importer != null)
            {
                //解析bundleName
                //去除前缀
                string bundleName = path.Substring(path.LastIndexOf('/') + 1);
                bundleName = bundleName.Substring(bundleName.LastIndexOf('\\') + 1);
                //去除后缀
                bundleName = bundleName.Remove(bundleName.LastIndexOf('.'));
                //设置bundleName--会自动转小写
                importer.assetBundleName = bundleName;
            }
        }
    }
}