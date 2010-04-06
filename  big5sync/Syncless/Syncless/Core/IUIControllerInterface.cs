using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Syncless.CompareAndSync.Manual.CompareObject;
using Syncless.Tagging;
using Syncless.CompareAndSync;
using System.IO;
using Syncless.Filters;
using Syncless.Core.View;
using Syncless.Logging;
namespace Syncless.Core
{
    public interface IUIControllerInterface
    {


        List<string> GetAllTags();
        List<string> GetTags(DirectoryInfo folder);

        TagView GetTag(string tagName);       

        bool DeleteTag(string tagName);

        TagView CreateTag(string tagName);
        TagView Tag(string tagName, DirectoryInfo folder);
                
        int Untag(string tagName, DirectoryInfo folder);
        
        bool SwitchMode(string tagName , TagMode mode);
        TagState GetTagState(string tagName);
        bool PrepareForTermination();
        void Terminate();

        bool Initiate(IUIInterface inf);
        //bool MonitorTag(string tagName, bool mode);

        bool StartManualSync(string tagName);
        bool CancelManualSync(string tagName);

        
        bool UpdateFilterList(string tagName, List<Filter> filterlist);

        List<Filter> GetAllFilters(string tagName);
        
        RootCompareObject PreviewSync(string tagName);

        bool AllowForRemoval(DriveInfo drive);

        int Clean(string path);

        bool SetProfileName(string name);
        string GetProfileName();

        List<LogData> ReadLog();

    }

}
