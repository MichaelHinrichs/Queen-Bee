using System;

namespace Nanook.QueenBee.Parser
{
    public class QbItemStruct : QbItemBase
    {
        public QbItemStruct(QbFile root) : base(root)
        {
        }

        public override void Create(QbItemType type)
        {
            if (type != QbItemType.SectionStruct && type != QbItemType.StructItemStruct && type != QbItemType.StructHeader)
                throw new ApplicationException(string.Format("type '{0}' is not a struct item type", type.ToString()));

            base.Create(type);

            if (type != QbItemType.StructHeader)
            {
                _headerType = QbItemType.StructHeader;
                _headerValue = Root.PakFormat.GetQbItemValue(_headerType, Root);
            }

        }

        /// <summary>
        /// Deep clones this item and all children.  Positions and lengths are not cloned.  When inserted in to another item they should be calculated.
        /// </summary>
        /// <returns></returns>
        public override QbItemBase Clone()
        {
            QbItemStruct s = new QbItemStruct(Root);
            s.Create(QbItemType);

            if (ItemQbKey != null)
                s.ItemQbKey = ItemQbKey.Clone();

            foreach (QbItemBase qib in Items)
                s.Items.Add(qib.Clone());

            s.ItemCount = ItemCount;

            return s;
        }

        public override void Construct(BinaryEndianReader br, QbItemType type)
        {
            //System.Diagnostics.Debug.WriteLine(string.Format("{0} - 0x{1}", type.ToString(), (base.StreamPos(br) - 4).ToString("X").PadLeft(8, '0')));

            base.Construct(br, type);

            uint pointer;

            if (type != QbItemType.StructHeader)
                _headerValue = br.ReadUInt32(Root.PakFormat.EndianType);
            else
                _headerValue = Root.PakFormat.GetQbItemValue(type, Root);

             _headerType = Root.PakFormat.GetQbItemType(_headerValue);

            QbItemBase qib = null;
            QbItemType structType;
            uint structValue;

            if (_headerType == QbItemType.StructHeader)
            {
                pointer = br.ReadUInt32(Root.PakFormat.EndianType); //Should be the current stream position after reading

                _iniNextItemPointer = pointer;

                if (pointer != 0 && StreamPos(br) != pointer) //pointer test
                    throw new ApplicationException(QbFile.FormatBadPointerExceptionMessage(this, StreamPos(br), pointer));

                while (pointer != 0)
                {
                    structValue = br.ReadUInt32(Root.PakFormat.EndianType);
                    structType = Root.PakFormat.GetQbItemType(structValue);

                    switch (structType)
                    {
                        case QbItemType.StructItemStruct:
                            Root.PakFormat.StructItemChildrenType = StructItemChildrenType.StructItems;
                            qib = new QbItemStruct(Root);
                            break;
                        case QbItemType.StructItemStringPointer:
                        case QbItemType.StructItemInteger:
                            Root.PakFormat.StructItemChildrenType = StructItemChildrenType.StructItems;
                            qib = new QbItemInteger(Root);
                            break;
                        case QbItemType.StructItemQbKeyString:
                        case QbItemType.StructItemQbKeyStringQs:
                        case QbItemType.StructItemQbKey:
                            Root.PakFormat.StructItemChildrenType = StructItemChildrenType.StructItems;
                            qib = new QbItemQbKey(Root);
                            break;
                        case QbItemType.StructItemString:
                        case QbItemType.StructItemStringW:
                            Root.PakFormat.StructItemChildrenType = StructItemChildrenType.StructItems;
                            qib = new QbItemString(Root);
                            break;
                        case QbItemType.StructItemFloat:
                            Root.PakFormat.StructItemChildrenType = StructItemChildrenType.StructItems;
                            qib = new QbItemFloat(Root);
                            break;
                        case QbItemType.StructItemFloatsX2:
                        case QbItemType.StructItemFloatsX3:
                            Root.PakFormat.StructItemChildrenType = StructItemChildrenType.StructItems;
                            qib = new QbItemFloatsArray(Root);
                            break;
                        case QbItemType.StructItemArray:
                            Root.PakFormat.StructItemChildrenType = StructItemChildrenType.StructItems;
                            qib = new QbItemArray(Root);
                            break;

                        //Convert array types to structitems to fit in with this parser (if QbFile.HasStructItems is false then internal type will be swapped back to array)
                        case QbItemType.ArrayStruct:
                            structType = QbItemType.StructItemStruct;
                            qib = new QbItemArray(Root);
                            break;
                        case QbItemType.ArrayInteger:
                            structType = QbItemType.StructItemInteger;
                            qib = new QbItemInteger(Root);
                            break;
                        case QbItemType.ArrayQbKeyString:
                            structType = QbItemType.StructItemQbKeyString;
                            qib = new QbItemQbKey(Root);
                            break;
                        case QbItemType.ArrayStringPointer:
                            structType = QbItemType.StructItemStringPointer;
                            qib = new QbItemInteger(Root);
                            break;
                        case QbItemType.ArrayQbKeyStringQs:
                            structType = QbItemType.StructItemQbKeyStringQs;
                            qib = new QbItemQbKey(Root);
                            break;
                        case QbItemType.ArrayQbKey:
                            structType = QbItemType.StructItemQbKey;
                            qib = new QbItemQbKey(Root);
                            break;
                        case QbItemType.ArrayString:
                            structType = QbItemType.StructItemString;
                            qib = new QbItemString(Root);
                            break;
                        case QbItemType.ArrayStringW:
                            structType = QbItemType.StructItemStringW;
                            qib = new QbItemString(Root);
                            break;
                        case QbItemType.ArrayFloat:
                            structType = QbItemType.StructItemFloat;
                            qib = new QbItemFloat(Root);
                            break;
                        case QbItemType.ArrayFloatsX2:
                            structType = QbItemType.StructItemFloatsX2;
                            qib = new QbItemFloatsArray(Root);
                            break;
                        case QbItemType.ArrayFloatsX3:
                            structType = QbItemType.StructItemFloatsX3;
                            qib = new QbItemFloatsArray(Root);
                            break;
                        case QbItemType.ArrayArray:
                            structType = QbItemType.StructItemArray;
                            qib = new QbItemArray(Root);
                            break;
                        default:
                            qib = null;
                            break;
                    }

                    if (qib != null)
                    {
                        if (Root.PakFormat.StructItemChildrenType == StructItemChildrenType.NotSet) //will have been set to structItem if qib is not null)
                            Root.PakFormat.StructItemChildrenType = StructItemChildrenType.ArrayItems;

                        qib.Construct(br, structType);
                        AddItem(qib);
                        pointer = qib.NextItemPointer;
                    }
                    else
                        throw new ApplicationException(string.Format("Location 0x{0}: Unknown item type 0x{1} in struct ", (StreamPos(br) - 4).ToString("X").PadLeft(8, '0'), structValue.ToString("X").PadLeft(8, '0')));

                }
            }
            else
                throw new ApplicationException(string.Format("Location 0x{0}: Struct without header type", (StreamPos(br) - 4).ToString("X").PadLeft(8, '0')));

            base.ConstructEnd(br);
        }

        public override uint AlignPointers(uint pos)
        {
            uint next = pos + Length;

            pos = base.AlignPointers(pos);

            if (QbItemType != QbItemType.StructHeader)
                pos += (1 * 4); //skip header
            _iniNextItemPointer = (pos += (1 * 4)); //skip header and pointer

            foreach (QbItemBase qib in Items)
                pos = qib.AlignPointers(pos);
            if (Items.Count != 0)
                Items[Items.Count - 1].NextItemPointer = 0;
            else
                _iniNextItemPointer = 0; //no next item

            return next;
        }

        public override uint Length
        {
            get
            {
                return base.Length + ChildrenLength + (1 * 4) + (uint)(QbItemType != QbItemType.StructHeader ? 1 * 4 : 0 * 4);
            }
        }

        public uint InitNextItemPointer
        {
            get { return _iniNextItemPointer; }
            set { _iniNextItemPointer = value; }
        }

        internal override void Write(BinaryEndianWriter bw)
        {
            StartLengthCheck(bw);

            base.Write(bw);

            if (QbItemType != QbItemType.StructHeader)
                bw.Write(_headerValue, Root.PakFormat.EndianType);
            bw.Write(_iniNextItemPointer, Root.PakFormat.EndianType);

            foreach (QbItemBase qib in Items)
                qib.Write(bw);

            base.WriteEnd(bw);

            ApplicationException ex = TestLengthCheck(this, bw);
            if (ex != null) throw ex;
        }

        private QbItemType _headerType;
        private uint _headerValue;
        private uint _iniNextItemPointer;
    }
}
