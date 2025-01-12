﻿using System;

namespace Nanook.QueenBee.Parser
{
    public class QbItemQbKey : QbItemBase
    {
        public QbItemQbKey(QbFile root) : base(root)
        {
        }

        public override void Create(QbItemType type)
        {
            if (type != QbItemType.SectionQbKey && type != QbItemType.SectionQbKeyString && type != QbItemType.SectionQbKeyStringQs &&
                type != QbItemType.ArrayQbKey && type != QbItemType.ArrayQbKeyString && type != QbItemType.ArrayQbKeyStringQs &&
                type != QbItemType.StructItemQbKey && type != QbItemType.StructItemQbKeyString && type != QbItemType.StructItemQbKeyStringQs)
                throw new ApplicationException(string.Format("type '{0}' is not a QB key item type", type.ToString()));

            base.Create(type);

            Values = new QbKey[1]; //sets item count
            _values[0] = QbKey.Create(0);
        }

        /// <summary>
        /// Deep clones this item and all children.  Positions and lengths are not cloned.  When inserted in to another item they should be calculated.
        /// </summary>
        /// <returns></returns>
        public override QbItemBase Clone()
        {
            QbItemQbKey qk = new QbItemQbKey(Root);
            qk.Create(QbItemType);

            if (ItemQbKey != null)
                qk.ItemQbKey = ItemQbKey.Clone();

            QbKey[] q = new QbKey[Values.Length];
            for (int i = 0; i < q.Length; i++ )
                q[i] = Values[i].Clone();

            qk.Values = q;
            qk.ItemCount = ItemCount;
            return qk;
        }

        public override void Construct(BinaryEndianReader br, QbItemType type)
        {
            //System.Diagnostics.Debug.WriteLine(string.Format("{0} - 0x{1}", type.ToString(), (base.StreamPos(br) - 4).ToString("X").PadLeft(8, '0')));

            base.Construct(br, type);

            _values = new QbKey[ItemCount];

            uint crc;
            string debug;

            for (int i = 0; i < ItemCount; i++)
            {
                crc = br.ReadUInt32(Root.PakFormat.EndianType);
                debug = Root.LookupDebugName(crc);
                if (debug.Length != 0)
                    _values[i] = QbKey.Create(crc, debug);
                else
                    _values[i] = QbKey.Create(crc);

            }

            base.ConstructEnd(br);
        }

        protected override int CalcItemCount()
        {
            if (_values != null)
                return _values.Length;
            else
                return 0;
        }

        public override uint AlignPointers(uint pos)
        {
            uint next = pos + Length;

            pos = base.AlignPointers(pos);

            return next;
        }

        public override uint Length
        {
            get
            {
                return base.Length + ((uint)_values.Length * 4);
            }
        }

        [GenericEditable("QB Key", typeof(string), false, false)]
        public QbKey[] Values
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

            foreach (QbKey qb in _values)
                bw.Write(qb.Crc, Root.PakFormat.EndianType);

            base.WriteEnd(bw);

            ApplicationException ex = TestLengthCheck(this, bw);
            if (ex != null) throw ex;
        }

        private QbKey[] _values;

    }
}
