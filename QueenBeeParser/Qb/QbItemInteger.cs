using System;

namespace Nanook.QueenBee.Parser
{
    public class QbItemInteger : QbItemBase
    {
        public QbItemInteger(QbFile root) : base(root)
        {
        }

        public override void Create(QbItemType type)
        {
            if (type != QbItemType.SectionInteger && type != QbItemType.SectionStringPointer &&
                type != QbItemType.ArrayInteger && type != QbItemType.ArrayStringPointer &&
                type != QbItemType.StructItemInteger && type != QbItemType.StructItemStringPointer)
                throw new ApplicationException(string.Format("type '{0}' is not an integer item type", type.ToString()));

            base.Create(type);

            Values = new uint[1]; //sets item count
            _values[0] = 0;
        }

        /// <summary>
        /// Deep clones this item and all children.  Positions and lengths are not cloned.  When inserted in to another item they should be calculated.
        /// </summary>
        /// <returns></returns>
        public override QbItemBase Clone()
        {
            QbItemInteger qi = new QbItemInteger(Root);
            qi.Create(QbItemType);

            if (ItemQbKey != null)
                qi.ItemQbKey = ItemQbKey.Clone();

            uint[] ii = new uint[Values.Length];
            for (int i = 0; i < ii.Length; i++)
                ii[i] = Values[i];

            qi.Values = ii;
            qi.ItemCount = ItemCount;

            return qi;
        }

        public override void Construct(BinaryEndianReader br, QbItemType type)
        {
            //System.Diagnostics.Debug.WriteLine(string.Format("{0} - 0x{1}", type.ToString(), (base.StreamPos(br) - 4).ToString("X").PadLeft(8, '0')));

            base.Construct(br, type);

            Values = new uint[ItemCount];

            for (int i = 0; i < ItemCount; i++)
                _values[i] = br.ReadUInt32(Root.PakFormat.EndianType);

            base.ConstructEnd(br);
        }

        public override uint AlignPointers(uint pos)
        {
            base.AlignPointers(pos);

            uint next = pos + Length;

            return next;
        }

        protected override int CalcItemCount()
        {
            if (_values != null)
                return _values.Length;
            else
                return 0;
        }

        public override uint Length
        {
            get
            {
                return base.Length + ((uint)_values.Length * 4);
            }
        }

        [GenericEditable("Number", typeof(int), true, false)]
        public uint[] Values
        {
            get { return _values; }
            set
            {
                _values = value;
                ItemCount = (uint)_values.Length;
            }
        }

        internal override void Write(BinaryEndianWriter bw)
        {
            StartLengthCheck(bw);

            base.Write(bw);

            foreach (uint i in _values)
                bw.Write(i, Root.PakFormat.EndianType);

            base.WriteEnd(bw);

            ApplicationException ex = TestLengthCheck(this, bw);
            if (ex != null) throw ex;
        }

        private uint[] _values;

    }
}
