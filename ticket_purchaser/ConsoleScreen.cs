using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ticket_purchaser
{
    internal class ConsoleScreen
    {
        private List<Command> Commands { get; } = [];
        private string Title { get; set; }
        private bool Cancelable { get;  set; } = false; 

        public ConsoleScreen(string title = "") 
        {
            Title = title;
        }

        public ConsoleScreen(List<Command> commands, string title = "")
        {
            Commands = commands;
            Title = title;
        }

        public void AddCommand(Command command, int index)
        {
            Commands.Insert(index, command);
        }

        public void AddCommand(Command command)
        {
            Commands.Add(command);
        }

        void Show() 
        {
            Console.Clear();
            Console.WriteLine(Title);
            if (Commands.Count > 0)
            {
                ConsoleOptionManager.ChooseOne<Command>(Title, true, Commands);
            }
        }
    }
}
