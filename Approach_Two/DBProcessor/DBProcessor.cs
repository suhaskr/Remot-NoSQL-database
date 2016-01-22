///////////////////////////////////////////////////////////////
// DBProcessor.cs - Methods for processing request messages  //
//                   and performing action on DB.            //
// Ver 1.0                                                   //
// Application: Demonstration for CSE681-SMA, Project#4      //
// Language:    C#, ver 6.0, Visual Studio 2015              //
// Platform:    Dell Inspiron 15, Core-i5, Windows 10        //
// Author:      Suhas Kamasetty Ramesh                       //
//              MS Computer Engineering, Syracuse University //
//              (315) 278-3888 skamaset@syr.edu              //
///////////////////////////////////////////////////////////////
/*
 * Public Interface:
 * -----------------
 *  process_msg<Key, Value, Data> - Dedtermines which operation needs to be done.
 *
 *  insert_elem<Key, Data> - performs insert operation on database and generates response msg accordingly.
 *
 *  delete_elem<Key, Data> - performs delete operation on DB and generates response msg accordingly.
 *
 *   edit_name<Key, Data>  - edits name of DBElement of a particular key.
 *
 *   add_children<Key, Data> - Adds child relation to a DBElement.
 * 
 *   get_value<Key, Data> - Retrieves DBElement from database.
 *
 *   get_children<Key,Data> - Retrieves children of a particular DBElement.
 *
 *   keys_metada<Key, Data> - Retrieves keys of all DBElements with a "pattern" in metadata section.
 *
 *  persist_database<Key, Data> - Saves the database contents into an XML file.
 *
 *   load_database<Key, Data> - Loads the database from saved XML files.
 *
 * Package Operations:
 * --------------------
 *  This package implements methods that perform operations 
 *  on database and generate response message. 
 */
/*
 * Maintenance:
 * ------------
 * Required Files: Database files of projectTwo present in a class library.(DatabaseLib)
 *                 
 *
 * Build Process:  devenv Project4Starter.sln /Rebuild debug
 *                 Run from Developer Command Prompt
 *                 To find: search for developer
 *
 * Maintenance History
 * --------------------
 * ver 1.0 : 22 Nov 2015
 * First release
 *
 */


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Threading;

namespace Project4Starter
{
    public class DBProcessor
    {
        QueryEngine sendQuery = new QueryEngine();
        public XElement process_msg<Key, Value, Data>(string msg, DBEngine<Key, DBElement<Key,Data>> db)
        {
            XElement request_msg = XElement.Parse(msg);
            XElement response = new XElement("Result","Request type {0} not found.", request_msg.Element("Request_Type").Value); 
            //Determine which methods needs to be called. (Based on the Request_Type tag)

            string method = request_msg.Element("Request_Type").Value;
            if (method == "Insert")
                response = insert_elem<Key, Data>(request_msg, db);
                                               
            if (method == "Delete")           
                response = delete_elem<Key, Data>(request_msg, db);                
            
            if (method == "Edit Name")            
                response = edit_name<Key, Data>(request_msg, db);
            
            if (method == "Edit Description")           
                response = edit_description<Key, Data>(request_msg, db);
            
            if (method == "Add Children")            
                response = add_children<Key, Data>(request_msg, db);
            
            if (method == "Show DB")
            {
                Console.Write("\n  Request to display database contents:");
                db.show<Key, DBElement<Key, Data>, Data>();
                response = new XElement("Result", "Database displayed successfuly");
                Console.WriteLine();             
            }
            if (method == "Get Value")
                response = get_value<Key, Data>(request_msg, db);
            if (method == "Get Children")
                response = get_children<Key,Data>(request_msg, db);
            if (method == "Get Keys metadata")
                response = keys_metada<Key, Data>(request_msg, db);
            if (method == "Persist Database")
                response = persist_database<Key, Data>(request_msg, db);
            if (method == "Load Database")
                response = load_database<Key, Data>(request_msg, db);

            return response;    
        }

        //<------------performs insert operation on database and generates response msg accordingly
        XElement insert_elem<Key, Data>(XElement request_msg, DBEngine<Key, DBElement<Key, Data>> db)
        {
            Console.Write("\n  Request to Insert DB Element");
            XElement response_msg = new XElement("Query_Response");
            XElement type = new XElement("Query_Type", "Insert");
            XElement result = new XElement("Result","Could not insert element");
            try {
                string name = request_msg.Element("Name").Value;
                string descr = request_msg.Element("Description").Value;
                string payload = request_msg.Element("Payload").Value;
                Key k = (Key)Convert.ChangeType(request_msg.Element("Key").Value, typeof(Key));
                Console.WriteLine(" with key - {0}",k);
                DBElement<Key, Data> elem = new DBElement<Key, Data>();
                elem.name = name;
                elem.descr = descr;
                bool children_present = false;
                List<Key> child_list = new List<Key>();
                if (request_msg.Element("Children") != null)
                {
                    children_present = true;
                    foreach (var child_key in request_msg.Element("Children").Elements("Key"))                   
                        child_list.Add((Key)Convert.ChangeType(child_key.Value, typeof(Key)));                    
                }
                if (children_present)
                    elem.children = child_list;
                elem.payload = (Data)Convert.ChangeType(payload, typeof(Data));
                bool operation_result = db.insert(k, elem);
                
                if (operation_result)
                    result = new XElement("Result", "DB Element inserted successfully into database.");                
                else
                    result = new XElement("Result", "Could not insert element into database");
                response_msg.Add(type);
                response_msg.Add(result);
                return response_msg;
            }
            catch (Exception ex)
            {
                Console.WriteLine("\n  XML file for request message is not in the expected format. A particular tag could not be found.\n  Exception = {0}",ex);
                result = new XElement("Result", "Could not insert element into database");
                response_msg.Add(type);
                response_msg.Add(result);
                return response_msg;
            }           
        }

        //<------------performs delete operation on DB and generates response msg accordingly.
        XElement delete_elem<Key, Data>(XElement request_msg, DBEngine<Key, DBElement<Key, Data>> db)
        {
            Console.Write("\n  Request to Delete DB Element");
            XElement response_msg = new XElement("Query_Response");
            XElement type = new XElement("Query_Type", "Delete");
            XElement result = new XElement("Result", "Could not delete element");
            try
            {
                Key k = (Key)Convert.ChangeType(request_msg.Element("Key").Value, typeof(Key));
                Console.WriteLine(" with key - {0}", k);
                if (db.remove(k))
                    result = new XElement("Result", "DB Element deleted successfully from database.");
                else
                    result = new XElement("Result", "Could not delete element from database");
                response_msg.Add(type);
                response_msg.Add(result);
                return response_msg;               
            }
            catch (NullReferenceException ex)
            {
                Console.WriteLine("\n  XML file for request message is not in the expected format. A particular tag could not be found.\n  Exception = {0}", ex);
                response_msg.Add(type);
                response_msg.Add(result);
                return response_msg;
            }         
        }

        //<----------------------edits name of DBElement of a particular key.
        XElement edit_name<Key, Data>(XElement request_msg, DBEngine<Key, DBElement<Key, Data>> db)
        {
            Console.Write("\n  Request to Edit name of DB Element");
            XElement response_msg = new XElement("Query_Response");
            XElement type = new XElement("Query_Type", "Edit Name");
            XElement result = new XElement("Result", "Could not Edit name element");
            try
            {
                Key k = (Key)Convert.ChangeType(request_msg.Element("Key").Value, typeof(Key));
                Console.WriteLine(" with key - {0}", k);
                string new_name = request_msg.Element("Name").Value;
                bool name_edit = db.editName<Key, DBElement<Key, Data>, Data>(k, new_name);
    
                if (name_edit)
                    result = new XElement("Result", "Name of DB Element editted successfully.");
                else
                    result = new XElement("Result", "Could not edit name of DB-element.");
                response_msg.Add(type);
                response_msg.Add(result);
                return response_msg;

            }
            catch (Exception e)
            {
                Console.WriteLine("\n  XML file for request message is not in the expected format. A particular tag could not be found.\n  Exception = {0}", e);
                response_msg.Add(type);
                response_msg.Add(result);
                return response_msg;
            }
        }

        //<--------------------edits description of DBElement of a particular key.
        XElement edit_description<Key, Data>(XElement request_msg, DBEngine<Key, DBElement<Key, Data>> db)
        {
            Console.Write("\n  Request to Edit description of DB Element");
            XElement response_msg = new XElement("Query_Response");
            XElement type = new XElement("Query_Type", "Edit Description");
            XElement result = new XElement("Result", "Could not Edit description of element");
            try
            {
                Key k = (Key)Convert.ChangeType(request_msg.Element("Key").Value, typeof(Key));
                Console.WriteLine(" with key - {0}", k);
                string new_descr = request_msg.Element("Description").Value;
                bool descr_edit = db.editDescr<Key, DBElement<Key, Data>, Data>(k, new_descr);

                if (descr_edit)
                    result = new XElement("Result", "Description of DB Element editted successfully.");
                else
                    result = new XElement("Result", "Could not edit description of DB-element.");
                response_msg.Add(type);
                response_msg.Add(result);
                return response_msg;
            }
            catch (NullReferenceException ex)
            {
                Console.WriteLine("\n  XML file for request message is not in the expected format. A particular tag could not be found.\n  Exception = {0}", ex);
                response_msg.Add(type);
                response_msg.Add(result);
                return response_msg;
            }
        }

        //<-------------------Adds child relation to a DBElement
        XElement add_children<Key, Data>(XElement request_msg, DBEngine<Key, DBElement<Key, Data>> db)
        {
            Console.Write("\n  Request to Add child relation for DB Element");
            XElement response_msg = new XElement("Query_Response");
            XElement type = new XElement("Query_Type", "Add Children");
            XElement result = new XElement("Result", "Could not Add children to DBElement");
            try
            {
                Key k = (Key)Convert.ChangeType(request_msg.Element("Key").Value, typeof(Key));
                Console.WriteLine(" with key - {0}", k);
                bool final_res = true;
                foreach (var new_key in request_msg.Element("Children").Elements("Key"))
                {
                    Key child_key = (Key)Convert.ChangeType(new_key.Value, typeof(Key));
                    bool res = db.addRelation<Key, DBElement<Key, Data>, Data>(k, child_key);
                    if (!res)
                        final_res = false;
                }
                
                if (final_res)
                    result = new XElement("Result", "Child Relation added successfully to DB Element.");
                else
                    result = new XElement("Result", "Could not add Child Relation to DB Element.");
                response_msg.Add(type);
                response_msg.Add(result);
                return response_msg;
            }
            catch (NullReferenceException ex)
            {
                Console.WriteLine("\n  XML file for request message is not in the expected format. A particular tag could not be found.\n  Exception = {0}", ex);
                response_msg.Add(type);
                response_msg.Add(result);
                return response_msg;
            }
        }

        //<--------------------------------Retrieves DBElement from database.
        XElement get_value<Key, Data>(XElement request_msg, DBEngine<Key, DBElement<Key, Data>> db)
        {
            Console.Write("\n  Request to Query for DB Element ");
            try
            {
                Key k = (Key)Convert.ChangeType(request_msg.Element("Key").Value, typeof(Key));
                Console.WriteLine(" with key - {0}", k);
                DBElement<Key, Data> elem = new DBElement<Key, Data>();
                elem = sendQuery.findValue<Key, DBElement<Key, Data>, Data>(db, k);
                if (elem == null) {
                    XElement response_msg = new XElement("Query_Response");
                    XElement type = new XElement("Query_Type", "Get Value");
                    string rst = "Could not find element with key - " + k.ToString() + " in database.";
                    XElement result = new XElement("Result", rst);
                    response_msg.Add(type);
                    response_msg.Add(result);
                    return response_msg;

                }
                else {
                    XElement response_msg = new XElement("Query_Response");
                    XElement type = new XElement("Query_Type", "Get Value");
                    XElement result = new XElement("Result", "Success");
                    string payload = "  Payload of DBElement with key - " + k.ToString() + " = " + elem.payload;                    
                    XElement par = new XElement("Partial", payload);
                    XElement com = new XElement("Complete");
                    string nam = "  Name: " + elem.name + "\n";
                    XElement name = new XElement("Name", nam);
                    string desc = "  Description: " + elem.descr + "\n";
                    XElement descr = new XElement("Description", desc);
                    string tm_stmp = "  TimeStamp: " + elem.timeStamp.ToString() + "\n";
                    XElement time = new XElement("Time", tm_stmp);
                    com.Add(name);
                    com.Add(descr);
                    com.Add(time);
                    response_msg.Add(type);
                    response_msg.Add(result);
                    response_msg.Add(par);
                    response_msg.Add(com);
                    return response_msg;
                }               
            }
            catch (Exception)
            {
                Console.WriteLine("\n  XML file for request message is not in the expected format. A particular tag could not be found.");
                XElement response_msg = new XElement("Result", "Could not insert element into database");
                return request_msg;
            }
        }

        //<-------------------Retrieves child keys of a DBElement from database.
        XElement get_children<Key, Data>(XElement request_msg, DBEngine<Key, DBElement<Key, Data>> db)
        {
            Console.Write("\n  Request to Query for children of DB Element ");
            try
            {
                Key k = (Key)Convert.ChangeType(request_msg.Element("Key").Value, typeof(Key));
                Console.WriteLine(" with key - {0}", k);
                List<Key> result_list = sendQuery.getChildren<Key, DBElement<Key, Data>, Data>(db, k);
                if (result_list == null)
                {
                    XElement response_msg = new XElement("Query_Response");
                    XElement type = new XElement("Query_Type", "Get Children");
                    string rst = "Could not find element with key - " + k.ToString() + " in database.";
                    XElement result = new XElement("Result", rst);
                    response_msg.Add(type);
                    response_msg.Add(result);
                    return response_msg;
                }
                else
                {
                    XElement response_msg = new XElement("Query_Response");
                    XElement type = new XElement("Query_Type", "Get Children");
                    XElement result = new XElement("Result", "Success");                    
                    string list_num = "  Number of keys present in children list of DBelement with key - " + k.ToString() + " = " + result_list.Count;
                    XElement par = new XElement("Partial", list_num);
                    XElement com = new XElement("Complete");
                    XElement data = new XElement("Text", "  Key list =");
                    com.Add(data);
                    foreach (Key child_k in result_list)
                    {
                        string child_space = " " + child_k.ToString();
                        XElement child_key = new XElement("Key", child_space);
                        com.Add(child_key);
                    }                   
                    response_msg.Add(type);
                    response_msg.Add(result);
                    response_msg.Add(par);
                    response_msg.Add(com);
                    return response_msg;
                }
            }
            catch (Exception)
            {
                Console.WriteLine("\n  XML file for request message is not in the expected format. A particular tag could not be found.");
                XElement response_msg = new XElement("Result", "Could not insert element into database");
                return request_msg;
            }
        }

        //<-----------------Retrieves keys of all DBElements with pattern in metadata section.
        XElement keys_metada<Key, Data>(XElement request_msg, DBEngine<Key, DBElement<Key, Data>> db)
        {
            Console.Write("\n  Request to Query for DBElements");
            try
            {
                string pat = request_msg.Element("Pattern").Value;
                Console.WriteLine(" with pattern - \"{0}\" present in metadata.", pat);
                List<Key> result_list = sendQuery.get_keys_with_metadata<Key, DBElement<Key, Data>, Data>(db, pat);
                if (result_list == null) {
                    XElement response_msg = new XElement("Query_Response");
                    XElement type = new XElement("Query_Type", "Get Keys metadata");
                    string rst = "Could not find element with pattern - " + pat + " in metadata section";
                    XElement result = new XElement("Result", rst);
                    response_msg.Add(type);
                    response_msg.Add(result);
                    return response_msg;
                }
                else {
                    XElement response_msg = new XElement("Query_Response");
                    XElement type = new XElement("Query_Type", "Get Keys metadata");
                    XElement result = new XElement("Result", "Success");
                    string list_num = "  Number of DBElements with pattern - \"" + pat + "\" in metadat = " + result_list.Count;
                    XElement par = new XElement("Partial", list_num);
                    XElement com = new XElement("Complete");
                    XElement data = new XElement("Text", "  DBElements key list =");
                    com.Add(data);
                    foreach (Key child_k in result_list)
                    {
                        string child_space = " " + child_k.ToString();
                        XElement child_key = new XElement("Key", child_space);
                        com.Add(child_key);
                    }
                    response_msg.Add(type);
                    response_msg.Add(result);
                    response_msg.Add(par);
                    response_msg.Add(com);
                    return response_msg;
                }
            }
            catch (Exception)
            {
                Console.WriteLine("\n  XML file for request message is not in the expected format. A particular tag could not be found.");
                XElement response_msg = new XElement("Result", "Could not insert element into database");
                return request_msg;
            }
        }

        //<--------------------Save database contents into an XML file.
        XElement persist_database<Key, Data>(XElement request_msg, DBEngine<Key, DBElement<Key, Data>> db)
        {
            Console.WriteLine("\n  Request to Persist Database: ");
            try
            {
                //string pat = request_msg.Element("Pattern").Value;
                //Console.WriteLine(" with pattern - \"{0}\" present in metadata.", pat);
                PersistEngine pe = new PersistEngine();
                string file_name = pe.persistDB<Key, DBElement<Key, Data>, Data>(db);
                Console.WriteLine("  XML File saved at: {0}", file_name.Substring(13));
                XElement response_msg = new XElement("Query_Response");
                XElement type = new XElement("Query_Type", "Persist Database");
                
                string reply = "XML file saved at " + file_name.Substring(13);
                XElement result = new XElement("Result", reply);
                response_msg.Add(type);
                //response_msg.Add(file);
                
                // return result;       
                return result;  
                
            }
            catch (Exception)
            {
                Console.WriteLine("\n  XML file for request message is not in the expected format. A particular tag could not be found.");
                XElement response_msg = new XElement("Result", "Could not persist database.");
                return request_msg;
            }
            
        }

        //<----------------------Load database from an XML file.
        XElement load_database<Key, Data>(XElement request_msg, DBEngine<Key, DBElement<Key, Data>> db)
        {
            Console.Write("\n  Request to Load Database from XML file at: ");
            try
            {
                //string pat = request_msg.Element("Pattern").Value;
                //Console.WriteLine(" with pattern - \"{0}\" present in metadata.", pat);
                PersistEngine pe = new PersistEngine();
                string file_name = request_msg.Element("File_Name").Value;
                Console.WriteLine("{0}", file_name.Substring(13));
                pe.loadDB<Key, DBElement<Key, Data>, Data>(db, file_name);

                XElement response_msg = new XElement("Query_Response");
                XElement type = new XElement("Query_Type", "Load Database");

                string reply = "Loaded database from XML file saved at " + file_name.Substring(13);
                XElement result = new XElement("Result", reply);
                response_msg.Add(type);
                response_msg.Add(result);

                return result;

            }
            catch (Exception)
            {
                Console.WriteLine("\n  XML file for request message is not in the expected format. A particular tag could not be found.");
                XElement response_msg = new XElement("Result", "Could not insert element into database");
                return request_msg;
            }
        }



    }

#if (TEST_DBPROCESSOR)
    class Test_DBProcessor
    {
        static void Main(string[] args)
        {
            DBProcessor db_process = new DBProcessor();
            DBEngine<int, DBElement<int, string>> db = new DBEngine<int, DBElement<int, string>>();
            /*
            for (int i=0;i<100;i++)
            {
               // int tm = DateTime.Now
                Thread.Sleep(1100);
                Console.WriteLine("tm = {0}", DateTime.Now);
                Console.WriteLine("tm1 = {0}", DateTime.Now.ToString("M / dd / yyyy h: mm:ss.fff tt"));
                //DateTime.Parse               
                lkfs
            }*/
            string tm1 = DateTime.Now.ToString("M / dd / yyyy h: mm:ss.fff tt");
            Thread.Sleep(150);
            string tm2 = DateTime.Now.ToString("M / dd / yyyy h: mm:ss.fff tt");
            DateTime tma = DateTime.Parse(tm1);
            DateTime tmb = DateTime.Parse(tm2);
            TimeSpan ts = tmb - tma;
            long ts_tick = ts.Ticks;
            Console.WriteLine("tm1 = {0}\ntm2 = {1}", tm1, tm2);
            Console.WriteLine("ts = {0}\nts_tick = {1}", ts, ts.TotalMilliseconds);

            DateTime tm1_t = DateTime.Now;
            Thread.Sleep(160);          
            DateTime tm2_t = DateTime.Now;
            TimeSpan a = tm2_t - tm1_t;
            long ms = a.Ticks;
            Console.WriteLine("tm1 = {0}\ntm2 = {1}", tm1_t, tm2_t);
            Console.WriteLine("a = {0}\nms = {1}", a,(tm2_t-tm1_t).TotalMilliseconds);


            //------<Test inserting Element into database >----------
            XElement request_msg = new XElement("Request_Message");
            XElement req_type = new XElement("Request_Type", "Insert");
            XElement key = new XElement("Key", 45);
           // XElement name = new XElement("Name", "element_one");
            XElement descr = new XElement("Description", "Descr of element_one");
            XElement children = new XElement("Children");
            XElement k1 = new XElement("Key", 55);
            XElement k2 = new XElement("Key", 65);
            children.Add(k1);
            children.Add(k2);
            XElement payload = new XElement("Payload", "This the payload of element one.");

            request_msg.Add(req_type);
            request_msg.Add(key);
            //request_msg.Add(name);
            request_msg.Add(descr);
            request_msg.Add(children);
            request_msg.Add(payload);
            string msg_content = request_msg.ToString();
            Console.WriteLine("Going to call show db:");
            db.showDB();
            Console.WriteLine("Going to to call process query with xml = \n{0}", request_msg.ToString());

          //  string reply = db_process.process_msg<int, DBElement<int, string>, string>(msg_content, db);
           // Console.WriteLine("Reply from processing = {0}", reply);
            db.showDB();

            XElement req_msg3 = new XElement("Request_Message");
            XElement req_type3 = new XElement("Request_Type", "Edit Name");
            XElement req_elem = new XElement("Key", 45);
            XElement new_name = new XElement("Name", "This is new name for element.");
            req_msg3.Add(req_type3);
            req_msg3.Add(req_elem);
            req_msg3.Add(new_name);
            Console.WriteLine("Going to to call process query with xml = \n{0}", req_msg3.ToString());
          //  string reply3 = db_process.process_msg<int, DBElement<int, string>, string>(req_msg3.ToString(),db);
            db.showDB();

            XElement req_msg4 = new XElement("Request_Message");
            XElement req_type4 = new XElement("Request_Type", "Edit Description");
            XElement req_key = new XElement("Key", 45);
            XElement new_descr = new XElement("Description", "New decription for element.");
            req_msg4.Add(req_type4);
            req_msg4.Add(req_key);
            req_msg4.Add(new_descr);
            Console.WriteLine("Going to to call process query with xml = \n{0}", req_msg4.ToString());
           // string reply4 = db_process.process_msg<int, DBElement<int, string>, string>(req_msg4.ToString(), db);
            db.showDB();
            Console.WriteLine("Testing XML");
            XElement rep = new XElement("Reply_msg");
            XElement result = new XElement("Result", "Success");
            XElement query = new XElement("Query");
            XElement qres = new XElement("Type", "File Name");
            rep.Add(qres);
            if (rep.Element("Result") == null)
                Console.Write("Hello");
            

        }
    }
    
#endif
}
