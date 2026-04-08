using System;

namespace itolib.editor.Data
{
    [Serializable]
    internal struct ProjectInformation
    {
        [Serializable]
        internal struct Guid
        {
            public string assemblyName;
            public string fullTypeName;
            public string originalGuid;
        }

        [Serializable]
        internal struct AssetGuid
        {
            public string assetPath;
            public string originalGuid;
            public string fileId;
        }

        public Guid[] guids;
        public AssetGuid[] assetGuids;
    }
}