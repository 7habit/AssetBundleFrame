using UnityEngine;

namespace AssetBundleFrame
{
    public class AssetBundleUnload : MonoBehaviour
    {
        public string assetBundleName;
        public string[] dependencies;
        
        private void OnDestroy()
        {
            //自动销毁AssetBundle
            AssetBundleLoad.Instance.AutoUnloadBundle(assetBundleName, dependencies);
        }
    }
}