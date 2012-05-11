/*
* Copyright 2002-2010 the original author or authors.
* 
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
* 
*      http://www.apache.org/licenses/LICENSE-2.0
* 
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using NUnit.Framework;

using Quartz;

#if QUARTZ_2_0
using JobDetail = Quartz.Impl.JobDetailImpl;
#endif

namespace Spring.Scheduling.Quartz
{
    /// <summary>
    /// Unit tests for MethodInvokingJobDetailFactoryObject.
    /// </summary>
    /// <author>Marko Lahma (.NET)</author>
    [TestFixture]
    public class MethodInvokingJobDetailFactoryObjectTest
    {
        private const string FACTORY_NAME = "springObjectFactory";
        private MethodInvokingJobDetailFactoryObject factory;

        /// <summary>
        /// Setup for the test.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            factory = new MethodInvokingJobDetailFactoryObject();
            factory.ObjectName = FACTORY_NAME;
            factory.TargetMethod = "Invoke";
            factory.TargetObject = new InvocationCountingJob();
        }

        /// <summary>
        /// Tests JobDetail retrieval and it's set properties.
        /// </summary>
        [Test]
        public void TestGetObject_MinimalDefaults()
        {
            factory.AfterPropertiesSet();
            JobDetail jd = (JobDetail) factory.GetObject();
            Assert.IsNotNull(jd, "job detail was null");
            Assert.AreEqual(FACTORY_NAME, jd.Name, "job name did not default to factory name");
            Assert.AreEqual(jd.JobType, typeof(MethodInvokingJob), "factory did not create method invoking job");
            Assert.IsTrue(jd.Durable, "job was not durable");
#if !QUARTZ_2_0
            Assert.IsTrue(jd.Volatile, "job was not volatile");
#endif
        }

        /// <summary>
        /// Tests JobDetail retrieval and it's set properties.
        /// </summary>
        [Test]
        public void TestGetObject_ConcurrentJob()
        {
            factory.Concurrent = false;
            factory.AfterPropertiesSet();
            JobDetail jd = (JobDetail)factory.GetObject();
            Assert.IsNotNull(jd, "job detail was null");
            Assert.AreEqual(jd.JobType, typeof(StatefulMethodInvokingJob), "factory did not create stateful method invoking job");
        }

#if !QUARTZ_2_0
        /// <summary>
        /// Tests JobDetail retrieval and it's set properties.
        /// </summary>
        [Test]
        public void TestGetObject_TriggerListenersSet()
        {
            string[] LISTENER_NAMES = new string[] {"Foo", "Bar"};
            factory.JobListenerNames = LISTENER_NAMES;
            factory.AfterPropertiesSet();
            JobDetail jd = (JobDetail)factory.GetObject();
            CollectionAssert.AreEquivalent(LISTENER_NAMES, jd.JobListenerNames);
        }
#endif

    }
}
