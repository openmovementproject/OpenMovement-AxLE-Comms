using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OpenMovement.AxLE.Comms.Exceptions;
using OpenMovement.AxLE.Comms.Interfaces;

namespace OpenMovement.AxLE.Comms.Commands
{
    public abstract class AxLECommandStream<T> : IAxLECommand
    {
        protected IList<string> Data { get; set; }
        protected bool Processing { get; set; }

        public TaskCompletionSource<int> TCS { get; private set; }

        public IAxLEDevice Device { get; set; }
        public event EventHandler<T> NewBlock;

        protected AxLECommandStream()
        {
            TCS = new TaskCompletionSource<int>();
            Data = new List<string>();
            Processing = false;
        }

        public async Task Execute()
        {
            Device.RxUart += DataRecieved;

            try
            {
                await SendStartCommand();
            }
            catch (Exception e)
            {
                TCS.SetException(new CommandFailedException(e));
            }

            await TCS.Task;
        }

        public abstract Task SendStartCommand();

        public abstract Task SendStopCommand();

        protected virtual void DataRecieved(object sender, string data)
        {
            Data.Add(data);

            if (!Processing && LookForBlock()) {
                Processing = true;
                NewBlock?.Invoke(this, ProcessBlock());
                Processing = false;
            }
        }

        protected abstract bool LookForBlock();

        protected abstract T ProcessBlock();

        public async virtual Task StopStream()
        {
            Device.RxUart -= DataRecieved;
            await SendStopCommand();
            TCS.SetResult(0);
        }
    }
}
