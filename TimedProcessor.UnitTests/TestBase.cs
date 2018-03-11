using Autofac.Extras.Moq;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tigrinum.TimedProcessor.UnitTests
{
    public class TestBase : IDisposable
    {
        public readonly AutoMock Mock;
        public TestBase()
        {
            Mock = AutoMock.GetLoose();
        }

        public TImplementation Provide<TInterface, TImplementation>()
            where TInterface : class
            where TImplementation : TInterface
        {
            TImplementation impl = Mock.Create<TImplementation>();
            Mock.Provide<TInterface>(impl);
            return impl;
        }

        public void Dispose()
        {
            Mock.Dispose();
        }
    }
}
