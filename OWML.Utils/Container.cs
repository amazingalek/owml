﻿using Microsoft.Practices.Unity;

namespace OWML.Utils
{
    public class Container
    {
        private readonly IUnityContainer _container = new UnityContainer();

        public Container Add<TInterface>(TInterface instance)
        {
            _container.RegisterInstance(instance);
            return this;
        }

        public Container Add<TInterface, TImplementation>() where TImplementation : TInterface
        {
            _container.RegisterType<TInterface, TImplementation>();
            return this;
        }

        public Container Add<TImplementation>()
        {
            _container.RegisterType<TImplementation>();
            return this;
        }

        public T Resolve<T>()
        {
            return _container.Resolve<T>();
        }
    }
}
