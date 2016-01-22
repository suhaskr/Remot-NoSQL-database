///////////////////////////////////////////////////////////////
// DBEngine.cs - define noSQL database                       //
// Ver 1.3                                                   //
// Application: Demonstration for CSE681-SMA, Project#2      //
// Language:    C#, ver 6.0, Visual Studio 2015              //
// Platform:    Dell Inspiron 15, Core-i5, Windows 10        //
// Author:      Suhas Kamasetty Ramesh                       //
//              MS Computer Engineering, Syracuse University //
//              (315) 278-3888 skamaset@syr.edu              //
// Source:      Jim Fawcett, CST 4-187, Syracuse University  //
//              (315) 443-3948, jfawcett@twcny.rr.com        //
///////////////////////////////////////////////////////////////
/*
 * Public Interface:
 * -----------------
 *  bool insert(Key,Value) - Will insert the key-value pair to the existing dictionary. 
 *  
 *  bool getValue(Key, out Value) - Used to find the value of a particular key.
 *
 *  IEnumerable<Key> Keys() - Returns set of all keys in the dictionary.
 *
 *  bool remove(Key) - Will remove the entry for a particular key from dictionary.
 *
 * Package Operations:
 * -------------------
 * This package implements DBEngine<Key, Value> where Value
 * is the DBElement<key, Data> type.
 *
 * DBEngine<Key, Value> is a instance of the dictionary that stores
 *   all the Key-Value pairs.
 */
/*
 * Maintenance:
 * ------------
 * Required Files: DBEngine.cs, DBElement.cs, and
 *                 UtilityExtensions.cs only if you enable the test stub
 *
 * Build Process:  devenv ProjectTwo.sln /Rebuild debug
 *                 Run from Developer Command Prompt
 *                 To find: search for developer
 *
 * Maintenance History:
 * --------------------
 * ver 1.3 : 9 Oct 15
 * - added method to remove elements from database, changed namespace
 * - added comments to methods, added public interface
 * ver 1.2 : 24 Sep 15
 * - removed extensions methods and tests in test stub
 * - testing is now done in DBEngineTest.cs to avoid circular references
 * ver 1.1 : 15 Sep 15
 * - fixed a casting bug in one of the extension methods
 * ver 1.0 : 08 Sep 15
 * - first release
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
    /////////////////////////////////////////////////////////////////////
    // DBEngine<Key, Value> class
    // where Value is DBElement<Key, Data>
    // - Key and Value are unspecified classes, to be supplied by the
    //   application that uses the noSQL database.
    //   See the teststub present in DBEngineTest for examples of use.

    public class DBEngine<Key, Value>
    {
        private Dictionary<Key, Value> dbStore;

        //----< Construtor of DBEngine will create a dictionary for storing key/value pairs >------
        public DBEngine()
        {
            dbStore = new Dictionary<Key, Value>();
        }

        //-------< Method will insert the key-val pair to the existing dictionary >------
        public bool insert(Key key, Value val)
        {
            if (dbStore.Keys.Contains(key))
                return false;
            dbStore[key] = val;
            return true;
        }

        //-------< Search for key in dictionary, and return the value >------
        public bool getValue(Key key, out Value val)
        {
            if (dbStore.Keys.Contains(key))
            {
                val = dbStore[key];
                return true;
            }
            val = default(Value);
            return false;

        }

        //-------< Returns set of all keys in the dictionary. >------
        public IEnumerable<Key> Keys()
        {
            return dbStore.Keys;
        }

        //-------< Method will delete entry with matching key in the dictionary. >------
        public bool remove(Key key)
        {
            if (!dbStore.Keys.Contains(key))
                return false;
            dbStore.Remove(key);
            return true;
        }
    }

#if (TEST_DBENGINE)

    class TestDBEngine
    {
        static void Main(string[] args)
        {
            "Testing DBEngine Package".title('=');
            WriteLine();

            Write("\n  All testing of DBEngine class moved to DBEngineTest package.");
            Write("\n  This allow use of DBExtensions package without circular dependencies.");

            Write("\n\n");
        }
    }
#endif
}
