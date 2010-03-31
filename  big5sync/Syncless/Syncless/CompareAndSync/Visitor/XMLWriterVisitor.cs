using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncless.CompareAndSync.CompareObject;
using System.Xml;
using System.IO;
using Syncless.CompareAndSync.Enum;
using Syncless.Notification;


namespace Syncless.CompareAndSync.Visitor
{
    public class XMLWriterVisitor : IVisitor
    {
        #region IVisitor Members

        private const string MetaDir = ".syncless";
        private const string XmlName = "syncless.xml";
        private const string TodoName = "todo.xml";
        private const string Metadatapath = MetaDir + "\\" + XmlName;
        private const string Todopath = MetaDir + "\\" + TodoName;
        private const string FOLDER = "folder";
        private const string FILE = "file";
        private const string NodeName = "name";
        private const string NodeSize = "size";
        private const string NodeHash = "hash";
        private const string NodeLastModified = "last_modified";
        private const string NodeLastCreated = "last_created";
        private const string NodeLastUpdated = "last_updated";
        private const string XpathExpr = "/meta-data";
        private const string LastModified = "/last_modified";
        private const string LastKnownState = "last_known_state";
        private const string Action = "action";
        private const string Deleted = "Deleted";
        private long dateTime = DateTime.Now.Ticks;
        private static readonly object SyncLock = new object();
        private SyncProgress _progress;
        #endregion

        public SyncProgress Progress
        {
            get { return _progress; }
        }

        public XMLWriterVisitor(SyncProgress progress)
        {
            _progress = progress;
        }

        public void Visit(FileCompareObject file, int numOfPaths)
        {
            for (int i = 0; i < numOfPaths; i++) // HANDLE ALL EXCEPT PROPAGATED
            {
                ProcessMetaChangeType(file, i);
            }

            _progress.complete();
        }

        public void Visit(FolderCompareObject folder, int numOfPaths)
        {
            
            for (int i = 0; i < numOfPaths; i++)
            {
                ProcessFolderFinalState(folder, i);
            }

            _progress.complete();
        }

        public void Visit(RootCompareObject root)
        {
            _progress.complete();
        }

        #region File State Processor
        private void ProcessMetaChangeType(FileCompareObject file, int counter)
        {
            FinalState? changeType = file.FinalState[counter];

            if (changeType == null)
            {
                HandleNullCases(file, counter);
                return;
            }

            switch (changeType)
            {
                case FinalState.Created:
                    CreateFileObject(file, counter);
                    break;
                case FinalState.Updated:
                    UpdateFileObject(file, counter);
                    break;
                case FinalState.Deleted:
                    DeleteFileObject(file, counter);
                    break;
                case FinalState.Renamed:
                    RenameFileObject(file, counter);
                    break;
                case FinalState.Unchanged:
                    HandleUnchangedOrPropagatedFile(file, counter);
                    break;
                case FinalState.Propagated:
                    HandleUnchangedOrPropagatedFile(file, counter);
                    break;
            }


        }
        #endregion

        private void UpdateLastModifiedTime(XmlDocument xmlDoc)
        {
            XmlNode node = xmlDoc.SelectSingleNode(XpathExpr + LastModified);
            node.InnerText = DateTime.Now.Ticks.ToString();
        }

        private string getFolderString(string filePath)
        {
            string[] splitWords = filePath.Split('\\');
            string folderPath = "";
            for (int i = 0; i < splitWords.Length; i++)
            {
                if (i == splitWords.Length - 1)
                    break;
                if (folderPath.Equals(""))
                    folderPath = splitWords[i];
                else
                    folderPath = folderPath + "\\" + splitWords[i];
            }

            return folderPath;
        }

        private string GetLastFileIndex(string filePath)
        {
            string[] splitWords = filePath.Split('\\');
            string folderPath = "";
            for (int i = 0; i < splitWords.Length; i++)
            {
                if (i == splitWords.Length - 1)
                    return splitWords[i];
            }

            return folderPath;
        }

        #region File Operations

        private void CreateRenamedFileObject(FileCompareObject file, int counter)
        {
            XmlDocument xmlDoc = new XmlDocument();
            string xmlPath = Path.Combine(file.GetSmartParentPath(counter), Metadatapath);
            CommonMethods.CreateFileIfNotExist(file.GetSmartParentPath(counter));
            CommonMethods.LoadXML(ref xmlDoc, xmlPath);
            int position = GetPropagated(file);
            DoFileCleanUp(xmlDoc, file.Name);
            XmlText hashText = xmlDoc.CreateTextNode(file.MetaHash[position]);
            XmlText nameText = xmlDoc.CreateTextNode(file.NewName);
            XmlText sizeText = xmlDoc.CreateTextNode(file.MetaLength[position].ToString());
            XmlText lastModifiedText = xmlDoc.CreateTextNode(file.MetaLastWriteTime[counter].ToString());
            XmlText lastCreatedText = xmlDoc.CreateTextNode(file.MetaCreationTime[counter].ToString());
            XmlText lastUpdated = xmlDoc.CreateTextNode(dateTime.ToString());

            XmlElement fileElement = xmlDoc.CreateElement(FILE);
            XmlElement hashElement = xmlDoc.CreateElement(NodeHash);
            XmlElement nameElement = xmlDoc.CreateElement(NodeName);
            XmlElement sizeElement = xmlDoc.CreateElement(NodeSize);
            XmlElement lastModifiedElement = xmlDoc.CreateElement(NodeLastModified);
            XmlElement lastCreatedElement = xmlDoc.CreateElement(NodeLastCreated);
            XmlElement lastUpdatedElement = xmlDoc.CreateElement(NodeLastUpdated);

            hashElement.AppendChild(hashText);
            nameElement.AppendChild(nameText);
            sizeElement.AppendChild(sizeText);
            lastModifiedElement.AppendChild(lastModifiedText);
            lastCreatedElement.AppendChild(lastCreatedText);
            lastUpdatedElement.AppendChild(lastUpdated);

            fileElement.AppendChild(nameElement);
            fileElement.AppendChild(sizeElement);
            fileElement.AppendChild(hashElement);
            fileElement.AppendChild(lastModifiedElement);
            fileElement.AppendChild(lastCreatedElement);
            fileElement.AppendChild(lastUpdatedElement);

            XmlNode node = xmlDoc.SelectSingleNode(XpathExpr);
            node.AppendChild(fileElement);
            CommonMethods.SaveXML(ref xmlDoc, xmlPath);

            XmlDocument todoXMLDoc = new XmlDocument();
            string todoPath = Path.Combine(file.GetSmartParentPath(counter), Todopath);
            DeleteFileTodoByName(file, counter);
        }


        private void CreateFileObject(FileCompareObject file, int counter)
        {
            XmlDocument xmlDoc = new XmlDocument();
            string xmlPath = Path.Combine(file.GetSmartParentPath(counter), Metadatapath);
            CommonMethods.CreateFileIfNotExist(file.GetSmartParentPath(counter));
            CommonMethods.LoadXML(ref xmlDoc, xmlPath);
            int position = GetPropagated(file);
            DoFileCleanUp(xmlDoc, file.Name);
            XmlText hashText = xmlDoc.CreateTextNode(file.Hash[position]);
            XmlText nameText = xmlDoc.CreateTextNode(file.Name);
            XmlText sizeText = xmlDoc.CreateTextNode(file.Length[position].ToString());
            XmlText lastModifiedText = xmlDoc.CreateTextNode(file.LastWriteTime[counter].ToString());
            XmlText lastCreatedText = xmlDoc.CreateTextNode(file.CreationTime[counter].ToString());
            XmlText lastUpdated = xmlDoc.CreateTextNode(dateTime.ToString());

            XmlElement fileElement = xmlDoc.CreateElement(FILE);
            XmlElement hashElement = xmlDoc.CreateElement(NodeHash);
            XmlElement nameElement = xmlDoc.CreateElement(NodeName);
            XmlElement sizeElement = xmlDoc.CreateElement(NodeSize);
            XmlElement lastModifiedElement = xmlDoc.CreateElement(NodeLastModified);
            XmlElement lastCreatedElement = xmlDoc.CreateElement(NodeLastCreated);
            XmlElement lastUpdatedElement = xmlDoc.CreateElement(NodeLastUpdated);

            hashElement.AppendChild(hashText);
            nameElement.AppendChild(nameText);
            sizeElement.AppendChild(sizeText);
            lastModifiedElement.AppendChild(lastModifiedText);
            lastCreatedElement.AppendChild(lastCreatedText);
            lastUpdatedElement.AppendChild(lastUpdated);

            fileElement.AppendChild(nameElement);
            fileElement.AppendChild(sizeElement);
            fileElement.AppendChild(hashElement);
            fileElement.AppendChild(lastModifiedElement);
            fileElement.AppendChild(lastCreatedElement);
            fileElement.AppendChild(lastUpdatedElement);

            XmlNode node = xmlDoc.SelectSingleNode(XpathExpr);
            node.AppendChild(fileElement);
            CommonMethods.SaveXML(ref xmlDoc, xmlPath);

            XmlDocument todoXMLDoc = new XmlDocument();
            string todoPath = Path.Combine(file.GetSmartParentPath(counter), Todopath);
            DeleteFileTodoByName(file, counter);
        }

        private void UpdateFileObject(FileCompareObject file, int counter)
        {
            XmlDocument xmlDoc = new XmlDocument();
            string xmlPath = Path.Combine(file.GetSmartParentPath(counter), Metadatapath);
            CommonMethods.CreateFileIfNotExist(file.GetSmartParentPath(counter));

            CommonMethods.LoadXML(ref xmlDoc, xmlPath);

            int position = GetPropagated(file);
            XmlNode node = xmlDoc.SelectSingleNode(XpathExpr + "/" + FILE + "[name=" + CommonMethods.ParseXPathString(file.Name) + "]");
            if (node == null)
            {
                CreateFileObject(file, counter);
                return;
            }

            XmlNodeList childNodeList = node.ChildNodes;
            for (int i = 0; i < childNodeList.Count; i++)
            {
                XmlNode nodes = childNodeList[i];
                if (nodes.Name.Equals(NodeSize))
                {
                    nodes.InnerText = file.Length[position].ToString();
                }
                else if (nodes.Name.Equals(NodeHash))
                {
                    nodes.InnerText = file.Hash[position];
                }
                else if (nodes.Name.Equals(NodeName))
                {
                    nodes.InnerText = file.Name;
                }
                else if (nodes.Name.Equals(NodeLastModified))
                {
                    nodes.InnerText = file.LastWriteTime[counter].ToString();
                }
                else if (nodes.Name.Equals(NodeLastCreated))
                {
                    nodes.InnerText = file.CreationTime[counter].ToString();
                }
                else if (nodes.Name.Equals(NodeLastUpdated))
                {
                    nodes.InnerText = dateTime.ToString();
                }
            }

            CommonMethods.SaveXML(ref xmlDoc, xmlPath);
            DeleteFileTodoByName(file, counter);
        }

        private void RenameFileObject(FileCompareObject file, int counter)
        {
            XmlDocument xmlDoc = new XmlDocument();
            string xmlPath = Path.Combine(file.GetSmartParentPath(counter), Metadatapath);
            CommonMethods.CreateFileIfNotExist(file.GetSmartParentPath(counter));
            CommonMethods.LoadXML(ref xmlDoc, xmlPath);

            XmlNode node = xmlDoc.SelectSingleNode(XpathExpr + "/" + FILE + "[name=" + CommonMethods.ParseXPathString(file.Name) + "]");
            if (node == null)
            {
                CreateRenamedFileObject(file, counter);
                //CreateFileObject(file, counter);
                return;
            }
            node.FirstChild.InnerText = file.NewName;
            node.LastChild.InnerText = dateTime.ToString();
            CommonMethods.SaveXML(ref xmlDoc, xmlPath);
            GenerateFileTodo(file, counter);
        }

        private void DeleteFileObject(FileCompareObject file, int counter)
        {
            XmlDocument xmlDoc = new XmlDocument();
            string parentPath = file.GetSmartParentPath(counter);
            string xmlPath = Path.Combine(parentPath, Metadatapath);
            if (File.Exists(xmlPath))
            {
                CommonMethods.LoadXML(ref xmlDoc, xmlPath);

                XmlNode node = xmlDoc.SelectSingleNode(XpathExpr + "/" + FILE + "[name=" + CommonMethods.ParseXPathString(file.Name) + "]");
                if (node == null)
                    return;
                node.ParentNode.RemoveChild(node);

                CommonMethods.SaveXML(ref xmlDoc, xmlPath);
            }

            GenerateFileTodo(file, counter);
        }

        private void HandleUnchangedOrPropagatedFile(FileCompareObject file, int counter)
        {
            string name = Path.Combine(file.GetSmartParentPath(counter), file.Name);
            if (File.Exists(name)) //CREATE OR UPDATED
            {
                bool metaExist = file.MetaExists[counter];
                bool fileExist = file.Exists[counter];
                if (metaExist == true && fileExist == true) //UPDATE
                {
                    UpdateFileObject(file, counter);
                }
                else  //NEW
                {
                    CreateFileObject(file, counter);
                }
            }
            else                 //DELETE OR RENAME
            {
                if (file.NewName != null)
                    RenameFileObject(file, counter);
                else
                    DeleteFileObject(file, counter);
            }
        }

        private void HandleNullCases(FileCompareObject file, int counter)
        {

            string fullPath = Path.Combine(file.GetSmartParentPath(counter), file.Name);
            if (File.Exists(fullPath))
                return;

            XmlDocument xmlDoc = new XmlDocument();
            string xmlPath = Path.Combine(file.GetSmartParentPath(counter), Metadatapath);

            CommonMethods.LoadXML(ref xmlDoc, xmlPath);

            XmlNode node = xmlDoc.SelectSingleNode(XpathExpr + "/" + FILE + "[name=" + CommonMethods.ParseXPathString(file.Name) + "]");
            if (node != null)
                node.ParentNode.RemoveChild(node);
            CommonMethods.SaveXML(ref xmlDoc, xmlPath);

            GenerateFileTodo(file, counter);
        }

        #endregion

        #region Misc operations
        private int GetPropagated(FileCompareObject file)
        {
            FinalState?[] states = file.FinalState;
            for (int i = 0; i < states.Length; i++)
            {
                if (FinalState.Propagated == states[i])
                    return i;
            }

            return 0; // never happen
        }

        private int GetPropagated(FolderCompareObject folder)
        {
            FinalState?[] states = folder.FinalState;
            for (int i = 0; i < states.Length; i++)
            {
                if (FinalState.Propagated == states[i])
                    return i;
            }

            return -1; // never happen
        }


        private void DoFileCleanUp(XmlDocument xmlDoc, string name)
        {
            XmlNode node = xmlDoc.SelectSingleNode(XpathExpr + "/" + FILE + "[name=" + CommonMethods.ParseXPathString(name) + "]");
            if (node == null)
                return;
            node.ParentNode.RemoveChild(node);
        }

        private void DoFolderCleanUp(XmlDocument xmlDoc, string name)
        {
            XmlNode node = xmlDoc.SelectSingleNode(XpathExpr + "/" + FOLDER + "[name=" + CommonMethods.ParseXPathString(name) + "]");
            if (node == null)
                return;
            node.ParentNode.RemoveChild(node);
        }
        #endregion

        #region Folder State Processor
        private void ProcessFolderFinalState(FolderCompareObject folder, int counter)
        {

            FinalState?[] finalStateList = folder.FinalState;
            FinalState? changeType = finalStateList[counter];
            if (changeType == null)
            {
                HandleNullFolderCases(folder, counter);
                return;
            }
            switch (changeType)
            {
                case FinalState.Created:
                    CreateFolderObject(folder, counter);
                    break;
                case FinalState.Deleted:
                    DeleteFolderObject(folder, counter);
                    break;
                case FinalState.Renamed:
                    RenameFolderObject(folder, counter);
                    break;
                case FinalState.Unchanged:
                    HandleUnchangedOrPropagatedFolder(folder, counter);
                    break;
                case FinalState.Propagated:
                    HandleUnchangedOrPropagatedFolder(folder, counter);
                    break;
            }
        }
        #endregion

        #region Folder Operations
        private void CreateFolderObject(FolderCompareObject folder, int counter)
        {
            XmlDocument xmlDoc = new XmlDocument();
            string xmlPath = Path.Combine(folder.GetSmartParentPath(counter), Metadatapath);
            CommonMethods.CreateFileIfNotExist(folder.GetSmartParentPath(counter));
            CommonMethods.LoadXML(ref xmlDoc, xmlPath);

            DoFolderCleanUp(xmlDoc, folder.Name);
            XmlText nameText = xmlDoc.CreateTextNode(folder.Name);
            XmlText lastUpdatedText = xmlDoc.CreateTextNode(dateTime.ToString());
            XmlElement nameElement = xmlDoc.CreateElement(NodeName);
            XmlElement lastUpdatedElement = xmlDoc.CreateElement(NodeLastUpdated);
            XmlElement folderElement = xmlDoc.CreateElement(FOLDER);

            nameElement.AppendChild(nameText);
            lastUpdatedElement.AppendChild(lastUpdatedText);
            folderElement.AppendChild(nameElement);
            folderElement.AppendChild(lastUpdatedElement);

            XmlNode node = xmlDoc.SelectSingleNode(XpathExpr);
            node.AppendChild(folderElement);

            string subFolderXML = Path.Combine(folder.GetSmartParentPath(counter), folder.Name);
            CommonMethods.CreateFileIfNotExist(subFolderXML);
            CommonMethods.SaveXML(ref xmlDoc, xmlPath);
            ModifyFolderName(folder, folder.GetSmartParentPath(counter));
            DeleteFolderTodoByName(folder, counter);
        }

        private void RenameFolderObject(FolderCompareObject folder, int counter)
        {

            if (Directory.Exists(Path.Combine(folder.GetSmartParentPath(counter), folder.Name)))
            {
                XmlDocument xmlDoc = new XmlDocument();
                string xmlPath = Path.Combine(folder.GetSmartParentPath(counter), Metadatapath);

                CommonMethods.LoadXML(ref xmlDoc, xmlPath);

                XmlNode node = xmlDoc.SelectSingleNode(XpathExpr + "/" + FOLDER + "[name=" + CommonMethods.ParseXPathString(folder.Name) + "]");
                if (node == null)
                {
                    CreateFolderObject(folder, counter);
                    return;
                }

                node.FirstChild.InnerText = folder.NewName;
                node.LastChild.InnerText = dateTime.ToString();
                CommonMethods.SaveXML(ref xmlDoc, xmlPath);
                GenerateFolderTodo(folder, counter);
            }
            else
            {
                XmlDocument newXmlDoc = new XmlDocument();
                string editOldXML = Path.Combine(Path.Combine(folder.GetSmartParentPath(counter), folder.NewName), Metadatapath);
                CommonMethods.CreateFileIfNotExist(Path.Combine(folder.GetSmartParentPath(counter), folder.NewName));
                CommonMethods.LoadXML(ref newXmlDoc, editOldXML);
                XmlNode xmlNameNode = newXmlDoc.SelectSingleNode(XpathExpr + "/name");
                if(xmlNameNode!=null)
                    xmlNameNode.InnerText = folder.NewName;
                CommonMethods.SaveXML(ref newXmlDoc, editOldXML);

                string parentXML = Path.Combine(folder.GetSmartParentPath(counter), Metadatapath);
                XmlDocument parentXmlDoc = new XmlDocument();
                CommonMethods.CreateFileIfNotExist(folder.GetSmartParentPath(counter));
                CommonMethods.LoadXML(ref parentXmlDoc, parentXML);
                XmlNode parentXmlFolderNode = parentXmlDoc.SelectSingleNode(XpathExpr + "/" + FOLDER + "[name=" + CommonMethods.ParseXPathString(folder.Name) + "]");
                if (parentXmlFolderNode != null)
                {
                    parentXmlFolderNode.FirstChild.InnerText = folder.NewName;
                    parentXmlFolderNode.LastChild.InnerText = dateTime.ToString();
                }
                CommonMethods.SaveXML(ref parentXmlDoc, Path.Combine(folder.GetSmartParentPath(counter), Metadatapath));
                GenerateFolderTodo(folder, counter);
            }
        }

        private void DeleteFolderObject(FolderCompareObject folder, int counter)
        {
            XmlDocument xmlDoc = new XmlDocument();
            string xmlPath = Path.Combine(folder.GetSmartParentPath(counter), Metadatapath);
            if (File.Exists(xmlPath))
            {
                CommonMethods.LoadXML(ref xmlDoc, xmlPath);
                XmlNode node = xmlDoc.SelectSingleNode(XpathExpr + "/" + FOLDER + "[name=" + CommonMethods.ParseXPathString(folder.Name) + "]");
                if (node == null)
                    return;
                node.ParentNode.RemoveChild(node);
                CommonMethods.SaveXML(ref xmlDoc, xmlPath);
            }

            GenerateFolderTodo(folder, counter);
        }

        private void HandleUnchangedOrPropagatedFolder(FolderCompareObject folder, int counter)
        {
            string name = Path.Combine(folder.GetSmartParentPath(counter), folder.Name);
            if (Directory.Exists(name)) //CREATE OR RENAME
            {
                bool metaExist = folder.MetaExists[counter];
                bool folderExist = folder.Exists[counter];
                if (folderExist == true) //CREATE
                {
                    CreateFolderObject(folder, counter);
                }
            }
            else                 //DELETE OR RENAME
            {
                if (folder.NewName != null)
                    RenameFolderObject(folder, counter);
                else
                    DeleteFolderObject(folder, counter);
            }
        }

        private void HandleNullFolderCases(FolderCompareObject folder , int counter)
        {

            string fullPath = Path.Combine(folder.GetSmartParentPath(counter), folder.Name);
            if (Directory.Exists(fullPath))
                return;

            XmlDocument xmlDoc = new XmlDocument();
            string xmlPath = Path.Combine(folder.GetSmartParentPath(counter), Metadatapath);
            if (!File.Exists(xmlPath))
                return;

            CommonMethods.LoadXML(ref xmlDoc, xmlPath);
            XmlNode node = xmlDoc.SelectSingleNode(XpathExpr + "/" + FOLDER + "[name=" + CommonMethods.ParseXPathString(folder.Name) + "]");
            if (node != null)
                node.ParentNode.RemoveChild(node);
            CommonMethods.SaveXML(ref xmlDoc, xmlPath);

            GenerateFolderTodo(folder, counter);
        }
        
        private void ModifyFolderName(FolderCompareObject folder, string subFolderPath)
        {
            string name = string.Empty;
            if(folder.NewName!=null)
            {
                name = folder.NewName;
            }
            else
            {
                name = folder.Name;
            }

            string xmlPath = Path.Combine(Path.Combine(subFolderPath,folder.Name),Metadatapath);
            XmlDocument subFolderDoc = new XmlDocument();
            CommonMethods.LoadXML(ref subFolderDoc , xmlPath);

            XmlNode xmlNameNode = subFolderDoc.SelectSingleNode(XpathExpr + "/name");
            if(xmlNameNode != null)
                xmlNameNode.InnerText = name;
            CommonMethods.SaveXML(ref subFolderDoc , xmlPath);
        }

        #endregion

        #region Todo Operations

        private void CreateTodoFile(string path)
        {
            
                string todoXML = Path.Combine(path, Todopath);
                if (File.Exists(todoXML))
                    return;
            try
            {
                DirectoryInfo di = Directory.CreateDirectory(Path.Combine(path, MetaDir));
                di.Attributes = FileAttributes.Directory | FileAttributes.Hidden;
                XmlTextWriter writer = new XmlTextWriter(todoXML, null);
                writer.Formatting = Formatting.Indented;
                writer.WriteStartDocument();
                writer.WriteStartElement(LastKnownState);
                writer.WriteEndElement();
                writer.WriteEndDocument();
                writer.Flush();
                writer.Close();
            }
            catch (IOException)
            {
                
            }
            catch (UnauthorizedAccessException)
            {
            
            }
            catch (XmlException)
            {
                if (File.Exists(todoXML))
                {
                    File.Delete(todoXML);
                }
                CreateTodoFile(path);
            }
        }

        private void AppendActionFileTodo(XmlDocument xmlDoc, FileCompareObject file, int counter, string changeType)
        {
            XmlText hashText = xmlDoc.CreateTextNode(file.MetaHash[counter]);
            XmlText actionText = xmlDoc.CreateTextNode(changeType);
            XmlText lastModifiedText = xmlDoc.CreateTextNode(file.MetaLastWriteTime[counter].ToString());
            XmlText nameText = xmlDoc.CreateTextNode(file.Name);
            XmlText lastUpdatedText = xmlDoc.CreateTextNode(dateTime.ToString());

            XmlElement fileElement = xmlDoc.CreateElement(FILE);
            XmlElement nameElement = xmlDoc.CreateElement(NodeName);
            XmlElement hashElement = xmlDoc.CreateElement(NodeHash);
            XmlElement actionElement = xmlDoc.CreateElement(Action);
            XmlElement lastModifiedElement = xmlDoc.CreateElement(NodeLastModified);
            XmlElement lastUpdatedElement = xmlDoc.CreateElement(NodeLastUpdated);

            hashElement.AppendChild(hashText);
            actionElement.AppendChild(actionText);
            lastModifiedElement.AppendChild(lastModifiedText);
            nameElement.AppendChild(nameText);
            lastUpdatedElement.AppendChild(lastUpdatedText);

            fileElement.AppendChild(nameElement);
            fileElement.AppendChild(actionElement);
            fileElement.AppendChild(hashElement);
            fileElement.AppendChild(lastModifiedElement);
            fileElement.AppendChild(lastUpdatedElement);

            XmlNode rootNode = xmlDoc.SelectSingleNode("/" + LastKnownState);
            rootNode.AppendChild(fileElement);
        }

        private void AppendActionFolderTodo(XmlDocument xmlDoc, FolderCompareObject folder, int counter, string changeType)
        {
            string name = string.Empty;
            if (folder.MetaName != null)
                name = folder.MetaName;
            else
                name = folder.Name;

            XmlText nameText = xmlDoc.CreateTextNode(name);
            XmlText action = xmlDoc.CreateTextNode(changeType);
            XmlText lastUpdatedText = xmlDoc.CreateTextNode(dateTime.ToString());

            XmlElement folderElement = xmlDoc.CreateElement(FOLDER);
            XmlElement nameElement = xmlDoc.CreateElement(NodeName);
            XmlElement actionElement = xmlDoc.CreateElement(Action);
            XmlElement lastUpdatedElement = xmlDoc.CreateElement(NodeLastUpdated);

            nameElement.AppendChild(nameText);
            actionElement.AppendChild(action);
            lastUpdatedElement.AppendChild(lastUpdatedText);

            folderElement.AppendChild(nameElement);
            folderElement.AppendChild(actionElement);
            folderElement.AppendChild(lastUpdatedElement);
            XmlNode rootNode = xmlDoc.SelectSingleNode("/" + LastKnownState);
            rootNode.AppendChild(folderElement);
        }

        private void DeleteFileTodoByName(FileCompareObject file, int counter)
        {
            string todoXMLPath = Path.Combine(file.GetSmartParentPath(counter), Todopath);
            if (!File.Exists(todoXMLPath))
                return;

            XmlDocument todoXMLDoc = new XmlDocument();
            CommonMethods.LoadXML(ref todoXMLDoc, todoXMLPath);
            XmlNode fileNode = todoXMLDoc.SelectSingleNode("/" + LastKnownState + "/" + FILE + "[name=" + CommonMethods.ParseXPathString(file.Name) + "]");
            if (fileNode != null)
                fileNode.ParentNode.RemoveChild(fileNode);
            CommonMethods.SaveXML(ref todoXMLDoc, todoXMLPath);
        }

        private void DeleteFolderTodoByName(FolderCompareObject folder, int counter)
        {
            string todoXMLPath = Path.Combine(folder.GetSmartParentPath(counter), Todopath);
            if (!File.Exists(todoXMLPath))
                return;

            XmlDocument todoXMLDoc = new XmlDocument();
            CommonMethods.LoadXML(ref todoXMLDoc, todoXMLPath);
            XmlNode folderNode = todoXMLDoc.SelectSingleNode("/" + LastKnownState + "/" + FOLDER + "[name=" + CommonMethods.ParseXPathString(folder.Name) + "]");
            if (folderNode != null)
                folderNode.ParentNode.RemoveChild(folderNode);
            CommonMethods.SaveXML(ref todoXMLDoc, todoXMLPath);
        }

        private void GenerateFileTodo(FileCompareObject file, int counter)
        {
            string parentPath = file.GetSmartParentPath(counter);
            XmlDocument xmlTodoDoc = new XmlDocument();
            string todoPath = Path.Combine(parentPath, Todopath);
            CreateTodoFile(parentPath);
            CommonMethods.LoadXML(ref xmlTodoDoc, todoPath);
            AppendActionFileTodo(xmlTodoDoc, file, counter, Deleted);
            CommonMethods.SaveXML(ref xmlTodoDoc, todoPath);
        }

        private void GenerateFolderTodo(FolderCompareObject folder, int counter)
        {
            string parentPath = folder.GetSmartParentPath(counter);
            XmlDocument xmlTodoDoc = new XmlDocument();
            string todoPath = Path.Combine(parentPath, Todopath);
            CreateTodoFile(parentPath);
            CommonMethods.LoadXML(ref xmlTodoDoc, todoPath);
            AppendActionFolderTodo(xmlTodoDoc, folder, counter, Deleted);
            CommonMethods.SaveXML(ref xmlTodoDoc, todoPath);
        }
        #endregion
    }
}
