using System.Collections.Generic;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Networking;

namespace AssetBundleFrame
{
    
    public class AssetBundleCount
    {
        public readonly AssetBundle AssetBundle;
        //Bundle被使用的次数
        public int bundleUseNum { get; set; }

        public AssetBundleCount(AssetBundle assetBundle)
        {
            AssetBundle = assetBundle;
            bundleUseNum = 1;
        }
        
    }
    
    public class AssetBundleLoad : Singleton<AssetBundleLoad>
    {
        //缓存列表<AB包名称, 使用数量对象>
        private readonly Dictionary<string, AssetBundleCount> _assetBundleDic;

        //构造
        private  AssetBundleLoad()
        {
            _assetBundleDic = new Dictionary<string, AssetBundleCount>();
        }

        /// <summary>
        /// 自动销毁Bundle
        /// </summary>
        /// <param name="assetBundleName">assetBundleName</param>
        public void AutoUnloadBundle(string assetBundleName, string[] dependencies)
        {
            //Bundle使用数量-1，为0时释放资源
            _assetBundleDic[assetBundleName].bundleUseNum--;
            // Debug.Log($"{assetBundleName}--：{_assetBundleDic[assetBundleName].bundleUseNum}");
            if (_assetBundleDic[assetBundleName].bundleUseNum <= 0)
            {
                //卸载AssetBundle
                _assetBundleDic[assetBundleName].AssetBundle.Unload(true);
                //从字典移除
                _assetBundleDic.Remove(assetBundleName);
                // Debug.Log($"卸载{assetBundleName}");
            }
            
            if (dependencies != null)
            {
                //卸载依赖
                for (int i = 0; i < dependencies.Length; i++)
                {
                    AutoUnloadBundle(dependencies[i], null);
                }
            }
        }


        /// <summary>
        /// 通过本地AB包获取实例化的资源对象
        /// </summary>
        /// <param name="assetName">要获取的资源在资源文件夹下的相对路径</param>
        /// <param name="pos">初始化的位置</param>
        /// <param name="rotation">初始化的旋转</param>
        /// <param name="parent">初始化的父对象</param>
        /// <returns></returns>
        public GameObject LoadAssetByFile(string assetName, Vector3 pos, Quaternion rotation, Transform parent)
        {
            //加载manifest的AB文件
            AssetBundle manifest;
            if (_assetBundleDic.ContainsKey("manifest"))
            {
                //直接使用原有manifest
                manifest = _assetBundleDic["manifest"].AssetBundle;
                //使用量+1
                _assetBundleDic["manifest"].bundleUseNum++;
                // Debug.Log($"\"manifest\"++：{_assetBundleDic["manifest"].bundleUseNum}");
            }
            else
            {
                //获取manifest的AssetBundle
                manifest = AssetBundle.LoadFromFile(AssetBundleDefine.ManifestFullPath);
                // Debug.Log("manifest创建");
                //存储到列表
                _assetBundleDic.Add("manifest", new AssetBundleCount(manifest));
            }
            //获取manifest文件
            AssetBundleManifest manifestFile = manifest.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
            //去除前缀获取资源的独立名称
            string assetLocalName = assetName.Substring(assetName.LastIndexOf('/') + 1);
            assetLocalName = assetLocalName.Substring(assetName.LastIndexOf('\\') + 1);
            
            //从manifest中获取AB资源依赖的AB资源名称
            string[] dependencies = manifestFile.GetAllDependencies(assetLocalName.ToLower());
            
            //卸载manifest资源AB包
            AutoUnloadBundle("manifest", null);
            
            //声明一个存放依赖的AB包的数组
            AssetBundle[] assetBundles = new AssetBundle[dependencies.Length];
            
            //获取所有依赖的AB资源
            for (int i = 0; i < dependencies.Length; i++)
            {
                if (_assetBundleDic.ContainsKey(dependencies[i]))
                {
                    //直接使用缓存的包
                    assetBundles[i] = _assetBundleDic[dependencies[i]].AssetBundle;
                    //使用量+1
                    _assetBundleDic[dependencies[i]].bundleUseNum++;
                    // Debug.Log($"{dependencies[i]}++：{_assetBundleDic[dependencies[i]].bundleUseNum}");
                }
                else
                {
                    //拼凑依赖的AB资源名称
                    string assetBundlePath = AssetBundleDefine.AbPath + "/" + dependencies[i];
                    //加载依赖的AB资源
                    assetBundles[i] = AssetBundle.LoadFromFile(assetBundlePath);
                    // Debug.Log($"{assetBundles[i]}创建");
                    //存储到列表
                    _assetBundleDic.Add(dependencies[i], new AssetBundleCount(assetBundles[i]));
                }
                
            }
            
            //加载资源的AB文件
            AssetBundle assetBundle;
            if (_assetBundleDic.ContainsKey(assetLocalName))
            {
                //直接使用原有AB包
                assetBundle = _assetBundleDic[assetLocalName].AssetBundle;
                //使用量+1
                _assetBundleDic[assetLocalName].bundleUseNum++;
                // Debug.Log($"{assetLocalName}++：{_assetBundleDic[assetLocalName].bundleUseNum}");
            }
            else
            {
                //拼凑依赖的AB资源名称
                string assetBundlePath = AssetBundleDefine.AbPath + "/"  + assetLocalName.ToLower();
                //获取manifest的AssetBundle
                assetBundle = AssetBundle.LoadFromFile(assetBundlePath);
                // Debug.Log($"{assetLocalName}创建");
                //存储到列表
                _assetBundleDic.Add(assetLocalName, new AssetBundleCount(assetBundle));
            }
            
            //获取资源文件
            Object assetObj = assetBundle.LoadAsset(assetLocalName);
            //复制克隆
            GameObject obj;
            obj = Object.Instantiate(assetObj, pos, rotation, parent) as GameObject;
            //给资源挂载自动销毁脚本
            obj.transform.AddComponent<AssetBundleUnload>();
            obj.transform.GetComponent<AssetBundleUnload>().assetBundleName = assetLocalName;
            obj.transform.GetComponent<AssetBundleUnload>().dependencies = dependencies;

            return obj;
        }

        /// <summary>
        /// 通过网络AB包获取资源生成对象
        /// </summary>
        /// <param name="assetName">要获取的资源在资源文件夹下的相对路径</param>
        /// <param name="obj">找到的对象</param>
        /// <returns></returns>
        public IEnumerator LoadAssetByWeb(string assetName, Vector3 pos, Quaternion rotation, Transform parent)
        {
            //加载manifest的AB文件
            AssetBundle manifest;
            UnityWebRequest request;
            if (_assetBundleDic.ContainsKey("manifest"))
            {
                manifest = _assetBundleDic["manifest"].AssetBundle;
            }
            else
            {
                //通过WEB获取manifest的AB文件
                request = UnityWebRequestAssetBundle.GetAssetBundle(AssetBundleDefine.WebManifestFullPath);
                yield return request.SendWebRequest();
                //加载manifest的AB文件
                manifest = DownloadHandlerAssetBundle.GetContent(request);
                //存储到列表
                _assetBundleDic.Add("manifest", new AssetBundleCount(manifest));
            }
            //获取manifest文件
            AssetBundleManifest manifestFile = manifest.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
            //去除前缀获取资源的独立名称
            string assetLocalName = assetName.Substring(assetName.LastIndexOf('/') + 1);
            assetLocalName = assetLocalName.Substring(assetName.LastIndexOf('\\') + 1);
            
            //从manifest中获取AB资源依赖的AB资源名称
            string[] dependencies = manifestFile.GetAllDependencies(assetLocalName.ToLower());
            
            //卸载manifest资源AB包
            AutoUnloadBundle("manifest", null);
            
            //声明一个存放依赖的AB包的数组
            AssetBundle[] assetBundles = new AssetBundle[dependencies.Length];
            
            //获取所有依赖的AB资源
            for (int i = 0; i < dependencies.Length; i++)
            {
                //拼凑依赖的AB资源路径
                string assetBundlePath = AssetBundleDefine.WebAbPath + "/" + dependencies[i];
                //提前通过WEB获取资源的AB资源-防止多次进字典
                request = UnityWebRequestAssetBundle.GetAssetBundle(assetBundlePath);
                yield return request.SendWebRequest();
                //判断列表是否有
                if (_assetBundleDic.ContainsKey(dependencies[i]))
                {
                    //直接使用原有AB包
                    assetBundles[i] = _assetBundleDic[dependencies[i]].AssetBundle;
                    //使用量+1
                    _assetBundleDic[dependencies[i]].bundleUseNum++;
                    // Debug.Log($"{dependencies[i]}++：{_assetBundleDic[dependencies[i]].bundleUseNum}");
                }
                else
                {
                    //加载资源的AB资源
                    assetBundles[i] = DownloadHandlerAssetBundle.GetContent(request);
                    // Debug.Log($"{dependencies[i]}创建");
                    //存储到列表
                    _assetBundleDic.Add(dependencies[i], new AssetBundleCount(assetBundles[i]));
                }
            }
            
            //加载资源的AB文件
            AssetBundle assetBundle;
            if (_assetBundleDic.ContainsKey(assetLocalName))
            {
                //直接使用原有AB包
                assetBundle = _assetBundleDic[assetLocalName].AssetBundle;
                //使用量+1
                _assetBundleDic[assetLocalName].bundleUseNum++;
                // Debug.Log($"{assetLocalName}++：{_assetBundleDic[assetLocalName].bundleUseNum}");
            }
            else
            {
                //拼凑依赖的AB资源路径
                string assetBundlePath = AssetBundleDefine.WebAbPath + "/" + assetLocalName.ToLower();
                //通过WEB获取资源的AB资源
                request = UnityWebRequestAssetBundle.GetAssetBundle(assetBundlePath);
                yield return request.SendWebRequest();
                //加载资源的AB资源
                assetBundle = DownloadHandlerAssetBundle.GetContent(request);
                // Debug.Log($"{assetLocalName}创建");
                //存储到列表
                _assetBundleDic.Add(assetLocalName, new AssetBundleCount(assetBundle));
            }
            
            //获取资源文件
            Object assetObj = assetBundle.LoadAsset(assetLocalName);
            //复制克隆
            GameObject obj;
            obj = Object.Instantiate(assetObj, pos, rotation, parent) as GameObject;
            //给资源挂载自动销毁脚本
            obj.transform.AddComponent<AssetBundleUnload>();
            obj.transform.GetComponent<AssetBundleUnload>().assetBundleName = assetLocalName;
            obj.transform.GetComponent<AssetBundleUnload>().dependencies = dependencies;
            
            //测试销毁
            // yield return new WaitForSeconds(2);
            // GameObject.Destroy(obj);
        }

    }
}
