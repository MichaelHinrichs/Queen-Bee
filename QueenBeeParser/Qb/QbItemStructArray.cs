using System;

namespace Nanook.QueenBee.Parser
{
    public class QbItemStructArray : QbItemBase
    {
        public QbItemStructArray(QbFile root) : base(root)
        {
        }

        public override void Create(QbItemType type)
        {
            if (type != QbItemType.ArrayStruct)
                throw new ApplicationException(string.Format("type '{0}' is not a struct array item type", type.ToString()));

            base.Create(type);

        }

        /// <summary>
        /// Deep clones this item and all children.  Positions and lengths are not cloned.  When inserted in to another item they should be calculated.
        /// </summary>
        /// <returns></returns>
        public override QbItemBase Clone()
        {
            QbItemStructArray a = new QbItemStructArray(Root);
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
            QbItemType headerType;
            uint headerValue;

            for (int i = 0; i < base.ItemCount; i++)
            {
                if (base.StreamPos(br) != base.Pointers[i]) //pointer test
                    throw new ApplicationException(QbFile.FormatBadPointerExceptionMessage(this, base.StreamPos(br), base.Pointers[i]));

                headerValue = br.ReadUInt32(Root.PakFormat.EndianType);
                headerType = Root.PakFormat.GetQbItemType(headerValue);

                switch (headerType)
                {
                    case QbItemType.StructHeader:
                        qib = new QbItemStruct(Root);
                        break;
                    default:
                        throw new ApplicationException(string.Format("Location 0x{0}: Not a struct header type 0x{1}", (base.StreamPos(br) - 4).ToString("X").PadLeft(8, '0'), headerValue.ToString("X").PadLeft(8, '0')));
                }
                qib.Construct(br, headerType);
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
            if (Items.Count != 0)
                Items[Items.Count - 1].NextItemPointer = 0;

            return next;
        }

        public override uint Length
        {
            get
            {
                return base.Length + base.ChildrenLength;
            }
        }

        internal override void Write(BinaryEndianWriter bw)
        {
            base.StartLengthCheck(bw);

            base.Write(bw);

            foreach (QbItemBase qib in base.Items)
                qib.Write(bw);

            base.WriteEnd(bw);

            ApplicationException ex = base.TestLengthCheck(this, bw);
            if (ex != null) throw ex;
        }

    }
}
