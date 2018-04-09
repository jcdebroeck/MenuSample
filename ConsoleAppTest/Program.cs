using System;
using CommandLine;
using System.Collections.Generic;
using System.Xml.XPath;
using System.IO;
using XmlMenus;

namespace MenuSample
{
    class Program
    {
        public const string ActiveItemLabel = "ACTIVE";

        static void Main(string[] args)
        {
            bool successfulParse = false;
            CommandParameters parameters = new CommandParameters (args);
            if ( !parameters.Valid () )
            {
                Console.WriteLine ("Please provide a menu file path and optionally an active menu path.");
                //Console.ReadKey ();
                return;
            }

            parameters.DumpToConsole ();

            List<MenuNode> menuItems = new List<MenuNode> ();
            try
            {
                MenuParser menu = ParseXmlMenuFile (parameters.InputFilePath, parameters.ActivePath);
                menuItems = menu.Items;
                successfulParse = true;
            }
            catch (Exception)
            {
                Console.WriteLine ("A problem accessing or parsing \"{0}\" occured.", parameters.InputFilePath);
            }

            if ( successfulParse )
            {
                if ( menuItems.Count > 0 )
                {
                    DumpMenuToConsole (menuItems);
                }
                else 
                {
                    Console.WriteLine ("No Menu Items were found in \"{0}\".", parameters.InputFilePath);
                }
            }

            //Console.ReadKey ();
        }

        static MenuParser ParseXmlMenuFile (string filePath, string activePath)
        {
            FileStream file = new FileStream (filePath, FileMode.Open);

            XPathDocument xpath = new XPathDocument (file);

            MenuParser menus = new MenuParser (activePath);
            menus.Parse (xpath);

            return menus;
        }

        //
        // menu dumping routines
        //
        static public void DumpMenuToConsole (List<MenuNode> menuItems)
        {
            int depth = 0;
            DumpSubMenuToConsole (menuItems, depth);
        }

        static public void DumpSubMenuToConsole (List<MenuNode> subMenu, int depth)
        {
            string tabSize = "        ";
            string indent = "";
            
            for (int i=0; i < depth; i++)
            {
                indent += tabSize;
            }

            foreach (MenuNode node in subMenu)
            {
                string activeLabel = node.Active ? ActiveItemLabel : String.Empty;

                Console.WriteLine ("{0}{1}, {2} {3}", indent, node.Name, node.Path, activeLabel);
                DumpSubMenuToConsole (node.SubMenu, depth + 1);
            }
        }

    }

}
