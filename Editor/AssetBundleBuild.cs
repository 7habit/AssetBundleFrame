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
            //��յ�ǰ���е�Bundle����
            ClearAssetBundleName();
            //������Դ�ļ��µ��ļ���Bundle����
            SetAssetBundlesName(AssetBundleDefine.ResPath);
            //���
            BeginAssetBundleBuild();
            //��յ�ǰ���е�Bundle����
            ClearAssetBundleName();
            //ˢ��
            AssetDatabase.Refresh();
        }
        
        /// <summary>
        /// һ������������AB���Ƶ���Դ��AB��
        /// </summary>
        private static void BeginAssetBundleBuild()
        {
            //���
            BuildPipeline.BuildAssetBundles(
                AssetBundleDefine.AbPath, //���·��
                //ѹ��ѡ�None-��С��������ChunkBasedCompression-���еȼ��ؿ죬UncompressedAssetBundle-��ѹ�����ؿ�
                BuildAssetBundleOptions.ChunkBasedCompression, 
                EditorUserBuildSettings.activeBuildTarget); //����BuildSetting�е�ѡ����ѡ���Ӧ�Ĺ���ƽ̨
        }

        /// <summary>
        /// ��յ�ǰ���е�Bundle����
        /// </summary>
        private static void ClearAssetBundleName()
        {
            //��ȡ���е�Bundle����
            string[] bundleNames = AssetDatabase.GetAllAssetBundleNames();
            //����ɾ��AB����
            for (int i = 0; i < bundleNames.Length; i++)
            {
                //ɾ��Bundle����
                AssetDatabase.RemoveAssetBundleName(bundleNames[i], true);
            }
        }

        /// <summary>
        /// ������Դ�ļ��µ��ļ���Bundle����
        /// </summary>
        /// <param name="path">��Ҫ���õ���Դ�ļ���·��</param>
        private static void SetAssetBundlesName(string path)
        {
            //��ȡ��Դ�ļ���
            DirectoryInfo rootInfo = new DirectoryInfo(path);
            //��ȡ���ļ�
            FileSystemInfo[] fileInfo = rootInfo.GetFileSystemInfos();
            //�������ļ�
            for (int i = 0; i < fileInfo.Length; i++)
            {
                //������ļ���
                if (fileInfo[i] is DirectoryInfo)
                {
                    //�ݹ����
                    SetAssetBundlesName(fileInfo[i].FullName);
                }
                //�ų�meta�ļ�
                else if (!fileInfo[i].Name.EndsWith(".meta"))
                {
                    //���õ�����Դ��Bundle����
                    SetAssetBundleName(fileInfo[i].FullName);
                }
            }
        }

        /// <summary>
        /// ���õ�����Դ��Bundle����
        /// </summary>
        /// <param name="path">��Դ·��</param>
        private static void SetAssetBundleName(string path)
        {
            //�õ���Դ��������·��
            string importerPath = "Assets/" + path.Substring(Application.dataPath.Length + 1);
            //������Դ������
            AssetImporter importer = AssetImporter.GetAtPath(importerPath);
            //����bundle����
            if (importer != null)
            {
                //����bundleName
                //ȥ��ǰ׺
                string bundleName = path.Substring(path.LastIndexOf('/') + 1);
                bundleName = path.Substring(path.LastIndexOf('\\') + 1);
                //ȥ����׺
                bundleName = bundleName.Remove(bundleName.LastIndexOf('.'));
                //����bundleName--���Զ�תСд
                importer.assetBundleName = bundleName;
            }
        }
    }
}