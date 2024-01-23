using System;

namespace Nanook.QueenBee.Parser
{
    public class QbItemArray : QbItemBase
    {
        public QbItemArray(QbFile root) : base(root)
        {
        }

        public override void Create(QbItemType type)
        {
            if (type != QbItemType.SectionArray && type != QbItemType.ArrayArray && type != QbItemType.StructItemArray && type != QbItemType.StructItemStruct)
                throw new ApplicationException(string.Format("type '{0}' is not an array item type", type.ToString()));

            base.Create(type);
        }

        /// <summary>
        /// Deep clones this item and all children.  Positions and lengths are not cloned.  When inserted in to another item they should be calculated.
        /// </summary>
        /// <returns></returns>
        public override QbItemBase Clone()
        {
            QbItemArray a = new QbItemArray(Root);
            a.Create(QbItemType);

            if (ItemQbKey != null)
                a.ItemQbKey = ItemQbKey.Clone();

            foreach (QbItemBase qib in Items)
                a.Items.Add(qib.Clone());

            a.ItemCount = ItemCount;

            return a;
        }

        public override void Construct(BinaryEndianReader br, QbItemType type)
        {
            //System.Diagnostics.Debug.WriteLine(string.Format("{0} - 0x{1}", type.ToString(), (base.StreamPos(br) - 4).ToString("X").PadLeft(8, '0')));

            base.Construct(br, type);

            QbItemBase qib = null;
            QbItemType arrayType;
            uint arrayValue;

            for (int i = 0; i < ItemCount; i++)
            {
                arrayValue = br.ReadUInt32(Root.PakFormat.EndianType);
                arrayType = Root.PakFormat.GetQbItemType(arrayValue);

                switch (arrayType)
                {
                    case QbItemType.Floats:
                        qib = new QbItemFloats(Root);
                        break;
                    case QbItemType.ArrayStruct:
                        qib = new QbItemStructArray(Root);
                        break;
                    case QbItemType.ArrayFloat:
                        qib = new QbItemFloat(Root);
                        break;
                    case QbItemType.ArrayString:
                    case QbItemType.ArrayStringW:
                        qib = new QbItemString(Root);
                        break;
                    case QbItemType.ArrayFloatsX2:
                    case QbItemType.ArrayFloatsX3:
                        qib = new QbItemFloatsArray(Root);
                        break;
                    case QbItemType.ArrayStringPointer:
                    case QbItemType.ArrayInteger:
                        qib = new QbItemInteger(Root);
                        break;
                    case QbItemType.ArrayArray:
                        qib = new QbItemArray(Root);
                        break;
                    case QbItemType.ArrayQbKey:
                    case QbItemType.ArrayQbKeyString:
                    case QbItemType.ArrayQbKeyStringQs: //GH:GH
                        qib = new QbItemQbKey(Root);
                        break;
                    case QbItemType.StructHeader:
                        qib = new QbItemStruct(Root);
                        break;
                    default:
                        throw new ApplicationException(string.Format("Location 0x{0}: Unknown array type 0x{1}", (StreamPos(br) - 4).ToString("X").PadLeft(8, '0'), arrayValue.ToString("X").PadLeft(8, '0')));
                }
                qib.Construct(br, arrayType);
                AddItem(qib);

            }
            base.ConstructEnd(br);
        }

        public override uint AlignPointers(uint pos)
        {
            uint next = pos + Length;
            pos = base.AlignPointers(pos);

            foreach (QbItemBase qib in Items)
                pos = qib.AlignPointers(pos);

            //if items exist then null the last item's pointer
            if (Items.Count != 0)
                Items[Items.Count - 1].NextItemPointer = 0;

            return next;
        }

        public override uint Length
        {
            get
            {
                return base.Length + ChildrenLength;
            }
        }

        internal override void Write(BinaryEndianWriter bw)
        {
            StartLengthCheck(bw);

            base.Write(bw);
            foreach (QbItemBase qib in Items)
                qib.Write(bw);

            base.WriteEnd(bw);

            ApplicationException ex = TestLengthCheck(this, bw);
            if (ex != null) throw ex;
        }

    }
}
