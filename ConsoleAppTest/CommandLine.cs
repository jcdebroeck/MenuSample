using System;

namespace CommandLine
{
    class CommandParameters
    {
        public CommandParameters (string[] args)
        {
            m_args = args;
        }

        private string [] m_args { get; set; }

        public string InputFilePath
        {
            get
            {
                return m_args[0];
            }
        }

        public string ActivePath
        {
            get
            {
                return m_args[1];
            }
        }

        public void DumpToConsole ()
        {
            foreach (string arg in m_args)
            {
                Console.WriteLine (arg);
            }
        }
    }

}
