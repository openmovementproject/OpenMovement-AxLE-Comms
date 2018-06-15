using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using OpenMovement.AxLE.Comms.Exceptions;
using OpenMovement.AxLE.Comms.Interfaces;

namespace OpenMovement.AxLE.Comms.Commands
{
    public abstract class AxLECommand<T> : IAxLECommand
    {
        protected readonly float _timeout;

        protected IList<string> Data { get; set; }

        public IAxLEDevice Device { get; set; }
        public TaskCompletionSource<T> TCS { get; private set; }

        protected Timer TimeoutTimer { get; set; }

        protected AxLECommand(float timeout = 2000)
        {
            _timeout = timeout;

            TCS = new TaskCompletionSource<T>();
            Data = new List<string>();
        }

        private void ResetTimer()
        {
            TimeoutTimer?.Stop();
            TimeoutTimer?.Dispose();

            TimeoutTimer = new Timer
            {
                Interval = _timeout,
                AutoReset = false
            };

            TimeoutTimer.Elapsed += (s, e) =>
            {
                if (LookForEnd())
                {
                    CompleteCommand();
                }
                else
                {
                    ErrorCommand();
                }
            };

            TimeoutTimer.Start();
        }

        public async Task Execute()
        {
            Device.RxUart += DataRecieved;

            try
            {
                await SendCommand();
            }
            catch (Exception e)
            {
                TCS.SetException(new CommandFailedException(e));
            }

            ResetTimer();
            await TCS.Task;
        }

        public abstract Task SendCommand();

        protected virtual void DataRecieved(object sender, string data)
        {
            ResetTimer();
            Data.Add(data);
            if (LookForEnd())
            {
                CompleteCommand();
            }
        }

        protected abstract bool LookForEnd();

        protected abstract T ProcessResult();

        private void CompleteCommand()
        {
            Device.RxUart -= DataRecieved;
            TimeoutTimer.Stop();
            TimeoutTimer.Dispose();

            try
            {
                var result = ProcessResult();
                TCS.SetResult(result);
            }
            catch (Exception e)
            {
                TCS.SetException(new CommandFailedException(Data.ToArray(), e));
            }
        }

        private void ErrorCommand()
        {
            Device.RxUart -= DataRecieved;
            TimeoutTimer.Stop();
            TimeoutTimer.Dispose();

            TCS.SetException(new CommandFailedException(Data.ToArray()));
        }

        public void Cancel()
        {
            Device.RxUart -= DataRecieved;
            TimeoutTimer.Stop();
            TimeoutTimer.Dispose();

            TCS.SetCanceled();
        }
    }
}