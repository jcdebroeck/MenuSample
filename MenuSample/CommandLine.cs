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

        public bool Valid()
        {
            return ( m_args.Length >= 1 );
        }

        public string InputFilePath
        {
            get
            {
                return ( m_args.Length > 0 ) ? m_args[0] : "";
            }
        }

        public string ActivePath
        {
            get
            {
                return ( m_args.Length > 1 ) ? m_args[1] : "";
            }
        }

        public void DumpToConsole ()
        {
            foreach (string arg in m_args)
            {
                Console.WriteLine (arg);
            }
            Console.WriteLine ("");
        }
    }

}
