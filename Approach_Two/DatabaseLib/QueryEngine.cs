///////////////////////////////////////////////////////////////
// QueryEngine.cs - define methods for querying from database//
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
 *  findValue<Key, Value, Data> - Find the DBElement of specified key.
 * 
 *  getChildren<Key, Value, Data> - Get children in the DBElement of a specified key.
 *
 *  get_keys_with_metadata<Key, Value, Data> - Get keys of all DBElements containing a particular pattern in metadata.
 *
 *  get_keys_within_timeInterval<Key, Value, Data> - Get all keys written within specified time interval.
 *
 *  *********************For new method*******************
 *
 * Package Operations:
 * --------------------
 *  This package implements methods that are used to query for particular 
 *  items in the database. Methods either return DBElement, or
 *  specific contents of the element or set of keys matching a condition.
 */
/*
 * Maintenance:
 * ------------
 * Required Files: QueryEngine.cs, DBElement.cs, DBEngine.cs, 
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
    public class QueryEngine
    {
        //-------< Find the DBElement of a particular key. >---------------------------
        public DBElement<Key,Data> findValue<Key, Value, Data>(DBEngine<Key, Value> db, Key key)
        {
            Value val;
            if (db.getValue(key, out val))
                return val as DBElement<Key, Data>;
            else
                return default(DBElement<Key, Data>);    
        }

        //-------< Get the children list in DBelement of a particular key. >--------------
        public List<Key> getChildren<Key, Value, Data>(DBEngine<Key, Value> db, Key key)
        {
            Value val;
            List<Key> ret = new List<Key>();
            if(db.getValue(key, out val))
            {
                DBElement<Key, Data> elem = val as DBElement<Key, Data>;
                return elem.children;
            }
            return default(List<Key>);
        }

        //-------< Get keys of DBElements containing a pattern in the metadata section >-------------
        public List<Key> get_keys_with_metadata<Key, Value, Data>(DBEngine<Key, Value> db, string pat)
        {
            List<Key> key_collection = new List<Key>();           
            foreach (Key key in db.Keys())
            {
                Value value;
                db.getValue(key, out value);
                DBElement<Key, Data> elem = value as DBElement<Key, Data>;
                if (elem.name.Contains(pat) || elem.descr.Contains(pat) || elem.timeStamp.ToString().Contains(pat))
                    key_collection.Add(key);
                else if (elem.children.Count() > 0)
                {
                    foreach (Key x in elem.children)
                        if (x.ToString().Contains(pat))
                            key_collection.Add(key);
                }   
            }
            if (key_collection.Count > 0)
                return key_collection;
            else
                return default(List<Key>);
        }

        //-------< Get keys of DBElements written within a specified time interval >-------------
        public List<Key> get_keys_within_timeInterval<Key, Value, Data>(DBEngine<Key,Value> db, DateTime t1, DateTime t2 = default(DateTime))
        {
            List<Key> key_collection = new List<Key>();
            if (t2 == default(DateTime))
                t2 = DateTime.Now;
            foreach (Key key in db.Keys())
            {
                Value value;
                db.getValue(key, out value);
                DBElement<Key, Data> elem = value as DBElement<Key, Data>;
                if (DateTime.Compare(elem.timeStamp,t1) >= 0 && DateTime.Compare(elem.timeStamp,t2) <= 0)
                    key_collection.Add(key);
            }
            return key_collection;
        }

        //-------< Get all keys that start with a specified pattern which defaults to all keys >-------------
        public List<Key> get_keys_with_pattern<Key, Value, Data>(DBEngine<Key,Value> db, string pat)
        {
            List<Key> key_collection = new List<Key>();
            List<Key> all_keys = new List<Key>();
            foreach (Key key in db.Keys())
            {
                all_keys.Add(key);
                if (key.ToString().StartsWith(pat))
                    key_collection.Add(key);
            }
            if (key_collection.Count > 0)
                return key_collection;
            else
                return all_keys;
        }
        
    }



#if(TEST_QUERYENGINE)
    public class Test_QueryEngine
    {
        static void Main(string[] args)
        {
            "Testing QueryEngine Package".title('=');
            WriteLine();
            QueryEngine sendQuery = new QueryEngine();
            
            DBElement<int, string> elem1 = new DBElement<int, string>("Element-1", "Description of Element-1");
            elem1.payload = "Payload of element-1.";
            elem1.children.AddRange(new List<int> { 9, 10, 11, 79 });
            DBElement<int, string> elem2 = new DBElement<int, string>("Element-2", "Description of Element-2");
            elem2.payload = "Payload of element-2.";
            DBElement<int, string> elem3 = new DBElement<int, string>("Element-3", "Description of Element-3");
            elem3.payload = "Payload of element-3.";

            DBEngine<int, DBElement<int, string>> db = new DBEngine<int, DBElement<int, string>>();
            db.insert(1, elem1);
            db.insert(2, elem2);
            db.insert(3, elem3);
            db.showDB();

            DBElement<int, string> result_elem = new DBElement<int, string>();
            DBElement<int, string> result_elem2 = new DBElement<int, string>();
            //DBElement<int, string> result_elem = new DBElement<int, string>();
            Write("\n\n  Send query -> Find element with key = 2");
            
            result_elem = sendQuery.findValue<int, DBElement<int, string>, string>(db, 2);
            if (result_elem == null)
                WriteLine("\n  Element with Key= 2 was not found in the database.");
            else
                result_elem.showElement();
           
            Write("\n\n  Send query -> Find element with key = 5");
            result_elem = sendQuery.findValue<int, DBElement<int, string>, string>(db, 5);
            if (result_elem == null)
                WriteLine("\n  Element with Key= 5 was not found in the database.");
            else
                result_elem.showElement();

            Write("\n  Send query -> Get children of element with key = 1");
            List<int> x1 = sendQuery.getChildren<int, DBElement<int, string>, string>(db, 1);
            Write("\n  Children of element with key=1 are ");
            foreach (int i in x1) Write("{0} ", i);

            Write("\n\n  Send query -> Get children of element with key = 3");
            List<int> y1 = sendQuery.getChildren<int, DBElement<int, string>, string>(db, 3);
            if (y1.Count() > 0)
            {
                Write("\n  Children of element with key=3 are ");
                foreach (int i in y1) Write("{0} ", i);
            }
            else
                WriteLine("\n  Element with key=3 has no children.");

            WriteLine("\n  Send query -> Get keys with string -\"blah\" in metadata section.");
            List<int> m1 = sendQuery.get_keys_with_metadata<int, DBElement<int, string>, string>(db, "18779");
            if (m1.Count() > 0)
            {
                Write("  Keys that contain the string \"ent-2\" in their metadata section are: ");
                foreach (int i in m1) Write("{0} ", i);
            }
            else
                WriteLine("  No keys found having \"ent-2\" in their metadata section.");
            
            //WriteLine("Type of x = {0}", x.ToString());
            

            
            //----< Test query methods for DBEngine<string, DBElement<string, List<string>>>  >----------
            Write("\n\n --- Test Query methods for DBEngine<string, DBElement<string,List<string>>> ---");
            DBElement<string, List<string>> new_elem1 = new DBElement<string, List<string>>("Element-One", "Description of Element-One");
            new_elem1.payload = new List<string> { "First string in payload of Element-One", "Second string in payload of Element-One", "Third string" };
            new_elem1.children.AddRange(new List<string> { "Nine", "Ten", "Eleven" });
            System.Threading.Thread.Sleep(1200);
            DateTime tm1 = DateTime.Now;
            System.Threading.Thread.Sleep(1200);
            DBElement<string, List<string>> new_elem2 = new DBElement<string, List<string>>("Element-Two", "Description of Element-Two");
            new_elem2.payload = new List<string> { "First string in payload of Element-Two", "Mars", "Venus" };
            System.Threading.Thread.Sleep(1200);
            DBElement<string, List<string>> new_elem3 = new DBElement<string, List<string>>("Element-Three", "Description of Element-Three");
            new_elem3.payload = new List<string> { "First string in payload of element-3", "Beta", "Gamma" };

            DBEngine<string, DBElement<string, List<string>>> new_db = new DBEngine<string, DBElement<string, List<string>>>();
            new_db.insert("One", new_elem1);
            new_db.insert("Two", new_elem2);
            new_db.insert("Three", new_elem3);
            
            DateTime tm2 = DateTime.Now;
            new_db.showEnumerableDB();

            Write("\n\n  Send query -> Find element with key = One");
            if (sendQuery.findValue<string, DBElement<string, List<string>>, List<string>>(new_db, "One") == null)
                WriteLine("\n  Element with Key= \"One\" was not found in the database.");
            else
                sendQuery.findValue<string, DBElement<string, List<string>>, List<string>>(new_db, "One").showEnumerableElement();

            Write("\n\n  Send query -> Find element with key = Five");
            if (sendQuery.findValue<string, DBElement<string, List<string>>, List<string>>(new_db, "Five") == null)
                WriteLine("\n  Element with Key= \"Five\" was not found in the database.");
            else
                sendQuery.findValue<string, DBElement<string, List<string>>, List<string>>(new_db, "Five").showEnumerableElement();

            Write("\n  Send query -> Get children of element with key = \"One\"");
            //List<int> x = sendQuery.getChildren<int, DBElement<int, string>, string>(db, 1);
            List<string> x = sendQuery.getChildren<string, DBElement<string, List<string>>, List<string>>(new_db, "One");
            if(x.Count() > 0)
            {
                Write("\n  Children of element with key=\"One\" are ");
                foreach (string i in x) Write("{0} ", i);
            }
            else
                WriteLine("\n  Element with key=\"One\"  has no children.");


            Write("\n\n  Send query -> Get children list of element with key = \"Three\"");
            //List<int> x = sendQuery.getChildren<int, DBElement<int, string>, string>(db, 1);
            List<string> y = sendQuery.getChildren<string, DBElement<string, List<string>>, List<string>>(new_db, "Three");
            if (y.Count > 0)
            {
                Write("\n  Children of element with key=\"Three\" are ");
                foreach (string i in y) Write("{0} ", i);
            }
            else
                WriteLine("\n  Element with key=\"Three\"  has no children.");

            List<string> search_pat = new List<string> { "UnKnown", "lement" };
            foreach (string pattern_in_metadata in search_pat)
            {
                WriteLine("\n  Send query -> Get all keys containing string - \"{0}\" in their metadata section.", pattern_in_metadata);
                List<string> res1 = sendQuery.get_keys_with_metadata<string, DBElement<string, List<string>>, List<string>>(new_db, pattern_in_metadata);
                if (res1.Count > 0)
                {
                    Write("  Keys containing string \"{0}\" in their metadata section are: ", pattern_in_metadata);
                    foreach (string i in res1)
                        Write("{0} ", i);
                }
                else
                    WriteLine("  No keys found containing pattern \"{0}\" in their metadata section.", pattern_in_metadata);
            }

            WriteLine("\n\n  Send query -> Get all keys written within time intervals \"{0}\" and \"{1}\"",tm1,tm2);
            List<string> result1 = sendQuery.get_keys_within_timeInterval<string, DBElement<string, List<string>>, List<string>>(new_db, tm1);
            if (result1.Count > 0)
            {
                Write("  Keys written within specified time intervals are: ");
                //Write("  Keys are: ");
                foreach (string i in result1)
                    Write("{0} ", i);
            }
            else
                WriteLine("  No keys found written withing specified time interevals tm1 and tm2.");
            //WriteLine("tm1 = {0}\ntm2 = {1}", tm1, tm2);

        }
    }
#endif
}
