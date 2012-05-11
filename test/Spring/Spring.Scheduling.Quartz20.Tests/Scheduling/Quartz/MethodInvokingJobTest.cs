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

using System;
using System.Threading;

using NUnit.Framework;

using Quartz;
using Quartz.Job;
using Quartz.Spi;

using Rhino.Mocks;

using Spring.Objects.Support;

#if QUARTZ_2_0
using JobExecutionContext = Quartz.Impl.JobExecutionContextImpl;
using JobDetail = Quartz.Impl.JobDetailImpl;
using SimpleTrigger = Quartz.Impl.Triggers.SimpleTriggerImpl;
#endif

namespace Spring.Scheduling.Quartz
{
    /// <summary>
    /// Tests for MethodInvokingJob.
    /// </summary>
    /// <author>Marko Lahma (.NET)</author>
    [TestFixture]
    public class MethodInvokingJobTest
    {
        private MethodInvokingJob methodInvokingJob;

        /// <summary>
        /// Test setup.
        /// </summary>
        [SetUp]
        public void SetUp()
        {
            methodInvokingJob = new MethodInvokingJob();
        }

        /// <summary>
        /// Test method invoke via execute.
        /// </summary>
        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void TestMethodInvoker_SetWithNull()
        {
            methodInvokingJob.MethodInvoker = null;
        }

        /// <summary>
        /// Test method invoke via execute.
        /// </summary>
        [Test]
        [ExpectedException(typeof(JobExecutionException))]
        public void TestMethodInvocation_NullMethodInvokder()
        {
            methodInvokingJob.Execute(CreateMinimalJobExecutionContext());
        }

        /// <summary>
        /// Test method invoke via execute.
        /// </summary>
        [Test]
        public void TestMethodInvoker_MethodSetCorrectly()
        {
            InvocationCountingJob job = new InvocationCountingJob();
            MethodInvoker mi = new MethodInvoker();
            mi.TargetObject = job;
            mi.TargetMethod = "Invoke";
            mi.Prepare();
            methodInvokingJob.MethodInvoker = mi;
            methodInvokingJob.Execute(CreateMinimalJobExecutionContext());
            Assert.AreEqual(1, job.CounterValue, "Job was not invoked once");
        }

        /// <summary>
        /// Test that invocation result is set to execution context (SPRNET-1340).
        /// </summary>
        [Test]
        public void TestMethodInvoker_ShouldSetResultToExecutionContext()
        {
            InvocationCountingJob job = new InvocationCountingJob();
            MethodInvoker mi = new MethodInvoker();
            mi.TargetObject = job;
            mi.TargetMethod = "InvokeWithReturnValue";
            mi.Prepare();
            methodInvokingJob.MethodInvoker = mi;
            JobExecutionContext context = CreateMinimalJobExecutionContext();
            methodInvokingJob.Execute(context);

            Assert.AreEqual(InvocationCountingJob.DefaultReturnValue, context.Result, "result value was not set to context");
        }

        /// <summary>
        /// Test method invoke via execute.
        /// </summary>
        [Test]
        public void TestMethodInvoker_MethodSetCorrectlyThrowsException()
        {
            InvocationCountingJob job = new InvocationCountingJob();
            MethodInvoker mi = new MethodInvoker();
            mi.TargetObject = job;
            mi.TargetMethod = "InvokeAndThrowException";
            mi.Prepare();
            methodInvokingJob.MethodInvoker = mi;
            try
            {
                methodInvokingJob.Execute(CreateMinimalJobExecutionContext());
                Assert.Fail("Successful invoke when method threw exception");
            }
            catch (JobMethodInvocationFailedException)
            {
                // ok
            }
            Assert.AreEqual(1, job.CounterValue, "Job was not invoked once");
        }

        /// <summary>
        /// Test method invoke via execute.
        /// </summary>
        [Test]
        public void TestMethodInvoker_PrivateMethod()
        {
            InvocationCountingJob job = new InvocationCountingJob();
            MethodInvoker mi = new MethodInvoker();
            mi.TargetObject = job;
            mi.TargetMethod = "PrivateMethod";
            mi.Prepare();
            methodInvokingJob.MethodInvoker = mi;
            methodInvokingJob.Execute(CreateMinimalJobExecutionContext());
        }
        
        private static JobExecutionContext CreateMinimalJobExecutionContext()
        {
            MockRepository repo = new MockRepository();
            IScheduler sched = (IScheduler) repo.DynamicMock(typeof (IScheduler));
            JobExecutionContext ctx = new JobExecutionContext(sched, ConstructMinimalTriggerFiredBundle(), null);
            return ctx;
        }

        private static TriggerFiredBundle ConstructMinimalTriggerFiredBundle()
        {
            JobDetail jd = new JobDetail("jobName", "jobGroup", typeof(NoOpJob));
            SimpleTrigger trigger = new SimpleTrigger("triggerName", "triggerGroup");
            TriggerFiredBundle retValue = new TriggerFiredBundle(jd, trigger, null, false, null, null, null, null);

            return retValue;
        }

    }

    /// <summary>
    /// Test class for method invoker.
    /// </summary>
    public class InvocationCountingJob
    {
        private int counter;
        internal const string DefaultReturnValue = "return value";

        /// <summary>
        /// Increments method invoke counter.
        /// </summary>
        public void Invoke()
        {
            Interlocked.Increment(ref counter);    
        }

        /// <summary>
        /// Throws exception after incrementing counter.
        /// </summary>
        public void InvokeAndThrowException()
        {
            Interlocked.Increment(ref counter);
            throw new Exception();
        }

        private void PrivateMethod()
        {
        }

        /// <summary>
        /// Returns <see cref="DefaultReturnValue" /> as return value.
        /// </summary>
        public string InvokeWithReturnValue()
        {
            return DefaultReturnValue;
        }

        /// <summary>
        /// Invocation count.
        /// </summary>
        public int CounterValue
        {
            get { return counter; }
        }
    }
}
