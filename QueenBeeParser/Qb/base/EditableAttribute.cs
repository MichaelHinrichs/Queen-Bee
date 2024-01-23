using System;

namespace Nanook.QueenBee.Parser
{
    [AttributeUsage(AttributeTargets.Property, Inherited = false)]
	public sealed class GenericEditableAttribute : System.Attribute
	{
        public GenericEditableAttribute(string defaultDisplayName, Type editType, bool useQbItemType, bool readOnly)
        {
            _defaultDisplayName = defaultDisplayName;
            _editType = editType;
            _useQbItemType = useQbItemType;
            _readOnly = readOnly;
        }

        public string DefaultDisplayName
        {
            get { return _defaultDisplayName; }
        }

        public bool ReadOnly
        {
            get { return _readOnly; }
        }

        public Type EditType
        {
            get { return _editType; }
        }

        public bool UseQbItemType
        {
            get { return _useQbItemType; }
        }

        private readonly string _defaultDisplayName;
        private readonly Type _editType;
        private readonly bool _readOnly;
        private readonly bool _useQbItemType;
    }
}
