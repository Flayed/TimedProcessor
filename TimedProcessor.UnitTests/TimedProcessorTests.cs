using FluentAssertions;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Tigrinum.TimedProcessor.UnitTests
{
    public class TimedProcessorTests : TestBase
    {
        [Fact(DisplayName = "Start should set IsRunning to true")]
        public void Start_SetsIsRunningToTrue()
        {
            TimedProcessor processor = new TimedProcessor(1000, () => { }, true);
            processor.Start();

            processor.IsRunning.Should().BeTrue();
        }

        [Fact(DisplayName ="Start should invoke the method immediately if WhenStartedImmediately is set to true")]
        public async Task Start_WhenStartedImmediatelyTrue_InvokesImmediately()
        {
            bool invokedAction = false;

            Action processorAction = new Action(() => { invokedAction = true; });
            TimedProcessor processor = new TimedProcessor(100000, processorAction, true);

            processor.Start();
            CancellationTokenSource cts = new CancellationTokenSource();
            cts.CancelAfter(1000);
            while (processor.TimesElapsed == 0 && !cts.IsCancellationRequested)
            {
                await Task.Delay(10);
            }                        

            invokedAction.Should().BeTrue();
        }

        [Fact(DisplayName = "Start should invoke the asynchronous method immediately if WhenStartedImmediately is set to true")]
        public async Task Start_WhenStartedImmediatelyTrue_InvokesAsyncImmediately()
        {
            bool invokedAction = false;

            Func<Task> processorAction = new Func<Task>(() => { invokedAction = true; return Task.CompletedTask; });
            TimedProcessor processor = new TimedProcessor(100000, processorAction, true);

            processor.Start();
            CancellationTokenSource cts = new CancellationTokenSource();
            cts.CancelAfter(1000);
            while (processor.TimesElapsed == 0 && !cts.IsCancellationRequested)
            {
                await Task.Delay(10);
            }

            invokedAction.Should().BeTrue();
        }

        [Fact(DisplayName = "Start should not invoke the method immediately if WhenStartedImmediately is set to false")]
        public async Task Start_WhenStartedImmediatelyFalse_DoesNotInvokesImmediately()
        {
            bool invokedAction = false;

            Action processorAction = new Action(() => { invokedAction = true; });
            TimedProcessor processor = new TimedProcessor(1000, processorAction, false);

            processor.Start();
            await Task.Delay(500);

            invokedAction.Should().BeFalse();
        }

        [Fact(DisplayName = "Start should not invoke the async method immediately if WhenStartedImmediately is set to false")]
        public async Task Start_WhenStartedImmediatelyFalse_DoesNotInvokesAsyncImmediately()
        {
            bool invokedAction = false;

            Func<Task> processorAction = new Func<Task>(() => { invokedAction = true; return Task.CompletedTask; });
            TimedProcessor processor = new TimedProcessor(1000, processorAction, false);

            processor.Start();
            await Task.Delay(500);

            invokedAction.Should().BeFalse();
        }

        [Fact(DisplayName = "Stop should set IsRunning to false")]
        public async Task Stop_SetsIsRunningFalse()
        {
            TimedProcessor processor = new TimedProcessor(10000, () => { }, true);

            processor.Start();
            await Task.Delay(10);
            processor.Stop();

            processor.IsRunning.Should().BeFalse();
        }

        [Fact(DisplayName = "TimerElapsed should increment TimesElapsed")]
        public async Task TimerElapsed_IncrementsTimesElapsed()
        {
            TimedProcessor processor = new TimedProcessor(100000, ()=> { }, true);

            processor.Start();
            CancellationTokenSource cts = new CancellationTokenSource();
            cts.CancelAfter(1000);
            while (processor.TimesElapsed == 0 && !cts.IsCancellationRequested)
            {
                await Task.Delay(10);
            }
            
            processor.TimesElapsed.Should().BeGreaterThan(0);        
        }

        [Fact(DisplayName = "Method should not fire until the previous one is completed")]
        public async Task FastInterval_LongDurationMethod_OnlyInvokesMethodOnce()
        {
            int actionCount = 0;
            Action processorAction = new Action(() => 
            {
                Thread.Sleep(200);
                actionCount++;
            });
            TimedProcessor processor = new TimedProcessor(1, processorAction, true);

            processor.Start();

            await Task.Delay(400);
            actionCount.Should().Be(1);
        }

        [Fact(DisplayName = "Async Method should not fire until the previous one is completed")]
        public async Task FastInterval_LongDurationMethod_OnlyInvokesAsyncMethodOnce()
        {
            int actionCount = 0;
            Func<Task> processorAction = new Func<Task>(() =>
            {
                Thread.Sleep(200);
                actionCount++;
                return Task.CompletedTask;
            });
            TimedProcessor processor = new TimedProcessor(1, processorAction, true);

            processor.Start();

            await Task.Delay(400);
            actionCount.Should().Be(1);
        }

        [Fact(DisplayName = "Method should fire as fast as possible, but never more then one at a time")]
        public async Task FastInterval_OnlyInvokesAsFastAsTheMethodFinishes()
        {
            int actionCount = 0;
            Action processorAction = new Action(() => {
                Thread.Sleep(50);
                actionCount++;
            });
            TimedProcessor processor = new TimedProcessor(1, processorAction, true);

            processor.Start(); ;

            await Task.Delay(300);

            actionCount.Should().BeGreaterOrEqualTo(2);
            actionCount.Should().BeLessOrEqualTo(4);
        }

        [Fact(DisplayName = "Async Method should fire as fast as possible, but never more then one at a time")]
        public async Task FastInterval_OnlyInvokesAsFastAsTheAsyncMethodFinishes()
        {
            int actionCount = 0;
            Func<Task> processorAction = new Func<Task>(async () => {
                await Task.Delay(50);
                actionCount++;
            });
            TimedProcessor processor = new TimedProcessor(1, processorAction, true);

            processor.Start();

            await Task.Delay(400);

            actionCount.Should().BeGreaterOrEqualTo(2);
            actionCount.Should().BeLessOrEqualTo(4);
        }

    }
}
