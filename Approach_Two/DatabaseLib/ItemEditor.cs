///////////////////////////////////////////////////////////////
// ItemEditor.cs - define extensions methods for Editing     //
// Ver 1.0                                                   //
// Application: Demonstration for CSE681-SMA, Project#2      //
// Language:    C#, ver 6.0, Visual Studio 2015              //
// Platform:    Dell Inspiron 15, Core-i5, Windows 10        //
// Author:      Suhas Kamasetty Ramesh                       //
//              MS Computer Engineering, Syracuse University //
//              (315) 278-3888 skamaset@syr.edu              //
///////////////////////////////////////////////////////////////
/*
 * Public Interface:
 * -----------------
 *  addRelation<Key, Value, Data> - Add a key to the children list of a particular DB-element.
 *
 *  removeRelation<Key, Value, Data> - Remove a paricular key from the children list of a DB-element.
 *
 *  editName<Key, Value, Data> - Edit name of a particular DB-element
 *
 *  editDescr<Key, Value, Data> - Edit description of a particular DB-element
 *
 *  editInstance<Key, Value, Data> - Replace an existing DB element's payload with new payload.
 *
 *
 * Package Operations:
 * --------------------
 *  This package implements the extension methods
 *  to support editing of DB elements.
 *  This package is used to edit the name, description,
 *  children list and payload of the DBElement. 
 */
/*
 * Maintenance:
 * ------------
 * Required Files: ItemEditor.cs, DBElement.cs, DBEngine.cs
 *                 Display.cs, UtilityExtensions.cs, DBExtensions.cs
 *
 * Build Process:  devenv ProjectTwo.sln /Rebuild debug
 *                 Run from Developer Command Prompt
 *                 To find: search for developer
 *
 * Maintenance History
 * --------------------
 * ver 1.0 : 9 Oct 2015
 * First release
 *
 */
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Console;

namespace Project4Starter
{
    // Extension method class
    //  -These methods are all extending the DBEngine<Key,Value> class,
    //   where Value = DBElement<Key,Data>
    public static class ItemEditor
    {
        //-------< Add child relation to a particular DB-element >---------------------------
        public static bool addRelation<Key, Value, Data>(this DBEngine<Key, Value> db_edit, Key key1, Key key2)
        {
            Value val1, val2;
            bool key2_present = db_edit.getValue(key2, out val2);
            if (key2_present)
            {
                bool key1_present = db_edit.getValue(key1, out val1);
                if (key1_present)
                {
                    DBElement<Key, Data> elem = val1 as DBElement<Key, Data>;
                    elem.children.Add(key2);
                    elem.timeStamp = DateTime.Now;
                    return true;
                }
                else
                    return false;
            }
            else
                return false;
        }

        //-------< Remove child relation of a particular DB-element >--------------------
        public static bool removeRelation<Key, Value, Data>(this DBEngine<Key, Value> db_edit, Key key1, Key key2)
        {
            Value value;
            if (db_edit.getValue(key1, out value))
            {
                DBElement<Key, Data> elem = value as DBElement<Key, Data>;
                if (elem.children.Contains(key2))
                {
                    elem.children.Remove(key2);
                    elem.timeStamp = DateTime.Now;
                    return true;
                }
                else
                    return false;
            }
            else
                return false;    
        }

        //-------< Edit name of a particular DB-element >--------------------
        public static bool editName<Key, Value, Data>(this DBEngine<Key, Value> db_edit, Key key1, string new_name)
        {
            Value value;
            if (db_edit.getValue(key1, out value))
            {
                DBElement<Key, Data> elem = value as DBElement<Key, Data>;
                elem.name = new_name;
                elem.timeStamp = DateTime.Now;
                return true;
            }
            else
                return false;
        }

        //-------< Edit description of a particular DB-element >--------------------
        public static bool editDescr<Key, Value, Data>(this DBEngine<Key, Value> db_edit, Key key1, string new_descr)
        {
            Value value;
            if (db_edit.getValue(key1, out value))
            {
                DBElement<Key, Data> elem = value as DBElement<Key, Data>;
                elem.descr = new_descr;
                elem.timeStamp = DateTime.Now;
                return true;
            }
            else
                return false;
        }

        //-------< Replace payload of a particular DB-element with a new payload.>--------------------
        public static bool editInstance<Key, Value, Data>(this DBEngine<Key, Value> db_edit, Key key1, Data new_instance)
        {
            Value value;
            if (db_edit.getValue(key1, out value))
            {
                DBElement<Key, Data> elem = value as DBElement<Key, Data>;
                elem.payload = new_instance;
                elem.timeStamp = DateTime.Now;
                return true;
            }
            else
                return false;
        }
    }

#if (TEST_ITEMEDITOR)

    //-------------------------------< Test Stub >--------------------------------------
    class TestItemEditor
    {
        static void Main(string[] args)
        {
            "Testing ItemEditor Package".title('=');
            WriteLine();

            Write("\n --- Test DBElement<int,string> ---");

            DBElement<int, string> elem1 = new DBElement<int, string>("Element-1", "Description of Element-1");
            elem1.payload = "Payload of element-1.";
            elem1.children.AddRange(new List<int> { 9, 10, 11 });
            DBElement<int, string> elem2 = new DBElement<int, string>("Element-2", "Description of Element-2");
            elem2.payload = "Payload of element-2.";
            DBElement<int, string> elem3 = new DBElement<int, string>("Element-3", "Description of Element-3");
            elem3.payload = "Payload of element-3.";

            DBEngine<int, DBElement<int, string>> db = new DBEngine<int, DBElement<int, string>>();
            db.insert(1, elem1);
            db.insert(2, elem2);
            db.insert(3, elem3);
            db.showDB();

            Write("\n\n  Going to test adding of relationship to DB-elements: Element-1 and Element-2");
            bool add1 = db.addRelation<int, DBElement<int, string>, string>(1, 2);
            bool add2 = db.addRelation<int, DBElement<int, string>, string>(2, 3);
            bool add3 = db.addRelation<int, DBElement<int, string>, string>(2, 17);
            //add3 will be equal to false because we cannot add a child key(17) if it is not present in the database.

            db.showDB();
            if (add1 && add2 && add3)
                Write("\n  Adding relationship to all items successded.");
            else
                Write("\n  Adding relationship failed in one of the cases.");

            Write("\n\n  Now going to test removing of relationship in DB-element: Element-1.");
            bool rem1 = db.removeRelation<int, DBElement<int, string>, string>(1, 10);
            bool rem2 = db.removeRelation<int, DBElement<int, string>, string>(1, 15);
            //The above case will fail because key-15 is not present in children list of Element-1.

            db.showDB();
            if (rem1 && rem2)
                Write("\n  Deleting of relationship to both items successded.");
            else
                Write("\n  Deleting of relationship failed in one of the cases.");
            

            Write("\n\n  Now going to test edition of name, description and replacing instance of payload with new instance in element1.");
            bool ed_name1 = db.editName<int, DBElement<int, string>, string>(1, "Elemen1_Renamed.");
            bool ed_descr = db.editDescr<int, DBElement<int, string>, string>(1, "New description for element 1.");
            bool ed_inst = db.editInstance<int, DBElement<int, string>, string>(1, "New instance of payload for element-1.");
            db.showDB();

            Write("\n\n --- Test DBElement<string,List<string>> ---");
            DBElement<string, List<string>> new_elem1 = new DBElement<string, List<string>>("Element-One", "Description of Element-One");
            new_elem1.payload = new List<string> { "First string in payload of Element-One", "Second string in payload of Element-One", "Third string" };
            new_elem1.children.AddRange(new List<string> { "Nine", "Ten", "Eleven" });
            DBElement<string, List<string>> new_elem2 = new DBElement<string, List<string>>("Element-Two", "Description of Element-Two");
            new_elem2.payload = new List<string> { "First string in payload of Element-Two", "Mars", "Venus" } ;
            DBElement<string, List<string>> new_elem3 = new DBElement<string, List<string>>("Element-Three", "Description of Element-Three");
            new_elem3.payload = new List<string> { "First string in payload of element-3", "Beta", "Gamma" };

            DBEngine<string, DBElement<string, List<string>>> new_db = new DBEngine<string, DBElement<string, List<string>>>();
            new_db.insert("One", new_elem1);
            new_db.insert("Two", new_elem2);
            new_db.insert("Three", new_elem3);
            new_db.showEnumerableDB();

            Write("\n\n  Going to test adding of relationship to DB-elements: Element-One and Element-Two");
            bool a1 = new_db.addRelation<string, DBElement<string, List<string>>, List<string>>("One", "Two");
            bool a2 = new_db.addRelation<string, DBElement<string, List<string>>, List<string>>("Two", "Three");
            bool a3 = new_db.addRelation<string, DBElement<string, List<string>>, List<string>>("Three", "Not_presnet");
            //a3 will be false because we are trying to add a key that is not present in the database.

            new_db.showEnumerableDB();
            if (a1 && a2 && a3)
                Write("\n  Adding relationship to all items successded.");
            else
                Write("\n  Adding relationship failed in one of the cases.");
            

            Write("\n\n  Now going to test removing of relationships in DB-element: Element-One");
            bool r1 = new_db.removeRelation<string, DBElement<string, List<string>>, List<string>>("One", "Nine");
            bool r2 = new_db.removeRelation<string, DBElement<string, List<string>>, List<string>>("One", "Not_present");
            //r2 will be false because we are trying to remove a relationship in element one which is not even presnet.

            new_db.showEnumerableDB();
            if (r1 && r2)
                Write("\n  Deleting of relationships successded in both cases.");
            else
                Write("\n  Deleting of relationship failed in one of the cases.");
            

            Write("\n\n  Now going to test edition of name, description and replacing instance of payload with new instance in Element-One.");
            new_db.editName<string, DBElement<string, List<string>>, List<string>>("One", "Edited name for Element-One");
            new_db.editDescr<string, DBElement<string, List<string>>, List<string>>("One", "New description for Element-One");
            new_db.editInstance<string, DBElement<string, List<string>>, List<string>>("One", new List<string> { "New payload - String One", "New payload - String Two", "New payload - String Three" });
            new_db.showEnumerableDB();
            WriteLine();
        }
    }
#endif
}
