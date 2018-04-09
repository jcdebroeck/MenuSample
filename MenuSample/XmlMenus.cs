using System;
using System.Collections.Generic;
using System.Xml.XPath;

namespace XmlMenus
{
    //
    // simple tree structure to represent menus
    //
    public class MenuNode
    {
        public string Name { get; set; }
        public string Path { get; set; }
        public bool Active { get; set; }

        public List<MenuNode> SubMenu { get; set; }

        public MenuNode ()
        {
            SubMenu = new List<MenuNode> ();
        }

        public List<MenuNode> FindAll (Predicate<MenuNode> match)
        {
            // First, search for the submenu items that match
            List<MenuNode> found = SubMenu.FindAll (match);

            // Then, ask each submenu Item to search it's submenu's
            foreach (MenuNode x in SubMenu)
            {
                found.AddRange (x.FindAll (match));
            }
            return found;
        }
    }


    //
    // parser class to do the work
    //
    public class MenuParser
    {
        public const string MenuItemTag =           @"item";
        public const string RootMenuTags =          @"//menu/item";
        public const string MenuTagName =           @"displayName";
        public const string MenuTagPath =           @"path";
        public const string MenuTagPathAttribute =  @"value";
        public const string MenuTagSubmenu =        @"subMenu";

        private List<MenuNode>  m_nodes;
        private string          m_activePath = "";


        public List<MenuNode> Items
        {
            get { return m_nodes; }
        }

        public MenuParser (string activePath)
        {
            m_nodes = new List<MenuNode> ();
            m_activePath = activePath;
        }

        public bool Parse(XPathDocument xpath)
        {
            XPathNavigator xpn = xpath.CreateNavigator ();

            m_nodes = ParseMenuItems (RootMenuTags, xpn);

            return ( m_nodes.Count > 0 );
        }

        private List<MenuNode> ParseMenuItems (string xPathExpression, XPathNavigator xpn)
        {
            // always have at least an empty but valid submenu
            List<MenuNode> newMenu = new List<MenuNode> ();

            // no branch to process
            if ( null == xpn )
            {
                return newMenu;
            }

            // process the menu items at this level
            XPathNodeIterator xpMenuItems = xpn.Select (xPathExpression);
            foreach ( XPathNavigator xpMenuItem in xpMenuItems)
            {
                // a menu item can have only one name and path by definition
                XPathNavigator tagName = xpMenuItem.SelectSingleNode (MenuTagName);
                XPathNavigator tagPath = xpMenuItem.SelectSingleNode (MenuTagPath);
                
                // Ignore a node that is not well defined
                if ( null != tagName && null != tagPath )
                {
                    string name = tagName.Value;
                    string path = tagPath.GetAttribute (MenuTagPathAttribute, "");
                    bool active = m_activePath.Equals (path);

                    // a menu item can only invoke a single sub-menu
                    XPathNavigator xpSubItems = xpMenuItem.SelectSingleNode (MenuTagSubmenu);
                    List<MenuNode> subMenu = ParseMenuItems (MenuItemTag, xpSubItems);

                    MenuNode node = new MenuNode () {
                        Name = name,
                        Path = path,
                        Active = active,
                        SubMenu = subMenu
                    };

                    newMenu.Add (node);
                }
            }

            // return the menu including its submenus in the tree
            return newMenu;
        }

        public List<MenuNode> FindAll (Predicate<MenuNode> match)
        {
            // first invoke find on the menu item list at the root
            List<MenuNode> found = m_nodes.FindAll (match);
            
            // then ask each submenu to search themselves
            foreach (MenuNode x in m_nodes)
            {
                found.AddRange (x.FindAll (match));
            }
            return found;
        }
    }
}
