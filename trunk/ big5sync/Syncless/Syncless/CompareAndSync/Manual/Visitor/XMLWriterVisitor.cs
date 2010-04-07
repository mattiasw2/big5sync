using System;
using System.IO;
using System.Xml;
using Syncless.CompareAndSync.Enum;
using Syncless.CompareAndSync.Manual.CompareObject;
using Syncless.Notification;


namespace Syncless.CompareAndSync.Manual.Visitor
{
    public class XMLWriterVisitor : IVisitor
    {
        private readonly long _dateTime;
        private readonly SyncProgress _progress;

        public XMLWriterVisitor(SyncProgress progress)
        {
            _progress = progress;
            _dateTime = DateTime.Now.Ticks;
        }

        #region IVisitor Members

        public void Visit(FileCompareObject file, int numOfPaths)
        {
            if (file.Invalid)
                return;

            bool needLastKnownState = false;

            for (int i = 0; i < numOfPaths; i++)
            {
                _progress.Message = Path.Combine(file.GetSmartParentPath(i), file.Name);
                needLastKnownState = ProcessMetaChangeType(file, i) || needLastKnownState;
            }

            if (needLastKnownState)
                GenerateFileLastKnownState(file, numOfPaths);

            _progress.Complete();
        }

        public void Visit(FolderCompareObject folder, int numOfPaths)
        {
            if (folder.Invalid)
                return;

            bool needLastKnownState = false;

            for (int i = 0; i < numOfPaths; i++)
            {
                _progress.Message = Path.Combine(folder.GetSmartParentPath(i), folder.Name);
                needLastKnownState = ProcessFolderFinalState(folder, i) || needLastKnownState;
            }

            if (needLastKnownState)
                GenerateFolderLastKnownState(folder, numOfPaths);

            _progress.Complete();
        }

        public void Visit(RootCompareObject root)
        {
            _progress.Complete();
        }

        #endregion

        public SyncProgress Progress
        {
            get { return _progress; }
        }

        #region File Operations

        private bool ProcessMetaChangeType(FileCompareObject file, int counter)
        {
            FinalState? changeType = file.FinalState[counter];
            bool needLastKnownState = false;

            switch (changeType)
            {
                case FinalState.Created:
                    CreateFileObject(file, counter, false);
                    break;
                case FinalState.Updated:
                    UpdateFileObject(file, counter);
                    break;
                case FinalState.Deleted:
                    DeleteFileObject(file, counter);
                    needLastKnownState = true;
                    break;
                case FinalState.Renamed:
                    RenameFileObject(file, counter);
                    needLastKnownState = true;
                    break;
                case FinalState.CreatedRenamed:
                    CreateFileObject(file, counter, true);
                    DeleteFileObject(file, counter);
                    needLastKnownState = true;
                    break;
            }

            return needLastKnownState;
        }


        private void CreateFileObject(FileCompareObject file, int counter, bool useNewName)
        {
            XmlDocument xmlDoc = new XmlDocument();
            string xmlPath = Path.Combine(file.GetSmartParentPath(counter), CommonXMLConstants.MetadataPath);
            CommonMethods.CreateFileIfNotExist(file.GetSmartParentPath(counter));
            CommonMethods.LoadXML(ref xmlDoc, xmlPath);
            CommonMethods.DoFileCleanUp(xmlDoc, useNewName ? file.NewName : file.Name);

            XmlText hashText = xmlDoc.CreateTextNode(file.Hash[counter]);
            XmlText nameText = xmlDoc.CreateTextNode(useNewName ? file.NewName : file.Name);
            XmlText sizeText = xmlDoc.CreateTextNode(file.Length[counter].ToString());
            XmlText lastModifiedText = xmlDoc.CreateTextNode(file.LastWriteTime[counter].ToString());
            XmlText lastCreatedText = xmlDoc.CreateTextNode(file.CreationTime[counter].ToString());
            XmlText lastUpdated = xmlDoc.CreateTextNode(_dateTime.ToString());

            XmlElement fileElement = xmlDoc.CreateElement(CommonXMLConstants.NodeFile);
            XmlElement hashElement = xmlDoc.CreateElement(CommonXMLConstants.NodeHash);
            XmlElement nameElement = xmlDoc.CreateElement(CommonXMLConstants.NodeName);
            XmlElement sizeElement = xmlDoc.CreateElement(CommonXMLConstants.NodeSize);
            XmlElement lastModifiedElement = xmlDoc.CreateElement(CommonXMLConstants.NodeLastModified);
            XmlElement lastCreatedElement = xmlDoc.CreateElement(CommonXMLConstants.NodeLastCreated);
            XmlElement lastUpdatedElement = xmlDoc.CreateElement(CommonXMLConstants.NodeLastUpdated);

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

            XmlNode node = xmlDoc.SelectSingleNode(CommonXMLConstants.XPathExpr);
            node.AppendChild(fileElement);
            CommonMethods.SaveXML(ref xmlDoc, xmlPath);

            DeleteFileLastKnownStateByName(file, counter);
        }

        private void UpdateFileObject(FileCompareObject file, int counter)
        {
            XmlDocument xmlDoc = new XmlDocument();
            string xmlPath = Path.Combine(file.GetSmartParentPath(counter), CommonXMLConstants.MetadataPath);
            CommonMethods.CreateFileIfNotExist(file.GetSmartParentPath(counter));
            CommonMethods.LoadXML(ref xmlDoc, xmlPath);
            XmlNode node = xmlDoc.SelectSingleNode(CommonXMLConstants.XPathExpr + CommonXMLConstants.XPathFile + "[name=" + CommonMethods.ParseXPathString(file.Name) + "]");

            if (node == null)
            {
                CreateFileObject(file, counter, false);
                return;
            }

            XmlNodeList childNodeList = node.ChildNodes;

            for (int i = 0; i < childNodeList.Count; i++)
            {
                XmlNode nodes = childNodeList[i];

                switch (nodes.Name)
                {
                    case CommonXMLConstants.NodeSize:
                        nodes.InnerText = file.Length[counter].ToString();
                        break;
                    case CommonXMLConstants.NodeHash:
                        nodes.InnerText = file.Hash[counter];
                        break;
                    case CommonXMLConstants.NodeName:
                        nodes.InnerText = file.Name;
                        break;
                    case CommonXMLConstants.NodeLastModified:
                        nodes.InnerText = file.LastWriteTime[counter].ToString();
                        break;
                    case CommonXMLConstants.NodeLastCreated:
                        nodes.InnerText = file.CreationTime[counter].ToString();
                        break;
                    case CommonXMLConstants.NodeLastUpdated:
                        nodes.InnerText = _dateTime.ToString();
                        break;
                }
            }

            CommonMethods.SaveXML(ref xmlDoc, xmlPath);
            DeleteFileLastKnownStateByName(file, counter);
        }

        private void RenameFileObject(FileCompareObject file, int counter)
        {
            XmlDocument xmlDoc = new XmlDocument();
            string xmlPath = Path.Combine(file.GetSmartParentPath(counter), CommonXMLConstants.MetadataPath);
            CommonMethods.CreateFileIfNotExist(file.GetSmartParentPath(counter));
            CommonMethods.LoadXML(ref xmlDoc, xmlPath);
            XmlNode node = xmlDoc.SelectSingleNode(CommonXMLConstants.XPathExpr + CommonXMLConstants.XPathFile + "[name=" + CommonMethods.ParseXPathString(file.Name) + "]");

            if (node == null)
            {
                CreateFileObject(file, counter, true);
                return;
            }

            XmlNodeList childNodeList = node.ChildNodes;

            for (int i = 0; i < childNodeList.Count; i++)
            {
                XmlNode nodes = childNodeList[i];
                switch (nodes.Name)
                {
                    case CommonXMLConstants.NodeName:
                        nodes.InnerText = file.NewName;
                        break;
                    case CommonXMLConstants.NodeLastUpdated:
                        nodes.InnerText = _dateTime.ToString();
                        break;
                }
            }

            CommonMethods.SaveXML(ref xmlDoc, xmlPath);
        }

        private void DeleteFileObject(FileCompareObject file, int counter)
        {
            XmlDocument xmlDoc = new XmlDocument();
            string parentPath = file.GetSmartParentPath(counter);
            string xmlPath = Path.Combine(parentPath, CommonXMLConstants.MetadataPath);

            if (File.Exists(xmlPath))
            {
                CommonMethods.LoadXML(ref xmlDoc, xmlPath);
                XmlNode node = xmlDoc.SelectSingleNode(CommonXMLConstants.XPathExpr + CommonXMLConstants.XPathFile + "[name=" + CommonMethods.ParseXPathString(file.Name) + "]");

                if (node == null)
                    return;

                node.ParentNode.RemoveChild(node);
                CommonMethods.SaveXML(ref xmlDoc, xmlPath);
            }
        }

        #endregion

        #region Folder Operations

        private bool ProcessFolderFinalState(FolderCompareObject folder, int counter)
        {
            FinalState?[] finalStateList = folder.FinalState;
            FinalState? changeType = finalStateList[counter];
            bool needLastKnownState = false;

            switch (changeType)
            {
                case FinalState.Created:
                    CreateFolderObject(folder, counter, false);
                    break;
                case FinalState.Deleted:
                    DeleteFolderObject(folder, counter);
                    needLastKnownState = true;
                    break;
                case FinalState.Renamed:
                    RenameFolderObject(folder, counter);
                    needLastKnownState = true;
                    break;
                case FinalState.CreatedRenamed:
                    CreateFolderObject(folder, counter, true);
                    DeleteFolderObject(folder, counter);
                    needLastKnownState = true;
                    break;
            }

            return needLastKnownState;
        }

        private void CreateFolderObject(FolderCompareObject folder, int counter, bool useNewName)
        {
            string name = useNewName ? folder.NewName : folder.Name;
            XmlDocument xmlDoc = new XmlDocument();
            string xmlPath = Path.Combine(folder.GetSmartParentPath(counter), CommonXMLConstants.MetadataPath);
            CommonMethods.CreateFileIfNotExist(folder.GetSmartParentPath(counter));
            CommonMethods.LoadXML(ref xmlDoc, xmlPath);
            CommonMethods.DoFolderCleanUp(xmlDoc, name);
            XmlText nameText = xmlDoc.CreateTextNode(name);
            XmlText lastUpdatedText = xmlDoc.CreateTextNode(_dateTime.ToString());
            XmlElement nameElement = xmlDoc.CreateElement(CommonXMLConstants.NodeName);
            XmlElement lastUpdatedElement = xmlDoc.CreateElement(CommonXMLConstants.NodeLastUpdated);
            XmlElement folderElement = xmlDoc.CreateElement(CommonXMLConstants.NodeFolder);

            nameElement.AppendChild(nameText);
            lastUpdatedElement.AppendChild(lastUpdatedText);
            folderElement.AppendChild(nameElement);
            folderElement.AppendChild(lastUpdatedElement);

            XmlNode node = xmlDoc.SelectSingleNode(CommonXMLConstants.XPathExpr);
            node.AppendChild(folderElement);

            string subFolderXML = Path.Combine(folder.GetSmartParentPath(counter), name);
            CommonMethods.CreateFileIfNotExist(subFolderXML);
            CommonMethods.SaveXML(ref xmlDoc, xmlPath);
            ModifyFolderName(folder, folder.GetSmartParentPath(counter));
            DeleteFolderLastKnownStateByName(folder, counter);
        }

        private void RenameFolderObject(FolderCompareObject folder, int counter)
        {
            if (Directory.Exists(Path.Combine(folder.GetSmartParentPath(counter), folder.Name)))
            {
                XmlDocument xmlDoc = new XmlDocument();
                string xmlPath = Path.Combine(folder.GetSmartParentPath(counter), CommonXMLConstants.MetadataPath);
                CommonMethods.LoadXML(ref xmlDoc, xmlPath);
                XmlNode node = xmlDoc.SelectSingleNode(CommonXMLConstants.XPathExpr + CommonXMLConstants.XPathFolder + "[name=" + CommonMethods.ParseXPathString(folder.Name) + "]");

                if (node == null)
                {
                    CreateFolderObject(folder, counter, true);
                    return;
                }

                node.FirstChild.InnerText = folder.NewName;
                node.LastChild.InnerText = _dateTime.ToString();
                CommonMethods.SaveXML(ref xmlDoc, xmlPath);
            }
            else
            {
                XmlDocument newXmlDoc = new XmlDocument();
                string editOldXML = Path.Combine(Path.Combine(folder.GetSmartParentPath(counter), folder.NewName), CommonXMLConstants.MetadataPath);
                CommonMethods.CreateFileIfNotExist(Path.Combine(folder.GetSmartParentPath(counter), folder.NewName));
                CommonMethods.LoadXML(ref newXmlDoc, editOldXML);
                XmlNode xmlNameNode = newXmlDoc.SelectSingleNode(CommonXMLConstants.XPathExpr + CommonXMLConstants.XPathName);

                if (xmlNameNode != null)
                    xmlNameNode.InnerText = folder.NewName;

                CommonMethods.SaveXML(ref newXmlDoc, editOldXML);
                string parentXML = Path.Combine(folder.GetSmartParentPath(counter), CommonXMLConstants.MetadataPath);
                XmlDocument parentXmlDoc = new XmlDocument();
                CommonMethods.CreateFileIfNotExist(folder.GetSmartParentPath(counter));
                CommonMethods.LoadXML(ref parentXmlDoc, parentXML);
                XmlNode parentXmlFolderNode = parentXmlDoc.SelectSingleNode(CommonXMLConstants.XPathExpr + CommonXMLConstants.XPathFolder + "[name=" + CommonMethods.ParseXPathString(folder.Name) + "]");

                if (parentXmlFolderNode != null)
                {
                    parentXmlFolderNode.FirstChild.InnerText = folder.NewName;
                    parentXmlFolderNode.LastChild.InnerText = _dateTime.ToString();
                }

                CommonMethods.SaveXML(ref parentXmlDoc, Path.Combine(folder.GetSmartParentPath(counter), CommonXMLConstants.MetadataPath));
            }
        }

        private void DeleteFolderObject(FolderCompareObject folder, int counter)
        {
            XmlDocument xmlDoc = new XmlDocument();
            string xmlPath = Path.Combine(folder.GetSmartParentPath(counter), CommonXMLConstants.MetadataPath);

            if (File.Exists(xmlPath))
            {
                CommonMethods.LoadXML(ref xmlDoc, xmlPath);
                XmlNode node = xmlDoc.SelectSingleNode(CommonXMLConstants.XPathExpr + CommonXMLConstants.XPathFolder + "[name=" + CommonMethods.ParseXPathString(folder.Name) + "]");

                if (node == null)
                    return;

                node.ParentNode.RemoveChild(node);
                CommonMethods.SaveXML(ref xmlDoc, xmlPath);
            }
        }

        private void ModifyFolderName(FolderCompareObject folder, string subFolderPath)
        {
            string name = folder.NewName ?? folder.Name;
            string xmlPath = Path.Combine(Path.Combine(subFolderPath, name), CommonXMLConstants.MetadataPath);
            XmlDocument subFolderDoc = new XmlDocument();
            CommonMethods.LoadXML(ref subFolderDoc, xmlPath);

            XmlNode xmlNameNode = subFolderDoc.SelectSingleNode(CommonXMLConstants.XPathExpr + CommonXMLConstants.XPathName);

            if (xmlNameNode != null)
                xmlNameNode.InnerText = name;

            CommonMethods.SaveXML(ref subFolderDoc, xmlPath);
        }

        #endregion

        #region ToDo Operations

        private void AppendActionFileLastKnownState(XmlDocument xmlDoc, FileCompareObject file, int counter, string changeType)
        {
            XmlText hashText = xmlDoc.CreateTextNode(file.MetaHash[counter]);
            XmlText actionText = xmlDoc.CreateTextNode(changeType);
            XmlText lastModifiedText = xmlDoc.CreateTextNode(file.MetaLastWriteTime[counter].ToString());
            XmlText nameText = xmlDoc.CreateTextNode(file.Name);
            XmlText lastUpdatedText = xmlDoc.CreateTextNode(_dateTime.ToString());

            XmlElement fileElement = xmlDoc.CreateElement(CommonXMLConstants.NodeFile);
            XmlElement nameElement = xmlDoc.CreateElement(CommonXMLConstants.NodeName);
            XmlElement hashElement = xmlDoc.CreateElement(CommonXMLConstants.NodeHash);
            XmlElement actionElement = xmlDoc.CreateElement(CommonXMLConstants.NodeAction);
            XmlElement lastModifiedElement = xmlDoc.CreateElement(CommonXMLConstants.NodeLastModified);
            XmlElement lastUpdatedElement = xmlDoc.CreateElement(CommonXMLConstants.NodeLastUpdated);

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

            XmlNode rootNode = xmlDoc.SelectSingleNode(CommonXMLConstants.XPathLastKnownState);
            rootNode.AppendChild(fileElement);
        }

        private void AppendActionFolderLastKnownState(XmlDocument xmlDoc, FolderCompareObject folder, string changeType)
        {
            string name = folder.MetaName ?? folder.Name;

            XmlText nameText = xmlDoc.CreateTextNode(name);
            XmlText action = xmlDoc.CreateTextNode(changeType);
            XmlText lastUpdatedText = xmlDoc.CreateTextNode(_dateTime.ToString());

            XmlElement folderElement = xmlDoc.CreateElement(CommonXMLConstants.NodeFolder);
            XmlElement nameElement = xmlDoc.CreateElement(CommonXMLConstants.NodeName);
            XmlElement actionElement = xmlDoc.CreateElement(CommonXMLConstants.NodeAction);
            XmlElement lastUpdatedElement = xmlDoc.CreateElement(CommonXMLConstants.NodeLastUpdated);

            nameElement.AppendChild(nameText);
            actionElement.AppendChild(action);
            lastUpdatedElement.AppendChild(lastUpdatedText);

            folderElement.AppendChild(nameElement);
            folderElement.AppendChild(actionElement);
            folderElement.AppendChild(lastUpdatedElement);
            XmlNode rootNode = xmlDoc.SelectSingleNode(CommonXMLConstants.XPathLastKnownState);
            rootNode.AppendChild(folderElement);
        }

        private void DeleteFileLastKnownStateByName(FileCompareObject file, int counter)
        {
            string todoXMLPath = Path.Combine(file.GetSmartParentPath(counter), CommonXMLConstants.LastKnownStatePath);

            if (!File.Exists(todoXMLPath))
                return;

            XmlDocument todoXMLDoc = new XmlDocument();
            CommonMethods.LoadXML(ref todoXMLDoc, todoXMLPath);
            XmlNode fileNode = todoXMLDoc.SelectSingleNode(CommonXMLConstants.XPathLastKnownState + CommonXMLConstants.XPathFile + "[name=" + CommonMethods.ParseXPathString(file.Name) + "]");

            if (fileNode != null)
                fileNode.ParentNode.RemoveChild(fileNode);

            CommonMethods.SaveXML(ref todoXMLDoc, todoXMLPath);
        }

        private void DeleteFolderLastKnownStateByName(FolderCompareObject folder, int counter)
        {
            string todoXMLPath = Path.Combine(folder.GetSmartParentPath(counter), CommonXMLConstants.LastKnownStatePath);

            if (!File.Exists(todoXMLPath))
                return;

            XmlDocument todoXMLDoc = new XmlDocument();
            CommonMethods.LoadXML(ref todoXMLDoc, todoXMLPath);
            XmlNode folderNode = todoXMLDoc.SelectSingleNode(CommonXMLConstants.XPathLastKnownState + CommonXMLConstants.XPathFolder + "[name=" + CommonMethods.ParseXPathString(folder.Name) + "]");

            if (folderNode != null)
                folderNode.ParentNode.RemoveChild(folderNode);

            CommonMethods.SaveXML(ref todoXMLDoc, todoXMLPath);
        }

        private void GenerateFileLastKnownState(FileCompareObject file, int numOfPaths)
        {
            for (int i = 0; i < numOfPaths; i++)
            {
                string parentPath = file.GetSmartParentPath(i);
                XmlDocument xmlTodoDoc = new XmlDocument();
                string todoPath = Path.Combine(parentPath, CommonXMLConstants.LastKnownStatePath);
                CommonMethods.CreateLastKnownStateFile(parentPath);
                CommonMethods.LoadXML(ref xmlTodoDoc, todoPath);
                AppendActionFileLastKnownState(xmlTodoDoc, file, i, CommonXMLConstants.ActionDeleted);
                CommonMethods.SaveXML(ref xmlTodoDoc, todoPath);
            }
        }

        private void GenerateFolderLastKnownState(FolderCompareObject folder, int numOfPaths)
        {
            for (int i = 0; i < numOfPaths; i++)
            {
                string parentPath = folder.GetSmartParentPath(i);
                XmlDocument xmlTodoDoc = new XmlDocument();
                string todoPath = Path.Combine(parentPath, CommonXMLConstants.LastKnownStatePath);
                CommonMethods.CreateLastKnownStateFile(parentPath);
                CommonMethods.LoadXML(ref xmlTodoDoc, todoPath);
                AppendActionFolderLastKnownState(xmlTodoDoc, folder, CommonXMLConstants.ActionDeleted);
                CommonMethods.SaveXML(ref xmlTodoDoc, todoPath);
            }
        }

        #endregion

    }
}