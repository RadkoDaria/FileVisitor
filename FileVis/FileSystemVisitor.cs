using FileVis.args;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileVis
{
    class FileSystemVisitor
    {
        private readonly DirectoryInfo _startDirectory;
        private readonly Func<FileSystemInfo, bool> _filter;

    
        public event EventHandler<StartArgs> Start;
        public event EventHandler<FinishArgs> Finish;
        public event EventHandler<ItemFindedArgs<FileInfo>> FileFinded;
        public event EventHandler<ItemFindedArgs<FileInfo>> FilteredFileFinded;
        public event EventHandler<ItemFindedArgs<DirectoryInfo>> DirectoryFinded;
        public event EventHandler<ItemFindedArgs<DirectoryInfo>> FilteredDirectoryFinded;

      
        public FileSystemVisitor(string path,
            Func<FileSystemInfo, bool> filter = null)
            : this(new DirectoryInfo(path), filter) { }

      
        public FileSystemVisitor(DirectoryInfo startDirectory,
             Func<FileSystemInfo, bool> filter = null)
        {
            _startDirectory = startDirectory;
            _filter = filter;
        }

        
        public IEnumerable<FileSystemInfo> GetFileInfoSequence()
        {
            OnEvent(Start, new StartArgs());

            foreach (var info in BypassFileSystem(_startDirectory, CurrentStep.ContinueSearch))
            {
                yield return info;
            }

            OnEvent(Finish, new FinishArgs());
        }

        private IEnumerable<FileSystemInfo> BypassFileSystem(DirectoryInfo directory, CurrentStep currentStep)
        {
            foreach (FileSystemInfo fileSystemInfo in directory.EnumerateFileSystemInfos())
            {
                if (fileSystemInfo is FileInfo file)
                {
                    currentStep.Step = FilterItemFinded(file, _filter, FileFinded, FilteredFileFinded, OnEvent);
                }

                if (fileSystemInfo is DirectoryInfo dir)
                {
                    currentStep.Step = FilterItemFinded(directory, _filter, DirectoryFinded, FilteredDirectoryFinded, OnEvent);

                    if (currentStep.Step == FilteringSteps.Continue)
                    {
                        yield return dir;

                        foreach (var innerInfo in BypassFileSystem(dir, currentStep))
                        {
                            yield return innerInfo;
                        }

                        continue;
                    }
                }

                if (currentStep.Step == FilteringSteps.Stop)
                {
                    yield break;
                }

                yield return fileSystemInfo;
            }
        }

        private void OnEvent<TArgs>(EventHandler<TArgs> someEvent, TArgs args)
        {
            someEvent?.Invoke(this, args);
        }

        public FilteringSteps FilterItemFinded<TItemInfo>(
                TItemInfo itemInfo,
                Func<FileSystemInfo, bool> filter,
                EventHandler<ItemFindedArgs<TItemInfo>> itemFinded,
                EventHandler<ItemFindedArgs<TItemInfo>> filteredItemFinded,
                Action<EventHandler<ItemFindedArgs<TItemInfo>>, ItemFindedArgs<TItemInfo>> eventEmitter)
                where TItemInfo : FileSystemInfo
        {
            var args = new ItemFindedArgs<TItemInfo>
            {
                FoundItem = itemInfo,
                StepType = FilteringSteps.Continue
            };

            eventEmitter(itemFinded, args);

            if (args.StepType != FilteringSteps.Continue || filter == null)
            {
                return args.StepType;
            }

            if (filter(itemInfo))
            {
                args = new ItemFindedArgs<TItemInfo>
                {
                    FoundItem = itemInfo,
                    StepType = FilteringSteps.Continue
                };

                eventEmitter(filteredItemFinded, args);

                return args.StepType;
            }

            return FilteringSteps.Skip;
        }

        private class CurrentStep
        {
            public FilteringSteps Step { get; set; }
            public static CurrentStep ContinueSearch = new CurrentStep { Step = FilteringSteps.Continue };
        }
    }
}

