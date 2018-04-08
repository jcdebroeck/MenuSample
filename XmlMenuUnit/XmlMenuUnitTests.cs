using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.XPath;
using XmlMenus;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text;

namespace XmlMenuUnitTests
{
    [TestClass]
    public class MenuParserTests
    {
        private XPathDocument CreateXmLDocument(string testXml)
        {
            MemoryStream testStream = new MemoryStream (Encoding.UTF8.GetBytes (testXml));
            XPathDocument testDocument = new XPathDocument (testStream);
            return testDocument;
        }

        [TestMethod]
        public void ParseMenuExample1Success()
        {
            //Arrange
            // This example menu xml taken directly from SchedAero Menu.txt provided.
            // In the PDF describing the parsing it is used as the example in discussion.
            // The discussion of the example is innacurate for the following reasons.
            // 1. There is only one path that can be active by spec but the example output 
            //    flags two different paths, /Requests/Quotes/CreateQuote.aspx and 
            //    /Requests/OpenQuotes.aspx as ACTIVE.
            // 2. The parameters for the example specificy the path /Requests/OpenQuotes.aspx
            //    therefore flagging ...CreateQuote.aspx is an error.
            //
            // This test covers just the basis that parsing happens successfully.
            string testXml = @"<menu>
	            <item>
		            <displayName>Home</displayName>
		            <path value='/Default.aspx'/>
	            </item>
	            <item>
		            <displayName>Trips</displayName>
		            <path value='/Requests/Quotes/CreateQuote.aspx'/>
		            <subMenu>
			            <item>
				            <displayName>Create Quote</displayName>
				            <path value='/Requests/Quotes/CreateQuote.aspx'/>
			            </item>
			            <item>
				            <displayName>Open Quotes</displayName>
				            <path value='/Requests/OpenQuotes.aspx'/>
			            </item>
			            <item superOverride='true'>
				            <displayName>Scheduled Trips</displayName>
				            <path value='/Requests/Trips/ScheduledTrips.aspx'/>
			            </item>
		            </subMenu>
	            </item>
	            <item>
		            <displayName>Company</displayName>
		            <path value='/mvc/company/view' />
		            <subMenu>
			            <item>
				            <displayName>Customers</displayName>
				            <path value='/customers/customers.aspx'/>
			            </item>
			            <item>
				            <displayName>Pilots</displayName>
				            <path value='/pilots/pilots.aspx'/>
			            </item>
			            <item>
				            <displayName>Aircraft</displayName>
				            <path value='/aircraft/Aircraft.aspx'/>
			            </item>
		            </subMenu>
	            </item>
            </menu>";
            XPathDocument testInput = CreateXmLDocument (testXml);

            //Act
            MenuParser menu = new MenuParser ("/Requests/OpenQuotes.aspx");
            bool result = menu.Parse (testInput);

            //Assert
            Assert.IsNotNull (menu);
            Assert.IsTrue (result);
        }

        [TestMethod]
        public void EmptyMenuParseSuccessAndReturnsFalse()
        {
            //Arrange
            string testXml = @"
                <menu>
                </menu>";
            XPathDocument testInput = CreateXmLDocument (testXml);

            //Act
            MenuParser menu = new MenuParser ("/Default.aspx");
            bool result = menu.Parse (testInput);

            //Assert
            Assert.IsNotNull (menu);
            Assert.IsFalse (result);
        }

        [TestMethod]
        public void SingleMenuParseSuccess()
        {
            //Arrange
            string testXml = @"
                <menu>
	                <item>
		                <displayName>Home</displayName>
		                <path value='/Default.aspx'/>
	                </item>
                </menu>";
            XPathDocument testInput = CreateXmLDocument (testXml);

            //Act
            MenuParser menu = new MenuParser ("/Default.aspx");
            bool result = menu.Parse (testInput);

            //Assert
            Assert.IsNotNull (menu);
            Assert.IsTrue (result);
        }

        [TestMethod]
        public void SingleMenuParseNoDefaultSuccess()
        {
            //Arrange
            string testXml = @"
                <menu>
	                <item>
		                <displayName>Home</displayName>
		                <path value='/Default.aspx'/>
	                </item>
                </menu>";
            XPathDocument testInput = CreateXmLDocument (testXml);

            //Act
            MenuParser menu = new MenuParser ("");
            bool result = menu.Parse (testInput);

            //Assert
            Assert.IsNotNull (menu);
            Assert.IsTrue ( result );
            Assert.IsFalse ( menu.Items [0].Active );
        }

        [TestMethod]
        public void ActiveMenuRootAndThreeDeepSuccess()
        {
            //Arrange
            // To test for active paths in root and submenus
            string testXml = @"
                <menu>
	                <item>
		                <displayName>Home</displayName>
		                <path value='/Default.aspx'/>
	                </item>
	                <item>
		                <displayName>Trips</displayName>
		                <path value='/Requests/Quotes/CreateQuote.aspx'/>
		                <subMenu>
			                <item superOverride='true'>
				                <displayName>Scheduled Trips</displayName>
				                <path value='/Requests/Trips/ScheduledTrips.aspx'/>
			                </item>
			                <item>
				                <displayName>Open Quotes</displayName>
				                <path value='/Requests/OpenQuotes.aspx'/>
		                        <subMenu>
			                        <item superOverride='true'>
				                        <displayName>Scheduled Trips</displayName>
				                        <path value='/Requests/Trips/ScheduledTrips.aspx'/>
			                        </item>
			                        <item>
				                        <displayName>Open Quotes at leaf</displayName>
		                                <path value='/Default.aspx'/>
			                        </item>
		                        </subMenu>
			                </item>
		                </subMenu>
	                </item>
                </menu>";
            XPathDocument testInput = CreateXmLDocument (testXml);

            //Act
            MenuParser menu = new MenuParser ("/Default.aspx");
            bool result = menu.Parse (testInput);
            List<MenuNode> found = menu.FindAll (x => true == x.Active);

            //Assert
            Assert.IsNotNull (menu);
            Assert.IsTrue ( 2 == found.Count );
            Assert.IsTrue ( found[0].Name.Equals ("Home") && found[1].Name.Equals ("Open Quotes at leaf") );
        }

        [TestMethod]
        public void FindAtThreeDeepSuccess()
        {
            //Arrange
            // To test for matching paths in root and submenus
            string testXml = @"
                <menu>
	                <item>
		                <displayName>Home</displayName>
		                <path value='/Default.aspx'/>
	                </item>
	                <item>
		                <displayName>Trips</displayName>
		                <path value='/Requests/Quotes/CreateQuote.aspx'/>
		                <subMenu>
			                <item superOverride='true'>
				                <displayName>Scheduled Trips</displayName>
				                <path value='/Requests/Trips/ScheduledTrips.aspx'/>
			                </item>
			                <item>
				                <displayName>Open Quotes</displayName>
				                <path value='/Requests/OpenQuotes.aspx'/>
		                        <subMenu>
			                        <item superOverride='true'>
				                        <displayName>Scheduled Trips</displayName>
				                        <path value='/Requests/Trips/ScheduledTrips.aspx'/>
			                        </item>
			                        <item>
				                        <displayName>Open Quotes at leaf</displayName>
		                                <path value='/Default.aspx'/>
			                        </item>
		                        </subMenu>
			                </item>
		                </subMenu>
	                </item>
                </menu>";
            XPathDocument testInput = CreateXmLDocument (testXml);

            //Act
            MenuParser menu = new MenuParser ("/Requests/Trips/ScheduledTrips.aspx");
            bool result = menu.Parse (testInput);
            List<MenuNode> found = menu.FindAll (x => x.Path.Equals ("/Requests/Trips/ScheduledTrips.aspx"));

            //Assert
            // The following is not an idea Unit test construction because it assumes knowledge of what is being tested.
            // That is a fault of the abstraction being incomplete. This is solved by providing an acces abstraction,
            // such as an iterator so the individual menu items can be retrieved with operations. too complex for this
            // code exercise but would definatately be needed for production use - just in general.
            Assert.IsNotNull (menu);
            Assert.IsTrue ( 2 == found.Count );
            Assert.AreSame ( menu.Items [1].SubMenu[0], found[0] );
            Assert.AreSame ( menu.Items [1].SubMenu[1].SubMenu[0], found[1] );
        }
        
    }

    [TestClass]
    public class MenuNodeTests
    {
         [TestMethod]
        public void MenuNodeFindSuccess()
        {
            //Arrange
            // Test the node's ability to find items in the submenu
            MenuNode testNode = new MenuNode ()
            {
                Name = "First item",
                Active = false,
                Path = "/root",
                SubMenu = new List<MenuNode> ()
                {
                    new MenuNode ()
                    {
                        Name = "Submenu First item",
                        Active = false,
                        Path = "/root/submenu/item1",
                        SubMenu = new List<MenuNode> ()
                    },
                    new MenuNode ()
                    {
                        Name = "Submenu Second item",
                        Active = false,
                        Path = "/root/submenu/item2",
                        SubMenu = new List<MenuNode> ()
                    }
                }
            };

            //Act
            List<MenuNode> found = testNode.FindAll (x => x.Name.Equals ("Submenu Second item"));

            //Assert
            Assert.IsTrue ( 1 == found.Count );
            Assert.AreSame ( testNode.SubMenu[1], found[0] );
        }

        [TestMethod]
        public void MenuNodeFindFails()
        {
            //Arrange
            // Test the node's ability to find items in the submenu
            MenuNode testNode = new MenuNode ()
            {
                Name = "First item",
                Active = false,
                Path = "/root",
                SubMenu = new List<MenuNode> ()
                {
                    new MenuNode ()
                    {
                        Name = "Submenu First item",
                        Active = false,
                        Path = "/root/submenu/item1",
                        SubMenu = new List<MenuNode> ()
                    },
                    new MenuNode ()
                    {
                        Name = "Submenu Second item",
                        Active = false,
                        Path = "/root/submenu/item2",
                        SubMenu = new List<MenuNode> ()
                    }
                }
            };

            //Act
            List<MenuNode> found = testNode.FindAll (x => x.Name.Equals ("garbage"));

            //Assert
            Assert.IsFalse ( found.Count > 0 );
        }

        [TestMethod]
        public void MenuNodeFindToDepthSuccess()
        {
            //Arrange
            // Test the node's ability to find items in the submenu
            MenuNode testNode = new MenuNode ()
            {
                Name = "First item",
                Active = false,
                Path = "/root",
                SubMenu = new List<MenuNode> ()
                {
                    new MenuNode ()
                    {
                        Name = "Submenu First item",
                        Active = false,
                        Path = "/root/submenu/item1",
                        SubMenu = new List<MenuNode> ()
                    },
                    new MenuNode ()
                    {
                        Name = "Submenu Second item",
                        Active = false,
                        Path = "/root/submenu/item2",
                        SubMenu = new List<MenuNode> ()
                        {
                            new MenuNode ()
                            {
                                Name = "SubSubmenu First item",
                                Active = false,
                                Path = "/root/submenu2/item1",
                                SubMenu = new List<MenuNode> ()
                                {
                                    new MenuNode ()
                                    {
                                        Name = "SubSubSubmenu First item",
                                        Active = false,
                                        Path = "/root/submenu3/item1",
                                        SubMenu = new List<MenuNode> ()
                                    },
                                    new MenuNode ()
                                    {
                                        Name = "SubSubSubmenu Second item",
                                        Active = false,
                                        Path = "/root/submenu3/item2",
                                        SubMenu = new List<MenuNode> ()
                                    }
                                }
                            },
                            new MenuNode ()
                            {
                                Name = "SubSubmenu Second item",
                                Active = false,
                                Path = "/root/submenu2/item2",
                                SubMenu = new List<MenuNode> ()
                            }
                        }
                    }
                }
            };

            //Act
            List<MenuNode> found = testNode.FindAll (x => x.Path.Equals ("/root/submenu3/item1"));

            //Assert
            Assert.IsTrue ( 1 == found.Count );
            Assert.AreSame ( testNode.SubMenu[1].SubMenu[0].SubMenu[0], found[0] );
        }
        
        [TestMethod]
        public void MenuNodeFindEverythingSuccess()
        {
            //Arrange
            // Test the node's ability to find items in the submenu
            MenuNode testNode = new MenuNode ()
            {
                Name = "First item",
                Active = false,
                Path = "/root",
                SubMenu = new List<MenuNode> ()
                {
                    new MenuNode ()
                    {
                        Name = "Submenu First item",
                        Active = false,
                        Path = "/root/submenu/item1",
                        SubMenu = new List<MenuNode> ()
                    },
                    new MenuNode ()
                    {
                        Name = "Submenu Second item",
                        Active = false,
                        Path = "/root/submenu/item2",
                        SubMenu = new List<MenuNode> ()
                        {
                            new MenuNode ()
                            {
                                Name = "SubSubmenu First item",
                                Active = false,
                                Path = "/root/submenu2/item1",
                                SubMenu = new List<MenuNode> ()
                                {
                                    new MenuNode ()
                                    {
                                        Name = "SubSubSubmenu First item",
                                        Active = false,
                                        Path = "/root/submenu3/item1",
                                        SubMenu = new List<MenuNode> ()
                                    },
                                    new MenuNode ()
                                    {
                                        Name = "SubSubSubmenu Second item",
                                        Active = false,
                                        Path = "/root/submenu3/item2",
                                        SubMenu = new List<MenuNode> ()
                                    }
                                }
                            },
                            new MenuNode ()
                            {
                                Name = "SubSubmenu Second item",
                                Active = false,
                                Path = "/root/submenu2/item2",
                                SubMenu = new List<MenuNode> ()
                            }
                        }
                    }
                }
            };

            //Act
            List<MenuNode> found = testNode.FindAll (x => false == x.Active);

            //Assert
            Assert.IsTrue ( 6 == found.Count );
        }
   }
}
