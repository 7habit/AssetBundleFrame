using System;

namespace AssetBundleFrame
{
    public class Singleton<T> where T:class
    {
        private static T _instance;
        public static T Instance
        {
            get
            {
                if(_instance == null)
                {
                    //通过反射创建私有构造的对象
                    _instance = Activator.CreateInstance(typeof(T), true) as T;
                }
                return _instance;
            }
        }

    }
}

