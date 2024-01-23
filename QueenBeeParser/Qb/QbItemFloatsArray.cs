using System;

namespace Nanook.QueenBee.Parser
{
    public class QbItemFloatsArray : QbItemBase
    {
        public QbItemFloatsArray(QbFile root) : base(root)
        {
        }

        public override void Create(QbItemType type)
        {
            if (type != QbItemType.SectionFloatsX2 && type != QbItemType.SectionFloatsX3 &&
                type != QbItemType.ArrayFloatsX2 && type != QbItemType.ArrayFloatsX3 &&
                type != QbItemType.StructItemFloatsX2 && type != QbItemType.StructItemFloatsX3)
                throw new ApplicationException(string.Format("type '{0}' is not a floats array item type", type.ToString()));

            base.Create(type);

        }

        /// <summary>
        /// Deep clones this item and all children.  Positions and lengths are not cloned.  When inserted in to another item they should be calculated.
        /// </summary>
        /// <returns></returns>
        public override QbItemBase Clone()
        {
            QbItemFloatsArray a = new QbItemFloatsArray(Root);
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

            QbItemBase qib;
            QbItemType floatsType;
            uint floatsValue;
            bool is3d;
            for (int i = 0; i < ItemCount; i++)
            {
                if (StreamPos(br) != Pointers[i]) //pointer test
                    throw new ApplicationException(QbFile.FormatBadPointerExceptionMessage(this, StreamPos(br), Pointers[i]));

                floatsValue = br.ReadUInt32(Root.PakFormat.EndianType);
                floatsType = Root.PakFormat.GetQbItemType(floatsValue);
                
                is3d = (type == QbItemType.SectionFloatsX3 || type == QbItemType.StructItemFloatsX3 || type == QbItemType.ArrayFloatsX3);

                switch (floatsType)
                {
                    case QbItemType.Floats:
                        qib = new QbItemFloats(Root, is3d);
                        break;
                    default:
                        throw new ApplicationException(string.Format("Location 0x{0}: Not a float type 0x{1}", (StreamPos(br) - 4).ToString("X").PadLeft(8, '0'), floatsValue.ToString("X").PadLeft(8, '0')));
                }
                qib.Construct(br, floatsType);
                AddItem(qib);

                base.ConstructEnd(br);
            }
        }

        public override uint AlignPointers(uint pos)
        {
            uint next = pos + Length;

            pos = base.AlignPointers(pos);

            foreach (QbItemBase qib in Items)
                pos = qib.AlignPointers(pos);
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
