using System;

namespace Nanook.QueenBee.Parser
{
    public class QbItemFloat : QbItemBase
    {
        public QbItemFloat(QbFile root) : base(root)
        {
        }

        public override void Create(QbItemType type)
        {
            if (type != QbItemType.SectionFloat && type != QbItemType.ArrayFloat && type != QbItemType.StructItemFloat)
                throw new ApplicationException(string.Format("type '{0}' is not a float item type", type.ToString()));

            base.Create(type);

            Values = new float[1]; //sets item count
            _values[0] = 0;
        }

        /// <summary>
        /// Deep clones this item and all children.  Positions and lengths are not cloned.  When inserted in to another item they should be calculated.
        /// </summary>
        /// <returns></returns>
        public override QbItemBase Clone()
        {
            QbItemFloat qi = new QbItemFloat(Root);
            qi.Create(QbItemType);

            if (ItemQbKey != null)
                qi.ItemQbKey = ItemQbKey.Clone();

            float[] fi = new float[Values.Length];
            for (int i = 0; i < fi.Length; i++)
                fi[i] = Values[i];

            qi.Values = fi;
            qi.ItemCount = ItemCount;

            return qi;
        }

        public override void Construct(BinaryEndianReader br, QbItemType type)
        {
            //System.Diagnostics.Debug.WriteLine(string.Format("{0} - 0x{1}", type.ToString(), (base.StreamPos(br) - 4).ToString("X").PadLeft(8, '0')));

            base.Construct(br, type);

            _values = new float[base.ItemCount];

            for (int i = 0; i < base.ItemCount; i++)
                _values[i] = br.ReadSingle(base.Root.PakFormat.EndianType);

            base.ConstructEnd(br);
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
                return base.Length + ((uint)(_values.Length) * 4);
            }
        }

        [GenericEditable("Decimal", typeof(float), true, false)]
        public float[] Values
        {
            get { return _values; }
            set
            {
                _values = value;
                base.ItemCount = (uint)_values.Length;
            }
        }

        protected override int CalcItemCount()
        {
            if (_values != null)
                return _values.Length;
            else
                return 0;
        }

        internal override void Write(BinaryEndianWriter bw)
        {
            base.StartLengthCheck(bw);

            base.Write(bw);

            foreach (float f in _values)
                bw.Write(f, base.Root.PakFormat.EndianType);

            base.WriteEnd(bw);

            ApplicationException ex = base.TestLengthCheck(this, bw);
            if (ex != null) throw ex;
        }

        private float[] _values;

    }
}
