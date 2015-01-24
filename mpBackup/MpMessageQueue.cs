using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mpBackup.MpGUI
{
    public class MpMessageQueue
    {
        public List<MpMessage> messages = new List<MpMessage>();

        public void addMessage(MpMessage message)
        {
            lock (this.messages)
            {
                this.messages.Add(message);
            }
        }

        public void addMessageAsync(MpMessage message)
        {
            Task.Factory.StartNew(() =>
            {
                addMessage(message);
            });
        }

        public List<MpMessage> getMessages(MpMessage.DisplayAs displayFilter)
        {
            lock (this.messages)
            {
                List<MpMessage> result = this.messages.Where(m => m.displayAs == displayFilter).ToList();
                this.messages = this.messages.Except(result).ToList();
                return result;
            }
        }
    }
}