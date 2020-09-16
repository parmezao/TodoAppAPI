using System;
using System.Collections.Generic;
using System.Text;

namespace Todo.Domain.Commands
{
    public class PushNotificationCommand
    {
        public PushNotificationCommand() { }

        public PushNotificationCommand(string token, string name, int count)
        {
            this.Token = token;
            this.Name = name;
            this.Count = count;
        }

        public string Token { get; set; }
        public string Name { get; set; }
        public int Count { get; set; }
    }
}
