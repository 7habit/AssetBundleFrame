using UnityEngine;

namespace AssetBundleFrame
{
    public class AssetBundleDefine
    {
        //需要打包的资源路径
        public static readonly string ResPath = Application.dataPath + "/Resources/ABRes";

        //AB资源存放路径
        public static readonly string AbPath = Application.dataPath + "/AssetBundle";
        //manifest说明文件全路径
        public static readonly string ManifestFullPath = Application.dataPath + "/AssetBundle" + "/AssetBundle";
        
        
        //AB资源存放路径-WEB
        public const string WebAbPath = "http://pmhyl.cn" + "/AssetBundle";
        //manifest说明文件全路径-WEB
        public const string WebManifestFullPath = "http://pmhyl.cn" + "/AssetBundle" + "/AssetBundle";
    }
}