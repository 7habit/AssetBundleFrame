# AssetBundleFrame
专用于Unity的AssetBundle简单框架

# 功能特性
- 生成对象时，自动管理对象和其依赖
- 通过计数管理对象和其依赖的Bundle的自动销毁
- 兼容通过本地文件加载资源、通过网络加载资源

# 使用方法
1. 导入框架脚本资源
2. 编辑AssetBundleDefine.cs
   - 需要打包的资源路径：原始资源的存放地
   - AB资源存放路径：打包完成的AB资源放在哪里
   - manifest说明文件全路径：AB资源文件夹下和文件夹同名的资源
```
        //需要打包的资源路径
        public static readonly string ResPath = Application.dataPath + "/Resources/ABRes";

        //AB资源存放路径
        public static readonly string AbPath = Application.dataPath + "/AssetBundle";
        //manifest说明文件全路径
        public static readonly string ManifestFullPath = Application.dataPath + "/AssetBundle" + "/AssetBundle";
        
        
        //AB资源存放路径-WEB
        public const string WebAbPath = "http://xxx.cn" + "/AssetBundle";
        //manifest说明文件全路径-WEB
        public const string WebManifestFullPath = "http://xxx.cn" + "/AssetBundle" + "/AssetBundle";
```
3. 一键生成AB资源
   - 在编辑器菜单下点击【AssetBundle/BuildBundles】
   - 注意：生成的AB包与【BuildSetting】菜单中的目标平台一致，更换平台时需要重新Build
4. 通过资源相对路径从本地AB包/网络AB包获取对应的资源
   - 通过本地AB包获取对象：AssetBundleLoad.Instance.LoadAssetByFile("Cube", Vector3.one, Quaternion.identity, transform);
   - 通过网络AB包获取对象：StartCoroutine(AssetBundleLoad.Instance.LoadAssetByWeb("2/Cube1", Vector3.one, Quaternion.identity, transform));
   - 参数：原始资源文件夹下的相对路径，初始位置，初始旋转，父对象Transfrom
```
using UnityEngine;
using AssetBundleFrame;

public class NormalLoad : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            AssetBundleLoad.Instance.LoadAssetByFile
                ("Cube", Vector3.one, Quaternion.identity, transform);

        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            StartCoroutine(AssetBundleLoad.Instance.LoadAssetByWeb
                ("2/Cube1", Vector3.one, Quaternion.identity, transform));
        }
    }
}
```
