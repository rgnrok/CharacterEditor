using System;

namespace CharacterEditor
{
    public class AllServices
    {
        private static AllServices _instance;
        public static AllServices Container => _instance ?? (_instance = new AllServices());

        public void RegisterSingle<TService>(TService implementation) where TService : IService =>
            Implementation<TService>.ServiceInstance = implementation;

        public TService Single<TService>() where TService : IService =>
            Implementation<TService>.ServiceInstance;

        public void Dispose<TService>() where TService : IService
        {
            var service = Implementation<TService>.ServiceInstance;
            if (service is IDisposable disposable)
                disposable.Dispose();

            service = default(TService);
        }

        private static class Implementation<TService> where TService : IService
        {
            public static TService ServiceInstance;
        }
    }
}